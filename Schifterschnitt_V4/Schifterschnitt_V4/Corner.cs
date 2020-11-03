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
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Schifterschnitt
{
    /// <summary>
    /// A corner with a compound miter cut.
    /// </summary>
    class Corner : CompoundMiterObject
    {
        #region Methods

        /// <summary>
        /// Creates a 3D model of the corner.
        /// </summary>
        /// <param name="model">The model in which the corner will be build.</param>
        public override void CreateModel(ModelVisual3D model)
        {
            // We need a reference length to make all models equal size.
            // This makes sure a model of any size fits in the view area.
            double reference = WidthFirstBoard < WidthSecondBoard ? WidthSecondBoard : WidthFirstBoard;

            double movx = Calc.Tan(AngleAlphaSecondBoard) * (Height / reference);
            double movy = Calc.Cos(AngleBeta - 90) * Calc.Tan(AngleAlphaFirstBoard) * (Height / reference) -
                (Calc.Tan(AngleBeta - 90) * (Calc.Tan(AngleAlphaSecondBoard) * (Height / reference) -
                Calc.Sin(AngleBeta - 90) * Calc.Tan(AngleAlphaFirstBoard) * (Height / reference)));
            double mohxAdd = (ThicknessSecondBoard / reference) / Calc.Cos(AngleAlphaSecondBoard);

            // Berechnung des Y-Versatzes von Mitte-Oben-Hinten zu Mitte-Oben-Vorne.
            double schraegeEins = (ThicknessFirstBoard / reference) / Calc.Cos(AngleAlphaFirstBoard);
            double schraegeZwei = (ThicknessSecondBoard / reference) / Calc.Cos(AngleAlphaSecondBoard);

            double linie = Math.Sqrt(Math.Pow(schraegeEins, 2) + Math.Pow(schraegeZwei, 2) - 2 * schraegeEins * schraegeZwei * Calc.Cos(360 - AngleBeta - 180));

            double winkelgruenEins = Calc.Acos((Math.Pow(linie, 2) + Math.Pow(schraegeEins, 2) - Math.Pow(schraegeZwei, 2)) / (2 * linie * schraegeEins));
            double winkelgruenZwei = Calc.Acos((Math.Pow(linie, 2) + Math.Pow(schraegeZwei, 2) - Math.Pow(schraegeEins, 2)) / (2 * linie * schraegeZwei));

            double winkelgelbEins = 0;
            double winkelgelbZwei = 0;

            if (winkelgruenZwei > 90)
                winkelgelbEins = 90 + winkelgruenEins;
            else
                winkelgelbEins = 90 - winkelgruenEins;

            if (winkelgruenEins > 90)
                winkelgelbZwei = 90 + winkelgruenZwei;
            else
                winkelgelbZwei = 90 - winkelgruenZwei;

            winkelgelbEins = Math.Abs(winkelgelbEins);
            winkelgelbZwei = Math.Abs(winkelgelbZwei);

            double yVersatz = linie / Calc.Sin(180 - winkelgelbEins - winkelgelbZwei) * Calc.Sin(winkelgelbEins);

            if (winkelgruenEins > 90)
                yVersatz *= -1;

            double laenge = 2.5;
            double zusatzEins = Calc.Tan(AngleCrossCutFirstBoard) * WidthFirstBoard;
            double zusatzZwei = Calc.Tan(AngleCrossCutSecondBoard) * WidthSecondBoard;

            if (2.5 * reference < zusatzZwei)
                laenge = zusatzZwei / reference;
            if (2.5 * reference < zusatzEins)
                laenge = zusatzEins / reference;

            double mohyAdd = yVersatz;

            if (System.Double.IsNaN(mohyAdd))
                mohyAdd = 0;

            double luvx = -1 * Calc.Cos(Math.Abs(AngleBeta - 90)) * laenge;
            double luvy = Calc.Sin(AngleBeta - 90) * laenge;
            double lovx = luvx + Calc.Sin(AngleBeta - 90) * Calc.Tan(AngleAlphaFirstBoard) * (Height / reference);
            double lovy = luvy + Calc.Cos(Math.Abs(AngleBeta - 90)) * Calc.Tan(AngleAlphaFirstBoard) * (Height / reference);

            double halbeHöheModell = Height / reference / 2;

            Point3D ruv = new Point3D(0, -laenge, halbeHöheModell * -1);
            Point3D ruh = new Point3D(schraegeZwei, -laenge, halbeHöheModell * -1);
            Point3D rov = new Point3D(Calc.Tan(AngleAlphaSecondBoard) * (Height / reference), -laenge, halbeHöheModell);
            Point3D roh = new Point3D(Calc.Tan(AngleAlphaSecondBoard) * (Height / reference) + schraegeZwei, -laenge, halbeHöheModell);
            Point3D muv = new Point3D(0, 0, halbeHöheModell * -1);
            Point3D muh = new Point3D(mohxAdd, mohyAdd, halbeHöheModell * -1);
            Point3D mov = new Point3D(movx, movy, halbeHöheModell);
            Point3D moh = new Point3D(movx + mohxAdd, movy + mohyAdd, halbeHöheModell);
            Point3D luv = new Point3D(luvx, luvy, halbeHöheModell * -1);
            Point3D luh = new Point3D(luvx + Calc.Sin(AngleBeta - 90) * schraegeEins, luvy + Calc.Cos(Math.Abs(AngleBeta - 90)) * schraegeEins,
                halbeHöheModell * -1);
            Point3D lov = new Point3D(lovx, lovy, halbeHöheModell);
            Point3D loh = new Point3D(lovx + Calc.Sin(AngleBeta - 90) * schraegeEins, lovy + Calc.Cos(Math.Abs(AngleBeta - 90)) * schraegeEins,
                halbeHöheModell);

            var group = new Model3DGroup();

            group.Children.Add(Square(ruv, ruh, roh, rov));
            group.Children.Add(Square(muv, ruv, rov, mov));
            group.Children.Add(Square(mov, rov, roh, moh));
            group.Children.Add(Square(lov, mov, moh, loh));
            group.Children.Add(Square(luv, muv, mov, lov));
            group.Children.Add(Square(luh, luv, lov, loh));
            group.Children.Add(Square(muh, luh, loh, moh));
            group.Children.Add(Square(ruh, muh, moh, roh));
            group.Children.Add(Square(ruv, muv, muh, ruh));
            group.Children.Add(Square(muv, luv, luh, muh));

            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(2, 1, -1)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-2, -1, -1)));

            model.Content = group;
        }

        #endregion
    }
}
