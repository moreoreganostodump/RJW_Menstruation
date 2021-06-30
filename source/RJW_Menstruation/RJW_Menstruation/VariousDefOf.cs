using RimWorld;
using rjw;
using System.Linq;
using System;
using System.Collections.Generic;
using Verse;

namespace RJW_Menstruation
{
    public static class VariousDefOf
    {

        public static readonly ThingDef CumFilth = DefDatabase<ThingDef>.GetNamed("FilthCum");
        public static readonly ThingDef Tampon = DefDatabase<ThingDef>.GetNamed("Absorber_Tampon");
        public static readonly ThingDef Tampon_Dirty = DefDatabase<ThingDef>.GetNamed("Absorber_Tampon_Dirty");
        public static readonly ThingDef FilthMixture = DefDatabase<ThingDef>.GetNamed("FilthMixture");
        public static readonly HediffDef RJW_IUD = DefDatabase<HediffDef>.GetNamed("RJW_IUD");
        public static readonly HediffDef Hediff_MenstrualCramp = DefDatabase<HediffDef>.GetNamed("Hediff_MenstrualCramp");
        public static readonly HediffDef Hediff_Climacteric = DefDatabase<HediffDef>.GetNamed("Hediff_Climacteric");
        public static readonly HediffDef Hediff_Menopause = DefDatabase<HediffDef>.GetNamed("Hediff_Menopause");
        public static readonly HediffDef Hediff_Estrus = DefDatabase<HediffDef>.GetNamed("Hediff_Estrus");
        public static readonly HediffDef Hediff_Estrus_Consealed = DefDatabase<HediffDef>.GetNamed("Hediff_Estrus_Consealed");
        public static readonly StatDef MaxAbsorbable = DefDatabase<StatDef>.GetNamed("MaxAbsorbable");
        public static readonly PawnRelationDef Relation_birthgiver = DefDatabase<PawnRelationDef>.AllDefs.FirstOrDefault(d => d.defName == "RJW_Sire");
        public static readonly PawnRelationDef Relation_spawn = DefDatabase<PawnRelationDef>.AllDefs.FirstOrDefault(d => d.defName == "RJW_Pup");
        public static readonly NeedDef SexNeed = DefDatabase<NeedDef>.GetNamed("Sex");
        public static readonly JobDef VaginaWashing = DefDatabase<JobDef>.GetNamed("VaginaWashing");
        public static readonly JobDef Job_LactateSelf = DefDatabase<JobDef>.GetNamed("LactateSelf");
        public static readonly ThoughtDef LeakingFluids = DefDatabase<ThoughtDef>.GetNamed("LeakingFluids");
        public static readonly ThoughtDef CameInsideF = DefDatabase<ThoughtDef>.GetNamed("CameInsideF");
        public static readonly ThoughtDef CameInsideFLowFert = DefDatabase<ThoughtDef>.GetNamed("CameInsideFLowFert");
        public static readonly ThoughtDef CameInsideFFetish = DefDatabase<ThoughtDef>.GetNamed("CameInsideFFetish");
        public static readonly ThoughtDef CameInsideFFetishSafe = DefDatabase<ThoughtDef>.GetNamed("CameInsideFFetishSafe");
        public static readonly ThoughtDef HaterCameInsideFSafe = DefDatabase<ThoughtDef>.GetNamed("HaterCameInsideFSafe");
        public static readonly ThoughtDef HaterCameInsideF = DefDatabase<ThoughtDef>.GetNamed("HaterCameInsideF");
        public static readonly ThoughtDef CameInsideM = DefDatabase<ThoughtDef>.GetNamed("CameInsideM");
        public static readonly ThoughtDef HaterCameInsideM = DefDatabase<ThoughtDef>.GetNamed("HaterCameInsideM");
        public static readonly ThoughtDef UnwantedPregnancy = DefDatabase<ThoughtDef>.GetNamed("UnwantedPregnancy");
        public static readonly ThoughtDef UnwantedPregnancyMild = DefDatabase<ThoughtDef>.GetNamed("UnwantedPregnancyMild");
        public static readonly ThoughtDef TookContraptivePill = DefDatabase<ThoughtDef>.GetNamed("TookContraptivePill");
        public static readonly ThoughtDef HateTookContraptivePill = DefDatabase<ThoughtDef>.GetNamed("HateTookContraptivePill");
        public static readonly HediffDef_PartBase Vagina = DefDatabase<HediffDef_PartBase>.GetNamed("Vagina");
        public static readonly CompProperties_Menstruation VaginaCompProperties = (CompProperties_Menstruation)Vagina.comps.FirstOrDefault(x => x is CompProperties_Menstruation);
        public static readonly KeyBindingDef OpenStatusWindowKey = DefDatabase<KeyBindingDef>.GetNamed("OpenStatusWindow");
        public static readonly PawnColumnDef RJW_EarnedMoneyByWhore = DefDatabase<PawnColumnDef>.GetNamed("RJW_EarnedMoneyByWhore");

        private static List<ThingDef> allraces = null;
        private static List<PawnKindDef> allkinds = null;

        public static List<ThingDef> AllRaces
        {
            get
            {
                if (allraces == null)
                {
                    
                    List<ThingDef> allThings = DefDatabase<ThingDef>.AllDefsListForReading;
                    allraces = allThings.FindAll(x => x.race != null && x.race.IsFlesh);
                }
                return allraces;
            }
        }
        public static List<PawnKindDef> AllKinds
        {
            get
            {
                if (allkinds == null)
                {
                    List<PawnKindDef> allKinds = DefDatabase<PawnKindDef>.AllDefsListForReading;
                    allkinds = allKinds.FindAll(x => x.race != null);
                }
                return allkinds;
            }
        }



        // Defs from Milkable Colonists
        public static readonly HediffDef Hediff_Lactating_Drug = DefDatabase<HediffDef>.GetNamedSilentFail("Lactating_Drug");
        public static readonly HediffDef Hediff_Lactating_Natural = DefDatabase<HediffDef>.GetNamedSilentFail("Lactating_Natural");
        public static readonly HediffDef Hediff_Lactating_Permanent = DefDatabase<HediffDef>.GetNamedSilentFail("Lactating_Permanent");
        public static readonly HediffDef Hediff_Heavy_Lactating_Permanent = DefDatabase<HediffDef>.GetNamedSilentFail("Heavy_Lactating_Permanent");
        public static readonly JobDef Job_LactateSelf_MC = DefDatabase<JobDef>.GetNamedSilentFail("LactateSelf_MC");







    }
}
