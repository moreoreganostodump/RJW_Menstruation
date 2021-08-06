using System;
using RimWorld;
using rjw;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RJW_Menstruation
{
    public class Dialog_WombStatus : Window
    {
        public Pawn pawn;
        private HediffComp_Menstruation comp;
        private const float windowMargin = 20f;
        private const float pawnRectWidth = 150f;
        private const float pawnRectHeight = 150f;
        private const float wombRectHeight = 300f;
        private const float wombRectWidth = 300f;
        private const float fontheight = 30;
        private const float genitalRectWidth = 102;
        private const float genitalRectHeight = 140;
        private const float breastRectWidth = 102;
        private const float breastRectHeight = 100;

        private static bool naked = false;

        private Texture2D womb;
        private Texture2D cum;
        private Texture2D vagina;
        private Texture2D anal;
        private Color cumcolor;

        private GUIStyle fontstylecenter = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
        private GUIStyle fontstyleright = new GUIStyle() { alignment = TextAnchor.MiddleRight };
        private GUIStyle fontstyleleft = new GUIStyle() { alignment = TextAnchor.MiddleLeft };
        private GUIStyle boxstyle = new GUIStyle(GUI.skin.textArea);
        private GUIStyle buttonstyle = new GUIStyle(GUI.skin.button);




        public override Vector2 InitialSize
        {
            get
            {
                float width = 450f + 2 * windowMargin;
                float height = 780f + 2 * windowMargin;
                if (!Configurations.DrawWombStatus) height -= wombRectHeight;
                if (!Configurations.DrawVaginaStatus || pawn.IsAnimal()) width -= 150f;
                return new Vector2(width, height);
            }
        }

        public Dialog_WombStatus(Pawn pawn, HediffComp_Menstruation comp)
        {
            this.pawn = pawn;
            this.comp = comp;
        }

        public void ChangePawn(Pawn pawn, HediffComp_Menstruation comp)
        {

            if (this.pawn.IsAnimal() && !pawn.IsAnimal()) windowRect.width += 150f;
            else if (!this.pawn.IsAnimal() && pawn.IsAnimal()) windowRect.width -= 150f;
            this.pawn = pawn;
            this.comp = comp;
        }

        public static void ToggleWindow(Pawn pawn, HediffComp_Menstruation comp)
        {
            Dialog_WombStatus window = (Dialog_WombStatus)Find.WindowStack.Windows.FirstOrDefault(x => x.GetType().Equals(typeof(Dialog_WombStatus)));
            if (window != null)
            {
                if (window.pawn != pawn)
                {
                    SoundDefOf.TabOpen.PlayOneShotOnCamera();
                    window.ChangePawn(pawn, comp);
                }
                else Find.WindowStack.TryRemove(typeof(Dialog_WombStatus), true);
            }
            else
            {
                SoundDefOf.InfoCard_Open.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_WombStatus(pawn, comp));
            }


        }

        public override void DoWindowContents(Rect inRect)
        {
            soundClose = SoundDefOf.InfoCard_Close;
            //closeOnClickedOutside = true;
            absorbInputAroundWindow = false;
            forcePause = false;
            preventCameraMotion = false;
            draggable = true;
            //resizeable = true;

            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
            {
                Event.current.Use();
            }

            Rect windowRect = inRect.ContractedBy(windowMargin);
            Rect mainRect = new Rect(windowRect.x, windowRect.y, windowRect.width, windowRect.height);
            Rect closeRect = new Rect(windowRect.xMax, 0f, 20f, 20f);
            MainContents(mainRect);
            if (Widgets.CloseButtonFor(closeRect))
            {
                Close();
            }
            closeRect.x -= 20f;

        }

        private void MainContents(Rect mainRect)
        {

            boxstyle.hover = boxstyle.normal;
            boxstyle.onHover = boxstyle.normal;
            boxstyle.onNormal = boxstyle.normal;

            buttonstyle.onHover = buttonstyle.onNormal;
            buttonstyle.hover = buttonstyle.normal;
            boxstyle.border.left = 4; boxstyle.border.right = 4; boxstyle.border.bottom = 4; boxstyle.border.top = 4;

            fontstyleleft.normal.textColor = Color.white;

            float preginfoheight = 0f;
            bool pregnant = pawn.IsPregnant();
            Hediff hediff = PregnancyHelper.GetPregnancy(pawn);
            if (pregnant && Utility.ShowFetusImage((Hediff_BasePregnancy)hediff))
            {
                womb = comp.GetPregnancyIcon(hediff);
                if (hediff is Hediff_MultiplePregnancy)
                {
                    Hediff_MultiplePregnancy h = (Hediff_MultiplePregnancy)hediff;
                    if (h.GestationProgress < 0.2f) cum = comp.GetCumIcon();
                    else cum = ContentFinder<Texture2D>.Get(("Womb/Empty"), true);
                    Pawn fetus = pawn.GetFetus();
                    if (fetus != null && Utility.ShowFetusInfo())
                    {
                        string feinfo = h.GetBabyInfo();
                        string fainfo = h.GetFatherInfo() + "  ";
                        if (feinfo.Length + fainfo.Length > 45)
                        {
                            preginfoheight = fontheight + 2;
                            buttonstyle.alignment = TextAnchor.UpperLeft;
                            fontstyleright.alignment = TextAnchor.LowerRight;
                        }
                        else
                        {
                            preginfoheight = fontheight;
                            buttonstyle.alignment = TextAnchor.MiddleLeft;

                        }
                        Rect preginfo = new Rect(0f, mainRect.yMax - wombRectHeight - 2, wombRectWidth, preginfoheight);
                        fontstyleright.normal.textColor = Color.white;
                        GUI.Box(preginfo, feinfo, buttonstyle);
                        GUI.Label(preginfo, fainfo, fontstyleright);
                    }

                }
                else if (hediff is Hediff_BasePregnancy)
                {
                    Hediff_BasePregnancy h = (Hediff_BasePregnancy)hediff;
                    if (h.GestationProgress < 0.2f) cum = comp.GetCumIcon();
                    else cum = ContentFinder<Texture2D>.Get(("Womb/Empty"), true);
                    Pawn fetus = pawn.GetFetus();
                    if (fetus != null && Utility.ShowFetusInfo())
                    {

                        preginfoheight = fontheight;
                        Rect preginfo = new Rect(0f, mainRect.yMax - wombRectHeight - 2, wombRectWidth, preginfoheight);
                        fontstyleright.normal.textColor = Color.white;
                        fontstyleright.alignment = TextAnchor.MiddleRight;
                        buttonstyle.alignment = TextAnchor.MiddleLeft;

                        GUI.Box(preginfo, h.babies.Count + " " + fetus.def.label + " " + Translations.Dialog_WombInfo02, buttonstyle);
                        GUI.Label(preginfo, Translations.Dialog_WombInfo03 + ": " + h.father.LabelShort + "  ", fontstyleright);
                    }

                }
                else cum = ContentFinder<Texture2D>.Get(("Womb/Empty"), true);
            }
            else
            {
                womb = comp.GetWombIcon();
                cum = comp.GetCumIcon();
            }


            Rect pawnRect = new Rect(0, 0, pawnRectWidth, pawnRectHeight);
            Widgets.DrawTextureFitted(pawnRect, PortraitsCache.Get(pawn, pawnRect.size, Rot4.South, default, 1, true, true, false, !naked), 1.0f);
            if (Widgets.ButtonInvisible(pawnRect))
            {
                naked = !naked;
            }
            Rect pawnLabelRect = new Rect(0, pawnRectHeight, pawnRectWidth, fontheight - 10);
            Rect pawnLabel2Rect = new Rect(0, pawnRectHeight + fontheight - 10, pawnRectWidth, fontheight - 10);
            fontstylecenter.normal.textColor = pawn.DrawColor;
            GUI.Label(pawnLabelRect, pawn.Name.ToStringFull, fontstylecenter);
            if (pawn.story != null) GUI.Label(pawnLabel2Rect, pawn.story.Title, fontstylecenter);
            GUI.color = Color.white;



            float wombrecth = 0;
            if (Configurations.DrawWombStatus)
            {
                wombrecth = wombRectHeight;
                cumcolor = comp.GetCumMixtureColor;
                Rect wombRect = new Rect(0f, mainRect.yMax - wombRectHeight + preginfoheight, wombRectWidth, wombRectWidth * 0.9f);
                DrawWomb(wombRect);


                if (Configurations.DrawEggOverlay)
                {
                    comp.DrawEggOverlay(wombRect);
                }


            }

            Rect wombInfoRect = new Rect(0f, mainRect.yMax - wombrecth - fontheight - 2, wombRectWidth, fontheight);

            buttonstyle.normal.textColor = Color.white;
            //boxstyle.normal.background = Texture2D.whiteTexture;
            buttonstyle.alignment = TextAnchor.MiddleLeft;
            GUI.backgroundColor = new Color(0.24f, 0.29f, 0.35f, 1);
            GUI.Box(wombInfoRect, Translations.Dialog_WombInfo01 + ": " + comp.GetCurStageLabel, buttonstyle);
            GUI.color = Color.white;


            fontstyleright.normal.textColor = Color.red;
            fontstyleright.alignment = TextAnchor.MiddleRight;
            //if (comp.GetFertilization) GUI.Label(wombInfoRect, Translations.Dialog_WombInfo05 + "  ", fontstyleright);
            //else if (comp.GetEggFertilizing) GUI.Label(wombInfoRect, Translations.Dialog_WombInfo06 + "  ", fontstyleright);
            //else if (comp.GetEgg) GUI.Label(wombInfoRect, Translations.Dialog_WombInfo07 + "  ", fontstyleright);
            GUI.Label(wombInfoRect, comp.GetFertilizingInfo + "  ", fontstyleright);


            //Widgets.Label(wombInfoRect,Translations.Dialog_WombInfo01 + ": " + comp.GetCurStageLabel);

            Rect cumlistTitle, cumlistRect;
            if (Configurations.DrawVaginaStatus && !pawn.IsAnimal())
            {
                Rect genitalRect = new Rect(pawnRectWidth + 24, pawnRectHeight + 2 * fontheight, genitalRectWidth, genitalRectHeight + fontheight * 2);
                Rect breastRect = new Rect(pawnRectWidth + 24, 40f, breastRectWidth, breastRectHeight + fontheight);
                DrawVagina(genitalRect);
                DrawBreast(breastRect);
                cumlistTitle = new Rect(wombRectWidth + 4f, 0, 150f, fontheight);
                cumlistRect = new Rect(wombRectWidth + 4f, fontheight, 150f, mainRect.yMax - fontheight + preginfoheight + 3);
            }
            else
            {
                cumlistTitle = new Rect(pawnRectWidth, 0, 150f, fontheight);
                cumlistRect = new Rect(pawnRectWidth, fontheight, 150f, mainRect.yMax - wombRectHeight - fontheight);
            }

            Rect infoRect = new Rect(0, pawnRectHeight + 2*fontheight, pawnRectWidth, pawnRect.yMax + 2*fontheight - wombInfoRect.y);
            DrawInfos(infoRect);



            GUI.Label(cumlistTitle, Translations.Dialog_WombInfo04);
            DrawCumlist(cumlistRect);





        }




        private void DrawCumlist(Rect rect)
        {
            Listing_Standard cumlist = new Listing_Standard
            {
                maxOneColumn = true,
                ColumnWidth = wombRectWidth - pawnRectWidth
            };
            cumlist.Begin(rect);
            Listing_Standard cumlistsection = cumlist.BeginSection(rect.height - fontheight - 12f);
            foreach (string s in comp.GetCumsInfo)
            {
                cumlistsection.Label(s);
            }
            cumlist.EndSection(cumlistsection);
            cumlist.End();
        }

        private void DrawWomb(Rect rect)
        {
            GUI.color = new Color(1.00f, 0.47f, 0.47f, 1);
            GUI.Box(rect, "", boxstyle);
            //GUI.color = Color.white;
            //Widgets.DrawTextureFitted(wombRect, womb,1.0f);
            //GUI.color = cumcolor;
            //Widgets.DrawTextureFitted(wombRect, cum,1.0f);
            GUI.DrawTexture(rect, womb, ScaleMode.ScaleToFit, true, 0, Color.white, 0, 0);
            GUI.DrawTexture(rect, cum, ScaleMode.ScaleToFit, true, 0, cumcolor, 0, 0);

            GUI.color = Color.white;


        }



        private void DrawVagina(Rect rect)
        {
            Rect genitalIconRect = new Rect(rect.x, rect.y + fontheight, genitalRectWidth, genitalRectHeight);
            Rect genitalVaginaLabelRect = new Rect(rect.x, rect.y + 10f, genitalRectWidth, fontheight);
            Rect genitalAnusLabelRect = new Rect(rect.x, rect.y + fontheight + genitalRectHeight, genitalRectWidth, fontheight);

            vagina = pawn.GetGenitalIcon();
            anal = pawn.GetAnalIcon();
            GUI.color = new Color(1.00f, 0.47f, 0.47f, 1);
            GUI.Box(rect, "", boxstyle);
            GUI.color = pawn.story.SkinColor;
            //Widgets.DrawTextureFitted(genitalIconRect, anal, 1.0f);
            //Widgets.DrawTextureFitted(genitalIconRect, vagina, 1.0f);
            GUI.DrawTexture(genitalIconRect, anal, ScaleMode.ScaleToFit);
            GUI.DrawTexture(genitalIconRect, vagina, ScaleMode.ScaleToFit);

            GUI.color = Color.white;
            GUI.Label(genitalVaginaLabelRect, pawn.GetVaginaLabel(), fontstylecenter);
            GUI.Label(genitalAnusLabelRect, pawn.GetAnusLabel(), fontstylecenter);
        }

        private void DrawBreast(Rect rect)
        {
            Rect BreastIconRect = new Rect(rect.x, rect.y + fontheight, breastRectWidth, breastRectHeight);
            Rect BreastLabelRect = new Rect(rect.x, rect.y, breastRectWidth, fontheight);
            Rect MilkGaugeRect = new Rect(rect.x, rect.yMax, breastRectWidth, 20f);

            GUI.color = new Color(1.00f, 0.47f, 0.47f, 1);
            GUI.Box(rect, "", boxstyle);

            pawn.DrawBreastIcon(BreastIconRect);


            GUI.color = Color.white;
            GUI.Label(BreastLabelRect, pawn.GetBreastLabel(), fontstylecenter);

            pawn.DrawMilkBars(MilkGaugeRect);


        }

        private void DrawInfos(Rect rect)
        {
            Rect lineRect = new Rect(rect.x, rect.y, rect.width, 20f);
            float statvalue;
            const float height = 24f;
            statvalue = pawn.GetStatValue(xxx.sex_drive_stat);
            FillableBarLabeled(lineRect, "  " + xxx.sex_drive_stat.LabelCap.CapitalizeFirst() + " " + statvalue.ToStringPercent(), statvalue/2 ,TextureCache.slaaneshTexture,Texture2D.blackTexture, xxx.sex_drive_stat.description);
            lineRect.y += height;

            statvalue = pawn.GetStatValue(xxx.vulnerability_stat);
            FillableBarLabeled(lineRect, "  " + xxx.vulnerability_stat.LabelCap.CapitalizeFirst() + " " + statvalue.ToStringPercent(), statvalue/2, TextureCache.khorneTexture,Texture2D.blackTexture, xxx.vulnerability_stat.description);
            lineRect.y += height;

            statvalue = pawn.GetStatValue(xxx.sex_stat);
            FillableBarLabeled(lineRect, "  " + xxx.sex_stat.LabelCap.CapitalizeFirst() + " " + statvalue.ToStringPercent(), statvalue / 2, TextureCache.tzeentchTexture, Texture2D.blackTexture, xxx.sex_stat.description);
            lineRect.y += height;

            statvalue = pawn.records.GetValue(xxx.CountOfBirthHuman);
            FillableBarLabeled(lineRect, "  " + xxx.CountOfBirthHuman.LabelCap.CapitalizeFirst() + " " + statvalue, statvalue / 10, TextureCache.humanTexture, Texture2D.blackTexture, xxx.CountOfBirthHuman.description);
            lineRect.y += height;

            statvalue = pawn.records.GetValue(xxx.CountOfBirthAnimal);
            FillableBarLabeled(lineRect, "  " + xxx.CountOfBirthAnimal.LabelCap.CapitalizeFirst() + " " + statvalue, statvalue / 20, TextureCache.animalTexture, Texture2D.blackTexture, xxx.CountOfBirthAnimal.description);
            lineRect.y += height;

            statvalue = pawn.records.GetValue(xxx.CountOfBirthEgg);
            FillableBarLabeled(lineRect, "  " + xxx.CountOfBirthEgg.LabelCap.CapitalizeFirst() + " " + statvalue, statvalue / 100, TextureCache.nurgleTexture, Texture2D.blackTexture, xxx.CountOfBirthEgg.description);
            lineRect.y += height;

            statvalue = pawn.records.GetValue(xxx.CountOfWhore);
            if (statvalue > 0)
            {
                FillableBarLabeled(lineRect, "  " + xxx.CountOfWhore.LabelCap.CapitalizeFirst() + " " + statvalue, statvalue / 50, TextureCache.slaaneshTexture, Texture2D.blackTexture, xxx.CountOfWhore.description);
                statvalue = pawn.records.GetValue(xxx.EarnedMoneyByWhore);
                lineRect.y += height;
                FillableBarLabeled(lineRect, "  " + VariousDefOf.RJW_EarnedMoneyByWhore.label.CapitalizeFirst() + " " + statvalue, statvalue / 10000, TextureCache.ghalmarazTexture, Texture2D.blackTexture);
                
                lineRect.y += height;
            }
            else
            {
                lineRect.y += height;
                lineRect.y += height;
            }

            statvalue = Configurations.ImplantationChance * comp.Props.baseImplantationChanceFactor * comp.ImplantFactor;
            float fertchance = comp.GetFertilityChance();
            FillableBarLabeled(lineRect, "  " + xxx.reproduction.LabelCap.CapitalizeFirst() + " " + statvalue.ToStringPercent(), statvalue, TextureCache.fertilityTexture, Texture2D.blackTexture, Translations.FertilityDesc(String.Format("{0:0.##}", fertchance*100)));
            Rect overayRect = new Rect(lineRect.x, lineRect.y, lineRect.width * Math.Min(1.0f, fertchance), lineRect.height);
            GUI.DrawTexture(overayRect, TextureCache.FertChanceTex);
            lineRect.y += height;
        }

        private void FillableBarLabeled(Rect rect, string label, float fillPercent, Texture2D filltexture, Texture2D bgtexture, string tooltip = null)
        {
            Widgets.FillableBar(rect, Math.Min(fillPercent,1.0f), filltexture, bgtexture, true);
            GUI.Label(rect, label, fontstyleleft);
            Widgets.DrawHighlightIfMouseover(rect);
            if (tooltip != null) TooltipHandler.TipRegion(rect, tooltip);
        }


    }
}
