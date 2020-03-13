using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class ControlSurfaceFailureModule : BaseFailureModule
    {
        ModuleControlSurface controlSurface;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Control Surface Failure";
            Fields["safetyRating"].guiName = "Control Surface Safety Rating";
            failureType = "Stuck Control Surface";
            //Part is mechanical so can be repaired remotely.
            remoteRepairable = true;
            controlSurface = part.FindModuleImplementing<ModuleControlSurface>();
        }

        public override bool FailureAllowed()
        {
            if (part.vessel.atmDensity == 0) return false;
            if (controlSurface == null) return false;
            return (HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ControlSurfaceFailureModuleAllowed
            &&  !ModWrapper.FerramWrapper.available); 
        }
        //control surface will stick and not respond to input
        public override void FailPart()
        {
            if (!controlSurface) return;
            if(!hasFailed)
            {
                Debug.Log("[OhScrap]: " + SYP.ID + " has suffered a control surface failure");
            }
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            controlSurface.ignorePitch = true;
            controlSurface.ignoreRoll = true;
            controlSurface.ignoreYaw = true;
        }
        //restores control to the control surface
        public override void RepairPart()
        {
            controlSurface.ignorePitch = false;
            controlSurface.ignoreRoll = false;
            controlSurface.ignoreYaw = false;
        }
    }
}
