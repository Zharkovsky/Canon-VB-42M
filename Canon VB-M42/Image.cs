using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Canon_VB_M42
{
    public class Image
    {
        private byte[] _jpegData;
        private DateTime _fixationTime;

        public int Id { get; set; }

        public virtual int Width { get; set; }
        public virtual int Height { get; set; }
        public virtual DateTime FixationTime { get => _fixationTime; set => _fixationTime = value; }

        [XmlIgnore]
        public virtual byte[] RawData { get; set; }

        public virtual byte[] JpegData
        {
            get
            {
                if (_jpegData == null || _jpegData.Length == 0)
                {
                    using (var img = GetCvImage())
                    {
                        _jpegData = img.ToJpegData();
                    }
                }
                return _jpegData;
            }
            set { _jpegData = value; }
        }

        #region constructors
        public Image() { }

        public Image(byte[] rawData, int width, int height, DateTime fixationTime)
        {
            RawData = rawData;
            Width = width;
            Height = height;
            FixationTime = fixationTime;
        }
        #endregion

        public Image<Gray, byte> GetCvImage() =>
            new Image<Gray, byte>(Width, Height) { Bytes = RawData };
    }
}
