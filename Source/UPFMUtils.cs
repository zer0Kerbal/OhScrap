using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using KSP.UI.Screens;
using ScrapYard;
using ScrapYard.Modules;
using System.Collections;
using System.IO;

namespace OhScrap
{
    //This is a KSPAddon that does everything that PartModules don't need to. Mostly handles the UI
    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    class EditorAnyWarnings : UPFMUtils
    {

    }
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class FlightWarnings : UPFMUtils
    {

    }
    class UPFMUtils : MonoBehaviour
    {
        //These hold all "stats" for parts that have already been generated (to stop them getting different results each time)
        public Dictionary<uint, int> generations = new Dictionary<uint, int>();
        public List<uint> testedParts = new List<uint>();
        public int vesselSafetyRating = -1;
        double nextFailureCheck = 0;
        Part worstPart;
        public bool display = false;
        bool dontBother = false;
        public static UPFMUtils instance;
        Rect Window = new Rect(500, 100, 480, 50);
        ApplicationLauncherButton ToolbarButton;
        ShipConstruct editorConstruct;
        public bool editorWindow = false;
        public bool flightWindow = true;
        bool highlightWorstPart = false;
        public System.Random _randomiser = new System.Random();
        public float minimumFailureChance = 0.01f;
        int timeBetweenChecksPlanes = 10;
        int timeBetweenChecksRocketsAtmosphere = 10;
        int timeBetweenChecksRocketsLocalSpace = 1800;
        int timeBetweenChecksRocketsDeepSpace = 25400;
        public bool ready = false;
        public bool debugMode = false;
        bool advancedDisplay = false;
        public double timeToOrbit = 300;
        double chanceOfFailure = 0;
        string failureMode = "Space/Landed";
        double displayFailureChance = 0;
        string sampleTime = "1 year";



        private void Awake()
        {
            instance = this;
            ReadDefaultCfg();
        }

        private void ReadDefaultCfg()
        {
            ConfigNode cn = ConfigNode.Load(KSPUtil.ApplicationRootPath + "/GameData/OhScrap/PluginData/DefaultSettings.cfg");
            if(cn == null)
            {
                Debug.Log("[OhScrap]: Default Settings file is missing. Using hardcoded defaults");
                ready = true;
                return;
            }
            float.TryParse(cn.GetValue("minimumFailureChance"), out minimumFailureChance);
            Debug.Log("[OhScrap]: minimumFailureChance: "+minimumFailureChance);
            int.TryParse(cn.GetValue("timeBetweenChecksPlanes"), out timeBetweenChecksPlanes);
            Debug.Log("[OhScrap]: timeBetweenChecksPlanes: "+timeBetweenChecksPlanes);
            int.TryParse(cn.GetValue("timeBetweenChecksRocketsAtmosphere"), out timeBetweenChecksRocketsAtmosphere);
            Debug.Log("[OhScrap]: timeBetweenChecksRocketsAtmosphere: "+timeBetweenChecksRocketsAtmosphere);
            int.TryParse(cn.GetValue("timeBetweenChecksRocketsLocalSpace"), out timeBetweenChecksRocketsLocalSpace);
            Debug.Log("[OhScrap]: timeBetweenChecksRocketsLocalSpace: "+timeBetweenChecksRocketsLocalSpace);
            int.TryParse(cn.GetValue("timeBetweenChecksRocketsDeepSpace"), out timeBetweenChecksRocketsDeepSpace);
            Debug.Log("[OhScrap]: timeBetweenChecksRocketsDeepSpace: "+timeBetweenChecksRocketsDeepSpace);
            double.TryParse(cn.GetValue("timeToOrbit"), out timeToOrbit);
            Debug.Log("[OhScrap]: timeToOrbit: "+timeToOrbit);
            bool.TryParse(cn.GetValue("debugMode"), out debugMode);
            Debug.Log("[OhScrap]: debugMode: "+debugMode);
            ready = true;
        }

