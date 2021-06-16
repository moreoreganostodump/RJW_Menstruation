﻿using System.Collections.Generic;
using RimWorld;
using Verse;
using rjw;

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
