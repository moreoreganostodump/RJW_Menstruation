using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using rjw;
using UnityEngine;
using Verse.Sound;

namespace RJW_Menstruation
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_GetGizmos
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
        {
            List<Gizmo> gizmoList = __result.ToList();
            bool isCreatureMine = __instance.Faction != null && (__instance.Faction.IsPlayer || __instance.IsPrisonerOfColony);

            if (!isCreatureMine)
            {
                return;
            }

            if (Configurations.EnableWombIcon && __instance.gender == Gender.Female)
            {
                if (!__instance.IsAnimal())
                {
                    AddWombGizmos(__instance, ref gizmoList);
                }
                else if (Configurations.EnableAnimalCycle)
                {
                    AddWombGizmos(__instance, ref gizmoList);
                }
            }



            
            __result = gizmoList;
        }


        private static void AddWombGizmos(Pawn __instance, ref List<Gizmo> gizmoList)
        {
            HediffComp_Menstruation comp = Utility.GetMenstruationComp(__instance);

            if (comp != null) gizmoList.Add(CreateGizmo_WombStatus(__instance, comp));
        }

        private static Gizmo CreateGizmo_WombStatus(Pawn pawn , HediffComp_Menstruation comp)
        {
            Texture2D icon,icon_overay;
            string description = "";
            if (Configurations.Debug) description += comp.curStage + ": " + comp.curStageHrs + "\n" + "fertcums: " + comp.TotalFertCum + "\n";
            else description += comp.GetCurStageLabel + "\n";
            if (pawn.IsPregnant())
            {
                Hediff hediff = PregnancyHelper.GetPregnancy(pawn);
                icon = Utility.GetPregnancyIcon(comp,hediff);
                if (hediff is Hediff_BasePregnancy)
                {
                    Hediff_BasePregnancy h = (Hediff_BasePregnancy)hediff;
                    if (h.GestationProgress < 0.2f) icon_overay = Utility.GetCumIcon(comp);
                    else icon_overay = ContentFinder<Texture2D>.Get(("Womb/Empty"), true);
                }
                else icon_overay = ContentFinder<Texture2D>.Get(("Womb/Empty"), true);
            }
            else
            {
                icon = Utility.GetWombIcon(comp);
                icon_overay = Utility.GetCumIcon(comp);
            }
            foreach (string s in comp.GetCumsInfo) description += s + "\n";

            Color c = comp.GetCumMixtureColor;

            Gizmo gizmo = new Gizmo_Womb
            {
                defaultLabel = pawn.LabelShort,
                defaultDesc = description,
                icon = icon,
                icon_overay = icon_overay,
                cumcolor = c,
                order = 100,
                action = delegate
                {
                    SoundDefOf.InfoCard_Open.PlayOneShotOnCamera();
                    Find.WindowStack.Add(new Dialog_WombStatus(pawn, comp,icon));
                }
            };
            return gizmo;
        }
    }






}
