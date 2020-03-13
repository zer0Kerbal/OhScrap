using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ScrapYard.Modules;
using ScrapYard;

namespace OhScrap
{
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.EDITOR)]
    public class UPFMData : ScenarioModule
    {
        public override void OnSave(ConfigNode node)
        {
            ConfigNode temp = node.GetNode("UPFMTracker");
            if (temp == null)
            {
                node.AddNode("UPFMTracker");
                temp = node.GetNode("UPFMTracker");
            }
            foreach(var v in UPFMUtils.instance.generations)
            {
                if (v.Key == 0) continue;
                ConfigNode cn = new ConfigNode("PART");
                cn.SetValue("ID", v.Key, true);
                cn.SetValue("Generation", v.Value, true);
                cn.SetValue("Tested", UPFMUtils.instance.testedParts.Contains(v.Key), true);
                temp.AddNode(cn);
            }
            temp.SetValue("FlightWindow", UPFMUtils.instance.flightWindow, true);
            temp.SetValue("EditorWindow", UPFMUtils.instance.editorWindow, true);
            Debug.Log("[OhScrap]: Saved");
        }

        public override void OnLoad(ConfigNode node)
        {
            ConfigNode temp = node.GetNode("UPFMTracker");
            if (temp == null) return;
            UPFMUtils.instance.generations.Clear();
            UPFMUtils.instance.testedParts.Clear();
            bool.TryParse(temp.GetValue("FlightWindow"), out UPFMUtils.instance.flightWindow);
            bool.TryParse(temp.GetValue("EditorWindow"), out UPFMUtils.instance.editorWindow);
            ConfigNode[] nodes = temp.GetNodes("PART");
            if (nodes.Count() == 0) return;
            for (int i = 0; i < nodes.Count(); i++)
            {
                ConfigNode cn = nodes.ElementAt(i);
                string s = cn.GetValue("ID");
                uint.TryParse(s, out uint u);
                if (int.TryParse(cn.GetValue("Generation"), out int g)) UPFMUtils.instance.generations.Add(u, g);
                if (bool.TryParse(cn.GetValue("Tested"), out bool b) == true) UPFMUtils.instance.testedParts.Add(u);
            }
            nodes = temp.GetNodes("FAILURE");
            if (nodes.Count() == 0) return;
            for (int i = 0; i < nodes.Count(); i++)
            {
                ConfigNode cn = nodes.ElementAt(i);
                string s = cn.GetValue("Name");
            }
            Debug.Log("[OhScrap]: Loaded");
        }
    }
}
