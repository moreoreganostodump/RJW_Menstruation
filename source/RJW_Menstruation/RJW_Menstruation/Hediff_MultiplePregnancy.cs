using RimWorld;
using rjw;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RJW_Menstruation
{
    public class Hediff_MultiplePregnancy : Hediff_BasePregnancy
    {
        public override void DiscoverPregnancy()
        {
            PregnancyThought();
            base.DiscoverPregnancy();
        }

        protected void PregnancyThought()
        {
            if (!is_discovered && xxx.is_human(pawn))
            {
                if (!pawn.Has(Quirk.Breeder) && pawn.relations?.DirectRelations?.Find(x => x.def.Equals(PawnRelationDefOf.Spouse) || x.def.Equals(PawnRelationDefOf.Fiance)) == null)
                {
                    if (pawn.Has(Quirk.ImpregnationFetish) || pawn.relations?.DirectRelations?.Find(x => x.def.Equals(PawnRelationDefOf.Lover)) != null)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(VariousDefOf.UnwantedPregnancyMild);
                    }
                    else
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemory(VariousDefOf.UnwantedPregnancy);
                    }
                }

            }
        }

        public override void GiveBirth()
        {

            if (babies.NullOrEmpty())
            {
                ModLog.Warning(" no babies (debug?) " + this.GetType().Name);
                if (father == null)
                {
                    father = Trytogetfather(ref pawn);
                }
                Initialize(pawn, father);
            }

            List<Pawn> siblings = new List<Pawn>();
            foreach (Pawn baby in babies)
            {
                if (xxx.is_animal(baby))
                {
                    BestialBirth(baby, siblings);
                }
                else
                {
                    HumanlikeBirth(baby, siblings);
                }

            }

            pawn.health.RemoveHediff(this);
        }

        public string GetBabyInfo()
        {
            string res = "";
            if (!babies.NullOrEmpty())
            {
                var babiesdistinct = babies.Distinct(new RaceComparer());
                int iteration = 0;
                foreach (Pawn baby in babiesdistinct)
                {
                    int num = babies.Where(x => x.def.Equals(baby.def)).Count();
                    if (iteration > 0) res += ", ";
                    res += num + " " + baby.def.label;
                    iteration++;
                }
                res += " " + Translations.Dialog_WombInfo02;
                return res;
            }
            return "Null";
        }

        public string GetFatherInfo()
        {
            string res = Translations.Dialog_WombInfo03 + ": ";
            if (!babies.NullOrEmpty())
            {
                var babiesdistinct = babies.Distinct(new FatherComparer(pawn));
                int iteration = 0;
                foreach (Pawn baby in babiesdistinct)
                {
                    if (iteration > 0) res += ", ";
                    res += Utility.GetFather(baby, pawn)?.LabelShort ?? "Unknown";
                    iteration++;
                }
                return res;
            }
            return "Null";
        }




        private void HumanlikeBirth(Pawn baby, List<Pawn> siblings)
        {
            Pawn mother = pawn; Pawn father = Utility.GetFather(baby, pawn);
            //backup melanin, LastName for when baby reset by other mod on spawn/backstorychange
            //var skin_whiteness = baby.story.melanin;
            //var last_name = baby.story.birthLastName;

            PawnUtility.TrySpawnHatchedOrBornPawn(baby, mother);

            var sex_need = mother.needs?.TryGetNeed<Need_Sex>();
            if (mother.Faction != null && !(mother.Faction?.IsPlayer ?? false) && sex_need != null)
            {
                sex_need.CurLevel = 1.0f;
            }
            if (mother.Faction != null)
            {
                if (mother.Faction != baby.Faction)
                    baby.SetFaction(mother.Faction);
            }
            if (mother.IsPrisonerOfColony)
            {
                baby.guest.CapturedBy(Faction.OfPlayer);
            }

            foreach (Pawn sibling in siblings)
            {
                baby.relations.AddDirectRelation(PawnRelationDefOf.Sibling, sibling);
            }
            siblings.Add(baby);
            
            PostBirth(mother, father, baby);
        }


        private void BestialBirth(Pawn baby, List<Pawn> siblings)
        {
            Pawn mother = pawn; Pawn father = Utility.GetFather(baby, pawn);
            //backup melanin, LastName for when baby reset by other mod on spawn/backstorychange
            //var skin_whiteness = baby.story.melanin;
            //var last_name = baby.story.birthLastName;

            PawnUtility.TrySpawnHatchedOrBornPawn(baby, mother);

            Need_Sex sex_need = mother.needs?.TryGetNeed<Need_Sex>();
            if (mother.Faction != null && !(mother.Faction?.IsPlayer ?? false) && sex_need != null)
            {
                sex_need.CurLevel = 1.0f;
            }
            if (mother.Faction != null)
            {
                if (mother.Faction != baby.Faction)
                    baby.SetFaction(mother.Faction);
            }


            foreach (Pawn sibling in siblings)
            {
                baby.relations.AddDirectRelation(PawnRelationDefOf.Sibling, sibling);
            }
            siblings.Add(baby);
            train(baby, mother, father);

            PostBirth(mother, father, baby);

            //restore melanin, LastName for when baby reset by other mod on spawn/backstorychange
            //baby.story.melanin = skin_whiteness;
            //baby.story.birthLastName = last_name;
        }

        protected override void GenerateBabies()
        {
            AddNewBaby(pawn, father);
        }



        protected void train(Pawn baby, Pawn mother, Pawn father)
        {
            bool _;
            if (!xxx.is_human(baby) && baby.Faction == Faction.OfPlayer)
            {
                if (xxx.is_human(mother) && baby.Faction == Faction.OfPlayer && baby.training.CanAssignToTrain(TrainableDefOf.Obedience, out _).Accepted)
                {
                    baby.training.Train(TrainableDefOf.Obedience, mother);
                }
                if (xxx.is_human(mother) && baby.Faction == Faction.OfPlayer && baby.training.CanAssignToTrain(TrainableDefOf.Tameness, out _).Accepted)
                {
                    baby.training.Train(TrainableDefOf.Tameness, mother);
                }
            }
        }


        public bool AddNewBaby(Pawn mother, Pawn father)
        {
            float melanin;
            string lastname;
            if (xxx.is_human(mother))
            {
                if (xxx.is_human(father))
                {
                    melanin = (mother.story.melanin + father.story.melanin) / 2;
                    lastname = NameTriple.FromString(father.Name.ToStringFull).Last;
                }
                else
                {
                    melanin = mother.story.melanin;
                    lastname = NameTriple.FromString(mother.Name.ToStringFull).Last;
                }

            }
            else
            {
                if (xxx.is_human(father))
                {
                    melanin = father.story.melanin;
                    lastname = NameTriple.FromString(father.Name.ToStringFull).Last;
                }
                else
                {
                    melanin = Rand.Range(0, 1.0f);
                    lastname = NameTriple.FromString(mother.Name.ToStringFull).Last;
                }
            }



            PawnGenerationRequest request = new PawnGenerationRequest(
                newborn: true,
                allowDowned: true,
                faction: mother.IsPrisoner ? null : mother.Faction,
                canGeneratePawnRelations: false,
                forceGenerateNewPawn: true,
                colonistRelationChanceFactor: 0,
                allowFood: false,
                allowAddictions: false,
                relationWithExtraPawnChanceFactor: 0,
                fixedMelanin: melanin,
                fixedLastName: lastname,
                kind: BabyPawnKindDecider(mother, father)
                );

            int division = 1;
            HairDef firsthair = null;
            Color firsthaircolor = Color.white;
            BodyTypeDef firstbody = null;
            CrownType firstcrown = CrownType.Undefined;
            string firstheadpath = null;
            string firstHARcrown = null;
            while (Rand.Chance(Configurations.EnzygoticTwinsChance) && division < Configurations.MaxEnzygoticTwins) division++;
            for (int i = 0; i < division; i++)
            {
                Pawn baby = GenerateBaby(request, mother, father);
                if (division > 1)
                {
                    if (i == 0 && baby.story != null)
                    {
                        firsthair = baby.story.hairDef;
                        firsthaircolor = baby.story.hairColor;
                        request.FixedGender = baby.gender;
                        firstbody = baby.story.bodyType;
                        firstcrown = baby.story.crownType;
                        firstheadpath = (string)baby.story.GetMemberValue("headGraphicPath");
                        if (firstheadpath == null)
                        {
                            baby.story.SetMemberValue("headGraphicPath", GraphicDatabaseHeadRecords.GetHeadRandom(baby.gender, baby.story.SkinColor, baby.story.crownType).GraphicPath);
                            firstheadpath = (string)baby.story.GetMemberValue("headGraphicPath");
                        }
                        if (Configurations.HARActivated && baby.IsHAR())
                        {
                            firstHARcrown = baby.GetHARCrown();
                        }

                    }
                    else
                    {
                        if (baby.story != null)
                        {
                            baby.story.hairDef = firsthair;
                            baby.story.hairColor = firsthaircolor;
                            baby.story.bodyType = firstbody;
                            baby.story.crownType = firstcrown;
                            baby.story.SetMemberValue("headGraphicPath", firstheadpath);

                            if (Configurations.HARActivated && baby.IsHAR())
                            {
                                baby.SetHARCrown(firstHARcrown);
                            }
                        }
                    }
                }

                if (baby != null) babies.Add(baby);
            }




            return true;

        }

        public Pawn GenerateBaby(PawnGenerationRequest request, Pawn mother, Pawn father)
        {
            
            Pawn baby = PawnGenerator.GeneratePawn(request);
            if (baby != null)
            {
                if (xxx.is_human(baby))
                {
                    List<Trait> traitpool = new List<Trait>();
                    baby.SetMother(mother);
                    if (mother != father)
                    {
                        if (father.gender != Gender.Female) baby.SetFather(father);
                        else
                        {
                            baby.relations.AddDirectRelation(PawnRelationDefOf.Parent, father);
                        }
                    }

                    if (xxx.has_traits(pawn) && pawn.RaceProps.Humanlike)
                    {
                        foreach (Trait momtrait in pawn.story.traits.allTraits)
                        {
                            if (!RJWPregnancySettings.trait_filtering_enabled || !non_genetic_traits.Contains(momtrait.def.defName))
                                traitpool.Add(momtrait);
                        }
                    }
                    if (father != null && xxx.has_traits(father) && father.RaceProps.Humanlike)
                    {
                        foreach (Trait poptrait in father.story.traits.allTraits)
                        {
                            if (!RJWPregnancySettings.trait_filtering_enabled || !non_genetic_traits.Contains(poptrait.def.defName))
                                traitpool.Add(poptrait);
                        }
                    }
                    updateTraits(baby, traitpool);

                }
                else if (baby.relations != null)
                {
                    baby.relations.AddDirectRelation(VariousDefOf.Relation_birthgiver, mother);
                    mother.relations.AddDirectRelation(VariousDefOf.Relation_spawn, baby);
                    if (mother != father)
                    {
                        baby.relations.AddDirectRelation(VariousDefOf.Relation_birthgiver, father);
                        father.relations.AddDirectRelation(VariousDefOf.Relation_spawn, baby);
                    }
                }
            }
            return baby;
        }

        /// <summary>
        /// Decide pawnkind from mother and father <para/>
        /// Come from RJW
        /// </summary>
        /// <param name="mother"></param>
        /// <param name="father"></param>
        /// <returns></returns>
        public PawnKindDef BabyPawnKindDecider(Pawn mother, Pawn father)
        {
            PawnKindDef spawn_kind_def = mother.kindDef;

            int flag = 0;
            if (xxx.is_human(mother)) flag += 2;
            if (xxx.is_human(father)) flag += 1;
            //Mother - Father = Flag
            //Human  - Human  =  3
            //Human  - Animal =  2
            //Animal - Human  =  1
            //Animal - Animal =  0

            switch (flag)
            {
                case 3:
                    if (!Rand.Chance(RJWPregnancySettings.humanlike_DNA_from_mother)) spawn_kind_def = father.kindDef;
                    break;
                case 2:
                    if (RJWPregnancySettings.bestiality_DNA_inheritance == 0f) spawn_kind_def = father.kindDef;
                    else if (!Rand.Chance(RJWPregnancySettings.bestial_DNA_from_mother)) spawn_kind_def = father.kindDef;
                    break;
                case 1:
                    if (RJWPregnancySettings.bestiality_DNA_inheritance == 1f) spawn_kind_def = father.kindDef;
                    else if (!Rand.Chance(RJWPregnancySettings.bestial_DNA_from_mother)) spawn_kind_def = father.kindDef;
                    break;
                case 0:
                    if (!Rand.Chance(RJWPregnancySettings.bestial_DNA_from_mother)) spawn_kind_def = father.kindDef;
                    break;
            }

            bool IsAndroidmother = AndroidsCompatibility.IsAndroid(mother);
            bool IsAndroidfather = AndroidsCompatibility.IsAndroid(father);
            if (IsAndroidmother && !IsAndroidfather)
            {
                spawn_kind_def = father.kindDef;
            }
            else if (!IsAndroidmother && IsAndroidfather)
            {
                spawn_kind_def = mother.kindDef;
            }

            string MotherRaceName = "";
            string FatherRaceName = "";
            MotherRaceName = mother.kindDef.race.defName;
            PawnKindDef tmp = spawn_kind_def;
            if (father != null)
                FatherRaceName = father.kindDef.race.defName;


            if (FatherRaceName != "" && Configurations.UseHybridExtention)
            {
                spawn_kind_def = GetHybrid(father, mother);
                //Log.Message("pawnkind: " + spawn_kind_def?.defName);
            }

            if (MotherRaceName != FatherRaceName && FatherRaceName != "")
            {
                if (!Configurations.UseHybridExtention || spawn_kind_def == null)
                {
                    spawn_kind_def = tmp;
                    var groups = DefDatabase<RaceGroupDef>.AllDefs.Where(x => !(x.hybridRaceParents.NullOrEmpty() || x.hybridChildKindDef.NullOrEmpty()));


                    //ModLog.Message(" found custom RaceGroupDefs " + groups.Count());
                    foreach (var t in groups)
                    {
                        if ((t.hybridRaceParents.Contains(MotherRaceName) && t.hybridRaceParents.Contains(FatherRaceName))
                            || (t.hybridRaceParents.Contains("Any") && (t.hybridRaceParents.Contains(MotherRaceName) || t.hybridRaceParents.Contains(FatherRaceName))))
                        {
                            //ModLog.Message(" has hybridRaceParents");
                            if (t.hybridChildKindDef.Contains("MotherKindDef"))
                                spawn_kind_def = mother.kindDef;
                            else if (t.hybridChildKindDef.Contains("FatherKindDef") && father != null)
                                spawn_kind_def = father.kindDef;
                            else
                            {
                                //ModLog.Message(" trying hybridChildKindDef " + t.defName);
                                var child_kind_def_list = new List<PawnKindDef>();
                                child_kind_def_list.AddRange(DefDatabase<PawnKindDef>.AllDefs.Where(x => t.hybridChildKindDef.Contains(x.defName)));

                                //ModLog.Message(" found custom hybridChildKindDefs " + t.hybridChildKindDef.Count);
                                if (!child_kind_def_list.NullOrEmpty())
                                    spawn_kind_def = child_kind_def_list.RandomElement();
                            }
                        }
                    }
                }

            }
            else if (!Configurations.UseHybridExtention || spawn_kind_def == null)
            {
                spawn_kind_def = mother.RaceProps.AnyPawnKind;
            }

            if (spawn_kind_def.defName.Contains("Nymph"))
            {
                //child is nymph, try to find other PawnKindDef
                var spawn_kind_def_list = new List<PawnKindDef>();
                spawn_kind_def_list.AddRange(DefDatabase<PawnKindDef>.AllDefs.Where(x => x.race == spawn_kind_def.race && !x.defName.Contains("Nymph")));
                //no other PawnKindDef found try mother
                if (spawn_kind_def_list.NullOrEmpty())
                    spawn_kind_def_list.AddRange(DefDatabase<PawnKindDef>.AllDefs.Where(x => x.race == mother.kindDef.race && !x.defName.Contains("Nymph")));
                //no other PawnKindDef found try father
                if (spawn_kind_def_list.NullOrEmpty() && father != null)
                    spawn_kind_def_list.AddRange(DefDatabase<PawnKindDef>.AllDefs.Where(x => x.race == father.kindDef.race && !x.defName.Contains("Nymph")));
                //no other PawnKindDef found fallback to generic colonist
                if (spawn_kind_def_list.NullOrEmpty())
                    spawn_kind_def = PawnKindDefOf.Colonist;

                spawn_kind_def = spawn_kind_def_list.RandomElement();
            }




            return spawn_kind_def;

        }

        public PawnKindDef GetHybrid(Pawn first, Pawn second)
        {
            PawnKindDef res = null;
            Pawn opposite = second;
            HybridInformations info = Configurations.HybridOverride.FirstOrDefault(x => x.defName == first.def?.defName && (x.hybridExtension?.Exists(y => y.defName == second.def?.defName) ?? false));
            if (info == null)
            {
                info = Configurations.HybridOverride.FirstOrDefault(x => x.defName == second.def?.defName && (x.hybridExtension?.Exists(y => y.defName == first.def?.defName) ?? false));
                opposite = first;
            }

            if (info != null)
            {
                res = info.GetHybridWith(opposite.def.defName) ?? null;
            }
            if (res != null) return res;


            PawnDNAModExtension dna;
            dna = first.def.GetModExtension<PawnDNAModExtension>();
            if (dna != null)
            {
                res = dna.GetHybridWith(second.def.defName) ?? null;
            }
            else
            {
                dna = second.def.GetModExtension<PawnDNAModExtension>();
                if (dna != null)
                {
                    res = dna.GetHybridWith(first.def.defName) ?? null;
                }
            }
            return res;
        }

        /// <summary>
        /// Copy from RJW
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="parenttraits"></param>
        public void updateTraits(Pawn pawn, List<Trait> parenttraits)
        {
            if (pawn?.story?.traits == null) return;

            List<Trait> traitlist = new List<Trait>(pawn.story.traits.allTraits);
            if (!parenttraits.NullOrEmpty()) traitlist.AddRange(parenttraits);
            else return;


            var forcedTraits = traitlist
                .Where(x => x.ScenForced)
                .Distinct(new TraitComparer(ignoreDegree: true));

            List<Trait> res = new List<Trait>();
            res.AddRange(forcedTraits);

            var comparer = new TraitComparer(); // trait comparision implementation, because without game compares traits *by reference*, makeing them all unique.

            while (res.Count < traitlist.Count && traitlist.Count > 0)
            {
                int index = Rand.Range(0, traitlist.Count); // getting trait and removing from the pull
                var trait = traitlist[index];
                traitlist.RemoveAt(index);

                if (!res.Any(x => comparer.Equals(x, trait) ||  // skipping traits conflicting with already added
                                             x.def.ConflictsWith(trait)))
                {
                    res.Add(new Trait(trait.def, trait.Degree, false));
                }
            }


            pawn.story.traits.allTraits = res;
        }



    }

    /// <summary>
    /// Copy from RJW
    /// </summary>
    public class TraitComparer : IEqualityComparer<Trait>
    {
        bool ignoreForced;
        bool ignoreDegree;

        public TraitComparer(bool ignoreDegree = false, bool ignoreForced = true)
        {
            this.ignoreDegree = ignoreDegree;
            this.ignoreForced = ignoreForced;
        }

        public bool Equals(Trait x, Trait y)
        {
            return
                x.def == y.def &&
                (ignoreDegree || (x.Degree == y.Degree)) &&
                (ignoreForced || (x.ScenForced == y.ScenForced));
        }

        public int GetHashCode(Trait obj)
        {
            return
                (obj.def.GetHashCode() << 5) +
                (ignoreDegree ? 0 : obj.Degree) +
                ((ignoreForced || obj.ScenForced) ? 0 : 0x10);
        }
    }

    public class RaceComparer : IEqualityComparer<Pawn>
    {
        public bool Equals(Pawn x, Pawn y)
        {
            return x.def.Equals(y.def);
        }

        public int GetHashCode(Pawn obj)
        {
            return obj.def.GetHashCode();
        }
    }

    public class FatherComparer : IEqualityComparer<Pawn>
    {
        Pawn mother;

        public FatherComparer(Pawn mother)
        {
            this.mother = mother;
        }

        public bool Equals(Pawn x, Pawn y)
        {
            if (Utility.GetFather(x, mother) == null && Utility.GetFather(y, mother) == null) return true;
            return Utility.GetFather(x, mother)?.Label.Equals(Utility.GetFather(y, mother)?.Label) ?? false;
        }

        public int GetHashCode(Pawn obj)
        {
            return obj.def.GetHashCode();
        }
    }

}
