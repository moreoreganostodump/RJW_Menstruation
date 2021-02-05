using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;


namespace RJW_Menstruation
{
	public class DNADef : Def
	{
		public bool IsNone => string.IsNullOrEmpty(defName);
		public static readonly DNADef None = new DNADef();

		public string fetusTexPath;
		public ColorInt cumColor;
		public Color CumColor => cumColor.ToColor;
		public float cumThickness = 0f;

	}



}
