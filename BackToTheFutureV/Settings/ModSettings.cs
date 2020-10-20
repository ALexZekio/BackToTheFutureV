﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using GTA;
using GTA.Math;
using System.Globalization;
using BackToTheFutureV.GUI;
using BackToTheFutureV.TimeMachineClasses.RC;
using BackToTheFutureV.TimeMachineClasses;
using System.Net;
using BackToTheFutureV.Utility;
using System.Runtime.InteropServices.WindowsRuntime;
using GTA.NaturalMotion;
using System.Web;
using BackToTheFutureV.Settings;

namespace BackToTheFutureV
{
    public delegate void OnGUIChange();

    public class ModSettings
    {
        private static Version LastCompatibleVersion = new Version(2, 0, 0, 0);
        public static OnGUIChange OnGUIChange { get; set; }

        public static PointF TCDPosition { get; set; } = new PointF(0.88f, 0.75f);
        public static float TCDScale { get; set; } = 0.3f;
        public static TCDBackground TCDBackground { get; set; } = TCDBackground.Metal;        
        public static bool PlayFluxCapacitorSound { get; set; } = true;
        public static bool PlayDiodeBeep { get; set; } = true;
        public static bool PlaySpeedoBeep { get; set; } = true;
        public static bool PlayEngineSounds { get; set; } = true;
        public static bool CinematicSpawn { get; set; } = true;
        public static bool UseInputToggle { get; set; } = false;
        public static bool ForceFlyMode { get; set; } = true;
        public static bool GlowingWormholeEmitter { get; set; } = true;
        public static bool GlowingPlutoniumReactor { get; set; } = true;
        public static bool LightningStrikeEvent { get; set; } = true;
        public static bool EngineStallEvent { get; set; } = true;
        public static bool TurbulenceEvent { get; set; } = true;
        public static bool TerroristsEvent { get; set; } = true;
        public static bool LandingSystem { get; set; } = true;
        public static bool PersistenceSystem { get; set; } = false;
        public static bool RandomTrains { get; set; } = true;

        private static ScriptSettings settings;
        private static CultureInfo info = CultureInfo.CreateSpecificCulture("en-US");

        public static void LoadSettings()
        {
            string path = "./scripts/BackToTheFutureV/settings.ini";

            settings = ScriptSettings.Load(path);

            string savedStringVersion = settings.GetValue<string>("General", "Version", default);

            Version savedVersion = savedStringVersion == default ? null : new Version(savedStringVersion);

            if (savedStringVersion == default || savedVersion < LastCompatibleVersion)
            {
                RemoteTimeMachineHandler.DeleteAll();
                TimeMachineCloneManager.Delete();
                TimeMachineClone.DeleteAll();

                File.Delete(path);

                settings = ScriptSettings.Load(path);

                settings.SetValue("General", "Version", Main.Version);

                SaveSettings();

                ModControls.LoadControls(settings);

                return;
            } 
            else if (savedVersion != Main.Version)
                settings.SetValue("General", "Version", Main.Version);
                       
            TCDScale = float.Parse(settings.GetValue("TimeCircuits", "Scale", TCDScale.ToString("G", info)), info);
            TCDPosition = new PointF(float.Parse(settings.GetValue("TimeCircuits", "PositionX", TCDPosition.X.ToString("G", info)), info), float.Parse(settings.GetValue("TimeCircuits", "PositionY", TCDPosition.Y.ToString("G", info)), info));
            TCDBackground = (TCDBackground)Enum.Parse(typeof(TCDBackground), settings.GetValue("TimeCircuits", "Background", "Metal"));
            UseInputToggle = settings.GetValue("TimeCircuits", "InputMode", UseInputToggle);
            GlowingWormholeEmitter = settings.GetValue("TimeCircuits", "GlowingWormholeEmitter", GlowingWormholeEmitter);
            GlowingPlutoniumReactor = settings.GetValue("TimeCircuits", "GlowingPlutoniumReactor", GlowingPlutoniumReactor);

            PlayFluxCapacitorSound = settings.GetValue("Sounds", "FluxCapacitor", PlayFluxCapacitorSound);
            PlayDiodeBeep = settings.GetValue("Sounds", "DiodeBeep", PlayDiodeBeep);
            PlayEngineSounds = settings.GetValue("Sounds", "CustomEngine", PlayEngineSounds);
            PlaySpeedoBeep = settings.GetValue("Sounds", "SpeedoBeep", PlaySpeedoBeep);
                       
            CinematicSpawn = settings.GetValue("General", "CinematicSpawn", CinematicSpawn);
            PersistenceSystem = settings.GetValue("General", "PersistenceSystem", PersistenceSystem);
            RandomTrains = settings.GetValue("General", "RandomTrains", RandomTrains);

            ForceFlyMode = settings.GetValue("Hover", "ForceFly", ForceFlyMode);
            LandingSystem = settings.GetValue("Hover", "LandingSystem", LandingSystem);
                     
            LightningStrikeEvent = settings.GetValue("Events", "LightningStrike", LightningStrikeEvent);
            EngineStallEvent = settings.GetValue("Events", "EngineStall", EngineStallEvent);
            TurbulenceEvent = settings.GetValue("Events", "Turbulence", TurbulenceEvent);
            TerroristsEvent = settings.GetValue("Events", "Terrorists", TerroristsEvent);

            ModControls.LoadControls(settings);

            SaveSettings();

            OnGUIChange?.Invoke();
        }

        public static void SaveSettings()
        {
            settings.SetValue("TimeCircuits", "Scale", TCDScale.ToString("G", info));
            settings.SetValue("TimeCircuits", "PositionX", TCDPosition.X.ToString("G", info));
            settings.SetValue("TimeCircuits", "PositionY", TCDPosition.Y.ToString("G", info));            
            settings.SetValue("TimeCircuits", "Background", TCDBackground.ToString());
            settings.SetValue("TimeCircuits", "InputMode", UseInputToggle);
            settings.SetValue("TimeCircuits", "GlowingWormholeEmitter", GlowingWormholeEmitter);
            settings.SetValue("TimeCircuits", "GlowingPlutoniumReactor", GlowingPlutoniumReactor);

            settings.SetValue("Sounds", "FluxCapacitor", PlayFluxCapacitorSound);
            settings.SetValue("Sounds", "DiodeBeep", PlayDiodeBeep);            
            settings.SetValue("Sounds", "CustomEngine", PlayEngineSounds);
            settings.SetValue("Sounds", "SpeedoBeep", PlaySpeedoBeep);

            settings.SetValue("General", "PersistenceSystem", PersistenceSystem);
            settings.SetValue("General", "CinematicSpawn", CinematicSpawn);
            settings.SetValue("General", "RandomTrains", RandomTrains);

            settings.SetValue("Hover", "ForceFly", ForceFlyMode);
            settings.SetValue("Hover", "LandingSystem", LandingSystem);
           
            settings.SetValue("Events", "LightningStrike", LightningStrikeEvent);
            settings.SetValue("Events", "EngineStall", EngineStallEvent);
            settings.SetValue("Events", "Turbulence", TurbulenceEvent);
            settings.SetValue("Events", "Terrorists", TerroristsEvent);

            settings.Save();

            ModControls.SaveControls(settings);
        }
    }
}
