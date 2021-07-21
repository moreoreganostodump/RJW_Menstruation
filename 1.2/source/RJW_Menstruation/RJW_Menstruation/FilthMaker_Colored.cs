using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RJW_Menstruation
{
    public class FilthMaker_Colored
    {

        public static bool TryMakeFilth(IntVec3 c, Map map, ThingDef filthDef, IEnumerable<string> sources, Color color, bool shouldPropagate, FilthSourceFlags additionalFlags = FilthSourceFlags.None)
        {
            Filth_Colored filth = (Filth_Colored)(from t in c.GetThingList(map)
                                                  where t.def == filthDef
                                                  select t).FirstOrDefault<Thing>();
            if (!c.Walkable(map) || (filth != null && !filth.CanBeThickened))
            {
                if (shouldPropagate)
                {
                    List<IntVec3> list = GenAdj.AdjacentCells8WayRandomized();
                    for (int i = 0; i < 8; i++)
                    {
                        IntVec3 c2 = c + list[i];
                        if (c2.InBounds(map) && TryMakeFilth(c2, map, filthDef, sources, color, false, FilthSourceFlags.None))
                        {
                            return true;
                        }
                    }
                }
                if (filth != null)
                {
                    filth.AddSources(sources);
                }
                return false;
            }
            if (filth != null)
            {
                filth.ThickenFilth();
                filth.AddSources(sources);
            }
            else
            {
                if (!FilthMaker.CanMakeFilth(c, map, filthDef, additionalFlags))
                {
                    return false;
                }
                Filth_Colored filth2 = (Filth_Colored)ThingMaker.MakeThing(filthDef, null);
                filth2.DrawColor = color;
                filth2.AddSources(sources);
                GenSpawn.Spawn(filth2, c, map, WipeMode.Vanish);
            }
            //FilthMonitor.Notify_FilthSpawned();
            return true;
        }


    }
}
