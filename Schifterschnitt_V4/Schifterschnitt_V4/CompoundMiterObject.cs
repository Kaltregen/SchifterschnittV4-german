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
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Schifterschnitt
{
    /// <summary>
    /// An object with a compound miter cut.
    /// </summary>
    public abstract class CompoundMiterObject
    {
        #region Properties

        /// <summary>
        /// The height of the object.
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// The thickness of the first board.
        /// </summary>
        public double ThicknessFirstBoard { get; set; }

        /// <summary>
        /// The thickness of the second board.
        /// </summary>
        public double ThicknessSecondBoard { get; set; }

        /// <summary>
        /// The tilt angle of the first board.
        /// </summary>
        public double AngleAlphaFirstBoard { get; set; }

        /// <summary>
        /// The tilt angle of the second board.
        /// </summary>
        public double AngleAlphaSecondBoard { get; set; }

        /// <summary>
        /// The angle between the boards.
        /// </summary>
        public double AngleBeta { get; set; }
        
        /// <summary>
        /// Sets if there is a miter joint.
        /// </summary>
        public bool MiterJoint { get; set; }
        
        /// <summary>
        /// The cross cut angle for the first board.
        /// </summary>
        public double AngleCrossCutFirstBoard { get; set; }

        /// <summary>
        /// The cross cut angle for the second board.
        /// </summary>
        public double AngleCrossCutSecondBoard { get; set; }

        /// <summary>
        /// The tilt angle of the saw blade for the first board.
        /// </summary>
        public double AngleSawBladeTiltFirstBoard { get; set; }

        /// <summary>
        /// The tilt angle of the saw blade for the second board.
        /// </summary>
        public double AngleSawBladeTiltSecondBoard { get; set; }

        /// <summary>
        /// The board width without the slant area for the first board.
        /// </summary>
        public double WidthFirstBoard { get; set; }

        /// <summary>
        /// The board width without the slant area for the second board.
        /// </summary>
        public double WidthSecondBoard { get; set; }

        /// <summary>
        /// The total board width with the slant area for the first board.
        /// </summary>
        public double WidthWithSlantFirstBoard { get; set; }

        /// <summary>
        /// The total board width with the slant area for the second board.
        /// </summary>
        public double WidhtWithSlantSecondBoard { get; set; }

        /// <summary>
        /// The dihedral angle of the boards.
        /// </summary>
        public double AngleDihedral { get; set; }

        #endregion

        #region Variables

        /// <summary>
        /// The material shown in the graphic.
        /// </summary>
        DiffuseMaterial woodMaterial;

        /// <summary>
        /// The imagebrush for the material shown in the graphic.
        /// </summary>
        ImageBrush wood = new ImageBrush();

        #endregion

        #region ctor

        /// <summary>
        /// Create a new compound miter object.
        /// </summary>
        public CompoundMiterObject()
        {
            wood.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/Holz.bmp"));
            woodMaterial = new DiffuseMaterial(wood);
        }

        #endregion

        #region Methoden

        /// <summary>
        /// Calculates the compound miter cut.
        /// </summary>
        public void Calculation()
        {
            double winkelAlphaEinsRadian = Calculate.DegreeToRadian(AngleAlphaFirstBoard);
            double winkelAlphaZweiRadian = Calculate.DegreeToRadian(AngleAlphaSecondBoard);
            double winkelBetaRadian = Calculate.DegreeToRadian(AngleBeta);

            var vectorOne = new Vector3D()
            {
                X = Math.Sin(winkelBetaRadian - Calculate.DegreeToRadian(90)) * Math.Tan(winkelAlphaEinsRadian),
                Y = Math.Cos(winkelBetaRadian - Calculate.DegreeToRadian(90)) * Math.Tan(winkelAlphaEinsRadian)
            };

            var vectorTwo = new Vector3D()
            {
                X = Math.Tan(winkelAlphaZweiRadian)
            };

            var vectorThree = new Vector3D()
            {
                X = Math.Tan(winkelAlphaZweiRadian),
                Y = Math.Cos(winkelBetaRadian - Calculate.DegreeToRadian(90)) * Math.Tan(winkelAlphaEinsRadian) -
                (Math.Tan(winkelBetaRadian - Calculate.DegreeToRadian(90)) * (Math.Tan(winkelAlphaZweiRadian) - Math.Sin(winkelBetaRadian -
                Calculate.DegreeToRadian(90)) * Math.Tan(winkelAlphaEinsRadian)))
            };
            
            // Berechnung der Queranschlagswinkel.
            double winkelQueranschlagEinsRadian = Math.Acos((vectorOne.X * vectorThree.X + vectorOne.Y * vectorThree.Y + 1) /
                (Math.Sqrt(Math.Pow(vectorOne.X, 2) + Math.Pow(vectorOne.Y, 2) + 1) * Math.Sqrt(Math.Pow(vectorThree.X, 2) + Math.Pow(vectorThree.Y, 2) + 1)));
            double winkelQueranschlagZweiRadian = Math.Acos((vectorTwo.X * vectorThree.X + 1) / (Math.Sqrt(Math.Pow(vectorTwo.X, 2) + 1) *
                Math.Sqrt(Math.Pow(vectorThree.X, 2) + Math.Pow(vectorThree.Y, 2) + 1)));
            
            // Setzt den Queranschlagswinkel des Teil zwei wenn nötig ins negative.
            if (vectorThree.Y < 0)
                winkelQueranschlagZweiRadian *= -1;

            // Setzt den Queranschlagswinkel des Teil eins wenn nötig ins negative.
            if (360 - AngleBeta - 180 <= 90)
            {
                // Vektor 3 Oben Links immer negativ
                if (vectorThree.X < 0 && vectorThree.Y >= 0)
                    winkelQueranschlagEinsRadian *= -1;

                // Vektor 3 Oben Rechts
                if (vectorThree.X >= 0 && vectorThree.Y >= 0 && (vectorThree.Y / vectorThree.X) > (vectorOne.Y / vectorOne.X))
                    winkelQueranschlagEinsRadian *= -1;

                // Vektor 3 Unten Rechts immer positiv

                // Vektor 3 Unten Links
                if (vectorThree.X < 0 && vectorThree.Y < 0 && (Math.Abs(vectorThree.Y) / Math.Abs(vectorThree.X)) < (vectorOne.Y / vectorOne.X))
                    winkelQueranschlagEinsRadian *= -1;
            }
            else
            {
                // Vektor 3 Oben Links
                if (vectorThree.X < 0 && vectorThree.Y >= 0 && (Math.Abs(vectorThree.Y) / Math.Abs(vectorThree.X)) < (Math.Abs(vectorOne.Y) / Math.Abs(vectorOne.X)))
                    winkelQueranschlagEinsRadian *= -1;

                // Vektor 3 Oben Rechts immer positiv

                // Vektor 3 Unten Rechts
                if (vectorThree.X >= 0 && vectorThree.Y < 0 && (Math.Abs(vectorThree.Y) / Math.Abs(vectorThree.X)) > (Math.Abs(vectorOne.Y) / Math.Abs(vectorOne.X)))
                    winkelQueranschlagEinsRadian *= -1;

                // Vektor 3 Unten Links immer negativ
                if (vectorThree.X < 0 && vectorThree.Y < 0)
                    winkelQueranschlagEinsRadian *= -1;
            }

            // Setzt die Queranschlagswinkel auf Null wenn sie keine Zahl sind.
            if (double.IsNaN(AngleCrossCutFirstBoard))
                winkelQueranschlagEinsRadian = 0;

            if (double.IsNaN(AngleCrossCutSecondBoard))
                winkelQueranschlagZweiRadian = 0;

            // Zuweisung der Queranschlagswinkel zu den Eigenschaften.
            AngleCrossCutFirstBoard = Calculate.RadianToDegree(winkelQueranschlagEinsRadian);
            AngleCrossCutSecondBoard = Calculate.RadianToDegree(winkelQueranschlagZweiRadian);

            // Berechnung der Werte der Vektoren für den Flächenwinkel.
            Vector3D vektorVier = new Vector3D();
            Vector3D vektorFuenf = new Vector3D();

            vektorVier.Z = Math.Cos(winkelAlphaEinsRadian) * Math.Sin(winkelQueranschlagEinsRadian);

            double winkelBodenlinieRadian = Math.Atan(Math.Sin(winkelAlphaEinsRadian) * Math.Sin(winkelQueranschlagEinsRadian) / Math.Cos(winkelQueranschlagEinsRadian));
            double bodenlinie = Math.Cos(winkelQueranschlagEinsRadian) / Math.Cos(winkelBodenlinieRadian);

            vektorVier.X = -1 * Math.Cos(Calculate.DegreeToRadian(AngleBeta - 90 + Calculate.RadianToDegree(winkelBodenlinieRadian))) * bodenlinie;
            vektorVier.Y = Math.Sin(Calculate.DegreeToRadian(AngleBeta - 90 + Calculate.RadianToDegree(winkelBodenlinieRadian))) * bodenlinie;

            vektorFuenf.Z = Math.Cos(winkelAlphaZweiRadian) * Math.Sin(winkelQueranschlagZweiRadian);
            vektorFuenf.X = Math.Sin(winkelAlphaZweiRadian) * Math.Sin(winkelQueranschlagZweiRadian);
            vektorFuenf.Y = -1 * Math.Cos(winkelQueranschlagZweiRadian);

            // Berechnung des Flächenwinkels.
            AngleDihedral = Vector3D.AngleBetween(vektorVier, vektorFuenf);

            // Setzt den Flächenwinkel auf 90° wenn er keine Zahl ist.
            if (System.Double.IsNaN(AngleDihedral))
                AngleDihedral = 90;

            // Berechnung der Sägeblattwinkel.
            double linie = Math.Sqrt(Math.Pow(ThicknessFirstBoard, 2) + Math.Pow(ThicknessSecondBoard, 2) - 2 * ThicknessFirstBoard * ThicknessSecondBoard *
                Math.Cos(Calculate.DegreeToRadian(360 - AngleDihedral - 180)));
            double winkelgruenEins = Calculate.RadianToDegree(Math.Acos((Math.Pow(linie, 2) + Math.Pow(ThicknessFirstBoard, 2) - Math.Pow(ThicknessSecondBoard, 2)) /
                (2 * linie * ThicknessFirstBoard)));
            double winkelgruenZwei = Calculate.RadianToDegree(Math.Acos((Math.Pow(linie, 2) + Math.Pow(ThicknessSecondBoard, 2) - Math.Pow(ThicknessFirstBoard, 2)) /
                (2 * linie * ThicknessSecondBoard)));
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

            AngleSawBladeTiltFirstBoard = Calculate.RadianToDegree(Math.Atan(linie / Math.Sin(Calculate.DegreeToRadian(180 - winkelgelbEins - winkelgelbZwei)) *
                Math.Sin(Calculate.DegreeToRadian(winkelgelbZwei)) / ThicknessFirstBoard));
            AngleSawBladeTiltSecondBoard = Calculate.RadianToDegree(Math.Atan(linie / Math.Sin(Calculate.DegreeToRadian(180 - winkelgelbEins - winkelgelbZwei)) *
                Math.Sin(Calculate.DegreeToRadian(winkelgelbEins)) / ThicknessSecondBoard));

            // Setzt die Sägeblattwinkel auf Null wenn sie keine Zahl sind.
            if (System.Double.IsNaN(AngleSawBladeTiltFirstBoard))
                AngleSawBladeTiltFirstBoard = 0;

            if (System.Double.IsNaN(AngleSawBladeTiltSecondBoard))
                AngleSawBladeTiltSecondBoard = 0;

            // Setzt die Sägeblattwinkel ins negative wenn nötig.
            if (winkelgruenEins > 90)
                AngleSawBladeTiltSecondBoard *= -1;

            if (winkelgruenZwei > 90)
                AngleSawBladeTiltFirstBoard *= -1;

            // Wenn keine Gehrung geschnitten werden soll.
            if (!MiterJoint)
            {
                // Die Sägeblattwinkel anpassen.
                AngleSawBladeTiltFirstBoard = (90 - (360 - AngleDihedral - 180)) * -1;
                AngleSawBladeTiltSecondBoard = (90 - (360 - AngleDihedral - 180)) * -1;
            }

            // Berechnet die Breiten der Teile und weist sie der Eigenschaft zu.
            WidthFirstBoard = Height / Math.Cos(winkelAlphaEinsRadian);
            WidthSecondBoard = Height / Math.Cos(winkelAlphaZweiRadian);

            // Berechnet die Breiten mit Schräge der Teile und weist sie der Eigenschaft zu.
            WidthWithSlantFirstBoard = WidthFirstBoard + Math.Abs(Math.Tan(winkelAlphaEinsRadian)) * ThicknessFirstBoard;
            WidhtWithSlantSecondBoard = WidthSecondBoard + Math.Abs(Math.Tan(winkelAlphaZweiRadian)) * ThicknessSecondBoard;
        }

        /// <summary>
        /// Erzeugt ein 3D-Modell des Objekts.
        /// </summary>
        public abstract void ModellErzeugen(ModelVisual3D modell);

        /// <summary>
        /// Erstellt ein Viereck und weist ihm ein Material mit der Holztextur zu.
        /// </summary>
        /// <param name="punktEins">Der erste Punkt des Vierecks.</param>
        /// <param name="punktZwei">Der zweite Punkt des Vierecks.</param>
        /// <param name="punktDrei">Der dritte Punkt des Vierecks.</param>
        /// <param name="punktVier">Der vierte Punkt des Vierecks.</param>
        /// <returns>Ein Viereck als Geometrie.</returns>
        public GeometryModel3D Viereck(Point3D punktEins, Point3D punktZwei, Point3D punktDrei, Point3D punktVier)
        {
            MeshGeometry3D m = new MeshGeometry3D();

            m.Positions.Add(punktEins);
            m.Positions.Add(punktZwei);
            m.Positions.Add(punktDrei);
            m.Positions.Add(punktVier);

            m.TextureCoordinates.Add(new Point(0, 0));
            m.TextureCoordinates.Add(new Point(1, 0));
            m.TextureCoordinates.Add(new Point(0, 1));

            m.TriangleIndices.Add(0);
            m.TriangleIndices.Add(1);
            m.TriangleIndices.Add(2);

            m.TextureCoordinates.Add(new Point(1, 1));
            m.TextureCoordinates.Add(new Point(1, 0));
            m.TextureCoordinates.Add(new Point(0, 1));

            m.TriangleIndices.Add(2);
            m.TriangleIndices.Add(3);
            m.TriangleIndices.Add(0);

            return new GeometryModel3D(m, woodMaterial);
        }

        /// <summary>
        /// Erstellt ein Dreieck und weist ihm ein Material mit der Holztextur zu.
        /// </summary>
        /// <param name="punktEins">Der erste Punkt des Dreiecks.</param>
        /// <param name="punktZwei">Der zweite Punkt des Dreiecks.</param>
        /// <param name="punktDrei">Der dritte Punkt des Dreiecks.</param>
        /// <returns>Ein Dreieck als Geometrie.</returns>
        public GeometryModel3D Dreieck(Point3D punktEins, Point3D punktZwei, Point3D punktDrei)
        {
            MeshGeometry3D m = new MeshGeometry3D();

            m.Positions.Add(punktEins);
            m.Positions.Add(punktZwei);
            m.Positions.Add(punktDrei);

            m.TextureCoordinates.Add(new Point(0, 0));
            m.TextureCoordinates.Add(new Point(1, 0));
            m.TextureCoordinates.Add(new Point(0, 1));

            m.TriangleIndices.Add(0);
            m.TriangleIndices.Add(1);
            m.TriangleIndices.Add(2);

            return new GeometryModel3D(m, woodMaterial);
        }

        #endregion
    }
}
