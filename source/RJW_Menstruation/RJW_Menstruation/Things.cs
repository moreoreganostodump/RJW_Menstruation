using System;
using System.Xml;
using System.Collections.Generic;
using RimWorld;
using rjw;
using UnityEngine;
using Verse;

namespace RJW_Menstruation
{
    public class PawnDNAModExtension : DefModExtension
    {
        public string fetusTexPath;
        public ColorInt cumColor;
        public Color CumColor => cumColor.ToColor;
        public float cumThickness = 0f;
        public List<HybridExtension> hybridExtension;

        public HybridExtension GetHybridExtention(string race)
        {
            if (hybridExtension.NullOrEmpty()) return null;
            else
            {
                return hybridExtension.Find(x => x.thingDef.defName.Equals(race));
            }
        }

        public PawnKindDef GetHybridWith(string race)
        {
            return GetHybridExtention(race)?.ChooseOne() ?? null;
        }
    }

    public class HybridExtension
    {

        public Dictionary<string, float> hybridInfo;
        public ThingDef thingDef;
       
        public HybridExtension() { }

        public PawnKindDef ChooseOne()
        {
            if (hybridInfo.EnumerableNullOrEmpty()) return null;
            PawnKindDef res = null;
            do
            {
                res = DefDatabase<PawnKindDef>.GetNamedSilentFail(hybridInfo.RandomElementByWeight(x => x.Value).Key);
                if (res == null) res = DefDatabase<ThingDef>.GetNamedSilentFail(hybridInfo.RandomElementByWeight(x => x.Value).Key).race.AnyPawnKind;

                if (res == null) hybridInfo.Remove(res.defName);
            } while (res == null && !hybridInfo.EnumerableNullOrEmpty());

            return res;
        }


        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            hybridInfo = new Dictionary<string, float>();
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thingDef", xmlRoot.Name);
            XmlNodeList childNodes = xmlRoot.ChildNodes;
            
            if (childNodes.Count >= 1) foreach (XmlNode node in childNodes)
                {
                    //Log.Message(xmlRoot.Name + "HybridInfo: " + node.Name + " " + node.InnerText);
                    hybridInfo.Add(node.Name, ParseHelper.FromString<float>(node.InnerText));
                }



        }

    }





    public class AbsorberModExtension : DefModExtension
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
        public virtual bool LeakAfterDirty => def.GetModExtension<AbsorberModExtension>().leakAfterDirty;
        public virtual bool EffectAfterDirty => def.GetModExtension<AbsorberModExtension>().effectsAfterDirty;
        public virtual ThingDef DirtyDef => def.GetModExtension<AbsorberModExtension>().dirtyDef;
        public virtual int MinHrstoDirtyEffect => def.GetModExtension<AbsorberModExtension>().minHourstoDirtyEffect;

        public Color fluidColor = Color.white;



        public virtual void DirtyEffect() { }

        public virtual void WearEffect()
        {
            absorbedfluids += 0.1f;
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
