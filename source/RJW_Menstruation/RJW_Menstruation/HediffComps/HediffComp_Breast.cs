using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using HugsLib;
using rjw;

namespace RJW_Menstruation
{
    public class CompProperties_Breast : HediffCompProperties
    {
        public string BreastTex = "Breasts/Breast";
        public ColorInt BlacknippleColor = new ColorInt(55,20,0);
        

        public Color BlackNippleColor
        {
            get
            {
                return BlacknippleColor.ToColor;
            }
        }
        

        public CompProperties_Breast()
        {
            compClass = typeof(HediffComp_Breast);
        }
    }

    public class HediffComp_Breast : HediffComp
    {
        public const float DEFAULTALPHA = -1;
        public const float DEFAULTAREOLA = -1;
        public const float DEFAULTNIPPLE = -1;
        public const float VARIANT = 0.2f;
        public const int TICKINTERVAL = 3750;
        public const float MAX_BREAST_INCREMENT = 0.10f;

        public CompProperties_Breast Props;

        protected float alphaPermanent = -1;
        protected float alphaCurrent = -1;
        protected float alpha = -1;
        protected float areolaSizePermanent = -1f;
        protected float areolaSizeCurrent = -1f;
        protected float areolaSize = -1f;
        protected float nippleSizePermanent = -1f;
        protected float nippleSizeCurrent = -1f;
        protected float nippleSize = -1f;
        protected float breastSizeIncreased = 0f;
        protected float originalpha = -1f;
        protected float originareola = -1f;
        protected float originnipple = -1f;
        protected Color cachedcolor;
        protected bool loaded = false;
        protected bool pregnant = false;
        public Action action;

        public float MaxAlpha
        {
            get
            {
                return originalpha + Configurations.NippleMaximumTransition;
            }
        }
        public float MaxAreola
        {
            get
            {
                return originareola + Configurations.NippleMaximumTransition;
            }
        }
        public float MaxNipple
        {
            get
            {
                return originnipple + Configurations.NippleMaximumTransition;
            }
        }


