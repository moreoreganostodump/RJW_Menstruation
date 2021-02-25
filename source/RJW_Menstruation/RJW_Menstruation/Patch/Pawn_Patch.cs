using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RimWorld;
using Verse;
using HarmonyLib;
using HugsLib;
using rjw;

namespace RJW_Menstruation
{

    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    public class Pawn_Patch
    {
        public static void Postfix(Map map, bool respawningAfterLoad, Pawn __instance)
        {
            //Log.Message("Initialize on spawnsetup");
            HediffComp_Menstruation comp = Utility.GetMenstruationComp(__instance);
            if (comp != null)
            {
                HugsLibController.Instance.TickDelayScheduler.TryUnscheduleCallback(comp.actionref);
                comp.Initialize();
            }
        }
    }

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public class HumanlikeOrder_Patch
    {
        public static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            var selftargets = GenUI.TargetsAt_NewTemp(clickPos, TargetingParameters.ForSelf(pawn));

            foreach (LocalTargetInfo t in selftargets)
            {
                if (pawn.HasMenstruationComp()) opts.AddDistinct(MakeSelfMenu(pawn, t));
                break;
            }
            
            


        }
        
        public static FloatMenuOption MakeSelfMenu(Pawn pawn, LocalTargetInfo target)
        {
            FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Translations.FloatMenu_CleanSelf, delegate ()
                 {
                     pawn.jobs.TryTakeOrderedJob_NewTemp(new Verse.AI.Job(VariousDefOf.VaginaWashing, null, null, target.Cell));
                 }, MenuOptionPriority.Low), pawn, target);

            return option;
        }



    }
    
    //[HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain_NewTmp")]
    //public class OptimizeApparel_Patch
    //{
    //    public static bool Prefix(ref float __result, Pawn pawn, Apparel ap, List<float> wornScoresCache)
    //    {
    //        if (ap is Absorber)
    //        {
    //            __result = -1000f;
    //            return false;
    //        }
    //        return true;
    //    }
    //
    //}




}
