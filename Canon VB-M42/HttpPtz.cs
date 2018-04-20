using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using NCalc;
using Stationary.Fixing.Grab;

namespace Canon_VB_M42
{
    public class HttpPtz : IPtz
    {
        #region private

        private const double _inchSize = 25.4f;

        private CameraModel _cameraModel;

        private object _syncRoot = new object();
        private TimeSpan _timeout;
        private string _baseAddress;
        private HttpClientHandler _clientHandler;
        private HttpClient _client;
        private CultureInfo culture = CultureInfo.InvariantCulture;

        private Subject<CameraPositionOffset> _cameraPositionOffsetSubject;
        private IDisposable _camerPositionOffsetSubscription;

        private ISubject<CameraPosition> _ptzSubject = new Subject<CameraPosition>();
        public ISubject<CameraPosition> PtzSubject { get => _ptzSubject; set => _ptzSubject = value; }

        private void SetParameters(CameraEntity device)
        {
            _cameraModel = device.Model;
            _clientHandler = new HttpClientHandler { Credentials = device.Credential };
            _baseAddress = device.BaseHttp;
            _timeout = device.Timeout;
        }
        
        private void PositionHandler(List<CameraPositionOffset> offsets)
        {
            Console.WriteLine(string.Join(" + ", offsets.Select(_ => _.ToString())) + " = " + offsets.Aggregate((o1, o2) => o1 + o2));
            GoToPosition(LastCameraPosition + offsets.Aggregate((o1, o2) => o1 + o2));
        }

        private void EnqueueGoTo(CameraPositionOffset offset)
        {
            _cameraPositionOffsetSubject.OnNext(offset);
        }

        private double ZoomStepsToZoom(int zoomSteps)
        {
            var x = _cameraModel.FoldZoom - (zoomSteps - _cameraModel.MinZoomValue) * (_cameraModel.FoldZoom) / (_cameraModel.MaxZoomValue - _cameraModel.MinZoomValue);
            return Math.Max(Math.Min(x, _cameraModel.FoldZoom), 1);
        }

        private int ZoomToZoomSteps(double zoom)
        {
            if (_cameraModel.ZoomReverse)
            {
                return (int)(_cameraModel.MinZoomValue + ((_cameraModel.MaxZoomValue - _cameraModel.MinZoomValue) * (_cameraModel.FoldZoom - zoom)) / (_cameraModel.FoldZoom - 1));
            }
            else
            {
                return (int)(_cameraModel.MaxZoomValue - ((_cameraModel.MaxZoomValue - _cameraModel.MinZoomValue) * (_cameraModel.FoldZoom - zoom)) / (_cameraModel.FoldZoom - 1));
            }
        }

        private void ValidatePosition(CameraPosition position)
        {
            if (position.X < _cameraModel.MinPanValue)
            {
                var px = _cameraModel.MinPanValue - position.X;
                position.X = _cameraModel.MaxPanValue - px;
            }
            else if (position.X > _cameraModel.MaxPanValue)
            {
                var px = position.X - _cameraModel.MaxPanValue;
                position.X = _cameraModel.MinPanValue + px;
            }

            if(!_cameraModel.TiltReverse)
            {
                if (position.Y < _cameraModel.MinTiltValue) position.Y = _cameraModel.MinTiltValue;
                else if (position.Y > _cameraModel.MaxTiltValue) position.Y = _cameraModel.MaxTiltValue;
            }
            else
            {
                if (position.Y < - _cameraModel.MaxTiltValue) position.Y = - _cameraModel.MaxTiltValue;
                else if (position.Y > _cameraModel.MaxTiltValue) position.Y = - _cameraModel.MinTiltValue;
            }
            

            if (position.Z < 1) position.Z = 1;
            else if (position.Z > _cameraModel.FoldZoom) position.Z = _cameraModel.FoldZoom;
        }

