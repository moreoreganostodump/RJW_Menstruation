using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection;
using Verse;

namespace RJW_Menstruation.Sexperience
{
    [StaticConstructorOnStartup]
    internal static class First
    {
        static First()
        {
            var har = new Harmony("RJW_Menstruation.Sexperience");
            har.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
