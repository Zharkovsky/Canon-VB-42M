namespace Canon_VB_M42
{
    public class PlatePositionDto
    {
        public int Top { get; set; }
        public int Left { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public PlatePositionDto() { }

        public PlatePositionDto(int top, int left, int width, int height)
        {
            Top = top;
            Left = left;
            Width = width;
            Height = height;
        }
    }
}