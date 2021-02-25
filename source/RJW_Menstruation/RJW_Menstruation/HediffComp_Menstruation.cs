using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using HugsLib;
using rjw;
using UnityEngine;

namespace RJW_Menstruation
{
    public class CompProperties_Menstruation : HediffCompProperties
    {
        public float maxCumCapacity; // ml
        public float baseImplantationChanceFactor;  
        public float basefertilizationChanceFactor; 
        public float deviationFactor;
        public int folicularIntervalDays = 14; //before ovulation including beginning of bleeding
        public int lutealIntervalDays = 14; //after ovulation until bleeding
        public int bleedingIntervalDays = 6; //must be less than folicularIntervalDays
        public int recoveryIntervalDays = 10; //additional infertile days after gave birth
        public int eggLifespanDays = 2; //fertiledays = ovaluationday - spermlifespan ~ ovaluationday + egglifespanday
        public string wombTex = "Womb/Womb"; //fertiledays = ovaluationday - spermlifespan ~ ovaluationday + egglifespanday
        public string vagTex = "Genitals/Vagina"; //fertiledays = ovaluationday - spermlifespan ~ ovaluationday + egglifespanday
        public bool infertile = false;
        public int ovaryPower = 600000000; // default: almost unlimited ovulation 
        public bool consealedEstrus = false;

        public CompProperties_Menstruation()
        {
            
            compClass = typeof(HediffComp_Menstruation);
        }
    }


    public class CompProperties_Anus : HediffCompProperties
    {
        public string analTex = "Genitals/Anal";

        public CompProperties_Anus()
        {
            compClass = typeof(HediffComp_Anus);
        }
    }



    


    
    public class HediffComp_Menstruation : HediffComp
    {
        const float minmakefilthvalue = 1.0f;
        //const int ovarypowerthreshold = 72;

        public static readonly int tickInterval = 2500; // an hour
        public CompProperties_Menstruation Props;
        public Stage curStage = Stage.Follicular;
        public int curStageHrs = 0;
        public Action actionref;
        public bool loaded = false;
        public int ovarypower = -100000;
        public int eggstack = 0;


        public enum Stage
        {
            Follicular,
            Ovulatory,
            Luteal,
            Bleeding,
            Fertilized, //Obsoleted
            Pregnant,
            Recover,
            None,
            Young,
            ClimactericFollicular,
            ClimactericLuteal,
            ClimactericBleeding,
        }

        private List<Cum> cums;
        private List<Egg> eggs;
        private int follicularIntervalhours = -1;
        private int lutealIntervalhours = -1;
        private int bleedingIntervalhours = -1;
        private int recoveryIntervalhours = -1;
        private float crampPain= -1;
        private Need sexNeed = null;
        private string customwombtex = null;
        private string customvagtex = null;
        private bool estrusflag = false;
        private int opcache = -1;

        public int ovarypowerthreshold
        {
            get
            {
                if (opcache < 0) opcache = (int)(72f * ThingDefOf.Human.race.lifeExpectancy / parent.pawn.def.race.lifeExpectancy);
                return opcache;
            }
        }

        public int FollicularIntervalHours
        {
            get
            {
                return (int)((follicularIntervalhours - bleedingIntervalhours) * CycleFactor);
            }
        }

        public float TotalCum
        {
            get
            {
                float res = 0;
                if (cums.NullOrEmpty()) return 0;
                foreach (Cum cum in cums)
                {
                    res += cum.volume;
                }
                return res;
            }
        }
        public float TotalFertCum
        {
            get
            {
                float res = 0;
                if (cums.NullOrEmpty()) return 0;
                foreach (Cum cum in cums)
                {
                    if (!cum.notcum) res += cum.fertvolume;
                }
                return res;
            }
        }
        public float TotalCumPercent
        {
            get
            {
                float res = 0;
                if (cums.NullOrEmpty()) return 0;
                foreach (Cum cum in cums)
                {
                    res += cum.volume;
                }
                return res/ Props.maxCumCapacity;
            }
        }
        public float CumCapacity
        {
            get
            {
                float res = Props.maxCumCapacity * parent.pawn.BodySize;
                if (curStage != Stage.Pregnant) res *= 500f;
                return res;
            }
        }
        public float CumInFactor
        {
            get
            {
                float res = 1.0f;
                if (parent.pawn.health.hediffSet.HasHediff(VariousDefOf.RJW_IUD)) res = 0.001f;
                return res;
            }
        }
        //make follicular interval into half and double egg lifespan
        public float CycleFactor
        {
            get
            {
                if (xxx.has_quirk(parent.pawn, "Breeder")) return 0.5f;

                return 1.0f;
            }
        }
        //effect on implant chance
        public float ImplantFactor
        {
            get
            {
                float factor = 1.0f;
                if (parent.pawn.Has(Quirk.Breeder)) factor = 10.0f;
                if (xxx.is_animal(parent.pawn)) factor *= RJWPregnancySettings.animal_impregnation_chance/100f;
                else factor *= RJWPregnancySettings.humanlike_impregnation_chance/100f;
                return parent.pawn.health.capacities.GetLevel(xxx.reproduction) * factor;
            }
        }
        public IEnumerable<string> GetCumsInfo
        {
            get
            {
                if (!cums.NullOrEmpty()) foreach (Cum cum in cums)
                    {
                        if (!cum.notcum) yield return String.Format(cum.pawn?.Label + ": {0:0.##}ml", cum.volume);
                        else yield return String.Format(cum.notcumLabel + ": {0:0.##}ml", cum.volume);
                    }
                else yield return Translations.Info_noCum;
            }
        }
        public Color GetCumMixtureColor
        {
            get
            {
                Color mixedcolor = Color.white;

                if (!cums.NullOrEmpty())
                {
                    float mixedsofar = 0;
                    foreach (Cum cum in cums)
                    {
                        mixedcolor = Colors.CMYKLerp(mixedcolor, cum.color, cum.volume/(mixedsofar + cum.volume));
                        mixedsofar += cum.volume;
                    }
                }
                return mixedcolor;
            }
        }
        public string GetCurStageLabel
        {
            get
            {
                switch (curStage)
                {
                    case Stage.Follicular:
                        return Translations.Stage_Follicular;
                    case Stage.Ovulatory:
                        return Translations.Stage_Ovulatory;
                    case Stage.Luteal:
                        return Translations.Stage_Luteal;
                    case Stage.Bleeding:
                        return Translations.Stage_Bleeding;
                    case Stage.Fertilized:
                        return Translations.Stage_Fertilized;
                    case Stage.Pregnant:
                        if (Configurations.InfoDetail == Configurations.DetailLevel.All || (PregnancyHelper.GetPregnancy(parent.pawn)?.Visible ?? false)) return Translations.Stage_Pregnant;
                        else return Translations.Stage_Luteal;
                    case Stage.Recover:
                        return Translations.Stage_Recover;
                    case Stage.None:
                    case Stage.Young:
                        return Translations.Stage_None;
                    case Stage.ClimactericFollicular:
                        return Translations.Stage_Follicular + " - " + Translations.Stage_Climacteric;
                    case Stage.ClimactericLuteal:
                        return Translations.Stage_Luteal + " - " + Translations.Stage_Climacteric;
                    case Stage.ClimactericBleeding:
                        return Translations.Stage_Bleeding + " - " + Translations.Stage_Climacteric;
                    default:
                        return "";
                }
            }

        }

