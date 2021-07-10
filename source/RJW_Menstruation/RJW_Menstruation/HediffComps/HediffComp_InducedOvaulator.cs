using HugsLib;
using RimWorld;
using rjw;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RJW_Menstruation
{
    public class CompProperties_InducedOvulator : CompProperties_Menstruation
    {
        public CompProperties_InducedOvulator()
        {
            compClass = typeof(HediffComp_InducedOvulator);
        }

    }

    public class HediffComp_InducedOvulator : HediffComp_Menstruation
    {
        protected override void FollicularAction()
        {
            if (!IsBreedingSeason())
            {
                GoNextStage(Stage.Anestrus);
                return;
            }
            if (curStageHrs >= FollicularIntervalHours)
            {
                GoNextStage(Stage.Luteal);
            }
            else
            {
                curStageHrs += Configurations.CycleAcceleration;
                if (!estrusflag && curStageHrs > FollicularIntervalHours - Props.estrusDaysBeforeOvulation * 24)
                {
                    estrusflag = true;
                    SetEstrus(Props.eggLifespanDays + Props.estrusDaysBeforeOvulation);
                }
                StayCurrentStage();
            }
        }

        protected override void AfterCumIn(Pawn cummer)
        {
            base.AfterCumIn(cummer);
            if (curStage == Stage.Follicular) curStage = Stage.Ovulatory;
        }



    }









}
