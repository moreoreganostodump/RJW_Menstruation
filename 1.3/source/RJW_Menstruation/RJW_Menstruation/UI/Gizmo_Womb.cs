using UnityEngine;
using Verse;

namespace RJW_Menstruation
{
    public class Gizmo_Womb : Command_Action
    {
        public Texture2D icon_overay;
        public Color cumcolor;
        public HediffComp_Menstruation comp;

        protected override void DrawIcon(Rect rect, Material buttonMat, GizmoRenderParms parms)
        {
            Texture2D badTex = icon;
            Texture2D overay = icon_overay;
            Color color = cumcolor;

            if (badTex == null)
            {
                badTex = BaseContent.BadTex;
            }
            if (overay == null)
            {
                overay = BaseContent.BadTex;
            }
            if (color == null) color = Color.white;
            rect.position += new Vector2(iconOffset.x * rect.size.x, iconOffset.y * rect.size.y);
            GUI.color = IconDrawColor;
            Widgets.DrawTextureFitted(rect, badTex, this.iconDrawScale * 0.85f, this.iconProportions, this.iconTexCoords, this.iconAngle, buttonMat);
            GUI.color = color;
            Widgets.DrawTextureFitted(rect, overay, iconDrawScale * 0.85f, iconProportions, iconTexCoords, iconAngle, buttonMat);
            GUI.color = Color.white;
            if (Configurations.DrawEggOverlay) comp.DrawEggOverlay(rect);

        }
        



    }
}