        public string wombTex
        {
            get
            {
                if (customwombtex == null) return Props.wombTex;
                else return customwombtex;
            }
            set
            {
                customwombtex = value;
            }
        }

        public string vagTex
        {
            get
            {
                if (customvagtex == null) return Props.vagTex;
                else return customvagtex;
            }
            set
            {
                customvagtex = value;
            }
        }

        public string GetFertilizingInfo
        {
            get
            {
                string res = "";
                if (!eggs.NullOrEmpty())
                {
                    if (cums.NullOrEmpty() || TotalFertCum == 0) return eggs.Count + " " + Translations.Dialog_WombInfo07;
                    else
                    {
                        int fertilized = 0;
                        foreach (Egg egg in eggs)
                        {
                            if (egg.fertilized) fertilized++;
                        }
                        if (fertilized != 0) res += fertilized + " " + Translations.Dialog_WombInfo05;
                        if (fertilized != 0 && eggs.Count - fertilized != 0) res += ", ";
                        if (eggs.Count - fertilized != 0) res += eggs.Count - fertilized + " " + Translations.Dialog_WombInfo06;
                    }
                }
                return res;
            }
        }

        public bool IsEggFertilizing
        {
            get
            {
                if (!eggs.NullOrEmpty())
                {
                    if (!cums.NullOrEmpty()) foreach(Cum cum in cums)
                        {
                            if (cum.fertvolume > 0) return true;
                        }
                    return false;

                }
                else return false;
            }
        }

        /// <summary>
        /// returns fertstage. if not fertilized returns -1
        /// </summary>
        public int IsFertilized
        {
            get
            {
                if (!eggs.NullOrEmpty()) foreach(Egg egg in eggs)
                    {
                        if (egg.fertilized) return egg.fertstage;
                    }
                return -1;
            }
        }
        public bool IsEggExist
        {
            get
            {
                return !eggs.NullOrEmpty();
            }
        }
        public bool IsDangerDay
        {
            get
            {
                if (curStage == Stage.Follicular || curStage == Stage.ClimactericFollicular)
                {
                    if (curStageHrs > 0.7f * (follicularIntervalhours - bleedingIntervalhours)) return true;
                }
                else if (curStage == Stage.Luteal || curStage == Stage.ClimactericLuteal)
                {
                    if (curStageHrs < Props.eggLifespanDays * 24) return true;
                }
                else if (curStage == Stage.Ovulatory) return true;
                return false;
            }
        }
        public int GetNumofEggs
        {
            get
            {
                if (eggs.NullOrEmpty()) return 0;
                else return eggs.Count;
            }
        }

        public Color BloodColor
        {
            get
            {
                try
                {
                    Color c = parent.pawn.def.race.BloodDef.graphicData.color;
                    return c;
                }
                catch
                {
                    return Colors.blood;
                }

            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Collections.Look(ref cums, saveDestroyedThings: true, label: "cums", lookMode: LookMode.Deep, ctorArgs: new object[0]);
            Scribe_Collections.Look(ref eggs, saveDestroyedThings: true, label: "eggs", lookMode: LookMode.Deep, ctorArgs: new object[0]);
            Scribe_Values.Look(ref curStage, "curStage", curStage, true);
            Scribe_Values.Look(ref curStageHrs, "curStageHrs", curStageHrs, true);
            Scribe_Values.Look(ref follicularIntervalhours, "follicularIntervalhours", follicularIntervalhours, true);
            Scribe_Values.Look(ref lutealIntervalhours, "lutealIntervalhours", lutealIntervalhours, true);
            Scribe_Values.Look(ref bleedingIntervalhours, "bleedingIntervalhours", bleedingIntervalhours, true);
            Scribe_Values.Look(ref recoveryIntervalhours, "recoveryIntervalhours", recoveryIntervalhours, true);
            Scribe_Values.Look(ref crampPain, "crampPain", crampPain, true);
            Scribe_Values.Look(ref ovarypower, "ovarypower", ovarypower, true);
            Scribe_Values.Look(ref eggstack, "eggstack", eggstack, true);
            Scribe_Values.Look(ref estrusflag, "estrusflag", estrusflag, true);


        }

        
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            if (!loaded) Initialize();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            //initializer moved to SpawnSetup
            //if (!loaded)
            //{
            //    Initialize();
            //}
        }

        public override void CompPostPostRemoved()
        {
            
            HugsLibController.Instance.TickDelayScheduler.TryUnscheduleCallback(actionref);
            ModLog.Message(parent.pawn.Label + "tick scheduler removed");
            base.CompPostPostRemoved();
        }




        
        /// <summary>
        /// Get fluid in womb that not a cum
        /// </summary>
        /// <param name="notcumlabel"></param>
        /// <returns></returns>
        public Cum GetNotCum(string notcumlabel)
        {
            if (!cums.NullOrEmpty()) foreach (Cum cum in cums)
                {
                    if (cum.notcum && cum.notcumLabel.Equals(notcumlabel)) return cum;
                }
            return null;
        }

        /// <summary>
        /// Get pawn's cum in womb
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public Cum GetCum(Pawn pawn)
        {
            if (!cums.NullOrEmpty()) foreach (Cum cum in cums)
                {
                    if (!cum.notcum && cum.pawn.Equals(pawn)) return cum;
                }
            return null;
        }
        
        /// <summary>
        /// Inject pawn's cum into womb
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="injectedvolume"></param>
        /// <param name="fertility"></param>
        /// <param name="filthdef"></param>
        public void CumIn(Pawn pawn, float injectedvolume, float fertility = 1.0f, ThingDef filthdef = null)
        {
            float volume = injectedvolume * CumInFactor;
            float cumd = TotalCumPercent;
            float tmp = TotalCum + volume;
            if (tmp > CumCapacity)
            {
                float cumoutrate = 1 - (CumCapacity / tmp);
                bool merged = false;
                if (!cums.NullOrEmpty()) foreach (Cum cum in cums)
                {
                    if (cum.pawn.Equals(pawn))
                    {
                        cum.volume += volume;
                        cum.fertvolume += volume;
                        cum.FilthDef = filthdef;
                        merged = true;
                    }
                    cum.volume *= 1 - cumoutrate;
                    cum.fertvolume *= 1 - cumoutrate;
                }
                if (!merged) cums.Add(new Cum(pawn, volume * (1 - cumoutrate),fertility, filthdef));
            }
            else
            {

                bool merged = false;
                if (!cums.NullOrEmpty()) foreach (Cum cum in cums)
                    {
                        if (cum.pawn.Equals(pawn))
                        {
                            cum.volume += volume;
                            cum.fertvolume += volume;
                            cum.FilthDef = filthdef;
                            merged = true;
                        }
                    }
                if (!merged) cums.Add(new Cum(pawn, volume, fertility, filthdef));
            }
            cumd = TotalCumPercent - cumd;
            AfterCumIn(pawn);
            AfterFluidIn(cumd);
        }

