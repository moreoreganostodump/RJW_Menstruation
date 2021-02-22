using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using rjw;
using UnityEngine;

namespace RJW_Menstruation
{
    public class PawnDNAModExtention : DefModExtension
    {
        public string fetusTexPath;
        public ColorInt cumColor;
        public Color CumColor => cumColor.ToColor;
        public float cumThickness = 0f;

    }


    public class AbsorberModExtention : DefModExtension
    {
        public bool leakAfterDirty = false;
        public bool effectsAfterDirty = false;
        public ThingDef dirtyDef = null;
        public int minHourstoDirtyEffect = 0;
    }

    public class Absorber : Apparel
    {

        public float absorbedfluids = 0;
        public bool dirty = false;
        public int wearhours = 0;
        public virtual bool LeakAfterDirty => def.GetModExtension<AbsorberModExtention>().leakAfterDirty;
        public virtual bool EffectAfterDirty => def.GetModExtension<AbsorberModExtention>().effectsAfterDirty;
        public virtual ThingDef DirtyDef => def.GetModExtension<AbsorberModExtention>().dirtyDef;
        public virtual int MinHrstoDirtyEffect => def.GetModExtension<AbsorberModExtention>().minHourstoDirtyEffect;

        public Color fluidColor = Color.white;

        

        public virtual void DirtyEffect() {}

        public virtual void WearEffect() 
        {
            absorbedfluids+=0.1f;
            wearhours++;
        }

        public override Color DrawColorTwo => fluidColor;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref absorbedfluids, "absorbedfluids", absorbedfluids, true);
            Scribe_Values.Look(ref dirty, "dirty", dirty, true);
            Scribe_Values.Look(ref wearhours, "wearhours", wearhours, true);
            Scribe_Values.Look(ref fluidColor, "fluidColor", fluidColor, true);
        }

    }

    public class Absorber_Tampon : Absorber
    {

        public override void WearEffect()
        {
            wearhours++;
            absorbedfluids += 0.5f;
        }

        public override void DirtyEffect()
        {
            if (wearhours > MinHrstoDirtyEffect && Rand.Chance(0.02f))
            {
                Wearer.health.AddHediff(HediffDefOf.WoundInfection, Genital_Helper.get_genitalsBPR(Wearer));
            }
        }

    }


    public class Filth_Colored : Filth
    {

        private Color color = Color.white;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref color, "color", color, true);
        }

        public override Color DrawColor
        {
            get
            {
                if (color != Color.white)
                {
                    return color;
                }
                if (Stuff != null)
                {
                    return def.GetColorForStuff(Stuff);
                }
                if (def.graphicData != null)
                {
                    return def.graphicData.color;
                }
                return color;
            }
            set
            {
                color = value;
            }
        }



    }





}
