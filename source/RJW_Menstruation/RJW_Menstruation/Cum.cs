using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace RJW_Menstruation
{
    public class Cum : IExposable
    {
        public Pawn pawn;

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
        private PawnDNAModExtension DNAcache = null;
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


        public Cum() { }

        public Cum(Pawn pawn)
        {
            this.pawn = pawn;
            volume = 1.0f;
            fertvolume = 1.0f;
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