        /// <summary>
        /// Inject pawn's fluid into womb
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="volume"></param>
        /// <param name="notcumlabel"></param>
        /// <param name="decayresist"></param>
        /// <param name="filthdef"></param>
        public void CumIn(Pawn pawn, float volume, string notcumlabel, float decayresist = 0, ThingDef filthdef = null)
        {
            float tmp = TotalCum + volume;
            float cumd = TotalCumPercent;
            if (tmp > CumCapacity)
            {
                float cumoutrate = 1 - (CumCapacity / tmp);
                bool merged = false;
                if (!cums.NullOrEmpty()) foreach (Cum cum in cums)
                    {
                        if (cum.notcum && cum.pawn.Equals(pawn) && cum.notcumLabel.Equals(notcumlabel))
                        {
                            cum.volume += volume;
                            cum.decayresist = decayresist;
                            cum.fertvolume = 0;
                            cum.FilthDef = filthdef;
                            merged = true;
                        }
                        cum.volume *= 1 - cumoutrate;
                        cum.fertvolume *= 1 - cumoutrate;
                    }
                if (!merged) cums.Add(new Cum(pawn, volume * (1 - cumoutrate), notcumlabel,decayresist, filthdef));
            }
            else
            {

                bool merged = false;
                if (!cums.NullOrEmpty()) foreach (Cum cum in cums)
                    {
                        if (cum.notcum && cum.pawn.Equals(pawn) && cum.notcumLabel.Equals(notcumlabel))
                        {
                            cum.volume += volume;
                            cum.decayresist = decayresist;
                            cum.fertvolume = 0;
                            cum.FilthDef = filthdef;
                            merged = true;
                        }
                    }
                if (!merged) cums.Add(new Cum(pawn, volume, notcumlabel,decayresist, filthdef));
            }
            cumd = TotalCumPercent - cumd;
            AfterNotCumIn();
            AfterFluidIn(cumd);
        }

        public void AfterCumIn(Pawn cummer)
        {
            if (xxx.is_human(parent.pawn) && xxx.is_human(cummer))
            {
                if (parent.pawn.GetStatValue(StatDefOf.PawnBeauty) >= 0 || cummer.Has(Quirk.ImpregnationFetish) || cummer.Has(Quirk.Breeder))
                {
                    if (cummer.relations.OpinionOf(parent.pawn) <= -25)
                    {
                        cummer.needs.mood.thoughts.memories.TryGainMemory(VariousDefOf.HaterCameInsideM, parent.pawn);
                    }
                    else
                    {
                        cummer.needs.mood.thoughts.memories.TryGainMemory(VariousDefOf.CameInsideM, parent.pawn);
                    }
                }

                if (IsDangerDay)
                {
                    if (parent.pawn.Has(Quirk.Breeder) || parent.pawn.Has(Quirk.ImpregnationFetish))
                    {
                        parent.pawn.needs.mood.thoughts.memories.TryGainMemory(VariousDefOf.CameInsideFFetish, cummer);
                    }
                    else if (!parent.pawn.relations.DirectRelationExists(PawnRelationDefOf.Spouse, cummer) && !parent.pawn.relations.DirectRelationExists(PawnRelationDefOf.Fiance, cummer))
                    {
                        if (parent.pawn.health.capacities.GetLevel(xxx.reproduction) < 0.50f) parent.pawn.needs.mood.thoughts.memories.TryGainMemory(VariousDefOf.CameInsideFLowFert, cummer);
                        else parent.pawn.needs.mood.thoughts.memories.TryGainMemory(VariousDefOf.CameInsideF, cummer);
                    }
                    else if (parent.pawn.relations.OpinionOf(cummer) <= -5)
                    {
                        parent.pawn.needs.mood.thoughts.memories.TryGainMemory(VariousDefOf.HaterCameInsideF, cummer);
                    }
                }
                else
                {
                    if (parent.pawn.Has(Quirk.Breeder) || parent.pawn.Has(Quirk.ImpregnationFetish))
                    {
                        parent.pawn.needs.mood.thoughts.memories.TryGainMemory(VariousDefOf.CameInsideFFetishSafe, cummer);
                    }
                    else if (parent.pawn.relations.OpinionOf(cummer) <= -5)
                    {
                        parent.pawn.needs.mood.thoughts.memories.TryGainMemory(VariousDefOf.HaterCameInsideFSafe, cummer);
                    }
                }
            }
        }

        public void AfterNotCumIn()
        {

        }

        /// <summary>
        /// Action for both Cum and NotCum
        /// </summary>
        /// <param name="fd">Fluid deviation</param>
        public void AfterFluidIn(float fd)
        {
            //ModLog.Message("LLActivated: " + Configurations.LLActivated);
            //if (Configurations.LLActivated)
            //{
            //    LLCumflationIn(fd);
            //}


        }

        /// <summary>
        /// Cumflation for Licentia Labs
        /// </summary>
        /// <param name="fd"></param>
        public void LLCumflationIn(float fd)
        {
            if (TotalCumPercent > 1.0f)
            {
                ModLog.Message("cumflation in");
                BodyPartRecord genital = Genital_Helper.get_genitalsBPR(parent.pawn);
                HediffWithComps hediff = parent.pawn.health?.hediffSet?.GetHediffs<HediffWithComps>()?.FirstOrDefault(x => x.def == VariousDefOf.Cumflation && x.Part.Equals(genital));
                
                if (hediff == null) // 1.0 fd = 0.002 severity
                {
                    ModLog.Message("hediff null");
                    hediff = (HediffWithComps)HediffMaker.MakeHediff(VariousDefOf.Cumflation, parent.pawn);
                    hediff.Severity = (TotalCumPercent - 1.0f) * 0.002f;
                    parent.pawn.health.AddHediff(hediff, genital);
                    ModLog.Message("added hediff");
                }
                else
                {
                    ModLog.Message("increase severity: " + hediff.Part.Label);
                    hediff.Severity += fd * 0.002f;
                }
            }
        }

