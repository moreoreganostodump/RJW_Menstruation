using RimWorld;
using Verse;

namespace RJW_Menstruation
{
    public class FertPillOutcomDoer : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            HediffComp_Menstruation comp = Utility.GetMenstruationComp(pawn);
            if (comp != null && (comp.curStage.Equals(HediffComp_Menstruation.Stage.Follicular)
                || comp.curStage.Equals(HediffComp_Menstruation.Stage.Luteal)
                || comp.curStage.Equals(HediffComp_Menstruation.Stage.ClimactericFollicular)
                || comp.curStage.Equals(HediffComp_Menstruation.Stage.ClimactericLuteal)
                || comp.curStage.Equals(HediffComp_Menstruation.Stage.Anestrus)
                ))
            {
                comp.SetEstrus(comp.Props.eggLifespanDays);
                comp.curStage = HediffComp_Menstruation.Stage.Ovulatory;
                comp.ovarypower--;
            }
        }
    }

    public class OvaryPillOutcomDoer : IngestionOutcomeDoer
    {
        public float effectOffset;


        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            HediffComp_Menstruation comp = Utility.GetMenstruationComp(pawn);
            if (comp != null)
            {
                comp.ovarypower = (int)(comp.ovarypower * (1 + effectOffset));
            }


        }
    }

    public class SuperOvulationOutcomDoer : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            HediffComp_Menstruation comp = Utility.GetMenstruationComp(pawn);
            if (comp != null)
            {
                comp.eggstack += Rand.Range(1, 4);
            }


        }
    }



}
