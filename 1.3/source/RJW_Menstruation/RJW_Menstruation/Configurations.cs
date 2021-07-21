using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RJW_Menstruation
{
    public class Configurations : ModSettings
    {
        public const float ImplantationChanceDefault = 0.65f;
        public const int ImplantationChanceAdjustDefault = 65;
        public const float FertilizeChanceDefault = 0.15f;
        public const int FertilizeChanceAdjustDefault = 150;
        public const float CumDecayRatioDefault = 0.30f;
        public const int CumDecayRatioAdjustDefault = 300;
        public const float CumFertilityDecayRatioDefault = 0.2f;
        public const int CumFertilityDecayRatioAdjustDefault = 200;
        public const int CycleAccelerationDefault = 6;
        public const float EnzygoticTwinsChanceDefault = 0.002f;
        public const int EnzygoticTwinsChanceAdjustDefault = 2;
        public const int MaxEnzygoticTwinsDefault = 9;
        public const int BleedingAmountDefault = 50;
        public const float NippleTransitionVarianceDefault = 0.2f;
        public const float NipplePermanentTransitionVarianceDefault = 0.02f;
        public const float NippleMaximumTransitionDefault = 0.4f;
        public const float NippleTransitionSpeedDefault = 0.035f;

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
        public static bool DrawEggOverlay = true;
        public static bool Debug = false;
        public static bool EnableMenopause = true;
        public static DetailLevel InfoDetail = DetailLevel.All;
        public static bool UseMultiplePregnancy = true;
        public static bool EnableHeteroOvularTwins = true;
        public static bool EnableEnzygoticTwins = true;
        public static float EnzygoticTwinsChance = EnzygoticTwinsChanceDefault;
        public static int EnzygoticTwinsChanceAdjust = EnzygoticTwinsChanceAdjustDefault;
        public static int MaxEnzygoticTwins = MaxEnzygoticTwinsDefault;
        public static int BleedingAmount = BleedingAmountDefault;
        public static bool EnableButtonInHT = false;
        public static PawnFlags ShowFlag = PawnFlags.Colonist | PawnFlags.Prisoner;
        public static bool UseHybridExtention = true;
        public static bool MotherFirst = false;
        public static bool AllowShrinkIcon = false;

        public static float NippleTransitionVariance = NippleTransitionVarianceDefault;
        public static float NipplePermanentTransitionVariance = NipplePermanentTransitionVarianceDefault;
        public static float NippleMaximumTransition = NippleMaximumTransitionDefault;
        public static float NippleTransitionSpeed = NippleTransitionSpeedDefault;
        public static float NippleTransitionRatio
        {
            get
            {
                return NippleTransitionVariance * NippleTransitionSpeed;
            }
        }
        


        public static List<HybridInformations> HybridOverride = new List<HybridInformations>();


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
        public static string HybridString(bool b)
        {
            if (b) return Translations.Option23_Label_1;
            else return Translations.Option23_Label_2;
        }

        public static bool IsOverrideExist(ThingDef def)
        {
            List<HybridInformations> removeList = new List<HybridInformations>();
            if (!HybridOverride.NullOrEmpty())
                foreach(HybridInformations o in HybridOverride)
                {
                    if (o.IsNull) removeList.Add(o);
                    if (o.defName == def.defName) return true;
                }
            if (!removeList.NullOrEmpty())
            {
                foreach(HybridInformations o in removeList)
                {
                    HybridOverride.Remove(o);
                }
            }
            removeList.Clear();
            return false;
        }

        [Flags]
        public enum PawnFlags
        {
            None = 0,
            Colonist = 1,
            Prisoner = 2,
            Ally = 4,
            Neutral = 8,
            Hostile = 16
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
            Scribe_Values.Look(ref DrawEggOverlay, "DrawEggOvray", DrawEggOverlay, true);
            Scribe_Values.Look(ref Debug, "Debug", Debug, true);
            Scribe_Values.Look(ref InfoDetail, "InfoDetail", InfoDetail, true);
            Scribe_Values.Look(ref EnableMenopause, "EnableMenopause", EnableMenopause, true);
            Scribe_Values.Look(ref UseMultiplePregnancy, "UseMultiplePregnancy", UseMultiplePregnancy, true);
            Scribe_Values.Look(ref EnableHeteroOvularTwins, "EnableHeteroOvularTwins", EnableHeteroOvularTwins, true);
            Scribe_Values.Look(ref EnableEnzygoticTwins, "EnableEnzygoticTwins", EnableEnzygoticTwins, true);
            Scribe_Values.Look(ref EnzygoticTwinsChance, "EnzygoticTwinsChance", EnzygoticTwinsChance, true);
            Scribe_Values.Look(ref EnzygoticTwinsChanceAdjust, "EnzygoticTwinsChanceAdjust", EnzygoticTwinsChanceAdjust, true);
            Scribe_Values.Look(ref MaxEnzygoticTwins, "MaxEnzygoticTwins", MaxEnzygoticTwins, true);
            Scribe_Values.Look(ref BleedingAmount, "BleedingAmount", BleedingAmount, true);
            Scribe_Values.Look(ref EnableButtonInHT, "EnableButtonInHT", EnableButtonInHT, true);
            Scribe_Values.Look(ref ShowFlag, "ShowFlag", ShowFlag, true);
            Scribe_Values.Look(ref UseHybridExtention, "UseHybridExtention", UseHybridExtention, true);
            Scribe_Values.Look(ref MotherFirst, "MotherFirst", MotherFirst, true);
            Scribe_Values.Look(ref NippleTransitionVariance, "NippleTransitionVariance", NippleTransitionVariance, true);
            Scribe_Values.Look(ref NipplePermanentTransitionVariance, "NipplePermanentTransitionVariance", NipplePermanentTransitionVariance, true);
            Scribe_Values.Look(ref NippleMaximumTransition, "NippleMaximumTransition", NippleMaximumTransition, true);
            Scribe_Values.Look(ref NippleTransitionSpeed, "NippleTransitionSpeed", NippleTransitionSpeed, true);
            Scribe_Values.Look(ref AllowShrinkIcon, "AllowShrinkIcon", AllowShrinkIcon, true);
            Scribe_Collections.Look(ref HybridOverride, saveDestroyedThings: true, label: "HybridOverride", lookMode: LookMode.Deep, ctorArgs: new object[0]);
            base.ExposeData();
        }

       

    }


    public class RJW_Menstruation : Mod
    {

        private readonly Configurations config;
        private static Vector2 scroll;



        public static float EstimatedBleedingAmount
        {
            get
            {
                int days = VariousDefOf.VaginaCompProperties.bleedingIntervalDays;
                return days * 0.03f * Configurations.BleedingAmount * 6;
            }
        }

        public static float EstimatedBleedingAmountPerHour
        {
            get
            {
                int days = VariousDefOf.VaginaCompProperties.bleedingIntervalDays;
                return 0.03f * Configurations.BleedingAmount * Configurations.CycleAcceleration;
            }
        }


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
            Rect mainRect = new Rect(0f, 0f, inRect.width - 30f, inRect.height + 480f);
    
            Listing_Standard listmain = new Listing_Standard();
            listmain.maxOneColumn = true;
            Widgets.BeginScrollView(outRect, ref scroll, mainRect);
            listmain.Begin(mainRect);
            listmain.Gap(20f);
            Rect optionrect1 = listmain.GetRect(30f);
            Widgets.CheckboxLabeled(optionrect1.LeftHalf(), Translations.Option1_Label_1, ref Configurations.EnableWombIcon,false,null,null,true);
            Widgets.CheckboxLabeled(optionrect1.RightHalf(), Translations.Option1_Label_2, ref Configurations.EnableButtonInHT, false, null, null, true);
            //listmain.CheckboxLabeled(Translations.Option1_Label, ref Configurations.EnableWombIcon, Translations.Option1_Desc);
            if (Configurations.EnableWombIcon || Configurations.EnableButtonInHT)
            {
                Listing_Standard wombsection = listmain.BeginSection(400);
                wombsection.CheckboxLabeled(Translations.Option9_Label, ref Configurations.DrawWombStatus, Translations.Option9_Desc);
                if (Configurations.DrawWombStatus)
                {
                    wombsection.CheckboxLabeled(Translations.Option18_Label, ref Configurations.DrawEggOverlay, Translations.Option18_Desc);
                }
                
                wombsection.CheckboxLabeled(Translations.Option10_Label, ref Configurations.DrawVaginaStatus, Translations.Option10_Desc);
                wombsection.CheckboxLabeled(Translations.Option29_Label, ref Configurations.AllowShrinkIcon, Translations.Option29_Desc);
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
                wombsection.Label(Translations.Option21_Label + " " + Configurations.ShowFlag, -1, Translations.Option21_Desc);
                Rect flagrect = wombsection.GetRect(30f);
                Rect[] flagrects = new Rect[5];
                for (int i = 0; i < 5; i++)
                {
                    flagrects[i] = new Rect(flagrect.x + (flagrect.width / 5) * i, flagrect.y, flagrect.width / 5, flagrect.height);
                }

                if (Widgets.ButtonText(flagrects[0], Translations.Option20_Label_1 + ": " + Configurations.ShowFlag.HasFlag(Configurations.PawnFlags.Colonist)))
                {
                    Configurations.ShowFlag ^= Configurations.PawnFlags.Colonist;
                }
                if (Widgets.ButtonText(flagrects[1], Translations.Option20_Label_2 + ": " + Configurations.ShowFlag.HasFlag(Configurations.PawnFlags.Prisoner)))
                {
                    Configurations.ShowFlag ^= Configurations.PawnFlags.Prisoner;
                }
                if (Widgets.ButtonText(flagrects[2], Translations.Option20_Label_3 + ": " + Configurations.ShowFlag.HasFlag(Configurations.PawnFlags.Ally)))
                {
                    Configurations.ShowFlag ^= Configurations.PawnFlags.Ally;
                }
                if (Widgets.ButtonText(flagrects[3], Translations.Option20_Label_4 + ": " + Configurations.ShowFlag.HasFlag(Configurations.PawnFlags.Neutral)))
                {
                    Configurations.ShowFlag ^= Configurations.PawnFlags.Neutral;
                }
                if (Widgets.ButtonText(flagrects[4], Translations.Option20_Label_5 + ": " + Configurations.ShowFlag.HasFlag(Configurations.PawnFlags.Hostile)))
                {
                    Configurations.ShowFlag ^= Configurations.PawnFlags.Hostile;
                }

                int Adjust = (int)(Configurations.NippleTransitionVariance * 1000);
                wombsection.Label(Translations.Option24_Label + " " + Configurations.NippleTransitionVariance* 100 + " / 100", -1,Translations.Option24_Desc);
                Adjust = (int)wombsection.Slider(Adjust,0,1000);
                Configurations.NippleTransitionVariance = (float)Adjust / 1000;

                Adjust = (int)(Configurations.NipplePermanentTransitionVariance * 1000);
                wombsection.Label(Translations.Option25_Label + " " + Configurations.NipplePermanentTransitionVariance*100 + " / 100", -1, Translations.Option25_Desc);
                Adjust = (int)wombsection.Slider(Adjust, 0, 1000);
                Configurations.NipplePermanentTransitionVariance = (float)Adjust / 1000;

                Adjust = (int)(Configurations.NippleMaximumTransition * 1000);
                wombsection.Label(Translations.Option26_Label + " " + Configurations.NippleMaximumTransition* 100 + " / 100", -1, Translations.Option26_Desc);
                Adjust = (int)wombsection.Slider(Adjust, 0, 1000);
                Configurations.NippleMaximumTransition = (float)Adjust / 1000;

                Adjust = (int)(Configurations.NippleTransitionSpeed * 1000);
                wombsection.Label(Translations.Option27_Label + " " + Configurations.NippleTransitionSpeed, -1, Translations.Option27_Desc);
                Adjust = (int)wombsection.Slider(Adjust, 0, 1000);
                Configurations.NippleTransitionSpeed = (float)Adjust / 1000;

                listmain.EndSection(wombsection);
            }

            listmain.CheckboxLabeled(Translations.Option2_Label, ref Configurations.EnableAnimalCycle, Translations.Option2_Desc);

            listmain.CheckboxLabeled(Translations.Option12_Label, ref Configurations.EnableMenopause, Translations.Option12_Desc);

            listmain.Label(Translations.Option3_Label + " " + Configurations.ImplantationChance * 100 + "%", -1, Translations.Option3_Desc);
            Configurations.ImplantationChanceAdjust = (int)listmain.Slider(Configurations.ImplantationChanceAdjust, 0, 1000);
            Configurations.ImplantationChance = (float)Configurations.ImplantationChanceAdjust / 100;

            listmain.Label(Translations.Option4_Label + " " + Configurations.FertilizeChance * 100 + "%", -1, Translations.Option4_Desc);
            Configurations.FertilizeChanceAdjust = (int)listmain.Slider(Configurations.FertilizeChanceAdjust, 0, 1000);
            Configurations.FertilizeChance = (float)Configurations.FertilizeChanceAdjust / 1000;

            listmain.Label(Translations.Option5_Label + " " + Configurations.CumDecayRatio * 100 + "%", -1, Translations.Option5_Desc);
            Configurations.CumDecayRatioAdjust = (int)listmain.Slider(Configurations.CumDecayRatioAdjust, 0, 1000);
            Configurations.CumDecayRatio = (float)Configurations.CumDecayRatioAdjust / 1000;

            int semenlifespan = (int)(-5 / ((float)Math.Log10((1 - Configurations.CumFertilityDecayRatio)*10) - 1)) + 1;
            string estimatedlifespan;
            if (semenlifespan < 0)
            {
                estimatedlifespan = String.Format(": Infinite", semenlifespan);
            }
            else
            {
                estimatedlifespan = String.Format(": {0:0}h", semenlifespan);
            }
            listmain.LabelDouble(Translations.Option6_Label + " " + Configurations.CumFertilityDecayRatio * 100 + "%", Translations.EstimatedCumLifespan + estimatedlifespan, Translations.Option6_Desc);
            Configurations.CumFertilityDecayRatioAdjust = (int)listmain.Slider(Configurations.CumFertilityDecayRatioAdjust, 0, 1000);
            Configurations.CumFertilityDecayRatio = (float)Configurations.CumFertilityDecayRatioAdjust / 1000;

            listmain.Label(Translations.Option7_Label + " x" + Configurations.CycleAcceleration, -1, Translations.Option7_Desc);
            Configurations.CycleAcceleration = (int)listmain.Slider(Configurations.CycleAcceleration, 1, 50);


            float var2 = EstimatedBleedingAmountPerHour;
            float var1 = Math.Max(EstimatedBleedingAmount, var2);
            listmain.LabelDouble(Translations.Option19_Label_1, Translations.Option19_Label_2 + ": " + var1 + "ml, " + var2 + "ml/h", Translations.Option19_Desc);
            Configurations.BleedingAmount = (int)listmain.Slider(Configurations.BleedingAmount, 0, 200);

            listmain.CheckboxLabeled(Translations.Option13_Label, ref Configurations.UseMultiplePregnancy, Translations.Option13_Desc);
            if (Configurations.UseMultiplePregnancy)
            {
                float sectionheight = 75f;
                if (Configurations.EnableEnzygoticTwins) sectionheight += 100;
                Listing_Standard twinsection = listmain.BeginSection(sectionheight);
                Rect hybridrect = twinsection.GetRect(25);
                Widgets.CheckboxLabeled(hybridrect.LeftHalf(), Translations.Option22_Label, ref Configurations.UseHybridExtention, false, null, null, true);
                if (Widgets.ButtonText(hybridrect.RightHalf(), Translations.Option28_Label))
                {
                    Dialog_HybridCustom.ToggleWindow();
                    //Configurations.MotherFirst = !Configurations.MotherFirst;
                }
                TooltipHandler.TipRegion(hybridrect, Translations.Option28_Tooltip);

                twinsection.CheckboxLabeled(Translations.Option14_Label, ref Configurations.EnableHeteroOvularTwins, Translations.Option14_Desc);
                twinsection.CheckboxLabeled(Translations.Option15_Label, ref Configurations.EnableEnzygoticTwins, Translations.Option15_Desc);
                if (Configurations.EnableEnzygoticTwins)
                {
                    twinsection.Label(Translations.Option16_Label + " " + Configurations.EnzygoticTwinsChance * 100 + "%", -1, Translations.Option16_Desc);
                    Configurations.EnzygoticTwinsChanceAdjust = (int)twinsection.Slider(Configurations.EnzygoticTwinsChanceAdjust, 0, 1000);
                    Configurations.EnzygoticTwinsChance = (float)Configurations.EnzygoticTwinsChanceAdjust / 1000;

                    twinsection.Label(Translations.Option17_Label + " " + Configurations.MaxEnzygoticTwins, -1, Translations.Option17_Desc);
                    Configurations.MaxEnzygoticTwins = (int)twinsection.Slider(Configurations.MaxEnzygoticTwins, 2, 100);
                }
                listmain.EndSection(twinsection);
            }


            Widgets.EndScrollView();

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
                Configurations.BleedingAmount = Configurations.BleedingAmountDefault;
                Configurations.MotherFirst = false;
                Configurations.NippleTransitionVariance = Configurations.NippleTransitionVarianceDefault;
                Configurations.NipplePermanentTransitionVariance = Configurations.NipplePermanentTransitionVarianceDefault;
                Configurations.NippleMaximumTransition = Configurations.NippleMaximumTransitionDefault;
                Configurations.NippleTransitionSpeed = Configurations.NippleTransitionSpeedDefault;

            }

            listmain.End();


        }


    }




}
