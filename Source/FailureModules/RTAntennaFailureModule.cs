using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;


namespace OhScrap
{
    //Allows For failures on Antennas when remote tech is installed. Can fail Parts with ModuleRTAntenna.
    class RTAntennaFailureModule : BaseFailureModule
    {
        private PartModule antenna;
        [KSPField(isPersistant = true, guiActive = false)]
        bool RTAvailable = ModWrapper.RemoteTechWrapper.available;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Antenna Failure";
            Fields["safetyRating"].guiName = "Antenna Safety Rating";
            failureType = "communication failure";
            FindAntennaModule();
            remoteRepairable = true;
        }

        public override bool FailureAllowed()
        {
                if (!antenna) FindAntennaModule();
                if (!antenna) return false;
                if (!RTAvailable) return false;
                if (!ModWrapper.RemoteTechWrapper.GetAntennaDeployed(antenna)) return false; //Do not fail antennas that are deployed. Returns true if it cant be animated.
                return (HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().AntennaFailureModuleAllowed);
        }

        public override void FailPart()
        {
            if (!antenna) FindAntennaModule();
            if (!antenna) return;
            if (OhScrap != null && OhScrap.highlight) OhScrap.SetFailedHighlight();
            ModWrapper.RemoteTechWrapper.SetRTBrokenStatus(antenna, true);
            if (hasFailed) return;
            Debug.Log("[OhScrap](RemoteTech): " + (SYP == null ? "unknown part" : SYP.ID.ToString()) + " has stopped transmitting");
        }
        
        public override void RepairPart()
        {
            if (antenna)
            {
                ModWrapper.RemoteTechWrapper.SetRTBrokenStatus(antenna, false);
            }
        }

        private void FindAntennaModule()
        {
            if (part == null || part.Modules == null) return;
            foreach (PartModule pm in part.Modules)
            {
                if (pm.moduleName.Equals("ModuleRTAntenna"))
                {
                    antenna = pm;
                    return;
                }
            }
        }

    
    }
}
