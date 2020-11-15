﻿using BackToTheFutureV.Entities;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Players;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using KlangRageAudioLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RogersSierraRailway;
using static RogersSierraRailway.Commons;
using BackToTheFutureV.TimeMachineClasses;
using GTA.UI;

namespace BackToTheFutureV.Story
{
    public delegate void OnVehicleAttachedToRogersSierra(TimeMachine timeMachine);
    public delegate void OnVehicleDetachedFromRogersSierra(TimeMachine timeMachine);

    public class TrainMission : Mission
    {
        public OnVehicleAttachedToRogersSierra OnVehicleAttachedToRogersSierra;
        public OnVehicleDetachedFromRogersSierra OnVehicleDetachedFromRogersSierra;

        public TimeMachine TimeMachine { get; private set; }
        public RogersSierra RogersSierra { get; private set; }

        public float TimeMultiplier = 1f;
        public bool PlayMusic = true;
        
        public AudioPlayer MissionMusic { get; private set; }
        //private AudioPlayer _missionMusic = new AudioPlayer($"TrainMissionWithVoices.wav", false, 0.8f);

        private List<PtfxEntityBonePlayer> _wheelPtfxes = null;

        public TimedEventManager VehicleTimedEventManager = new TimedEventManager();

        public TrainMission()
        {
            OnVehicleAttachedToRogersSierra += OnVehicleAttached;
            OnVehicleDetachedFromRogersSierra += OnVehicleDetached;

            MissionMusic = Main.CommonAudioEngine.Create($"story/trainMission/music.wav", Presets.No3D);
        }

        private void OnVehicleDetached(TimeMachine timeMachine)
        {
            if (timeMachine != TimeMachine || !IsPlaying)
                return;

            if (VehicleTimedEventManager.ManageCamera)
                VehicleTimedEventManager.ResetCamera();

            VehicleTimedEventManager.ClearEvents();

            TimeMachine.Events.OnTimeTravelStarted -= StartExplodingScene;
            TimeMachine.Properties.BlockSparks = false;

            TimeMachine = null;
        }

        private void OnVehicleAttached(TimeMachine timeMachine)
        {
            if (timeMachine == null || TimeMachine != null || !IsPlaying)
                return;

            TimeMachine = timeMachine;

            TimeMachine.Events.OnTimeTravelStarted += StartExplodingScene;
            TimeMachine.Vehicle.FuelLevel = 0;
            TimeMachine.Properties.BlockSparks = true;

            VehicleTimedEventManager.ManageCamera = false;

            VehicleTimedEventManager.Add(1, 31, 386, 1, 40, 137, TimeMultiplier);
            VehicleTimedEventManager.Last.SetCamera(TimeMachine.Vehicle, new Vector3(1f, 3f, 0), TimeMachine.Vehicle, Vector3.Zero);

            VehicleTimedEventManager.Add(5, 18, 0, 5, 19, 0, TimeMultiplier); //wheelie up
            VehicleTimedEventManager.Last.OnExecute += WheelieUp_OnExecute;

            VehicleTimedEventManager.Add(5, 26, 0, 5, 27, 0, TimeMultiplier); //wheelie down
            VehicleTimedEventManager.Last.OnExecute += WheelieDown_OnExecute;

            VehicleTimedEventManager.Add(5, 27, 500, 5, 28, 500, TimeMultiplier); //add ptfx wheels on front
            VehicleTimedEventManager.Last.OnExecute += GlowingWheelsFront_OnExecute;

            VehicleTimedEventManager.Add(5, 29, 0, 5, 30, 0, TimeMultiplier); //delete wheelie effects
            VehicleTimedEventManager.Last.OnExecute += DeleteEffects_OnExecute;

            VehicleTimedEventManager.Add(6, 55, 0, 6, 56, 0, TimeMultiplier); //start sparks
            VehicleTimedEventManager.Last.OnExecute += StartSparks_OnExecute;
        }

