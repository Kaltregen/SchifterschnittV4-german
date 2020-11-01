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
            double schrägeS = ThicknessFirstBoard / Calc.Cos(AngleAlphaFirstBoard);

            // Berechnet die Radien der Umkreise und fügt sie einem Array hinzu.
            double umkreisradiusUnten = Calc.CircumscribedCircleRadius(BottomSideLength, NumberOfSides);
            double umkreisradiusOben = Calc.CircumscribedCircleRadius(TopSideLength, NumberOfSides);
            double umkreisradiusInnenUnten = Calc.CircumscribedCircleRadius(BottomSideLength, NumberOfSides) - (schrägeS / Calc.Sin((NumberOfSides - 2.0) / NumberOfSides * 180.0 / 2));
            double umkreisradiusInnenOben = Calc.CircumscribedCircleRadius(TopSideLength, NumberOfSides) - (schrägeS / Calc.Sin((NumberOfSides - 2.0) / NumberOfSides * 180.0 / 2));
            double[] umkreise = { umkreisradiusUnten, umkreisradiusOben, umkreisradiusInnenUnten, umkreisradiusInnenOben };

            // We need a reference length to make all pyramids the same size.
            // This makes sure they will be the same size in the view area.
            double referenz;

            if (Height > umkreisradiusUnten && Height > umkreisradiusOben)
                referenz = Height;
            else if (umkreisradiusUnten > Height && umkreisradiusUnten > umkreisradiusOben)
                referenz = umkreisradiusUnten;
            else if (umkreisradiusOben > Height && umkreisradiusOben > umkreisradiusUnten)
                referenz = umkreisradiusOben;
            else
                referenz = Height;

            var punkteUnten = new List<Point3D>();
            var punkteOben = new List<Point3D>();
            var punkteInnenUnten = new List<Point3D>();
            var punkteInnenOben = new List<Point3D>();
            List<Point3D>[] punkteListen = { punkteUnten, punkteOben, punkteInnenUnten, punkteInnenOben };

            double[] schleifenWerte = { Height / referenz * -1, Height / referenz, Height / referenz * -1, Height / referenz };

            double w = 360.0 / NumberOfSides / 2.0;
            double x;
            double y;

            for (int i = 0; i < 4; i++)
            {
                double item = umkreise[i];
                for (int j = 0; j < NumberOfSides; j++)
                {
                    if (w < 90)
                    {
                        x = -1 * Calc.Sin(w) * (item / referenz) * 2;
                        y = -1 * Calc.Cos(w) * (item / referenz) * 2;
                    }
                    else if (w >= 90 && w < 180)
                    {
                        x = -1 * Calc.Cos(w - 90) * (item / referenz) * 2;
                        y = Calc.Sin(w - 90) * (item / referenz) * 2;
                    }
                    else if (w >= 180 && w < 270)
                    {
                        x = Calc.Sin(w - 180) * (item / referenz) * 2;
                        y = Calc.Cos(w - 180) * (item / referenz) * 2;
                    }
                    else
                    {
                        x = Calc.Cos(w - 270) * (item / referenz) * 2;
                        y = -1 * Calc.Sin(w - 270) * (item / referenz) * 2;
                    }

                    punkteListen[i].Add(new Point3D(x, y, schleifenWerte[i]));

                    w += (360.0 / NumberOfSides);
                }
                w = 360.0 / NumberOfSides / 2.0;
            }

            // When the top or bottem side lengths are to small we need a point for the inner planes.
            Point3D innenMitteUnten = new Point3D(0, 0, Height / referenz * -1 + (Calc.Tan(90 - Math.Abs(AngleAlphaFirstBoard)) * Math.Abs(umkreisradiusInnenUnten) / referenz * 2));
            Point3D innenMitteOben = new Point3D(0, 0, Height / referenz - (Calc.Tan(90 - Math.Abs(AngleAlphaFirstBoard)) * Math.Abs(umkreisradiusInnenOben) / referenz * 2));

            var group = new Model3DGroup();

            // Verbindet die Punkte und füllt die Flächen.

            // Füllt die äußeren Flächen.
            group.Children.Add(Square(punkteUnten[0], punkteUnten[NumberOfSides - 1], punkteOben[NumberOfSides - 1], punkteOben[0]));

            for (int i = 0; i < NumberOfSides - 1; i++)
                group.Children.Add(Square(punkteUnten[i + 1], punkteUnten[i], punkteOben[i], punkteOben[i + 1]));

            // Füllt die inneren Flächen wenn oben und unten ein Loch entsteht.
            if (umkreisradiusInnenOben > 0 && umkreisradiusInnenUnten > 0)
            {
                group.Children.Add(Square(punkteInnenUnten[NumberOfSides - 1], punkteInnenUnten[0], punkteInnenOben[0], punkteInnenOben[NumberOfSides - 1]));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Square(punkteInnenUnten[i], punkteInnenUnten[i + 1], punkteInnenOben[i + 1], punkteInnenOben[i]));
            }

            // Füllt die inneren Flächen wenn nur oben ein Loch entsteht.
            if (umkreisradiusInnenOben > 0 && umkreisradiusInnenUnten <= 0)
            {
                group.Children.Add(Dreieck(punkteInnenOben[0], punkteInnenOben[NumberOfSides - 1], innenMitteUnten));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Dreieck(punkteInnenOben[i + 1], punkteInnenOben[i], innenMitteUnten));
            }

            // Füllt die inneren Flächen wenn nur unten ein Loch entsteht.
            if (umkreisradiusInnenUnten > 0 && umkreisradiusInnenOben <= 0)
            {
                group.Children.Add(Dreieck(punkteInnenUnten[NumberOfSides - 1], punkteInnenUnten[0], innenMitteOben));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Dreieck(punkteInnenUnten[i], punkteInnenUnten[i + 1], innenMitteOben));
            }

            // Wenn oben kein Loch entsteht.
            if (umkreisradiusInnenOben <= 0)
            {
                // Füllt die obere Fläche mit Dreiecken.
                Point3D middle = new Point3D(0, 0, Height / referenz);

                group.Children.Add(Dreieck(punkteOben[0], punkteOben[NumberOfSides - 1], middle));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Dreieck(punkteOben[i + 1], punkteOben[i], middle));
            }
            else
            {
                // Füllt die oberen Flächen.
                group.Children.Add(Square(punkteOben[0], punkteOben[NumberOfSides - 1], punkteInnenOben[NumberOfSides - 1], punkteInnenOben[0]));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Square(punkteOben[i + 1], punkteOben[i], punkteInnenOben[i], punkteInnenOben[i + 1]));
            }

            // Wenn unten kein Loch entsteht.
            if (umkreisradiusInnenUnten <= 0)
            {
                // Füllt die untere Fläche mit Dreiecken.
                Point3D middle = new Point3D(0, 0, Height / referenz * -1);

                group.Children.Add(Dreieck(punkteUnten[NumberOfSides - 1], punkteUnten[0], middle));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Dreieck(punkteUnten[i], punkteUnten[i + 1], middle));
            }
            else
            {
                // Füllt die unteren Flächen.
                group.Children.Add(Square(punkteUnten[NumberOfSides - 1], punkteUnten[0], punkteInnenUnten[0], punkteInnenUnten[NumberOfSides - 1]));

                for (int i = 0; i < NumberOfSides - 1; i++)
                    group.Children.Add(Square(punkteUnten[i], punkteUnten[i + 1], punkteInnenUnten[i + 1], punkteInnenUnten[i]));
            }

            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(2, 1, -1)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-2, 1, -1)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(0, -2, -1)));

            model.Content = group;
        }

        #endregion
    }
}
