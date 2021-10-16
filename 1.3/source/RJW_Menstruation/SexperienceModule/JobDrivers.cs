using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;
using RJWSexperience;

namespace RJW_Menstruation.Sexperience
{
    public class JobDriver_VaginaWashingWithBucket : JobDriver_CleanSelfWithBucket
    {
        const int excretingTime = 300;//ticks - 120 = 2 real seconds, 3 in-game minutes


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(pawn, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {

            HediffComp_Menstruation Comp = pawn.GetMenstruationComp();
            //this.FailOn(delegate
            //{
            //    return !(Comp.TotalCumPercent > 0.001);
            //});
            yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
            Toil excreting = Toils_General.Wait(excretingTime, TargetIndex.None);//duration of 

            excreting.WithProgressBarToilDelay(TargetIndex.A);
            yield return excreting;
            yield return new Toil()
            {
                initAction = delegate ()
                {
                    if (Comp.TotalCumPercent > 0.001)
                    {
                        CumMixture mixture = Comp.MixtureOut(RJWSexperience.VariousDefOf.GatheredCum, 0.5f);
                        float amount = mixture.Volume;
                        if (mixture.ispurecum)
                        {
                            Bucket.AddCum(amount);
                        }
                        else
                        {
                            GatheredCumMixture cummixture = (GatheredCumMixture)ThingMaker.MakeThing(VariousDefOf.GatheredCumMixture);
                            cummixture.InitwithCum(mixture);
                            Bucket.AddCum(amount, cummixture);
                        }
                    }
                    else ReadyForNextToil();
                    if (Comp.TotalCumPercent > 0.001) JumpToToil(excreting);
                }
            };

            Toil cleaning = new Toil();
            cleaning.initAction = CleaningInit;
            cleaning.tickAction = CleaningTick;
            cleaning.AddFinishAction(Finish);
            cleaning.defaultCompleteMode = ToilCompleteMode.Never;
            cleaning.WithProgressBar(TargetIndex.A, () => progress / CleaningTime);

            yield return cleaning;

            //yield return excreting;
            yield break;
        }
    }
}
