namespace OhScrap
{
    class AlternatorFailureModule : BaseFailureModule
    {
        private ModuleAlternator _alternator;
        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Alternator Failure";
            Fields["safetyRating"].guiName = "Alternator Safety Rating";
            failureType = "Alternator Failure";
            _alternator = part.FindModuleImplementing<ModuleAlternator>();
        }

        //This actually makes the failure happen
        public override void FailPart()
        {
            _alternator.enabled = false;
            if (OhScrap.highlight) OhScrap.SetFailedHighlight();
        }
        //this repairs the part.
        public override void RepairPart()
        {
            _alternator.enabled = true;
        }
        //this should read from the Difficulty Settings.
        public override bool FailureAllowed()
        {
            if (_alternator == null) return false;
            if (_alternator.outputRate < 0.1f) return false;
            return HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().AlternatorFailureModuleAllowed;
        }
    }
}