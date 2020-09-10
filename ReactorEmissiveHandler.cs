using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using VRageMath;
using VRage.ObjectBuilders;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Entity;

namespace PSYCHO_SuperThrusters.ThrusterEmissiveColors
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Reactor), false, "YourReactorSubtypeID")]

    public class ThrusterEmissiveColorsLogic : MyGameLogicComponent
    {
        public IMyReactor block;
        public IMyPowerProducer powerBlock;

        string EmissiveMaterialName = "Emissive";

        Color LowColor = new Color(0, 20, 255);
        Color HighColor = new Color(255, 40, 10);
        Color OffColor = new Color(0, 0, 0);

        float LowMult = 1f;
        float HighMult = 50f;

        Color ErrorColor = Color.Magenta;

        float RampUp = 0.005f;
        float RampDown = 0.001f;

        float CurrentGlow = 0f;

        // END
        // ============================================================================================================================

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            block = (IMyReactor)Entity;
            powerBlock = (IMyPowerProducer)block;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (block == null || powerBlock == null)
                return;

            NeedsUpdate = MyEntityUpdateEnum.EACH_FRAME;
        }

        public override void Close()
        {
            if (block == null)
                return;

            block = null;

            NeedsUpdate = MyEntityUpdateEnum.NONE;
        }

        Color CurrentColor = Color.Black;
        int Tick = 0;
        int TickThreshold = 10;
        // Handle dynamic color changes.
        public override void UpdateAfterSimulation()
        {
            if (block == null || block.MarkedForClose || block.Closed)
            {
                NeedsUpdate = MyEntityUpdateEnum.NONE;
                return;
            }

            var PowerPercent = powerBlock.CurrentOutput / powerBlock.MaxOutput;

            if (PowerPercent == CurrentGlow)
            {
                Tick = 0;
            }
            else
            {
                Tick++;
            }

            if (Tick > TickThreshold)
            {
                if (PowerPercent > CurrentGlow)
                {
                    CurrentGlow = MathHelper.Clamp((CurrentGlow + RampUp), 0f, 1f);
                }
                else if (PowerPercent < CurrentGlow)
                {
                    CurrentGlow = MathHelper.Clamp((CurrentGlow - RampDown), 0f, 1f);
                }
                else
                {
                    CurrentGlow = PowerPercent;
                }
            }


            Color color = ErrorColor;

            if (block.IsFunctional)
            {
                if (block.IsWorking)
                {
                    if (block.Enabled)
                    {
                        // When everything's dandy.
                        color = CurrentColor = Color.Lerp(LowColor, HighColor, CurrentGlow);
                    }
                    else
                    {
                        // When block is OFF.
                        color = Color.Lerp(OffColor, CurrentColor, CurrentGlow);
                    }
                }
                else
                {
                    // When block isn't working, missing power etc.
                    color = Color.Lerp(OffColor, CurrentColor, CurrentGlow);
                }
            }
            else
            {
                // When block is broken, grinded or damaged.
                color = Color.Lerp(OffColor, CurrentColor, CurrentGlow);
            }

            float mult = (LowMult + (HighMult - LowMult)) * CurrentGlow;

            block.SetEmissiveParts(EmissiveMaterialName, color, mult);
        }
    }
}
