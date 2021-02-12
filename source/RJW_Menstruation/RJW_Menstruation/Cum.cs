using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using rjw;

namespace RJW_Menstruation
{
    public class Cum : IExposable
    {
        public Pawn pawn;

        //public bool failedFertilization = false;
        //public bool dead = false;
        public float volume; // ml
        public float fertvolume;
        public float fertFactor = 1.0f;
        public bool notcum = false; // for other fluids
        public string notcumLabel = "";
        private bool useCustomColor = false;
        private float notcumthickness = 0;
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
        private Color customColor;
        public DNADef DNA
        {
            get
            {
                if (DNAcache == null)
                {
                    DNAcache = DefDatabase<DNADef>.GetNamedSilentFail(pawn?.def.defName ?? "Human");
                    if (DNAcache == null)
                    {
                        DNAcache = VariousDefOf.HumanDNA;
                    }
                    return DNAcache;
                }
                else return DNAcache;
            }
        }
        private DNADef DNAcache = null;
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
        private ThingDef filthDef = null;
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


        public Cum() {}

        public Cum(Pawn pawn)
        {
            this.pawn = pawn;
            volume = 1.0f;
            fertvolume = 1.0f;
            decayresist = 0;
        }

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
            if (fertility > 0)
            {
                this.fertvolume = volume;
                this.fertFactor = fertility;
            }
            else this.fertvolume = 0;
            this.filthDef = filthDef;
        }



        public void ExposeData()
        {
            Scribe_References.Look(ref pawn, "pawn", true);
            Scribe_Defs.Look(ref DNAcache, "DNAcache");
            Scribe_Values.Look(ref volume, "volume", volume, true);
            Scribe_Values.Look(ref fertvolume, "fertvolume", fertvolume, true);
            Scribe_Values.Look(ref notcumthickness, "notcumthickness", notcumthickness, true);
            Scribe_Values.Look(ref fertFactor, "fertFactor", fertFactor, true);
            Scribe_Values.Look(ref notcum, "notcum", notcum, true);
            Scribe_Values.Look(ref notcumLabel, "notcumLabel", notcumLabel, true);
            Scribe_Values.Look(ref useCustomColor, "useCustomColor", useCustomColor, true);
            Scribe_Values.Look(ref customColor, "customColor", customColor, true);

        }
    }






}
