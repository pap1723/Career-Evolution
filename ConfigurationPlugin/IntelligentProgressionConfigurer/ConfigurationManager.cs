using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using FinePrint.Utilities;

namespace IntelligentProgressionConfigurer
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class ConfigurationManager : MonoBehaviour
    {
        private readonly string saveLocation = KSPUtil.ApplicationRootPath + "/GameData/ContractPacks/IPContracts/IntelligentProgression.cfg";
        bool done = false;
        bool showGUI = false;
        Rect Window = new Rect(20, 100, 240, 50);

        public void OnGUI()
        {
            if (showGUI)
            {
                Window = GUILayout.Window(65468754, Window, GUIDisplay, "IntelligentProgression", GUILayout.Width(200));
            }
        }

        void GUIDisplay(int windowID)
        {
            GUILayout.Label("Intelligent Progression: Configuration File created. Please restart KSP so changes can take effect");
            if (GUILayout.Button("OK")) showGUI = false;
        }

        private void Awake()
        {
            if (done) Destroy(this);
            if (File.Exists(saveLocation))
            {
                Debug.Log("[IntelligentProgression]: Planets have already been configured");
                done = true;
                return;
            }
            if(FlightGlobals.Bodies.Count == 0)
            {
                Debug.Log("[IntelligentProgression]: Error loading bodies");
                done = true;
                return;
            }
            CelestialBody HomeWorld = Planetarium.fetch.Home;
            CelestialBody Sun = Planetarium.fetch.Sun;
            ConfigNode masterNode = new ConfigNode();
            ConfigNode baseNode = new ConfigNode("@CONTRACT_GROUP[CareerEvolution]");
            ConfigNode planetNode  = new ConfigNode("@DATA[Planets]");
            double d = HomeWorld.orbit.semiMajorAxis;
            CelestialBody Candidate = null;
            for(int i = 0; i<FlightGlobals.Bodies.Count(); i++)
            {
                CelestialBody findingMercury = FlightGlobals.Bodies.ElementAt(i);
                if (findingMercury.orbit == null) continue;
                if (findingMercury.orbit.semiMajorAxis < d && findingMercury.referenceBody == Sun)
                {
                    d = findingMercury.orbit.semiMajorAxis;
                    Candidate = findingMercury;
                }
            }
            if (Candidate != null) planetNode.SetValue("@Mercury", "[" + Candidate.name+"].Random()",true);
            Candidate = null;
            d = HomeWorld.orbit.semiMajorAxis;
            for (int i = 0; i < FlightGlobals.Bodies.Count(); i++)
            {
                CelestialBody findingVenus = FlightGlobals.Bodies.ElementAt(i);
                if (findingVenus.orbit == null) continue;
                if (d == HomeWorld.orbit.semiMajorAxis)
                {
                    d = findingVenus.orbit.semiMajorAxis;
                    Candidate = findingVenus;
                }
                else if (findingVenus.orbit.semiMajorAxis > d && findingVenus.orbit.semiMinorAxis < HomeWorld.orbit.semiMinorAxis && findingVenus.referenceBody == Sun)
                {
                    d = findingVenus.orbit.semiMajorAxis;
                    Candidate = findingVenus;
                }
            }
            if (Candidate != null) planetNode.SetValue("@Venus", "[" + Candidate.name + "].Random()", true);
            Candidate = null;
            d = HomeWorld.orbit.semiMajorAxis;
            for (int i = 0; i < FlightGlobals.Bodies.Count(); i++)
            {
                CelestialBody findingMars = FlightGlobals.Bodies.ElementAt(i);
                if (findingMars.orbit == null) continue;
                if (findingMars.orbit.semiMinorAxis > d && HomeWorld.orbit.semiMinorAxis == d)
                {
                    d = findingMars.orbit.semiMajorAxis;
                    Candidate = findingMars;
                }
                else if (findingMars.orbit.semiMajorAxis < d && findingMars.orbit.semiMinorAxis > HomeWorld.orbit.semiMinorAxis && findingMars.referenceBody == Sun)
                {
                    d = findingMars.orbit.semiMajorAxis;
                    Candidate = findingMars;
                }
            }
            if (Candidate != null) planetNode.SetValue("@Mars", "[" + Candidate.name + "].Random()", true);
            CelestialBody CandidateMoon = null;
            if ( Candidate != null) CandidateMoon = Candidate.orbitingBodies.FirstOrDefault();
            if (CandidateMoon != null) planetNode.SetValue("@Phobos", "[" + CandidateMoon.name + "].Random()", true);
            d = HomeWorld.orbit.semiMajorAxis;
            Candidate = null;
            for (int i = 0; i < FlightGlobals.Bodies.Count(); i++)
            {
                CelestialBody findingJupiter = FlightGlobals.Bodies.ElementAt(i);
                if (CelestialUtilities.IsGasGiant(findingJupiter) && findingJupiter != Sun)
                {
                    Candidate = findingJupiter;
                    break;
                }
            }
            if (Candidate != null) planetNode.SetValue("@Jupiter", "[" + Candidate.name + "].Random()", true);
            Candidate = null;
            d = HomeWorld.orbit.semiMajorAxis;
            baseNode.AddNode(planetNode);
            masterNode.AddNode(baseNode);
            masterNode.Save(saveLocation);
            showGUI = true;
            done = true;
        }
    }
}
