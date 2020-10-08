﻿using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;

namespace BackToTheFutureV.TimeMachineClasses
{
    public class StoryTimeMachine
    {
        public Vector3 Position { get; }
        public float Heading { get; }
        public WormholeType WormholeType { get; }
        public bool Broken { get; }
        public DateTime SpawnDate { get; }
        public DateTime DeleteDate { get; }
        public bool IsInvincible { get; }

        public TimeMachine TimeMachine { get; private set; }
        public bool Spawned => TimeMachineHandler.Exists(TimeMachine);
        public bool IsUsed { get; private set; }

        public StoryTimeMachine(Vector3 position, float heading, WormholeType wormholeType, bool broken, DateTime spawnDate, DateTime deleteDate, bool isInvincible)
        {
            Position = position;
            Heading = heading;
            WormholeType = wormholeType;
            Broken = broken;
            SpawnDate = spawnDate;
            DeleteDate = deleteDate;
            IsInvincible = isInvincible;
        }

        public TimeMachine Spawn()
        {
            TimeMachine = TimeMachineHandler.CreateTimeMachine(Position, Heading, WormholeType);

            if (Broken)
                TimeMachine.Break();

            TimeMachine.Vehicle.IsInvincible = IsInvincible;
            
            TimeMachineHandler.AddStory(TimeMachine);

            return TimeMachine;
        }

        public bool Exists(DateTime time)
        {
            return time >= SpawnDate && time <= DeleteDate;
        }

        public void Process()
        {
            if (!Spawned && IsUsed)
                IsUsed = false;

            if (Spawned && !IsUsed && Main.PlayerPed.IsInVehicle(TimeMachine))
            {
                TimeMachine.Vehicle.IsInvincible = false;
                TimeMachineHandler.RemoveStory(TimeMachine);
                IsUsed = true;
                return;
            }
                
            if (!Exists(Main.CurrentTime) && Spawned && !IsUsed)
            {
                TimeMachine.Dispose(true);
                return;
            }

            if (Exists(Main.CurrentTime) && !Spawned)
                Spawn();
        }

        public static List<StoryTimeMachine> StoryTimeMachines { get; private set; } = new List<StoryTimeMachine>();
        
        static StoryTimeMachine()
        {
            //Inside mine
            //StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(-594.14f, 2083.52f, 130.78f), 14.78f, WormholeType.BTTF2, true, new DateTime(1885, 9, 1, 0, 0, 1), new DateTime(1955, 11, 15, 23, 59, 59), true));
            //Parking lot
            StoryTimeMachines.Add(new StoryTimeMachine(new Vector3(-264.22f, -2092.08f, 26.76f), 287.57f, WormholeType.BTTF1, false, new DateTime(1985, 10, 26, 1, 15, 0), new DateTime(1985, 10, 26, 1, 35, 0), false));
        }

        public static void ProcessAll()
        {
            foreach(var x in StoryTimeMachines)
                x.Process();
        }

        public static void Abort()
        {
            foreach(var x in StoryTimeMachines)
            {
                if (x.Spawned && !x.IsUsed)
                    x.TimeMachine.Dispose();
            }
        }
    }
}
