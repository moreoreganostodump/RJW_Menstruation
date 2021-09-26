using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;
using UnityEngine;
using RJWSexperience;

namespace RJW_Menstruation.Sexperience
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public class HumanlikeOrder_Patch
    {
        public static void Postfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            var targets = GenUI.TargetsAt(clickPos, TargetingParameters.ForBuilding());
            HediffComp_Menstruation comp = pawn.GetMenstruationComp();

            if (comp != null && comp.TotalCumPercent > 0.001f)
            foreach (LocalTargetInfo t in targets)
            {
                Building building = t.Thing as Building;
                if (building != null)
                {
                    if (building is Building_CumBucket)
                    {
                        opts.AddDistinct(MakeMenu(pawn, building));
                        break;
                    }
                }
            }




        }

        public static FloatMenuOption MakeMenu(Pawn pawn, LocalTargetInfo target)
        {
            FloatMenuOption option = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(Keyed.RS_GatherCum, delegate ()
            {
                pawn.jobs.TryTakeOrderedJob(new Verse.AI.Job(VariousDefOf.VaginaWashingwithBucket, null, target, target.Cell));
            }, MenuOptionPriority.Low), pawn, target);

            return option;
        }
    }
}
