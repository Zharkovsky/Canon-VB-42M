using Stationary.Fixing.Grab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Canon_VB_M42
{
    public interface IPtz
    {
        UnitState State { get; }
        CameraPosition LastCameraPosition { get; }
        double CurrentX { get; }
        double CurrentY { get; }
        double CurrentZ { get; }

        bool IsMaxZoom { get; }

        ISubject<CameraPosition> PtzSubject { get; set; }

        void Open(CameraEntity device);
        void Close();

        void PanTiltOffsetPercentage(double wp, double hp);
        void OffsetLeft();
        void OffsetRight();
        void OffsetUp();
        void OffsetDown();

        void ZoomOffsetBySteps(double steps);
        void OffsetZoomIn();
        void OffsetZoomOut();

        void GoToPosition(CameraPosition position);
        bool UpdateCurrentPosition();
        int StepTo(CameraPosition destination, double percentage);
    }
}
