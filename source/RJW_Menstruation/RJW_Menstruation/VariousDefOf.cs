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
        public static readonly DNADef defaultDNA = new DNADef
        {
            fetusTexPath = "Fetus/Fetus_Default",
            cumColor = new ColorInt(255, 255, 255, 255),
            cumThickness = 0
        };

        public static readonly ThingDef CumFilth = DefDatabase<ThingDef>.GetNamed("FilthCum");
        public static readonly HediffDef RJW_IUD = DefDatabase<HediffDef>.GetNamed("RJW_IUD");
        public static readonly HediffDef Hediff_MenstrualCramp = DefDatabase<HediffDef>.GetNamed("Hediff_MenstrualCramp");

    }
}
