using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;
using rjw;

namespace RJW_Menstruation
{
    public class Dialog_WombStatus : Window
    {
        private Pawn pawn;
		private HediffComp_Menstruation comp;
		private const float windowMargin = 20f;
		private const float pawnRectWidth = 150f;
		private const float pawnRectHeight = 150f;
		private const float wombRectHeight = 270f;
		private const float wombRectWidth = 300f;
		private const float fontheight = 30;
		private const float genitalRectWidth = 102;
		private const float genitalRectHeight = 140;


		private Texture2D womb;
		private Texture2D cum;
		private Texture2D vagina;
		private Texture2D anal;
		private Color cumcolor;

        public override Vector2 InitialSize
        {
            get
            {
				return new Vector2(300f + 2*windowMargin,800f);
            }
        }

        public Dialog_WombStatus(Pawn pawn, HediffComp_Menstruation comp, Texture2D icon)
        {
            this.pawn = pawn;
			this.comp = comp;
			womb = icon;
        }

        public override void DoWindowContents(Rect inRect)
        {
			bool flag = false;
			soundClose = SoundDefOf.InfoCard_Close;
			//closeOnClickedOutside = true;
			absorbInputAroundWindow = false;
			forcePause = false;
			preventCameraMotion = false;
			draggable = true;
			//resizeable = true;

			if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
			{
				flag = true;
				Event.current.Use();
			}

			Rect windowRect = inRect.ContractedBy(windowMargin);
			Rect mainRect = new Rect(windowRect.x, windowRect.y, windowRect.width, windowRect.height - 20f);
			Rect closeRect = new Rect(windowRect.xMax, 0f, 20f, 20f);
			MainContents(mainRect);
			if (Widgets.CloseButtonFor(closeRect))
            {
				Close();
            }
		}

