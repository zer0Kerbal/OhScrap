using ModuleWheels;

namespace OhScrap
{
    class LandingGearFailureModule : BaseFailureModule
    {
        private ModuleWheelDeployment _wheel;
        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Landing Gear Failure";
            Fields["safetyRating"].guiName = "Landing Gear Safety Rating";
            failureType = "Landing Gear Failure";
            _wheel = part.FindModuleImplementing<ModuleWheelDeployment>();
        }

        //This actually makes the failure happen
        public override void FailPart()
        {
            _wheel.enabled = false;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
        }
        //this repairs the part.
        public override void RepairPart()
        {
            _wheel.enabled = true;
        }
        //this should read from the Difficulty Settings.
        public override bool FailureAllowed()
        {
            if (_wheel == null) return false;
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().LandingGearFailureModuleAllowed;
        }
    }
}