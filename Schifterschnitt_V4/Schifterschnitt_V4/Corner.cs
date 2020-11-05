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

            // The board length needs to be adjusted for really long miter cuts.
            double normalBoardLength = 2.5 * reference;
            double boardLength = 2.5;
            double minBoardLengthFirst = Calc.Tan(AngleCrossCutFirstBoard) * WidthFirstBoard;
            double minBoardLengthSecond = Calc.Tan(AngleCrossCutSecondBoard) * WidthSecondBoard;

            if (normalBoardLength < minBoardLengthSecond)
                boardLength = minBoardLengthSecond / reference;
            if (normalBoardLength < minBoardLengthFirst)
                boardLength = minBoardLengthFirst / reference;

            double modelHeight = Height / reference;
            double halfModelHeight = Height / reference / 2;

            double offsetFirst = Calc.Tan(AngleAlphaFirstBoard) * modelHeight;
            double offsetSecond = Calc.Tan(AngleAlphaSecondBoard) * modelHeight;

            double centerTopFrontX = Calc.Tan(AngleAlphaSecondBoard) * modelHeight;
            double centerTopFrontY = Calc.Cos(AngleBeta - 90) * offsetFirst - (Calc.Tan(AngleBeta - 90) * (centerTopFrontX - Calc.Sin(AngleBeta - 90) * offsetFirst));
            double centerBottomBackX = (ThicknessSecondBoard / reference) / Calc.Cos(AngleAlphaSecondBoard);

            double slantFirst = ThicknessFirstBoard / reference / Calc.Cos(AngleAlphaFirstBoard);
            double slantSecond = ThicknessSecondBoard / reference / Calc.Cos(AngleAlphaSecondBoard);

            double line = Math.Sqrt(Math.Pow(slantFirst, 2) + Math.Pow(slantSecond, 2) - 2 * slantFirst * slantSecond * Calc.Cos(360 - AngleBeta - 180));

            double angleGreenOne = Calc.Acos((Math.Pow(line, 2) + Math.Pow(slantFirst, 2) - Math.Pow(slantSecond, 2)) / (2 * line * slantFirst));
            double angleGreenTwo = Calc.Acos((Math.Pow(line, 2) + Math.Pow(slantSecond, 2) - Math.Pow(slantFirst, 2)) / (2 * line * slantSecond));

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

            double centerBottomBackY = line / Calc.Sin(180 - angleYellowOne - angleYellowTwo) * Calc.Sin(angleYellowOne);

            if (angleGreenOne > 90)
                centerBottomBackY *= -1;

            if (System.Double.IsNaN(centerBottomBackY))
                centerBottomBackY = 0;

            double leftBottomFrontX = -1 * Calc.Cos(Math.Abs(AngleBeta - 90)) * boardLength;
            double leftBottomFrontY = Calc.Sin(AngleBeta - 90) * boardLength;
            double leftTopFrontX = leftBottomFrontX + Calc.Sin(AngleBeta - 90) * offsetFirst;
            double leftTopFrontY = leftBottomFrontY + Calc.Cos(Math.Abs(AngleBeta - 90)) * offsetFirst;

            double leftBackXAddition = Calc.Sin(AngleBeta - 90) * slantFirst;
            double leftBackYAddition = Calc.Cos(Math.Abs(AngleBeta - 90)) * slantFirst;

            Point3D rightBottomFront = new Point3D(0, -boardLength, halfModelHeight * -1);
            Point3D rightBottomBack = new Point3D(slantSecond, -boardLength, halfModelHeight * -1);
            Point3D rightTopFront = new Point3D(offsetSecond, -boardLength, halfModelHeight);
            Point3D rightTopBack = new Point3D(offsetSecond + slantSecond, -boardLength, halfModelHeight);
            Point3D centerBottomFront = new Point3D(0, 0, halfModelHeight * -1);
            Point3D centerBottomBack = new Point3D(centerBottomBackX, centerBottomBackY, halfModelHeight * -1);
            Point3D centerTopFront = new Point3D(centerTopFrontX, centerTopFrontY, halfModelHeight);
            Point3D centerTopBack = new Point3D(centerTopFrontX + centerBottomBackX, centerTopFrontY + centerBottomBackY, halfModelHeight);
            Point3D leftBottomFront = new Point3D(leftBottomFrontX, leftBottomFrontY, halfModelHeight * -1);
            Point3D leftBottomBack = new Point3D(leftBottomFrontX + leftBackXAddition, leftBottomFrontY + leftBackYAddition, halfModelHeight * -1);
            Point3D leftTopFront = new Point3D(leftTopFrontX, leftTopFrontY, halfModelHeight);
            Point3D leftTopBack = new Point3D(leftTopFrontX + leftBackXAddition, leftTopFrontY + leftBackYAddition, halfModelHeight);

            var group = new Model3DGroup();

            group.Children.Add(Square(rightBottomFront, rightBottomBack, rightTopBack, rightTopFront));
            group.Children.Add(Square(centerBottomFront, rightBottomFront, rightTopFront, centerTopFront));
            group.Children.Add(Square(centerTopFront, rightTopFront, rightTopBack, centerTopBack));
            group.Children.Add(Square(leftTopFront, centerTopFront, centerTopBack, leftTopBack));
            group.Children.Add(Square(leftBottomFront, centerBottomFront, centerTopFront, leftTopFront));
            group.Children.Add(Square(leftBottomBack, leftBottomFront, leftTopFront, leftTopBack));
            group.Children.Add(Square(centerBottomBack, leftBottomBack, leftTopBack, centerTopBack));
            group.Children.Add(Square(rightBottomBack, centerBottomBack, centerTopBack, rightTopBack));
            group.Children.Add(Square(rightBottomFront, centerBottomFront, centerBottomBack, rightBottomBack));
            group.Children.Add(Square(centerBottomFront, leftBottomFront, leftBottomBack, centerBottomBack));

            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(2, 1, -1)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-2, -1, -1)));

            model.Content = group;
        }

        #endregion
    }
}
