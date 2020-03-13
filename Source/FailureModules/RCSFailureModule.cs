using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{
    class RCSFailureModule : BaseFailureModule
    {
        ModuleRCS rcs;

        public override bool FailureAllowed()
        {
            if (rcs == null) return false;
            if(!vessel.ActionGroups[KSPActionGroup.RCS]) return false;
             return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().RCSFailureModuleAllowed;
        }

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of RCS Failure";
            Fields["safetyRating"].guiName = "RCS Safety Rating";
            failureType = "RCS Failure";
            rcs = part.FindModuleImplementing<ModuleRCS>();
        }

        //turns the RCS off.
        public override void FailPart()
        {
            if (rcs == null) return;
            rcs.rcsEnabled = false;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
        }
        //turns it back on again
        public override void RepairPart()
        {
            rcs.rcsEnabled = true;
        }
    }
}
