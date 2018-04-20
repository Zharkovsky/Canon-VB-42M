namespace Canon_VB_M42
{
    public class CameraPositionOffset
    {
        public float DX { get; set; }
        public float DY { get; set; }
        public float DZ { get; set; }

        public CameraPositionOffset() { }

        public CameraPositionOffset(float dx, float dy, float dz)
        {
            DX = dx; DY = dy; DZ = dz;
        }

        public override string ToString() => $"Offset({DX},{DY},{DZ})";

        public static CameraPositionOffset operator +(CameraPositionOffset A, CameraPositionOffset B)
        {
            return new CameraPositionOffset
            {
                DX = A.DX + B.DX,
                DY = A.DY + B.DY,
                DZ = A.DZ + B.DZ
            };
        }

        public static CameraPositionOffset operator -(CameraPositionOffset A, CameraPositionOffset B)
        {
            return new CameraPositionOffset
            {
                DX = A.DX - B.DX,
                DY = A.DY - B.DY,
                DZ = A.DZ - B.DZ
            };
        }

        public static CameraPositionOffset operator *(CameraPositionOffset A, float r)
        {
            return new CameraPositionOffset
            {
                DX = A.DX * r,
                DY = A.DY * r,
                DZ = A.DZ * r
            };
        }

        public static CameraPositionOffset operator /(CameraPositionOffset A, float r)
        {
            return new CameraPositionOffset
            {
                DX = A.DX / r,
                DY = A.DY / r,
                DZ = A.DZ / r
            };
        }
    }
}