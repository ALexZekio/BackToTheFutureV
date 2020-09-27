﻿using BackToTheFutureV.TimeMachineClasses.Handlers;
using BackToTheFutureV.TimeMachineClasses.Handlers.BaseHandlers;
using BackToTheFutureV.Utility;
using BackToTheFutureV.Vehicles;
using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.TimeMachineClasses
{
    [Serializable]
    public class TimeMachineMods : DMC12Mods
    {
        protected TimeMachine TimeMachine { get; }
        public TimeMachineMods(TimeMachine timeMachine, WormholeType wormholeType) : base(timeMachine.Vehicle)
        {
            TimeMachine = timeMachine;

            if (IsDMC12)
            {
                Vehicle.ToggleExtra(10, false);

                if ((ReactorType)Vehicle.Mods[VehicleModType.Plaques].Index != ReactorType.None)
                {
                    SyncMods();

                    return;
                }
            }
                           
            WormholeType = wormholeType;

            Exterior = ModState.On;
            Interior = ModState.On;
            SteeringWheelsButtons = ModState.On;
            Vents = ModState.On;
            OffCoils = ModState.On;
            Hook = HookState.Off;

            switch (WormholeType)
            {
                case WormholeType.BTTF1:
                    Reactor = ReactorType.Nuclear;
                    Exhaust = ExhaustType.BTTF;
                    Plate = PlateType.Outatime;
                    break;
                case WormholeType.BTTF2:
                    Reactor = ReactorType.MrFusion;
                    Exhaust = ExhaustType.None;
                    Plate = PlateType.BTTF2;

                    HoverUnderbody = ModState.On;
                    break;
                case WormholeType.BTTF3:
                    Reactor = ReactorType.MrFusion;
                    Exhaust = ExhaustType.BTTF;
                    Plate = PlateType.BTTF2;

                    Hoodbox = ModState.On;
                    Wheel = WheelType.Red;
                    SuspensionsType = SuspensionsType.LiftFront;
                    break;
            }
        }

        public void SyncMods()
        {
            SuspensionsType suspensionsType = (SuspensionsType)Vehicle.Mods[VehicleModType.Hydraulics].Index;

            WormholeType wormholeType = (WormholeType)Vehicle.Mods[VehicleModType.TrimDesign].Index;

            if (wormholeType != WormholeType)
                WormholeType = wormholeType;

            WheelType wheelType = (WheelType)Vehicle.Mods[VehicleModType.FrontWheel].Index;

            if (wheelType != Wheel)
                Wheel = wheelType;

            ReactorType reactorType = (ReactorType)Vehicle.Mods[VehicleModType.Plaques].Index;

            if (reactorType != Reactor)
                Reactor = reactorType;

            PlateType plateType = (PlateType)Vehicle.Mods[VehicleModType.Ornaments].Index;

            if (plateType != Plate)
                Plate = plateType;

            ExhaustType exhaustType = (ExhaustType)Vehicle.Mods[VehicleModType.Windows].Index;

            if (exhaustType != Exhaust)
                Exhaust = exhaustType;

            ModState modState = (ModState)Vehicle.Mods[VehicleModType.Spoilers].Index;

            if (modState != Exterior)
                Exterior = modState;

            modState = (ModState)Vehicle.Mods[VehicleModType.SideSkirt].Index;

            if (modState != Interior)
                Interior = modState;

            modState = (ModState)Vehicle.Mods[VehicleModType.FrontBumper].Index;

            if (modState != OffCoils)
                OffCoils = modState;

            modState = (ModState)Vehicle.Mods[VehicleModType.Frame].Index;

            if (modState != GlowingEmitter)
                GlowingEmitter = modState;

            modState = (ModState)Vehicle.Mods[VehicleModType.Fender].Index;

            if (modState != GlowingReactor)
                GlowingReactor = modState;

            modState = (ModState)Vehicle.Mods[VehicleModType.Aerials].Index;

            if (modState != DamagedBumper)
                DamagedBumper = modState;

            modState = (ModState)Vehicle.Mods[VehicleModType.DialDesign].Index;

            if (modState != HoverUnderbody)
                HoverUnderbody = modState;

            modState = (ModState)Vehicle.Mods[VehicleModType.SteeringWheels].Index;

            if (modState != SteeringWheelsButtons)
                SteeringWheelsButtons = modState;

            modState = (ModState)Vehicle.Mods[VehicleModType.ColumnShifterLevers].Index;

            if (modState != Vents)
                Vents = modState;

            modState = (ModState)Vehicle.Mods[VehicleModType.VanityPlates].Index;

            if (modState != Seats)
                Seats = modState;

            modState = (ModState)Vehicle.Mods[VehicleModType.Livery].Index;

            if (modState != Hoodbox)
                Hoodbox = modState;

            HookState hookState = HookState.Unknown;

            if (Vehicle.Mods[VehicleModType.Roof].Index == 1 && Vehicle.Mods[VehicleModType.ArchCover].Index == -1 && Vehicle.Mods[VehicleModType.Grille].Index == -1)
                hookState = HookState.Off;

            if (Vehicle.Mods[VehicleModType.Roof].Index == 0 && Vehicle.Mods[VehicleModType.ArchCover].Index == 0 && Vehicle.Mods[VehicleModType.Grille].Index == 0)
                hookState = HookState.OnDoor;

            if (Vehicle.Mods[VehicleModType.Roof].Index == 0 && Vehicle.Mods[VehicleModType.ArchCover].Index == 1 && Vehicle.Mods[VehicleModType.Grille].Index == 1)
                hookState = HookState.On;

            if (Vehicle.Mods[VehicleModType.Roof].Index == 0 && Vehicle.Mods[VehicleModType.ArchCover].Index == -1 && Vehicle.Mods[VehicleModType.Grille].Index == 0)
                hookState = HookState.Removed;

            if (hookState != Hook)
                Hook = hookState;

            if (suspensionsType != SuspensionsType)
                SuspensionsType = suspensionsType;
        }

        public new WheelType Wheel
        {
            get => base.Wheel;
            set
            {
                if (IsDMC12)
                {
                    if (TimeMachine.Properties != null && TimeMachine.Properties.IsFlying)
                        return;
                }

                base.Wheel = value;

                if (value == WheelType.RailroadInvisible)
                {
                    TimeMachine.Props?.RRWheels?.ForEach(x => x?.SpawnProp());

                    if (!IsDMC12)
                        return;
                   
                    if (HoverUnderbody == ModState.On)
                        HoverUnderbody = ModState.Off;

                    if (SuspensionsType != SuspensionsType.Stock)
                        SuspensionsType = SuspensionsType.Stock;

                    if (WormholeType == WormholeType.BTTF3)
                        TimeMachine.Events?.OnWormholeTypeChanged?.Invoke();
                }
                else
                {
                    TimeMachine.Props?.RRWheels?.ForEach(x => x?.DeleteProp());

                    Utils.SetTiresBurst(Vehicle, false);
                }
            }
        }

        public new SuspensionsType SuspensionsType
        {
            get => base.SuspensionsType;
            set
            {
                if (IsDMC12)
                {
                    if (TimeMachine.Properties != null && TimeMachine.Properties.IsFlying)
                        return;
                }

                base.SuspensionsType = value;

                if (!IsDMC12)
                    return;

                if (value != SuspensionsType.Stock)
                {
                    if (HoverUnderbody == ModState.On)
                        HoverUnderbody = ModState.Off;

                    if (Wheel == WheelType.RailroadInvisible)
                        Wheel = WheelType.Stock;
                } else
                {
                    if (TimeMachine.Properties != null)
                        TimeMachine.Properties.TorqueMultiplier = 1;
                }
            }
        }

        public new ModState HoverUnderbody
        {
            get => base.HoverUnderbody;
            set 
            {
                if (IsDMC12)
                {
                    if (TimeMachine.Properties != null && TimeMachine.Properties.IsFlying)
                        return;
                }

                base.HoverUnderbody = value;

                if (IsDMC12)
                {
                    if (value == ModState.On)
                    {
                        if (Wheel == WheelType.RailroadInvisible)
                            Wheel = WheelType.Stock;

                        if (SuspensionsType != SuspensionsType.Stock)
                            SuspensionsType = SuspensionsType.Stock;

                        Exhaust = ExhaustType.None;
                    }

                    TimeMachine.DMC12.SetStockSuspensions?.Invoke(value == ModState.Off);

                    TimeMachine.Events?.OnHoverUnderbodyToggle?.Invoke();
                }                                  
            }
        }

        public new ExhaustType Exhaust
        {
            get => base.Exhaust;
            set
            {
                if (IsDMC12)
                {
                    if (TimeMachine.Properties != null && TimeMachine.Properties.IsFlying)
                        return;
                }

                base.Exhaust = value;

                if (IsDMC12 && value != ExhaustType.None && HoverUnderbody == ModState.On)
                    HoverUnderbody = ModState.Off;
            }
        }

        public new WormholeType WormholeType
        {
            get => base.WormholeType;
            set
            {
                base.WormholeType = value;

                TimeMachine.Events?.OnWormholeTypeChanged?.Invoke();
            }
        }

        public new ReactorType Reactor
        {
            get => base.Reactor;
            set
            {
                base.Reactor = value;

                TimeMachine.Events?.OnReactorTypeChanged?.Invoke();
            }
        }

        public new ModState Hoodbox
        {
            get => base.Hoodbox;
            set
            {
                base.Hoodbox = value;

                DamagedBumper = value;

                if (value == ModState.On)
                {
                    if (TimeMachine.Properties == null || !TimeMachine.Properties.AreTimeCircuitsBroken)
                        return;

                    TimeMachine.Events?.SetTimeCircuitsBroken?.Invoke(false);
                    WormholeType = WormholeType.BTTF3;
                }                    
            }
        }
    }
}
