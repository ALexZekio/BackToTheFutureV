﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using System.Globalization;
using System.Drawing;
using BackToTheFutureV.Entities;
using BackToTheFutureV.GUI;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Settings;
using KlangRageAudioLibrary;
using BackToTheFutureV.Story;
using BackToTheFutureV.TimeMachineClasses;
using BackToTheFutureV.Vehicles;

namespace BackToTheFutureV.TimeMachineClasses.Handlers
{
    public class TCDSlot
    {
        private static Dictionary<string, Vector3> offsets = new Dictionary<string, Vector3>()
        {
            { "red", new Vector3(-0.01477456f, 0.3175744f, 0.6455771f) },
            { "yellow", new Vector3(-0.01964803f, 0.2769623f, 0.565388f) },
            { "green", new Vector3(-0.01737539f, 0.2979541f, 0.6045464f) }
        };

        public RenderTarget RenderTarget { get; private set; }
        public TCDRowScaleform Scaleform { get; private set; }
        public TimeCircuitsScaleform ScreenTCD { get; private set; }
        public TimeMachine TimeMachine { get; private set; }

        public string SlotType { get; private set; }

        private DateTime date;

        public bool IsDoingTimedVisible { get; private set; }
        private bool toggle;
        private int showPropsAt;
        private int showMonthAt;

        private AnimateProp amProp;
        private AnimateProp pmProp;

        public TCDSlot(string slotType, TimeCircuitsScaleform scaleform, TimeMachine timeMachine)
        {
            SlotType = slotType;
            ScreenTCD = scaleform;
            TimeMachine = timeMachine;

            if (TimeMachine.Mods.IsDMC12)
            {
                Scaleform = new TCDRowScaleform(slotType);
                RenderTarget = new RenderTarget(new Model("bttf_3d_row_" + slotType), "bttf_tcd_row_" + slotType, TimeMachine.Vehicle, offsets[slotType], new Vector3(355.9951f, 0.04288517f, 352.7451f));
                RenderTarget.CreateProp();
                Scaleform.DrawInPauseMenu = true;

                amProp = new AnimateProp(TimeMachine.Vehicle, new Model($"bttf_{slotType}_am"), Vector3.Zero, Vector3.Zero);
                pmProp = new AnimateProp(TimeMachine.Vehicle, new Model($"bttf_{slotType}_pm"), Vector3.Zero, Vector3.Zero);

                RenderTarget.OnRenderTargetDraw += OnRenderTargetDraw;
            }

            date = new DateTime();
        }

        public void SetDate(DateTime dateToSet)
        {
            if (!TcdEditer.IsEditing)
            {
                ScreenTCD.SetDate(SlotType, dateToSet);
            }

            Scaleform?.SetDate(dateToSet);
            amProp?.SetState(dateToSet.ToString("tt", CultureInfo.InvariantCulture) == "AM");
            pmProp?.SetState(dateToSet.ToString("tt", CultureInfo.InvariantCulture) != "AM");

            date = dateToSet;
            toggle = true;
        }

        public void SetVisible(bool toggleTo, bool month = true, bool day = true, bool year = true, bool hour = true, bool minute = true, bool amPm = true)
        {
            if(!TcdEditer.IsEditing)
            {
                ScreenTCD.SetVisible(SlotType, toggleTo, month, day, year, hour, minute, amPm);
            }

            Scaleform?.SetVisible(toggleTo, month, day, year, hour, minute);

            if((!toggleTo && amPm) || (toggleTo && !amPm))
            {
                amProp?.DeleteProp();
                pmProp?.DeleteProp();
            }
            else if((!toggleTo && !amPm) || (toggleTo && amPm))
            {
                amProp?.SetState(date.ToString("tt", CultureInfo.InvariantCulture) == "AM");
                pmProp?.SetState(date.ToString("tt", CultureInfo.InvariantCulture) != "AM");
            }

            toggle = toggleTo;
        }

        public void SetVisibleAt(bool toggle, int showPropsAt, int showMonthAt)
        {
            this.toggle = toggle;
            this.showPropsAt = Game.GameTime + showPropsAt;
            this.showMonthAt = Game.GameTime + showMonthAt;
            this.IsDoingTimedVisible = true;
        }

