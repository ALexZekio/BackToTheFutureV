﻿using BackToTheFutureV.TimeMachineClasses;
using FusionLibrary;
using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.Utility
{
    public static class WaybackMachineHandler
    {
        public static List<WaybackMachine> WaybackMachines { get; } = new List<WaybackMachine>();

        public static bool Enabled { get; set; } = false;

        public static void SetState(bool enabled)
        {
            if (!enabled)
                Abort();

            Enabled = enabled;
        }

        public static void Abort()
        {
            WaybackMachines.ForEach(x => x.Abort());
            WaybackMachines.Clear();
        }

        public static void Stop()
        {
            WaybackMachines.ForEach(x => x.Stop());
        }

        public static WaybackMachine Start(TimeMachine timeMachine)
        {
            if (!Enabled)
                return null;

            WaybackMachine waybackMachine = Script.InstantiateScript<WaybackMachine>();

            waybackMachine.Create(timeMachine);

            return waybackMachine;
        }

        public static WaybackMachine CheckIfExists(TimeMachine timeMachine)
        {
            if (!Enabled)
                return null;

            WaybackMachine waybackMachine = WaybackMachines.FirstOrDefault(x => x.GUID == timeMachine.Properties.GUID && Utils.CurrentTime >= x.StartTime && Utils.CurrentTime <= x.EndTime);

            if (waybackMachine == default)
                return Start(timeMachine);
            else
                waybackMachine.TimeMachine = timeMachine;
            
            return waybackMachine;
        }
    }
}
