using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using rjw;


namespace RJW_Menstruation
{
    public class Recipe_BreastSurgery : Recipe_Surgery
    {
        public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
        {

            if (pawn.gender != Gender.Female)
            {
                yield break;
            }

            BodyPartRecord part = Genital_Helper.get_breastsBPR(pawn);
            if (part != null)
            {
                
                if (pawn.GetBreastComp() != null) yield return part;
            }
        }

        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            HediffComp_Breast breast = pawn.GetBreastComp();
            if (billDoer != null && breast != null)
            {
                TaleRecorder.RecordTale(TaleDefOf.DidSurgery, new object[]
                {
                    billDoer,
                    pawn
                });
                SurgeryResult(breast);
            }
        }

        protected virtual void SurgeryResult(HediffComp_Breast breast)
        {
        }
    }


    public class Recipe_ExpandAreola : Recipe_BreastSurgery
    {
        protected override void SurgeryResult(HediffComp_Breast breast)
        {
            breast.AdjustAreolaSizeImmidiately(0.1f);
        }
    }

    public class Recipe_ContractAreola : Recipe_BreastSurgery
    {
        protected override void SurgeryResult(HediffComp_Breast breast)
        {
            breast.AdjustAreolaSizeImmidiately(-0.1f);
        }
    }

    public class Recipe_ExpandNipple : Recipe_BreastSurgery
    {
        protected override void SurgeryResult(HediffComp_Breast breast)
        {
            breast.AdjustNippleSizeImmidiately(0.1f);
        }
    }

    public class Recipe_ContractNipple : Recipe_BreastSurgery
    {
        protected override void SurgeryResult(HediffComp_Breast breast)
        {
            breast.AdjustNippleSizeImmidiately(-0.1f);
        }
    }
}