        public void Update()
        {
            if (toggle || IsDoingTimedVisible)
                RenderTarget?.Draw();

            if (!IsDoingTimedVisible)
            {
                SetVisible(toggle);

                return;
            }

            if (Game.GameTime > showPropsAt)
            {
                SetVisible(toggle, false);
            }

            if(Game.GameTime > showMonthAt)
            {
                SetVisible(toggle);
            }

            if(Game.GameTime > showPropsAt && Game.GameTime > showMonthAt)
            {
                IsDoingTimedVisible = false;
            }
        }

        public void Dispose()
        {
            RenderTarget?.Dispose();
            amProp?.DeleteProp();
            pmProp?.DeleteProp();
        }

        private void OnRenderTargetDraw()
        {
            Scaleform?.Render2D(new PointF(0.379f, 0.12f), new SizeF(0.75f, 0.27f));
        }
    }

    public class TCDHandler : Handler
    {
        //private DateTime oldDate;
        private DateTime errorDate = new DateTime(1885, 1, 1, 12, 0, 0);

        private TimedEventManager glitchEvents = new TimedEventManager();

        private bool doGlitch;
        //private int nextGlitch;
        //private int glitchCount;

        private TCDSlot destinationSlot;
        private TCDSlot presentSlot;
        private TCDSlot previousSlot;

        private bool currentState;
        private AnimateProp tickingDiodes;
        private AnimateProp tickingDiodesOff;

        private AudioPlayer fluxCapacitor;
        private AudioPlayer beep;
        private int nextTick;

        private DateTime lastTime;
        private int nextCheck;

        public bool IsDoingTimedVisible => destinationSlot.IsDoingTimedVisible;

        public TCDHandler(TimeMachine timeMachine) : base(timeMachine)
        {
            destinationSlot = new TCDSlot("red", Scaleforms.GUI, timeMachine);
            destinationSlot.SetVisible(false);

            presentSlot = new TCDSlot("green", Scaleforms.GUI, timeMachine);
            presentSlot.SetVisible(false);

            previousSlot = new TCDSlot("yellow", Scaleforms.GUI, timeMachine);
            previousSlot.SetVisible(false);

            beep = Sounds.AudioEngine.Create("general/timeCircuits/beep.wav", Presets.Interior);
            fluxCapacitor = Sounds.AudioEngine.Create("general/fluxCapacitor.wav", Presets.InteriorLoop);

            fluxCapacitor.Volume = 0.1f;

            fluxCapacitor.MinimumDistance = 0.5f;
            beep.MinimumDistance = 0.3f;
            fluxCapacitor.SourceBone = "flux_capacitor";
            beep.SourceBone = "bttf_tcd_green";

            tickingDiodes = new AnimateProp(TimeMachine.Vehicle, ModelHandler.TickingDiodes, Vector3.Zero, Vector3.Zero);
            tickingDiodesOff = new AnimateProp(TimeMachine.Vehicle, ModelHandler.TickingDiodesOff, Vector3.Zero, Vector3.Zero);
            tickingDiodesOff.SpawnProp();

            Events.OnTimeCircuitsToggle += OnTimeCircuitsToggle;
            Events.OnDestinationDateChange += OnDestinationDateChange;
            Events.OnScaleformPriority += OnScaleformPriority;

            Events.OnTimeTravelStarted += OnTimeTravel;
            Events.OnTimeTravelCompleted += OnTimeTravelComplete;

            Events.SetTimeCircuits += SetTimeCircuitsOn;
            Events.SetTimeCircuitsBroken += SetTimeCircuitsBroken;

            Events.StartTimeCircuitsGlitch += StartTimeCircuitsGlitch;

            int _time = 0;

            for (int i = 0; i < 7; i++)
            {
                glitchEvents.Add(0, 0, _time, 0, 0, _time + 499);
                glitchEvents.Last.OnExecute += Blank_OnExecute;

                _time += 500;

                glitchEvents.Add(0, 0, _time, 0, 0, _time + 199);
                glitchEvents.Last.OnExecute += RandomDate_OnExecute;

                _time += 200;

                glitchEvents.Add(0, 0, _time, 0, 0, _time + 499);
                glitchEvents.Last.OnExecute += ErrorDate_OnExecute;

                _time += 500;
            }            
        }

        private void Blank_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
                destinationSlot.SetVisible(false);
        }

