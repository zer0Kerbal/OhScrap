using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class BatteryFailureModule : BaseFailureModule
    {
        PartResource battery;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Battery Failure";
            Fields["safetyRating"].guiName = "Battery Safety Rating";
            failureType = "Short Circuit";
            battery = part.Resources["ElectricCharge"];
        }

        // Failure will drain the battery and stop it from recharging.
        public override void FailPart()
        {
            battery.amount = 0;
            battery.flowState = false;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            if (hasFailed) return;
            Debug.Log("[OhScrap]: " + SYP.ID + " has suffered a short circuit failure");
        }

        //Repair allows it to be charged again.
        public override void RepairPart()
        {
           battery.flowState = true;
        }

        public override bool FailureAllowed()
        {
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().BatteryFailureModuleAllowed;
        }
    }
}
