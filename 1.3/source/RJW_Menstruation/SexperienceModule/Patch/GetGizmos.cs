using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using RJWSexperience;
using rjw;
using HarmonyLib;


namespace RJW_Menstruation.Sexperience
{
    [HarmonyPatch(typeof(Pawn_GetGizmos), "AddMenstruationGizmos")]
    public static class GetGizmos_Patch
    {
        public static void Postfix(Pawn pawn, HediffComp_Menstruation comp, ref List<Gizmo> gizmolist)
        {
            gizmolist.Add(CreateGizmo_GatherCum(pawn, comp));
        }
        
        private static Gizmo CreateGizmo_GatherCum(Pawn pawn, HediffComp_Menstruation comp)
        {
            Texture2D icon = TextureCache.GatherCum_Bucket;
            string label = Keyed.RS_GatherCum;
            string description = Keyed.RS_GatherCum;
            Gizmo gizmo = new Command_Toggle
            {
                defaultLabel = label,
                defaultDesc = description,
                icon = icon,
                isActive = delegate() { return comp.DoCleanWomb; },
                toggleAction = delegate
                {
                    comp.DoCleanWomb = !comp.DoCleanWomb;
                }
            };

            return gizmo;
        }

    }
}