        private void StartSparks_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            TimeMachine.Properties.BlockSparks = false;
        }

        public override void Process()
        {
            if (!IsPlaying)
                return;

            TimedEventManager.RunEvents();

            if (TimedEventManager.AllExecuted())
                StartExplodingScene();

            if (TimeMachine != null && TimeMachine.Properties.IsAttachedToRogersSierra)
            {
                VehicleTimedEventManager.RunEvents(TimedEventManager.CurrentTime);

                if (_wheelPtfxes != null)
                    foreach (var wheelPTFX in _wheelPtfxes)
                        wheelPTFX.Process();
            }
        }

        protected override void OnEnd()
        {
            if (!IsPlaying)
                return;

            if (TimedEventManager.IsCustomCameraActive)
                TimedEventManager.ResetCamera();

            if (MissionMusic.IsAnyInstancePlaying)
                MissionMusic.Stop();

            _wheelPtfxes?.ForEach(x => x?.Stop());
            _wheelPtfxes.Clear();

            RogersSierra.IsOnTrainMission = false;

            if (RogersSierra.IsExploded == false)
                RogersSierra.FunnelSmoke = SmokeColor.Default;

            if (TimeMachine != null)
                OnVehicleDetached(TimeMachine);

            RogersSierra = null;            
        }

        public void StartExplodingScene()
        {
            TimedEventManager.ClearEvents();

            TimedEventManager.Add(TimedEventManager.CurrentTime.Add(TimeSpan.FromSeconds(2)), TimedEventManager.CurrentTime.Add(TimeSpan.FromSeconds(3)));
            TimedEventManager.Last.OnExecute += TrainExplosion_OnExecute;
        }

