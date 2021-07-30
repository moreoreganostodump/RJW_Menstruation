using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RJW_Menstruation
{
    public class Cum : IExposable
    {
        public Pawn pawn;

        protected float volume; // ml
        protected float fertvolume;
        public float fertFactor = 1.0f;
        public bool notcum = false; // for other fluids
        public string notcumLabel = "";
        protected bool useCustomColor = false;
        protected float notcumthickness = 0;
        protected float cumthickness = 1.0f;

        public float Volume
        {
            get
            {
                return volume;
            }
        }

        public float FertVolume
        {
            get
            {
                return fertvolume;
            }
        }

        public float CumThickness
        {
            get
            {
                return cumthickness;
            }
        }

        public float decayresist
        {
            get
            {
                if (!notcum) return DNA.cumThickness;
                else return notcumthickness;
            }
            set
            {
                notcumthickness = value;
            }
        }
        protected Color customColor;

        public PawnDNAModExtension DNA
        {
            get
            {
                if (DNAcache == null)
                {
                    try
                    {
                        DNAcache = pawn.def.GetModExtension<PawnDNAModExtension>();
                    }
                    catch (NullReferenceException)
                    {
                        DNAcache = ThingDefOf.Human.GetModExtension<PawnDNAModExtension>();
                    }
                    if (DNAcache == null)
                    {
                        DNAcache = ThingDefOf.Human.GetModExtension<PawnDNAModExtension>();
                    }
                    return DNAcache;
                }
                else return DNAcache;
            }
        }
        protected PawnDNAModExtension DNAcache = null;
        public ThingDef FilthDef
        {
            get
            {
                if (filthDef == null) return VariousDefOf.CumFilth;
                else return filthDef;
            }
            set
            {
                filthDef = value;
            }
        }
        protected ThingDef filthDef = null;
        public Color color
        {
            get
            {
                if (!useCustomColor) return DNA.CumColor;
                else return customColor;
            }

            set
            {
                useCustomColor = true;
                customColor = value;
            }
        }


        public Cum() { }

        public Cum(Pawn pawn)
        {
            this.pawn = pawn;
            volume = 1.0f;
            fertvolume = 1.0f;
        }

        /// <summary>
        /// Not Cum
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="volume"></param>
        /// <param name="notcumlabel"></param>
        /// <param name="decayresist"></param>
        /// <param name="filthDef"></param>
        public Cum(Pawn pawn, float volume, string notcumlabel, float decayresist = 0, ThingDef filthDef = null)
        {
            this.pawn = pawn;
            this.volume = volume;
            this.fertvolume = volume;
            this.notcum = true;
            this.notcumLabel = notcumlabel;
            this.notcumthickness = decayresist;
            this.filthDef = filthDef;
        }

        public Cum(Pawn pawn, float volume, float fertility, ThingDef filthDef = null)
        {
            this.pawn = pawn;
            this.volume = volume;
            this.fertvolume = volume * fertility;
            this.filthDef = filthDef;
        }



        public virtual void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn", true);
            Scribe_Values.Look(ref volume, "volume", volume, true);
            Scribe_Values.Look(ref fertvolume, "fertvolume", fertvolume, true);
            Scribe_Values.Look(ref notcumthickness, "notcumthickness", notcumthickness, true);
            Scribe_Values.Look(ref fertFactor, "fertFactor", fertFactor, true);
            Scribe_Values.Look(ref notcum, "notcum", notcum, true);
            Scribe_Values.Look(ref notcumLabel, "notcumLabel", notcumLabel, true);
            Scribe_Values.Look(ref useCustomColor, "useCustomColor", useCustomColor, true);
            Scribe_Values.Look(ref customColor, "customColor", customColor, true);
            Scribe_Defs.Look(ref filthDef, "filthDef");

        }

        public void MakeThinner(int speed)
        {
            cumthickness = cumthickness.LerpMultiple(decayresist, 0.3f, speed);
        }

        public void MergeWithCum(float volumein, float fertility,  ThingDef updatefilthDef = null)
        {
            if (updatefilthDef != null) filthDef = updatefilthDef;
            volume += volumein;
            fertvolume += volumein*fertility;
            cumthickness = Mathf.Lerp(cumthickness, 1.0f, volumein / volume);
        }

        public void MergeWithFluid(float volumein, float thickness, ThingDef updatefilthDef = null)
        {
            if (updatefilthDef != null) filthDef = updatefilthDef;
            volume += volumein;
            cumthickness = Mathf.Lerp(cumthickness, thickness, volumein / volume);
        }

        public bool ShouldRemove()
        {
            if (fertvolume < 0.001f && volume < 0.001f) return true;
            return false;
        }


        public float DismishNatural(float leakfactor, float antisperm = 0.0f)
        {
            float totalleak = volume;
            volume *= Math.Max(0, (1 - (Configurations.CumDecayRatio * (1 - decayresist)) * leakfactor));
            fertvolume *= Math.Max(0, 1 - (Configurations.CumFertilityDecayRatio * (1 - decayresist) + antisperm));
            CutMinor();
            totalleak -= volume;
            return totalleak;
        }
        
        public float DismishForce(float portion, float leakfactor = 1.0f)
        {
            float totalleak = volume;
            volume *= Math.Max(0, 1 - (portion * (1 - decayresist)) * leakfactor);
            fertvolume *= Math.Max(0, 1 - (portion * (1 - decayresist)) * leakfactor);
            CutMinor();
            totalleak -= volume;
            return totalleak;

        }

        protected void CutMinor()
        {
            if (volume < 0.01f) volume = 0f;
            if (fertvolume < 0.001f) fertvolume = 0f;

        }


    }

    public class CumMixture : Cum, IDisposable
    {
        protected List<string> cums;


        public CumMixture()
        {
            notcum = true;
            cums = new List<string>();
        }

        public CumMixture(Pawn pawn, float volume, List<string> cums, Color color,  ThingDef mixtureDef)
        {
            this.pawn = pawn;
            this.volume = volume;
            this.cums = cums;
            this.customColor = color;
            this.useCustomColor = true;
        }

        public void Dispose()
        {
            cums.Clear();
            
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref cums, "cumslabel", LookMode.Value, new object[0]);
        }

        public string GetIngredients()
        {
            string res = "";
            if (!cums.NullOrEmpty()) for(int i=0; i<cums.Count; i++)
                {
                    res += cums[i];
                    if (i < cums.Count - 1) res += ", ";
                }
            return res;
        }

    }




}
