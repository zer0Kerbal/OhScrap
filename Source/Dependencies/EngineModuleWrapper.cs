/*
    This file is part of OhScrap!
        Copyright 2016 Magico13
        Copyright 2018 Martin Joy (aka severedsolo)
        Copyright 2020 zer0Kerbal

    and it's Copyright 2020 Lisias T : http://lisias.net <support@lisias.net>
    and licensed to you under The MIT License (MIT)

    Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
    files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
    modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
    Software is furnished to do so, subject to the following conditions:

        The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
    INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
    PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
    FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
    ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

*/
using System;

namespace OhScrap
{
    internal static class Log
    {
        public static void Info(string msg, params object[] @params)
        {
            UnityEngine.Debug.LogErrorFormat("[EngineModuleWrapper] INFO: " + msg, @params);
        }

        public static void Error(string msg, params object[] @params)
        {
            UnityEngine.Debug.LogErrorFormat("[EngineModuleWrapper] ERROR: " + msg, @params);
        }
    }

    internal interface EngineModuleIfc
    {
        void SetFuelFlowMult(float v);
        void SetFuelIspMult(float v);
    }

    internal class UnknownEngine : EngineModuleIfc
    {
        internal UnknownEngine(Part part)
        {
            Log.Error("No suitable module found on part {0}! Add'On may not behave as intended...", part.name);
        }

        void EngineModuleIfc.SetFuelFlowMult(float v)
        {
            return;
        }

        void EngineModuleIfc.SetFuelIspMult(float v)
        {
            return;
        }
    }

    internal abstract class AbstractEngine
    {
        protected readonly Part part;
        protected readonly ModuleEngines e;

        protected readonly float minFuelFlow;
        protected readonly float maxFuelFlow;
        protected readonly float g;

        internal AbstractEngine(Part part, ModuleEngines e)
        {
            this.part = part;
            this.e = e;

            this.minFuelFlow = this.e.minFuelFlow;
            this.maxFuelFlow = this.e.maxFuelFlow;
            this.g = this.e.g;
        }

    }

    internal class RealFuelsEngine : AbstractEngine, EngineModuleIfc
    {
        private static readonly string MODULENAME = "ModuleEnginesRF";
        internal static bool IsCompatible(ModuleEngines engine)
        {
            return MODULENAME.Equals(engine.GetType().Name);
        }

        internal RealFuelsEngine(Part part, ModuleEngines engine) : base(part, engine)
        {
            Log.Info("{0} found on part {1}, engine {2}! Add'On may not behave as intended...", MODULENAME, part.name, e.name);
        }

        void EngineModuleIfc.SetFuelFlowMult(float v)
        {
            this.e.minFuelFlow = this.minFuelFlow * v;
            this.e.maxFuelFlow *= this.maxFuelFlow * v;
        }

        void EngineModuleIfc.SetFuelIspMult(float v)
        {
            this.e.g = this.g * v;
        }
    }

    internal class SolverEngine : AbstractEngine, EngineModuleIfc
    {
        private static readonly string MODULENAME = "ModuleEnginesAJE";
        internal static bool IsCompatible(ModuleEngines engine)
        {
            return engine.GetType().Name.Contains(MODULENAME);
        }

        internal SolverEngine(Part part, ModuleEngines engine) : base(part, engine)
        {
            Log.Info("{0} found on part {1}, engine {2}! Add'On may not behave as intended...", MODULENAME, part.name, e.name);
        }

        void EngineModuleIfc.SetFuelFlowMult(float v)
        {
            this.e.GetType().GetField("flowMult").SetValue(this.e, v);
        }

        void EngineModuleIfc.SetFuelIspMult(float v)
        {
            this.e.GetType().GetField("ispMult").SetValue(this.e, v);
        }
    }

    internal class StockEngine : AbstractEngine, EngineModuleIfc
    {
        StockEngine(Part part, ModuleEngines engine) : base(part, engine)
        {
            Log.Info("Stock Engine found on part {0}, engine {1}! Add'On may not behave as intended...", part.name, e.name);
        }

        void EngineModuleIfc.SetFuelFlowMult(float v)
        {
            this.e.minFuelFlow = this.minFuelFlow * v;
            this.e.maxFuelFlow *= this.maxFuelFlow * v;
        }

        void EngineModuleIfc.SetFuelIspMult(float v)
        {
            this.e.g = this.g * v;
        }
    }

    internal class EngineModuleWrapper : EngineModuleIfc
    {
        private readonly Part myPart;
        private readonly EngineModuleIfc engine = null;

        internal static EngineModuleIfc getInstance(Part part, string engineID)
        {
            foreach (PartModule pm in part.Modules)
            {
                ModuleEngines e = pm as ModuleEngines;
                if (null == e) break;
                if (string.IsNullOrEmpty(engineID) || engineID.ToLowerInvariant() == e.engineID.ToLowerInvariant())
                {
                    if (RealFuelsEngine.IsCompatible(e))    return new EngineModuleWrapper(part, new RealFuelsEngine(part, e));
                    if (SolverEngine.IsCompatible(e))       return new EngineModuleWrapper(part, new SolverEngine(part, e));
                }
            }
            return new EngineModuleWrapper(part, new UnknownEngine(part));
        }

        private EngineModuleWrapper(Part myPart, EngineModuleIfc engine)
        {
            this.myPart = myPart;
            this.engine = engine;
        }

        void EngineModuleIfc.SetFuelFlowMult(float v)
        {
            this.engine.SetFuelFlowMult(v);
        }

        void EngineModuleIfc.SetFuelIspMult(float v)
        {
            this.engine.SetFuelIspMult(v);
        }
    }

}
