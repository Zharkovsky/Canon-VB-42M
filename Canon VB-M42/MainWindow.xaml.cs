using Stationary.Fixing.Grab;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NCalc;
using System.Globalization;

namespace Canon_VB_M42
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private IGrabber grabber;
        private IPtz ptz;

        public MainWindow()
        {
            InitializeComponent();

            CreateCommands();

            //grabber = new HttpGrabber();
            grabber = new DefaultGrabber();

            grabber.Start(new CameraEntity());
            grabber.GrabberSubject.Subscribe(_ => ImageFrame = _.JpegData);

            ptz = new HttpPtz();
            //ptz = new CanonHttpPtz();
            var model = new CameraModel();

            //UpdateInfo(model);
            UpdateDahuaPtzInfo(model);

            ptz.Open(new CameraEntity()
            {
                Model = model,
                //IpAddress = "192.168.100.100",
                Login = "admin",
                Password = "admin",
                IpAddress = "192.168.111.127",
            });

            DataContext = this;
        }

        private void UpdateDahuaPtzInfo(CameraModel model, string info = null)
        {
            model.ModelName = "DahuaPtz";
            model.PtzCgi = "http://192.168.111.127/cgi-bin/ptz.cgi";
            model.InfoCgi = "http://192.168.111.127/cgi-bin/ptz.cgi";
            model.GetPositionCommand = "action=getStatus";
            model.GoToPositionCommand = "action=start&channel=0&code=PositionABS&arg1={0}&arg2={1}&arg3={2}";
            model.ParsePositionSeparator = "=";
            model.ParsePositionPan = "status.Postion[0]";
            model.ParsePositionTilt = "status.Postion[1]";
            model.ParsePositionZoom = "status.Postion[2]";
            model.MinPanValue = 0;
            model.MaxPanValue = 360;
            model.MinTiltValue = -15;
            model.MaxTiltValue = 90;
            model.MinZoomValue = 1;
            model.MaxZoomValue = 128;
            model.FoldZoom = 30;
            model.MinFocalLength = 4.3;
            model.MaxFocalLength = 129;
            model.MatrixSize = 1 / 3f; //1.0/3.0,
            model.AspectRatioWidth = 16;
            model.AspectRatioHeight = 9;
            model.VideoStreamPath = "rtsp://{0}:{1}@{2}/cam/realmonitor?channel=1&subtype=0";
            model.OffsetMultiplicity = 1;
            model.PanReverse = false;
            model.TiltReverse = false;
            model.ZoomReverse = false;
            //model.FocalLengthByZoomStepsExpression = "4.3 + ([zoomSteps] - 1) * (129 - 4.3) / (128 - 1)";
            //model.ZoomStepsByFocalLengthExpression = "Round(1 + ([focalLength] - 4.3) * (128 - 1) / (129 - 4.3))";
            model.FocalLengthByZoomStepsExpression = "0.0037 * [zoomSteps] * [zoomSteps] + 0.711 * [zoomSteps] + 2.9955";
            model.ZoomStepsByFocalLengthExpression = "-0.0054 * [focalLength] * [focalLength] + 1.3667 * [focalLength] - 3.7474";
            model.ZoomStepsParameter = "zoomSteps";
            model.FocalLengthParameter = "focalLength";
        }

        private void UpdateInfo(CameraModel model, string info = null)
        {
            model.PtzCgi = "http://192.168.100.100/-wvhttp-01-/control.cgi";
            model.InfoCgi = "http://192.168.100.100/-wvhttp-01-/info.cgi";
            model.GetPositionCommand = "";
            model.GoToPositionCommand = "pan={0}&tilt={1}&zoom={2}";
            model.MinTiltValue = -90;
            model.MaxTiltValue = 10;
            model.MinPanValue = -170;
            model.MaxPanValue = 170;
            model.MinZoomValue = 320;
            model.MaxZoomValue = 6040;
            model.FoldZoom = 20;
            model.MinFocalLength = 4.7;
            model.MaxFocalLength = 94;
            model.MatrixSize = 1 / 3f; //1.0/3.0,
            model.AspectRatioWidth = 16;
            model.AspectRatioHeight = 9;
            model.OffsetMultiplicity = 100;
            model.PanReverse = true;
            model.TiltReverse = true;
            model.ZoomReverse = true;
            model.ParsePositionSeparator = ":=";
            model.ParsePositionPan = "c.1.pan";
            model.ParsePositionTilt = "c.1.tilt";
            model.ParsePositionZoom = "c.1.zoom";
            model.FocalLengthByZoomStepsExpression = "26926.3465 * Pow([zoomSteps], -0.9896)";
            model.ZoomStepsByFocalLengthExpression = "29852.0733 * Pow([focalLength], -1.009)";
            model.ZoomStepsParameter = "zoomSteps";
            model.FocalLengthParameter = "focalLength";
        }

        #region 

        byte[] img;
        public byte[] ImageFrame
        {
            get => img;
            set { img = value; Raise(); }
        }

        private void CreateCommands()
        {
            OffsetCommand = new RelayCommand(OffsetAction);
            ZoomInCommand = new RelayCommand(ZoomInAction);
            ZoomOutCommand = new RelayCommand(ZoomOutAction);
        }

        public RelayCommand SaveCommand { get; private set; }

        public RelayCommand OffsetCommand { get; private set; }
        public RelayCommand ZoomInCommand { get; private set; }
        public RelayCommand ZoomOutCommand { get; private set; }


        private async void OffsetAction(object parameter)
        {

            var element = parameter as FrameworkElement;
            if (element == null) return;


            var pos = Mouse.GetPosition(element);
            var panDeg = ((element.ActualWidth / 2) - pos.X) / element.ActualWidth;
            var tiltDeg = (pos.Y - (element.ActualHeight / 2)) / element.ActualHeight;

            await Task.Factory.StartNew(() => PercentageOffset(panDeg, tiltDeg));
        }

        private async void ZoomInAction(object parameter)
        {

            await Task.Factory.StartNew(() => ZoomStep(.5f));
        }

        private async void ZoomOutAction(object parameter)
        {

            await Task.Factory.StartNew(() => ZoomStep(-.5f));
        }
        #endregion

        #region capture controller
        public CommandState PercentageOffset(double pan, double tilt)
        {
            var result = CommandState.Ok;

            ptz.PanTiltOffsetPercentage(pan, tilt);
            return result;
        }

        public CommandState ZoomStep(float step)
        {
            var result = CommandState.Ok;

            ptz.ZoomOffsetBySteps(step);
            return result;
        }
        #endregion

        #region Buttons
        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void Raise([CallerMemberName]string property = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ptz.OffsetLeft();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ptz.OffsetRight();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ptz.OffsetUp();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ptz.OffsetDown();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            ptz.OffsetZoomIn();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            ptz.OffsetZoomOut();
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            ptz.StepTo(new CameraPosition() { X = -17000, Y = -9000, Z = 320 }, 1E0);
        }

        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            ptz.ZoomOffsetBySteps(5);
        }

        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            ptz.PanTiltOffsetPercentage(0, 0);
        }

        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            ptz.GoToPosition(new CameraPosition() { X = 17000, Y = 1000, Z = 6040 });
        }

        private void Button_Click_10(object sender, RoutedEventArgs e)
        {
            ptz.UpdateCurrentPosition();
        }
        #endregion
    }
}
