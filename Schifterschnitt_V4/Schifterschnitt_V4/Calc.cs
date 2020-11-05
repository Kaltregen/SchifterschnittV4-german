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
    /// Helps calculating repetitive tasks.
    /// </summary>
    public static class Calc
    {
        #region Methods

        /// <summary>
        /// Converts an angle from degree to radian.
        /// </summary>
        /// <param name="angle">The angle to be converted.</param>
        /// <returns>The converted angle.</returns>
        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary>
        /// Converts an angle from radian to degree.
        /// </summary>
        /// <param name="angle">The angle to be converted.</param>
        /// <returns>The converted angle.</returns>
        public static double RadianToDegree(double angle)
        {
            return angle * 180.0 / Math.PI;
        }

        /// <summary>
        /// Calculates the tangens of an angle.
        /// </summary>
        /// <param name="angle">The angle in degree.</param>
        /// <returns>The tangens of the angle.</returns>
        public static double Tan(double angle)
        {
            return Math.Tan(DegreeToRadian(angle));
        }

        /// <summary>
        /// Calculates the sinus of an angle.
        /// </summary>
        /// <param name="angle">The angle in degree.</param>
        /// <returns>The sinus of the angle.</returns>
        public static double Sin(double angle)
        {
            return Math.Sin(DegreeToRadian(angle));
        }

        /// <summary>
        /// Calculates the cosinus of an angle.
        /// </summary>
        /// <param name="angle">The angle in degree.</param>
        /// <returns>The cosinus of the angle.</returns>
        public static double Cos(double angle)
        {
            return Math.Cos(DegreeToRadian(angle));
        }

        /// <summary>
        /// Calculates the arc-tangens of a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The angle in degree.</returns>
        public static double Atan(double value)
        {
            return RadianToDegree(Math.Atan(value));
        }

        /// <summary>
        /// Calculates the arc-sinus of a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The angle in degree.</returns>
        public static double Asin(double value)
        {
            return RadianToDegree(Math.Asin(value));
        }

        /// <summary>
        /// Calculates the arc-cosinus of a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The angle in degree.</returns>
        public static double Acos(double value)
        {
            return RadianToDegree(Math.Acos(value));
        }

        /// <summary>
        /// Calculates the radius of the circumscribed circle of a regular polygon.
        /// </summary>
        /// <param name="lengthOfSide">The length of one side of the regular polygon.</param>
        /// <param name="numberOfSides">The number of sides of the regular polygon.</param>
        /// <returns>The radius of the circumscribed circle of the regular polygon.</returns>
        public static double CircumscribedCircleRadius(double lengthOfSide, short numberOfSides)
        {
            return lengthOfSide / (2 * Sin(180.0 / numberOfSides));
        }

        /// <summary>
        /// Calculates the radius of the inscribed circle of a regular polygon.
        /// </summary>
        /// <param name="lengthOfSide">The length of one side of the regular polygon.</param>
        /// <param name="numberOfSides">The number of sides of the regular polygon.</param>
        /// <returns>The radius of the inscribed circle of the regular polygon.</returns>
        public static double InscribedCircleRadius(double lengthOfSide, short numberOfSides)
        {
            return lengthOfSide / (2 * Tan(180.0 / numberOfSides));
        }

        #endregion
    }
}
