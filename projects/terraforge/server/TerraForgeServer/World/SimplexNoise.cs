using System;

namespace TerraForgeServer.World
{
    /// <summary>
    /// OpenSimplex noise implementation for terrain generation
    /// </summary>
    public class SimplexNoise
    {
        private readonly int[] _perm = new int[512];
        private readonly int[] _perm3D = new int[512];
        private const double SKEW_2D = -0.21132486540518708;
        private const double UNSKEW_2D = 0.21132486540518713;
        private const double SKEW_3D = 1.0 / 3.0;
        private const double UNSKEW_3D = 1.0 / 6.0;
        
        private static readonly int[] Grad3 = {
            1, 1, 0,  -1, 1, 0,  1,-1, 0,  -1,-1, 0,
            1, 0, 1,  -1, 0, 1,  1, 0,-1,  -1, 0,-1,
            0, 1, 1,   0,-1, 1,  0, 1,-1,   0,-1,-1
        };
        
        public SimplexNoise(int seed)
        {
            var random = new Random(seed);
            var source = new int[256];
            
            for (int i = 0; i < 256; i++)
                source[i] = i;
                
            for (int i = 255; i >= 0; i--)
            {
                int r = random.Next(i + 1);
                (source[i], source[r]) = (source[r], source[i]);
            }
            
            for (int i = 0; i < 512; i++)
            {
                _perm[i] = source[i & 255];
                _perm3D[i] = _perm[i] % 12;
            }
        }
        
        /// <summary>
        /// 2D noise value -1 to 1
        /// </summary>
        public double Noise2D(double x, double y)
        {
            double s = (x + y) * SKEW_2D;
            int i = FastFloor(x + s);
            int j = FastFloor(y + s);
            double t = (i + j) * UNSKEW_2D;
            double X0 = i - t;
            double Y0 = j - t;
            double x0 = x - X0;
            double y0 = y - Y0;
            
            int i1, j1;
            if (x0 > y0) { i1 = 1; j1 = 0; }
            else { i1 = 0; j1 = 1; }
            
            double x1 = x0 - i1 + UNSKEW_2D;
            double y1 = y0 - j1 + UNSKEW_2D;
            double x2 = x0 - 1.0 + 2.0 * UNSKEW_2D;
            double y2 = y0 - 1.0 + 2.0 * UNSKEW_2D;
            
            int ii = i & 255;
            int jj = j & 255;
            
            double n0 = 0, n1 = 0, n2 = 0;
            
            double t0 = 0.5 - x0 * x0 - y0 * y0;
            if (t0 >= 0)
            {
                t0 *= t0;
                int gi = _perm[ii + _perm[jj]] % 12;
                n0 = t0 * t0 * Dot(Grad3, gi, x0, y0, 0);
            }
            
            double t1 = 0.5 - x1 * x1 - y1 * y1;
            if (t1 >= 0)
            {
                t1 *= t1;
                int gi = _perm[ii + i1 + _perm[jj + j1]] % 12;
                n1 = t1 * t1 * Dot(Grad3, gi, x1, y1, 0);
            }
            
            double t2 = 0.5 - x2 * x2 - y2 * y2;
            if (t2 >= 0)
            {
                t2 *= t2;
                int gi = _perm[ii + 1 + _perm[jj + 1]] % 12;
                n2 = t2 * t2 * Dot(Grad3, gi, x2, y2, 0);
            }
            
            return 70.0 * (n0 + n1 + n2);
        }
        
        /// <summary>
        /// 3D noise for caves
        /// </summary>
        public double Noise3D(double x, double y, double z)
        {
            double s = (x + y + z) * SKEW_3D;
            int i = FastFloor(x + s);
            int j = FastFloor(y + s);
            int k = FastFloor(z + s);
            double t = (i + j + k) * UNSKEW_3D;
            double X0 = i - t;
            double Y0 = j - t;
            double Z0 = k - t;
            double x0 = x - X0;
            double y0 = y - Y0;
            double z0 = z - Z0;
            
            int i1, j1, k1;
            int i2, j2, k2;
            
            if (x0 >= y0)
            {
                if (y0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
                else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; }
                else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; }
            }
            else
            {
                if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; }
                else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; }
                else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; }
            }
            
            double x1 = x0 - i1 + UNSKEW_3D;
            double y1 = y0 - j1 + UNSKEW_3D;
            double z1 = z0 - k1 + UNSKEW_3D;
            double x2 = x0 - i2 + 2.0 * UNSKEW_3D;
            double y2 = y0 - j2 + 2.0 * UNSKEW_3D;
            double z2 = z0 - k2 + 2.0 * UNSKEW_3D;
            double x3 = x0 - 1.0 + 3.0 * UNSKEW_3D;
            double y3 = y0 - 1.0 + 3.0 * UNSKEW_3D;
            double z3 = z0 - 1.0 + 3.0 * UNSKEW_3D;
            
            int ii = i & 255;
            int jj = j & 255;
            int kk = k & 255;
            
            double n = 0;
            
            double t0 = 0.6 - x0 * x0 - y0 * y0 - z0 * z0;
            if (t0 > 0) { t0 *= t0; n += t0 * t0 * Dot3D(_perm3D[ii + _perm[jj + _perm[kk]]], x0, y0, z0); }
            
            double t1 = 0.6 - x1 * x1 - y1 * y1 - z1 * z1;
            if (t1 > 0) { t1 *= t1; n += t1 * t1 * Dot3D(_perm3D[ii + i1 + _perm[jj + j1 + _perm[kk + k1]]], x1, y1, z1); }
            
            double t2 = 0.6 - x2 * x2 - y2 * y2 - z2 * z2;
            if (t2 > 0) { t2 *= t2; n += t2 * t2 * Dot3D(_perm3D[ii + i2 + _perm[jj + j2 + _perm[kk + k2]]], x2, y2, z2); }
            
            double t3 = 0.6 - x3 * x3 - y3 * y3 - z3 * z3;
            if (t3 > 0) { t3 *= t3; n += t3 * t3 * Dot3D(_perm3D[ii + 1 + _perm[jj + 1 + _perm[kk + 1]]], x3, y3, z3); }
            
            return 32.0 * n;
        }
        
        private static int FastFloor(double x) => x >= 0 ? (int)x : (int)x - 1;
        
        private static double Dot(int[] grad, int i, double x, double y, double z) =>
            grad[i * 3] * x + grad[i * 3 + 1] * y + grad[i * 3 + 2] * z;
            
        private static double Dot3D(int gi, double x, double y, double z) =>
            Grad3[gi * 3] * x + Grad3[gi * 3 + 1] * y + Grad3[gi * 3 + 2] * z;
        
        /// <summary>
        /// Fractal Brownian Motion for layered terrain
        /// </summary>
        public double FBM2D(double x, double y, int octaves, double persistence, double lacunarity)
        {
            double total = 0;
            double frequency = 1;
            double amplitude = 1;
            double maxValue = 0;
            
            for (int i = 0; i < octaves; i++)
            {
                total += Noise2D(x * frequency, y * frequency) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }
            
            return total / maxValue;
        }
        
        public double FBM3D(double x, double y, double z, int octaves, double persistence, double lacunarity)
        {
            double total = 0;
            double frequency = 1;
            double amplitude = 1;
            double maxValue = 0;
            
            for (int i = 0; i < octaves; i++)
            {
                total += Noise3D(x * frequency, y * frequency, z * frequency) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }
            
            return total / maxValue;
        }
    }
}
