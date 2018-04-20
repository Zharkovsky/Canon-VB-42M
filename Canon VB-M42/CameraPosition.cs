namespace Canon_VB_M42
{
    public class CameraPosition
    {
        public virtual float X { get; set; }
        public virtual float Y { get; set; }
        public virtual float Z { get; set; }

        public override string ToString() => $"Position({X},{Y},{Z})";

        public static CameraPosition operator +(CameraPosition A, CameraPositionOffset B)
        {
            return new CameraPosition
            {
                X = A.X + B.DX,
                Y = A.Y + B.DY,
                Z = A.Z + B.DZ
            };
        }

        public static CameraPosition operator +(CameraPosition A, CameraPosition B)
        {
            return new CameraPosition
            {
                X = A.X + B.X,
                Y = A.Y + B.Y,
                Z = A.Z + B.Z
            };
        }

        public static CameraPositionOffset operator -(CameraPosition A, CameraPosition B)
        {
            return new CameraPositionOffset
            {
                DX = A.X - B.X,
                DY = A.Y - B.Y,
                DZ = A.Z - B.Z
            };
        }

        public CameraPosition Copy() => new CameraPosition { X = X, Y = Y, Z = Z };
    }
}