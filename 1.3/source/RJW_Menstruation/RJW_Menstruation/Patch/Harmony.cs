using HarmonyLib;
using System.Reflection;
using Verse;

namespace RJW_Menstruation
{
    [StaticConstructorOnStartup]
    internal static class First
    {
        static First()
        {
            var har = new Harmony("RJW_Menstruation");
            har.PatchAll(Assembly.GetExecutingAssembly());
            
        }
    }




}