        public void LLCumflationOut(float fd)
        {
            HediffWithComps hediff = parent.pawn.health?.hediffSet?.GetHediffs<HediffWithComps>()?.FirstOrDefault(x => x.def == VariousDefOf.Cumflation && x.Part.def.Equals(xxx.genitalsDef));
            if (hediff != null)
            {
                if (TotalCumPercent < 1.0f) parent.pawn.health.RemoveHediff(hediff);
                else
                {
                    ModLog.Message("decrease severity: " + hediff?.Part?.Label + TotalCumPercent * 0.002f);
                    hediff.Severity -= fd * 0.002f;
                    if (hediff.Severity < TotalCumPercent * 0.002f) hediff.Severity = TotalCumPercent * 0.002f; 
                }

            }
        }


        public void BeforeCumOut(out Absorber absorber)
        {
            absorber = (Absorber)parent.pawn.apparel?.WornApparel?.Find(x => x is Absorber);
            if (absorber != null)
            {
                absorber.WearEffect();
                if (absorber.dirty && absorber.EffectAfterDirty) absorber.DirtyEffect();
            }
        }

        /// <summary>
        /// For natural leaking
        /// </summary>
        public void AfterCumOut()
        {
            parent.pawn.needs.mood.thoughts.memories.TryGainMemory(VariousDefOf.LeakingFluids);
        }

        /// <summary>
        /// For all type of leaking
        /// </summary>
        /// <param name="fd"></param>
        public void AfterFluidOut(float fd)
        {
            //if (Configurations.LLActivated)
            //{
            //    LLCumflationOut(fd);
            //}
        }




        /// <summary>
        /// Excrete cums in womb naturally
        /// </summary>
        public void CumOut()
        {
            float leakfactor = 1.0f;
            float totalleak = 0f;
            float cumd = TotalCumPercent;
            List<string> filthlabels = new List<string>();
            BeforeCumOut(out Absorber absorber);
            if (cums.NullOrEmpty()) return;
            else if (absorber != null && absorber.dirty && !absorber.LeakAfterDirty) leakfactor = 0f;
            List<Cum> removecums = new List<Cum>();
            foreach(Cum cum in cums)
            {
                float vd = cum.volume;
                cum.volume *= Math.Max(0, (1 - (Configurations.CumDecayRatio * (1 - cum.decayresist)) * leakfactor));
                cum.fertvolume *= Math.Max(0, (1 - (Configurations.CumDecayRatio * (1 - cum.decayresist)) * leakfactor) * (1 - (Configurations.CumFertilityDecayRatio * (1 - cum.decayresist))));
                vd -= cum.volume;
                totalleak += AbsorbCum(cum, vd, absorber);
                string tmp = "FilthLabelWithSource".Translate(cum.FilthDef.label, cum.pawn?.LabelShort ?? "Unknown", 1.ToString());
                filthlabels.Add(tmp.Replace(" x1",""));
                if (cum.fertvolume < 0.01f) cum.fertvolume = 0;
                if (cum.volume < 0.01f) removecums.Add(cum);
            }
            if (cums.Count > 1) MakeCumFilthMixture(totalleak, filthlabels);
            else if (cums.Count == 1) MakeCumFilth(cums.First(), totalleak);
            foreach (Cum cum in removecums)
            {
                cums.Remove(cum);
            }
            removecums.Clear();
            cumd = TotalCumPercent - cumd;
            if (totalleak >= 1.0f) AfterCumOut();
            AfterFluidOut(cumd);

        }

        /// <summary>
        /// Force excrete cums in womb and get excreted amount of specific cum.
        /// </summary>
        /// <param name="targetcum"></param>
        /// <param name="portion"></param>
        /// <returns></returns>
        public float CumOut(Cum targetcum, float portion = 0.1f)
        {
            float leakfactor = 1.0f;
            float totalleak = 0;
            List<string> filthlabels = new List<string>();
            if (cums.NullOrEmpty()) return 0;
            float outcum = 0;
            float cumd = TotalCumPercent;
            List<Cum> removecums = new List<Cum>();
            foreach (Cum cum in cums)
            {
                float vd = cum.volume;
                if (cum.Equals(targetcum)) outcum = cum.volume * (portion * (1 - cum.decayresist));
                cum.volume *= Math.Max(0, 1 - (portion * (1 - cum.decayresist)) * leakfactor);
                cum.fertvolume *= Math.Max(0, (1 - (portion * (1 - cum.decayresist)) * leakfactor) * (1 - (Configurations.CumFertilityDecayRatio * (1 - cum.decayresist))));
                //MakeCumFilth(cum, vd - cum.volume);
                string tmp = "FilthLabelWithSource".Translate(cum.FilthDef.label, cum.pawn?.LabelShort ?? "Unknown", 1.ToString());
                filthlabels.Add(tmp.Replace(" x1", ""));
                totalleak += vd - cum.volume;
                if (cum.fertvolume < 0.01f) cum.fertvolume = 0;
                if (cum.volume < 0.01f) removecums.Add(cum);
            }
            if (cums.Count > 1) MakeCumFilthMixture(totalleak, filthlabels);
            else if (cums.Count == 1) MakeCumFilth(cums.First(), totalleak);
            foreach (Cum cum in removecums)
            {
                cums.Remove(cum);
            }
            removecums.Clear();
            cumd = TotalCumPercent - cumd;
            AfterFluidOut(cumd);
            return outcum;
        }

        /// <summary>
        /// Ignores cum's decayratio and absorber and get excreted amount of specific cum
        /// </summary>
        /// <param name="targetcum"></param>
        /// <param name="portion"></param>
        /// <returns></returns>
        public float CumOutForce(Cum targetcum = null, float portion = 0.1f)
        {
            if (cums.NullOrEmpty()) return 0;
            float outcum = 0;
            float totalleak = 0;
            List<string> filthlabels = new List<string>();
            float cumd = TotalCumPercent;
            List<Cum> removecums = new List<Cum>();
            foreach (Cum cum in cums)
            {
                float vd = cum.volume;
                if (cum.Equals(targetcum)) outcum = cum.volume * (portion);
                cum.volume *= 1 - (portion);
                cum.fertvolume *= (1 - (portion)) * (1 - (Configurations.CumFertilityDecayRatio));
                //MakeCumFilth(cum, vd - cum.volume);
                string tmp = "FilthLabelWithSource".Translate(cum.FilthDef.label, cum.pawn?.LabelShort ?? "Unknown", 1.ToString());
                filthlabels.Add(tmp.Replace(" x1", ""));
                totalleak += vd - cum.volume;
                if (cum.fertvolume < 0.01f) cum.fertvolume = 0;
                if (cum.volume < 0.1f) removecums.Add(cum);
            }
            if (cums.Count > 1) MakeCumFilthMixture(totalleak, filthlabels);
            else if (cums.Count == 1) MakeCumFilth(cums.First(), totalleak);
            foreach (Cum cum in removecums)
            {
                cums.Remove(cum);
            }
            removecums.Clear();
            cumd = TotalCumPercent - cumd;
            AfterFluidOut(cumd);
            return outcum;
        }

        