        private void DoRequest(string reqStr)
        {
            lock (_syncRoot)
            {
                if (State != UnitState.Run)
                {
                    throw new InvalidOperationException($"{nameof(HttpPtz)} не был открыт");
                }
                try
                {
                    var reqtask = _client.GetStringAsync(reqStr);
                    reqtask.Wait();
                    LastResponse = reqtask.Result;
                    LastRequest = reqStr;
                    State = UnitState.Run;
                }
                catch (Exception ex)
                {
                    LastResponse = null;
                    State = UnitState.Error;
                    _ptzSubject.OnError(ex);
                }
            }
        }

        private double FocalLengthByZoomSteps(int zoomSteps)
        {
            Expression e = new Expression(_cameraModel.FocalLengthByZoomStepsExpression);
            e.Parameters[_cameraModel.ZoomStepsParameter] = zoomSteps;
            return (double)e.Evaluate();
        }

        private int ZoomStepsByFocalLength(double focalLength)
        {
            Expression e = new Expression(_cameraModel.ZoomStepsByFocalLengthExpression);
            e.Parameters[_cameraModel.FocalLengthParameter] = focalLength;
            return (int)e.Evaluate();
        }

        private double GetAngle(double length)
        {
            var focalLength = FocalLengthByZoomSteps(ZoomToZoomSteps(CurrentZ));
            var angle = 2.0 * Math.Atan(length / (2.0 * focalLength)) * (180.0 / Math.PI);
            return angle;
        }

        private double GetPanDegree(double percentage)
        {
            percentage = Math.Min(100, Math.Max(0, percentage));
            return percentage * CurrentWidthAngle;
        }

        private double GetTiltDegree(double percentage)
        {
            percentage = Math.Min(100, Math.Max(0, percentage));
            return percentage * CurrentHeightAngle;
        }

        #endregion

        #region interface

        public UnitState State { get; private set; }
        public CameraPosition LastCameraPosition { get; private set; }

        public double CurrentX => LastCameraPosition.X;
        public double CurrentY => LastCameraPosition.Y;
        public double CurrentZ => LastCameraPosition.Z;

        public bool IsMaxZoom => CurrentZ >= _cameraModel.FoldZoom;

        public void Open(CameraEntity device)
        {
            lock (_syncRoot)
            {
                Close();
                SetParameters(device);
                _client = new HttpClient(_clientHandler, true)
                {
                    BaseAddress = new Uri(_baseAddress),
                    Timeout = _timeout
                };
                State = UnitState.Run;
                if (!UpdateCurrentPosition() || State != UnitState.Run)
                {
                    throw new Exception($"Не удалось инициализировать {nameof(HttpPtz)}");
                }
                _camerPositionOffsetSubscription = _cameraPositionOffsetSubject.ObserveWithBuffer(TaskPoolScheduler.Default).Subscribe(PositionHandler);
            }
        }

        public void Close()
        {
            lock (_syncRoot)
            {
                _camerPositionOffsetSubscription?.Dispose();
                State = UnitState.Void;
                _client?.Dispose();
                _client = null;
            }
        }

        public void PanTiltOffsetPercentage(double wp, double hp) =>
            PanTiltOffset(Math.Sign(wp) * GetPanDegree(Math.Abs(wp)), Math.Sign(hp) * GetTiltDegree(Math.Abs(hp)));

        public void OffsetLeft() => PanTiltOffset(GetPanDegree(.1), 0);
        public void OffsetRight() => PanTiltOffset(-GetPanDegree(.1), 0);
        public void OffsetUp() => PanTiltOffset(0, -GetTiltDegree(.1));
        public void OffsetDown() => PanTiltOffset(0, GetTiltDegree(.1));

        public void ZoomOffsetBySteps(double steps) => ZoomOffset(steps);

        public void OffsetZoomIn() => ZoomOffset(1);
        public void OffsetZoomOut() => ZoomOffset(-1);

        public virtual void GoToPosition(CameraPosition position)
        {
            if (position == null) return;

            ValidatePosition(position);

            var pan = (_cameraModel.PanReverse ? -1 : 1) * position.X;
            var tilt = (_cameraModel.TiltReverse ? -1 : 1) * position.Y;
            var zoom =  ZoomToZoomSteps(position.Z);

            if (_cameraModel.OffsetMultiplicity != 1)
            {
                pan = (int)(_cameraModel.OffsetMultiplicity * pan);
                tilt = (int)(_cameraModel.OffsetMultiplicity * tilt);
            }

            string cmds = string.Format(culture, _cameraModel.GoToPositionCommand, pan, tilt, zoom);

            string RequestStr = $"{_cameraModel.PtzCgi}?{cmds}";

            lock (_syncRoot)
            {
                DoRequest(RequestStr);
                if (State != UnitState.Run) return;
            }

            LastCameraPosition = position;
            PtzSubject.OnNext(LastCameraPosition);
        }

