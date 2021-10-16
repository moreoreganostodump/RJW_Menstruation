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
    public class IngestionOutcomeDoer_GiveHediff_StackCount : IngestionOutcomeDoer_GiveHediff
	{
		private bool divideByBodySize = false;

		protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
		{
			Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
			float effect = ((!(severity > 0f)) ? hediffDef.initialSeverity : severity) * ingested.stackCount;
			if (divideByBodySize)
			{
				effect /= pawn.BodySize;
			}
			AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, toleranceChemical, ref effect);
			hediff.Severity = effect;
			pawn.health.AddHediff(hediff);
		}


    }
}