        /// <summary>
        /// Fertilize eggs and return the result
        /// </summary>
        /// <returns></returns>
        public bool FertilizationCheck()
        {
            if (!eggs.NullOrEmpty())
            {
                bool onefertilized = false;
                foreach (Egg egg in eggs)
                {
                    if (!egg.fertilized) egg.fertilizer = Fertilize();
                    if (egg.fertilizer != null) {
                        egg.fertilized = true;
                        egg.lifespanhrs += 240;
                        onefertilized = true;
                    }
                }
                return onefertilized;
            }
            else return false;
        }

        public void Initialize()
        {
            Props = (CompProperties_Menstruation)props;

            if (!Props.infertile)
            {
                if (follicularIntervalhours < 0)
                {
                    follicularIntervalhours = PeriodRandomizer(Props.folicularIntervalDays * 24, Props.deviationFactor);
                    curStage = RandomStage();
                }

                if (lutealIntervalhours < 0) lutealIntervalhours = PeriodRandomizer(Props.lutealIntervalDays * 24, Props.deviationFactor);
                if (bleedingIntervalhours < 0) bleedingIntervalhours = PeriodRandomizer(Props.bleedingIntervalDays * 24, Props.deviationFactor);
                if (recoveryIntervalhours < 0) recoveryIntervalhours = PeriodRandomizer(Props.recoveryIntervalDays * 24, Props.deviationFactor);
                if (crampPain < 0) crampPain = PainRandomizer();
                if (cums == null) cums = new List<Cum>();
                if (eggs == null) eggs = new List<Egg>();
                
                
                if (!Configurations.EnableMenopause)
                {
                    Hediff hediff = parent.pawn.health.hediffSet.GetFirstHediffOfDef(VariousDefOf.Hediff_Climacteric);
                    if (hediff != null) parent.pawn.health.RemoveHediff(hediff);
                    hediff = parent.pawn.health.hediffSet.GetFirstHediffOfDef(VariousDefOf.Hediff_Menopause);
                    if (hediff != null) parent.pawn.health.RemoveHediff(hediff);
                }
                else if (ovarypower < -50000)
                {
                    if (Props.ovaryPower > 10000000) ovarypower = Props.ovaryPower;
                    else
                    {
                        float avglittersize;
                        try
                        {
                            avglittersize = Rand.ByCurveAverage(parent.pawn.def.race.litterSizeCurve);
                        }
                        catch (NullReferenceException)
                        {
                            avglittersize = 1;
                        }
                        ovarypower = (int)(((Props.ovaryPower * Rand.Range(0.7f, 1.3f) * parent.pawn.def.race.lifeExpectancy / ThingDefOf.Human.race.lifeExpectancy)
                            - (Math.Max(0, parent.pawn.ageTracker.AgeBiologicalYears - 15)) * (60 / (Props.folicularIntervalDays + Props.lutealIntervalDays) * Configurations.CycleAcceleration)) * avglittersize);
                        if (ovarypower < 1)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(VariousDefOf.Hediff_Menopause, parent.pawn);
                            hediff.Severity = 0.2f;
                            parent.pawn.health.AddHediff(hediff, Genital_Helper.get_genitalsBPR(parent.pawn));
                            curStage = Stage.Young;
                        }
                        else if (ovarypower < ovarypowerthreshold)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(VariousDefOf.Hediff_Climacteric, parent.pawn);
                            hediff.Severity = 0.008f * (ovarypowerthreshold - ovarypower);
                            parent.pawn.health.AddHediff(hediff, Genital_Helper.get_genitalsBPR(parent.pawn));
                        }
                    }
                }
                
                
                if (parent.pawn.IsPregnant()) curStage = Stage.Pregnant;
                if (parent.pawn.IsAnimal())
                {
                    if (Configurations.EnableAnimalCycle)
                    {
                        HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(PeriodSimulator(curStage), tickInterval, parent.pawn, false);
                    }
                }
                else
                {
                    if (!parent.pawn.IsPregnant() && parent.pawn.health.capacities.GetLevel(xxx.reproduction) <= 0) HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(PeriodSimulator(Stage.Young), tickInterval, parent.pawn, false);
                    else HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(PeriodSimulator(curStage), tickInterval, parent.pawn, false);
                }
            }
            else
            {
                if (cums == null) cums = new List<Cum>();
                curStage = Stage.None;
                HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(PeriodSimulator(curStage), tickInterval, parent.pawn, false);
            }
            //Log.Message(parent.pawn.Label + " - Initialized menstruation comp");
            loaded = true;
        }

        public void AfterSimulator()
        {
            if (ovarypower < ovarypowerthreshold)
            {
                if (sexNeed == null) sexNeed = parent.pawn.needs.TryGetNeed(VariousDefOf.SexNeed);
                else
                {
                    if (sexNeed.CurLevel < 0.5) sexNeed.CurLevel += 0.01f;
                }
            }
        }

        public void SetEstrus(int days)
        {
            HediffDef estrusdef;
            if (Props.consealedEstrus) estrusdef = VariousDefOf.Hediff_Estrus_Consealed;
            else estrusdef = VariousDefOf.Hediff_Estrus;
            
            HediffWithComps hediff = (HediffWithComps)parent.pawn.health.hediffSet.GetFirstHediffOfDef(estrusdef);
            if (hediff != null)
            {
                hediff.Severity = (float)days / Configurations.CycleAcceleration + 0.2f;
            }
            else
            {
                hediff = (HediffWithComps)HediffMaker.MakeHediff(estrusdef, parent.pawn);
                hediff.Severity = (float)days / Configurations.CycleAcceleration + 0.2f;
                parent.pawn.health.AddHediff(hediff);
            }
        }


        private Pawn Fertilize()
        {
            if (cums.NullOrEmpty()) return null;
            foreach (Cum cum in cums)
            {
                float rand = Rand.Range(0.0f, 1.0f);
                if (cum.pawn != null && !cum.notcum && rand < cum.fertvolume * cum.fertFactor * Configurations.FertilizeChance * Props.basefertilizationChanceFactor)
                {
                    if (!RJWPregnancySettings.bestial_pregnancy_enabled && (xxx.is_animal(parent.pawn) ^ xxx.is_animal(cum.pawn))) continue;
                    return cum.pawn;
                }
            }
            return null;
        }

        //for now, only one egg can be implanted
        private bool Implant()
        {
            if (!eggs.NullOrEmpty())
            {
                List<Egg> deadeggs = new List<Egg>();
                bool pregnant = false;
                foreach(Egg egg in eggs)
                {
                    if (!egg.fertilized || egg.fertstage < 168) continue;
                    else if (Rand.Range(0.0f, 1.0f) <= Configurations.ImplantationChance * Props.baseImplantationChanceFactor * ImplantFactor * InterspeciesImplantFactor(egg.fertilizer))
                    {
                        if (!parent.pawn.IsPregnant())
                        {
                            if (!Configurations.UseMultiplePregnancy)
                            {
                                PregnancyHelper.PregnancyDecider(parent.pawn, egg.fertilizer);
                                pregnant = true;
                                break;
                            }
                            else
                            {
                                Hediff_BasePregnancy.Create<Hediff_MultiplePregnancy>(parent.pawn, egg.fertilizer);
                                Hediff hediff = PregnancyHelper.GetPregnancy(parent.pawn);
                                //if (hediff is Hediff_BasePregnancy)
                                //{
                                //    Hediff_BasePregnancy h = (Hediff_BasePregnancy)hediff;
                                //    if (h.babies.Count > 1) h.babies.RemoveRange(1, h.babies.Count - 1);
                                //}
                                pregnant = true;
                                deadeggs.Add(egg);
                            }
                        }
                        else if (Configurations.UseMultiplePregnancy && Configurations.EnableHeteroOvularTwins)
                        {
                            Hediff hediff = PregnancyHelper.GetPregnancy(parent.pawn);
                            if (hediff is Hediff_MultiplePregnancy)
                            {
                                Hediff_MultiplePregnancy h = (Hediff_MultiplePregnancy)hediff;
                                h.AddNewBaby(parent.pawn, egg.fertilizer);
                            }
                            pregnant = true;
                            deadeggs.Add(egg);
                        }
                        else
                        {
                            pregnant = true;
                            break;
                        }
                    }
                    else deadeggs.Add(egg);
                }

                if (pregnant && (!Configurations.UseMultiplePregnancy || !Configurations.EnableHeteroOvularTwins))
                {
                    eggs.Clear();
                    deadeggs.Clear();
                    return true;
                }
                else if (!deadeggs.NullOrEmpty())
                {
                    foreach (Egg egg in deadeggs)
                    {
                        eggs.Remove(egg);
                    }
                    deadeggs.Clear();
                }
                if (pregnant) return true;
            }
            return false;
        }

        private void BleedOut()
        {
            //FilthMaker.TryMakeFilth(parent.pawn.Position, parent.pawn.Map, ThingDefOf.Filth_Blood,parent.pawn.Label);
            CumIn(parent.pawn, Rand.Range(1f, 2f), Translations.Menstrual_Blood,-5.0f,ThingDefOf.Filth_Blood);
            GetNotCum(Translations.Menstrual_Blood).color = BloodColor;
        }

        /// <summary>
        /// Make filth ignoring absorber
        /// </summary>
        /// <param name="cum"></param>
        /// <param name="amount"></param>
        private void MakeCumFilth(Cum cum, float amount) 
        {
            if (amount >= minmakefilthvalue) FilthMaker.TryMakeFilth(parent.pawn.Position, parent.pawn.Map, cum.FilthDef, cum.pawn?.LabelShort ?? "Unknown");
        }

        /// <summary>
        /// Absorb cum and return leaked amount
        /// </summary>
        /// <param name="cum"></param>
        /// <param name="amount"></param>
        /// <param name="absorber"></param>
        /// <returns></returns>
        private float AbsorbCum(Cum cum, float amount, Absorber absorber)
        {
            
            if (absorber != null)
            {
                float absorbable = absorber.GetStatValue(VariousDefOf.MaxAbsorbable);
                absorber.SetColor(Colors.CMYKLerp(GetCumMixtureColor, absorber.DrawColor, 1f - amount/absorbable));
                if (!absorber.dirty)
                {
                    absorber.absorbedfluids += amount;
                    if (absorber.absorbedfluids > absorbable)
                    {
                        absorber.def = absorber.DirtyDef;
                        //absorber.fluidColor = GetCumMixtureColor;
                        absorber.dirty = true;
                    }
                }
                else
                {
                    
                    //if (absorber.LeakAfterDirty) FilthMaker.TryMakeFilth(parent.pawn.Position, parent.pawn.Map, cum.FilthDef, cum.pawn.LabelShort);
                    return amount;
                }
            }
            else
            {
                //if (amount >= minmakefilthvalue) FilthMaker.TryMakeFilth(parent.pawn.Position, parent.pawn.Map, cum.FilthDef, cum.pawn.LabelShort);
                return amount;
            }
            return 0;
        }

        private float MakeCumFilthMixture(float amount, List<string> cumlabels)
        {
            
            if (amount >= minmakefilthvalue)
            {
                FilthMaker_Colored.TryMakeFilth(parent.pawn.Position, parent.pawn.Map, VariousDefOf.FilthMixture, cumlabels,GetCumMixtureColor,false);
            }
            return amount;
        }




        private void EggDecay()
        {
            List<Egg> deadeggs = new List<Egg>();
            foreach (Egg egg in eggs)
            {
                egg.lifespanhrs -= Configurations.CycleAcceleration;
                egg.position += Configurations.CycleAcceleration;
                if (egg.lifespanhrs < 0) deadeggs.Add(egg);
                if (egg.fertilized) egg.fertstage += Configurations.CycleAcceleration;
            }

            if (!deadeggs.NullOrEmpty())
            {
                foreach (Egg egg in deadeggs)
                {
                    eggs.Remove(egg);
                }
                deadeggs.Clear();
            }
        }


        private Action PeriodSimulator(Stage targetstage)
        {
            Action action = null;
            switch (targetstage)
            {
                case Stage.Follicular:
                    action = delegate
                    {
                        if (curStageHrs >= FollicularIntervalHours)
                        {
                            GoNextStage(Stage.Ovulatory);
                        }
                        else
                        {
                            curStageHrs+=Configurations.CycleAcceleration;
                            if (!estrusflag && curStageHrs > FollicularIntervalHours - 72)
                            {
                                estrusflag = true;
                                SetEstrus(Props.eggLifespanDays + 3);
                            }
                            StayCurrentStage();
                        }
                    };
                    break;
                case Stage.Ovulatory:
                    action = delegate
                    {
                        estrusflag = false;
                        int i = 0;
                        do
                        {
                            ovarypower--;
                            eggs.Add(new Egg((int)(Props.eggLifespanDays * 24 / CycleFactor)));
                            i++;
                        } while (i < Rand.ByCurve(parent.pawn.RaceProps.litterSizeCurve) + eggstack);
                        eggstack = 0;
                        if (Configurations.EnableMenopause && ovarypower < 1)
                        {
                            eggs.Clear();
                            Hediff hediff = parent.pawn.health.hediffSet.GetFirstHediffOfDef(VariousDefOf.Hediff_Climacteric);
                            if (hediff != null) parent.pawn.health.RemoveHediff(hediff);
                            hediff = HediffMaker.MakeHediff(VariousDefOf.Hediff_Menopause, parent.pawn);
                            hediff.Severity = 0.2f;
                            parent.pawn.health.AddHediff(hediff, Genital_Helper.get_genitalsBPR(parent.pawn));
                            GoNextStage(Stage.Young);
                        }
                        else if (Configurations.EnableMenopause && ovarypower < ovarypowerthreshold)
                        {
                            Hediff hediff = HediffMaker.MakeHediff(VariousDefOf.Hediff_Climacteric, parent.pawn);
                            hediff.Severity = 0.008f * i;
                            parent.pawn.health.AddHediff(hediff, Genital_Helper.get_genitalsBPR(parent.pawn));
                            lutealIntervalhours = PeriodRandomizer(lutealIntervalhours, Props.deviationFactor * 6);
                            GoNextStage(Stage.ClimactericLuteal);
                        }
                        else
                        {
                            lutealIntervalhours = PeriodRandomizer(lutealIntervalhours, Props.deviationFactor);
                            GoNextStage(Stage.Luteal);
                        }
                    };
                    break;
                case Stage.Luteal:
                    action = delegate
                    {
                        if (!eggs.NullOrEmpty())
                        {
                            EggDecay();
                            FertilizationCheck();
                            if (Implant()) GoNextStage(Stage.Pregnant);
                            else
                            {
                                curStageHrs += Configurations.CycleAcceleration;
                                StayCurrentStage();
                            }
                        }
                        else if (curStageHrs <= lutealIntervalhours)
                        {
                            curStageHrs+=Configurations.CycleAcceleration;
                            StayCurrentStage(); 
                        }
                        else
                        {
                            if (Props.bleedingIntervalDays == 0)
                            {
                                follicularIntervalhours = PeriodRandomizer(follicularIntervalhours, Props.deviationFactor);
                                GoNextStage(Stage.Follicular);
                            }
                            else
                            {
                                bleedingIntervalhours = PeriodRandomizer(bleedingIntervalhours, Props.deviationFactor);
                                if (crampPain >= 0.05f)
                                {
                                    Hediff hediff = HediffMaker.MakeHediff(VariousDefOf.Hediff_MenstrualCramp, parent.pawn);
                                    hediff.Severity = crampPain * Rand.Range(0.9f, 1.1f);
                                    HediffCompProperties_SeverityPerDay Prop = (HediffCompProperties_SeverityPerDay)hediff.TryGetComp<HediffComp_SeverityPerDay>().props;
                                    Prop.severityPerDay = - hediff.Severity / (bleedingIntervalhours/24) * Configurations.CycleAcceleration;
                                    parent.pawn.health.AddHediff(hediff, Genital_Helper.get_genitalsBPR(parent.pawn));
                                }
                                GoNextStage(Stage.Bleeding);
                            }
                        }
                    };
                    break;
                case Stage.Bleeding:
                    action = delegate
                    {
                        if (curStageHrs >= bleedingIntervalhours)
                        {
                            follicularIntervalhours = PeriodRandomizer(follicularIntervalhours, Props.deviationFactor);
                            Hediff hediff = parent.pawn.health.hediffSet.GetFirstHediffOfDef(VariousDefOf.Hediff_MenstrualCramp);
                            if (hediff != null) parent.pawn.health.RemoveHediff(hediff);
                            GoNextStage(Stage.Follicular);
                        }
                        else
                        {
                            if (curStageHrs < bleedingIntervalhours / 4) for (int i = 0; i < Configurations.CycleAcceleration; i++) BleedOut();
                            curStageHrs+=Configurations.CycleAcceleration;
                            StayCurrentStage();
                        }
                    };
                    break;
                case Stage.Fertilized:  //Obsoleted stage. merged in luteal stage
                    action = delegate
                    {
                        ModLog.Message("Obsoleted stage. skipping...");
                        GoNextStage(Stage.Luteal);
                        //if (curStageHrs >= 24)
                        //{
                        //    if (Implant())
                        //    {
                        //        GoNextStage(Stage.Pregnant);
                        //    }
                        //    else
                        //    {
                        //        GoNextStageSetHour(Stage.Luteal, 96);
                        //    }
                        //}
                        //else
                        //{
                        //    curStageHrs+=Configurations.CycleAcceleration;
                        //    StayCurrentStage();
                        //}
                    };
                    break;
                case Stage.Pregnant:
                    action = delegate
                    {
                        if (!eggs.NullOrEmpty())
                        {
                            EggDecay();
                            FertilizationCheck();
                            Implant();
                        }
                        if (parent.pawn.IsPregnant()) StayCurrentStageConst(Stage.Pregnant);
                        else GoNextStage(Stage.Recover);
                    };
                    break;
                case Stage.Recover:
                    action = delegate
                    {
                        if (curStageHrs >= recoveryIntervalhours)
                        {
                            if (ovarypower < ovarypowerthreshold)
                            {
                                GoNextStage(Stage.ClimactericFollicular);
                            }
                            else if (parent.pawn.health.capacities.GetLevel(xxx.reproduction) == 0)
                            {
                                GoNextStage(Stage.Young);
                            }
                            else
                            {
                                follicularIntervalhours = PeriodRandomizer(follicularIntervalhours, Props.deviationFactor);
                                GoNextStage(Stage.Follicular);
                            }
                        }
                        else
                        {
                            curStageHrs+=Configurations.CycleAcceleration;
                            StayCurrentStage();
                        }
                    };
                    break;
                case Stage.None:
                    action = delegate
                    {
                        StayCurrentStageConst(Stage.None);
                    };
                    break;
                case Stage.Young:
                    action = delegate
                    {
                        if (parent.pawn.health.capacities.GetLevel(xxx.reproduction) <= 0) StayCurrentStageConst(Stage.Young);
                        else GoNextStage(Stage.Follicular);
                    };
                    break;
                case Stage.ClimactericFollicular:
                    action = delegate
                    {
                        if (curStageHrs >= (follicularIntervalhours - bleedingIntervalhours) * CycleFactor)
                        {
                            GoNextStage(Stage.Ovulatory);
                        }
                        else if (ovarypower < ovarypowerthreshold / 3 && Rand.Range(0.0f, 1.0f) < 0.2f) //skips ovulatory
                        {
                            follicularIntervalhours = PeriodRandomizer(follicularIntervalhours, Props.deviationFactor * 6);
                            GoNextStage(Stage.ClimactericFollicular);
                        }
                        else
                        {
                            curStageHrs += Configurations.CycleAcceleration;
                            StayCurrentStage();
                        }
                    };
                    break;
                case Stage.ClimactericLuteal:
                    action = delegate
                    {
                        if (!eggs.NullOrEmpty())
                        {
                            EggDecay();
                            FertilizationCheck();
                            if (Implant()) GoNextStage(Stage.Pregnant);
                            else
                            {
                                curStageHrs += Configurations.CycleAcceleration;
                                StayCurrentStage();
                            }
                        }
                        else if (curStageHrs <= lutealIntervalhours)
                        {
                            curStageHrs += Configurations.CycleAcceleration;
                            StayCurrentStage();
                        }
                        else
                        {
                            if (Props.bleedingIntervalDays == 0)
                            {
                                follicularIntervalhours = PeriodRandomizer(follicularIntervalhours, Props.deviationFactor * 6);
                                GoNextStage(Stage.ClimactericFollicular);
                            }
                            else if (ovarypower < ovarypowerthreshold / 4 || (ovarypower < ovarypowerthreshold / 3 && Rand.Range(0.0f,1.0f) < 0.3f)) //skips bleeding
                            {
                                follicularIntervalhours = PeriodRandomizer(follicularIntervalhours, Props.deviationFactor * 6);
                                GoNextStage(Stage.ClimactericFollicular);
                            }
                            else
                            {
                                bleedingIntervalhours = PeriodRandomizer(bleedingIntervalhours, Props.deviationFactor);
                                if (crampPain >= 0.05f)
                                {
                                    Hediff hediff = HediffMaker.MakeHediff(VariousDefOf.Hediff_MenstrualCramp, parent.pawn);
                                    hediff.Severity = crampPain * Rand.Range(0.9f, 1.1f);
                                    HediffCompProperties_SeverityPerDay Prop = (HediffCompProperties_SeverityPerDay)hediff.TryGetComp<HediffComp_SeverityPerDay>().props;
                                    Prop.severityPerDay = -hediff.Severity / (bleedingIntervalhours / 24) * Configurations.CycleAcceleration;
                                    parent.pawn.health.AddHediff(hediff, Genital_Helper.get_genitalsBPR(parent.pawn));
                                }
                                GoNextStage(Stage.ClimactericBleeding);
                            }
                        }

                    };
                    break;
                case Stage.ClimactericBleeding:
                    action = delegate
                    {
                        if (curStageHrs >= bleedingIntervalhours)
                        {
                            follicularIntervalhours = PeriodRandomizer(follicularIntervalhours, Props.deviationFactor * 6);
                            GoNextStage(Stage.ClimactericFollicular);
                        }
                        else
                        {
                            if (curStageHrs < bleedingIntervalhours / 6) for (int i = 0; i < Configurations.CycleAcceleration; i++) BleedOut();
                            curStageHrs += Configurations.CycleAcceleration;
                            StayCurrentStage();
                        }
                    };
                    break;

                default:
                    curStage = Stage.Follicular;
                    curStageHrs = 0;
                    if (follicularIntervalhours < 0) follicularIntervalhours = PeriodRandomizer(Props.folicularIntervalDays*24, Props.deviationFactor);
                    HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(PeriodSimulator(Stage.Follicular), tickInterval, parent.pawn, false);
                    break;
            }
            action += () =>
            {
                if (parent.pawn.health.capacities.GetLevel(xxx.reproduction) <= 0) curStage = Stage.Young;
                CumOut();
                AfterSimulator();
            };

            actionref = action;
            return actionref;

            void GoNextStage(Stage nextstage, float factor = 1.0f)
            {
                curStageHrs = 0;
                curStage = nextstage;
                HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(PeriodSimulator(nextstage), (int)(tickInterval * factor), parent.pawn, false);
            }

            
            void GoNextStageSetHour(Stage nextstage, int hour, float factor = 1.0f)
            {
                curStageHrs = hour;
                curStage = nextstage;
                HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(PeriodSimulator(nextstage), (int)(tickInterval * factor), parent.pawn, false);
            }

            //stage can be interrupted in other reasons
            void StayCurrentStage(float factor = 1.0f)
            {
                HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(PeriodSimulator(curStage), (int)(tickInterval * factor), parent.pawn, false);
            }

            //stage never changes
            void StayCurrentStageConst(Stage curstage, float factor = 1.0f)
            {
                HugsLibController.Instance.TickDelayScheduler.ScheduleCallback(PeriodSimulator(curstage), (int)(tickInterval * factor), parent.pawn, false);
            }
            

        }


        private int PeriodRandomizer(int intervalhours, float deviation)
        {
            return intervalhours + (int)(intervalhours*Rand.Range(-deviation,deviation));
        }

        private float InterspeciesImplantFactor(Pawn fertilizer)
        {
            if (RJWPregnancySettings.complex_interspecies) return SexUtility.BodySimilarity(parent.pawn, fertilizer);
            else return RJWPregnancySettings.interspecies_impregnation_modifier;
        }

        private float PainRandomizer()
        {
            float rand = Rand.Range(0.0f, 1.0f);
            if (rand < 0.01f) return Rand.Range(0.0f,0.2f);
            else if (rand < 0.2f) return Rand.Range(0.1f, 0.2f);
            else if (rand < 0.8f) return Rand.Range(0.2f, 0.4f);
            else if (rand < 0.95f) return Rand.Range(0.4f, 0.6f);
            else return Rand.Range(0.6f, 1.0f);
        }

        private Stage RandomStage()
        {
            int rand = Rand.Range(0,2);

            switch (rand)
            {
                case 0:
                    curStageHrs = Rand.Range(0, (Props.folicularIntervalDays - Props.bleedingIntervalDays) * 24);
                    return Stage.Follicular;
                case 1:
                    curStageHrs = Rand.Range(0, Props.eggLifespanDays * 24);
                    return Stage.Luteal;
                case 2:
                    curStageHrs = Rand.Range(0, Props.bleedingIntervalDays * 24);
                    return Stage.Bleeding;
                default: return Stage.Follicular;
            }


        }




        
        public class Egg : IExposable
        {
            public bool fertilized;
            public int lifespanhrs;
            public Pawn fertilizer;
            public int position;
            public int fertstage = 0;

            public Egg()
            {
                fertilized = false;
                lifespanhrs = 96;
                fertilizer = null;
                position = 0;
            }

            public Egg(int lifespanhrs)
            {
                fertilized = false;
                this.lifespanhrs = lifespanhrs;
                fertilizer = null;
                position = 0;
            }

            public void ExposeData()
            {
                Scribe_References.Look(ref fertilizer, "fertilizer", true);
                Scribe_Values.Look(ref fertilized, "fertilized", fertilized, true);
                Scribe_Values.Look(ref lifespanhrs, "lifespanhrs", lifespanhrs, true);
                Scribe_Values.Look(ref position, "position", position, true);
                Scribe_Values.Look(ref fertstage, "fertstage", fertstage, true);
            }
        }


    }

    public class HediffComp_Anus : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
        }
    }








}
