using RimWorld;
using rjw;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Threading;
using System.Threading.Tasks;


namespace RJW_Menstruation
{
    public static class Colors
    {
        public static Color blood = new Color(0.78f, 0, 0);
        //public static Color nippleblack = new Color(0.215f, 0.078f, 0); // 81,20,0
        public static ColorInt white = new ColorInt(255,255,255,255);



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
        public static System.Random random = new System.Random(Environment.TickCount);




        public static float GetCumVolume(this Pawn pawn)
        {
            List<Hediff> hediffs = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_genitalsBPR(pawn));
            if (hediffs.NullOrEmpty()) return 0;
            else return pawn.GetCumVolume(hediffs);
        }

        public static float GetCumVolume(this Pawn pawn, List<Hediff> hediffs)
        {
            CompHediffBodyPart part = hediffs?.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("penis")).InRandomOrder().FirstOrDefault()?.TryGetComp<CompHediffBodyPart>();
            if (part == null) part = hediffs?.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("ovipositorf")).InRandomOrder().FirstOrDefault()?.TryGetComp<CompHediffBodyPart>();
            if (part == null) part = hediffs?.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("ovipositorm")).InRandomOrder().FirstOrDefault()?.TryGetComp<CompHediffBodyPart>();
            if (part == null) part = hediffs?.FindAll((Hediff hed) => hed.def.defName.ToLower().Contains("tentacle")).InRandomOrder().FirstOrDefault()?.TryGetComp<CompHediffBodyPart>();


            return pawn.GetCumVolume(part);
        }


        public static float GetCumVolume(this Pawn pawn, CompHediffBodyPart part)
        {
            float res;

            try
            {
                res = part.FluidAmmount * part.FluidModifier * pawn.BodySize / pawn.RaceProps.baseBodySize * Rand.Range(0.8f, 1.2f) * RJWSettings.cum_on_body_amount_adjust * 0.3f;
            }
            catch (NullReferenceException)
            {
                res = 0.0f;
            }
            if (pawn.Has(Quirk.Messy)) res *= Rand.Range(4.0f, 8.0f);

            return res;
        }



        public static HediffComp_Breast GetBreastComp(this Pawn pawn)
        {
            var hedifflist = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_breastsBPR(pawn))?.FindAll((Hediff h) => h is Hediff_PartBaseNatural || h is Hediff_PartBaseArtifical);
            HediffComp_Breast result;
            if (hedifflist.NullOrEmpty()) return null;
            else
            {
                foreach(Hediff h in hedifflist)
                {
                    result = h.TryGetComp<HediffComp_Breast>();
                    if (result != null) return result;
                }
            }
            return null;
        }

        public static HediffComp_Breast GetBreastComp(this Hediff hediff)
        {
            if (hediff is Hediff_PartBaseNatural)
            {
                return hediff.TryGetComp<HediffComp_Breast>();
            }
            return null;
        }

        public static List<ThingComp> GetMilkComps(this Pawn pawn)
        {
            List<ThingComp> milkcomp = pawn.AllComps.FindAll(x => x is CompMilkable || x.GetType().ToString().ToLower().Contains("milkable"));
            return milkcomp;
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
                    Log.Error("Baby not exist: baby was not created or removed. Remove pregnancy.");
                    pawn.health.RemoveHediff(hediff);
                    return null;
                }
            }


            return null;
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

        public static void DrawBreastIcon(this Pawn pawn, Rect rect , bool drawOrigin = false)
        {
            var hediff = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_breastsBPR(pawn)).FirstOrDefault((Hediff h) => h.def.defName.ToLower().Contains("breast"));
            Texture2D breast, nipple, areola;
            if (hediff != null)
            {
                HediffComp_Breast comp = hediff.TryGetComp<HediffComp_Breast>();
                string icon;
                if (comp != null) icon = comp.Props.BreastTex ?? "Breasts/Breast_Breast";
                else
                {
                    breast = ContentFinder<Texture2D>.Get("Breasts/Breast_Breast00", false);
                    nipple = ContentFinder<Texture2D>.Get("Breasts/Breast_Breast00_Nipple00", false);
                    areola = ContentFinder<Texture2D>.Get("Breasts/Breast_Breast00_Areola00", false);

                    GUI.color = pawn.story.SkinColor;
                    GUI.DrawTexture(rect, breast, ScaleMode.ScaleToFit);
                    GUI.color = Color.white;
                    GUI.DrawTexture(rect, areola, ScaleMode.ScaleToFit);
                    GUI.DrawTexture(rect, nipple, ScaleMode.ScaleToFit);
                    return;
                }

                if (hediff.Severity < 0.20f) icon += "_Breast00";
                else if (hediff.Severity < 0.40f) icon += "_Breast01";
                else if (hediff.Severity < 0.60f) icon += "_Breast02";
                else if (hediff.Severity < 0.80f) icon += "_Breast03";
                else if (hediff.Severity < 1.00f) icon += "_Breast04";
                else icon += "_Breast05";

                string nippleicon, areolaicon;
                float nipplesize, areolasize;
                if (drawOrigin)
                {
                    nipplesize = comp.OriginNipple;
                    areolasize = comp.OriginAreola;
                }
                else
                {
                    nipplesize = comp.NippleSize;
                    areolasize = comp.AreolaSize;
                }

                nippleicon = icon + "_Nipple0" + GetNippleIndex(nipplesize);
                areolaicon = icon + "_Areola0" + GetAreolaIndex(areolasize);
                

                breast = ContentFinder<Texture2D>.Get(icon, false);
                areola = ContentFinder<Texture2D>.Get(areolaicon, false);
                nipple = ContentFinder<Texture2D>.Get(nippleicon, false);
                GUI.color = pawn.story.SkinColor;
                GUI.DrawTexture(rect, breast, ScaleMode.ScaleToFit);

                if (drawOrigin)
                {
                    GUI.color = comp.OriginColor;
                }
                else
                {
                    GUI.color = comp.NippleColor;
                }
                GUI.DrawTexture(rect, areola, ScaleMode.ScaleToFit);

                GUI.DrawTexture(rect, nipple, ScaleMode.ScaleToFit);


                if (Configurations.Debug) TooltipHandler.TipRegion(rect, comp.DebugInfo());

            }
            else
            {
                breast = ContentFinder<Texture2D>.Get("Breasts/Breast_Breast00", false);
                nipple = ContentFinder<Texture2D>.Get("Breasts/Breast_Breast00_Nipple00", false);
                areola = ContentFinder<Texture2D>.Get("Breasts/Breast_Breast00_Areola00", false);

                GUI.color = pawn.story.SkinColor;
                GUI.DrawTexture(rect, breast, ScaleMode.ScaleToFit);
                GUI.color = Color.white;
                GUI.DrawTexture(rect, areola, ScaleMode.ScaleToFit);
                GUI.DrawTexture(rect, nipple, ScaleMode.ScaleToFit);
            }


        }

        public static int GetNippleIndex(float nipplesize)
        {
            if (nipplesize < 0.25f) return 0;
            else if (nipplesize < 0.50f) return 1;
            else if (nipplesize < 0.75f) return 2;
            else return 3;
        }

        public static int GetAreolaIndex(float nipplesize)
        {
            if (nipplesize < 0.15f) return 0;
            else if (nipplesize < 0.30f) return 1;
            else if (nipplesize < 0.45f) return 2;
            else if (nipplesize < 0.70f) return 3;
            else return 4;
        }

        public static void DrawMilkBars(this Pawn pawn, Rect rect)
        {
            //List<ThingComp> milkcomp = pawn.AllComps.FindAll(x => x is CompMilkable || x.GetType().ToString().ToLower().Contains("milkable"));
            ThingComp milkcomp = null;
            float res = 0;
            if (VariousDefOf.Hediff_Heavy_Lactating_Permanent != null)
            {
                if (pawn.health.hediffSet.HasHediff(VariousDefOf.Hediff_Heavy_Lactating_Permanent)) milkcomp = pawn.AllComps.FirstOrDefault(x => x.GetType().ToString().ToLower().Contains("hypermilkable"));
                else milkcomp = pawn.AllComps.FirstOrDefault(x => x.GetType().ToString().ToLower().Contains("milkable"));
            }
            else
            {
                milkcomp = pawn.GetComp<CompMilkable>();
            }

            if (milkcomp != null)
            {
                if (milkcomp is CompMilkable)
                {
                    bool active = (bool)milkcomp.GetPropertyValue("Active");
                    if (active)
                    {
                        CompMilkable m = (CompMilkable)milkcomp;
                        res = Math.Max(m.Fullness, res);
                        Widgets.FillableBar(rect, Math.Min(res, 1.0f), TextureCache.milkTexture, Texture2D.blackTexture, true);
                        DrawMilkBottle(rect, pawn, VariousDefOf.Job_LactateSelf, m.Fullness);
                    }
                }
                else
                {
                    bool active = (bool)milkcomp.GetPropertyValue("Active");
                    if (active)
                    {
                        float fullness = (float)milkcomp.GetMemberValue("fullness");
                        res = Math.Max(fullness, res);
                        Widgets.FillableBar(rect, Math.Min(res, 1.0f), TextureCache.milkTexture, Texture2D.blackTexture, true);
                        DrawMilkBottle(rect, pawn, VariousDefOf.Job_LactateSelf_MC, fullness);
                    }
                }
            }
        }

        public static void DrawMilkBottle(Rect rect, Pawn pawn, JobDef milkjob,float fullness)
        {
            Texture2D texture;
            Rect buttonrect = new Rect(rect.x, rect.y, rect.height, rect.height);
            if (fullness > 0.0f)
            {
                if (fullness < 0.3f) texture = ContentFinder<Texture2D>.Get("Milk/Milkbottle_Small", false);
                else if (fullness < 0.7f) texture = ContentFinder<Texture2D>.Get("Milk/Milkbottle_Medium", false);
                else texture = ContentFinder<Texture2D>.Get("Milk/Milkbottle_Large", false);
                GUIContent icon = new GUIContent(texture);
                GUIStyle style = GUIStyle.none;
                style.normal.background = texture;
                string tooltip = Translations.Button_MilkTooltip;

                TooltipHandler.TipRegion(buttonrect, tooltip);
                if (GUI.Button(buttonrect, icon, style))
                {
                    if (fullness < 0.1f 
                        || !pawn.IsColonistPlayerControlled 
                        || pawn.Downed) SoundDefOf.ClickReject.PlayOneShotOnCamera();
                    else
                    {
                        SoundDefOf.Click.PlayOneShotOnCamera();
                        pawn.jobs.TryTakeOrderedJob(new Verse.AI.Job(milkjob, pawn));
                    }
                }
                Widgets.DrawHighlightIfMouseover(buttonrect);
            }
        }


        public static string GetVaginaLabel(this Pawn pawn)
        {
            var hediff = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_genitalsBPR(pawn)).Find((Hediff h) => h.def.defName.ToLower().Contains("vagina"));
            return hediff.LabelBase.CapitalizeFirst() + "\n(" + hediff.LabelInBrackets + ")" + "\n" + xxx.CountOfSex.LabelCap.CapitalizeFirst() + ": " + pawn.records.GetAsInt(xxx.CountOfSex);
        }
        public static string GetAnusLabel(this Pawn pawn)
        {
            var hediff = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_anusBPR(pawn)).FirstOrDefault((Hediff h) => h.def.defName.ToLower().Contains("anus"));
            if (hediff != null) return hediff.LabelBase.CapitalizeFirst() + "\n(" + hediff.LabelInBrackets + ")";
            else return "";
        }
        public static string GetBreastLabel(this Pawn pawn)
        {
            var hediff = Genital_Helper.get_PartsHediffList(pawn, Genital_Helper.get_breastsBPR(pawn)).FirstOrDefault((Hediff h) => h.def.defName.ToLower().Contains("breast"));
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
                
                res = pawn.relations?.GetFirstDirectRelationPawn(PawnRelationDefOf.Parent, x => x != mother) ?? null;
                if (res == null) res = pawn.relations?.GetFirstDirectRelationPawn(VariousDefOf.Relation_birthgiver, x => x != mother) ?? null;
                return res;
            }
        }

        public static float RandGaussianLike(float min, float max, int iterations = 3)
        {
            double res = 0;
            for (int i = 0; i < iterations; i++)
            {
                res += random.NextDouble();
            }
            res = res / iterations;

            return (float)res*(max-min) + min;

        }

        public static float LerpMultiple(this float a, float b, float t, int num)
        {
            float tmult = Mathf.Pow(1 - t, num);
            return tmult * a + (1 - tmult) * b;
        }
        
        public static float VariationRange(this float num, float variant)
        {
            return num * Rand.Range(1.0f - variant, 1.0f + variant);
        }

    }
}