        public virtual bool UpdateCurrentPosition()
        {
            string RequestStr = $"{_cameraModel.InfoCgi}?{_cameraModel.GetPositionCommand}";
            var resp = "";
            lock (_syncRoot)
            {
                DoRequest(RequestStr);
                if (State != UnitState.Run) return false;
                resp = LastResponse;
            }
            var dict = resp
                .Split('\n')
                .Select(
                    line =>
                    line.Trim()
                        .Split(new[] { _cameraModel.ParsePositionSeparator }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(_ => _.Trim()))
                .Where(pair => pair.Count() == 2)
                .ToDictionary(key => key.First(), value => value.Last());

            if ((dict.ContainsKey(_cameraModel.ParsePositionPan) &&
                float.TryParse(dict[_cameraModel.ParsePositionPan], NumberStyles.Any, culture, out float x)) &&
                (dict.ContainsKey(_cameraModel.ParsePositionTilt) &&
                float.TryParse(dict[_cameraModel.ParsePositionTilt], NumberStyles.Any, culture, out float y)) &&
                (dict.ContainsKey(_cameraModel.ParsePositionZoom) &&
                float.TryParse(dict[_cameraModel.ParsePositionZoom], NumberStyles.Any, culture, out float z)))
            {
                LastCameraPosition.X = x / _cameraModel.OffsetMultiplicity;
                LastCameraPosition.Y = y / _cameraModel.OffsetMultiplicity;
                LastCameraPosition.Z = (float)ZoomStepsToZoom((int)z);
                return true;
            }

            return false;
        }

        public int StepTo(CameraPosition destination, double percentage)
        {
            var mainOffset = (destination - LastCameraPosition);

            var dx = mainOffset.DX;
            if (Math.Abs(dx) > 180) dx = dx > 0 ? -(360 - dx) : (360 + dx);
            mainOffset.DX = dx;

            var Xsteps = Math.Abs(mainOffset.DX / (CurrentWidthAngle * percentage));
            var Ysteps = Math.Abs(mainOffset.DY / (CurrentHeightAngle * percentage));
            var Zsteps = Math.Abs(mainOffset.DZ);

            var stepsCount = (float)Math.Max(Zsteps, Math.Max(Xsteps, Ysteps));

            if (stepsCount <= 1) GoToPosition(destination);
            else GoToPosition(LastCameraPosition + mainOffset / stepsCount);

            return Math.Max(0, (int)(stepsCount--));
        }

        #endregion

        public string LastRequest { get; private set; }
        public string LastResponse { get; private set; }

        public double CurrentWidthAngle
        {
            get
            {
                var widthLength = _cameraModel.AspectRatioWidth * (_cameraModel.MatrixSize * _inchSize) /
                    (_cameraModel.AspectRatioWidth + _cameraModel.AspectRatioHeight);
                return GetAngle(widthLength);
            }
        }
        public double CurrentHeightAngle
        {
            get
            {
                var heightLength = _cameraModel.AspectRatioHeight * (_cameraModel.MatrixSize * _inchSize) /
                    (_cameraModel.AspectRatioWidth + _cameraModel.AspectRatioHeight);
                return GetAngle(heightLength);
            }
        }

        public void PanTiltOffset(double dx, double dy) => EnqueueGoTo(new CameraPositionOffset((float)dx, (float)dy, 0));
        public void ZoomOffset(double dz) => EnqueueGoTo(new CameraPositionOffset(0, 0, (float)dz));

        public HttpPtz()
        {
            LastCameraPosition = new CameraPosition();
            _cameraPositionOffsetSubject = new Subject<CameraPositionOffset>();
        }
    }
}