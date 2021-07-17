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
    public static class MenstruationUtility
    {
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

        public static float GetFertilityChance(this HediffComp_Menstruation comp)
        {
            return comp.TotalFertCum * Configurations.FertilizeChance;
        }

        public static HediffComp_Menstruation.Stage GetCurStage(this Pawn pawn)
        {
            return GetMenstruationComp(pawn)?.curStage ?? HediffComp_Menstruation.Stage.Bleeding;
        }


        public static Texture2D GetPregnancyIcon(this HediffComp_Menstruation comp, Hediff hediff)
        {
            string icon = "";
            Texture2D result = null;
            int babycount = 1;
            if (hediff is Hediff_MechanoidPregnancy)
            {
                return ContentFinder<Texture2D>.Get(("Womb/Mechanoid_Fluid"), true);
            }
            else if (hediff is Hediff_BasePregnancy)
            {
                Hediff_BasePregnancy h = (Hediff_BasePregnancy)hediff;
                babycount = h.babies.Count;
                string fetustex = h.babies?.FirstOrDefault()?.def.GetModExtension<PawnDNAModExtension>()?.fetusTexPath ?? "Fetus/Fetus_Default";
                if (h.GestationProgress < 0.2f) icon = comp.wombTex + "_Implanted";
                else if (h.GestationProgress < 0.3f)
                {
                    if (h.babies?.First()?.def?.race?.FleshType == FleshTypeDefOf.Insectoid) icon += "Fetus/Insects/Insect_Early00";
                    else icon += "Fetus/Fetus_Early00";
                }
                else if (h.GestationProgress < 0.4f) icon += fetustex + "00";
                else if (h.GestationProgress < 0.5f) icon += fetustex + "01";
                else if (h.GestationProgress < 0.6f) icon += fetustex + "02";
                else if (h.GestationProgress < 0.7f) icon += fetustex + "03";
                else if (h.GestationProgress < 0.8f) icon += fetustex + "04";
                else icon += fetustex + "05";
            }
            else icon = "Fetus/Slime_Abomi02";

            result = TryGetTwinsIcon(icon, babycount);

            if (result == null) result = ContentFinder<Texture2D>.Get((icon), true);
            return result;
        }

        public static Texture2D TryGetTwinsIcon(string path, int babycount)
        {
            Texture2D result = null;
            for (int i = babycount; i > 1; i--)
            {
                result = ContentFinder<Texture2D>.Get((path + "_Multiplet_" + i), false);
                if (result != null) return result;
            }
            return null;
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
            if (comp.Pawn.health.hediffSet.GetHediffs<Hediff_InsectEgg>().FirstOrDefault() != null) return ContentFinder<Texture2D>.Get(("Womb/Womb_Egged"), true);
            string icon = comp.wombTex;
            HediffComp_Menstruation.Stage stage = comp.curStage;
            if (stage == HediffComp_Menstruation.Stage.Bleeding) icon += "_Bleeding";

            Texture2D wombtex = ContentFinder<Texture2D>.Get((icon), true);

            return wombtex;
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

        public static void DrawEggOverlay(this HediffComp_Menstruation comp, Rect wombRect)
        {
            Rect rect = new Rect(wombRect.xMax - wombRect.width / 3, wombRect.y, wombRect.width / 3, wombRect.width / 3);
            GUI.color = Color.white;
            GUI.DrawTexture(rect, comp.GetEggIcon(), ScaleMode.ScaleToFit);
        }

    }
}
