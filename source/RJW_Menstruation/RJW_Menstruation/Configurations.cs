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
        public static readonly float ImplantationChanceDefault = 0.25f;
        public static readonly int ImplantationChanceAdjustDefault = 25;
        public static readonly float FertilizeChanceDefault = 0.05f;
        public static readonly int FertilizeChanceAdjustDefault = 50;
        public static readonly float CumDecayRatioDefault = 0.05f;
        public static readonly int CumDecayRatioAdjustDefault = 50;
        public static readonly float CumFertilityDecayRatioDefault = 0.2f;
        public static readonly int CumFertilityDecayRatioAdjustDefault = 200;
        public static readonly int CycleAccelerationDefault = 6;


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
            Scribe_Values.Look(ref EnableWombIcon, "EnableWombIcon", EnableWombIcon, true);
            Scribe_Values.Look(ref EnableAnimalCycle, "EnableAnimalCycle", EnableAnimalCycle, true);
            Scribe_Values.Look(ref DrawWombStatus, "DrawWombStatus", DrawWombStatus, true);
            Scribe_Values.Look(ref DrawVaginaStatus, "DrawVaginaStatus", DrawVaginaStatus, true);
            Scribe_Values.Look(ref Debug, "Debug", Debug, true);
            Scribe_Values.Look(ref InfoDetail, "InfoDetail", InfoDetail, true);
            Scribe_Values.Look(ref EnableMenopause, "EnableMenopause", EnableMenopause, true);
            base.ExposeData();
        }


    }


    public class RJW_Menstruation : Mod
    {

        private readonly Configurations config;
        public RJW_Menstruation(ModContentPack content) : base(content)
        {
            config = GetSettings<Configurations>();

        }

        public override string SettingsCategory()
        {
            return Translations.Mod_Title;
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect mainRect = inRect.ContractedBy(20f);
            Listing_Standard listmain = new Listing_Standard();
            listmain.Begin(mainRect);

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
            }


            listmain.End();


        }


    }




}
