using System;
using System.Collections.Generic;
using System.Linq;

namespace Functions.TransformationConstituencyOS.EastingNorthingConversion
{
    public class CoordinateTransformation
    {
        private readonly double[][] shifts;
        public CoordinateTransformation()
        {
            string shiftTxt = getShifts();
            shifts = generateShiftTable(shiftTxt);
        }

        public double[][] TransformEastingNorthing(double[][] eastingNorthingPairs)
        {
            List<double[]> result = new List<double[]>();
            foreach (double[] enPair in eastingNorthingPairs)
                result.Add(eastingNorthingOSGB36ToETRS89Geo(enPair[0], enPair[1]));

            return result.ToArray();
        }

        private double[] eastingNorthingOSGB36ToETRS89Geo(double easting, double northing)
        {
            double e = easting;
            double n = northing;
            double previousE = easting;
            double previousN = northing;
            bool firstIteration = true;
            while (((previousE - e) > 0.0001) || ((previousN - n) > 0.0001) || (firstIteration))
            {
                firstIteration = false;
                int eIx = (int)Math.Floor(e / 1000);
                int nIx = (int)Math.Floor(n / 1000);

                double se0 = shifts[eIx + (nIx * 701)][0];
                double se1 = shifts[eIx + 1 + (nIx * 701)][0];
                double se2 = shifts[eIx + 1 + ((nIx + 1) * 701)][0];
                double se3 = shifts[eIx + ((nIx + 1) * 701)][0];
                double sn0 = shifts[eIx + (nIx * 701)][1];
                double sn1 = shifts[eIx + 1 + (nIx * 701)][1];
                double sn2 = shifts[eIx + 1 + ((nIx + 1) * 701)][1];
                double sn3 = shifts[eIx + ((nIx + 1) * 701)][1];

                double dx = e % 1000;
                double dy = n % 1000;
                double t = dx / 1000.0;
                double u = dy / 1000.0;

                double se = ((1 - t) * (1 - u) * se0) + (t * se1 * (1 - u)) + (t * u * se2) + (u * se3 * (1 - t));
                double sn = ((1 - t) * (1 - u) * sn0) + (t * sn1 * (1 - u)) + (t * u * sn2) + (u * sn3 * (1 - t));

                previousE = e;
                previousN = n;
                e = easting - se;
                n = northing - sn;
            }
            return new double[] { e, n };
        }

        private string getShifts()
        {
            string resourceName = "Functions.TransformationConstituencyOS.EastingNorthingConversion.ShiftReference.csv";
            string shiftTxt = null;
            using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                shiftTxt = reader.ReadToEnd();

            return shiftTxt;
        }

        private double[][] generateShiftTable(string shiftTxt)
        {
            double[][] shifts = shiftTxt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => new double[] { Convert.ToDouble(line.Split(',')[0]), Convert.ToDouble(line.Split(',')[1]) })
                .ToArray();

            return shifts;
        }
    }
}
