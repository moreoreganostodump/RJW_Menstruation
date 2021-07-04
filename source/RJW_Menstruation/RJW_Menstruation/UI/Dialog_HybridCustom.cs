using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using rjw;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RJW_Menstruation
{
    public class Dialog_HybridCustom : Window
    {
        private static Vector2 scroll;
        private const float windowMargin = 20f;
        static private List<FloatMenuOption> raceList = new List<FloatMenuOption>();
        static private List<HybridInformations> removeList = new List<HybridInformations>();

        public Dialog_HybridCustom()
        {
            BuildRaceList();
        }

        public void BuildRaceList()
        {
            raceList.Clear();
            if (!VariousDefOf.AllRaces.NullOrEmpty()) 
                foreach(ThingDef def in VariousDefOf.AllRaces)
                {
                    if (def.race != null)
                    {
                        if (Configurations.IsOverrideExist(def)) continue;
                        else
                        {
                            raceList.Add(new FloatMenuOption(def.label, delegate { AddHybridOverride(def); } ));
                        }
                    }
                }
            raceList.SortBy(x => x.Label);
        }

        public void AddHybridOverride(ThingDef def)
        {
            FloatMenuOption option = raceList.FirstOrDefault(x => x.Label.Equals(def?.label));
            if (option != null)
            {
                raceList.Remove(option);
                if (Configurations.HybridOverride.NullOrEmpty())
                {
                    Configurations.HybridOverride = new List<HybridInformations>();
                }
                Configurations.HybridOverride.Add(new HybridInformations(def));
                Configurations.HybridOverride.SortBy(x => x.GetDef?.label ?? "Undefined");
            }
            
        }

        public override Vector2 InitialSize
        {
            get
            {
                float width = 480f;
                float height = 640f;
                return new Vector2(width, height);
            }
        }


        public override void DoWindowContents(Rect inRect)
        {
            soundClose = SoundDefOf.CommsWindow_Close;
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

            DoMainContents(mainRect);

            if (Widgets.CloseButtonFor(closeRect))
            {
                Close();
            }
        }

        public override void PostClose()
        {
            base.PostClose();
            if (!removeList.NullOrEmpty())
            {
                foreach (HybridInformations extension in removeList)
                {
                    Configurations.HybridOverride.Remove(extension);
                }
                raceList.SortBy(x => x.Label);
            }
            removeList.Clear();
        }

        protected void DoMainContents(Rect inRect)
        {
            Rect buttonRect = new Rect(inRect.xMax - 120, 0, 100, 30);
            Rect titleRect = new Rect(inRect.x, 0, 200, 30);
            Widgets.Label(titleRect, Translations.CustomHybrid_List_Title);
            Widgets.DrawLineHorizontal(titleRect.x, titleRect.yMax + 12f, inRect.width);
            if (Widgets.ButtonText(buttonRect, "Add"))
            {
                if (!raceList.NullOrEmpty()) Find.WindowStack.Add(new FloatMenu(raceList));
            }
            if (!removeList.NullOrEmpty())
            {
                buttonRect.x -= 100;
                if (Widgets.ButtonText(buttonRect, "Undo"))
                {
                    var element = removeList.Last();
                    Configurations.HybridOverride.Add(element);
                    Configurations.HybridOverride.SortBy(x => x.GetDef?.label ?? "Undefined");
                    removeList.Remove(element);
                }

                foreach (HybridInformations extension in removeList)
                {
                    Configurations.HybridOverride.Remove(extension);
                }
                raceList.SortBy(x => x.Label);
            }


            
            Rect outRect = new Rect(inRect.x, inRect.y + 30f, inRect.width, inRect.height - 30f);
            Rect mainRect = new Rect(inRect.x, inRect.y + 30f, inRect.width - 30f, Math.Max(24f*Configurations.HybridOverride?.Count() ?? 1,10f));
            Listing_Standard listmain = new Listing_Standard();
            
            listmain.BeginScrollView(outRect, ref scroll, ref mainRect);
            listmain.Begin(mainRect);
            int i = 0;
            if (!Configurations.HybridOverride.NullOrEmpty()) 
                foreach(HybridInformations extension in Configurations.HybridOverride)
                {
                    if (extension.GetDef != null) DoRow(listmain.GetRect(24f),extension, i++);
                }
            listmain.EndScrollView(ref mainRect);
            listmain.End();


        }

        protected void DoRow(Rect rect, HybridInformations extension, int index)
        {
            Rect buttonRect = new Rect(rect.xMax - 90f, rect.y, 80f, rect.height);
            Widgets.Label(rect, extension.GetDef?.label ?? "Undefined");
            if (Widgets.ButtonText(buttonRect, "Delete"))
            {
                removeList.Add(extension);
                //raceList.Add(new FloatMenuOption(extension.GetDef.label, delegate { AddHybridOverride(extension.GetDef); }));
            }
            buttonRect.x -= 80f;
            if (Widgets.ButtonText(buttonRect, "Edit"))
            {
                Dialog_EditHybrid.OpenWindow(extension);
            }


            Widgets.DrawHighlightIfMouseover(rect);
        }


        public static void ToggleWindow()
        {
            Dialog_HybridCustom window = (Dialog_HybridCustom)Find.WindowStack.Windows.FirstOrDefault(x => x.GetType().Equals(typeof(Dialog_HybridCustom)));
            if (window != null)
            {
                Find.WindowStack.TryRemove(typeof(Dialog_HybridCustom), true);
            }
            else
            {
                SoundDefOf.CommsWindow_Open.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_HybridCustom());
            }


        }

    }

    public class Dialog_EditHybrid : Window
    {
        private const float windowMargin = 20f;
        private const float rowH = 24f;
        private HybridInformations info;
        private Vector2 scroll;
        protected List<FloatMenuOption> raceList = new List<FloatMenuOption>();
        protected List<HybridExtensionExposable> removeList = new List<HybridExtensionExposable>();
        protected float totalWeight = 0;

        public Dialog_EditHybrid(HybridInformations info)
        {
            this.info = info;
            BuildRaceList();
        }



        public override Vector2 InitialSize
        {
            get
            {
                float width = 840f;
                float height = 640f;
                return new Vector2(width, height);
            }
        }


        public override void DoWindowContents(Rect inRect)
        {
            soundClose = SoundDefOf.CommsWindow_Close;
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

            DoMainContents(mainRect);

            if (Widgets.CloseButtonFor(closeRect))
            {
                Close();
            }
        }

        public static void OpenWindow(HybridInformations info)
        {
            Dialog_EditHybrid window = (Dialog_EditHybrid)Find.WindowStack.Windows.FirstOrDefault(x => x.GetType().Equals(typeof(Dialog_EditHybrid)));
            if (window != null)
            {
                if (window.info != info)
                {
                    SoundDefOf.TabOpen.PlayOneShotOnCamera();
                    window.ChangeExtension(info);
                }
            }
            else
            {
                SoundDefOf.TabClose.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_EditHybrid(info));
            }
            

        }

        protected void ChangeExtension(HybridInformations info)
        {
            this.info = info;
            BuildRaceList();
        }

        protected void BuildRaceList()
        {
            raceList.Clear();
            if (!VariousDefOf.AllRaces.NullOrEmpty())
                foreach (ThingDef def in VariousDefOf.AllRaces)
                {
                    if (def.race != null)
                    {
                        if (info.hybridExtension.Exists(x => x.defName == def.defName)) continue;
                        else
                        {
                            raceList.Add(new FloatMenuOption(def.label, delegate { AddHybridInfo(def); }));
                        }
                    }
                }
            raceList.SortBy(x => x.Label);
        }

        protected void AddHybridInfo(ThingDef def)
        {
            FloatMenuOption option = raceList.FirstOrDefault(x => x.Label == def.label);
            if (option != null)
            {
                raceList.Remove(option);
            }
            info.hybridExtension.Add(new HybridExtensionExposable(def));
        }

        protected void DoMainContents(Rect inRect)
        {
            Rect labelRect = new Rect(inRect.xMin, inRect.yMin, 300, 24);
            Rect buttonRect = new Rect(inRect.xMax - 120, 0, 100, 30);

            Widgets.Label(labelRect, Translations.CustomHybrid_Title(info.GetDef?.label ?? "Undefined"));
            Widgets.DrawLineHorizontal(inRect.x, labelRect.yMax, inRect.width);
            if (Widgets.ButtonText(buttonRect, "Add"))
            {
                if (!raceList.NullOrEmpty()) Find.WindowStack.Add(new FloatMenu(raceList));
            }
            if (!removeList.EnumerableNullOrEmpty())
            {
                buttonRect.x -= 100;
                if (Widgets.ButtonText(buttonRect, "Undo"))
                {
                    var element = removeList.Last();
                    info.hybridExtension.Add(element);
                    removeList.Remove(element);
                }
            
                foreach (HybridExtensionExposable element in removeList)
                {
                    info.hybridExtension.Remove(element);
                }
            }


            float additionalHeight = 0f;
            if (!info.hybridExtension.NullOrEmpty()) foreach(HybridExtensionExposable e in info.hybridExtension)
                {
                    additionalHeight += e.hybridInfo?.Count() ?? 1 * rowH;
                }


            Rect outRect = new Rect(inRect.x, inRect.y + 30f, inRect.width, inRect.height - 30f);
            Rect mainRect = new Rect(inRect.x, inRect.y + 30f, inRect.width - 30f, rowH * info.hybridExtension?.Count() ?? 1 + additionalHeight);
            Listing_Standard listmain = new Listing_Standard();

            listmain.BeginScrollView(outRect, ref scroll, ref mainRect);
            listmain.Begin(mainRect);

            if (!info.hybridExtension.NullOrEmpty())
            {
                foreach (HybridExtensionExposable extension in info.hybridExtension)
                {
                    DoRow(listmain.GetRect(rowH + rowH * extension.hybridInfo?.Count() ?? 1), extension);
                }

            }


            
            listmain.EndScrollView(ref mainRect);
            listmain.End();
        }

        protected void DoRow(Rect rect, HybridExtensionExposable extension)
        {
            Rect mainRect = new Rect(rect.x, rect.y, rect.width, rowH);
            Rect subRect = new Rect(rect.x, rect.y + rowH, rect.width, rect.height - rowH);
            Rect buttonRect = new Rect(rect.xMax - 90f, rect.y, 80f, rowH);
            Widgets.Label(mainRect, extension.GetDef?.label ?? "Undefined");

            if (Widgets.ButtonText(buttonRect, "Delete"))
            {
                removeList.Add(extension);
            }
            buttonRect.x -= 80f;
            if (Widgets.ButtonText(buttonRect, "Add"))
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                if (!VariousDefOf.AllRaces.NullOrEmpty()) foreach(ThingDef def in VariousDefOf.AllRaces)
                    {
                        if (def.race != null)
                        {
                            if (extension.hybridInfo.ContainsKey(def.defName)) continue;
                            else
                            {
                                list.Add(new FloatMenuOption(def.label, delegate { extension.hybridInfo.Add(def.defName, 1.0f); }));
                            }
                        }
                    }
                if (!list.NullOrEmpty())
                {
                    list.SortBy(x => x.Label);
                    Find.WindowStack.Add(new FloatMenu(list));
                }
                    
            }
            buttonRect.x -= 80f;

            Listing_Standard sublist = new Listing_Standard();
            sublist.Begin(subRect);

            List<string> removeelements = new List<string>();
            if (!extension.hybridInfo.EnumerableNullOrEmpty())
            {
                totalWeight = 0;
                foreach(KeyValuePair<string,float> element in extension.hybridInfo)
                {
                    totalWeight += element.Value;
                }
            
                List<string> keys = new List<string>(extension.hybridInfo.Keys);
                foreach (string key in keys)
                {
                    DoSubRow(sublist.GetRect(rowH), key, extension, removeelements);
                }
            }
            if(!removeelements.NullOrEmpty()) foreach(string key in removeelements)
                {
                    extension.hybridInfo.Remove(key);
                }

            sublist.End();
            Widgets.DrawHighlightIfMouseover(rect);


        }

        protected void DoSubRow(Rect rect, string key, HybridExtensionExposable extension , List<string> removeelements)
        {
            bool isPawnKind = false;
            int value = (int)extension.hybridInfo.TryGetValue(key);
            string valuestr = value.ToString();
            string label = null;
            label = DefDatabase<ThingDef>.GetNamedSilentFail(key)?.label;
            if (label == null)
            {
                label = DefDatabase<PawnKindDef>.GetNamedSilentFail(key)?.label ?? "Undefined";
                isPawnKind = true;
            }
            Rect buttonRect = new Rect(rect.xMax - 90f, rect.y, 80f, rect.height);
            if (Widgets.ButtonText(buttonRect, "Delete"))
            {
                removeelements.Add(key);
            }
            buttonRect.x -= 80f;
            if (!isPawnKind)
            {
                if (Widgets.ButtonText(buttonRect, "PawnKind"))
                {
                    List<FloatMenuOption> list = new List<FloatMenuOption>();
                    if (!VariousDefOf.AllKinds.NullOrEmpty()) foreach (PawnKindDef def in VariousDefOf.AllKinds)
                        {
                            if (def.race.defName == key)
                            {
                                if (extension.hybridInfo.ContainsKey(def.defName)) continue;
                                else
                                {
                                    list.Add(new FloatMenuOption(def.label, delegate { extension.hybridInfo.Add(def.defName, 1.0f); }));
                                }
                            }
                        }
                    if (!list.NullOrEmpty()) Find.WindowStack.Add(new FloatMenu(list));
                    else SoundDefOf.ClickReject.PlayOneShotOnCamera();
                }
                buttonRect.x -= 80f;
            }
            else
            {
                Widgets.Label(buttonRect, "  PawnKind");
                buttonRect.x -= 80f;
            }
            label += ": " + key;
            Widgets.Label(rect, " - " + label);
            Widgets.TextFieldNumeric(buttonRect, ref value, ref valuestr,0,9999999);
            extension.hybridInfo.SetOrAdd(key, value);
            buttonRect.x -= 80f;
            Widgets.Label(buttonRect, String.Format("{0,0:P2}", value / totalWeight));
            Widgets.DrawHighlightIfMouseover(rect);
            TooltipHandler.TipRegion(rect, Translations.CustomHybrid_Tooltip(info.GetDef?.label ?? "Undefined", extension.GetDef?.label ?? "Undefined", label, String.Format("{0,0:0.########%}", value/totalWeight)));
        }

        
    }


}
