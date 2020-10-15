﻿using GTA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LemonUI.Menus;
using BackToTheFutureV.TimeMachineClasses;
using LemonUI.Elements;
using System.Drawing;
using BackToTheFutureV.Story;

namespace BackToTheFutureV.Menu
{
    public class TrainMissionMenu : CustomNativeMenu
    {
        private NativeCheckboxItem MissionToggle;
        private NativeSliderItem Speed;
        private NativeCheckboxItem PlayMusic;
        private NativeSliderItem MusicVolume;

        public TrainMissionMenu() : base("", "Train Mission")
        {
            Banner = new ScaledTexture(new PointF(0, 0), new SizeF(200, 100), "bttf_textures", "bttf_menu_banner");

            OnItemCheckboxChanged += TrainMissionMenu_OnItemCheckboxChanged;
            Shown += TrainMissionMenu_Shown;

            Add(MissionToggle = new NativeCheckboxItem("Mission toggle"));
            Add(Speed = new NativeSliderItem("Speed"));
            Speed.ValueChanged += Speed_ValueChanged;
            Add(PlayMusic = new NativeCheckboxItem("Play music"));
            Add(MusicVolume = new NativeSliderItem("Music volume"));
            MusicVolume.ValueChanged += MusicVolume_ValueChanged;

            Main.ObjectPool.Add(this);
        }

        private void MusicVolume_ValueChanged(object sender, EventArgs e)
        {
            MissionHandler.TrainMission.MissionMusic.Volume = MusicVolume.Value / 100.0f;
            MusicVolume.Title = "Music volume: " + MusicVolume.Value.ToString();            
        }

        private void TrainMissionMenu_Shown(object sender, EventArgs e)
        {
            MissionToggle.Checked = MissionHandler.TrainMission.IsPlaying;
            PlayMusic.Checked = MissionHandler.TrainMission.PlayMusic;
            Speed.Value = (int)(MissionHandler.TrainMission.TimeMultiplier * 100);
            MusicVolume.Value = (int)(MissionHandler.TrainMission.MissionMusic.Volume * 100);            
        }

        public override void Tick()
        {            
            Speed.Enabled = !MissionToggle.Checked;
            PlayMusic.Enabled = !MissionHandler.TrainMission.IsPlaying || MissionHandler.TrainMission.MissionMusic.IsAnyInstancePlaying;
        }

        private void Speed_ValueChanged(object sender, EventArgs e)
        {
            if (Speed.Value < 10)
                Speed.Value = 10;

            MissionHandler.TrainMission.TimeMultiplier = Speed.Value / 100.0f;
            Speed.Title = "Speed: " + Speed.Value.ToString();
        }

        private void TrainMissionMenu_OnItemCheckboxChanged(NativeCheckboxItem sender, EventArgs e, bool Checked)
        {
            if (sender == MissionToggle)
            {
                if (Checked)
                    MissionHandler.TrainMission.Start();
                else
                    MissionHandler.TrainMission.End();
            }

            if (sender == PlayMusic)
            {
                MissionHandler.TrainMission.PlayMusic = Checked;

                if (MissionHandler.TrainMission.IsPlaying && !Checked)
                    MissionHandler.TrainMission.MissionMusic.Stop();
            }                
        }
    }
}
