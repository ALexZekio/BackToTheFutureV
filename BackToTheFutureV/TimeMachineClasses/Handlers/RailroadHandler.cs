﻿using System;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Collections.Generic;
using System.Windows.Forms;
using BackToTheFutureV.Story;
using BackToTheFutureV.Vehicles;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class RailroadHandler : Handler
    {
        public CustomTrain customTrain;

        private bool _direction = false;

        private bool _isReentryOn = false;

        private int _attachDelay;

        private float _speed;

        public RailroadHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            Events.SetWheelie += SetWheelie;
            Events.OnTimeTravelStarted += OnTimeTravelStarted;
            Events.OnReenterCompleted += OnReenterCompleted;
            Events.SetStopTracks += Stop;
        }

        public void SetWheelie(bool goUp)
        {
            customTrain?.SetWheelie?.Invoke(goUp);
        }

        public void OnTimeTravelStarted()
        {
            Properties.WasOnTracks = Properties.IsOnTracks;

            if (!Properties.IsOnTracks)
                return;

            if (customTrain.IsRogersSierra)
            {
                if (Properties.TimeTravelType == TimeTravelType.Instant && customTrain.RogersSierra.isOnTrainMission)
                    MissionHandler.TrainMission.End();

                Stop();                
                return;
            }
                
            switch (Properties.TimeTravelType)
            {
                case TimeTravelType.Cutscene:
                    customTrain.SpeedMPH = 0;
                    break;
                case TimeTravelType.RC:
                    Stop();
                    break;
            }
        }

        public void OnReenterCompleted()
        {
            if (!Properties.WasOnTracks)
                return;

            _isReentryOn = true;

            if (customTrain == null || !customTrain.Exists)
                Start();

            switch (Properties.TimeTravelType)
            {
                case TimeTravelType.Instant:
                    if (customTrain.SpeedMPH == 0)
                        customTrain.SpeedMPH = 88;
                    break;
                case TimeTravelType.Cutscene:
                    customTrain.SpeedMPH = 88;
                    break;                
                case TimeTravelType.RC:
                    Start();
                    break;
            }            
        }

        public void Start()
        {
            _speed = Vehicle.Speed;

            customTrain = CustomTrainHandler.CreateInvisibleTrain(Vehicle, _direction);

            if (!(customTrain.Train.Heading >= Vehicle.Heading - 25 && customTrain.Train.Heading <= Vehicle.Heading + 25))
            {
                _direction = !_direction;
                customTrain.DeleteTrain();
                customTrain = CustomTrainHandler.CreateInvisibleTrain(Vehicle, _direction);
            }

            customTrain.IsAccelerationOn = true;
            customTrain.IsAutomaticBrakeOn = true;

            customTrain.SetCollision(false);

            customTrain.SetToAttach(Vehicle, new Vector3(0, 4.5f, Vehicle.HeightAboveGround - customTrain.Carriage(1).HeightAboveGround - 0.05f), 1, 0); //new Vector3(0, 4.48f, 0)

            customTrain.SetPosition(Vehicle.Position);
            
            customTrain.OnVehicleAttached += customTrain_OnVehicleAttached;
            customTrain.OnTrainDeleted += customTrain_OnTrainDeleted;            
        }

        private void customTrain_OnTrainDeleted()
        {            
            customTrain.OnTrainDeleted -= customTrain_OnTrainDeleted;
            customTrain.OnVehicleAttached -= customTrain_OnVehicleAttached;            
            customTrain = null;

            if (Properties.IsAttachedToRogersSierra)
                Start();
            else
                Stop();
        }

        private void customTrain_OnVehicleAttached(bool toRogersSierra = false)
        {
            customTrain.DisableToDestroy();

            customTrain.CruiseSpeedMPH = 0;
            customTrain.SpeedMPH = 0;
            customTrain.IsAutomaticBrakeOn = true;

            Properties.IsOnTracks = true;

            if (_isReentryOn && Properties.TimeTravelType == TimeTravelType.RC)
                customTrain.SpeedMPH = 88;
            else
                customTrain.Speed = Vehicle.IsGoingForward() ? _speed : -_speed;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyDown(Keys key)
        {
            if (key == Keys.N)
            {
                if (MissionHandler.TrainMission.IsPlaying)
                    MissionHandler.TrainMission.End();
                else if (Main.PlayerVehicle == Vehicle)
                    MissionHandler.TrainMission.Start();
            }
        }

        public override void Process()
        {
            if (Mods.Wheel != WheelType.RailroadInvisible)
            {
                if (Properties.IsOnTracks)
                    Stop();

                return;
            }

            if (Properties.IsOnTracks)
            {
                Properties.IsAttachedToRogersSierra = customTrain.IsRogersSierra;

                if (Properties.IsAttachedToRogersSierra)
                {
                    if (Main.PlayerVehicle == Vehicle && Game.IsControlPressed(GTA.Control.VehicleAccelerate))
                    {
                        customTrain.SwitchToRegular();
                        return;
                    }                     

                    if (customTrain.RogersSierra.Locomotive.Speed > 0 && Utils.EntitySpeedVector(customTrain.RogersSierra.Locomotive).Y < 0)
                        customTrain.SwitchToRegular();

                    return;
                }
                else
                    customTrain.IsAccelerationOn = Main.PlayerVehicle == Vehicle && Vehicle.IsVisible && Vehicle.IsEngineRunning;

                if (Main.PlayerVehicle == Vehicle)
                    Function.Call(Hash.DISABLE_CONTROL_ACTION, 27, 59, true);

                if (_isReentryOn && customTrain.AttachedToTarget && customTrain.SpeedMPH == 0)
                {
                    if (Utils.Random.NextDouble() <= 0.25f)
                        CustomTrainHandler.CreateFreightTrain(Vehicle, !_direction).SetToDestroy(Vehicle, 35);

                    _isReentryOn = false;
                    return;
                }

                Vehicle _train = World.GetClosestVehicle(Vehicle.Position, 25, ModelHandler.FreightModel, ModelHandler.FreightCarModel, ModelHandler.TankerCarModel);

                if (_train != null && Vehicle.IsTouching(_train))
                {
                    Stop();

                    if (_train.Velocity.Y == Vehicle.Velocity.Y)
                    {
                        if (Math.Abs(_train.GetMPHSpeed() + Vehicle.GetMPHSpeed()) > 35)
                            Vehicle.Explode();
                    } else
                        if (Math.Abs(_train.GetMPHSpeed() - Vehicle.GetMPHSpeed()) > 35)
                            Vehicle.Explode();

                    _attachDelay = Game.GameTime + 3000;
                }
                
                return;
            }

            if (_attachDelay < Game.GameTime && Mods.Wheel == WheelType.RailroadInvisible && !Properties.IsFlying)
            {                
                if (Utils.GetWheelsPositions(Vehicle).TrueForAll(x => Utils.IsWheelOnTracks(x, Vehicle)))
                {
                    customTrain?.DeleteTrain();
                    Start();
                }                    
            }
        }

        public override void Stop()
        {
            Stop(0);
        }

        public void Stop(int delay = 0)
        {
            _isReentryOn = false;
            Properties.IsOnTracks = false;

            if (delay > 0)
                _attachDelay = Game.GameTime + delay;

            if (customTrain != null)
            {
                if (customTrain.Exists && customTrain.AttachedToTarget)
                    customTrain.DetachTargetVehicle();

                customTrain.OnVehicleAttached -= customTrain_OnVehicleAttached;
                customTrain.OnTrainDeleted -= customTrain_OnTrainDeleted;                

                if (customTrain.Exists && !customTrain.IsRogersSierra)
                    customTrain.DeleteTrain();

                customTrain = null;
            }
        }
    }
}
