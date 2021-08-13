using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using rjw;

namespace RJW_Menstruation
{
    public class FertPillOutcomDoer : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            HediffComp_Menstruation comp = pawn.GetMenstruationComp();
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

    public class InduceOvulationOutcomDoer : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            HediffComp_Menstruation comp = pawn.GetMenstruationComp();
            if (comp != null && (comp.curStage.Equals(HediffComp_Menstruation.Stage.Follicular)
                || comp.curStage.Equals(HediffComp_Menstruation.Stage.ClimactericFollicular)
                || comp.curStage.Equals(HediffComp_Menstruation.Stage.Anestrus)
                ))
            {
                comp.SetEstrus(comp.Props.eggLifespanDays);
                comp.curStage = HediffComp_Menstruation.Stage.Ovulatory;
                comp.eggstack += ingested.stackCount - 1;
            }
        }
    }

    public class IngestionOutcomeDoer_AdjustSeverity : IngestionOutcomeDoer
    {
        public HediffDef hediffDef;
        public float severity;

        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (hediff != null) hediff.Severity += severity;
        }

    }

    public class OvaryPillOutcomDoer : IngestionOutcomeDoer
    {
        public float effectOffset;


        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            HediffComp_Menstruation comp = pawn.GetMenstruationComp();
            if (Configurations.EnableMenopause && comp != null)
            {
                comp.RecoverOvary(1 + effectOffset);
            }
        }
    }

    public class SuperOvulationOutcomDoer : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            HediffComp_Menstruation comp = pawn.GetMenstruationComp();
            if (comp != null)
            {
                comp.eggstack += Rand.Range(1, 4);
            }


        }
    }

    public class ContraptiveOutcomDoer : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            
            List<Thought_Memory> memories = pawn.needs?.mood?.thoughts?.memories?.Memories.FindAll(
                x => 
                x.def == VariousDefOf.CameInsideF
             || x.def == VariousDefOf.CameInsideFFetish
             || x.def == VariousDefOf.HaterCameInsideF);
            if (!memories.NullOrEmpty())
            {
                foreach (Thought_Memory m in memories)
                {
                    if (m.def == VariousDefOf.HaterCameInsideF) m.moodPowerFactor = 0.5f;
                    else m.moodPowerFactor = 0.3f;
                    
                }
                if (pawn.Has(Quirk.Breeder)) pawn.needs.mood.thoughts.memories.TryGainMemoryFast(VariousDefOf.HateTookContraptivePill);
                else pawn.needs.mood.thoughts.memories.TryGainMemoryFast(VariousDefOf.TookContraptivePill);
            }
        }
         
    }



}
