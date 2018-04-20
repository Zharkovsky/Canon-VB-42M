using Stationary.Fixing.Grab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Canon_VB_M42
{
    public class HttpGrabber : IGrabber
    {
        public ISubject<Image> GrabberSubject { get; set; } = new Subject<Image>();

        public int FrameWidth { get; private set; }

        public int FrameHeight { get; private set; }

        public UnitState State { get; private set; }

        public void Start(CameraEntity device)
        {
            var client = new HttpClient();
            State = UnitState.Run;
            Task.Factory.StartNew(async () =>
            {
                while (State == UnitState.Run)
                {
                    var frame = await client.GetByteArrayAsync("http://192.168.100.100/-wvhttp-01-/image.cgi?v=jpg:1280x720");
                    var i = new Image
                    {
                        JpegData = frame,
                    };
                    GrabberSubject.OnNext(i);
                }
            });
        }

        public void Stop()
        {
            State = UnitState.Void;
        }
    }
}
