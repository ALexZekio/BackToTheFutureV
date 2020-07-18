﻿using System;
using BackToTheFutureV.Utility;
using GTA;
using GTA.Math;

namespace BackToTheFutureV.Story
{
    public delegate void OnExecute(TimedEvent timedEvent);

    public enum CameraType
    {
        Position,
        Offset,
        Entity
    }

    public class TimedEvent
    {
        public event OnExecute OnExecute;

        public int Step { get; private set; }
        public TimeSpan StartTime { get; private set; }
        public TimeSpan EndTime { get; private set; }
        public TimeSpan Duration => EndTime - StartTime;
        public bool FirstExecution => _executionCount != 2;

        public double CurrentSpeed { get; private set; }    
        public int StartSpeed = 0;
        public int EndSpeed = 0;
        private bool _setSpeed = false;

        public bool IsSettingCamera { get; private set; }
        public Entity CameraOnEntity { get; private set; }
        public Vector3 CameraPosition { get; private set; }
        public CameraType _cameraType = CameraType.Position;
        public Vector3 LookAtPosition { get; private set; }
        public Entity LookAtEntity { get; private set; }

        public Camera CustomCamera;
        private CameraType _lookAtType = CameraType.Position;
        private bool _updateCamera = false;
        private bool _disableUpdate = false;        
        
        private int _executionCount = 0;

        public TimedEvent(int tStep, TimeSpan tStartTime, TimeSpan tEndTime, float tTimeMultiplier)
        {
            Step = tStep;

            if (tTimeMultiplier != 1.0f)
            {
                StartTime = new TimeSpan(Convert.ToInt64(Convert.ToSingle(tStartTime.Ticks) * tTimeMultiplier));
                EndTime = new TimeSpan(Convert.ToInt64(Convert.ToSingle(tEndTime.Ticks) * tTimeMultiplier));
            }
            else
            {
                StartTime = tStartTime;
                EndTime = tEndTime;
            }
        }

        public void SetSpeed(int tStartSpeedMPH, int tEndSpeedMPH)
        {
            StartSpeed = tStartSpeedMPH;
            EndSpeed = tEndSpeedMPH;

            _setSpeed = true;
        }

        public void SetCamera(Entity tOnEntity, Vector3 tCameraOffset, Entity tLookAtEntity, Vector3 tLookAtOffset, bool tUpdateCamera = true)
        {
            _lookAtType = CameraType.Entity;
            LookAtEntity = tLookAtEntity;
            LookAtPosition = tLookAtOffset;

            _cameraType = CameraType.Entity;
            CameraOnEntity = tOnEntity;
            CameraPosition = tCameraOffset;

            _updateCamera = tUpdateCamera;
            IsSettingCamera = true;
        }

        private void CalculateCurrentSpeed()
        {
            CurrentSpeed = (Utils.MphToMs(EndSpeed - StartSpeed + 2) / Duration.TotalSeconds) * Game.LastFrameTime;
        }

        private void PlaceCamera()
        {
            switch (_cameraType)
            {
                case CameraType.Entity:
                    if (CustomCamera == null)
                        CustomCamera = World.CreateCamera(CameraOnEntity.GetOffsetPosition(CameraPosition), Vector3.Zero, GameplayCamera.FieldOfView);
                    else
                        CustomCamera.Position = CameraOnEntity.GetOffsetPosition(CameraPosition);                    
                    break;
            }

            switch (_lookAtType)
            {
                case CameraType.Entity:
                    CustomCamera.PointAt(LookAtEntity, LookAtPosition);
                    break;
            }

            World.RenderingCamera = CustomCamera;

            if (!_updateCamera)
                _disableUpdate = true;
        }

        public bool Run(TimeSpan tCurrentTime, bool tManageCamera = false)
        {            
            bool ret = StartTime.TotalMilliseconds <= tCurrentTime.TotalMilliseconds && tCurrentTime.TotalMilliseconds <= EndTime.TotalMilliseconds;

            if (ret) {
                if (_executionCount < 2)
                    _executionCount += 1;

                if (_setSpeed)
                    CalculateCurrentSpeed();

                if (tManageCamera && IsSettingCamera && !_disableUpdate)
                    PlaceCamera();

                OnExecute?.Invoke(this);
            }                

            return ret;
        }

    }
}
