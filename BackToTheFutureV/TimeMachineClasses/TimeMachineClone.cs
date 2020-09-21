﻿using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackToTheFutureV.TimeMachineClasses
{
    [Serializable]
    public class TimeMachineCloneManager
    {
        public List<TimeMachineClone> timeMachineClones = new List<TimeMachineClone>();

        public TimeMachineCloneManager(List<TimeMachine> timeMachines)
        {
            foreach (var x in timeMachines)
                timeMachineClones.Add(x.Clone);
        }

        public void SpawnAll()
        {
            foreach (var x in timeMachineClones)
                x.Spawn();
        }

        private static string _saveFile = "./scripts/BackToTheFutureV/TimeMachines.dmc12";
        private static IFormatter formatter = new BinaryFormatter();

        public static void Delete()
        {
            if (File.Exists(_saveFile))
                File.Delete(_saveFile);
        }

        public static void Save(List<TimeMachine> timeMachines)
        {            
            Stream stream = new FileStream(_saveFile, FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, new TimeMachineCloneManager(timeMachines));
            stream.Close();
        }

        public static TimeMachineCloneManager Load()
        {
            if (!File.Exists(_saveFile))
                return null;

            Stream stream = new FileStream(_saveFile, FileMode.Open, FileAccess.Read);

            TimeMachineCloneManager timeMachineCloneManager  = (TimeMachineCloneManager)formatter.Deserialize(stream);

            stream.Close();

            return timeMachineCloneManager;
        }
    }

    [Serializable]
    public class TimeMachineClone
    {
        public BaseMods Mods { get; }
        public BaseProperties Properties { get; }
        public VehicleInfo Vehicle { get; }

        public TimeMachineClone(TimeMachine timeMachine)
        {
            Mods = timeMachine.Mods.Clone();
            Properties = timeMachine.Properties.Clone();
            Vehicle = new VehicleInfo(timeMachine.Vehicle);
        }

        public TimeMachine Spawn(bool asNew = false, bool reenter = false, bool rc = false)
        {
            bool _isNew = false;

            Vehicle veh = null;

            if (!rc && !asNew)
                veh = World.GetClosestVehicle(Vehicle.Position, 1.0f, Vehicle.Model);

            if (veh == null)
            {
                ModelHandler.RequestModel(Vehicle.Model);
                veh = World.CreateVehicle(Vehicle.Model, Vehicle.Position, Vehicle.Heading);
                _isNew = true;
            }
        
            TimeMachine timeMachine = TimeMachineHandler.CreateTimeMachine(veh, Mods.WormholeType);
               
            ApplyTo(timeMachine, asNew);

            if (_isNew)
                timeMachine.Properties.TorqueMultiplier = 1;

            if (!veh.IsOnAllWheels && veh.HeightAboveGround > 5 && timeMachine.Mods.HoverUnderbody == ModState.On)
                timeMachine.Events.SetFlyMode?.Invoke(true, true);

            if (reenter)
            {
                Utils.HideVehicle(timeMachine.Vehicle, true);

                timeMachine.Properties.DestinationTime = Main.CurrentTime;

                timeMachine.Properties.AreTimeCircuitsOn = true;
                timeMachine.Events.SetTimeCircuits?.Invoke(true);

                timeMachine.Events.OnReenter?.Invoke();
            }

            return timeMachine;
        }

        public void ApplyTo(TimeMachine timeMachine, bool asNew = false)
        {
            if (!Mods.IsDMC12)
                timeMachine.Vehicle.Mods.InstallModKit();

            Vehicle.ApplyTo(timeMachine.Vehicle);
            Mods.ApplyTo(timeMachine);
            
            if (!asNew)
                Properties.ApplyTo(timeMachine);
        }

        public void Save(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
                name = name + ".dmc12";

            name = RemoveIllegalFileNameChars(name);

            IFormatter formatter = new BinaryFormatter();

            if (!Directory.Exists(PresetsPath))
                Directory.CreateDirectory(PresetsPath);

            Stream stream = new FileStream($"{PresetsPath}/{name}", FileMode.Create, FileAccess.Write);

            formatter.Serialize(stream, this);
            stream.Close();
        }

        public static string PresetsPath = "./scripts/BackToTheFutureV/presets";

        public static bool PresetExists(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
                name = name + ".dmc12";

            return File.Exists($"{PresetsPath}/{name}");
        }

        public static List<string> ListPresets()
        {
            if (!Directory.Exists(PresetsPath))
                Directory.CreateDirectory(PresetsPath);

            return new DirectoryInfo(PresetsPath).GetFiles("*.dmc12").Select(x => x.Name.Replace(".dmc12", "")).ToList();
        }

        public static void DeleteSave(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
                name = name + ".dmc12";

            File.Delete($"{PresetsPath}/{name}");
        }

        public static void DeleteAll()
        {
            if (!Directory.Exists(PresetsPath))
                return;

            new DirectoryInfo(PresetsPath).GetFiles("*.dmc12").ToList().ForEach(x => x.Delete());
        }

        public static void RenameSave(string name, string newName)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
                name = name + ".dmc12";

            if (!newName.ToLower().EndsWith(".dmc12"))
                newName = newName + ".dmc12";

            File.Move($"{PresetsPath}/{name}", $"{PresetsPath}/{newName}");
        }

        private static string RemoveIllegalFileNameChars(string input, string replacement = "")
        {
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(input, replacement);
        }

        public static TimeMachineClone Load(string name)
        {
            if (!name.ToLower().EndsWith(".dmc12"))
                name = name + ".dmc12";

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream($"{PresetsPath}/{name}", FileMode.Open, FileAccess.Read);

            TimeMachineClone baseMods = (TimeMachineClone)formatter.Deserialize(stream);

            stream.Close();

            return baseMods;
        }
    }
}
