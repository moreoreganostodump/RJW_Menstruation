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
        const int ovarypowerthreshold = 72;

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
                        mixedcolor = Color.LerpUnclamped(mixedcolor, cum.color, cum.volume/(mixedsofar + cum.volume));
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

        public string GetFertilizingInfo
        {
            get
            {
                string res = "";
                if (!eggs.NullOrEmpty())
                {
                    if (cums.NullOrEmpty()) return eggs.Count + " " + Translations.Dialog_WombInfo07;
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

        public bool GetEggFertilizing
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
        public bool GetFertilization
        {
            get
            {
                if (!eggs.NullOrEmpty()) foreach(Egg egg in eggs)
                    {
                        if (egg.fertilized) return true;
                    }
                return false;
            }
        }
        public bool GetEgg
        {
            get
            {
                return !eggs.NullOrEmpty();
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
            AfterCumIn();
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
            AfterCumIn();
        }

        public void AfterCumIn()
        {
            
        }

        public void BeforeCumOut(out Absorber absorber)
        {
            absorber = (Absorber)parent.pawn.apparel?.WornApparel?.Find(x => x.def.apparel.tags.Contains("Absorber"));
            if (absorber != null)
            {
                if (absorber.dirty && absorber.EffectAfterDirty) absorber.DirtyEffect();
            }
        }

        /// <summary>
        /// Excrete cums in womb naturally
        /// </summary>
        public void CumOut()
        {
            float leakfactor = 1.0f;
            Absorber absorber;
            BeforeCumOut(out absorber);
            if (cums.NullOrEmpty()) return;
            else if (absorber != null && absorber.dirty && !absorber.LeakAfterDirty) leakfactor = 0f;
            List<Cum> removecums = new List<Cum>();
            foreach(Cum cum in cums)
            {
                float vd = cum.volume;
                cum.volume *= Math.Max(0, (1 - (Configurations.CumDecayRatio * (1 - cum.decayresist)) * leakfactor));
                cum.fertvolume *= Math.Max(0, (1 - (Configurations.CumDecayRatio * (1 - cum.decayresist)) * leakfactor) * (1 - (Configurations.CumFertilityDecayRatio * (1 - cum.decayresist))));
                MakeCumFilth(cum, vd - cum.volume, absorber);
                if (cum.fertvolume < 0.01f) cum.fertvolume = 0;
                if (cum.volume < 0.01f) removecums.Add(cum);
            }
            foreach(Cum cum in removecums)
            {
                cums.Remove(cum);
            }
            removecums.Clear();
        }

        /// <summary>
        /// Excrete cums in womb and get excreted amount of specific cum
        /// </summary>
        /// <param name="targetcum"></param>
        /// <param name="portion"></param>
        /// <returns></returns>
        public float CumOut(Cum targetcum, float portion = 0.1f)
        {
            float leakfactor = 1.0f;
            Absorber absorber;
            BeforeCumOut(out absorber);
            if (cums.NullOrEmpty()) return 0;
            else if (absorber != null && absorber.dirty && !absorber.LeakAfterDirty) leakfactor = 0;
            float outcum = 0;
            List<Cum> removecums = new List<Cum>();
            foreach (Cum cum in cums)
            {
                float vd = cum.volume;
                if (cum.Equals(targetcum)) outcum = cum.volume * (portion * (1 - cum.decayresist));
                cum.volume *= Math.Max(0, 1 - (portion * (1 - cum.decayresist)) * leakfactor);
                cum.fertvolume *= Math.Max(0, (1 - (portion * (1 - cum.decayresist)) * leakfactor) * (1 - (Configurations.CumFertilityDecayRatio * (1 - cum.decayresist))));
                MakeCumFilth(cum, vd - cum.volume,absorber);
                if (cum.fertvolume < 0.01f) cum.fertvolume = 0;
                if (cum.volume < 0.01f) removecums.Add(cum);
            }
            foreach (Cum cum in removecums)
            {
                cums.Remove(cum);
            }
            removecums.Clear();
            return outcum;
        }

        /// <summary>
        /// Ignores cum's decayratio and absorber and get excreted amount of specific cum
        /// </summary>
        /// <param name="targetcum"></param>
        /// <param name="portion"></param>
        /// <returns></returns>
        public float CumOutForce(Cum targetcum, float portion = 0.1f)
        {
            if (cums.NullOrEmpty()) return 0;
            float outcum = 0;
            List<Cum> removecums = new List<Cum>();
            foreach (Cum cum in cums)
            {
                float vd = cum.volume;
                if (cum.Equals(targetcum)) outcum = cum.volume * (portion);
                cum.volume *= 1 - (portion);
                cum.fertvolume *= (1 - (portion)) * (1 - (Configurations.CumFertilityDecayRatio));
                MakeCumFilth(cum, vd - cum.volume);
                if (cum.fertvolume < 0.01f) cum.fertvolume = 0;
                if (cum.volume < 0.1f) removecums.Add(cum);
            }
            foreach (Cum cum in removecums)
            {
                cums.Remove(cum);
            }
            removecums.Clear();
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
                    ovarypower = (int)(Props.ovaryPower * Rand.Range(0.7f, 1.3f)) - (Math.Max(0, parent.pawn.ageTracker.AgeBiologicalYears - 15)) * (60 / (Props.folicularIntervalDays + Props.lutealIntervalDays) * Configurations.CycleAcceleration);
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


        private Pawn Fertilize()
        {
            if (cums.NullOrEmpty()) return null;
            foreach (Cum cum in cums)
            {
                float rand = Rand.Range(0.0f, 1.0f);
                if (!cum.notcum && rand < cum.fertvolume * cum.fertFactor * Configurations.FertilizeChance * Props.basefertilizationChanceFactor)
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
                    if (egg.position < 24 || !egg.fertilized) continue;
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
                                if (hediff is Hediff_BasePregnancy)
                                {
                                    Hediff_BasePregnancy h = (Hediff_BasePregnancy)hediff;
                                    if (h.babies.Count > 1) h.babies.RemoveRange(1, h.babies.Count - 1);
                                }
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
            CumIn(parent.pawn, Rand.Range(0f, 5f), Translations.Menstrual_Blood,-5.0f,ThingDefOf.Filth_Blood);
            GetNotCum(Translations.Menstrual_Blood).color = Colors.blood;
        }

        private void MakeCumFilth(Cum cum, float amount, Absorber absorber = null)
        {
            
            if (absorber != null)
            {
                if (!absorber.dirty)
                {
                    absorber.absorbedfluids += amount;
                    if (absorber.absorbedfluids < 10)
                    {
                        absorber.def = absorber.DirtyDef;
                        absorber.dirty = true;
                    }
                }
                else
                {
                    if (absorber.LeakAfterDirty) FilthMaker.TryMakeFilth(parent.pawn.Position, parent.pawn.Map, cum.FilthDef, cum.pawn.LabelShort);
                }
            }
            else if (amount >= minmakefilthvalue) FilthMaker.TryMakeFilth(parent.pawn.Position, parent.pawn.Map, cum.FilthDef, cum.pawn.LabelShort);
        }



        private void EggDecay()
        {
            List<Egg> deadeggs = new List<Egg>();
            foreach (Egg egg in eggs)
            {
                egg.lifespanhrs -= Configurations.CycleAcceleration;
                egg.position += Configurations.CycleAcceleration;
                if (egg.lifespanhrs < 0) deadeggs.Add(egg);
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


        private Action PeriodSimulator(Enum targetstage)
        {
            Action action = null;
            switch (targetstage)
            {
                case Stage.Follicular:
                    action = delegate
                    {
                        if (curStageHrs >= (follicularIntervalhours - bleedingIntervalhours) * CycleFactor)
                        {
                            GoNextStage(Stage.Ovulatory);
                        }
                        else
                        {
                            curStageHrs+=Configurations.CycleAcceleration;
                            StayCurrentStage();
                        }
                    };
                    break;
                case Stage.Ovulatory:
                    action = delegate
                    {
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
                        if (curStageHrs >= 24)
                        {
                            if (Implant())
                            {
                                GoNextStage(Stage.Pregnant);
                            }
                            else
                            {
                                GoNextStageSetHour(Stage.Luteal, 96);
                            }
                        }
                        else
                        {
                            curStageHrs+=Configurations.CycleAcceleration;
                            StayCurrentStage();
                        }
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
