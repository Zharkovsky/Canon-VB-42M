//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using System.Linq;
//using System.Net.Http;
//using System.Reactive;
//using System.Reactive.Concurrency;
//using System.Reactive.Disposables;
//using System.Reactive.Linq;
//using System.Reactive.Subjects;
//using System.Text;
//using System.Threading.Tasks;
//using NCalc;
//using Stationary.Fixing.Grab;

//namespace Canon_VB_M42
//{
//    public class CanonHttpPtz : HttpPtz
//    {
//        #region protected

//        protected override double ZoomStepsToZoom(int zoomSteps)
//        {
//            var x = CameraModel.FoldZoom - (zoomSteps - CameraModel.MinZoomValue) * (CameraModel.FoldZoom) / (CameraModel.MaxZoomValue - CameraModel.MinZoomValue);
//            return Math.Max(Math.Min(x, CameraModel.FoldZoom), 1);
//        }

//        protected override int ZoomToZoomSteps(double zoom)
//        {
//            if (CameraModel.ZoomReverse)
//            {
//                return (int)(CameraModel.MinZoomValue + ((CameraModel.MaxZoomValue - CameraModel.MinZoomValue) * (CameraModel.FoldZoom - zoom)) / (CameraModel.FoldZoom - 1));
//            }
//            else
//            {
//                return (int)(CameraModel.MaxZoomValue - ((CameraModel.MaxZoomValue - CameraModel.MinZoomValue) * (CameraModel.FoldZoom - zoom)) / (CameraModel.FoldZoom - 1));
//            }
//        }

//        #endregion

//        public CanonHttpPtz() : base() { }

//    }
//}
