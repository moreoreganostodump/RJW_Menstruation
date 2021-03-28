using RimWorld;
using rjw;
using System;
using System.Linq;
using UnityEngine;
using Verse;

namespace RJW_Menstruation
{
    public static class Colors
    {
        public static Color blood = new Color(0.78f, 0, 0);


        public static Color CMYKLerp(Color a, Color b, float t)
        {
            RGBtoCMYK(a, out float ac, out float am, out float ay, out float ak);
            RGBtoCMYK(b, out float bc, out float bm, out float by, out float bk);

            return CMYKtoRGB(Mathf.Lerp(ac, bc, t), Mathf.Lerp(am, bm, t), Mathf.Lerp(ay, by, t), Mathf.Lerp(ak, bk, t));
        }

        public static void RGBtoCMYK(Color rgb, out float c, out float m, out float y, out float k)
        {
            k = 1 - Math.Max(rgb.r, Math.Max(rgb.g, rgb.b));
            c = (1 - rgb.r - k) / (1 - k);
            m = (1 - rgb.g - k) / (1 - k);
            y = (1 - rgb.b - k) / (1 - k);

        }

        public static Color CMYKtoRGB(float c, float m, float y, float k)
        {
            return new Color((1 - c) * (1 - k), (1 - m) * (1 - k), (1 - y) * (1 - k));
        }



    }


    public static class Utility
    {



