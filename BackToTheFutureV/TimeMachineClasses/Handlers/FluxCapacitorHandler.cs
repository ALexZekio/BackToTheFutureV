﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using BackToTheFutureV.TimeMachineClasses.Handlers;
using BackToTheFutureV.TimeMachineClasses;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class FluxCapacitorHandler : Handler
    {
        public FluxCapacitorHandler(TimeMachine timeMachine) : base(timeMachine)
        {           
            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
            Events.OnScaleformPriority += OnScaleformPriority;

            Events.OnWormholeStarted += StartTimeTravelEffect;

            Events.OnTimeTravelCompleted += StartNormalFluxing;
            Events.OnTimeTravelInterrupted += StartNormalFluxing;
        }

        public void StartTimeTravelEffect()
        {
            Scaleforms.FluxCapacitor.CallFunction("START_BLUE_ANIMATION");
            Properties.IsFluxDoingBlueAnim = true;
        }

        public void StartNormalFluxing()
        {
            Scaleforms.FluxCapacitor.CallFunction("START_ANIMATION");
            Properties.IsFluxDoingBlueAnim = false;
        }

        private void OnScaleformPriority()
        {
            Update();
        }

        private void OnTimeCircuitsToggle()
        {
            if (Properties.IsGivenScaleformPriority)
                Update();
        }

        public override void Process()
        {
            if(!Vehicle.IsVisible)
                return;

            if (!Properties.AreTimeCircuitsOn)
                return;

            if (Properties.IsGivenScaleformPriority)
                Scaleforms.FluxCapacitorRT.Draw();

            if (ModSettings.PlayFluxCapacitorSound)
            {
                if (!Vehicle.IsVisible && Sounds.FluxCapacitor.IsAnyInstancePlaying)
                    Sounds.FluxCapacitor?.Stop();

                if (!Sounds.FluxCapacitor.IsAnyInstancePlaying && Vehicle.IsVisible)
                    Sounds.FluxCapacitor.Play();
            }
            else if (Sounds.FluxCapacitor.IsAnyInstancePlaying)
                Sounds.FluxCapacitor?.Stop();
        }

        public void Update()
        {
            if (!Properties.AreTimeCircuitsOn)
            {
                Scaleforms.FluxCapacitor.CallFunction("STOP_ANIMATION");

                if (Sounds.FluxCapacitor.IsAnyInstancePlaying)
                    Sounds.FluxCapacitor.Stop();
            }                
            else
            {
                if (ModSettings.PlayFluxCapacitorSound && !Sounds.FluxCapacitor.IsAnyInstancePlaying)
                    Sounds.FluxCapacitor.Play();

                Scaleforms.FluxCapacitor.CallFunction("START_ANIMATION");
            }
            

            Properties.IsFluxDoingBlueAnim = false;
            Properties.PhotoFluxCapacitorActive = false;
        }

        public override void Stop()
        {

        }

        public override void Dispose()
        {
            
        }

        public override void KeyDown(Keys key)
        {

        }
    }
}
