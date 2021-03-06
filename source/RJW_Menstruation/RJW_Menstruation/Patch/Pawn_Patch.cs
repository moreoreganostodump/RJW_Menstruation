using HarmonyLib;
using HugsLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RJW_Menstruation
{

    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    public class Pawn_Patch
    {
        public static void Postfix(Map map, bool respawningAfterLoad, Pawn __instance)
        {
            //Log.Message("Initialize on spawnsetup");
            HediffComp_Menstruation comp = __instance.GetMenstruationComp();
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

    //[HarmonyPatch(typeof(HealthCardUtility), "DrawHediffListing")]
    //public class DrawHediffListing_Patch
    //{
    //    public const float buttonWidth = 80f;
    //    public const float buttonHeight = 20f;
    //
    //    public static void Postfix(Rect rect, Pawn pawn, bool showBloodLoss)
    //    {
    //        if (Configurations.EnableButtonInHT && pawn.HasMenstruationComp())
    //        {
    //            Rect buttonrect = new Rect(rect.xMax - buttonWidth, rect.yMax - buttonHeight, buttonWidth, buttonHeight);
    //            if (Widgets.ButtonText(buttonrect, "Status"))
    //            {
    //                Dialog_WombStatus.ToggleWindow(pawn,pawn.GetMenstruationComp());
    //            }
    //        }
    //
    //
    //    }
    //}

    [HarmonyPatch(typeof(HealthCardUtility), "DrawHediffRow")]
    public class DrawHediffRow_Patch
    {
        public const float buttonWidth = 50f;
        public const float buttonHeight = 20f;

        public static void Prefix(Rect rect, Pawn pawn, IEnumerable<Hediff> diffs, ref float curY)
        {
            if (Configurations.EnableButtonInHT && pawn.ShowStatus())
            {
                HediffComp_Menstruation comp = diffs.First().GetMenstruationComp();
                if (comp != null)
                {
                    Rect buttonrect = new Rect((rect.xMax) / 2 - 5f, curY + 2f, buttonWidth, buttonHeight);
                    if (Widgets.ButtonText(buttonrect, Translations.Button_HealthTab))
                    {
                        Dialog_WombStatus.ToggleWindow(pawn, comp);
                    }
                }
            }

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
