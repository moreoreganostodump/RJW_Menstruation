using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using Milk;
using RJW_Menstruation;

namespace MilkModule
{
    internal static class First
    {
        static First()
        {
            var har = new Harmony("RJW_Menstruation_MilkModule");
            har.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(HumanCompHasGatherableBodyResource), "Gathered")]
    public static class Milk_Patch
    {
        public static void Postfix(Pawn doer, HumanCompHasGatherableBodyResource __instance)
        {
            Pawn pawn = __instance.parent as Pawn;
            HediffComp_Breast comp = null;
            if (pawn != null) comp = pawn.GetBreastComp();
            if (comp != null)
            {
                comp.AdjustAreolaSize(Rand.Range(0.0f, 0.01f * Configurations.NipplePermanentTransitionVariance));
                comp.AdjustNippleSize(Rand.Range(0.0f, 0.01f * Configurations.NipplePermanentTransitionVariance));
            }

        }

    }
}
