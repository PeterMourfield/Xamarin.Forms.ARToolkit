using System;
namespace Xamarin.Forms.ARToolkit
{
    public static class MathHelpers
    {
        const double DEGREES_TO_RADIANS = Math.PI / 180.0;
        const double WGS84_A = 6378137.0;
        const double WGS84_E = 8.1819190842622e-2;

        public static void CreateProjectionMatrix(ref float[] mout, float aspect, float zNear, float zFar)
        {
            float fovy = (float)(60.0 * DEGREES_TO_RADIANS);
            float f = (float)(1.0 / Math.Tan(fovy / 2.0));

            mout[0] = f / aspect;
            mout[1] = 0.0f;
            mout[2] = 0.0f;
            mout[3] = 0.0f;

            mout[4] = 0.0f;
            mout[5] = f;
            mout[6] = 0.0f;
            mout[7] = 0.0f;

            mout[8] = 0.0f;
            mout[9] = 0.0f;
            mout[10] = (zFar + zNear) / (zNear - zFar);
            mout[11] = -1.0f;

            mout[12] = 0.0f;
            mout[13] = 0.0f;
            mout[14] = 2 * zFar * zNear / (zNear - zFar);
            mout[15] = 0.0f;
        }

        public static void MultiplyMatrixAndVector(ref float[] vout, float[] m, float[] v)
        {
            vout[0] = m[0] * v[0] + m[4] * v[1] + m[8] * v[2] + m[12] * v[3];
            vout[1] = m[1] * v[0] + m[5] * v[1] + m[9] * v[2] + m[13] * v[3];
            vout[2] = m[2] * v[0] + m[6] * v[1] + m[10] * v[2] + m[14] * v[3];
            vout[3] = m[3] * v[0] + m[7] * v[1] + m[11] * v[2] + m[15] * v[3];
        }

        public static void MultiplyMatrixAndMatrix(ref float[] c, float[] a, float[] b)
        {
            for (int col = 0; col < 4; col++)
            {
                for (int row = 0; row < 4; row++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        c[col * 4 + row] += a[i * 4 + row] * b[col * 4 + i];
                    }
                }
            }
        }

        public static void LatLonToEcef(double lat, double lon, double alt, ref double x, ref double y, ref double z)
        {
            double clat = Math.Cos(lat * DEGREES_TO_RADIANS);
            double slat = Math.Sin(lat * DEGREES_TO_RADIANS);
            double clon = Math.Cos(lon * DEGREES_TO_RADIANS);
            double slon = Math.Sin(lon * DEGREES_TO_RADIANS);

            double N = WGS84_A / Math.Sqrt(1.0 - WGS84_E * WGS84_E * slat * slat);

            x = (N + alt) * clat * clon;
            y = (N + alt) * clat * slon;
            z = (N + (1.0 - WGS84_E * WGS84_E) + alt) * slat;
        }

        public static void EcefToEnu(double lat, double lon, double x, double y, double z, double xr, double yr, double zr, ref double e, ref double n, ref double u)
        {
            double clat = Math.Cos(lat * DEGREES_TO_RADIANS);
            double slat = Math.Sin(lat * DEGREES_TO_RADIANS);
            double clon = Math.Cos(lon * DEGREES_TO_RADIANS);
            double slon = Math.Sin(lon * DEGREES_TO_RADIANS);
            double dx = x - xr;
            double dy = y - yr;
            double dz = z - zr;

            e = -slon * dx + clon * dy;
            n = -slat * clon * dx - slat * slon * dy + clat * dz;
            u = clat * clon * dx + clat * slon * dy + slat * dz;
        }
    }
}
