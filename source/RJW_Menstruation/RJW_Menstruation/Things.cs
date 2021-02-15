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
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.WoundInfection, Wearer, Genital_Helper.get_genitalsBPR(Wearer));
                Wearer.health.AddHediff(hediff, Genital_Helper.get_genitalsBPR(Wearer));
            }
        }

    }







}