		private void MainContents(Rect mainRect)
        {
			GUIStyle fontstylecenter = new GUIStyle() { alignment = TextAnchor.MiddleCenter };
			GUIStyle fontstyleright = new GUIStyle() { alignment = TextAnchor.MiddleRight };
			GUIStyle fontstyleleft = new GUIStyle() { alignment = TextAnchor.MiddleLeft };
			GUIStyle boxstyle = new GUIStyle(GUI.skin.textArea);
			GUIStyle buttonstyle = new GUIStyle(GUI.skin.button);
			boxstyle.hover = boxstyle.normal;
			boxstyle.onHover = boxstyle.normal;
			boxstyle.onNormal = boxstyle.normal;

			buttonstyle.onHover = buttonstyle.onNormal;
			buttonstyle.hover = buttonstyle.normal;
			boxstyle.border.left = 4; boxstyle.border.right = 4; boxstyle.border.bottom = 4; boxstyle.border.top = 4;

			float preginfoheight = 0f;
			Hediff hediff = PregnancyHelper.GetPregnancy(pawn);
			if (pawn.IsPregnant())
			{
				womb = Utility.GetPregnancyIcon(comp, hediff);
				if (hediff is Hediff_BasePregnancy)
				{
					Hediff_BasePregnancy h = (Hediff_BasePregnancy)hediff;
					if (h.GestationProgress < 0.2f) cum = Utility.GetCumIcon(comp);
					else cum = ContentFinder<Texture2D>.Get(("Womb/Empty"), true);
					Pawn fetus = Utility.GetFetus(pawn);
					preginfoheight = fontheight;
					Rect preginfo = new Rect(0f, mainRect.yMax - wombRectHeight - 2, wombRectWidth, preginfoheight);
					if (fetus != null)
					{
						fontstyleright.normal.textColor = Color.white;
						buttonstyle.alignment = TextAnchor.MiddleLeft;
						GUI.Box(preginfo, h.babies.Count + " " + fetus.def.label + " " + Translations.Dialog_WombInfo02, buttonstyle);
						GUI.Label(preginfo, Translations.Dialog_WombInfo03 + ": " + h.father.LabelShort + "  ", fontstyleright);
					}

				}
				else cum = ContentFinder<Texture2D>.Get(("Womb/Empty"), true);
			}
			else
			{
				womb = Utility.GetWombIcon(comp);
				cum = Utility.GetCumIcon(comp);
			}


			Rect pawnRect = new Rect(0, 0, pawnRectWidth, pawnRectHeight);
			Widgets.DrawTextureFitted(pawnRect,PortraitsCache.Get(pawn, pawnRect.size),1.0f);
			Rect pawnLabelRect = new Rect(0, pawnRectHeight, pawnRectWidth, fontheight-10);
			Rect pawnLabel2Rect = new Rect(0, pawnRectHeight+fontheight-10, pawnRectWidth, fontheight-10);
			fontstylecenter.normal.textColor = pawn.DrawColor;
			GUI.Label(pawnLabelRect, pawn.Name.ToStringFull, fontstylecenter);
			GUI.Label(pawnLabel2Rect, pawn.story.Title, fontstylecenter);
			GUI.color = Color.white;


			Rect wombInfoRect = new Rect(0f, mainRect.yMax - wombRectHeight - fontheight - 2, wombRectWidth, fontheight);

			buttonstyle.normal.textColor = Color.white;
			//boxstyle.normal.background = Texture2D.whiteTexture;
			buttonstyle.alignment = TextAnchor.MiddleLeft;
			GUI.backgroundColor = new Color(0.24f, 0.29f, 0.35f, 1);
			GUI.Box(wombInfoRect, Translations.Dialog_WombInfo01 + ": " + comp.GetCurStageLabel,buttonstyle);
			GUI.color = Color.white;


			fontstyleright.normal.textColor = Color.red;
			if (comp.GetFertilization) GUI.Label(wombInfoRect, Translations.Dialog_WombInfo05 + "  ", fontstyleright);
			else if (comp.GetEggFertilizing) GUI.Label(wombInfoRect, Translations.Dialog_WombInfo06 + "  ", fontstyleright);
			else if (comp.GetEgg) GUI.Label(wombInfoRect, Translations.Dialog_WombInfo07 + "  ", fontstyleright);
			
			//Widgets.Label(wombInfoRect,Translations.Dialog_WombInfo01 + ": " + comp.GetCurStageLabel);


			cumcolor = comp.GetCumMixtureColor;
			Rect wombRect = new Rect(0f, mainRect.yMax - wombRectHeight + preginfoheight, wombRectWidth, wombRectHeight);
			GUI.color = new Color(1.00f,0.47f,0.47f,1);
			GUI.Box(wombRect,"",boxstyle);
			//GUI.color = Color.white;
			//Widgets.DrawTextureFitted(wombRect, womb,1.0f);
			//GUI.color = cumcolor;
			//Widgets.DrawTextureFitted(wombRect, cum,1.0f);
			GUI.DrawTexture(wombRect, womb, ScaleMode.ScaleToFit, true, 0, Color.white, 0, 0);
			GUI.DrawTexture(wombRect, cum, ScaleMode.ScaleToFit, true, 0, cumcolor,0,0);
			GUI.color = Color.white;

			Rect cumlistTitle = new Rect(pawnRectWidth, 0, wombRectWidth - pawnRectWidth, fontheight);
			GUI.Label(cumlistTitle,Translations.Dialog_WombInfo04);

			Rect cumlistRect = new Rect(pawnRectWidth, fontheight, wombRectWidth - pawnRectWidth, mainRect.yMax - wombRectHeight - fontheight);
			Listing_Standard cumlist = new Listing_Standard
			{
				maxOneColumn = true,
				ColumnWidth = wombRectWidth - pawnRectWidth
			};
			cumlist.Begin(cumlistRect);
			Listing_Standard cumlistsection = cumlist.BeginSection_NewTemp(mainRect.yMax - wombRectHeight - 2 * fontheight - 12f);
			foreach(string s in comp.GetCumsInfo)
            {
				cumlistsection.Label(s);
            }
			cumlist.EndSection(cumlistsection);
			cumlist.End();

			Rect genitalRect = new Rect(24, pawnRectHeight + 2*fontheight, genitalRectWidth, genitalRectHeight + fontheight*2);
			Rect genitalIconRect = new Rect(genitalRect.x,genitalRect.y + fontheight ,genitalRectWidth,genitalRectHeight);
			Rect genitalVaginaLabelRect = new Rect(genitalRect.x,genitalRect.y,genitalRectWidth,fontheight);
			Rect genitalAnusLabelRect = new Rect(genitalRect.x,genitalRect.y + fontheight +genitalRectHeight ,genitalRectWidth,fontheight);

			vagina = Utility.GetGenitalIcon(pawn);
			anal = Utility.GetAnalIcon(pawn);
			GUI.color = new Color(1.00f, 0.47f, 0.47f, 1);
			GUI.Box(genitalRect, "", boxstyle);
			GUI.color = pawn.story.SkinColor;
			//Widgets.DrawTextureFitted(genitalIconRect, anal, 1.0f);
			//Widgets.DrawTextureFitted(genitalIconRect, vagina, 1.0f);
			GUI.DrawTexture(genitalIconRect, anal, ScaleMode.ScaleToFit);
			GUI.DrawTexture(genitalIconRect, vagina, ScaleMode.ScaleToFit);

			GUI.color = Color.white;
			GUI.Label(genitalVaginaLabelRect, Utility.GetVaginaLabel(pawn),fontstylecenter);
			GUI.Label(genitalAnusLabelRect, Utility.GetAnusLabel(pawn),fontstylecenter);

		}




    }
}
