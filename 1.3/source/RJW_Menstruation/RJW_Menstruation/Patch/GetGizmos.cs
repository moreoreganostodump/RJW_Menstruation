using HarmonyLib;
using rjw;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RJW_Menstruation
{
    [HarmonyPatch(typeof(Pawn), "GetGizmos")]
    public class Pawn_GetGizmos
    {
        public static void Postfix(ref IEnumerable<Gizmo> __result, Pawn __instance)
        {
            List<Gizmo> gizmoList = __result.ToList();

            if (!__instance.ShowStatus())
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
            HediffComp_Menstruation comp = __instance.GetMenstruationComp();
            if (comp != null) gizmoList.Add(CreateGizmo_WombStatus(__instance, comp));

        }

        private static Gizmo CreateGizmo_WombStatus(Pawn pawn, HediffComp_Menstruation comp)
        {
            Texture2D icon, icon_overay;
            string description = "";
            if (Configurations.Debug) description += comp.curStage + ": " + comp.curStageHrs + "\n" + "fertcums: " + comp.TotalFertCum + "\n" + "ovarypower: " + comp.ovarypower + "\n" + "eggs: " + comp.GetNumofEggs + "\n";
            else description += comp.GetCurStageLabel + "\n";
            if (pawn.IsPregnant())
            {
                Hediff hediff = PregnancyHelper.GetPregnancy(pawn);
                if (Utility.ShowFetusImage((Hediff_BasePregnancy)hediff))
                {
                    icon = comp.GetPregnancyIcon(hediff);
                    if (hediff is Hediff_BasePregnancy)
                    {
                        Hediff_BasePregnancy h = (Hediff_BasePregnancy)hediff;
                        if (h.GestationProgress < 0.2f) icon_overay = comp.GetCumIcon();
                        else icon_overay = ContentFinder<Texture2D>.Get(("Womb/Empty"), true);
                    }
                    else icon_overay = ContentFinder<Texture2D>.Get(("Womb/Empty"), true);
                }
                else
                {
                    icon = comp.GetWombIcon();
                    icon_overay = comp.GetCumIcon();
                }
            }
            else
            {
                Hediff hediff = pawn.health.hediffSet.GetHediffs<Hediff_InsectEgg>().FirstOrDefault();
                if (hediff != null)
                {
                    icon = ContentFinder<Texture2D>.Get(("Womb/Womb_Egged"), true);
                    icon_overay = ContentFinder<Texture2D>.Get(("Womb/Empty"), true);
                }
                else
                {
                    icon = comp.GetWombIcon();
                    icon_overay = comp.GetCumIcon();
                }
            }
            foreach (string s in comp.GetCumsInfo) description += s + "\n";

            Color c = comp.GetCumMixtureColor;

            Gizmo gizmo = new Gizmo_Womb
            {
                defaultLabel = pawn.LabelShort,
                defaultDesc = description,
                icon = icon,
                icon_overay = icon_overay,
                shrinkable = Configurations.AllowShrinkIcon,
                cumcolor = c,
                comp = comp,
                order = 100,
                hotKey = VariousDefOf.OpenStatusWindowKey,
                action = delegate
                {
                    Dialog_WombStatus.ToggleWindow(pawn, comp);
                }
            };
            
            return gizmo;
        }
    }






}
