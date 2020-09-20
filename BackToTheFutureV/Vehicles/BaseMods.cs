﻿using BackToTheFutureV.TimeMachineClasses;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackToTheFutureV.Vehicles
{
    public enum WormholeType
    {
        Unknown = -1,
        DMC12,
        BTTF1,
        BTTF2,
        BTTF3
    }

    public enum ModState
    {
        Off = -1,
        On = 0
    }

    public enum HookState
    {
        Off,
        OnDoor,
        On,
        Removed,
        Unknown
    }

    public enum PlateType
    {
        Empty = -1,
        Outatime = 0,
        BTTF2 = 1,
        NOTIME = 2,
        TIMELESS = 3,
        TIMELESS2 = 4,
        DMCFACTORY = 5,
        DMCFACTORY2 = 6
    }

    public enum ReactorType
    {
        None = -1,
        MrFusion = 0,
        Nuclear = 1
    }

    public enum ExhaustType
    {
        Stock = -1,
        BTTF = 0,
        None = 1
    }

    public enum WheelType
    {
        Stock = -1,
        StockInvisible = 0,
        RailroadInvisible = 1,
        RedInvisible = 2,
        Red = 3
    }

    public enum SuspensionsType
    {
        Unknown = -1,
        Stock = 0,
        LiftFrontLowerRear = 1,
        LiftFront = 2,
        LiftRear = 3,
        LiftFrontAndRear = 4,
        LowerFrontLiftRear = 5,
        LowerFront = 6,
        LowerRear = 7,
        LowerFrontAndRear = 8
    }

    [Serializable]
    public class BaseMods
    {
        public bool IsDMC12 { get; protected set; } = false;
        public WormholeType WormholeType { get; set; } = WormholeType.DMC12;
        public SuspensionsType SuspensionsType { get; set; } = SuspensionsType.Stock;
        public WheelType Wheel { get; set; } = WheelType.Stock;
        public ModState Exterior { get; set; } = ModState.Off;
        public ModState Interior { get; set; } = ModState.Off;
        public ModState OffCoils { get; set; } = ModState.Off;
        public ModState GlowingEmitter { get; set; } = ModState.Off;
        public ModState GlowingReactor { get; set; } = ModState.Off;
        public ModState DamagedBumper { get; set; } = ModState.Off;
        public ModState HoverUnderbody { get; set; } = ModState.Off;
        public ModState SteeringWheelsButtons { get; set; } = ModState.Off;
        public ModState Vents { get; set; } = ModState.Off;
        public ModState Seats { get; set; } = ModState.Off;
        public ReactorType Reactor { get; set; } = ReactorType.None;
        public PlateType Plate { get; set; } = PlateType.Empty;
        public ExhaustType Exhaust { get; set; } = ExhaustType.Stock;
        public ModState Hoodbox { get; set; } = ModState.Off;
        public HookState Hook { get; set; } = HookState.Off;

        public BaseMods()
        {

        }

        public BaseMods Clone()
        {
            BaseMods ret = new BaseMods();

            ret.IsDMC12 = IsDMC12;
            ret.WormholeType = WormholeType;
            ret.SuspensionsType = SuspensionsType;
            ret.Wheel = Wheel;
            ret.Exterior = Exterior;
            ret.Interior = Interior;
            ret.OffCoils = OffCoils;
            ret.GlowingEmitter = GlowingEmitter;
            ret.GlowingReactor = GlowingReactor;
            ret.DamagedBumper = DamagedBumper;
            ret.HoverUnderbody = HoverUnderbody;
            ret.SteeringWheelsButtons = SteeringWheelsButtons;
            ret.Vents = Vents;
            ret.Seats = Seats;
            ret.Reactor = Reactor;
            ret.Plate = Plate;
            ret.Exhaust = Exhaust;
            ret.Hoodbox = Hoodbox;
            ret.Hook = Hook;

            return ret;
        }

        public void ApplyTo(TimeMachine timeMachine)
        {
            TimeMachineMods ret = timeMachine.Mods;

            ret.IsDMC12 = IsDMC12;
            ret.WormholeType = WormholeType;
            ret.SuspensionsType = SuspensionsType;
            ret.Wheel = Wheel;
            ret.Exterior = Exterior;
            ret.Interior = Interior;
            ret.OffCoils = OffCoils;
            ret.GlowingEmitter = GlowingEmitter;
            ret.GlowingReactor = GlowingReactor;
            ret.DamagedBumper = DamagedBumper;
            ret.HoverUnderbody = HoverUnderbody;
            ret.SteeringWheelsButtons = SteeringWheelsButtons;
            ret.Vents = Vents;
            ret.Seats = Seats;
            ret.Reactor = Reactor;
            ret.Plate = Plate;
            ret.Exhaust = Exhaust;
            ret.Hoodbox = Hoodbox;
            ret.Hook = Hook;
        }
    }
}
