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
    /// Eine Ecke mit Schifterschnitt.
    /// </summary>
    class Ecke : CompoundMiterObject
    {
        #region Methoden

        /// <summary>
        /// Erzeugt ein 3D-Modell einer Ecke.
        /// </summary>
        /// <param name="modell">Das 3D-Modell in dem die Ecke gebaut werden soll.</param>
        public override void ModellErzeugen(ModelVisual3D modell)
        {
            // Erstellt eine Referenzgröße zur Anpassung an die Viewport3D-Größe.
            double referenz = WidthFirstBoard < WidthSecondBoard ? referenz = WidthSecondBoard : referenz = WidthFirstBoard;

            double movx = Math.Tan(Calculate.DegreeToRadian(AngleAlphaSecondBoard)) * (Height / referenz);
            double movy = Math.Cos(Calculate.DegreeToRadian(AngleBeta - 90)) * Math.Tan(Calculate.DegreeToRadian(AngleAlphaFirstBoard)) * (Height / referenz) -
                (Math.Tan(Calculate.DegreeToRadian(AngleBeta - 90)) * (Math.Tan(Calculate.DegreeToRadian(AngleAlphaSecondBoard)) * (Height / referenz) -
                Math.Sin(Calculate.DegreeToRadian(AngleBeta - 90)) * Math.Tan(Calculate.DegreeToRadian(AngleAlphaFirstBoard)) * (Height / referenz)));
            double mohxAdd = (ThicknessSecondBoard / referenz) / Math.Cos(Calculate.DegreeToRadian(AngleAlphaSecondBoard));

            // Berechnung des Y-Versatzes von Mitte-Oben-Hinten zu Mitte-Oben-Vorne.
            double schraegeEins = (ThicknessFirstBoard / referenz) / Math.Cos(Calculate.DegreeToRadian(AngleAlphaFirstBoard));
            double schraegeZwei = (ThicknessSecondBoard / referenz) / Math.Cos(Calculate.DegreeToRadian(AngleAlphaSecondBoard));

            double linie = Math.Sqrt(Math.Pow(schraegeEins, 2) + Math.Pow(schraegeZwei, 2) - 2 * schraegeEins * schraegeZwei * Math.Cos(Calculate.DegreeToRadian(360 - AngleBeta - 180)));

            double winkelgruenEins = Calculate.RadianToDegree(Math.Acos((Math.Pow(linie, 2) + Math.Pow(schraegeEins, 2) - Math.Pow(schraegeZwei, 2)) / (2 * linie * schraegeEins)));
            double winkelgruenZwei = Calculate.RadianToDegree(Math.Acos((Math.Pow(linie, 2) + Math.Pow(schraegeZwei, 2) - Math.Pow(schraegeEins, 2)) / (2 * linie * schraegeZwei)));

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

            double yVersatz = linie / Math.Sin(Calculate.DegreeToRadian(180 - winkelgelbEins - winkelgelbZwei)) * Math.Sin(Calculate.DegreeToRadian(winkelgelbEins));

            // Setzt den Y-Versatz negativ wenn nötig.
            if (winkelgruenEins > 90)
                yVersatz *= -1;

            // Sorgt für eine Größenanpassung bei sehr stark negativen Queranschlagswinkeln.
            double laenge = 2.5;
            double zusatzEins = Math.Tan(Calculate.DegreeToRadian(AngleCrossCutFirstBoard)) * WidthFirstBoard;
            double zusatzZwei = Math.Tan(Calculate.DegreeToRadian(AngleCrossCutSecondBoard)) * WidthSecondBoard;

            if (2.5 * referenz < zusatzZwei)
                laenge = zusatzZwei / referenz;
            if (2.5 * referenz < zusatzEins)
                laenge = zusatzEins / referenz;

            double mohyAdd = yVersatz;

            // Setzt die Variable auf Null wenn sie keine Zahl ist.
            if (System.Double.IsNaN(mohyAdd))
                mohyAdd = 0;

            // Berechnet und erstellt die Punkte im Raum.
            double luvx = -1 * Math.Cos(Calculate.DegreeToRadian(Math.Abs(AngleBeta - 90))) * laenge;
            double luvy = Math.Sin(Calculate.DegreeToRadian(AngleBeta - 90)) * laenge;
            double lovx = luvx + Math.Sin(Calculate.DegreeToRadian(AngleBeta - 90)) * Math.Tan(Calculate.DegreeToRadian(AngleAlphaFirstBoard)) * (Height / referenz);
            double lovy = luvy + Math.Cos(Calculate.DegreeToRadian(Math.Abs(AngleBeta - 90))) * Math.Tan(Calculate.DegreeToRadian(AngleAlphaFirstBoard)) * (Height / referenz);

            double halbeHöheModell = Height / referenz / 2;

            Point3D ruv = new Point3D(0, -laenge, halbeHöheModell * -1);
            Point3D ruh = new Point3D(schraegeZwei, -laenge, halbeHöheModell * -1);
            Point3D rov = new Point3D(Math.Tan(Calculate.DegreeToRadian(AngleAlphaSecondBoard)) * (Height / referenz), -laenge, halbeHöheModell);
            Point3D roh = new Point3D(Math.Tan(Calculate.DegreeToRadian(AngleAlphaSecondBoard)) * (Height / referenz) + schraegeZwei, -laenge, halbeHöheModell);
            Point3D muv = new Point3D(0, 0, halbeHöheModell * -1);
            Point3D muh = new Point3D(mohxAdd, mohyAdd, halbeHöheModell * -1);
            Point3D mov = new Point3D(movx, movy, halbeHöheModell);
            Point3D moh = new Point3D(movx + mohxAdd, movy + mohyAdd, halbeHöheModell);
            Point3D luv = new Point3D(luvx, luvy, halbeHöheModell * -1);
            Point3D luh = new Point3D(luvx + Math.Sin(Calculate.DegreeToRadian(AngleBeta - 90)) * schraegeEins, luvy + Math.Cos(Calculate.DegreeToRadian(Math.Abs(AngleBeta - 90))) * schraegeEins,
                halbeHöheModell * -1);
            Point3D lov = new Point3D(lovx, lovy, halbeHöheModell);
            Point3D loh = new Point3D(lovx + Math.Sin(Calculate.DegreeToRadian(AngleBeta - 90)) * schraegeEins, lovy + Math.Cos(Calculate.DegreeToRadian(Math.Abs(AngleBeta - 90))) * schraegeEins,
                halbeHöheModell);

            // Erstellt eine Gruppe, erstellt die Vierecke und fügt sie hinzu.
            Model3DGroup group = new Model3DGroup();

            group.Children.Add(Viereck(ruv, ruh, roh, rov));
            group.Children.Add(Viereck(muv, ruv, rov, mov));
            group.Children.Add(Viereck(mov, rov, roh, moh));
            group.Children.Add(Viereck(lov, mov, moh, loh));
            group.Children.Add(Viereck(luv, muv, mov, lov));
            group.Children.Add(Viereck(luh, luv, lov, loh));
            group.Children.Add(Viereck(muh, luh, loh, moh));
            group.Children.Add(Viereck(ruh, muh, moh, roh));
            group.Children.Add(Viereck(ruv, muv, muh, ruh));
            group.Children.Add(Viereck(muv, luv, luh, muh));

            // Lichtquellen hinzufügen.
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(2, 1, -1)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-2, -1, -1)));

            modell.Content = group;
        }

        #endregion
    }
}
