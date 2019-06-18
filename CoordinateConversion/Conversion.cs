using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordinateConversion
{
    struct PointBLH
    {
        public string Name;
        public double B;
        public double L;
        public double H;
    }

    struct PointBLL0
    {
        public string Name;
        public double B;
        public double L;
        public double L0;
    }

    struct PointXYZ
    {
        public string Name;
        public double X;
        public double Y;
        public double Z;
    }

    struct PointXYL0
    {
        public string Name;
        public double X;
        public double Y;
        public double L0;
    }

    struct Ellispoid
    {
        public double a;
        public double b;
        public double f;
        public double e1;
        public double e2;
        public double W;
        public double n;
        public double t;
        public double N;
        public double M;
        public double M0;
    }

    class Conversion
    {
        public double Angle2Radian(double data)
        {
            string du, fen, miao, mid;
            string str = data.ToString("0.000000");
            double angle, radian;
            mid = str.Substring(0,3);
            if (mid[2] == '.')
            {
                du = str.Substring(0, 2); 
                fen = str.Substring(3, 2);
                miao = str.Substring(5, str.Length - 5);
            }
            else
            {
                du = str.Substring(0, 3);
                fen = str.Substring(4, 2);
                miao = str.Substring(6, str.Length - 6);
            }
            angle = double.Parse(du) + double.Parse(fen) / 60.0 + (double.Parse(miao) / 100.0) / 3600.0;
            radian = angle * Math.PI / 180.0;
            return radian;
        }
        public double Radian2Angle(double data)
        {
            double angle = data * 180.0 / Math.PI;
            if (angle < 0)
                angle = 180 + angle;
            return angle;
        }
        public string Angle2DMS(double data)
        {
            string DMS;
            double du, fen, miao;
            string strdu, strfen, strmiao;
            du = Math.Floor(data);
            fen = (data - du) * 60;
            miao = (fen - Math.Floor(fen)) * 60;
            fen = Math.Floor(fen);
            if(miao >= 60)
            {
                miao = miao - 60;
                fen += 1;
            }
            if(fen >= 60)
            {
                fen = fen - 60;
                du += 1;
            }
            strdu = du.ToString();
            if (fen < 10)
                strfen = "0" + fen.ToString();
            else
                strfen = fen.ToString();
            if (miao < 10)
                strmiao = "0" + miao.ToString("0.0000");
            else
                strmiao = miao.ToString("0.0000");
            DMS = strdu + "°" + strfen + "′" + strmiao + "″";
            return DMS;
        }
        public int CalculateEllispoid(ref Ellispoid E, double B)
        {
            E.b = E.a - E.f * E.a;
            E.e1 = Math.Sqrt((Math.Pow(E.a, 2) - Math.Pow(E.b, 2)) / Math.Pow(E.a, 2));
            E.e2 = Math.Sqrt(Math.Pow(E.e1, 2) / (1 - Math.Pow(E.e1, 2)));
            E.W = Math.Sqrt(1 - Math.Pow(E.e1, 2) * Math.Pow(Math.Sin(B), 2));
            E.n = Math.Sqrt(Math.Pow(E.e2, 2) * Math.Pow(Math.Cos(B), 2));
            E.t = Math.Tan(B);
            E.N = E.a / E.W;
            E.M = E.a * (1 - Math.Pow(E.e1, 2)) / Math.Pow(E.W, 3);
            E.M0 = E.a * (1 - Math.Pow(E.e1, 2));
            return 0;
        }
        public int CalculateEllispoid(ref Ellispoid E)
        {
            E.b = E.a - E.f * E.a;
            E.e1 = Math.Sqrt((Math.Pow(E.a, 2) - Math.Pow(E.b, 2)) / Math.Pow(E.a, 2));
            E.e2 = Math.Sqrt(Math.Pow(E.e1, 2) / (1 - Math.Pow(E.e1, 2)));
            E.M0 = E.a * (1 - Math.Pow(E.e1, 2));
            return 0;
        }
        public int BLH2XYZ(PointBLH blh, Ellispoid E, ref PointXYZ xyz)
        {
            xyz.Name = blh.Name;
            xyz.X = (E.N + blh.H) * Math.Cos(blh.B) * Math.Cos(blh.L);
            xyz.Y = (E.N + blh.H) * Math.Cos(blh.B) * Math.Sin(blh.L);
            xyz.Z = (E.N * (1 - Math.Pow(E.e1, 2)) + blh.H) * Math.Sin(blh.B);
            return 0;
        }
        public int XYZ2BLH(PointXYZ xyz, double a, double f, ref PointBLH blh)
        {
            double error = 0;
            Ellispoid E = new Ellispoid();
            E.a = a;
            E.f = f;
            blh.Name = xyz.Name;
            double x = xyz.X + error;
            double y = xyz.Y + error;
            double z = xyz.Z + error;
            double B0 = Math.Atan(z / Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)));
            blh.L = Math.Atan(y / x);
            while (true)
            {
                CalculateEllispoid(ref E, B0);
                blh.B = Math.Atan((z + E.N * Math.Pow(E.e1, 2) * Math.Sin(B0)) / Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)));
                if (Math.Abs(blh.B - B0) <= Math.Pow(10.0, -15.0))
                    break;
                else
                    B0 = blh.B;
            }
            blh.H = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)) / Math.Cos(blh.B) - E.N;
            return 0;
        }
        public int BL2XY(PointBLL0 bll0, Ellispoid E, ref PointXYL0 xyl0)
        {
            xyl0.L0 = bll0.L0;
            double error = 0;
            CalculateEllispoid(ref E, bll0.B);
            double Ac, Bc, Cc, Dc, Ec, Fc, alpha, beta, gamma, delta, epsilon, zeta, X, l;
            double a0, a1, a2, a3, a4, a5, a6;
            Ac = 1 + 3.0 / 4.0 * Math.Pow(E.e1, 2) + 45.0 / 64.0 * Math.Pow(E.e1, 4) 
                + 175.0 / 256.0 * Math.Pow(E.e1, 6) + 11025.0 / 16384.0 * Math.Pow(E.e1, 8) 
                + 43659.0 / 65536.0 * Math.Pow(E.e1, 10);
            Bc = 3.0 / 4.0 * Math.Pow(E.e1, 2) + 15.0 / 16.0 * Math.Pow(E.e1, 4) 
                + 525.0 / 512.0 * Math.Pow(E.e1, 6) + 2205.0 / 2048.0 * Math.Pow(E.e1, 8) 
                + 72765.0 / 65536.0 * Math.Pow(E.e1, 10);
            Cc = 15.0 / 64.0 * Math.Pow(E.e1, 4) + 105.0 / 256.0 * Math.Pow(E.e1, 6) 
                + 2205.0 / 4096.0 * Math.Pow(E.e1, 8) + 10395.0 / 16384.0 * Math.Pow(E.e1, 10);
            Dc = 35.0 / 512.0 * Math.Pow(E.e1, 6) + 315.0 / 2048.0 * Math.Pow(E.e1, 8) 
                + 31185.0 / 131072.0 * Math.Pow(E.e1, 10);
            Ec = 315.0 / 16384.0 * Math.Pow(E.e1, 8) + 3465.0 / 65536.0 * Math.Pow(E.e1, 10);
            Fc = 693.0 / 131072.0 * Math.Pow(E.e1, 10);
            alpha = Ac * E.M0;
            beta = -1.0 / 2.0 * Bc * E.M0;
            gamma = 1.0 / 4.0 * Cc * E.M0;
            delta = -1.0 / 6.0 * Dc * E.M0;
            epsilon = 1.0 / 8.0 * Ec * E.M0;
            zeta = -1.0 / 10.0 * Fc * E.M0;
            X = alpha * bll0.B + beta * Math.Sin(2 * bll0.B) + gamma * Math.Sin(4 * bll0.B) 
                + delta * Math.Sin(6 * bll0.B) + epsilon * Math.Sin(8 * bll0.B) + zeta * Math.Sin(10 * bll0.B);
            l = bll0.L - bll0.L0;
            a0 = X;
            a1 = E.N * Math.Cos(bll0.B);
            a2 = 1.0 / 2.0 * E.N * Math.Pow(Math.Cos(bll0.B), 2) * E.t;
            a3 = 1.0 / 6.0 * E.N * Math.Pow(Math.Cos(bll0.B), 3) * (1 - Math.Pow(E.t, 2) + Math.Pow(E.n, 2));
            a4 = 1.0 / 24.0 * E.N * Math.Pow(Math.Cos(bll0.B), 4) * (5 - Math.Pow(E.t, 2) 
                + 9 * Math.Pow(E.n, 2) + 4 * Math.Pow(E.n, 4)) * E.t;
            a5 = 1.0 / 120.0 * E.N * Math.Pow(Math.Cos(bll0.B), 5) * (5 - 18 * Math.Pow(E.t, 2) 
                + Math.Pow(E.t, 4) + 14 * Math.Pow(E.n, 2) - 58 * Math.Pow(E.n, 2) * Math.Pow(E.t, 2));
            a6 = 1.0 / 720.0 * E.N * Math.Pow(Math.Cos(bll0.B), 6) * (61 - 58 * Math.Pow(E.t, 2) 
                + Math.Pow(E.t, 4) + 270 * Math.Pow(E.n, 2) - 330 * Math.Pow(E.n, 2) * Math.Pow(E.t, 2)) * E.t;
            xyl0.Name = bll0.Name;
            xyl0.X = a0 * Math.Pow(l, 0) + a2 * Math.Pow(l, 2) + a4 * Math.Pow(l, 4) + a6 * Math.Pow(l, 6);
            xyl0.Y = a1 * Math.Pow(l, 1) + a3 * Math.Pow(l, 3) + a5 * Math.Pow(l, 5) + error;
            return 0;
        }
        public int XY2BL(PointXYL0 xyl0, Ellispoid E, ref PointBLL0 bll0)
        {
            bll0.L0 = xyl0.L0;
            double error = 0;
            double x = xyl0.X + error;
            double y = xyl0.Y + error;
            CalculateEllispoid(ref E);
            double Ac, Bc, Cc, Dc, Ec, Fc, alpha, beta, gamma, delta, epsilon, zeta, X, B0, Bf;
            double b0, b1, b2, b3, b4, b5, b6;
            Ac = 1 + 3.0 / 4.0 * Math.Pow(E.e1, 2) + 45.0 / 64.0 * Math.Pow(E.e1, 4)
                + 175.0 / 256.0 * Math.Pow(E.e1, 6) + 11025.0 / 16384.0 * Math.Pow(E.e1, 8)
                + 43659.0 / 65536.0 * Math.Pow(E.e1, 10);
            Bc = 3.0 / 4.0 * Math.Pow(E.e1, 2) + 15.0 / 16.0 * Math.Pow(E.e1, 4)
                + 525.0 / 512.0 * Math.Pow(E.e1, 6) + 2205.0 / 2048.0 * Math.Pow(E.e1, 8)
                + 72765.0 / 65536.0 * Math.Pow(E.e1, 10);
            Cc = 15.0 / 64.0 * Math.Pow(E.e1, 4) + 105.0 / 256.0 * Math.Pow(E.e1, 6)
                + 2205.0 / 4096.0 * Math.Pow(E.e1, 8) + 10395.0 / 16384.0 * Math.Pow(E.e1, 10);
            Dc = 35.0 / 512.0 * Math.Pow(E.e1, 6) + 315.0 / 2048.0 * Math.Pow(E.e1, 8)
                + 31185.0 / 131072.0 * Math.Pow(E.e1, 10);
            Ec = 315.0 / 16384.0 * Math.Pow(E.e1, 8) + 3465.0 / 65536.0 * Math.Pow(E.e1, 10);
            Fc = 693.0 / 131072.0 * Math.Pow(E.e1, 10);
            alpha = Ac * E.M0;
            beta = -1.0 / 2.0 * Bc * E.M0;
            gamma = 1.0 / 4.0 * Cc * E.M0;
            delta = -1.0 / 6.0 * Dc * E.M0;
            epsilon = 1.0 / 8.0 * Ec * E.M0;
            zeta = -1.0 / 10.0 * Fc * E.M0;
            X = x;
            B0 = X / alpha;
            while (true)
            {
                double triangle = beta * Math.Sin(2 * B0) + gamma * Math.Sin(4 * B0)
                    + delta * Math.Sin(6 * B0) + epsilon * Math.Sin(8 * B0) + zeta * Math.Sin(10 * B0);
                Bf = (X - triangle) / alpha;
                if (Math.Abs(Bf - B0) <= Math.Pow(10.0, -15.0))
                    break;
                else
                    B0 = Bf;
            }
            CalculateEllispoid(ref E, Bf);
            b0 = Bf;
            b1 = 1 / (E.N * Math.Cos(Bf));
            b2 = -E.t / (2 * E.M * E.N);
            b3 = -(1 + 2 * Math.Pow(E.t, 2) + Math.Pow(E.n, 2)) / (6 * Math.Pow(E.N, 2)) * b1;
            b4 = -(5 + 3 * Math.Pow(E.t, 2) + Math.Pow(E.n, 2) 
                - 9 * Math.Pow(E.n, 2) * Math.Pow(E.t, 2)) / (12 * Math.Pow(E.N, 2)) * b2;
            b5 = -(5 + 28 * Math.Pow(E.t, 2) + 24 * Math.Pow(E.t, 4) + 6 * Math.Pow(E.n, 2) 
                + 8 * Math.Pow(E.n, 2) * Math.Pow(E.t, 2)) / (120 * Math.Pow(E.N, 4)) * b1;
            b6 = (61 + 90 * Math.Pow(E.t, 2) + 45 * Math.Pow(E.t, 4)) / (360 * Math.Pow(E.N, 4)) * b2;
            bll0.Name = xyl0.Name;
            bll0.B = b0 * Math.Pow(y, 0) + b2 * Math.Pow(y, 2) + b4 * Math.Pow(y, 4) + b6 * Math.Pow(y, 6);
            bll0.L = b1 * Math.Pow(y, 1) + b3 * Math.Pow(y, 3) + b5 * Math.Pow(y, 5) + bll0.L0;
            return 0;
        }
    }
}
