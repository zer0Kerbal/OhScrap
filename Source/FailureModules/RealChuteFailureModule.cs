using System;
using System.Reflection;
using UnityEngine;

namespace OhScrap
{
    class RealChuteFailureModule : BaseFailureModule
    {
        private static readonly BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private PartModule chute;

        public override bool FailureAllowed()
        {
            return chute != null && HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ParachuteFailureModuleAllowed;
        }

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Parachute Failure";
            failureType = "Parachute Failure";
            Fields["safetyRating"].guiName = "Parachute Safety Rating";
            chute = FindRealChuteModule();
        }

        public override void FailPart()
        {
            if (chute == null) chute = FindRealChuteModule();
            if (chute == null) return;
            if (OhScrap != null && OhScrap.highlight) OhScrap.SetFailedHighlight();
            if (chute.vessel != FlightGlobals.ActiveVessel) return;
            if (hasFailed) return;

            if (GetBoolProperty("AnyDeployed"))
            {
                InvokeMethod("GUICut");
            }
            else
            {
                InvokeMethod("ActivateRC");
                SetEventActive("GUIDisarm", false);
                SetEventActive("GUIRepack", false);
            }

            hasFailed = true;
        }

        public override void RepairPart()
        {
            if (chute == null) chute = FindRealChuteModule();
            if (chute == null) return;
            SetEventActive("GUIDisarm", true);
            SetEventActive("GUIRepack", true);
        }

        private PartModule FindRealChuteModule()
        {
            if (part == null || part.Modules == null) return null;
            foreach (PartModule pm in part.Modules)
            {
                if (pm.moduleName.Equals("RealChuteModule", StringComparison.Ordinal))
                {
                    return pm;
                }
            }
            return null;
        }

        private bool GetBoolProperty(string propertyName)
        {
            PropertyInfo property = chute.GetType().GetProperty(propertyName, flags);
            return property != null && (bool)property.GetValue(chute, null);
        }

        private void InvokeMethod(string methodName)
        {
            MethodInfo method = chute.GetType().GetMethod(methodName, flags);
            if (method != null) method.Invoke(chute, null);
        }

        private void SetEventActive(string eventName, bool active)
        {
            if (chute.Events[eventName] != null)
            {
                chute.Events[eventName].active = active;
            }
        }
    }
}
