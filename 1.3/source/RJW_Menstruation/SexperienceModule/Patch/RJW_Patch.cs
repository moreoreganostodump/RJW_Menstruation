using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.AI;
using rjw;
using RJW_Menstruation;
using HarmonyLib;
using RJWSexperience;

namespace RJW_Menstruation.Sexperience
{
    [HarmonyPatch(typeof(WorkGiver_CleanSelf))]
    public static class RJW_Patch_WorkGiver_CleanSelf
    {
        [HarmonyPrefix]
        [HarmonyPatch("HasJobOnThing")]
        public static bool HasJobOnThing(Pawn pawn, Thing t, bool forced, ref bool __result)
        {
            HediffComp_Menstruation comp = pawn.GetMenstruationComp();
            if (comp != null && comp.DoCleanWomb && comp.TotalCumPercent > 0.001f)
            {
                __result = true;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch("JobOnThing")]
        public static void JobOnThing(Pawn pawn, Thing t, bool forced, ref Job __result)
        {
            HediffComp_Menstruation comp = pawn.GetMenstruationComp();
            if (comp != null && comp.DoCleanWomb && comp.TotalCumPercent > 0.001f)
            {

                Building_CumBucket bucket = pawn.FindClosestBucket();
                if (bucket != null)
                {
                    __result = JobMaker.MakeJob(VariousDefOf.VaginaWashingwithBucket, null, bucket, bucket.Position);
                }
            }

        }



    }
}
