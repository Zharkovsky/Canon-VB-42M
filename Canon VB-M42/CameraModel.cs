namespace Canon_VB_M42
{
    public class CameraModel
    {
        /// <summary>
        /// Наименование модели
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Путь к видеопотоку. {login}{password}{ip[:port]}
        /// </summary>
        public string VideoStreamPath { get; set; }

        /// <summary>
        /// Путь к ptz.cgi
        /// </summary>
        public string PtzCgi { get; set; }

        /// <summary>
        /// Формат команды для изменения позиции камеры {pan}{tilt}{zoom}.
        /// Не принимает параметров
        /// </summary>
        public string GoToPositionCommand { get; set; }

        /// <summary>
        /// Формат команды для получения текущей позиции камеры.
        /// Не принимает параметров
        /// </summary>
        public string GetPositionCommand { get; set; }

        /// <summary>
        /// Имя переменной для парсинга pan
        /// </summary>
        public string ParsePositionPan { get; set; }
        /// <summary>
        /// Имя переменной для парсинга tilt
        /// </summary>
        public string ParsePositionTilt { get; set; }
        /// <summary>
        /// Имя переменной для парсинга zoom
        /// </summary>
        public string ParsePositionZoom { get; set; }
        /// <summary>
        /// Разделитель для парсинга значений позиции
        /// </summary>
        public string ParsePositionSeparator { get; set; }

        /// <summary>
        /// Минимальное смещение по вертикали
        /// </summary>
        public int MinTiltValue { get; set; }
        /// <summary>
        /// Максимальное смещение по вертикали
        /// </summary>
        public int MaxTiltValue { get; set; }
        /// <summary>
        /// Минимльное смещение по горизонтали
        /// </summary>
        public int MinPanValue { get; set; }
        /// <summary>
        /// Максимальное смещение по горизонтали
        /// </summary>
        public int MaxPanValue { get; set; }
        /// <summary>
        /// Минимальное значение уровня зума
        /// </summary>
        public int MinZoomValue { get; set; }
        /// <summary>
        /// Максимальное значение уровня зума
        /// </summary>
        public int MaxZoomValue { get; set; }
        /// <summary>
        /// Кратность зума
        /// </summary>
        public int FoldZoom { get; set; }
        /// <summary>
        /// Минимальное фокусное расстояние 
        /// </summary>
        public double MinFocalLength { get; set; }
        /// <summary>
        /// Максимальное фокусное расстояние 
        /// </summary>
        public double MaxFocalLength { get; set; }
        /// <summary>
        /// Соотношение сторон - ширина
        /// </summary>
        public int AspectRatioWidth { get; set; }
        /// <summary>
        /// Соотношение сторон - высота
        /// </summary>
        public int AspectRatioHeight { get; set; }
        /// <summary>
        /// Размер матрицы камеры в дюймах
        /// </summary>
        public double MatrixSize { get; set; }


        /// <summary>
        /// Путь к info.cgi
        /// </summary>
        public string InfoCgi { get; set; }
        /// <summary>
        /// Кратность смещения по горизонтали и вертикали
        /// </summary>
        public float OffsetMultiplicity { get; set; }
        /// <summary>
        /// Обратный отсчет по горизонтали
        /// </summary>
        public bool PanReverse { get; set; }
        /// <summary>
        /// Обратный отсчет по вертикали
        /// </summary>
        public bool TiltReverse { get; set; }
        /// <summary>
        /// Обратный отсчет увеличения
        /// </summary>
        public bool ZoomReverse { get; set; }
        /// <summary>
        /// Выражение для подсчет фокусного расстояния
        /// </summary>
        public string FocalLengthByZoomStepsExpression { get; set; }
        /// <summary>
        /// Выражение для подсчет фокусного расстояния
        /// </summary>
        public string ZoomStepsParameter { get; set; }
        /// <summary>
        /// Выражение для подсчет ZoomSteps
        /// </summary>
        public string ZoomStepsByFocalLengthExpression { get; set; }
        /// <summary>
        /// Выражение для подсчет ZoomSteps
        /// </summary>
        public string FocalLengthParameter { get; set; }
    }
}