        private void ErrorDate_OnExecute(TimedEvent timedEvent)
        {
            if (timedEvent.FirstExecution)
            {
                if (timedEvent.Step == glitchEvents.EventsCount - 1)
                    Properties.DestinationTime = errorDate;

                destinationSlot.SetDate(errorDate);
                destinationSlot.Update();
            }                
        }

        private void RandomDate_OnExecute(TimedEvent timedEvent)
        {
            if (!timedEvent.FirstExecution) 
                return;

            destinationSlot.SetDate(Utils.RandomDate());
            destinationSlot.Update();
        }

        private void OnScaleformPriority()
        {
            if (Properties.AreTimeCircuitsOn)
                UpdateScaleformDates();
        }

        private void OnTimeCircuitsToggle()
        {
            if (!Properties.IsGivenScaleformPriority)
                return;

            if(Properties.AreTimeCircuitsOn)
            {
                if (ModSettings.PlayFluxCapacitorSound)
                    fluxCapacitor.Play();

                destinationSlot.SetDate(Properties.DestinationTime);
                destinationSlot.SetVisible(false);
                destinationSlot.SetVisibleAt(true, 500, 600);

                previousSlot.SetDate(Properties.PreviousTime);
                previousSlot.SetVisible(false);
                previousSlot.SetVisibleAt(true, 500, 600);

                presentSlot.SetDate(Utils.GetWorldTime());
                presentSlot.SetVisible(false);
                presentSlot.SetVisibleAt(true, 500, 600);
            }
            else
            {
                if (fluxCapacitor.IsAnyInstancePlaying)
                    fluxCapacitor?.Stop();

                destinationSlot.SetVisibleAt(false, 750, 750);
                previousSlot.SetVisibleAt(false, 750, 750);
                presentSlot.SetVisibleAt(false, 750, 750);

                currentState = false;
                beep?.Stop();
                Scaleforms.GUI.CallFunction("SET_DIODE_STATE", false);
                tickingDiodes?.DeleteProp();
                tickingDiodesOff?.SpawnProp();
            }
        }

        private void OnTimeTravelComplete()
        {
            lastTime = Utils.GetWorldTime();
        }

        private void OnDestinationDateChange()
        {
            destinationSlot.SetDate(Properties.DestinationTime);
            destinationSlot.SetVisible(false);
            destinationSlot.SetVisibleAt(true, 500, 600);
        }

        private void OnTimeTravel()
        {
            previousSlot.SetDate(Properties.PreviousTime);
        }

        public void UpdateScaleformDates()
        {
            destinationSlot.SetDate(Properties.DestinationTime);
            previousSlot.SetDate(Properties.PreviousTime);
            presentSlot.SetDate(Utils.GetWorldTime());
        }

        public void StartTimeCircuitsGlitch()
        {
            glitchEvents.ResetExecution();

            doGlitch = true;
        }

        public override void Dispose()
        {
            Stop();
        }

        public override void KeyPress(Keys key)
        {
            if (Main.PlayerVehicle != Vehicle) return;

            if (key == Keys.Add && !Properties.IsRemoteControlled)
                SetTimeCircuitsOn(!Properties.AreTimeCircuitsOn);
        }

        public override void Process()
        {
            if (ModSettings.PlayFluxCapacitorSound)
            {
                if (!Vehicle.IsVisible && fluxCapacitor.IsAnyInstancePlaying)
                    fluxCapacitor?.Stop();

                if (!fluxCapacitor.IsAnyInstancePlaying && Properties.AreTimeCircuitsOn && Vehicle.IsVisible)
                    fluxCapacitor.Play();
            }
            else if (fluxCapacitor.IsAnyInstancePlaying)
                fluxCapacitor?.Stop();

            if (!Properties.IsGivenScaleformPriority || Game.Player.Character.Position.DistanceToSquared(Vehicle.Position) > 8f * 8f)
                return;

            destinationSlot.Update();
            previousSlot.Update();
            presentSlot.Update();

            DrawGUI();

            if (!Properties.AreTimeCircuitsOn)
                return;

            UpdateCurrentTimeDisplay();
            TickDiodes();

            if (!Vehicle.IsVisible)
                return;

            HandleGlitching();
        }

