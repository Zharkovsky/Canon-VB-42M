using Canon_VB_M42;
using System.Reactive.Subjects;

namespace Stationary.Fixing.Grab
{
    public interface IGrabber
    {
        ISubject<Image> GrabberSubject { get; set; }

        int FrameWidth { get; }
        int FrameHeight { get; }

        UnitState State { get; }

        void Start(CameraEntity device);
        void Stop();
    }
}