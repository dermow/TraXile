using System;
using System.Linq;
using System.Windows.Media;

namespace TraXile.Next.Tests
{
    /// <summary>
    /// The computable checks of the categorical palette validator (dataviz method): OKLab / OKLCH,
    /// Delta E (OKLab x 100) and color vision deficiency simulated with Machado, Oliveira and Fernandes (2009)
    /// at severity 1.0 in linear RGB.
    /// </summary>
    internal static class ColorMetrics
    {
        private static readonly double[,] Protan =
        {
            { 0.152286, 1.052583, -0.204868 },
            { 0.114503, 0.786281, 0.099216 },
            { -0.003882, -0.048116, 1.051998 },
        };

        private static readonly double[,] Deutan =
        {
            { 0.367322, 0.860646, -0.227968 },
            { 0.280085, 0.672501, 0.047413 },
            { -0.011820, 0.042940, 0.968881 },
        };

        public enum Vision { Normal, Protan, Deutan }

        private static double ToLinear(byte v)
        {
            double c = v / 255.0;
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        private static double[] Linear(Color c) => new[] { ToLinear(c.R), ToLinear(c.G), ToLinear(c.B) };

        private static double[] Simulate(double[] rgb, double[,] m) =>
            Enumerable.Range(0, 3)
                .Select(i => Math.Max(0, Math.Min(1, m[i, 0] * rgb[0] + m[i, 1] * rgb[1] + m[i, 2] * rgb[2])))
                .ToArray();

        private static double[] OkLab(double[] r)
        {
            double l = Math.Pow(0.4122214708 * r[0] + 0.5363325363 * r[1] + 0.0514459929 * r[2], 1.0 / 3);
            double m = Math.Pow(0.2119034982 * r[0] + 0.6806995451 * r[1] + 0.1073969566 * r[2], 1.0 / 3);
            double s = Math.Pow(0.0883024619 * r[0] + 0.2817188376 * r[1] + 0.6299787005 * r[2], 1.0 / 3);
            return new[]
            {
                0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s,
                1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s,
                0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s,
            };
        }

        /// <summary>OKLCH lightness and chroma.</summary>
        public static (double L, double C) OkLch(Color c)
        {
            double[] lab = OkLab(Linear(c));
            return (lab[0], Math.Sqrt(lab[1] * lab[1] + lab[2] * lab[2]));
        }

        public static double DeltaE(Color a, Color b, Vision vision = Vision.Normal)
        {
            double[] la = Linear(a), lb = Linear(b);
            if (vision != Vision.Normal)
            {
                double[,] m = vision == Vision.Protan ? Protan : Deutan;
                la = Simulate(la, m);
                lb = Simulate(lb, m);
            }
            double[] x = OkLab(la), y = OkLab(lb);
            return 100 * Math.Sqrt(Enumerable.Range(0, 3).Sum(i => (x[i] - y[i]) * (x[i] - y[i])));
        }

        /// <summary>Worse of protanopia and deuteranopia.</summary>
        public static double CvdDeltaE(Color a, Color b) =>
            Math.Min(DeltaE(a, b, Vision.Protan), DeltaE(a, b, Vision.Deutan));
    }
}
