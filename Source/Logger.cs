using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace OhScrap
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    class Logger : MonoBehaviour
    {
        public List<string> logs = new List<string>();
        public static Logger instance;
        string directory;
        public void Awake()
        {
            logs.Add("Using Oh Scrap " + Version.Number);
            instance = this;
            directory = KSPUtil.ApplicationRootPath + "/GameData/OhScrap/Logs/";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            DirectoryInfo source = new DirectoryInfo(directory);
            foreach (FileInfo fi in source.GetFiles())
            {
                var creationTime = fi.CreationTime;
                if (creationTime < (DateTime.Now - new TimeSpan(1, 0, 0, 0)))
                {
                    fi.Delete();
                }
            }
        }

        public void Log(string s)
        {
            logs.Add(s);
            Debug.Log("[OhScrap]: " + s);
        }

        public void OnDisable()
        {
            if (logs.Count() == 1) return;
            string path = directory + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")+".txt";
            using (StreamWriter writer = File.AppendText(path))
            {
                foreach (string s in logs)
                {
                    writer.WriteLine(s);
                }
            }
        }
    }
}

