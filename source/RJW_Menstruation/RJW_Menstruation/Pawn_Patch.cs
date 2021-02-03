using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using HugsLib;


namespace RJW_Menstruation
{

    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    public class Pawn_Patch
    {
        public static void Postfix(Map map, bool respawningAfterLoad, Pawn __instance)
        {
            Log.Message("Initialize on spawnsetup");
            HediffComp_Menstruation comp = Utility.GetMenstruationComp(__instance);
            if (comp != null)
            {
                HugsLibController.Instance.TickDelayScheduler.TryUnscheduleCallback(comp.actionref);
                comp.Initialize();
            }
        }



    }
}