        private void Start()
        {
            GameEvents.onPartDie.Add(OnPartDie);
            GameEvents.onGUIApplicationLauncherReady.Add(GUIReady);
            GameEvents.OnFlightGlobalsReady.Add(OnFlightGlobalsReady);
            GameEvents.onVesselSituationChange.Add(SituationChange);
            //Remembers if the player had the windows opened for closed last time they loaded this scene.
            if (!HighLogic.LoadedSceneIsEditor)
            {
                display = flightWindow;
            }
            else
            {
                display = editorWindow;
            }
            if (HighLogic.LoadedScene == GameScenes.FLIGHT) InvokeRepeating("CheckForFailures", 0.5f, 0.5f);
        }

        private void SituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> data)
        {
            if (data.host != FlightGlobals.ActiveVessel) return;
            nextFailureCheck = 0;
        }

        private void CheckForFailures()
        {
            if (!FlightGlobals.ready) return;
            if (KRASHWrapper.simulationActive()) return;
            if(FlightGlobals.ActiveVessel.FindPartModuleImplementing<ModuleUPFMEvents>() != null)
            {
                if(FlightGlobals.ActiveVessel.FindPartModuleImplementing<ModuleUPFMEvents>().tested == false) return;
            }
            if (Planetarium.GetUniversalTime() < nextFailureCheck) return;
            if (vesselSafetyRating == -1) return;
            List<BaseFailureModule> failureModules = FlightGlobals.ActiveVessel.FindPartModulesImplementing<BaseFailureModule>();
            if (failureModules.Count == 0) return;
            if (!VesselIsLaunched()) return;
            chanceOfFailure = 0.11-(vesselSafetyRating*0.01);
            if (chanceOfFailure < minimumFailureChance) chanceOfFailure = minimumFailureChance;
            SetNextCheck(failureModules);
            double failureRoll = _randomiser.NextDouble();
            if(HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().logging)
            {
                Logger.instance.Log("Failure Chance: " + chanceOfFailure + ", Rolled: " + failureRoll + " Succeeded: " + (failureRoll <= chanceOfFailure).ToString());
            }
            if (failureRoll > chanceOfFailure) return;
            Logger.instance.Log("Failure Event! Safety Rating: " + vesselSafetyRating + ", MET: " + FlightGlobals.ActiveVessel.missionTime);
            BaseFailureModule failedModule = null;
            int counter = failureModules.Count()-1;
            failureModules = failureModules.OrderBy(f => f.chanceOfFailure).ToList();
            while (counter >= 0)
            {
                failedModule = failureModules.ElementAt(counter);
                counter--;
                if (failedModule.hasFailed) continue;
                if (failedModule.isSRB) continue;
                if (failedModule.excluded) continue;
                if (!failedModule.launched) return;
                if (!failedModule.FailureAllowed()) continue;
                if (_randomiser.NextDouble() < failedModule.chanceOfFailure)
                {
                    if (failedModule.hasFailed) continue;
                    StartFailure(failedModule);
                    Logger.instance.Log("Failing " + failedModule.part.partInfo.title);
                    break;
                }
            }
            if (counter < 0)
            {
                Logger.instance.Log("No parts failed this time. Aborted failure");
            }
        }

        private bool VesselIsLaunched()
        {
            List<BaseFailureModule> modules = FlightGlobals.ActiveVessel.FindPartModulesImplementing<BaseFailureModule>();
            for (int i = 0; i < modules.Count; i++)
            {
                if (!modules.ElementAt(i).launched) return false;
            }
            return true;
        }

