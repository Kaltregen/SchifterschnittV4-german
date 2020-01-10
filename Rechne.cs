using System;

namespace Schifterschnitt
{
    /// <summary>
    /// Führt Rechenoperationen aus.
    /// </summary>
    public static class Rechne
    {
        #region Methoden

        /// <summary>
        /// Konvertiert einen Winkel von Degree nach Radian.
        /// </summary>
        /// <param name="angle">Der zu konvertierende Winkel.</param>
        /// <returns>Den konvertierten Winkel.</returns>
        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary>
        /// Konvertiert einen Winkel von Radian nach Degree.
        /// </summary>
        /// <param name="angle">Der zu konvertierende Winkel.</param>
        /// <returns>Den konvertierten Winkel.</returns>
        public static double RadianToDegree(double angle)
        {
            return angle * (180 / Math.PI);
        }

        /// <summary>
        /// Berechnet den Umkreisradius eines Vielecks.
        /// </summary>
        /// <param name="seitenlänge">Die Seitenlänge des Vielecks.</param>
        /// <param name="anzahlSeiten">Die Anzahl der Seiten des Vielecks.</param>
        /// <returns>Der Umkreisradius des Vielecks.</returns>
        public static double Umkreis(double seitenlänge, short anzahlSeiten)
        {
            return seitenlänge / (2 * Math.Sin(DegreeToRadian(180.0 / anzahlSeiten)));
        }

        /// <summary>
        /// Berechnet den Inkreisradius eines Vielecks.
        /// </summary>
        /// <param name="seitenlänge">Die Seitenlänge des Vielecks.</param>
        /// <param name="anzahlSeiten">Die Anzahl der Seiten des Vielecks.</param>
        /// <returns>Der Inkreisradius des Vielecks.</returns>
        public static double Inkreis(double seitenlänge, short anzahlSeiten)
        {
            return seitenlänge / (2 * Math.Tan(DegreeToRadian(180.0 / anzahlSeiten)));
        }

        #endregion
    }
}