        public Color NippleColor
        {
            get
            {
                return cachedcolor;
            }
        }
        public float Alpha
        {
            get
            {
                return alphaCurrent;
            }
        }
        public float NippleSize
        {
            get
            {
                return nippleSizeCurrent;
            }
        }
        public float AreolaSize
        {
            get
            {
                return areolaSizeCurrent;
            }
        }



        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref alphaPermanent, "alphaPermanent", DEFAULTALPHA, true);
            Scribe_Values.Look(ref alphaCurrent, "alphaCurrent", DEFAULTALPHA, true);
            Scribe_Values.Look(ref alpha, "alpha", DEFAULTALPHA, true);
            Scribe_Values.Look(ref areolaSizePermanent, "areolaSizePermanent", DEFAULTAREOLA, true);
            Scribe_Values.Look(ref areolaSizeCurrent, "areolaSizeCurrent", DEFAULTAREOLA, true);
            Scribe_Values.Look(ref areolaSize, "areolaSize", DEFAULTAREOLA, true);
            Scribe_Values.Look(ref nippleSizePermanent, "nippleSizePermanent", DEFAULTNIPPLE, true);
            Scribe_Values.Look(ref nippleSizeCurrent, "nippleSizeCurrent", DEFAULTNIPPLE, true);
            Scribe_Values.Look(ref nippleSize, "nippleSize", DEFAULTNIPPLE, true);
            Scribe_Values.Look(ref breastSizeIncreased, "breastSizeIncreased", breastSizeIncreased, true);
            Scribe_Values.Look(ref originalpha, "originalpha", originalpha, true);
            Scribe_Values.Look(ref originareola, "originareola", originareola, true);
            Scribe_Values.Look(ref originnipple, "originnipple", originnipple, true);
            Scribe_Values.Look(ref pregnant, "pregnant", pregnant, true);
            
        }

        public override void CompPostTick(ref float severityAdjustment) { }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (!loaded) Initialize();
        }

        public override void CompPostPostRemoved()
        {
            HugsLibController.Instance.TickDelayScheduler.TryUnscheduleCallback(action);
            ModLog.Message(parent.pawn.Label + "breast tick scheduler removed");
            base.CompPostPostRemoved();
        }

        public void Initialize()
        {
            Props = (CompProperties_Breast)props;
            action = Transition;
            if (alphaPermanent < 0f)
            {
                alphaPermanent = (Utility.RandGaussianLike(0.0f, 0.3f) + Rand.Range(0.0f,0.5f))/2;
                originalpha = alphaPermanent;
                alpha = alphaPermanent;
                alphaCurrent = alphaPermanent;
            }
            if (areolaSizePermanent < 0f)
            {
                areolaSizePermanent = Utility.RandGaussianLike(0f, parent.Severity);
                originareola = areolaSizePermanent;
                areolaSize = areolaSizePermanent;
                areolaSizeCurrent = areolaSizePermanent;
            }
            if (nippleSizePermanent < 0f)
            {
                nippleSizePermanent = Utility.RandGaussianLike(0f, parent.Severity);
                originnipple = nippleSizePermanent;
                nippleSize = nippleSizePermanent;
                nippleSizeCurrent = nippleSizePermanent;
            }
            UpdateColor();
            loaded = true;
            HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(action, TICKINTERVAL, parent.pawn);
        }

        

        public void Transition()
        {
            alphaCurrent = Mathf.Lerp(alphaCurrent, alpha, Configurations.NippleTransitionRatio);
            areolaSizeCurrent = Mathf.Lerp(areolaSizeCurrent, areolaSize, Configurations.NippleTransitionRatio);
            nippleSizeCurrent = Mathf.Lerp(nippleSizeCurrent, nippleSize, Configurations.NippleTransitionRatio);
            UpdateColor();
            HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(action, TICKINTERVAL, parent.pawn);
            if (pregnant)
            {
                if (breastSizeIncreased < MAX_BREAST_INCREMENT)
                {
                    breastSizeIncreased += 0.02f;
                    parent.Severity += 0.02f;
                }
            }
            else
            {
                if (breastSizeIncreased > 0)
                {
                    breastSizeIncreased -= 0.02f;
                    parent.Severity -= 0.02f;
                }
            }
            
        }

        public void ChangeColorFermanant(float alpha)
        {
            alphaPermanent = alpha;
        }

        public void ChangeColor(float alpha)
        {
            this.alpha = alpha;
        }

        public void PregnancyTransition()
        {
            alphaPermanent = Math.Min(MaxAlpha, alphaPermanent + Configurations.NipplePermanentTransitionVariance.VariationRange(VARIANT));
            areolaSizePermanent = Math.Min(MaxAreola, areolaSizePermanent + Configurations.NipplePermanentTransitionVariance.VariationRange(VARIANT));
            nippleSizePermanent = Math.Min(MaxNipple, nippleSizePermanent + Configurations.NipplePermanentTransitionVariance.VariationRange(VARIANT));
            alpha = Math.Min(MaxAlpha, alpha + Configurations.NippleTransitionVariance.VariationRange(VARIANT));
            areolaSize = Math.Min(MaxAreola, areolaSize + Configurations.NippleTransitionVariance.VariationRange(VARIANT));
            nippleSize = Math.Min(MaxNipple, nippleSize + Configurations.NippleTransitionVariance.VariationRange(VARIANT));
            pregnant = true;
        }

        public void BirthTransition()
        {
            alpha = alphaPermanent;
            areolaSize = areolaSizePermanent;
            nippleSize = nippleSizePermanent;
            pregnant = false;
        }


        public void AdjustBreastSize(float amount)
        {
            parent.Severity += amount;
            breastSizeIncreased += amount;
        }

        public void AdjustNippleSize(float amount)
        {
            nippleSizePermanent = Math.Min(MaxNipple, nippleSizePermanent + amount);
            nippleSize = Math.Min(MaxNipple, nippleSize + amount);
        }

        public void AdjustAreolaSize(float amount)
        {
            areolaSizePermanent = Math.Min(MaxAreola, areolaSizePermanent + amount);
            areolaSize = Math.Min(MaxAreola, areolaSize + amount);
        }

        public void RestoreBreastSize(float ratio)
        {
            float variance = breastSizeIncreased * Math.Min(ratio, 1.0f);
            breastSizeIncreased -= variance;
            parent.Severity -= variance;
            
        }
        
        public void UpdateColor()
        {
            cachedcolor = Colors.CMYKLerp(parent?.pawn?.story?.SkinColor ?? Color.white, Props.BlackNippleColor, Alpha);
        }

        public string DebugInfo()
        {
            return "Alpha: " + alpha + 
                "\nNippleSize: " + nippleSize + 
                "\nAreolaSize: " + areolaSize + 
                "\nAlphaCurrent: " + alphaCurrent +
                "\nNippleSizeCurrent: " + nippleSizeCurrent +
                "\nAreolaSizeCurrent: " + areolaSizeCurrent +
                "\nAlphaOrigin: " + originalpha +
                "\nNippleSizeOrigin: " + originnipple +
                "\nAreolaSizeOrigin: " + originareola +
                "\nAlphaMax: " + MaxAlpha +
                "\nNippleSizeMax: " + MaxNipple +
                "\nAreolaSizeMax: " + MaxAreola +
                "\nPermanentAlpha:" + alphaPermanent + 
                "\nPermanentNipple:" + nippleSizePermanent +
                "\nPermanentAreola:" + areolaSizePermanent;
        }

    }

}
