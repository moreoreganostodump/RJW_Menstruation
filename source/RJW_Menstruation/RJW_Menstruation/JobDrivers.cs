using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace RJW_Menstruation
{
    public class JobDriver_VaginaWashing : JobDriver
    {
        const int excretingTime = 300;//ticks - 120 = 2 real seconds, 3 in-game minutes

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(pawn, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {

            HediffComp_Menstruation Comp = Utility.GetMenstruationComp(pawn);
            this.FailOn(delegate
            {
                return !(Comp.TotalCumPercent > 0.01);
            });
            Toil excreting = Toils_General.Wait(excretingTime, TargetIndex.None);//duration of 

            excreting.WithProgressBarToilDelay(TargetIndex.A);
            yield return excreting;
            yield return new Toil()
            {
                initAction = delegate ()
                {
                    Comp.CumOutForce(null, 0.5f);
                    if (Comp.TotalCumPercent > 0.01) this.JumpToToil(excreting);
                }
            };
            //yield return excreting;
            yield break;
        }
    }

    


}
