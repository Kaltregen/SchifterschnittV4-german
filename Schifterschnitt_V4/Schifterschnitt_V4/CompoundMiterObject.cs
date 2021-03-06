﻿/*
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

        #region Fields

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

        #region Methods

        /// <summary>
        /// Calculates the compound miter cut.
        /// </summary>
        public void Calculation()
        {
            double offsetVectorFirst = Calc.Tan(AngleAlphaFirstBoard);
            double offsetVectorSecond = Calc.Tan(AngleAlphaSecondBoard);

            var vectorFirstBoard = new Vector3D()
            {
                X = Calc.Sin(AngleBeta - 90) * offsetVectorFirst,
                Y = Calc.Cos(AngleBeta - 90) * offsetVectorFirst,
                Z = 1.0
            };

            var vectorSecondBoard = new Vector3D()
            {
                X = offsetVectorSecond,
                Y = 0.0,
                Z = 1.0
            };

            var vectorCutLine = new Vector3D()
            {
                X = offsetVectorSecond,
                Y = Calc.Cos(AngleBeta - 90) * offsetVectorFirst
                    - (Calc.Tan(AngleBeta - 90) * (offsetVectorSecond - Calc.Sin(AngleBeta - 90) * offsetVectorFirst)),
                Z = 1.0
            };

            AngleCrossCutFirstBoard = Vector3D.AngleBetween(vectorFirstBoard, vectorCutLine);
            AngleCrossCutSecondBoard = Vector3D.AngleBetween(vectorSecondBoard, vectorCutLine);
            
            // Because there can be different thicknesses and negative tilt angles we need to set the cross cut angles to 
            // negative if neccessary with the vector describing the cut line as a point in a 2D coordinate system.
            if (vectorCutLine.Y < 0)
                AngleCrossCutSecondBoard *= -1;

            double angleTanPointCutLine = Math.Abs(vectorCutLine.Y) / Math.Abs(vectorCutLine.X);
            double angleTanHelpLine = Math.Abs(vectorFirstBoard.Y) / Math.Abs(vectorFirstBoard.X);

            if (360 - AngleBeta - 180 <= 90)
            {
                if (vectorCutLine.X < 0 && vectorCutLine.Y >= 0)
                    AngleCrossCutFirstBoard *= -1;

                if (vectorCutLine.X >= 0 && vectorCutLine.Y >= 0 && angleTanPointCutLine > angleTanHelpLine)
                    AngleCrossCutFirstBoard *= -1;

                if (vectorCutLine.X < 0 && vectorCutLine.Y < 0 && angleTanPointCutLine < angleTanHelpLine)
                    AngleCrossCutFirstBoard *= -1;
            }
            else
            {
                if (vectorCutLine.X < 0 && vectorCutLine.Y >= 0 && angleTanPointCutLine < angleTanHelpLine)
                    AngleCrossCutFirstBoard *= -1;

                if (vectorCutLine.X >= 0 && vectorCutLine.Y < 0 && angleTanPointCutLine > angleTanHelpLine)
                    AngleCrossCutFirstBoard *= -1;

                if (vectorCutLine.X < 0 && vectorCutLine.Y < 0)
                    AngleCrossCutFirstBoard *= -1;
            }

            if (double.IsNaN(AngleCrossCutFirstBoard))
                AngleCrossCutFirstBoard = 0;

            if (double.IsNaN(AngleCrossCutSecondBoard))
                AngleCrossCutSecondBoard = 0;

            double smallLine = Calc.Sin(AngleAlphaFirstBoard) * Calc.Sin(AngleCrossCutFirstBoard);
            double angleGroundLine = Calc.Atan(smallLine / Calc.Cos(AngleCrossCutFirstBoard));
            double groundLine = Calc.Cos(AngleCrossCutFirstBoard) / Calc.Cos(angleGroundLine);

            var vectorFour = new Vector3D()
            {
                X = -1 * Calc.Cos(AngleBeta - 90 + angleGroundLine) * groundLine,
                Y = Calc.Sin(AngleBeta - 90 + angleGroundLine) * groundLine,
                Z = Calc.Cos(AngleAlphaFirstBoard) * Calc.Sin(AngleCrossCutFirstBoard)
            };

            var vectorFive = new Vector3D()
            {
                Z = Calc.Cos(AngleAlphaSecondBoard) * Calc.Sin(AngleCrossCutSecondBoard),
                X = Calc.Sin(AngleAlphaSecondBoard) * Calc.Sin(AngleCrossCutSecondBoard),
                Y = -1 * Calc.Cos(AngleCrossCutSecondBoard)
            };

            AngleDihedral = Vector3D.AngleBetween(vectorFour, vectorFive);

            if (System.Double.IsNaN(AngleDihedral))
                AngleDihedral = 90;

            double line = Math.Sqrt(Math.Pow(ThicknessFirstBoard, 2) + Math.Pow(ThicknessSecondBoard, 2) - 2 * ThicknessFirstBoard * ThicknessSecondBoard *
                Calc.Cos(360 - AngleDihedral - 180));
            double angleGreenOne = Calc.Acos((Math.Pow(line, 2) + Math.Pow(ThicknessFirstBoard, 2) - Math.Pow(ThicknessSecondBoard, 2)) /
                (2 * line * ThicknessFirstBoard));
            double angleGreenTwo = Calc.Acos((Math.Pow(line, 2) + Math.Pow(ThicknessSecondBoard, 2) - Math.Pow(ThicknessFirstBoard, 2)) /
                (2 * line * ThicknessSecondBoard));
            double angleYellowOne;
            double angleYellowTwo;

            if (angleGreenTwo > 90)
                angleYellowOne = 90 + angleGreenOne;
            else
                angleYellowOne = 90 - angleGreenOne;

            if (angleGreenOne > 90)
                angleYellowTwo = 90 + angleGreenTwo;
            else
                angleYellowTwo = 90 - angleGreenTwo;

            angleYellowOne = Math.Abs(angleYellowOne);
            angleYellowTwo = Math.Abs(angleYellowTwo);

            AngleSawBladeTiltFirstBoard = Calc.Atan(line / Calc.Sin(180 - angleYellowOne - angleYellowTwo) * Calc.Sin(angleYellowTwo) / ThicknessFirstBoard);
            AngleSawBladeTiltSecondBoard = Calc.Atan(line / Calc.Sin(180 - angleYellowOne - angleYellowTwo) * Calc.Sin(angleYellowOne) / ThicknessSecondBoard);

            if (System.Double.IsNaN(AngleSawBladeTiltFirstBoard))
                AngleSawBladeTiltFirstBoard = 0;

            if (System.Double.IsNaN(AngleSawBladeTiltSecondBoard))
                AngleSawBladeTiltSecondBoard = 0;

            if (angleGreenOne > 90)
                AngleSawBladeTiltSecondBoard *= -1;

            if (angleGreenTwo > 90)
                AngleSawBladeTiltFirstBoard *= -1;

            if (!MiterJoint)
            {
                AngleSawBladeTiltFirstBoard = (90 - (360 - AngleDihedral - 180)) * -1;
                AngleSawBladeTiltSecondBoard = (90 - (360 - AngleDihedral - 180)) * -1;
            }

            WidthFirstBoard = Height / Calc.Cos(AngleAlphaFirstBoard);
            WidthSecondBoard = Height / Calc.Cos(AngleAlphaSecondBoard);

            WidthWithSlantFirstBoard = WidthFirstBoard + Math.Abs(Calc.Tan(AngleAlphaFirstBoard)) * ThicknessFirstBoard;
            WidhtWithSlantSecondBoard = WidthSecondBoard + Math.Abs(Calc.Tan(AngleAlphaSecondBoard)) * ThicknessSecondBoard;
        }

        /// <summary>
        /// Creates a 3D model of the object.
        /// </summary>
        public abstract void CreateModel(ModelVisual3D model);

        /// <summary>
        /// Creates a square with wood material.
        /// </summary>
        /// <param name="pointOne">The first point of the square.</param>
        /// <param name="pointTwo">The second point of the square.</param>
        /// <param name="pointThree">The third point of the square.</param>
        /// <param name="pointFour">The fourth point of the square.</param>
        /// <returns>A square.</returns>
        public GeometryModel3D Square(Point3D pointOne, Point3D pointTwo, Point3D pointThree, Point3D pointFour)
        {
            var geometry = new MeshGeometry3D();

            geometry.Positions.Add(pointOne);
            geometry.Positions.Add(pointTwo);
            geometry.Positions.Add(pointThree);
            geometry.Positions.Add(pointFour);

            geometry.TextureCoordinates.Add(new Point(0, 0));
            geometry.TextureCoordinates.Add(new Point(1, 0));
            geometry.TextureCoordinates.Add(new Point(0, 1));

            geometry.TriangleIndices.Add(0);
            geometry.TriangleIndices.Add(1);
            geometry.TriangleIndices.Add(2);

            geometry.TextureCoordinates.Add(new Point(1, 1));
            geometry.TextureCoordinates.Add(new Point(1, 0));
            geometry.TextureCoordinates.Add(new Point(0, 1));

            geometry.TriangleIndices.Add(2);
            geometry.TriangleIndices.Add(3);
            geometry.TriangleIndices.Add(0);

            return new GeometryModel3D(geometry, woodMaterial);
        }

        /// <summary>
        /// Creates a triangle with wood material.
        /// </summary>
        /// <param name="pointOne">The first point of the triangle.</param>
        /// <param name="pointTwo">The second point of the triangle.</param>
        /// <param name="pointThree">The third point of the triangle.</param>
        /// <returns>A triangle.</returns>
        public GeometryModel3D Triangle(Point3D pointOne, Point3D pointTwo, Point3D pointThree)
        {
            var geometry = new MeshGeometry3D();

            geometry.Positions.Add(pointOne);
            geometry.Positions.Add(pointTwo);
            geometry.Positions.Add(pointThree);

            geometry.TextureCoordinates.Add(new Point(0, 0));
            geometry.TextureCoordinates.Add(new Point(1, 0));
            geometry.TextureCoordinates.Add(new Point(0, 1));

            geometry.TriangleIndices.Add(0);
            geometry.TriangleIndices.Add(1);
            geometry.TriangleIndices.Add(2);

            return new GeometryModel3D(geometry, woodMaterial);
        }

        #endregion
    }
}