        private void SetNextCheck(List<BaseFailureModule> failureModules)
        {

            double chanceOfEvent = 0;
            double chanceOfIndividualFailure = 0;
            double exponent = 0;
            double preparedNumber;
            int moduleCount = 0;
            for (int i = 0; i < failureModules.Count(); i++)
            {
                BaseFailureModule failedModule = failureModules.ElementAt(i);
                if (failedModule.hasFailed) continue;
                if (failedModule.isSRB) continue;
                if (failedModule.excluded) continue;
                if (!failedModule.launched) return;
                if (!failedModule.FailureAllowed()) continue;
                moduleCount++;
            }
            preparedNumber = 1 - chanceOfFailure;
            preparedNumber = Math.Pow(preparedNumber, moduleCount);
            chanceOfIndividualFailure = 1 - preparedNumber;
            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.FLYING && FlightGlobals.ActiveVessel.mainBody == FlightGlobals.GetHomeBody())
            {
                if (FlightGlobals.ActiveVessel.missionTime < timeToOrbit)
                {
                    nextFailureCheck = Planetarium.GetUniversalTime() + timeBetweenChecksRocketsAtmosphere;
                    failureMode = "Atmosphere";
                    sampleTime = timeToOrbit/60+" minutes";
                }
                else
                {
                    nextFailureCheck = Planetarium.GetUniversalTime() + timeBetweenChecksPlanes;
                    failureMode = "Plane";
                    sampleTime = "15 minutes";
                }
            }
            else if (VesselIsInLocalSpace())
            {
                nextFailureCheck = Planetarium.GetUniversalTime()+timeBetweenChecksRocketsLocalSpace;
                failureMode = "Local Space";
                sampleTime = "7 days";
            }
            else
            {
                nextFailureCheck = Planetarium.GetUniversalTime() + timeBetweenChecksRocketsDeepSpace;
                failureMode = "Deep Space";
                sampleTime = "3 years";
            }
            switch(failureMode)
            {
                case "Atmosphere":
                    exponent = timeToOrbit / timeBetweenChecksRocketsAtmosphere;
                    break;
                case "Plane":
                    exponent = 900 / timeBetweenChecksPlanes;
                    break;
                case "Local Space":
                    exponent = FlightGlobals.GetHomeBody().solarDayLength *7 / timeBetweenChecksRocketsLocalSpace;
                    break;
                case "Deep Space":
                    exponent = FlightGlobals.GetHomeBody().orbit.period*3 / timeBetweenChecksRocketsDeepSpace;
                    break;
            }
            preparedNumber = vesselSafetyRating * 0.01;
            preparedNumber = 0.11f - preparedNumber;
            preparedNumber = 1 - preparedNumber;
            preparedNumber = Math.Pow(preparedNumber, exponent);
            chanceOfEvent = 1 - preparedNumber;
            displayFailureChance = Math.Round(chanceOfEvent * chanceOfIndividualFailure * 100,0);
            if (HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().logging)
            {
                Logger.instance.Log("[OhScrap]: Next Failure Check in "+(nextFailureCheck-Planetarium.GetUniversalTime()));
                Logger.instance.Log("[OhScrap]: Calculated chance of failure in next " + sampleTime + " is " + displayFailureChance + "%");
            }
        }

        private bool VesselIsInLocalSpace()
        {
            CelestialBody cb = FlightGlobals.ActiveVessel.mainBody;
            CelestialBody homeworld = FlightGlobals.GetHomeBody();
            if (cb == homeworld) return true;
            List<CelestialBody> children = homeworld.orbitingBodies;
            CelestialBody child;
            for (int i = 0; i < children.Count; i++)
            {
                child = children.ElementAt(i);
                if (child == cb) return true;
            }
            return false;
        }

        private void StartFailure(BaseFailureModule failedModule)
        {
            failedModule.FailPart();
            failedModule.hasFailed = true;         
            ModuleUPFMEvents eventModule = failedModule.part.FindModuleImplementing<ModuleUPFMEvents>();
            eventModule.highlight = true;
            eventModule.SetFailedHighlight();
            eventModule.Events["ToggleHighlight"].active = true;
            eventModule.Events["RepairChecks"].active = true;
            eventModule.doNotRecover = true;
            ScreenMessages.PostScreenMessage(failedModule.part.partInfo.title + ": " + failedModule.failureType);
            StringBuilder msg = new StringBuilder();
            msg.AppendLine(failedModule.part.vessel.vesselName);
            msg.AppendLine("");
            msg.AppendLine(failedModule.part.partInfo.title + " has suffered a " + failedModule.failureType);
            msg.AppendLine("");
            MessageSystem.Message m = new MessageSystem.Message("OhScrap", msg.ToString(), MessageSystemButton.MessageButtonColor.ORANGE, MessageSystemButton.ButtonIcons.ALERT);
            MessageSystem.Instance.AddMessage(m);
            Debug.Log("[OhScrap]: " + failedModule.SYP.ID + " of type " + failedModule.part.partInfo.title + " has suffered a " + failedModule.failureType);
            TimeWarp.SetRate(0, true);
            Logger.instance.Log("Failure Successful");
        }

        private void OnFlightGlobalsReady(bool data)
        {
            vesselSafetyRating = -1;
        }
        //This keeps track of which generation the part is.
        //If its been seen before it will be in the dictionary, so we can just return that (rather than having to guess by builds and times recovered)
        //Otherwise we can assume it's a new part and the "current" build count should be correct.
        public int GetGeneration(uint id, Part p)
        {
            if (generations.TryGetValue(id, out int i)) return i;
            if (HighLogic.LoadedSceneIsEditor) i = ScrapYardWrapper.GetBuildCount(p, ScrapYardWrapper.TrackType.NEW) + 1;
            else i = ScrapYardWrapper.GetBuildCount(p, ScrapYardWrapper.TrackType.NEW);
            generations.Add(id, i);
            return i;
        }

        //This is mostly for use in the flight scene, will only run once assuming everything goes ok.
        void Update()
        {
            try
            {
                int bfmCount = 0;
                vesselSafetyRating = 0;
                double worstPartChance = 0;
                if (!HighLogic.LoadedSceneIsEditor && FlightGlobals.ready)
                {
                    for (int i = 0; i < FlightGlobals.ActiveVessel.parts.Count(); i++)
                    {
                        Part p = FlightGlobals.ActiveVessel.parts.ElementAt(i);
                        List<BaseFailureModule> bfmList = p.FindModulesImplementing<BaseFailureModule>();
                        for (int b = 0; b < bfmList.Count(); b++)
                        {
                            BaseFailureModule bfm = bfmList.ElementAt(b);
                            if (bfm == null) continue;
                            if (!bfm.ready) return;
                            if (bfm.chanceOfFailure > worstPartChance && !bfm.isSRB && !bfm.hasFailed)
                            {
                                worstPart = p;
                                worstPartChance = bfm.chanceOfFailure;
                            }
                            vesselSafetyRating += bfm.safetyRating;
                            bfmCount++;
                        }
                    }
                }
                if (HighLogic.LoadedSceneIsEditor)
                {
                    if (editorConstruct == null || editorConstruct.parts.Count() == 0) editorConstruct = EditorLogic.fetch.ship;
                    for (int i = 0; i < editorConstruct.parts.Count(); i++)
                    {
                        Part p = editorConstruct.parts.ElementAt(i);
                        List<BaseFailureModule> bfmList = p.FindModulesImplementing<BaseFailureModule>();
                        for (int b = 0; b < bfmList.Count(); b++)
                        {
                            BaseFailureModule bfm = bfmList.ElementAt(b);
                            if (bfm == null) continue;
                            if (!bfm.ready) return;
                            if (bfm.chanceOfFailure > worstPartChance)
                            {
                                worstPart = p;
                                worstPartChance = bfm.chanceOfFailure;
                            }
                            vesselSafetyRating += bfm.safetyRating;
                            bfmCount++;
                        }
                    }
                    if (bfmCount == 0) editorConstruct = null;
                }
                vesselSafetyRating = vesselSafetyRating / bfmCount;
            }
            catch (DivideByZeroException)
            {
                return;
            }
            finally
            {
                if (worstPart != null)
                {
                    if (highlightWorstPart && worstPart.highlightType == Part.HighlightType.OnMouseOver)
                    {
                        worstPart.SetHighlightColor(Color.yellow);
                        worstPart.SetHighlightType(Part.HighlightType.AlwaysOn);
                        worstPart.SetHighlight(true, false);
                    }
                    if (!highlightWorstPart && worstPart.highlightType == Part.HighlightType.AlwaysOn && !worstPart.FindModuleImplementing<ModuleUPFMEvents>().highlightOverride)
                    {
                        worstPart.SetHighlightType(Part.HighlightType.OnMouseOver);
                        worstPart.SetHighlightColor(Color.green);
                        worstPart.SetHighlight(false, false);
                    }
                }
            }
        }

        //Removes the parts from the trackers when they die.
        private void OnPartDie(Part part)
        {
            ModuleSYPartTracker SYP = part.FindModuleImplementing<ModuleSYPartTracker>();
            if (SYP == null) return;
            generations.Remove(SYP.ID);
#if DEBUG
            Debug.Log("[UPFM]: Stopped Tracking " + SYP.ID);
#endif
        }

        //Add the toolbar button to the GUI
        public void GUIReady()
        {
            ToolbarButton = ApplicationLauncher.Instance.AddModApplication(GUISwitch, GUISwitch, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, GameDatabase.Instance.GetTexture("OhScrap/Plugins/Icon", false));
        }
        //switch the UI on/off
        public void GUISwitch()
        {
            display = !display;
            ToggleWindow();
        }
        
        //shouldn't really be using OnGUI but I'm too lazy to learn PopUpDialog
        private void OnGUI()
        {
            if (!HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().safetyWarning) return;
            if (HighLogic.CurrentGame.Mode == Game.Modes.MISSION) return;
            if (dontBother) return;
            if (!display) return;
            //Display goes away if EVA Kerbal
            if (FlightGlobals.ActiveVessel != null)
            {
                if (FlightGlobals.ActiveVessel.FindPartModuleImplementing<KerbalEVA>() != null) return;
            }
            Window = GUILayout.Window(98399854, Window, GUIDisplay, "OhScrap", GUILayout.Width(300));
        }
        void GUIDisplay(int windowID)
        {
            //Grabs the vessels safety rating and shows the string associated with it.
            string s;
            switch (vesselSafetyRating)
            {
                case 10:
                    s = "(Excellent)";
                    break;
                case 9:
                    s = "(Excellent)";
                    break;
                case 8:
                    s = "(Good)";
                    break;
                case 7:
                    s = "(Good)";
                    break;
                case 6:
                    s = "(Average)";
                    break;
                case 5:
                    s = "(Average)";
                    break;
                case 4:
                    s = "(Poor)";
                    break;
                case 3:
                    s = "(Poor)";
                    break;
                case 2:
                    s = "(Terrible)";
                    break;
                case 1:
                    s = "(Terrible)";
                    break;
                case 0:
                    s = "(Failure Imminent)";
                    break;
                default:
                    s = "(Invalid)";
                    break;
            }
            if(vesselSafetyRating == -1 || editorConstruct == null || editorConstruct.parts.Count() == 0)
            {
                if (HighLogic.LoadedSceneIsEditor || vesselSafetyRating == -1)
                {
                    GUILayout.Label("No parts detected. Place or right click on a part");
                    return;
                }
            }
            GUILayout.Label("Vessel Safety Rating: " + vesselSafetyRating + " " + s);
            advancedDisplay = File.Exists(KSPUtil.ApplicationRootPath + "GameData/OhScrap/debug.txt");
            if (advancedDisplay)
            {
                GUILayout.Label("WARNING! CALCULATIONS ARE EXPERIMENTAL");
                GUILayout.Label("MODE: " + failureMode);
                GUILayout.Label("Chance of Failure in next " + sampleTime + ": " + displayFailureChance + "%");
            }
            if (worstPart != null)
            {
                GUILayout.Label("Worst Part: " + worstPart.partInfo.title);
                if (GUILayout.Button("Highlight Worst Part")) highlightWorstPart = !highlightWorstPart;
            }
            if (GUILayout.Button("Close"))
            {
                display = false;
                ToggleWindow();
            }
            GUI.DragWindow();
        }
        
        void ToggleWindow()
        {
            if (HighLogic.LoadedSceneIsEditor) editorWindow = display;
            else flightWindow = display;
        }

        private void OnDisable()
        {
            display = false;
            GameEvents.onGUIApplicationLauncherReady.Remove(GUIReady);
            GameEvents.onPartDie.Remove(OnPartDie);
            GameEvents.OnFlightGlobalsReady.Remove(OnFlightGlobalsReady);
            GameEvents.onVesselSituationChange.Remove(SituationChange);
            if (ToolbarButton == null) return;
            ApplicationLauncher.Instance.RemoveModApplication(ToolbarButton);
        }
    }
}
