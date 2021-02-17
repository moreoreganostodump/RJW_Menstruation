using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using rjw;

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
