using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using rjw;

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
        public static readonly HediffDef_PartBase Vagina = DefDatabase<HediffDef_PartBase>.GetNamed("Vagina");
        public static readonly CompProperties_Menstruation VaginaCompProperties = (CompProperties_Menstruation)Vagina.comps.FirstOrDefault(x => x is CompProperties_Menstruation);
        public static readonly KeyBindingDef OpenStatusWindowKey = DefDatabase<KeyBindingDef>.GetNamed("OpenStatusWindow");

        // Defs from Licentia Labs
        public static readonly HediffDef Cumflation = DefDatabase<HediffDef>.GetNamedSilentFail("Cumflation");
        public static readonly ThoughtDef GotOverCumflated = DefDatabase<ThoughtDef>.GetNamedSilentFail("GotOverCumflated");
        public static readonly ThoughtDef GotOverCumflatedEnjoyed = DefDatabase<ThoughtDef>.GetNamedSilentFail("GotOverCumflatedEnjoyed");





    }
}
