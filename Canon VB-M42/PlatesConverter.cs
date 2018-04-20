using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Canon_VB_M42
{
    public class PlatesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 5) return null;

            var width = 0.0;
            var height = 0.0;
            var actualWidth = 0.0;
            var actualHeight = 0.0;
            try
            {
                width = System.Convert.ToDouble(values[0]);
                height = System.Convert.ToDouble(values[1]);
                actualWidth = System.Convert.ToDouble(values[2]);
                actualHeight = System.Convert.ToDouble(values[3]);
            }
            catch
            {
                return null;
            }
            var plates = values[4] as IEnumerable<RecognizeResultDto>;

            return plates?.Select(
                p => new RecognizeResultDto
                {
                    PlateText = p.PlateText,
                    PlatePosition = new PlatePositionDto(
                        (int)Math.Round(p.PlatePosition.Top / height * actualHeight),
                        (int)Math.Round(p.PlatePosition.Left / width * actualWidth),
                        (int)Math.Round(p.PlatePosition.Width / width * actualWidth),
                        (int)Math.Round(p.PlatePosition.Height / height * actualHeight))
                })?.ToList();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