        private void DrawGUI()
        {
            if (Main.HideGui || Main.PlayerVehicle != Vehicle || !Properties.IsGivenScaleformPriority || Utils.IsPlayerUseFirstPerson() || TcdEditer.IsEditing)
                return;

            Scaleforms.GUI.SetBackground(ModSettings.TCDBackground);
            Scaleforms.GUI.Render2D(ModSettings.TCDPosition, new SizeF(ModSettings.TCDScale * (1501f / 1100f) / GTA.UI.Screen.AspectRatio, ModSettings.TCDScale));
        }

        private void UpdateCurrentTimeDisplay()
        {
            if (Game.GameTime > nextCheck)
            {
                var time = Utils.GetWorldTime();

                if (Math.Abs((time - lastTime).TotalMilliseconds) > 600 && !presentSlot.IsDoingTimedVisible)
                {
                    if (Vehicle != null)
                        presentSlot.SetDate(time);

                    lastTime = time;
                }

                nextCheck = Game.GameTime + 500;
            }
        }

        public void SetTimeCircuitsOn(bool on)
        {
            if (Properties.IsTimeTravelling | Properties.IsReentering | TcdEditer.IsEditing)
                return;

            if (!Properties.AreTimeCircuitsOn && Mods.Hoodbox == ModState.On && !Properties.AreHoodboxCircuitsReady)
            {
                if (!TcdEditer.IsEditing)
                    Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Hoodbox_Warmup_TfcError"));

                return;
            }

            if (!Properties.AreTimeCircuitsOn && Properties.AreFlyingCircuitsBroken)
            {
                if (!TcdEditer.IsEditing)
                    Utils.DisplayHelpText(Game.GetLocalizedString("BTTFV_Chip_Damaged"));

                return;
            }

            if (IsDoingTimedVisible)
                return;

            Properties.AreTimeCircuitsOn = on;

            if (on)
            {
                Sounds.InputOn.Play();
            }
            else
            {
                Sounds.InputOff.Play();
            }

            Events.OnTimeCircuitsToggle?.Invoke();
        }

        public void SetTimeCircuitsBroken(bool state)
        {
            Properties.AreTimeCircuitsBroken = state;

            Properties.AreTimeCircuitsOn = false;
            Events.OnTimeCircuitsToggle?.Invoke();

            if (state == false && Mods.Hoodbox == ModState.On)
                Mods.WormholeType = WormholeType.BTTF3;
        }

        private void TickDiodes()
        {
            if (Game.GameTime > nextTick)
            {
                if (Vehicle != null && Vehicle.IsVisible)
                {
                    tickingDiodes?.SetState(currentState);
                    tickingDiodesOff?.SetState(!currentState);
                }

                Scaleforms.GUI.CallFunction("SET_DIODE_STATE", currentState);

                if(ModSettings.PlayDiodeBeep && currentState && Vehicle.IsVisible && !beep.IsAnyInstancePlaying)
                    beep.Play(true);

                nextTick = Game.GameTime + 500;
                currentState = !currentState;
            }
        }

        private void HandleGlitching()
        {
            if (doGlitch)
            {
                glitchEvents.RunEvents();

                if (glitchEvents.AllExecuted())
                    doGlitch = false;
            }

            //if (doGlitch && Game.GameTime > nextGlitch)
            //{
            //    if (glitchCount <= 5)
            //    {
            //        if (destinationDisplay.CurrentTime == null)
            //        {
            //            destinationDisplay.CurrentTime = errorDate;

            //            if (Vehicle != null && Vehicle.IsVisible)
            //                destinationDisplay.CreateProps();

            //            GUI.SetVisible("red", true);

            //            nextGlitch = Game.GameTime + 600;
            //        }
            //        else
            //        {
            //            destinationDisplay.CurrentTime = null;

            //            if(Vehicle != null && Vehicle.IsVisible)
            //                destinationDisplay.DeleteAllProps();

            //            GUI.SetVisible("red", false);

            //            nextGlitch = Game.GameTime + 230;
            //        }

            //        glitchCount++;
            //    }
            //    else
            //    {
            //        glitchCount = 0;
            //        doGlitch = false;
            //        TimeCircuits.GetHandler<InputHandler>().InputDate(oldDate);
            //    }
            //}
        }

        public override void Stop()
        {
            fluxCapacitor?.Stop();
            fluxCapacitor?.Dispose();
            destinationSlot.Dispose();
            previousSlot.Dispose();
            presentSlot.Dispose();
            tickingDiodes?.DeleteProp();
            tickingDiodesOff?.DeleteProp();
        }
    }
}