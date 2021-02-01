using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace RJW_Menstruation
{
    public class FertPillOutcomDoer : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            HediffComp_Menstruation comp = Utility.GetMenstruationComp(pawn);
            if (comp.curStage.Equals(HediffComp_Menstruation.Stage.Follicular) || comp.curStage.Equals(HediffComp_Menstruation.Stage.Luteal)) comp.curStage = HediffComp_Menstruation.Stage.Ovulatory;
        }
    }


}
