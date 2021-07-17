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

            HediffComp_Menstruation Comp = pawn.GetMenstruationComp();
            this.FailOn(delegate
            {
                return !(Comp.TotalCumPercent > 0.001);
            });
            Toil excreting = Toils_General.Wait(excretingTime, TargetIndex.None);//duration of 

            excreting.WithProgressBarToilDelay(TargetIndex.A);
            yield return excreting;
            yield return new Toil()
            {
                initAction = delegate ()
                {
                    Comp.CumOut(null, 0.5f);
                    if (Comp.TotalCumPercent > 0.001) JumpToToil(excreting);
                }
            };
            //yield return excreting;
            yield break;
        }
    }

    public class JobDriver_MilkSelf : JobDriver
    {
        protected float progress = 0;
        protected float MilkingTime
        {
            get
            {
                return 250f * Fullness + 50f;
            }
        }
        protected virtual float Fullness
        {
            get
            {
                return comp?.Fullness ?? 0;
            }
        }

        private CompMilkable comp;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(pawn, job, 1, -1, null, errorOnFailed);
        }

        protected virtual void PreMakeNewToils()
        {
            comp = pawn.GetComp<CompMilkable>();
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            PreMakeNewToils();
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnNotCasualInterruptible(TargetIndex.A);
            Toil milking = new Toil();
            milking.initAction = delegate ()
            {
                pawn.pather.StopDead();
            };
            milking.tickAction = MilkingTick;
            milking.AddFinishAction(Finish);
            milking.defaultCompleteMode = ToilCompleteMode.Never;
            milking.WithProgressBar(TargetIndex.A, () => progress / MilkingTime);
            yield return milking;
            yield break;
        }

        protected void MilkingTick()
        {
            progress += pawn.GetStatValue(StatDefOf.AnimalGatherSpeed);
            if (progress > MilkingTime)
            {
                Gathered();
                pawn.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
            PostTickAction();
        }

        protected virtual void Gathered()
        {
            pawn.GetComp<CompMilkable>().Gathered(pawn);
        }

        protected virtual void Finish()
        {
            if(pawn.CurJobDef == JobDefOf.Wait_MaintainPosture)
            {
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }

        protected virtual void PostTickAction()
        {
        }


    }


}
