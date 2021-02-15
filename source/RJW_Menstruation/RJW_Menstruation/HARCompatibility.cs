using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace RJW_Menstruation
{
    public static class HARCompatibility
    {

        public static bool IsHAR(this Pawn pawn)
        {
            return pawn.def.GetType().ToString().StartsWith("AlienRace");
        }

        public static ThingComp GetHARComp(this Pawn pawn)
        {
            return pawn?.GetComps<ThingComp>()?.First(x => x.GetType().Namespace.EqualsIgnoreCase("AlienRace") && x.GetType().Name.EndsWith("AlienComp"));
        }

        public static string GetHARCrown(this Pawn pawn)
        {
            return (string)pawn.GetHARComp().GetMemberValue("crownType");
        }

        public static void SetHARCrown(this Pawn pawn, string crown)
        {
            pawn.GetHARComp().SetMemberValue("crownType", crown);
        }


    }
}
