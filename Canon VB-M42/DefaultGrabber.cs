using Emgu.CV;
using Emgu.CV.Structure;
using Stationary.Fixing.Grab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Canon_VB_M42
{
    public class DefaultGrabber : IGrabber
    {
        public ISubject<Image> GrabberSubject { get; set; } = new Subject<Image>();

        public int FrameWidth { get; private set; }

        public int FrameHeight { get; private set; }

        public UnitState State { get; private set; }

        public void Start(CameraEntity device)
        {
            Func<VideoCapture> captureCreate = () => new VideoCapture(string.Format("rtsp://{0}:{1}@{2}/cam/realmonitor?channel=1&subtype=0", "admin", "admin", $"{"192.168.111.127"}:{554}"));
            _capture = Task.Factory.StartNew(captureCreate).Wait<VideoCapture>(device.Timeout);
            Task.Factory.StartNew(GrabLoop);
        }
        #region default grabber

        private VideoCapture _capture;
        private Mat _mat;

        private void GrabLoop()
        {
            while (true)
            {

                var retrieveFrameTask = Task.Factory.StartNew(RetrieveFrame);
                if (retrieveFrameTask.Result) OnGrab();

                Task.Delay(1).Wait();
            }
        }

        [HandleProcessCorruptedStateExceptions]
        private bool RetrieveFrame()
        {
            try
            {
                if (_mat == null)
                {
                    _mat = _capture.QueryFrame();
                    return true;
                }
                if (_capture.Grab()) return _capture.Retrieve(_mat);
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        private void OnGrab()
        {
            if (_mat != null)
            {
                using (Image<Gray, byte> i = _mat.ToImage<Gray, byte>())
                {
                    var image = new Image(i.Bytes, i.Width, i.Height, DateTime.Now);
                    try
                    {
                        GrabberSubject.OnNext(image);
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }

        #endregion

        public void Stop()
        {
            State = UnitState.Void;
        }
    }
}
