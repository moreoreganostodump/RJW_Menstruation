using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;
using Milk;
using RJW_Menstruation;

namespace MilkModule
{
    public class JobDriver_HumanMilkSelf : JobDriver
    {
        const float milkingTime = 250f;//ticks - 120 = 2 real seconds, 3 in-game minutes
    
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(pawn, job, 1, -1, null, errorOnFailed);
        }
    
        protected override IEnumerable<Toil> MakeNewToils()
        {
    
            HumanCompHasGatherableBodyResource Comp = GetMilkComp(pawn);
            HediffComp_Breast breastcomp = pawn.GetBreastComp();
            this.FailOn(delegate
            {
                return !(Comp.Fullness > 0.01f);
            });
            Toil milking = Toils_General.Wait((int)(50 + milkingTime * Comp.Fullness), TargetIndex.None);//duration of 
    
            milking.WithProgressBarToilDelay(TargetIndex.A);
            milking.tickAction = delegate ()
            {
                if (breastcomp != null)
                {
                    breastcomp.AdjustAreolaSize(Rand.Range(0.0f,0.0001f * Configurations.NipplePermanentTransitionVariance));
                    breastcomp.AdjustNippleSize(Rand.Range(0.0f,0.0001f * Configurations.NipplePermanentTransitionVariance));
                }

            };
            yield return milking;
            yield return new Toil()
            {
                initAction = delegate ()
                {
                    Comp.Gathered(pawn);
                }
            };
            //yield return excreting;
            yield break;
    
        }

        
        public static HumanCompHasGatherableBodyResource GetMilkComp(Pawn pawn)
        {
            HumanCompHasGatherableBodyResource result;
            if (pawn.health.hediffSet.HasHediff(VariousDefOf.Hediff_Heavy_Lactating_Permanent))
            {
                result = pawn.TryGetComp<CompHyperMilkableHuman>();
            }
            else
            {
                result = pawn.TryGetComp<CompMilkableHuman>();
            }
            return result;
        }


    }
    

}
