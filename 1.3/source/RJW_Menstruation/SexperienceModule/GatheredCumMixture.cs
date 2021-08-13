using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RJWSexperience;
using RJW_Menstruation;
using UnityEngine;

namespace RJW_Menstruation.Sexperience
{
    public class GatheredCumMixture : ThingWithComps
    {
        public Color cumColor;
        public List<string> ingredients = new List<string>();

        public override Color DrawColor => cumColor;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cumColor, "cumColor", Color.white, true);
            Scribe_Collections.Look(ref ingredients, "ingredients");
        }

        public override bool TryAbsorbStack(Thing other, bool respectStackLimit)
        {
            float amount = stackCount;
            float count = ThingUtility.TryAbsorbStackNumToTake(this, other, respectStackLimit);
            bool res = base.TryAbsorbStack(other, respectStackLimit);
            if (res && other is GatheredCumMixture)
            {
                GatheredCumMixture othercum = (GatheredCumMixture)other;
                cumColor = Colors.CMYKLerp(cumColor,othercum.cumColor,count/(amount+count));
                if (!othercum.ingredients.NullOrEmpty()) for (int i=0; i<othercum.ingredients.Count; i++)
                    {
                        if (!ingredients.Contains(othercum.ingredients[i])) ingredients.Add(othercum.ingredients[i]);
                    }
            }
            return res;
        }

        public override string GetInspectString()
        {
            string res = "";
            if (!ingredients.NullOrEmpty()) for(int i=0; i<ingredients.Count; i++)
                {
                    res += ingredients[i];
                    if (i != ingredients.Count - 1) res += ", ";
                }
            return res;
        }

        public void InitwithCum(CumMixture cum)
        {
            ingredients.AddRange(cum.Getingredients);
            cumColor = cum.color;
        }

    }
}
