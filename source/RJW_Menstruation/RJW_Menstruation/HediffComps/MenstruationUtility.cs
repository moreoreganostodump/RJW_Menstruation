using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;


namespace RJW_Menstruation
{
    public static class MenstruationUtility
    {
        public static float GetFertilityChance(this HediffComp_Menstruation comp)
        {
            return comp.TotalFertCum * Configurations.FertilizeChance;
        }

        

    }
}
