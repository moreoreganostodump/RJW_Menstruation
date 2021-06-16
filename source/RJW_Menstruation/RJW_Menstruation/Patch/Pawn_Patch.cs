using HarmonyLib;
using HugsLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using rjw;

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
            HediffComp_Breast bcomp = __instance.GetBreastComp();
            if (bcomp != null)
            {
                HugsLibController.Instance.TickDelayScheduler.TryUnscheduleCallback(bcomp.action);
                bcomp.Initialize();
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


    //Merged to RJW
    //[HarmonyPatch(typeof(PawnColumnWorker_Pregnant), "GetIconFor")]
    //public class PawnColumnWorker_Patch_Icon
    //{
    //    public static void Postfix(Pawn pawn, ref Texture2D __result)
    //    {
    //        if (pawn.IsVisiblyPregnant()) __result = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Pregnant", true);
    //    }
    //
    //}
    //
    //[HarmonyPatch(typeof(PawnColumnWorker_Pregnant), "GetTooltipText")]
    //public class PawnColumnWorker_Patch_Tooltip
    //{
    //    public static bool Prefix(Pawn pawn, ref string __result)
    //    {
    //        float gestationProgress = PregnancyHelper.GetPregnancy(pawn).Severity;
    //        int num = (int)(pawn.RaceProps.gestationPeriodDays * 60000f);
    //        int numTicks = (int)(gestationProgress * (float)num);
    //        __result = "PregnantIconDesc".Translate(numTicks.ToStringTicksToDays("F0"), num.ToStringTicksToDays("F0"));
    //        return false;
    //    }
    //
    //}
    //
    //[HarmonyPatch(typeof(TransferableUIUtility), "DoExtraAnimalIcons")]
    //public class TransferableUIUtility_Patch_Icon
    //{
    //    private static readonly Texture2D PregnantIcon = ContentFinder<Texture2D>.Get("UI/Icons/Animal/Pregnant", true);
    //    public static void Postfix(Transferable trad, Rect rect, ref float curX)
    //    {
    //        Pawn pawn = trad.AnyThing as Pawn;
    //        if (pawn.IsVisiblyPregnant())
    //        {
    //            Rect rect3 = new Rect(curX - 24f, (rect.height - 24f) / 2f, 24f, 24f);
    //            curX -= 24f;
    //            if (Mouse.IsOver(rect3))
    //            {
    //                TooltipHandler.TipRegion(rect3, PawnColumnWorker_Pregnant.GetTooltipText(pawn));
    //            }
    //            GUI.DrawTexture(rect3, PregnantIcon);
    //        }
    //    }
    //}


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
