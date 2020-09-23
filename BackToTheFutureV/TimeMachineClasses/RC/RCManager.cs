﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.UI;
using System.Drawing;
using LemonUI;
using System.Windows.Forms;
using Screen = GTA.UI.Screen;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.TimeMachineClasses.Handlers;
using LemonUI.TimerBars;

namespace BackToTheFutureV.TimeMachineClasses.RC
{
    public class RCManager
    {
        public static readonly float MAX_DIST = 650f;

        public static TimeMachine RemoteControlling { get; private set; }

        private static TimerBarProgress _signalBar = new TimerBarProgress(Game.GetLocalizedString("BTTFV_RC_Signal"));

        public static void RemoteControl(TimeMachine timeMachine)
        {
            if (timeMachine == null)
                return;

            if (RemoteControlling != null)
                RemoteControlling.Events.SetRCMode?.Invoke(false);

            timeMachine.Events.SetRCMode?.Invoke(true);
            RemoteControlling = timeMachine;

            Main.DisablePlayerSwitching = true;

            _signalBar.Recalculate(new PointF(0, 0));
        }

        public static void Process()
        {
            if (RemoteControlling == null || !RemoteControlling.Properties.IsRemoteControlled) return;

            float squareDist = RemoteControlling.Vehicle.Position.DistanceToSquared(RemoteControlling.OriginalPed.Position);
            float percentage = ((MAX_DIST * MAX_DIST - squareDist) / (MAX_DIST * MAX_DIST))*100;

            _signalBar.Progress = percentage;            
            _signalBar.Draw();

            if (squareDist > MAX_DIST * MAX_DIST)
                StopRemoteControl();
        }

        public static void StopRemoteControl(bool instant = false)
        {            
            RemoteControlling.Events.SetRCMode?.Invoke(false, instant);
            RemoteControlling = null;

            Main.DisablePlayerSwitching = false;
        }

        public static void KeyDown(Keys key) {}
    }
}