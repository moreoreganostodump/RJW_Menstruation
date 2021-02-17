using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RJW_Menstruation
{
    public class Configurations : ModSettings
    {
        public const float ImplantationChanceDefault = 0.25f;
        public const int ImplantationChanceAdjustDefault = 25;
        public const float FertilizeChanceDefault = 0.05f;
        public const int FertilizeChanceAdjustDefault = 50;
        public const float CumDecayRatioDefault = 0.05f;
        public const int CumDecayRatioAdjustDefault = 50;
        public const float CumFertilityDecayRatioDefault = 0.2f;
        public const int CumFertilityDecayRatioAdjustDefault = 200;
        public const int CycleAccelerationDefault = 6;
        public const float EnzygoticTwinsChanceDefault = 0.002f;
        public const int EnzygoticTwinsChanceAdjustDefault = 2;
        public const int MaxEnzygoticTwinsDefault = 9;

        public static float ImplantationChance = ImplantationChanceDefault;
        public static int ImplantationChanceAdjust = ImplantationChanceAdjustDefault;
        public static float FertilizeChance = FertilizeChanceDefault; 
        public static int FertilizeChanceAdjust = FertilizeChanceAdjustDefault;
        public static float CumDecayRatio = CumDecayRatioDefault;
        public static int CumDecayRatioAdjust = CumDecayRatioAdjustDefault;
        public static float CumFertilityDecayRatio = CumFertilityDecayRatioDefault;
        public static int CumFertilityDecayRatioAdjust = CumFertilityDecayRatioAdjustDefault;
        public static int CycleAcceleration = CycleAccelerationDefault;
        public static bool EnableWombIcon = true;
        public static bool EnableAnimalCycle = false;
        public static bool DrawWombStatus = true;
        public static bool DrawVaginaStatus = true;
        public static bool Debug = false;
        public static bool EnableMenopause = true;
        public static DetailLevel InfoDetail = DetailLevel.All;
        public static bool UseMultiplePregnancy = true;
        public static bool EnableHeteroOvularTwins = true;
        public static bool EnableEnzygoticTwins = true;
        public static float EnzygoticTwinsChance = EnzygoticTwinsChanceDefault;
        public static int EnzygoticTwinsChanceAdjust = EnzygoticTwinsChanceAdjustDefault;
        public static int MaxEnzygoticTwins = MaxEnzygoticTwinsDefault;



        public static bool HARActivated = false;
        public static bool LLActivated = false;

        public enum DetailLevel
        {
            All,
            OnReveal,
            HideFetusInfo,
            Hide
        }

        public static string LevelString(DetailLevel level)
        {
            switch (level)
            {
                case DetailLevel.All:
                    return "All";
                case DetailLevel.OnReveal:
                    return "On reveal";
                case DetailLevel.HideFetusInfo:
                    return "Hide fetus info";
                case DetailLevel.Hide:
                    return "Hide";
                default:
                    return "";

            }


        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ImplantationChanceAdjust, "ImplantationChanceAdjust", ImplantationChanceAdjust, true);
            Scribe_Values.Look(ref ImplantationChance, "ImplantationChance", ImplantationChance, true);
            Scribe_Values.Look(ref FertilizeChanceAdjust, "FertilizeChanceAdjust", FertilizeChanceAdjust, true);
            Scribe_Values.Look(ref FertilizeChance, "FertilizeChance", FertilizeChance, true);
            Scribe_Values.Look(ref CumDecayRatioAdjust, "CumDecayRatioAdjust", CumDecayRatioAdjust, true);
            Scribe_Values.Look(ref CumDecayRatio, "CumDecayRatio", CumDecayRatio, true);
            Scribe_Values.Look(ref CumFertilityDecayRatioAdjust, "CumFertilityDecayRatioAdjust", CumFertilityDecayRatioAdjust, true);
            Scribe_Values.Look(ref CumFertilityDecayRatio, "CumFertilityDecayRatio", CumFertilityDecayRatio, true);
            Scribe_Values.Look(ref CycleAcceleration, "CycleAcceleration", CycleAcceleration, true);
            Scribe_Values.Look(ref EnableWombIcon, "EnableWombIcon", EnableWombIcon, true);
            Scribe_Values.Look(ref EnableAnimalCycle, "EnableAnimalCycle", EnableAnimalCycle, true);
            Scribe_Values.Look(ref DrawWombStatus, "DrawWombStatus", DrawWombStatus, true);
            Scribe_Values.Look(ref DrawVaginaStatus, "DrawVaginaStatus", DrawVaginaStatus, true);
            Scribe_Values.Look(ref Debug, "Debug", Debug, true);
            Scribe_Values.Look(ref InfoDetail, "InfoDetail", InfoDetail, true);
            Scribe_Values.Look(ref EnableMenopause, "EnableMenopause", EnableMenopause, true);
            Scribe_Values.Look(ref UseMultiplePregnancy, "UseMultiplePregnancy", UseMultiplePregnancy, true);
            Scribe_Values.Look(ref EnableHeteroOvularTwins, "EnableHeteroOvularTwins", EnableHeteroOvularTwins, true);
            Scribe_Values.Look(ref EnableEnzygoticTwins, "EnableEnzygoticTwins", EnableEnzygoticTwins, true);
            Scribe_Values.Look(ref EnzygoticTwinsChance, "EnzygoticTwinsChance", EnzygoticTwinsChance, true);
            Scribe_Values.Look(ref EnzygoticTwinsChanceAdjust, "EnzygoticTwinsChanceAdjust", EnzygoticTwinsChanceAdjust, true);
            Scribe_Values.Look(ref MaxEnzygoticTwins, "MaxEnzygoticTwins", MaxEnzygoticTwins, true);
            base.ExposeData();
        }


    }


    public class RJW_Menstruation : Mod
    {

        private readonly Configurations config;
        private static Vector2 scroll;


        public RJW_Menstruation(ModContentPack content) : base(content)
        {
            config = GetSettings<Configurations>();
            Configurations.HARActivated = ModLister.HasActiveModWithName("Humanoid Alien Races 2.0");
            Configurations.LLActivated = ModLister.HasActiveModWithName("RimJobWorld - Licentia Labs");
        }

        

        public override string SettingsCategory()
        {
            return Translations.Mod_Title;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect outRect = new Rect(0f, 30f, inRect.width, inRect.height - 30f);
            Rect mainRect = new Rect(0f, 0f, inRect.width - 30f, inRect.height + 300f);
            Listing_Standard listmain = new Listing_Standard();
            listmain.maxOneColumn = true;
            listmain.BeginScrollView(outRect, ref scroll, ref mainRect);
            listmain.Begin(mainRect);
            listmain.Gap(20f);
            listmain.CheckboxLabeled(Translations.Option1_Label, ref Configurations.EnableWombIcon, Translations.Option1_Desc);
            if (Configurations.EnableWombIcon)
            {
                Listing_Standard wombsection = listmain.BeginSection_NewTemp(111);
                wombsection.CheckboxLabeled(Translations.Option9_Label, ref Configurations.DrawWombStatus, Translations.Option9_Desc);
                wombsection.CheckboxLabeled(Translations.Option10_Label, ref Configurations.DrawVaginaStatus, Translations.Option10_Desc);
                if (wombsection.ButtonText(Translations.Option11_Label + ": " + Configurations.LevelString(Configurations.InfoDetail)))
                {
                    if (Configurations.InfoDetail == Configurations.DetailLevel.Hide) Configurations.InfoDetail = Configurations.DetailLevel.All;
                    else Configurations.InfoDetail++;
                }
                switch (Configurations.InfoDetail)
                {
                    case Configurations.DetailLevel.All:
                        wombsection.Label(Translations.Option11_Desc_1);
                        break;
                    case Configurations.DetailLevel.OnReveal:
                        wombsection.Label(Translations.Option11_Desc_2);
                        break;
                    case Configurations.DetailLevel.HideFetusInfo:
                        wombsection.Label(Translations.Option11_Desc_3);
                        break;
                    case Configurations.DetailLevel.Hide:
                        wombsection.Label(Translations.Option11_Desc_4);
                        break;
                }
                listmain.EndSection(wombsection);
            }
            
            listmain.CheckboxLabeled(Translations.Option2_Label, ref Configurations.EnableAnimalCycle, Translations.Option2_Desc);

            listmain.CheckboxLabeled(Translations.Option12_Label, ref Configurations.EnableMenopause, Translations.Option12_Desc);
            
            listmain.Label(Translations.Option3_Label + " " + Configurations.ImplantationChance*100 + "%", -1, Translations.Option3_Desc);
            Configurations.ImplantationChanceAdjust = (int)listmain.Slider(Configurations.ImplantationChanceAdjust, 0, 1000);
            Configurations.ImplantationChance = (float)Configurations.ImplantationChanceAdjust/100;

            listmain.Label(Translations.Option4_Label + " " + Configurations.FertilizeChance*100 + "%", -1, Translations.Option4_Desc);
            Configurations.FertilizeChanceAdjust = (int)listmain.Slider(Configurations.FertilizeChanceAdjust, 0, 1000);
            Configurations.FertilizeChance = (float)Configurations.FertilizeChanceAdjust/1000;

            listmain.Label(Translations.Option5_Label + " " + Configurations.CumDecayRatio*100 + "%", -1, Translations.Option5_Desc);
            Configurations.CumDecayRatioAdjust = (int)listmain.Slider(Configurations.CumDecayRatioAdjust, 0, 1000);
            Configurations.CumDecayRatio = (float)Configurations.CumDecayRatioAdjust/1000;
            
            listmain.Label(Translations.Option6_Label + " " + Configurations.CumFertilityDecayRatio*100 + "%", -1, Translations.Option6_Desc);
            Configurations.CumFertilityDecayRatioAdjust = (int)listmain.Slider(Configurations.CumFertilityDecayRatioAdjust, 0, 1000);
            Configurations.CumFertilityDecayRatio = (float)Configurations.CumFertilityDecayRatioAdjust/1000;

            listmain.Label(Translations.Option7_Label + " x" + Configurations.CycleAcceleration, -1, Translations.Option7_Desc);
            Configurations.CycleAcceleration = (int)listmain.Slider(Configurations.CycleAcceleration,1,50);

            listmain.CheckboxLabeled(Translations.Option13_Label, ref Configurations.UseMultiplePregnancy, Translations.Option13_Desc);
            if (Configurations.UseMultiplePregnancy)
            {
                float sectionheight = 50f;
                if (Configurations.EnableEnzygoticTwins) sectionheight += 100;
                Listing_Standard twinsection = listmain.BeginSection_NewTemp(sectionheight);
                twinsection.CheckboxLabeled(Translations.Option14_Label, ref Configurations.EnableHeteroOvularTwins, Translations.Option14_Desc);
                twinsection.CheckboxLabeled(Translations.Option15_Label, ref Configurations.EnableEnzygoticTwins, Translations.Option15_Desc);
                if (Configurations.EnableEnzygoticTwins)
                {
                    twinsection.Label(Translations.Option16_Label + " " + Configurations.EnzygoticTwinsChance*100 + "%", -1, Translations.Option16_Desc);
                    Configurations.EnzygoticTwinsChanceAdjust = (int)twinsection.Slider(Configurations.EnzygoticTwinsChanceAdjust, 0, 1000);
                    Configurations.EnzygoticTwinsChance = (float)Configurations.EnzygoticTwinsChanceAdjust / 1000;

                    twinsection.Label(Translations.Option17_Label + " " + Configurations.MaxEnzygoticTwins, -1, Translations.Option17_Desc);
                    Configurations.MaxEnzygoticTwins = (int)twinsection.Slider(Configurations.MaxEnzygoticTwins, 2, 100);
                }


                listmain.EndSection(twinsection);
            }
            listmain.EndScrollView(ref mainRect);

            listmain.CheckboxLabeled(Translations.Option8_Label, ref Configurations.Debug, Translations.Option8_Desc);
            if (listmain.ButtonText("reset to default"))
            {
                Configurations.ImplantationChanceAdjust = Configurations.ImplantationChanceAdjustDefault;
                Configurations.FertilizeChanceAdjust = Configurations.FertilizeChanceAdjustDefault;
                Configurations.CumDecayRatioAdjust = Configurations.CumDecayRatioAdjustDefault;
                Configurations.CumFertilityDecayRatioAdjust = Configurations.CumFertilityDecayRatioAdjustDefault;
                Configurations.EnableWombIcon = true;
                Configurations.EnableAnimalCycle = false;
                Configurations.CycleAcceleration = Configurations.CycleAccelerationDefault;
                Configurations.EnzygoticTwinsChanceAdjust = Configurations.EnzygoticTwinsChanceAdjustDefault;
                Configurations.EnableEnzygoticTwins = true;
                Configurations.EnableHeteroOvularTwins = true;
                Configurations.UseMultiplePregnancy = true;
                Configurations.MaxEnzygoticTwins = Configurations.MaxEnzygoticTwinsDefault;
            }

            listmain.End();


        }


    }




}
