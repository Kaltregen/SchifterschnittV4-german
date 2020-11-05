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
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Schifterschnitt
{
    /// <summary>
    /// A pyramid with a compound miter cut.
    /// </summary>
    class Pyramid : CompoundMiterObject
    {
        #region Properties

        /// <summary>
        /// The bottom side length of the pyramid.
        /// </summary>
        public double BottomSideLength { get; set; }

        /// <summary>
        /// The top side length of the pyramid.
        /// </summary>
        public double TopSideLength { get; set; }

        /// <summary>
        /// The number of sides of the pyramid.
        /// </summary>
        public short NumberOfSides { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a 3D model of a pyramid.
        /// </summary>
        /// <param name="model">The model in which the pyramid will be build.</param>
        public override void CreateModel(ModelVisual3D model)
        {
            double slantS = ThicknessFirstBoard / Calc.Cos(AngleAlphaFirstBoard);

            double radiusBottomOuter = Calc.CircumscribedCircleRadius(BottomSideLength, NumberOfSides);
            double radiusTopOuter = Calc.CircumscribedCircleRadius(TopSideLength, NumberOfSides);
            double radiusBottomInner = Calc.CircumscribedCircleRadius(BottomSideLength, NumberOfSides) - (slantS / Calc.Sin((NumberOfSides - 2.0) / NumberOfSides * 180.0 / 2));
            double radiusTopInner = Calc.CircumscribedCircleRadius(TopSideLength, NumberOfSides) - (slantS / Calc.Sin((NumberOfSides - 2.0) / NumberOfSides * 180.0 / 2));
            double[] umkreise = { radiusBottomOuter, radiusTopOuter, radiusBottomInner, radiusTopInner };

            // We need a reference length to make all pyramids the same size.
            // This makes sure they will be the same size in the view area.
            double reference;

            if (Height > radiusBottomOuter && Height > radiusTopOuter)
                reference = Height;
            else if (radiusBottomOuter > Height && radiusBottomOuter >= radiusTopOuter)
                reference = radiusBottomOuter;
            else if (radiusTopOuter > Height && radiusTopOuter > radiusBottomOuter)
                reference = radiusTopOuter;
            else
                reference = Height;

            reference /= 2;

            var pointsBottomOuter = new List<Point3D>();
            var pointsTopOuter = new List<Point3D>();
            var pointsBottomInner = new List<Point3D>();
            var pointsTopInner = new List<Point3D>();
            List<Point3D>[] pointLists = { pointsBottomOuter, pointsTopOuter, pointsBottomInner, pointsTopInner };

            double halfmodelHeight = Height / reference / 2;

            double[] loopZValues = { halfmodelHeight * -1, halfmodelHeight, halfmodelHeight * -1, halfmodelHeight };

            double pointAngle = 360.0 / NumberOfSides / 2.0;
            double x;
            double y;

            for (int i = 0; i < 4; i++)
            {
                double radius = umkreise[i];

                for (int count = 0; count < NumberOfSides; count++)
                {
                    if (pointAngle < 90)
                    {
                        x = -1 * Calc.Sin(pointAngle) * (radius / reference);
                        y = -1 * Calc.Cos(pointAngle) * (radius / reference);
                    }
                    else if (pointAngle >= 90 && pointAngle < 180)
                    {
                        x = -1 * Calc.Cos(pointAngle - 90) * (radius / reference);
                        y = Calc.Sin(pointAngle - 90) * (radius / reference);
                    }
                    else if (pointAngle >= 180 && pointAngle < 270)
                    {
                        x = Calc.Sin(pointAngle - 180) * (radius / reference);
                        y = Calc.Cos(pointAngle - 180) * (radius / reference);
                    }
                    else
                    {
                        x = Calc.Cos(pointAngle - 270) * (radius / reference);
                        y = -1 * Calc.Sin(pointAngle - 270) * (radius / reference);
                    }

                    pointLists[i].Add(new Point3D(x, y, loopZValues[i]));

                    pointAngle += (360.0 / NumberOfSides);
                }
                pointAngle = 360.0 / NumberOfSides / 2.0;
            }

            // When the top or bottem side lengths are to small we need a point for the inner areas.
            double distanceBottomCenter = Calc.Tan(90 - Math.Abs(AngleAlphaFirstBoard)) * Math.Abs(radiusBottomInner) / reference;
            double distanceTopCenter = Calc.Tan(90 - Math.Abs(AngleAlphaFirstBoard)) * Math.Abs(radiusTopInner) / reference;

            Point3D innerCenterBottom = new Point3D(0, 0, halfmodelHeight * -1 + distanceBottomCenter);
            Point3D innerCenterTop = new Point3D(0, 0, halfmodelHeight - distanceTopCenter);

            var group = new Model3DGroup();

            group.Children.Add(Square(pointsBottomOuter[0], pointsBottomOuter[NumberOfSides - 1], pointsTopOuter[NumberOfSides - 1], pointsTopOuter[0]));

            for (int i = 0; i < NumberOfSides - 1; i++)
                group.Children.Add(Square(pointsBottomOuter[i + 1], pointsBottomOuter[i], pointsTopOuter[i], pointsTopOuter[i + 1]));

            // If there is a hole at the top and bottom we can fill the inner surfaces with squares.
            if (radiusTopInner > 0 && radiusBottomInner > 0)
            {
                group.Children.Add(Square(pointsBottomInner[NumberOfSides - 1], pointsBottomInner[0], pointsTopInner[0], pointsTopInner[NumberOfSides - 1]));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Square(pointsBottomInner[i], pointsBottomInner[i + 1], pointsTopInner[i + 1], pointsTopInner[i]));
            }

            // Otherwise we use triangles and the centerpoint.
            if (radiusTopInner > 0 && radiusBottomInner <= 0)
            {
                group.Children.Add(Triangle(pointsTopInner[0], pointsTopInner[NumberOfSides - 1], innerCenterBottom));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Triangle(pointsTopInner[i + 1], pointsTopInner[i], innerCenterBottom));
            }

            if (radiusBottomInner > 0 && radiusTopInner <= 0)
            {
                group.Children.Add(Triangle(pointsBottomInner[NumberOfSides - 1], pointsBottomInner[0], innerCenterTop));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Triangle(pointsBottomInner[i], pointsBottomInner[i + 1], innerCenterTop));
            }

            // If there is no hole at the top or bottom we need triangles to fill the top and bottom surfaces solid.
            if (radiusTopInner <= 0)
            {
                Point3D middle = new Point3D(0, 0, halfmodelHeight);

                group.Children.Add(Triangle(pointsTopOuter[0], pointsTopOuter[NumberOfSides - 1], middle));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Triangle(pointsTopOuter[i + 1], pointsTopOuter[i], middle));
            }
            else
            {
                group.Children.Add(Square(pointsTopOuter[0], pointsTopOuter[NumberOfSides - 1], pointsTopInner[NumberOfSides - 1], pointsTopInner[0]));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Square(pointsTopOuter[i + 1], pointsTopOuter[i], pointsTopInner[i], pointsTopInner[i + 1]));
            }

            if (radiusBottomInner <= 0)
            {
                Point3D middle = new Point3D(0, 0, halfmodelHeight * -1);

                group.Children.Add(Triangle(pointsBottomOuter[NumberOfSides - 1], pointsBottomOuter[0], middle));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Triangle(pointsBottomOuter[i], pointsBottomOuter[i + 1], middle));
            }
            else
            {
                group.Children.Add(Square(pointsBottomOuter[NumberOfSides - 1], pointsBottomOuter[0], pointsBottomInner[0], pointsBottomInner[NumberOfSides - 1]));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Square(pointsBottomOuter[i], pointsBottomOuter[i + 1], pointsBottomInner[i + 1], pointsBottomInner[i]));
            }

            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(2, 1, -1)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-2, 1, -1)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(0, -2, -1)));

            model.Content = group;
        }

        #endregion
    }
}