        public static float GetCumVolume(this Pawn pawn)
        {
            CompHediffBodyPart part = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_genitalsBPR(pawn))?.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("penis")).InRandomOrder().FirstOrDefault()?.TryGetComp<CompHediffBodyPart>();
            if (part == null) part = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_genitalsBPR(pawn))?.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("ovipositorf")).InRandomOrder().FirstOrDefault()?.TryGetComp<CompHediffBodyPart>();
            if (part == null) part = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_genitalsBPR(pawn))?.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("ovipositorm")).InRandomOrder().FirstOrDefault()?.TryGetComp<CompHediffBodyPart>();
            if (part == null) part = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_genitalsBPR(pawn))?.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("tentacle")).InRandomOrder().FirstOrDefault()?.TryGetComp<CompHediffBodyPart>();

            float res = 0;
            try
            {
                res = part.FluidAmmount * part.FluidModifier * pawn.BodySize * Rand.Range(0.8f, 1.2f) * RJWSettings.cum_on_body_amount_adjust * 0.3f;
            }
            catch (NullReferenceException)
            {
                res = 0.0f;
            }
            if (pawn.Has(Quirk.Messy)) res *= Rand.Range(4.0f, 8.0f);

            return res;
        }

        public static HediffComp_Menstruation GetMenstruationComp(this Pawn pawn)
        {
            var hedifflist = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_genitalsBPR(pawn))?.FindAll((Hediff h) => h.def.defName.ToLower().Contains("vagina"));
            HediffComp_Menstruation result;
            if (hedifflist.NullOrEmpty()) return null;
            else
            {
                foreach (Hediff h in hedifflist)
                {
                    result = h.TryGetComp<HediffComp_Menstruation>();
                    if (result != null) return result;
                }
            }
            return null;
        }

        public static HediffComp_Menstruation GetMenstruationComp(this Hediff hediff)
        {
            if (hediff is Hediff_PartBaseNatural || hediff is Hediff_PartBaseArtifical)
            {
                return hediff.TryGetComp<HediffComp_Menstruation>();
            }
            return null;
        }

        public static bool HasMenstruationComp(this Pawn pawn)
        {
            var hedifflist = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_genitalsBPR(pawn))?.FindAll((Hediff h) => h.def.defName.ToLower().Contains("vagina"));
            HediffComp_Menstruation result;
            if (hedifflist.NullOrEmpty()) return false;
            else
            {
                foreach (Hediff h in hedifflist)
                {
                    result = h.TryGetComp<HediffComp_Menstruation>();
                    if (result != null) return true;
                }
            }
            return false;
        }

        public static bool HasMenstruationComp(this Hediff hediff)
        {
            if (hediff is Hediff_PartBaseNatural || hediff is Hediff_PartBaseArtifical)
            {
                if (hediff.TryGetComp<HediffComp_Menstruation>() != null) return true;
            }
            return false;
        }


        public static HediffComp_Menstruation.Stage GetCurStage(this Pawn pawn)
        {
            return GetMenstruationComp(pawn)?.curStage ?? HediffComp_Menstruation.Stage.Bleeding;
        }


        public static float GetPregnancyProgress(this Pawn pawn)
        {
            Hediff hediff = PregnancyHelper.GetPregnancy(pawn);
            if (hediff is Hediff_BasePregnancy)
            {
                Hediff_BasePregnancy h = (Hediff_BasePregnancy)hediff;
                return h.GestationProgress;
            }
            return -1;
        }

        public static Pawn GetFetus(this Pawn pawn)
        {
            Hediff hediff = PregnancyHelper.GetPregnancy(pawn);
            if (hediff is Hediff_BasePregnancy)
            {
                Hediff_BasePregnancy h = (Hediff_BasePregnancy)hediff;
                if (!h.babies.NullOrEmpty()) return h.babies.First();
                else
                {
                    Log.Error("Baby not exist: baby was not created or removed");
                    return null;
                }
            }


            return null;
        }



        public static Texture2D GetPregnancyIcon(HediffComp_Menstruation comp, Hediff hediff)
        {
            string icon = "";
            if (hediff is Hediff_MechanoidPregnancy)
            {
                return ContentFinder<Texture2D>.Get(("Womb/Mechanoid_Fluid"), true);
            }
            else if (hediff is Hediff_BasePregnancy)
            {
                Hediff_BasePregnancy h = (Hediff_BasePregnancy)hediff;
                string fetustex = h.babies?.FirstOrDefault()?.def.GetModExtension<PawnDNAModExtension>()?.fetusTexPath ?? "Fetus/Fetus_Default";
                if (h.GestationProgress < 0.2f) icon = comp.wombTex + "_Implanted";
                else if (h.GestationProgress < 0.3f) icon += "Fetus/Fetus_Early00";
                else if (h.GestationProgress < 0.4f) icon += fetustex + "00";
                else if (h.GestationProgress < 0.5f) icon += fetustex + "01";
                else if (h.GestationProgress < 0.6f) icon += fetustex + "02";
                else if (h.GestationProgress < 0.7f) icon += fetustex + "03";
                else if (h.GestationProgress < 0.8f) icon += fetustex + "04";
                else icon += fetustex + "05";
            }
            else icon = "Fetus/Slime_Abomi02";
            return ContentFinder<Texture2D>.Get((icon), true);
        }

        public static Texture2D GetCumIcon(this HediffComp_Menstruation comp)
        {
            string icon = comp.wombTex;
            float cumpercent = comp.TotalCumPercent;
            if (cumpercent < 0.001f) return ContentFinder<Texture2D>.Get("Womb/Empty", true);
            else if (cumpercent < 0.01f) icon += "_Cum_00";
            else if (cumpercent < 0.05f) icon += "_Cum_01";
            else if (cumpercent < 0.11f) icon += "_Cum_02";
            else if (cumpercent < 0.17f) icon += "_Cum_03";
            else if (cumpercent < 0.23f) icon += "_Cum_04";
            else if (cumpercent < 0.29f) icon += "_Cum_05";
            else if (cumpercent < 0.35f) icon += "_Cum_06";
            else if (cumpercent < 0.41f) icon += "_Cum_07";
            else if (cumpercent < 0.47f) icon += "_Cum_08";
            else if (cumpercent < 0.53f) icon += "_Cum_09";
            else if (cumpercent < 0.59f) icon += "_Cum_10";
            else if (cumpercent < 0.65f) icon += "_Cum_11";
            else if (cumpercent < 0.71f) icon += "_Cum_12";
            else if (cumpercent < 0.77f) icon += "_Cum_13";
            else if (cumpercent < 0.83f) icon += "_Cum_14";
            else if (cumpercent < 0.89f) icon += "_Cum_15";
            else if (cumpercent < 0.95f) icon += "_Cum_16";
            else icon += "_Cum_17";
            Texture2D cumtex = ContentFinder<Texture2D>.Get((icon), true);
            return cumtex;
        }

        public static Texture2D GetWombIcon(this HediffComp_Menstruation comp)
        {
            string icon = comp.wombTex;
            HediffComp_Menstruation.Stage stage = comp.curStage;
            if (stage == HediffComp_Menstruation.Stage.Bleeding) icon += "_Bleeding";

            Texture2D wombtex = ContentFinder<Texture2D>.Get((icon), true);

            return wombtex;
        }

        public static Texture2D GetGenitalIcon(this Pawn pawn)
        {
            var hediff = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_genitalsBPR(pawn))?.Find((Hediff h) => h.def.defName.ToLower().Contains("vagina"));
            if (hediff == null) return ContentFinder<Texture2D>.Get("Genitals/Vagina00", true);
            HediffComp_Menstruation comp = hediff.GetMenstruationComp();
            string icon;
            if (comp != null) icon = comp.vagTex;
            else icon = "Genitals/Vagina";

            if (hediff.Severity < 0.20f) icon += "00";        //micro 
            else if (hediff.Severity < 0.30f) icon += "01";   //tight
            else if (hediff.Severity < 0.40f) icon += "02";   //tight
            else if (hediff.Severity < 0.47f) icon += "03";   //average
            else if (hediff.Severity < 0.53f) icon += "04";   //average
            else if (hediff.Severity < 0.60f) icon += "05";   //average
            else if (hediff.Severity < 0.70f) icon += "06";   //accomodating
            else if (hediff.Severity < 0.80f) icon += "07";   //accomodating
            else if (hediff.Severity < 0.87f) icon += "08";   //cavernous
            else if (hediff.Severity < 0.94f) icon += "09";   //cavernous
            else if (hediff.Severity < 1.01f) icon += "10";   //cavernous
            else icon += "11";                                //abyssal

            return ContentFinder<Texture2D>.Get((icon), true);
        }

        public static Texture2D GetAnalIcon(this Pawn pawn)
        {
            var hediff = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_anusBPR(pawn)).FirstOrDefault((Hediff h) => h.def.defName.ToLower().Contains("anus"));
            if (hediff != null)
            {
                CompProperties_Anus Props = (CompProperties_Anus)hediff.TryGetComp<HediffComp_Anus>()?.props;
                string icon;
                if (Props != null) icon = Props.analTex ?? "Genitals/Anal";
                else icon = "Genitals/Anal";
                if (hediff.Severity < 0.20f) icon += "00";        //micro 
                else if (hediff.Severity < 0.40f) icon += "01";   //tight
                else if (hediff.Severity < 0.60f) icon += "02";   //average
                else if (hediff.Severity < 0.80f) icon += "03";   //accomodating
                else if (hediff.Severity < 1.01f) icon += "04";   //cavernous
                else icon += "05";                                //abyssal

                return ContentFinder<Texture2D>.Get((icon), true);
            }
            else
            {
                return ContentFinder<Texture2D>.Get(("Genitals/Anal00"), true);
            }
        }

        public static string GetVaginaLabel(this Pawn pawn)
        {
            var hediff = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_genitalsBPR(pawn)).Find((Hediff h) => h.def.defName.ToLower().Contains("vagina"));
            return hediff.LabelBase.CapitalizeFirst() + "\n(" + hediff.LabelInBrackets + ")";
        }
        public static string GetAnusLabel(this Pawn pawn)
        {
            var hediff = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_anusBPR(pawn)).FirstOrDefault((Hediff h) => h.def.defName.ToLower().Contains("anus"));
            if (hediff != null) return hediff.LabelBase.CapitalizeFirst() + "\n(" + hediff.LabelInBrackets + ")";
            else return "";
        }

        public static bool ShowFetusImage(this Hediff_BasePregnancy hediff)
        {
            if (Configurations.InfoDetail == Configurations.DetailLevel.All) return true;
            else if (Configurations.InfoDetail == Configurations.DetailLevel.Hide) return false;
            else if (hediff.Visible) return true;
            else return false;
        }

        public static bool ShowFetusInfo()
        {
            if (Configurations.InfoDetail == Configurations.DetailLevel.All || Configurations.InfoDetail == Configurations.DetailLevel.OnReveal) return true;
            else return false;
        }

        public static bool ShowStatus(this Pawn pawn)
        {
            if (pawn.Faction != null && Configurations.ShowFlag != Configurations.PawnFlags.None)
            {
                if (pawn.Faction.IsPlayer)
                {
                    if ((Configurations.ShowFlag & Configurations.PawnFlags.Colonist) != 0) return true;
                }
                else if (pawn.IsPrisonerOfColony)
                {
                    if ((Configurations.ShowFlag & Configurations.PawnFlags.Prisoner) != 0) return true;
                }
                else if (pawn.Faction.PlayerRelationKind == FactionRelationKind.Ally)
                {
                    if ((Configurations.ShowFlag & Configurations.PawnFlags.Ally) != 0) return true;
                }
                else if (pawn.Faction.PlayerRelationKind == FactionRelationKind.Hostile)
                {
                    if ((Configurations.ShowFlag & Configurations.PawnFlags.Hostile) != 0) return true;
                }
                else if ((Configurations.ShowFlag & Configurations.PawnFlags.Neutral) != 0) return true;
                else return false;
            }
            else if ((Configurations.ShowFlag & Configurations.PawnFlags.Neutral) != 0) return true;

            return false;
        }

        public static Pawn GetFather(Pawn pawn, Pawn mother)
        {
            Pawn res = pawn.GetFather();
            if (res != null) return res;
            else
            {
                res = pawn.relations?.GetFirstDirectRelationPawn(VariousDefOf.Relation_birthgiver, x => !x.Equals(mother)) ?? null;
                return res;
            }
        }

        public static void DrawEggOverlay(this HediffComp_Menstruation comp, Rect wombRect)
        {
            Rect rect = new Rect(wombRect.xMax - wombRect.width / 3, wombRect.y, wombRect.width / 3, wombRect.width / 3);
            GUI.color = Color.white;
            GUI.DrawTexture(rect, comp.GetEggIcon(), ScaleMode.ScaleToFit);
        }

        public static Texture2D GetEggIcon(this HediffComp_Menstruation comp)
        {
            if (comp.parent.pawn.IsPregnant())
            {
                if (comp.parent.pawn.GetPregnancyProgress() < 0.2f) return ContentFinder<Texture2D>.Get("Eggs/Egg_Implanted00", true);
                else return ContentFinder<Texture2D>.Get("Womb/Empty", true);
            }
            else if (!comp.IsEggExist) return ContentFinder<Texture2D>.Get("Womb/Empty", true);
            else
            {
                int fertstage = comp.IsFertilized;
                if (fertstage >= 0)
                {
                    if (fertstage < 1) return ContentFinder<Texture2D>.Get("Eggs/Egg_Fertilized00", true);
                    else if (fertstage < 24) return ContentFinder<Texture2D>.Get("Eggs/Egg_Fertilized01", true);
                    else return ContentFinder<Texture2D>.Get("Eggs/Egg_Fertilized02", true);
                }
                else if (comp.IsEggFertilizing) return ContentFinder<Texture2D>.Get("Eggs/Egg_Fertilizing01", true);
                else return ContentFinder<Texture2D>.Get("Eggs/Egg", true);
            }
        }

        

    }
}