        private void TrainExplosion_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution)
                return;

            RogersSierra.Explode();
            End();
        }

        protected override void OnStart()
        {
            if (!(RogersSierra is null))
                return;

            RogersSierra = TrainManager.ClosestRogersSierra;

            if (RogersSierra is null || RogersSierra.IsExploded)
            {
                RogersSierra = null;
                IsPlaying = false;
                return;
            }

            TimedEventManager.ResetExecution();
            TimedEventManager.ClearEvents();

            TimedEventManager.ManageCamera = false;

            TimedEventManager.Add(0, 0, 0, 0, 10, 0, TimeMultiplier);
            TimedEventManager.Last.SetCamera(RogersSierra.Locomotive, new Vector3(2.5f, 13f, -1f), RogersSierra.Locomotive, Vector3.Zero);

            TimedEventManager.Add(0, 0, 0, 0, 25, 0, TimeMultiplier); //reach 25mph
            TimedEventManager.Last.SetSpeed(0, 25);            
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(1, 29, 386, 1, 31, 386, TimeMultiplier);
            TimedEventManager.Last.SetCamera(RogersSierra.Locomotive, new Vector3(0, 15f, 2f), RogersSierra.Locomotive, Vector3.Zero);

            TimedEventManager.Add(1, 29, 386, 1, 40, 137, TimeMultiplier); //green log explosion and reach 35mph
            TimedEventManager.Last.SetSpeed(25, 35);
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(2, 16, 266, 2, 25, 364, TimeMultiplier); //yellow log explosion and reach 40mph
            TimedEventManager.Last.SetSpeed(35, 40);
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(2, 25, 364, 3, 3, 406, TimeMultiplier); //reach 45mph
            TimedEventManager.Last.SetSpeed(40, 45);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 3, 406, 3, 16, 710, TimeMultiplier); //reach 50mph
            TimedEventManager.Last.SetSpeed(45, 50);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 20, 321, 3, 21, 587, TimeMultiplier); //first whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 21, 87, 3, 21, 587, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 21, 587, 3, 23, 315, TimeMultiplier); //second whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 22, 815, 3, 23, 315, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 23, 315, 3, 24, 511, TimeMultiplier); //third whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;
            TimedEventManager.Add(3, 24, 11, 3, 24, 511, TimeMultiplier); //stop whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 24, 511, 3, 25, 529, TimeMultiplier); //fourth whistle
            TimedEventManager.Last.OnExecute += Whistle_OnExecute;

            TimedEventManager.Add(3, 25, 529, 3, 47, 111, TimeMultiplier); //reach 55mph
            TimedEventManager.Last.SetSpeed(50, 55);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 47, 111, 4, 26, 239, TimeMultiplier); //reach 60mph
            TimedEventManager.Last.SetSpeed(55, 60);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(3, 47, 111, 4, 26, 239, TimeMultiplier); //reach 68mph
            TimedEventManager.Last.SetSpeed(60, 68);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(4, 44, 0, 4, 46, 0, TimeMultiplier); //reach 70mph
            TimedEventManager.Last.SetSpeed(68, 70);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 3, 239, 5, 4, 395, TimeMultiplier); //reach 72mph
            TimedEventManager.Last.SetSpeed(70, 72);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 10, 603, 5, 13, 603, TimeMultiplier); //red log explosion            
            TimedEventManager.Last.OnExecute += Explosion_OnExecute;

            TimedEventManager.Add(5, 19, 0, 5, 21, 0, TimeMultiplier); //reach 75mph
            TimedEventManager.Last.SetSpeed(72, 75);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 21, 100, 5, 54, 500, TimeMultiplier); //reach 79mph
            TimedEventManager.Last.SetSpeed(75, 79);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 54, 100, 5, 55, 100, TimeMultiplier); //reach 80mph
            TimedEventManager.Last.SetSpeed(79, 80);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(5, 56, 100, 5, 57, 100, TimeMultiplier); //reach 81mph
            TimedEventManager.Last.SetSpeed(80, 81);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(6, 8, 0, 6, 9, 0, TimeMultiplier); //reach 82mph
            TimedEventManager.Last.SetSpeed(81, 82);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(6, 10, 0, 6, 57, 0, TimeMultiplier); //reach 84mph
            TimedEventManager.Last.SetSpeed(82, 84);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(6, 58, 0, 7, 0, 0, TimeMultiplier); //reach 86mph
            TimedEventManager.Last.SetSpeed(84, 86);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(7, 1, 0, 7, 3, 0, TimeMultiplier); //reach 87mph
            TimedEventManager.Last.SetSpeed(86, 87);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(7, 3, 500, 7, 4, 500, TimeMultiplier); //reach 88mph
            TimedEventManager.Last.SetSpeed(87, 88);
            TimedEventManager.Last.OnExecute += SetSpeed_OnExecute;

            TimedEventManager.Add(7, 6, 0, 9, 12, 627, TimeMultiplier);

            RogersSierra.IsOnTrainMission = true;
            RogersSierra.RandomTrain = false;
            RogersSierra.UnlockSpeed = true;

            _wheelPtfxes = new List<PtfxEntityBonePlayer>();
            
            if (RogersSierra.AttachedVehicle != null)
                OnVehicleAttached(TimeMachineHandler.GetTimeMachineFromVehicle(RogersSierra.AttachedVehicle));

            if (PlayMusic)
            {
                MissionMusic.SourceEntity = Main.PlayerPed;                
                MissionMusic.Play();
            }                
        }

        private void DeleteEffects_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
                _wheelPtfxes?.ForEach(x => x?.Stop());
        }

        private void GlowingWheelsFront_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {
                SetupFrontWheelsPTFXs("des_bigjobdrill", "ent_ray_big_drill_start_sparks", new Vector3(0, -0.3f, 0), new Vector3(0, 90f, 0), 1f, true);
                SetupFrontWheelsPTFXs("veh_impexp_rocket", "veh_rocket_boost", new Vector3(0.2f, 0, 0), new Vector3(0, 0, 90f), 2.5f);
            }                
        }

        private void WheelieDown_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
                TimeMachine.Events.SetWheelie?.Invoke(false);
        }

        private void WheelieUp_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {                
                TimeMachine.Events.SetWheelie?.Invoke(true);
                SetupRearWheelsPTFXs("des_bigjobdrill", "ent_ray_big_drill_start_sparks", new Vector3(0, -0.3f, 0), new Vector3(0, 90f, 0), 1f, true);
                SetupRearWheelsPTFXs("veh_impexp_rocket", "veh_rocket_boost", new Vector3(0.2f, 0, 0), new Vector3(0, 0, 90f), 2.5f);
            }                
        }

        private void Whistle_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
                RogersSierra.Whistle = !RogersSierra.Whistle;
        }

        private void SetSpeed_OnExecute(TimedEvent timedEvent)
        {
            if (RogersSierra.Locomotive.GetMPHSpeed() <= timedEvent.EndSpeed)
                RogersSierra.LocomotiveSpeed += Convert.ToSingle(timedEvent.CurrentSpeed);
        }

        private void Explosion_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {                               
                switch (RogersSierra.FunnelSmoke)
                {
                    case SmokeColor.Default:
                        RogersSierra.PrestoLogExplosion(SmokeColor.Green);
                        break;
                    case SmokeColor.Green:
                        RogersSierra.PrestoLogExplosion(SmokeColor.Yellow);
                        break;
                    case SmokeColor.Yellow:
                        RogersSierra.PrestoLogExplosion(SmokeColor.Red);
                        break;
                }
            }

            if (RogersSierra.Locomotive.GetMPHSpeed() <= timedEvent.EndSpeed)
                RogersSierra.LocomotiveSpeed += Convert.ToSingle(timedEvent.CurrentSpeed);
        }

        private void SetupRearWheelsPTFXs(string particleAssetName, string particleName, Vector3 wheelOffset, Vector3 wheelRot, float size = 3f, bool doLoopHandling = false)
        {
            var ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, TimeMachine, "wheel_lr", wheelOffset, wheelRot, size, true, doLoopHandling);

            ptfx.Play();

            if (particleName == "veh_rocket_boost")
            {
                ptfx.SetEvolutionParam("boost", 0f);
                ptfx.SetEvolutionParam("charge", 1f);
            }

            _wheelPtfxes.Add(ptfx);

            ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, TimeMachine, "wheel_rr", wheelOffset, wheelRot, size, true, doLoopHandling);

            ptfx.Play();

            if (particleName == "veh_rocket_boost")
            {
                ptfx.SetEvolutionParam("boost", 0f);
                ptfx.SetEvolutionParam("charge", 1f);
            }

            _wheelPtfxes.Add(ptfx);
        }

        private void SetupFrontWheelsPTFXs(string particleAssetName, string particleName, Vector3 wheelOffset, Vector3 wheelRot, float size = 3f, bool doLoopHandling = false)
        {
            var ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, TimeMachine, "wheel_lf", wheelOffset, wheelRot, size, true, doLoopHandling);

            ptfx.Play();

            if (particleName == "veh_rocket_boost")
            {
                ptfx.SetEvolutionParam("boost", 0f);
                ptfx.SetEvolutionParam("charge", 1f);
            }

            _wheelPtfxes.Add(ptfx);

            ptfx = new PtfxEntityBonePlayer(particleAssetName, particleName, TimeMachine, "wheel_rf", wheelOffset, wheelRot, size, true, doLoopHandling);

            ptfx.Play();

            if (particleName == "veh_rocket_boost")
            {
                ptfx.SetEvolutionParam("boost", 0f);
                ptfx.SetEvolutionParam("charge", 1f);
            }

            _wheelPtfxes.Add(ptfx);
        }

        public override void KeyDown(KeyEventArgs key)
        {
            
        }
    }
}
