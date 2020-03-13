using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace OhScrap
{

    //Control surface failures for Ferram Aerospace. 
    class FARControlSurfaceFailureModule : BaseFailureModule
    {

        PartModule FARControlSurface;
        private static System.Random _randomizer = new System.Random();
        private int failMode;


        //We have two failures. The Control surface gets stuck (0 control input), or it flails about partialy broken and adjusts your control input with weighted random amounts. These are the weights.  
        [KSPField(isPersistant = true, guiActive = false)]
        public int stuckWeight = 20;
        [KSPField(isPersistant = true, guiActive = false)]
        public int hingeWeight = 80;

        private int weightTotal; 

        //Hinge Failure additional random weights. - Every tick we adjust pitch/yaw/roll input amount or we reset to the fail time values, or set all to 0. 
        [KSPField(isPersistant = true, guiActive = false)]
        public int hingeAdjustmentWeight = 20;  //chance of changing one of pitch,yaw or roll when a hinge failure happens.
        
        public int hingePitchWeight;
        public int hingeYawWeight; 
        public int hingeRollWeight;

        [KSPField(isPersistant = true, guiActive = false)]
        public int hingeResetWeight = 5; //chance of resetting control to fail time amounts during a hinge fail.
        [KSPField(isPersistant = true, guiActive = false)]
        public int hingeStuckWeight = 2; //Chance of setting input to 0 during failure. 

        public int hingeWeightTotal;

        //Upper and lower bounds of how much we adjust the control surface input during a hinge failure. 
        //Different hinge failures will feel more or less severe.
        [KSPField(isPersistant = true, guiActive = false)]
        public int minAdjustAmount = 8;
        [KSPField(isPersistant = true, guiActive = false)]
        public int maxAdjustAmount = 15;


        private int adjustmentAmount = 0;
        private StuckScenario stuckScenario;
        private ResetScenario resetScenario;
        private List<Scenario> hingeFailureScenarios = new List<Scenario>(); 

        protected override void Overrides()
        {
            Fields["displayChance"].guiName = "Chance of Control Surface Failure";
            Fields["safetyRating"].guiName = "Control Surface Safety Rating";
            failureType = "Control Surface Failure";
            foreach (PartModule pm in part.Modules)
            {
                if (pm.moduleName.Equals("FARControllableSurface"))
                {
                    FARControlSurface = pm;
                }
            }
            
            //Part is mechanical so can be repaired remotely.
            remoteRepairable = true;

            //Setup Weights and references. 
            CheckValidWeights(); 

            hingePitchWeight = hingeAdjustmentWeight;
            hingeYawWeight = hingeAdjustmentWeight;
            hingeRollWeight = hingeAdjustmentWeight;
            weightTotal = stuckWeight + hingeWeight;
            hingeWeightTotal = (hingePitchWeight * 3) + hingeResetWeight + hingeStuckWeight;

            hingeFailureScenarios.Add(new PitchChangeScenario(hingePitchWeight, adjustmentAmount));
            hingeFailureScenarios.Add(new YawChangeScenario(hingeYawWeight, adjustmentAmount));
            hingeFailureScenarios.Add(new RollChangeScenario(hingeRollWeight, adjustmentAmount));
            resetScenario = new ResetScenario(hingeResetWeight);
            hingeFailureScenarios.Add(resetScenario);
            stuckScenario = new StuckScenario(hingeStuckWeight);
            hingeFailureScenarios.Add(stuckScenario);
            hingeFailureScenarios = hingeFailureScenarios.OrderBy(i => (i.Weight)).ToList();
        }

        public override bool FailureAllowed()
        {
            if (part.vessel.atmDensity == 0) return false;
            return (HighLogic.CurrentGame.Parameters.CustomParams<UPFMSettings>().ControlSurfaceFailureModuleAllowed
            && ModWrapper.FerramWrapper.available
            && FARControlSurface);
        }
        
        public override void FailPart()
        {
            if (!FARControlSurface) return;
            if (!hasFailed) 
            {
                failMode = _randomizer.Next(1, weightTotal + 1);
                //failTimeBrakeRudder = ModWrapper.FerramWrapper.GetCtrlSurfBrakeRudder(FARControlSurface);
                resetScenario.failTimeYaw = ModWrapper.FerramWrapper.GetCtrlSurfYaw(FARControlSurface);
                resetScenario.failTimePitch = ModWrapper.FerramWrapper.GetCtrlSurfPitch(FARControlSurface);
                resetScenario.failTimeRoll = ModWrapper.FerramWrapper.GetCtrlSurfRoll(FARControlSurface);
                
            }

            if (failMode <= stuckWeight) //Stuck Surface.
            {
                if (!hasFailed)
                {
                    Debug.Log("[OhScrap](FAR): " + SYP.ID + " has a stuck control surface");
                    
                    hasFailed = true;
                }
                stuckScenario.Run(FARControlSurface);
            }
            else //Hinge Failure. 
            {
                if (!hasFailed)
                {
                    Debug.Log("[OhScrap](FAR): " + SYP.ID + " Control surface hinge failure.");
                    hasFailed = true;
                }
                int adjustmentRoll = _randomizer.Next(1, hingeWeightTotal + 1);
                
                int counter = hingeFailureScenarios.Count - 1;
                Scenario s = null;
                while (counter >= 0)
                {
                    s = hingeFailureScenarios.ElementAt(counter);
                    if ((adjustmentRoll -= s.Weight) <= 0)
                    {
                        counter = 0;
                        
                    }
                    counter--;
                }
                s.Run(FARControlSurface);
                if (OhScrap.highlight) OhScrap.SetFailedHighlight();
            }
        }
       
        
        //restores control to the control surface
        public override void RepairPart()
        {
            resetScenario.Run(FARControlSurface);
            
        }

        private abstract class Scenario
        {
            public int Weight { get; set; }
            public String Name { get; set; }
            public abstract void Run(PartModule surface);
        }

        private class PitchChangeScenario : Scenario
        {
            public int adjustmentAmount { get; set; }
            private float curr_amount;
            public PitchChangeScenario(int weight, int amount  )
            {
                adjustmentAmount = amount;
                Weight = weight;
            }
            public override void Run(PartModule surface)
            {
                curr_amount = ModWrapper.FerramWrapper.GetCtrlSurfPitch(surface);
                int sign = _randomizer.Next(0, 2);
                if (curr_amount > 90.0f)
                {
                    sign = 1;
                }else if(curr_amount < -90.0f)
                {
                    sign = 0;
                }
                

                if(sign == 0)
                {
                    ModWrapper.FerramWrapper.SetCtrlSurfPitch(surface, (curr_amount + ((curr_amount * adjustmentAmount) / 100)));
                }else
                {
                    ModWrapper.FerramWrapper.SetCtrlSurfPitch(surface, (curr_amount - ((curr_amount * adjustmentAmount) / 100)));
                }
                
            }
           
        }

        private class YawChangeScenario : Scenario
        {
            public int adjustmentAmount { get; set; }
            private float curr_amount;
            public YawChangeScenario(int weight, int amount)
            {
                adjustmentAmount = amount;
                Weight = weight;
            }
            public override void Run(PartModule surface)
            {
                curr_amount = ModWrapper.FerramWrapper.GetCtrlSurfYaw(surface);
                int sign = _randomizer.Next(0, 2);
                if (curr_amount > 90.0f)
                {
                    sign = 1;
                }
                else if (curr_amount < -90.0f)
                {
                    sign = 0;
                }

                if (sign == 0)
                {
                    ModWrapper.FerramWrapper.SetCtrlSurfYaw(surface, (curr_amount + ((curr_amount * adjustmentAmount) / 100)));
                }
                else
                {
                    ModWrapper.FerramWrapper.SetCtrlSurfYaw(surface, (curr_amount - ((curr_amount * adjustmentAmount) / 100)));
                }
               
            }

        }

        private class RollChangeScenario : Scenario
        {
            public int adjustmentAmount { get; set; }
            private float curr_amount;
            public RollChangeScenario(int weight, int amount)
            {
                adjustmentAmount = amount;
                Weight = weight;
            }
            public override void Run(PartModule surface)
            {
                curr_amount = ModWrapper.FerramWrapper.GetCtrlSurfRoll(surface);
                int sign = _randomizer.Next(0, 2);
                if (curr_amount > 90.0f) 
                {
                    sign = 1;
                }
                else if (curr_amount < -90.0f)
                {
                    sign = 0;
                }

                if (sign == 0)
                {
                    ModWrapper.FerramWrapper.SetCtrlSurfRoll(surface, (curr_amount + ((curr_amount * adjustmentAmount) / 100)));
                }
                else
                {
                    ModWrapper.FerramWrapper.SetCtrlSurfRoll(surface, (curr_amount - ((curr_amount * adjustmentAmount) / 100)));
                }
                
            }

        }

        private class ResetScenario : Scenario
        {
            public float failTimePitch { get; set; }
            public float failTimeYaw { get; set; }
            public float failTimeRoll { get; set; }
            public ResetScenario(int weight)
            {
                Weight = weight;
            }
            public override void Run(PartModule surface)
            {
                ModWrapper.FerramWrapper.SetCtrlSurfPitch(surface, failTimePitch);
                ModWrapper.FerramWrapper.SetCtrlSurfRoll(surface, failTimeRoll);
                ModWrapper.FerramWrapper.SetCtrlSurfYaw(surface, failTimeYaw);
            }

        }

        private class StuckScenario : Scenario
        {
            public StuckScenario(int weight)
            {
                Weight = weight;
            }
            public override void Run(PartModule surface)
            {
                ModWrapper.FerramWrapper.SetCtrlSurfPitch(surface, 0.0f);
                ModWrapper.FerramWrapper.SetCtrlSurfRoll(surface, 0.0f);
                ModWrapper.FerramWrapper.SetCtrlSurfYaw(surface, 0.0f);
            }
        }

        private void CheckValidWeights()
        {
            removeNegatives(stuckWeight);
            removeNegatives(hingeWeight);
            removeNegatives(hingeAdjustmentWeight);
            removeNegatives(hingeResetWeight);
            removeNegatives(hingeStuckWeight);
            if (minAdjustAmount >= maxAdjustAmount)
            {
                adjustmentAmount = minAdjustAmount;
            }
            else
            {
                adjustmentAmount = _randomizer.Next(minAdjustAmount, maxAdjustAmount);
            }
        }

        private int removeNegatives(int x)
        {
            return x < 0 ? 0 : x;
        }
    }

}

