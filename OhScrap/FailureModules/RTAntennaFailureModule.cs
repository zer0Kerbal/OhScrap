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
            if (!antenna)
            {
                foreach (PartModule pm in part.Modules)
                {
                    if (pm.moduleName.Equals("ModuleRTAntenna"))
                    {
                       antenna = pm;
                    }
                }
            }
            remoteRepairable = true;
        }

        public override bool FailureAllowed()
        {
                if (!antenna) return false;
                if (!RTAvailable) return false;
                if (!ModWrapper.RemoteTechWrapper.GetAntennaDeployed(antenna)) return false; //Do not fail antennas that are deployed. Returns true if it cant be animated.
                return (HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().AntennaFailureModuleAllowed);
        }

        public override void FailPart()
        {
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            ModWrapper.RemoteTechWrapper.SetRTBrokenStatus(antenna, true);
            if (hasFailed) return;
            Debug.Log("[OhScrap](RemoteTech): " + SYP.ID + " has stopped transmitting");
        }
        
        public override void RepairPart()
        {
            if (antenna)
            {
                ModWrapper.RemoteTechWrapper.SetRTBrokenStatus(antenna, false);
            }
        }

    
    }
}