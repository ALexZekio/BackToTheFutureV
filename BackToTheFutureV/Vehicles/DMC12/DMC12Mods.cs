﻿using BackToTheFutureV.Utility;
using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackToTheFutureV.Vehicles
{
    [Serializable]
    public class DMC12Mods : BaseMods
    {
        public Vehicle Vehicle;

        public DMC12Mods(Vehicle vehicle)
        {
            Vehicle = vehicle;

            IsDMC12 = Vehicle.Model == ModelHandler.DMC12;

            FirstSetup();
        }

        public void FirstSetup()
        {
            if (IsDMC12)
            {
                Vehicle.Mods.InstallModKit();

                Vehicle.Mods.PrimaryColor = VehicleColor.BrushedAluminium;
                Vehicle.Mods.TrimColor = VehicleColor.PureWhite;
                Vehicle.DirtLevel = 0;

                Function.Call(Hash.SET_VEHICLE_ENVEFF_SCALE, Vehicle, 0f);

                WormholeType = WormholeType.DMC12;

                Seats = ModState.On;
            }
            else
            {
                WormholeType = WormholeType.BTTF1;
                Seats = ModState.Off;

                Exterior = ModState.Off;
                Interior = ModState.Off;
                SteeringWheelsButtons = ModState.Off;
                Vents = ModState.Off;
                OffCoils = ModState.Off;
                DamagedBumper = ModState.Off;

                GlowingEmitter = ModState.Off;
                GlowingReactor = ModState.Off;

                Exhaust = ExhaustType.Stock;
                Reactor = ReactorType.None;
                Plate = PlateType.Empty;

                HoverUnderbody = ModState.Off;
                Hoodbox = ModState.Off;

                Hook = HookState.Off;

                SuspensionsType = SuspensionsType.Stock;

                Wheel = WheelType.Stock;
            }           
        }

        public new WormholeType WormholeType
        {
            get => base.WormholeType;
            set
            {
                base.WormholeType = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.TrimDesign].Index = (int)value;
            }
        }
        
        public new SuspensionsType SuspensionsType
        {
            get => base.SuspensionsType;
            set
            {
                if (value == base.SuspensionsType)
                    return;

                switch (value)
                {
                    case SuspensionsType.Stock:
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearLeft, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearRight, 0f);

                        Function.Call((Hash)0x1201E8A3290A3B98, Vehicle, false);
                        //Function.Call((Hash)0x28B18377EB6E25F6, Vehicle, false);

                        Function.Call(Hash.MODIFY_VEHICLE_TOP_SPEED, Vehicle, 0f);
                        break;
                    default:
                        HoverUnderbody = ModState.Off;

                        Function.Call((Hash)0x1201E8A3290A3B98, Vehicle, true);
                        //Function.Call((Hash)0x28B18377EB6E25F6, Vehicle, true);

                        Function.Call(Hash.MODIFY_VEHICLE_TOP_SPEED, Vehicle, 39.7866f);

                        Utils.LiftUpWheel(Vehicle, WheelId.FrontLeft, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.FrontRight, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearLeft, 0f);
                        Utils.LiftUpWheel(Vehicle, WheelId.RearRight, 0f);

                        break;
                }

                base.SuspensionsType = value;
            }
        }
        
        public new WheelType Wheel
        {
            get => base.Wheel;
            set
            {
                base.Wheel = value;

                Function.Call(Hash.SET_VEHICLE_WHEEL_TYPE, Vehicle, 12);
                Vehicle.Mods[VehicleModType.FrontWheel].Index = (int)value;
            }
        }
        
        public new ModState Exterior
        {
            get => base.Exterior;
            set
            {
                base.Exterior = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Spoilers].Index = (int)value;
            }
        }
        
        public new ModState Interior
        {
            get => base.Interior;
            set
            {
                base.Interior = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.SideSkirt].Index = (int)value;
            }
        }
        
        public new ModState OffCoils
        {
            get => base.OffCoils;
            set
            {
                base.OffCoils = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.FrontBumper].Index = (int)value;
            }
        }
        
        public new ModState GlowingEmitter
        {
            get => base.GlowingEmitter;
            set
            {
                base.GlowingEmitter = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Frame].Index = (int)value;
            }
        }
        
        public new ModState GlowingReactor
        {
            get => base.GlowingReactor;
            set
            {
                base.GlowingReactor = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Fender].Index = (int)value;
            }
        }
        
        public new ModState DamagedBumper
        {
            get => base.DamagedBumper;
            set
            {
                base.DamagedBumper = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Aerials].Index = (int)value;
            }
        }
        
        public new ModState HoverUnderbody
        {
            get => base.HoverUnderbody;
            set
            {
                base.HoverUnderbody = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.DialDesign].Index = (int)value;
            }
        }
        
        public new ModState SteeringWheelsButtons
        {
            get => base.SteeringWheelsButtons;
            set
            {
                base.SteeringWheelsButtons = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.SteeringWheels].Index = (int)value;
            }
        }
        
        public new ModState Vents
        {
            get => base.Vents;
            set
            {
                base.Vents = value;

                if (IsDMC12)
                Vehicle.Mods[VehicleModType.ColumnShifterLevers].Index = (int)value;
            }
        }
        
        public new ModState Seats
        {
            get => base.Seats;
            set
            {
                base.Seats = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.VanityPlates].Index = (int)value;
            }
        }
        
        public new ReactorType Reactor
        {
            get => base.Reactor;
            set
            {
                base.Reactor = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Plaques].Index = (int)value;
            }
        }
        
        public new PlateType Plate
        {
            get => base.Plate;
            set
            {
                base.Plate = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Ornaments].Index = (int)value;
            }
        }
        
        public new ExhaustType Exhaust
        {
            get => base.Exhaust;
            set
            {
                base.Exhaust = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Windows].Index = (int)value;
            }
        }
        
        public new ModState Hoodbox
        {
            get => base.Hoodbox;
            set
            {
                base.Hoodbox = value;

                if (IsDMC12)
                    Vehicle.Mods[VehicleModType.Livery].Index = (int)value;
            }
        }
        
        public new HookState Hook
        {
            get => base.Hook;
            set
            {
                base.Hook = value;

                if (!IsDMC12)
                    return;

                switch (value)
                {
                    case HookState.Off:
                        Vehicle.Mods[VehicleModType.Roof].Index = 1;
                        Vehicle.Mods[VehicleModType.Roof].Variation = false;

                        Vehicle.Mods[VehicleModType.ArchCover].Index = -1;
                        Vehicle.Mods[VehicleModType.ArchCover].Variation = false;

                        Vehicle.Mods[VehicleModType.Grille].Index = -1;
                        Vehicle.Mods[VehicleModType.Grille].Variation = false;
                        break;
                    case HookState.OnDoor:
                        Vehicle.Mods[VehicleModType.Roof].Index = 0;
                        Vehicle.Mods[VehicleModType.Roof].Variation = false;

                        Vehicle.Mods[VehicleModType.ArchCover].Index = 0;
                        Vehicle.Mods[VehicleModType.ArchCover].Variation = false;

                        Vehicle.Mods[VehicleModType.Grille].Index = 0;
                        Vehicle.Mods[VehicleModType.Grille].Variation = false;
                        break;
                    case HookState.On:
                        Vehicle.Mods[VehicleModType.Roof].Index = 0;
                        Vehicle.Mods[VehicleModType.Roof].Variation = false;

                        Vehicle.Mods[VehicleModType.ArchCover].Index = 1;
                        Vehicle.Mods[VehicleModType.ArchCover].Variation = false;

                        Vehicle.Mods[VehicleModType.Grille].Index = 1;
                        Vehicle.Mods[VehicleModType.Grille].Variation = false;
                        break;
                    case HookState.Removed:
                        Vehicle.Mods[VehicleModType.Roof].Index = 0;
                        Vehicle.Mods[VehicleModType.Roof].Variation = false;

                        Vehicle.Mods[VehicleModType.ArchCover].Index = -1;
                        Vehicle.Mods[VehicleModType.ArchCover].Variation = false;

                        Vehicle.Mods[VehicleModType.Grille].Index = 0;
                        Vehicle.Mods[VehicleModType.Grille].Variation = false;
                        break;
                }
            }
        }
    }
}
