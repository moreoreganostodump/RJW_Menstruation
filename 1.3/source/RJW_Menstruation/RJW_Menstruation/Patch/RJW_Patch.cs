using HarmonyLib;
using rjw;
using Verse;
using UnityEngine;

namespace RJW_Menstruation
{

    [HarmonyPatch(typeof(PregnancyHelper), "impregnate")]
    public static class impregnate_Patch
    {
        public static bool Prefix(SexProps props)
        {
            xxx.rjwSextype sextype = props.sexType;
            Pawn pawn = props.pawn;
            Pawn partner = props.partner;

            if (sextype == xxx.rjwSextype.Vaginal)
            {
                var pawnpartBPR = Genital_Helper.get_genitalsBPR(pawn);
                var maleparts = Genital_Helper.get_PartsHediffList(pawn, pawnpartBPR);
                var partnerpartBPR = Genital_Helper.get_genitalsBPR(partner);
                var femaleparts = Genital_Helper.get_PartsHediffList(partner, partnerpartBPR);

                Pawn female = null, male = null;

                if (Genital_Helper.has_vagina(partner, femaleparts))
                {
                    female = partner;
                    male = pawn;
                }
                else if (Genital_Helper.has_vagina(pawn, maleparts))
                {
                    female = pawn;
                    femaleparts = maleparts;
                    male = partner;
                    maleparts = Genital_Helper.get_PartsHediffList(partner, partnerpartBPR);
                }
                else return true;

                if (female.IsAnimal() && !Configurations.EnableAnimalCycle) return true;
                HediffComp_Menstruation comp = female.GetMenstruationComp();
                if (comp != null)
                {
                    if (Genital_Helper.has_penis_fertile(male, maleparts) && PregnancyHelper.CanImpregnate(male, female, sextype))
                    {
                        PregnancyHelper.Doimpregnate(male, female);
                        return false;
                    }
                    else if (Genital_Helper.has_ovipositorM(male, maleparts))
                    {
                        comp.CumIn(male, Rand.Range(0.5f,3.0f) * RJWSettings.cum_on_body_amount_adjust * male.BodySize, 1.0f);
                    }
                    else comp.CumIn(male, male.GetCumVolume(maleparts), 0);
                }

                //if (Genital_Helper.has_vagina(partner, partnerparts))
                //{
                //    if (partner.IsAnimal() && !Configurations.EnableAnimalCycle) return true;
                //    HediffComp_Menstruation comp = Utility.GetMenstruationComp(partner);
                //    if (comp != null)
                //    {
                //        if (Genital_Helper.has_penis_fertile(pawn, pawnparts) && PregnancyHelper.CanImpregnate(pawn, partner, sextype))
                //        {
                //            PregnancyHelper.Doimpregnate(pawn, partner);
                //            return false;
                //        }
                //        else comp.CumIn(pawn, pawn.GetCumVolume(), 0);
                //    }
                //}
                //else if (Genital_Helper.has_vagina(pawn, pawnparts))
                //{
                //    if (pawn.IsAnimal() && !Configurations.EnableAnimalCycle) return true;
                //    HediffComp_Menstruation comp = Utility.GetMenstruationComp(pawn);
                //    if (comp != null)
                //    {
                //        if (Genital_Helper.has_penis_fertile(partner, partnerparts) && PregnancyHelper.CanImpregnate(partner, pawn, sextype))
                //        {
                //            PregnancyHelper.Doimpregnate(partner, pawn);
                //            return false;
                //        }
                //        else comp.CumIn(partner, partner.GetCumVolume(), 0);
                //    }
                //}
            }
            return true;


        }
    }

    [HarmonyPatch(typeof(PregnancyHelper), "Doimpregnate")]
    public static class Doimpregnate_Patch
    {
        public static bool Prefix(Pawn pawn, Pawn partner) // partner has vagina
        {
            if (partner.IsAnimal() && !Configurations.EnableAnimalCycle) return true;
            HediffComp_Menstruation comp = partner.GetMenstruationComp();
            if (comp != null)
            {
                if (AndroidsCompatibility.IsAndroid(pawn) && !AndroidsCompatibility.AndroidPenisFertility(pawn))
                {
                    comp.CumIn(pawn, pawn.GetCumVolume(), 0);
                    return false;
                }
                else comp.CumIn(pawn, pawn.GetCumVolume(), pawn.health.capacities.GetLevel(xxx.reproduction));
                return false;
            }
            ModLog.Message("used original rjw method: Comp missing");
            return true;
        }
    }

    [HarmonyPatch(typeof(Hediff_BasePregnancy), "PostBirth")]
    public static class RJW_Patch_PostBirth
    {
        public static void Postfix(Pawn mother, Pawn father, Pawn baby)
        {
            if (Configurations.EnableBirthVaginaMorph)
            {
                Hediff vagina = mother.health.hediffSet.hediffs.FirstOrFallback(x => x.def.defName.ToLower().Contains("vagina"));
                float morph = Mathf.Max(baby.BodySize - Mathf.Pow(vagina.Severity * mother.BodySize,2), 0f);
                vagina.Severity += morph * Configurations.VaginaMorphPower;
            }
        }
    }







}
