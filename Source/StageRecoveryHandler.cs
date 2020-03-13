using System;
using UnityEngine;

namespace OhScrap
{
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class StageRecoveryHandler : MonoBehaviour
    {
        public void Awake()
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER && StageRecoveryWrapper.StageRecoveryEnabled && HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().overrideStageRecovery) StageRecoveryWrapper.AddRecoverySuccessEvent(RecoverySuccess);
        }

        private void RecoverySuccess(Vessel vessel, float[] returns, string result)
        {

            if (result == "SUCCESS") Funding.Instance.AddFunds(-returns[1], TransactionReasons.VesselRecovery);
        }

        public void OnDisable()
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER && StageRecoveryWrapper.StageRecoveryEnabled && HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().overrideStageRecovery) StageRecoveryWrapper.RemoveRecoverySuccessEvent(RecoverySuccess);
        }
    }
}
