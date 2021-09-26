using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RJW_Menstruation;
using HarmonyLib;
using rjw;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;
using RJWSexperience;

namespace RJW_Menstruation.Sexperience
{
    [HarmonyPatch(typeof(Dialog_WombStatus), "DrawWomb")]
    public static class Menstruation_Patch_Dialog_WombStatus
    {
        public const float ICONSIZE = 42f;

        public static void Postfix(Rect rect, Dialog_WombStatus __instance)
        {
            Rect buttonRect = new Rect(rect.x, rect.yMax - ICONSIZE, ICONSIZE, ICONSIZE).ContractedBy(2f);
            if (__instance.Comp.DoCleanWomb)
            {
                Widgets.DrawTextureFitted(buttonRect,TextureCache.GatherCum_Bucket,1.0f);
                TooltipHandler.TipRegion(buttonRect, Translations.Dialog_DoCleanWomb_Tooltip);
            }
            else
            {
                Widgets.DrawTextureFitted(buttonRect, TextureCache.GatherCum_Pussy, 1.0f);
                TooltipHandler.TipRegion(buttonRect, Translations.Dialog_DontCleanWomb_Tooltip);
            }

            if (Widgets.ButtonInvisible(buttonRect))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                __instance.Comp.DoCleanWomb = !__instance.Comp.DoCleanWomb;
            }
        }
    }



}
