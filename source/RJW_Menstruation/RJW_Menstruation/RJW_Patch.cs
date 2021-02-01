using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using rjw;
using Verse;


namespace RJW_Menstruation
{

	[HarmonyPatch(typeof(PregnancyHelper), "impregnate")]
	public static class impregnate_Patch
	{
		public static bool Prefix(Pawn pawn, Pawn partner, xxx.rjwSextype sextype = xxx.rjwSextype.None)
		{
			if (sextype == xxx.rjwSextype.Vaginal)
			{
			var pawnpartBPR = Genital_Helper.get_genitalsBPR(pawn);
			var pawnparts = Genital_Helper.get_PartsHediffList(pawn, pawnpartBPR);
			var partnerpartBPR = Genital_Helper.get_genitalsBPR(partner);
			var partnerparts = Genital_Helper.get_PartsHediffList(partner, partnerpartBPR);
			
				if (Genital_Helper.has_vagina(partner, partnerparts))
				{
					HediffComp_Menstruation comp = Utility.GetMenstruationComp(partner);
					if (comp != null)
					{
						if (Genital_Helper.has_penis_fertile(pawn, pawnparts) && PregnancyHelper.CanImpregnate(pawn, partner, sextype))
						{
							PregnancyHelper.Doimpregnate(pawn, partner);
							return false;
						}
						else comp.CumIn(pawn, Utility.GetCumVolume(pawn), 0);
					}
				}
				else if (Genital_Helper.has_vagina(pawn, pawnparts))
				{
					HediffComp_Menstruation comp = Utility.GetMenstruationComp(pawn);
					if (comp != null)
					{
						if (Genital_Helper.has_penis_fertile(partner, partnerparts) && PregnancyHelper.CanImpregnate(partner, pawn, sextype))
						{
							PregnancyHelper.Doimpregnate(partner, pawn);
							return false;
						}
						else comp.CumIn(partner, Utility.GetCumVolume(partner), 0);
					}
				}
			}
			return true;
			
			
		}
	}

	[HarmonyPatch(typeof(PregnancyHelper), "Doimpregnate")]
	public static class Doimpregnate_Patch
	{
		public static bool Prefix(Pawn pawn, Pawn partner) // partner has vagina
		{
			HediffComp_Menstruation comp = Utility.GetMenstruationComp(partner);
			if (comp != null)
            {
				if (AndroidsCompatibility.IsAndroid(pawn) && !AndroidsCompatibility.AndroidPenisFertility(pawn))
				{
					comp.CumIn(pawn, Utility.GetCumVolume(pawn),0);
					return false;
				}
				else comp.CumIn(pawn, Utility.GetCumVolume(pawn), pawn.health.capacities.GetLevel(xxx.reproduction));
				return false;
			}
			ModLog.Message("used original rjw method: Comp missing");
			return true;
		}
	}









}
