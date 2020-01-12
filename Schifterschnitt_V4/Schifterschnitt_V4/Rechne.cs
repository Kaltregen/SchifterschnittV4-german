/*
 * Schifterschnitt V4 - A program for joiners to calculate compound miters.
 * Copyright (C) 2020 Michael Pütz
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
