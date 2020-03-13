using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class ParachuteFailureModule : BaseFailureModule
    {
        ModuleParachute chute;

        public override bool FailureAllowed()
        {
            if (chute == null) return false;
            if (chute.deploymentState != ModuleParachute.deploymentStates.DEPLOYED) return false;
            if (chute.deploymentState != ModuleParachute.deploymentStates.SEMIDEPLOYED) return false;
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ParachuteFailureModuleAllowed;
        }

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Parachute Failure";
            failureType = "Parachute Failure";
            Fields["safetyRating"].guiName = "Parachute Safety Rating";
            chute = part.FindModuleImplementing<ModuleParachute>();
        }

        //Cuts the chute if it's deployed
        public override void FailPart()
        {
            
            if (chute == null) return;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            if (chute.vessel != FlightGlobals.ActiveVessel) return;
            if (chute.deploymentState == ModuleParachute.deploymentStates.SEMIDEPLOYED || chute.deploymentState == ModuleParachute.deploymentStates.DEPLOYED) chute.CutParachute();
        }
    }
}
