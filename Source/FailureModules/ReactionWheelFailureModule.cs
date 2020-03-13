using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class ReactionWheelFailureModule : BaseFailureModule
    {
        ModuleReactionWheel rw;

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Reaction Wheel Failure";
            Fields["safetyRating"].guiName = "Reaction Wheel Safety Rating";
            failureType = "Reaction Wheel Failure";
            remoteRepairable = true;
            rw = part.FindModuleImplementing<ModuleReactionWheel>();
        }

        public override bool FailureAllowed()
        {
            if (rw == null) return false;
            if (!rw.isEnabled && rw.wheelState != ModuleReactionWheel.WheelState.Active) return false;
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ReactionWheelFailureModuleAllowed;
        }

        // Reaction wheel stops working
        public override void FailPart()
        {
            if (!rw) return;
            rw.isEnabled = false;
            rw.wheelState = ModuleReactionWheel.WheelState.Broken;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            hasFailed = true;
        }
        //Turns it back on again,
        public override void RepairPart()
        {
            if (!rw) return;
            rw.isEnabled = true;
            rw.wheelState = ModuleReactionWheel.WheelState.Active;
        }
    }
}
