﻿using BackToTheFutureV.Entities;
using BackToTheFutureV.Utility;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers
{
    public class PropsHandler : Handler
    {
        private AnimateProp BTTFDecals;

        //Hover Mode
        public AnimateProp HoverModeWheelsGlow;
        public AnimateProp HoverModeVentsGlow;
        public List<AnimateProp> HoverModeUnderbodyLights = new List<AnimateProp>();

        //Fuel
        public AnimateProp EmptyGlowing;
        public AnimateProp EmptyOff;

        //Hoodbox
        public AnimateProp HoodboxLights;

        //Time travel
        public AnimateProp Coils;

        //Plutonium gauge
        public AnimateProp GaugeGlow;

        //Speedo
        public AnimateProp Speedo;

        //Compass
        public AnimateProp Compass;

        //TFC
        public AnimateProp TFCOn;
        public AnimateProp TFCOff;
        public AnimateProp TFCHandle;

        //Time travel
        public AnimateProp WhiteSphere;

        //Wheels
        public List<AnimateProp> RRWheels = new List<AnimateProp>();

        //TCD
        public AnimateProp TickingDiodes;
        public AnimateProp TickingDiodesOff;

        public PropsHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            //Wheels
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lf"));
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lr"));
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rf"));
            RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rr"));

            if (Vehicle.Bones["wheel_lm1"].Index > 0)
                RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lm1"));

            if (Vehicle.Bones["wheel_rm1"].Index > 0)
                RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rm1"));

            if (Vehicle.Bones["wheel_lm2"].Index > 0)
                RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_lm2"));

            if (Vehicle.Bones["wheel_rm2"].Index > 0)
                RRWheels.Add(new AnimateProp(Vehicle, ModelHandler.RRWheelProp, "wheel_rm2"));

            WhiteSphere = new AnimateProp(Vehicle, ModelHandler.WhiteSphere, Vector3.Zero, Vector3.Zero)
            {
                Duration = 0.25f
            };

            if (!Mods.IsDMC12)
                return;

            BTTFDecals = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.BTTFDecals), Vector3.Zero, Vector3.Zero);
            BTTFDecals.SpawnProp();

            //Hover Mode
            for (int i = 1; i < 6; i++)
            {
                if (!ModelHandler.UnderbodyLights.TryGetValue(i, out var model))
                    continue;

                ModelHandler.RequestModel(model);
                var prop = new AnimateProp(Vehicle, model, Vector3.Zero, Vector3.Zero);
                HoverModeUnderbodyLights.Add(prop);
            }

            HoverModeVentsGlow = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.VentGlowing), Vector3.Zero, Vector3.Zero);
            HoverModeWheelsGlow = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.HoverGlowing), Vector3.Zero, Vector3.Zero)
            {
                Duration = 1.7f
            };

            //Fuel
            EmptyGlowing = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.Empty), Vector3.Zero, Vector3.Zero);
            EmptyOff = new AnimateProp(Vehicle, ModelHandler.RequestModel(ModelHandler.EmptyOff), Vector3.Zero, Vector3.Zero);

            //Time travel
            Coils = new AnimateProp(Vehicle, ModelHandler.CoilsGlowing, Vector3.Zero, Vector3.Zero);

            //Plutonium gauge
            GaugeGlow = new AnimateProp(Vehicle, ModelHandler.GaugeGlow, Vector3.Zero, Vector3.Zero);

            //Speedo
            Speedo = new AnimateProp(Vehicle, ModelHandler.BTTFSpeedo, "bttf_speedo");
            Speedo.SpawnProp(false);

            //Compass
            Compass = new AnimateProp(Vehicle, ModelHandler.Compass, "bttf_compass");

            //TFC
            TFCOn = new AnimateProp(Vehicle, ModelHandler.TFCOn, Vector3.Zero, Vector3.Zero);
            TFCOff = new AnimateProp(Vehicle, ModelHandler.TFCOff, Vector3.Zero, Vector3.Zero);
            TFCHandle = new AnimateProp(Vehicle, ModelHandler.TFCHandle, new Vector3(-0.03805999f, -0.0819466f, 0.5508024f), Vector3.Zero);
            TFCHandle.SpawnProp(false);

            //TCD
            TickingDiodes = new AnimateProp(TimeMachine.Vehicle, ModelHandler.TickingDiodes, Vector3.Zero, Vector3.Zero);
            TickingDiodesOff = new AnimateProp(TimeMachine.Vehicle, ModelHandler.TickingDiodesOff, Vector3.Zero, Vector3.Zero);
            TickingDiodesOff.SpawnProp();
        }

        public override void Dispose()
        {
            BTTFDecals?.Dispose();

            //Hover Mode
            HoverModeWheelsGlow?.Dispose();
            HoverModeVentsGlow?.Dispose();

            foreach (var prop in HoverModeUnderbodyLights)
                prop?.Dispose();

            //Fuel
            EmptyGlowing?.DeleteProp();
            EmptyOff?.DeleteProp();

            //Hoodbox
            HoodboxLights?.DeleteProp();

            //Time travel
            Coils?.Dispose();
            WhiteSphere?.DeleteProp();

            //Plutonium gauge
            GaugeGlow?.Dispose();

            //Speedo
            Speedo?.Dispose();

            //Compass
            Compass?.Dispose();

            //TFC
            TFCOn?.Dispose();
            TFCOff?.Dispose();
            TFCHandle?.Dispose();

            //Wheels
            RRWheels?.ForEach(x => x?.Dispose());

            //TCD
            TickingDiodes?.DeleteProp();
            TickingDiodesOff?.DeleteProp();
        }

        public override void KeyDown(Keys key)
        {
            
        }

        public override void Process()
        {

        }

        public override void Stop()
        {
            
        }
    }
}
