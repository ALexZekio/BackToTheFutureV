﻿using BackToTheFutureV.Utility;
using System.Windows.Forms;
using KlangRageAudioLibrary;
using GTA;
using BackToTheFutureV.Story;
using System;

namespace BackToTheFutureV.Delorean.Handlers
{
    public class StarterHandler : Handler
    {
        private bool _isRestarting;
        private bool _isDead;
        private bool _firstTimeTravel;

        private readonly AudioPlayer _restarter;
        private int _restartAt;
        private int _nextCheck;

        private TimedEventManager timedEventManager;

        private bool _lightsOn;
        private bool _highbeamsOn;

        private float _lightsBrightness;

        private int _deloreanMaxFuelLevel = 65;

        public StarterHandler(TimeCircuits circuits) : base(circuits)
        {
            _restarter = circuits.AudioEngine.Create("bttf1/engine/restart.wav", Presets.ExteriorLoudLoop);
            _restarter.SourceBone = "engine";
            _restarter.FadeOutMultiplier = 6f;
            _restarter.FadeInMultiplier = 4f;
            _restarter.MinimumDistance = 6f;

            timedEventManager = new TimedEventManager();

            TimeCircuits.OnTimeTravelComplete += OnTimeTravelComplete;
        }

        private void OnTimeTravelComplete()
        {
            if (Mods.Reactor == ReactorType.Nuclear)
                _firstTimeTravel = true;
        }

        public override void Process()
        {
            if (_isDead)
            {
                Vehicle.FuelLevel = 0;

                if (_lightsOn)
                {
                    Vehicle.SetLightsMode(LightsMode.AlwaysOn);
                    Vehicle.SetLightsBrightness(_lightsBrightness);
                }                    
            }
                
            if (Mods.Reactor != ReactorType.Nuclear && _firstTimeTravel)
            {
                if (_isDead)
                    Stop();

                _firstTimeTravel = false;
            }               

            if (Game.GameTime < _nextCheck || !_firstTimeTravel || !Vehicle.IsVisible) return;

            if (Vehicle.Speed == 0 && !_isDead && !IsFueled)
            {
                var random = Utils.Random.NextDouble();

                if(random > 0.75)
                {
                    Vehicle.GetLightsState(out _lightsOn, out _highbeamsOn);

                    if (_highbeamsOn)
                        Vehicle.AreHighBeamsOn = false;

                    _lightsBrightness = 1;

                    timedEventManager.ClearEvents();

                    int _timeStart = 0;
                    int _timeEnd = _timeStart + 99;
                    
                    for (int i = 0; i<3; i++)
                    {
                        timedEventManager.Add(0, 0, _timeStart, 0, 0, _timeEnd);
                        timedEventManager.Last.SetFloat(1, 0.1f);
                        timedEventManager.Last.OnExecute += Last_OnExecute;

                        _timeStart = _timeEnd + 1;
                        _timeEnd = _timeStart + 99;
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        timedEventManager.Add(0, 0, _timeStart, 0, 0, _timeEnd);
                        timedEventManager.Last.SetFloat(1, 0.1f);
                        timedEventManager.Last.OnExecute += Last_OnExecute;

                        _timeStart = _timeEnd + 1;
                        _timeEnd = _timeStart + 199;
                    }

                    for (int i = 0; i < 3; i++)
                    {
                        timedEventManager.Add(0, 0, _timeStart, 0, 0, _timeEnd);
                        timedEventManager.Last.SetFloat(1, 0.1f);
                        timedEventManager.Last.OnExecute += Last_OnExecute;

                        _timeStart = _timeEnd + 1;
                        _timeEnd = _timeStart + 99;
                    }

                    _isDead = true;
                }
                else
                {
                    _nextCheck = Game.GameTime + 1000;
                    return;
                }
            }

            if (_isDead)
            {
                if((Game.IsControlPressed(GTA.Control.VehicleAccelerate) || Game.IsControlPressed(GTA.Control.VehicleBrake)) && Main.PlayerVehicle == Vehicle)
                {
                    if (timedEventManager.AllExecuted())
                        timedEventManager.ResetExecution();
                    
                    timedEventManager.RunEvents();

                    if (!_isRestarting)
                    {
                        _restarter.Play();
                        _restartAt = Game.GameTime + Utils.Random.Next(3000, 10000);
                        _isRestarting = true;
                    }

                    if (Game.GameTime > _restartAt)
                    {
                        Stop();
                        Vehicle.FuelLevel = _deloreanMaxFuelLevel;
                        Vehicle.IsEngineRunning = true;
                        _nextCheck = Game.GameTime + 10000;
                        return;
                    }
                }
                else
                {
                    _lightsBrightness = 1;

                    timedEventManager.ResetExecution();

                    _isRestarting = false;
                    _restarter.Stop();
                }
            }

            _nextCheck = Game.GameTime + 100;
        }

        private void Last_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
                _lightsBrightness = 1;

            _lightsBrightness += timedEvent.CurrentFloat;
        }

        public override void KeyPress(Keys key) {}

        public override void Stop()
        {
            _isDead = false;
            _isRestarting = false;
            Vehicle.FuelLevel = _deloreanMaxFuelLevel;
            _restarter.Stop();

            if (_lightsOn)
            {
                Vehicle.SetLightsBrightness(1);
                Vehicle.SetLightsMode(LightsMode.Default);

                Vehicle.AreHighBeamsOn = _highbeamsOn;
            }
        }

        public override void Dispose()
        {
            Vehicle.FuelLevel = _deloreanMaxFuelLevel;
            _restarter.Dispose();
        }
    }
}