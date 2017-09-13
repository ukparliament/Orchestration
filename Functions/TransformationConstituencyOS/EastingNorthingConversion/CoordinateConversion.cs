using System;
using System.Collections.Generic;

namespace Functions.TransformationConstituencyOS.EastingNorthingConversion
{
    public class CoordinateConversion
    {
        private readonly double n0 = -100000.0;
        private readonly double e0 = 400000.0;
        private readonly double a = 6377563.396;
        private readonly double f0 = 0.9996012717;
        private readonly double phi0 = (49.0 * Math.PI) / 180.0;
        private readonly double b = 6356256.909;
        private readonly double lambda0 = (-2.0 * Math.PI) / 180.0;

        public double[][] ConvertToLongitudeLatitude(double[][] eastingNorthingPairs)
        {
            List<double[]> result = new List<double[]>();
            foreach (double[] enPair in eastingNorthingPairs)
                result.Add(getLongLat(enPair[0], enPair[1]));

            return result.ToArray();
        }

        private double[] getLongLat(double easting, double northing)
        {
            double phi1 = ((northing - n0) / (a * f0)) + phi0;
            double marc = meridonalArc(phi1);
            double phi2 = ((northing - n0 - marc) / (a * f0)) + phi1;

            while (Math.Abs(northing - n0 - marc) > 0.00001)
            {
                phi2 = ((northing - n0 - marc) / (a * f0)) + phi1;
                marc = meridonalArc(phi2);
                phi1 = phi2;
            }
            double initialPhi = phi2;
            double et = easting - e0;
            double aF0 = a * f0;
            double esqr = (Math.Pow(aF0, 2) - Math.Pow(b * f0, 2)) / Math.Pow(aF0, 2);
            double niu = aF0 / Math.Sqrt(1 - (esqr * Math.Pow(Math.Sin(initialPhi), 2)));
            double ro = (niu * (1 - esqr)) / (1 - (esqr * Math.Pow(Math.Sin(initialPhi), 2)));
            double step7 = (Math.Tan(initialPhi)) / (2 * niu * ro);
            double eta2 = (niu / ro) - 1;
            double step8 = ((Math.Tan(initialPhi)) / (24 * ro * Math.Pow(niu, 3))) * (5 + (3 * Math.Pow(Math.Tan(initialPhi), 2)) + eta2 - (9 * Math.Pow(Math.Tan(initialPhi), 2) * eta2));
            double step9 = ((Math.Tan(initialPhi)) / (720 * ro * Math.Pow(niu, 5))) * (61 + (90 * Math.Pow(Math.Tan(initialPhi), 2)) + (45 * Math.Pow(Math.Tan(initialPhi), 4)));
            double radianN = (initialPhi - (Math.Pow(et, 2) * step7) + (Math.Pow(et, 4) * step8) - (Math.Pow(et, 6) * step9));

            double latitude = (radianN * 180) / Math.PI;

            double step10 = Math.Pow(Math.Cos(initialPhi), -1) / niu;
            double step11 = (Math.Pow(Math.Cos(initialPhi), -1) / (6 * Math.Pow(niu, 3))) * ((niu / ro) + (2 * Math.Pow(Math.Tan(initialPhi), 2)));
            double step12 = (Math.Pow(Math.Cos(initialPhi), -1) / (120 * Math.Pow(niu, 5))) * (5 + (28 * Math.Pow(Math.Tan(initialPhi), 2)) + (24 * Math.Pow(Math.Tan(initialPhi), 4)));
            double step12a = (Math.Pow(Math.Cos(initialPhi), -1) / (5040 * Math.Pow(niu, 7))) * (61 + (662 * Math.Pow(Math.Tan(initialPhi), 2)) + (1320 * Math.Pow(Math.Tan(initialPhi), 4)) + (720 * Math.Pow(Math.Tan(initialPhi), 6)));
            double radianE = lambda0 + (et * step10) - (Math.Pow(et, 3) * step11) + (Math.Pow(et, 5) * step12) - (Math.Pow(et, 7) * step12a);

            double longitude = (radianE * 180) / Math.PI;

            double[] result = { longitude, latitude };
            return result;
        }

        private double meridonalArc(double phi1)
        {
            double aF0 = a * f0;
            double bF0 = b * f0;
            double n = (aF0 - bF0) / (aF0 + bF0);
            double n2 = Math.Pow(n, 2);
            double n3 = Math.Pow(n, 3);

            return bF0 * (((1 + n + ((5.0 / 4.0) * n2) + ((5.0 / 4.0) * n3)) * (phi1 - phi0)) - (((3 * n) + (3 * n2) + ((21.0 / 8.0) * n3)) * (Math.Sin(phi1 - phi0)) * (Math.Cos(phi1 + phi0))) + ((((15.0 / 8.0) * n2) + ((15.0 / 8.0) * n3)) * (Math.Sin(2 * (phi1 - phi0))) * (Math.Cos(2 * (phi1 + phi0)))) - (((35.0 / 24.0) * n3) * (Math.Sin(3 * (phi1 - phi0))) * (Math.Cos(3 * (phi1 + phi0)))));
        }
    }

}
