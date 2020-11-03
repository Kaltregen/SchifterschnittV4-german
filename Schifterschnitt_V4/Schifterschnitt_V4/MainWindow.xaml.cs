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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

namespace Schifterschnitt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        Corner corner = new Corner();
        Pyramid pyramidLine = new Pyramid();
        Pyramid pyramidAngle = new Pyramid();

        bool linexyFirstCalculated = false;
        bool linexySecondCalculated = false;

        AxisAngleRotation3D cornerRotation;
        AxisAngleRotation3D pyramidLineRotation;
        AxisAngleRotation3D pyramidAngleRotation;

        Point mousePosition = new Point(0, 0);

        double cornerCameraAngle = new double();
        double pyramidLineCameraAngle = new double();
        double pyramidAngleCameraAngle = new double();

        FeedbackArea cornerFeedback;
        FeedbackArea pyramidLineFeedback;
        FeedbackArea pyramidAngleFeedback;

        GridColumnResize cornerColumnResize;
        GridColumnResize pyramidLineColumnResize;
        GridColumnResize pyramidAngleColumnResize;

        #endregion

        #region ctor

        /// <summary>
        /// Creates the window.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            #region Grid column resizing

            // The default values of the columns need to be saved to restore them in resizing later.
            var cornerColumnWidths = new List<double>();
            var pyramidLineColumnWidths = new List<double>();
            var pyramidAngleColumnWidths = new List<double>();

            foreach (var columnDefinition in gridCorner.ColumnDefinitions)
                cornerColumnWidths.Add(columnDefinition.Width.Value);

            foreach (var columnDefinition in gridPyramidLine.ColumnDefinitions)
                pyramidLineColumnWidths.Add(columnDefinition.Width.Value);

            foreach (var columnDefinition in gridPyramidAngle.ColumnDefinitions)
                pyramidAngleColumnWidths.Add(columnDefinition.Width.Value);

            cornerColumnResize = new GridColumnResize(cornerColumnWidths);
            pyramidLineColumnResize = new GridColumnResize(pyramidLineColumnWidths);
            pyramidAngleColumnResize = new GridColumnResize(pyramidAngleColumnWidths);

            #endregion

            cornerRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 1);
            RotateTransform3D eckeTransformation = new RotateTransform3D(cornerRotation);
            cornerModelVisual3D.Transform = eckeTransformation;

            pyramidLineRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 1);
            RotateTransform3D pyramideLinieTransformation = new RotateTransform3D(pyramidLineRotation);
            pyramidLineModelVisual3D.Transform = pyramideLinieTransformation;

            pyramidAngleRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 1);
            RotateTransform3D pyramideWinkelTransformation = new RotateTransform3D(pyramidAngleRotation);
            pyramidAngleModelVisual3D.Transform = pyramideWinkelTransformation;
            
            var cameraAngle = Calc.Atan(5 / Math.Sqrt(Math.Pow(5, 2) + Math.Pow(5, 2)));

            cornerCameraAngle = cameraAngle;
            pyramidLineCameraAngle = cameraAngle;
            pyramidAngleCameraAngle = cameraAngle;
            
            cornerFeedback = new FeedbackArea(gridEckeFeedback);
            pyramidLineFeedback = new FeedbackArea(gridPyramideLinieFeedback);
            pyramidAngleFeedback = new FeedbackArea(gridPyramideWinkelFeedback);

            cornerFeedback.Activate(cornerFeedback.EnterValues);
            pyramidLineFeedback.Activate(pyramidLineFeedback.EnterValues);
            pyramidAngleFeedback.Activate(pyramidAngleFeedback.EnterValues);
        }

        #endregion

        #region Methods corner
        
        /// <summary>
        /// Controls the zoom by changing the camera position and look direction.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D.</param>
        /// <param name="e">The MouseWheelEventArgs.</param>
        private void CornerGraphic_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Zoom(cornerPerspectiveCamera, cornerCameraAngle, e);
        }

        /// <summary>
        /// Rotates the 3D model on left or right mouse movement when the left button is hold.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D.</param>
        /// <param name="e">The MouseEventArgs.</param>
        private void CornerGraphic_MouseMove(object sender, MouseEventArgs e)
        {
            Rotate3DModel(cornerRotation, ref cornerCameraAngle, cornerPerspectiveCamera, sender, e);
        }

        /// <summary>
        /// Converts the angle if the input is valid and shows the result.
        /// </summary>
        /// <param name="sender">The textbox for angle conversion.</param>
        /// <param name="e">The TextChangedEventArgs.</param>
        private void CornerAngleConversion_TextChanged(object sender, TextChangedEventArgs e)
        {
            AngleConversion(CornerAngleConversion, CornerAngleConversionResult);
        }

        /// <summary>
        /// Resets all textblocks for the results.
        /// </summary>
        private void CornerResultReset()
        {
            var resultTextBlocks = new TextBlock[] {
                CornerAngleCrossCutFirstBoard,
                CornerAngleCrossCutSecondBoard,
                CornerAngleSawBladeTiltFirstBoard,
                CornerAngleSawBladeTiltSecondBoard,
                CornerWidthFirstBoard,
                CornerWidthSecondBoard,
                CornerWidthWithSlantFirstBoard,
                CornerWidthWithSlantSecondBoard,
                CornerAngleDihedral,
                CornerOffsetFirstBoardResult,
                CornerOffsetSecondBoardResult,
                CornerSlantSFirstBoard,
                CornerSlantSSecondBoard,
            };

            foreach (var textblock in resultTextBlocks)
                textblock.Text = "";
            
        }

        /// <summary>
        /// Resets everything.
        /// </summary>
        /// <param name="sender">The button to reset everything.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void CornerReset_Click(object sender, RoutedEventArgs e)
        {
            CornerResultReset();

            CornerCheckBox.IsChecked = false;

            var inputTextBoxes = new TextBox[] {
                CornerHeight,
                CornerThicknessFirst,
                CornerThicknessSecond,
                CornerAngleAlphaFirst,
                CornerAngleAlphaSecond,
                CornerAngleBeta,
                CornerOffsetFirst,
                CornerOffsetSecond,
                CornerNumberOfSides,
                CornerLineYFirst,
                CornerLineYSecond,
                CornerLineXFirst,
                CornerLineXSecond
            };

            foreach (var textbox in inputTextBoxes)
            {
                textbox.Text = "";
                textbox.Background = Brushes.White;
            }

            cornerModelVisual3D.Content = new Model3DGroup();
            
            cornerFeedback.Deactivate(
                cornerFeedback.Calculated, 
                cornerFeedback.InputChanged, 
                cornerFeedback.AlphaCalculated, 
                cornerFeedback.AlphaChanged, 
                cornerFeedback.BetaCalculated, 
                cornerFeedback.BetaChanged, 
                cornerFeedback.InvalidValues, 
                cornerFeedback.LineXYInvalidValues, 
                cornerFeedback.LineXYTooManyValues);

            cornerFeedback.Activate(cornerFeedback.EnterValues);
        }

        /// <summary>
        /// Calculates the compound miter and shows the results.
        /// </summary>
        /// <param name="sender">The button for the calculation.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void CornerCalculation_Click(object sender, RoutedEventArgs e)
        {
            // Make sure that there are no old results of linexy next to a new calculation.
            var linexyTextBoxes = new TextBox[]
            {
                CornerLineYFirst,
                CornerLineXFirst,
                CornerLineYSecond,
                CornerLineXSecond
            };

            foreach (var textBox in linexyTextBoxes)
                textBox.Text = "";

            var inputTextBoxes = new TextBox[]
            {
                CornerHeight,
                CornerThicknessFirst,
                CornerThicknessSecond,
                CornerAngleAlphaFirst,
                CornerAngleAlphaSecond,
                CornerAngleBeta
            };

            cornerFeedback.Deactivate(cornerFeedback.InputChanged);

            double height = 0;
            double thicknessFirst = 0;
            double thicknessSecond = 0;
            double angleAlphaFirst = 0;
            double angleAlphaSecond = 0;
            double angleBeta = 0;

            if (ATextBoxIsEmpty(inputTextBoxes))
            {
                CornerResultReset();
                cornerModelVisual3D.Content = new Model3DGroup();
                cornerFeedback.Deactivate(cornerFeedback.Calculated);
                cornerFeedback.Activate(cornerFeedback.EnterValues);

                return;
            }

            if (!InputValid(CornerHeight, ref height) || height <= 0)
                CornerHeight.Background = Brushes.Red;

            if (!InputValid(CornerThicknessFirst, ref thicknessFirst) || thicknessFirst <= 0)
                CornerThicknessFirst.Background = Brushes.Red;

            if (!InputValid(CornerThicknessSecond, ref thicknessSecond) || thicknessSecond <= 0)
                CornerThicknessSecond.Background = Brushes.Red;

            if (!InputValid(CornerAngleAlphaFirst, ref angleAlphaFirst) || angleAlphaFirst < -90 || angleAlphaFirst > 90)
                CornerAngleAlphaFirst.Background = Brushes.Red;

            if (!InputValid(CornerAngleAlphaSecond, ref angleAlphaSecond) || angleAlphaSecond < -90 || angleAlphaSecond > 90)
                CornerAngleAlphaSecond.Background = Brushes.Red;

            if (!InputValid(CornerAngleBeta, ref angleBeta) || angleBeta <= 0 || angleBeta >= 180)
                CornerAngleBeta.Background = Brushes.Red;
            
            if (ATextBoxIsRed(inputTextBoxes))
            {
                CornerResultReset();
                cornerModelVisual3D.Content = new Model3DGroup();
                cornerFeedback.Deactivate(cornerFeedback.Calculated);
                cornerFeedback.Activate(cornerFeedback.InvalidValues);
                cornerFeedback.Activate(cornerFeedback.EnterValues);

                return;
            }

            corner.Height = height;
            corner.ThicknessFirstBoard = thicknessFirst;
            corner.ThicknessSecondBoard = thicknessSecond;
            corner.AngleAlphaFirstBoard = angleAlphaFirst;
            corner.AngleAlphaSecondBoard = angleAlphaSecond;
            corner.AngleBeta = angleBeta;
            
            corner.MiterJoint = CornerCheckBox.IsChecked.Value;

            corner.Calculation();

            CornerAngleCrossCutFirstBoard.Text = Math.Round(corner.AngleCrossCutFirstBoard, 2) + "°";
            CornerAngleCrossCutSecondBoard.Text = Math.Round(corner.AngleCrossCutSecondBoard, 2) + "°";
            CornerAngleSawBladeTiltFirstBoard.Text = Math.Round(corner.AngleSawBladeTiltFirstBoard, 2) + "°";
            CornerAngleSawBladeTiltSecondBoard.Text = Math.Round(corner.AngleSawBladeTiltSecondBoard, 2) + "°";
            CornerWidthFirstBoard.Text = ErrorIfTooLarge(corner.WidthFirstBoard);
            CornerWidthSecondBoard.Text = ErrorIfTooLarge(corner.WidthSecondBoard);
            CornerWidthWithSlantFirstBoard.Text = ErrorIfTooLarge(corner.WidthWithSlantFirstBoard);
            CornerWidthWithSlantSecondBoard.Text = ErrorIfTooLarge(corner.WidhtWithSlantSecondBoard);

            CornerAngleDihedral.Text = Math.Round(corner.AngleDihedral, 2) + "°";

            double offsetFirst = Calc.Sin(corner.AngleAlphaFirstBoard) * corner.WidthFirstBoard;
            double offsetSecond = Calc.Sin(corner.AngleAlphaSecondBoard) * corner.WidthSecondBoard;
            double slantSFirst = corner.ThicknessFirstBoard / Calc.Cos(corner.AngleAlphaFirstBoard);
            double slantSSecond = corner.ThicknessSecondBoard / Calc.Cos(corner.AngleAlphaSecondBoard);

            CornerOffsetFirstBoardResult.Text = ErrorIfTooLarge(offsetFirst);
            CornerOffsetSecondBoardResult.Text = ErrorIfTooLarge(offsetSecond);
            CornerSlantSFirstBoard.Text = ErrorIfTooLarge(slantSFirst);
            CornerSlantSSecondBoard.Text = ErrorIfTooLarge(slantSSecond);

            corner.CreateModel(cornerModelVisual3D);

            cornerFeedback.Deactivate(cornerFeedback.EnterValues);
            cornerFeedback.Activate(cornerFeedback.Calculated);
        }

        /// <summary>
        /// Calculates the lines x and y.
        /// </summary>
        /// <param name="sender">The button to calculate the lines.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void CornerLineXY_Click(object sender, RoutedEventArgs e)
        {
            double LineYFirst = 0;
            double LineYSecond = 0;
            double LineXFirst = 0;
            double LineXSecond = 0;

            var inputTextBoxes = new TextBox[]
            {
                CornerLineYFirst,
                CornerLineXFirst,
                CornerLineYSecond,
                CornerLineXSecond
            };

            foreach (var textbox in inputTextBoxes)
                textbox.Background = Brushes.White;

            if (CornerLineYFirst.Text != "" && !InputValid(CornerLineYFirst, ref LineYFirst))
                CornerLineYFirst.Background = Brushes.Red;

            if (CornerLineYSecond.Text != "" && !InputValid(CornerLineYSecond, ref LineYSecond))
                CornerLineYSecond.Background = Brushes.Red;

            if (CornerLineXFirst.Text != "" && !InputValid(CornerLineXFirst, ref LineXFirst))
                CornerLineXFirst.Background = Brushes.Red;

            if (CornerLineXSecond.Text != "" && !InputValid(CornerLineXSecond, ref LineXSecond))
                CornerLineXSecond.Background = Brushes.Red;

            if (ATextBoxIsRed(inputTextBoxes))
                cornerFeedback.Activate(cornerFeedback.LineXYInvalidValues);

            if (CornerLineYFirst.Text != "" && CornerLineXFirst.Text != "" && linexyFirstCalculated == false)
            {
                CornerLineYFirst.Background = Brushes.Red;
                CornerLineXFirst.Background = Brushes.Red;

                cornerFeedback.Activate(cornerFeedback.LineXYTooManyValues);
            }

            if (CornerLineYSecond.Text != "" && CornerLineXSecond.Text != "" && linexySecondCalculated == false)
            {
                CornerLineYSecond.Background = Brushes.Red;
                CornerLineXSecond.Background = Brushes.Red;

                cornerFeedback.Activate(cornerFeedback.LineXYTooManyValues);
            }

            if (!cornerFeedback.Calculated.Active || ATextBoxIsRed(inputTextBoxes))
                return;

            double additionFirst = Calc.Tan(corner.AngleCrossCutFirstBoard) * corner.WidthFirstBoard;
            double additionSecond = Calc.Tan(corner.AngleCrossCutSecondBoard) * corner.WidthSecondBoard;

            if (CornerLineYFirst.Text != "" && linexyFirstCalculated == false)
            {
                CornerLineXFirst.Text = Convert.ToString(Math.Round(LineYFirst + (2 * additionFirst), 2));
                linexyFirstCalculated = true;
            }
            else if (CornerLineXFirst.Text != "" && linexyFirstCalculated == false)
            {
                CornerLineYFirst.Text = Convert.ToString(Math.Round(LineXFirst - (2 * additionFirst), 2));
                linexyFirstCalculated = true;
            }

            if (CornerLineYSecond.Text != "" && linexySecondCalculated == false)
            {
                CornerLineXSecond.Text = Convert.ToString(Math.Round(LineYSecond + (2 * additionSecond), 2));
                linexySecondCalculated = true;
            }
            else if (CornerLineXSecond.Text != "" && linexySecondCalculated == false)
            {
                CornerLineYSecond.Text = Convert.ToString(Math.Round(LineXSecond - (2 * additionSecond), 2));
                linexySecondCalculated = true;
            }
        }

        /// <summary>
        /// Calculates the angle alpha.
        /// </summary>
        /// <param name="sender">The button to calculate angle alpha.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void CornerAngleAlpha_Click(object sender, RoutedEventArgs e)
        {
            cornerFeedback.Deactivate(cornerFeedback.AlphaChanged);

            double height = 0;
            double offsetFirst = 0;
            double offsetSecond = 0;

            var inputTextBoxes = new TextBox[]
            {
                CornerOffsetFirst,
                CornerOffsetSecond
            };

            if (CornerHeight.Text != "" && (!InputValid(CornerHeight, ref height) || height <= 0))
                CornerHeight.Background = Brushes.Red;

            if (CornerOffsetFirst.Text != "" && (!InputValid(CornerOffsetFirst, ref offsetFirst)))
                CornerOffsetFirst.Background = Brushes.Red;

            if (CornerOffsetSecond.Text != "" && (!InputValid(CornerOffsetSecond, ref offsetSecond)))
                CornerOffsetSecond.Background = Brushes.Red;

            if (CornerHeight.Background == Brushes.Red || ATextBoxIsRed(inputTextBoxes))
                cornerFeedback.Activate(cornerFeedback.InvalidValues);

            var calculated = false;

            if (CornerHeight.Background == Brushes.White && CornerHeight.Text != "" 
                && CornerOffsetFirst.Background == Brushes.White && CornerOffsetFirst.Text != "")
            {
                CornerAngleAlphaFirst.Text = Convert.ToString(Math.Round(Calc.Atan(offsetFirst / height), 4));

                calculated = true;
            }

            if (CornerHeight.Background == Brushes.White && CornerHeight.Text != "" 
                && CornerOffsetSecond.Background == Brushes.White && CornerOffsetSecond.Text != "")
            {
                CornerAngleAlphaSecond.Text = Convert.ToString(Math.Round(Calc.Atan(offsetSecond / height), 4));

                calculated = true;
            }

            if (calculated)
                cornerFeedback.Activate(cornerFeedback.AlphaCalculated);
        }

        /// <summary>
        /// Calculates the angle beta.
        /// </summary>
        /// <param name="sender">The button to calculate angle beta.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void CornerAngleBeta_Click(object sender, RoutedEventArgs e)
        {
            cornerFeedback.Deactivate(cornerFeedback.BetaChanged);

            short numberOfSides = 0;

            if (CornerNumberOfSides.Text == "")
                return;

            if (!InputValid(CornerNumberOfSides, ref numberOfSides) || numberOfSides < 3 || numberOfSides > 100)
            {
                CornerNumberOfSides.Background = Brushes.Red;
                cornerFeedback.Activate(cornerFeedback.InvalidValues);

                return;
            }

            double angleBeta = (numberOfSides - 2.0) * 180.0 / numberOfSides;

            CornerAngleBeta.Text = Convert.ToString(Math.Round(angleBeta, 4));

            cornerFeedback.Activate(cornerFeedback.BetaCalculated);
        }

        /// <summary>
        /// Changes the background of the textbox to white and updates the feedback area.
        /// </summary>
        /// <param name="sender">The textbox that called the method.</param>
        /// <param name="e">The TextChangedEventArgs.</param>
        private void CornerInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;

            senderTextBox.Background = Brushes.White;

            var helper = new TextBox[]
            {
                CornerHeight,
                CornerThicknessFirst,
                CornerThicknessSecond,
                CornerAngleAlphaFirst,
                CornerAngleAlphaSecond,
                CornerAngleBeta,
            };

            if (!ATextBoxIsRed(helper))
                cornerFeedback.Deactivate(cornerFeedback.InvalidValues);

            if (cornerFeedback.Calculated.Active && helper.Contains(senderTextBox))
            {
                cornerFeedback.Activate(cornerFeedback.InputChanged);
                cornerFeedback.Deactivate(cornerFeedback.Calculated);
            }

            helper = new TextBox[]
            {
                CornerOffsetFirst,
                CornerOffsetSecond,
                CornerAngleAlphaFirst,
                CornerAngleAlphaSecond,
                CornerHeight
            };

            if (cornerFeedback.AlphaCalculated.Active && helper.Contains(senderTextBox))
            {
                cornerFeedback.Activate(cornerFeedback.AlphaChanged);
                cornerFeedback.Deactivate(cornerFeedback.AlphaCalculated);
            }

            helper = new TextBox[]
            {
                CornerNumberOfSides,
                CornerAngleBeta
            };

            if (cornerFeedback.BetaCalculated.Active && helper.Contains(senderTextBox))
            {
                cornerFeedback.Activate(cornerFeedback.BetaChanged);
                cornerFeedback.Deactivate(cornerFeedback.BetaCalculated);
            }
        }

        /// <summary>
        /// Updates the feedback area if the compund miter is calculated and the checkbox is clicked.
        /// </summary>
        /// <param name="sender">The checkbox that called the method.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void CornerCheckbox_Click(object sender, RoutedEventArgs e)
        {
            if (cornerFeedback.Calculated.Active)
            {
                cornerFeedback.Activate(cornerFeedback.InputChanged);
                cornerFeedback.Deactivate(cornerFeedback.Calculated);
            }
        }

        /// <summary>
        /// Sets the background of a textbox to white if the input is greater zero.
        /// </summary>
        /// <param name="textBox">The textbox.</param>
        private void WhiteIfValidAndGreaterZero(TextBox textBox)
        {
            double value = 0;

            if (InputValid(textBox, ref value) && value >= 0)
                textBox.Background = Brushes.White;
        }

        /// <summary>
        /// Sets the background of the textbox to white and updates the feedback area.
        /// </summary>
        /// <param name="sender">The textbox that called the method.</param>
        /// <param name="e">The TextChangedEventArgs.</param>
        private void CornerInputLineXY_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textboxSender = (TextBox)sender;
            textboxSender.Background = Brushes.White;

            if (textboxSender == CornerLineXFirst || textboxSender == CornerLineYFirst)
                linexyFirstCalculated = false;

            if (textboxSender == CornerLineXSecond || textboxSender == CornerLineYSecond)
                linexySecondCalculated = false;

            if (CornerLineYFirst.Background == Brushes.White || CornerLineXFirst.Background == Brushes.White)
            {
                WhiteIfValidAndGreaterZero(CornerLineYFirst);
                WhiteIfValidAndGreaterZero(CornerLineXFirst);
            }

            if (CornerLineYSecond.Background == Brushes.White || CornerLineXSecond.Background == Brushes.White)
            {
                WhiteIfValidAndGreaterZero(CornerLineYSecond);
                WhiteIfValidAndGreaterZero(CornerLineXSecond);
            }

            if ((CornerLineYFirst.Background == Brushes.White || CornerLineXFirst.Background == Brushes.White)
                && (CornerLineYSecond.Background == Brushes.White || CornerLineXSecond.Background == Brushes.White))
            {
                cornerFeedback.Deactivate(cornerFeedback.LineXYTooManyValues);
            }

            var inputTextBoxes = new TextBox[]
            {
                CornerLineYFirst,
                CornerLineXFirst,
                CornerLineYSecond,
                CornerLineXSecond
            };

            if (!ATextBoxIsRed(inputTextBoxes))
            {
                cornerFeedback.Deactivate(cornerFeedback.LineXYInvalidValues);
            }
        }

        /// <summary>
        /// Resizes the columns of the grid so only the most right one gets bigger if the window gets bigger.
        /// </summary>
        /// <param name="sender">The grid to resize the columns in.</param>
        /// <param name="e">The SizeChangedEventArgs.</param>
        private void CornerGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            cornerColumnResize.ExpandMostRightIfBigger(sender, e);
        }

        /// <summary>
        /// Resizes the columns of the grid so only the one below the mouse is fully shown if the window gets smaller.
        /// </summary>
        /// <param name="sender">The grid to resize the columns in.</param>
        /// <param name="e">The MouseEventArgs.</param>
        private void CornerGrid_MouseMove(object sender, MouseEventArgs e)
        {
            cornerColumnResize.ShowFullyIfSmaller(sender, e);
        }

        #endregion

        #region Methods pyramid with top and bottom line
        
        /// <summary>
        /// Controls the zoom by changing camera position and look direction.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D.</param>
        /// <param name="e">The MouseWheelEventArgs.</param>
        private void PyramidLineGraphic_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Zoom(pyramidLinePerspectiveCamera, pyramidLineCameraAngle, e);
        }

        /// <summary>
        /// Rotates the 3D model when the mouse is moved and the left mouse button is hold.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D.</param>
        /// <param name="e">The MouseEventArgs.</param>
        private void PyramidLineGraphic_MouseMove(object sender, MouseEventArgs e)
        {
            Rotate3DModel(
                pyramidLineRotation, 
                ref pyramidLineCameraAngle, 
                pyramidLinePerspectiveCamera, 
                sender, 
                e);
        }

        /// <summary>
        /// Converts the angle if the input is valid and shows the result.
        /// </summary>
        /// <param name="sender">The textbox for angle conversion.</param>
        /// <param name="e">The TextChangedEventArgs.</param>
        private void PyramidLineAngleConversion_TextChanged(object sender, TextChangedEventArgs e)
        {
            AngleConversion(PyramidLineAngleConversion, pyramidLineAngleConversionResult);
        }

        /// <summary>
        /// Resets the result textblocks.
        /// </summary>
        private void PyramidLineResultReset()
        {
            var resultTextblocks = new TextBlock[]
            {
                PyramidLineAngleCrossCut,
                PyramidLineTiltAngleSawBlade,
                PyramidLineWidth,
                PyramidLineWidthWithSlant,
                PyramidLineAngleDihedral,
                PyramidLineOffset,
                PyramidLineSlantS,
                PyramidLineTiltAngle,
                PyramidLineInscribedTopOuter,
                PyramidLineInscribedTopInner,
                PyramidLineInscribedBottomOuter,
                PyramidLineInscribedBottomInner,
                PyramidLineCircumscribedTopOuter,
                PyramidLineCircumscribedTopInner,
                PyramidLineCircumscribedBottomOuter,
                PyramidLineCircumscribedBottomInner,
            };

            foreach (var textBlock in resultTextblocks)
            {
                textBlock.Text = "";
            }
        }

        /// <summary>
        /// Resets everything.
        /// </summary>
        /// <param name="sender">The button to reset everything.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void PyramidLineReset_Click(object sender, RoutedEventArgs e)
        {
            PyramidLineResultReset();

            var inputTextboxes = new TextBox[] { 
                PyramidLineHeight, 
                PyramidLineThickness, 
                PyramidLineNumberOfSides,
                PyramidLineBottomLineLength, 
                PyramidLineTopLineLength 
            };

            foreach (var textbox in inputTextboxes)
            {
                textbox.Text = "";
                textbox.Background = Brushes.White;
            }

            pyramidLineModelVisual3D.Content = new Model3DGroup();
            
            pyramidLineFeedback.Deactivate(
                pyramidLineFeedback.InvalidValues, 
                pyramidLineFeedback.InputChanged, 
                pyramidLineFeedback.Calculated);

            pyramidLineFeedback.Activate(pyramidLineFeedback.EnterValues);
        }

        /// <summary>
        /// Calculates everything and shows the results.
        /// </summary>
        /// <param name="sender">The button to calculate the compound miter.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void PyramidLineCalculation_Click(object sender, RoutedEventArgs e)
        {
            pyramidLineFeedback.Deactivate(pyramidLineFeedback.InputChanged);

            double height = 0;
            double thickness = 0;
            short numberOfSides = 0;
            double bottomLine = 0;
            double topLine = 0;

            var inputTextBoxes = new TextBox[]
            {
                PyramidLineHeight,
                PyramidLineThickness,
                PyramidLineNumberOfSides,
                PyramidLineBottomLineLength,
                PyramidLineTopLineLength
            };

            if (ATextBoxIsEmpty(inputTextBoxes))
            {
                PyramidLineResultReset();
                pyramidLineModelVisual3D.Content = new Model3DGroup();
                pyramidLineFeedback.Deactivate(pyramidLineFeedback.Calculated);
                pyramidLineFeedback.Activate(pyramidLineFeedback.EnterValues);

                return;
            }

            if (!InputValid(PyramidLineHeight, ref height) 
                || height <= 0)
                PyramidLineHeight.Background = Brushes.Red;

            if (!InputValid(PyramidLineThickness, ref thickness) 
                || thickness <= 0)
                PyramidLineThickness.Background = Brushes.Red;

            if (!InputValid(PyramidLineNumberOfSides, ref numberOfSides) 
                || numberOfSides < 3 
                || numberOfSides > 100)
                PyramidLineNumberOfSides.Background = Brushes.Red;

            if (!InputValid(PyramidLineBottomLineLength, ref bottomLine) 
                || bottomLine < 0)
                PyramidLineBottomLineLength.Background = Brushes.Red;

            if (!InputValid(PyramidLineTopLineLength, ref topLine) 
                || topLine < 0)
                PyramidLineTopLineLength.Background = Brushes.Red;

            if (ATextBoxIsRed(inputTextBoxes))
            {
                PyramidLineResultReset();
                pyramidLineModelVisual3D.Content = new Model3DGroup();
                pyramidLineFeedback.Deactivate(pyramidLineFeedback.Calculated);
                pyramidLineFeedback.Activate(pyramidLineFeedback.InvalidValues);
                pyramidLineFeedback.Activate(pyramidLineFeedback.EnterValues);

                return;
            }

            pyramidLine.Height = height;
            pyramidLine.ThicknessFirstBoard = thickness;
            pyramidLine.ThicknessSecondBoard = thickness;
            pyramidLine.NumberOfSides = numberOfSides;
            pyramidLine.BottomSideLength = bottomLine;
            pyramidLine.TopSideLength = topLine;

            double doubleNumberOfSides = Convert.ToDouble(pyramidLine.NumberOfSides);

            pyramidLine.AngleBeta = (doubleNumberOfSides - 2.0) * 180.0 / doubleNumberOfSides;

            double inscribedTopOuter = Calc.InscribedCircleRadius(pyramidLine.TopSideLength, pyramidLine.NumberOfSides);
            double inscribedBottomOuter = Calc.InscribedCircleRadius(pyramidLine.BottomSideLength, pyramidLine.NumberOfSides);

            double angleAlpha = Calc.Atan((inscribedBottomOuter - inscribedTopOuter) / pyramidLine.Height);

            pyramidLine.AngleAlphaFirstBoard = angleAlpha;
            pyramidLine.AngleAlphaSecondBoard = angleAlpha;

            pyramidLine.MiterJoint = true;

            pyramidLine.Calculation();

            PyramidLineAngleCrossCut.Text = Math.Round(pyramidLine.AngleCrossCutFirstBoard, 2) + "°";
            PyramidLineTiltAngleSawBlade.Text = Math.Round(pyramidLine.AngleSawBladeTiltFirstBoard, 2) + "°";
            PyramidLineWidth.Text = ErrorIfTooLarge(pyramidLine.WidthFirstBoard);
            PyramidLineWidthWithSlant.Text = ErrorIfTooLarge(pyramidLine.WidthWithSlantFirstBoard);

            PyramidLineAngleDihedral.Text = Math.Round(pyramidLine.AngleDihedral, 2) + "°";

            double offset = Calc.Sin(pyramidLine.AngleAlphaFirstBoard) * pyramidLine.WidthFirstBoard;
            double slantS = pyramidLine.ThicknessFirstBoard / Calc.Cos(pyramidLine.AngleAlphaFirstBoard);

            PyramidLineOffset.Text = ErrorIfTooLarge(offset);
            PyramidLineSlantS.Text = ErrorIfTooLarge(slantS);

            PyramidLineTiltAngle.Text = Math.Round(pyramidLine.AngleAlphaFirstBoard, 2) + " °";

            double circumscribedTopOuter = Calc.CircumscribedCircleRadius(pyramidLine.TopSideLength, pyramidLine.NumberOfSides);
            double circumscribedBottomOuter = Calc.CircumscribedCircleRadius(pyramidLine.BottomSideLength, pyramidLine.NumberOfSides);

            double miterLine = slantS / Calc.Sin(pyramidLine.AngleBeta / 2.0);

            double inscribedTopInner = inscribedTopOuter - slantS;
            double circumscribedTopInner = circumscribedTopOuter - miterLine;
            double inscribedBottomInner = inscribedBottomOuter - slantS;
            double circumscribedBottomInner = circumscribedBottomOuter - miterLine;

            PyramidLineInscribedTopOuter.Text = ErrorIfTooLarge(inscribedTopOuter);
            PyramidLineCircumscribedTopOuter.Text = ErrorIfTooLarge(circumscribedTopOuter);
            PyramidLineInscribedBottomOuter.Text = ErrorIfTooLarge(inscribedBottomOuter);
            PyramidLineCircumscribedBottomOuter.Text = ErrorIfTooLarge(circumscribedBottomOuter);

            PyramidLineInscribedTopInner.Text = ErrorIfTooLarge(inscribedTopInner);
            PyramidLineCircumscribedTopInner.Text = ErrorIfTooLarge(circumscribedTopInner);
            PyramidLineInscribedBottomInner.Text = ErrorIfTooLarge(inscribedBottomInner);
            PyramidLineCircumscribedBottomInner.Text = ErrorIfTooLarge(circumscribedBottomInner);

            pyramidLine.CreateModel(pyramidLineModelVisual3D);

            pyramidLineFeedback.Deactivate(pyramidLineFeedback.EnterValues);
            pyramidLineFeedback.Activate(pyramidLineFeedback.Calculated);
        }

        /// <summary>
        /// Sets the background of the textbox to white and updates the feedback area.
        /// </summary>
        /// <param name="sender">The textbox that called the method.</param>
        /// <param name="e">The TextChangedEventArgs.</param>
        private void PyramidLineInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).Background = Brushes.White;

            var inputTextBoxes = new TextBox[]
            {
                PyramidLineHeight,
                PyramidLineThickness,
                PyramidLineNumberOfSides,
                PyramidLineBottomLineLength,
                PyramidLineTopLineLength
            };

            if (!ATextBoxIsRed(inputTextBoxes))
                pyramidLineFeedback.Deactivate(pyramidLineFeedback.InvalidValues);

            if (pyramidLineFeedback.Calculated.Active)
            {
                pyramidLineFeedback.Activate(pyramidLineFeedback.InputChanged);
                pyramidLineFeedback.Deactivate(pyramidLineFeedback.Calculated);
            }
        }

        /// <summary>
        /// Resizes the columns of the grid so only the most right one gets bigger if the window gets bigger.
        /// </summary>
        /// <param name="sender">The grid to resize the columns in.</param>
        /// <param name="e">The SizeChangedEventArgs.</param>
        private void PyramidLineGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            pyramidLineColumnResize.ExpandMostRightIfBigger(sender, e);
        }

        /// <summary>
        /// Resizes the columns of the grid so only the one below the mouse is fully shown if the window gets smaller.
        /// </summary>
        /// <param name="sender">The grid to resize the columns in.</param>
        /// <param name="e">The MouseEventArgs.</param>
        private void PyramidLineGrid_MouseMove(object sender, MouseEventArgs e)
        {
            pyramidLineColumnResize.ShowFullyIfSmaller(sender, e);
        }

        #endregion

        #region Methods pyramid with tilt angle

        /// <summary>
        /// Controls the zoom by changing the camera position and lookdirection.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D</param>
        /// <param name="e">The MouseWheelEventArgs.</param>
        private void PyramidAngleGraphic_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Zoom(pyramidAnglePerspectiveCamera, pyramidAngleCameraAngle, e);
        }

        /// <summary>
        /// Rotates the 3D model on mouse movement if the left button is hold.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D.</param>
        /// <param name="e">The MouseEventArgs.</param>
        private void PyramidAngleGraphic_MouseMove(object sender, MouseEventArgs e)
        {
            Rotate3DModel(
                pyramidAngleRotation, 
                ref pyramidAngleCameraAngle, 
                pyramidAnglePerspectiveCamera, 
                sender, 
                e);
        }

        /// <summary>
        /// Converts the angle if the input is valid and shows the result.
        /// </summary>
        /// <param name="sender">The textbox for angle conversion.</param>
        /// <param name="e">The TextChangedEventArgs.</param>
        private void PyramidAngleAngleConversion_TextChanged(object sender, TextChangedEventArgs e)
        {
            AngleConversion(PyramidAngleAngleConversion, PyramidAngleAngleConversionResult);
        }

        /// <summary>
        /// Resets the textboxes for the results.
        /// </summary>
        private void PyramidAngleResultReset()
        {
            var resultTextblocks = new TextBlock[]
            {
                PyramidAngleAngleCrossCut,
                PyramidAngleTiltAngleSawBlade,
                PyramidAngleWidth,
                PyramidAngleWidthWithSlant,
                PyramidAngleAngleDihedral,
                PyramidAngleOffsetResult,
                PyramidAngleSlantS,
                PyramidAngleTopLine,
                PyramidAngleInscribedTopOuter,
                PyramidAngleInscribedTopInner,
                PyramidAngleInscribedBottomOuter,
                PyramidAngleInscribedBottomInner,
                PyramidAngleCircumscribedTopOuter,
                PyramidAngleCircumscribedTopInner,
                PyramidAngleCircumscribedBottomOuter,
                PyramidAngleCircumscribedBottomInner,
            };

            foreach (var textBlock in resultTextblocks)
                textBlock.Text = "";
        }

        /// <summary>
        /// Resets everything.
        /// </summary>
        /// <param name="sender">The button to reset everything.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void PyramidAngleReset_Click(object sender, RoutedEventArgs e)
        {
            PyramidAngleResultReset();

            var inputTextboxes = new TextBox[] 
            {
                PyramidAngleHeight, 
                PyramidAngleThickness, 
                PyramidAngleNumberOfSides,
                PyramidAngleBottomLine, 
                PyramidAngleTiltAngle, 
                PyramidAngleOffset 
            };

            foreach (var textbox in inputTextboxes)
            {
                textbox.Text = "";
                textbox.Background = Brushes.White;
            }

            pyramidAngleModelVisual3D.Content = new Model3DGroup();

            pyramidAngleFeedback.Deactivate(
                pyramidAngleFeedback.InvalidValues, 
                pyramidAngleFeedback.InputChanged, 
                pyramidAngleFeedback.Calculated, 
                pyramidAngleFeedback.TiltAngleCalculated, 
                pyramidAngleFeedback.TiltAngleChanged, 
                pyramidAngleFeedback.HeightLargerThanResulting, 
                pyramidAngleFeedback.HeightNeeded
            );

            pyramidAngleFeedback.Activate(pyramidAngleFeedback.EnterValues);
        }

        /// <summary>
        /// Calculates everything and shows the results.
        /// </summary>
        /// <param name="sender">The button to calculate everything.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void PyramidAngleCalculation_Click(object sender, RoutedEventArgs e)
        {
            pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.InputChanged);

            short numberOfSides = 0;
            double thickness = 0;
            double bottomLine = 0;
            double tiltAngle = 0;
            double height = 0;
            double resultingHeight = 0;

            var inputTextBoxes = new TextBox[]
            {
                PyramidAngleThickness,
                PyramidAngleNumberOfSides,
                PyramidAngleBottomLine,
                PyramidAngleTiltAngle
            };

            if (ATextBoxIsEmpty(inputTextBoxes))
            {
                PyramidAngleResultReset();
                pyramidAngleModelVisual3D.Content = new Model3DGroup();
                pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.Calculated);
                pyramidAngleFeedback.Activate(pyramidAngleFeedback.EnterValues);

                return;
            }

            if (!InputValid(PyramidAngleNumberOfSides, ref numberOfSides) 
                || numberOfSides < 3 || numberOfSides > 100)
                PyramidAngleNumberOfSides.Background = Brushes.Red;

            if (!InputValid(PyramidAngleThickness, ref thickness) 
                || thickness <= 0)
                PyramidAngleThickness.Background = Brushes.Red;

            if (!InputValid(PyramidAngleBottomLine, ref bottomLine) 
                || bottomLine < 0)
                PyramidAngleBottomLine.Background = Brushes.Red;

            if (!InputValid(PyramidAngleTiltAngle, ref tiltAngle) 
                || tiltAngle < -90 || tiltAngle > 90)
                PyramidAngleTiltAngle.Background = Brushes.Red;

            if ((PyramidAngleHeight.Text != "") && (!InputValid(PyramidAngleHeight, ref height) 
                || height <= 0))
                PyramidAngleHeight.Background = Brushes.Red;

            inputTextBoxes = new TextBox[]
            {
                PyramidAngleHeight,
                PyramidAngleThickness,
                PyramidAngleNumberOfSides,
                PyramidAngleBottomLine,
                PyramidAngleTiltAngle
            };

            if (ATextBoxIsRed(inputTextBoxes))
            {
                PyramidAngleResultReset();
                pyramidAngleModelVisual3D.Content = new Model3DGroup();
                pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.Calculated);
                pyramidAngleFeedback.Activate(pyramidAngleFeedback.InvalidValues);
                pyramidAngleFeedback.Activate(pyramidAngleFeedback.EnterValues);
                
                return;
            }

            pyramidAngle.NumberOfSides = numberOfSides;
            pyramidAngle.ThicknessFirstBoard = thickness;
            pyramidAngle.ThicknessSecondBoard = thickness;
            pyramidAngle.BottomSideLength = bottomLine;
            pyramidAngle.AngleAlphaFirstBoard = tiltAngle;
            pyramidAngle.AngleAlphaSecondBoard = tiltAngle;

            double inscribedBottomOuter = Calc.InscribedCircleRadius(pyramidAngle.BottomSideLength, pyramidAngle.NumberOfSides);

            resultingHeight = Calc.Tan(90.0 - pyramidAngle.AngleAlphaFirstBoard) * inscribedBottomOuter;

            if (pyramidAngle.AngleAlphaFirstBoard > 0)
            {
                if ((PyramidAngleHeight.Text != "") && height > resultingHeight)
                {
                    PyramidAngleHeight.Background = Brushes.Red;

                    pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.Calculated);
                    pyramidAngleFeedback.Activate(pyramidAngleFeedback.HeightLargerThanResulting);
                    pyramidAngleFeedback.Activate(pyramidAngleFeedback.EnterValues);
                }
                else
                {
                    pyramidAngle.Height = height;
                }

                if (PyramidAngleHeight.Text == "")
                {
                    PyramidAngleHeight.Text = Convert.ToString(Math.Round(resultingHeight - 0.01, 2));
                    pyramidAngle.Height = double.Parse(PyramidAngleHeight.Text);
                }
            }
            else
            {
                if (PyramidAngleHeight.Text == "")
                {
                    PyramidAngleHeight.Background = Brushes.Red;

                    pyramidAngleFeedback.Activate(pyramidAngleFeedback.HeightNeeded);
                    pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.Calculated);
                    pyramidAngleFeedback.Activate(pyramidAngleFeedback.EnterValues);
                }
                else
                {
                    pyramidAngle.Height = height;
                }
            }

            if (PyramidAngleHeight.Background == Brushes.Red)
            {
                PyramidAngleResultReset();
                pyramidAngleModelVisual3D.Content = new Model3DGroup();

                return;
            }

            pyramidAngle.AngleBeta = Convert.ToDouble((pyramidAngle.NumberOfSides - 2) * 180.0 / pyramidAngle.NumberOfSides);

            pyramidAngle.MiterJoint = true;

            pyramidAngle.Calculation();

            PyramidAngleAngleCrossCut.Text = Math.Round(pyramidAngle.AngleCrossCutFirstBoard, 2) + "°";
            PyramidAngleTiltAngleSawBlade.Text = Math.Round(pyramidAngle.AngleSawBladeTiltFirstBoard, 2) + "°";
            PyramidAngleWidth.Text = ErrorIfTooLarge(pyramidAngle.WidthFirstBoard);
            PyramidAngleWidthWithSlant.Text = ErrorIfTooLarge(pyramidAngle.WidthWithSlantFirstBoard);

            PyramidAngleAngleDihedral.Text = Math.Round(pyramidAngle.AngleDihedral, 2) + "°";

            double slantS = pyramidAngle.ThicknessFirstBoard / Calc.Cos(pyramidAngle.AngleAlphaFirstBoard);
            double offset = Calc.Sin(pyramidAngle.AngleAlphaFirstBoard) * pyramidAngle.WidthFirstBoard;

            PyramidAngleSlantS.Text = ErrorIfTooLarge(slantS);
            PyramidAngleOffsetResult.Text = ErrorIfTooLarge(offset);

            double circumscribedBottomOuter = Calc.CircumscribedCircleRadius(pyramidAngle.BottomSideLength, pyramidAngle.NumberOfSides);

            pyramidAngle.TopSideLength = (inscribedBottomOuter - offset) * (2 * Calc.Tan(180.0 / pyramidAngle.NumberOfSides));

            double inscribedTopOuter = Calc.InscribedCircleRadius(pyramidAngle.TopSideLength, pyramidAngle.NumberOfSides);
            double circumscribedTopOuter = Calc.CircumscribedCircleRadius(pyramidAngle.TopSideLength, pyramidAngle.NumberOfSides);
            
            double miterLine = slantS / Calc.Sin(pyramidAngle.AngleBeta / 2.0);

            double inscribedTopInner = inscribedTopOuter - slantS;
            double circumscribedTopInner = circumscribedTopOuter - miterLine;
            double inscribedBottomInner = inscribedBottomOuter - slantS;
            double circumscribedBottomInner = circumscribedBottomOuter - miterLine;

            PyramidAngleTopLine.Text = ErrorIfTooLarge(pyramidAngle.TopSideLength);

            PyramidAngleInscribedTopOuter.Text = ErrorIfTooLarge(inscribedTopOuter);
            PyramidAngleCircumscribedTopOuter.Text = ErrorIfTooLarge(circumscribedTopOuter);
            PyramidAngleInscribedBottomOuter.Text = ErrorIfTooLarge(inscribedBottomOuter);
            PyramidAngleCircumscribedBottomOuter.Text = ErrorIfTooLarge(circumscribedBottomOuter);

            PyramidAngleInscribedTopInner.Text = ErrorIfTooLarge(inscribedTopInner);
            PyramidAngleCircumscribedTopInner.Text = ErrorIfTooLarge(circumscribedTopInner);
            PyramidAngleInscribedBottomInner.Text = ErrorIfTooLarge(inscribedBottomInner);
            PyramidAngleCircumscribedBottomInner.Text = ErrorIfTooLarge(circumscribedBottomInner);


            pyramidAngle.CreateModel(pyramidAngleModelVisual3D);

            pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.EnterValues);
            pyramidAngleFeedback.Activate(pyramidAngleFeedback.Calculated);
        }

        /// <summary>
        /// Calculates the tilt angle if input is valid.
        /// </summary>
        /// <param name="sender">The button to calculate the tilt angle.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void PyramidAngleTiltAngle_Click(object sender, RoutedEventArgs e)
        {
            pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.TiltAngleChanged);

            double height = 0;
            double offset = 0;

            if (PyramidAngleHeight.Text == "" || PyramidAngleOffset.Text == "")
                return;

            if (!InputValid(PyramidAngleHeight, ref height) || height <= 0)
                PyramidAngleHeight.Background = Brushes.Red;

            if (!InputValid(PyramidAngleOffset, ref offset))
                PyramidAngleOffset.Background = Brushes.Red;

            if (PyramidAngleHeight.Background == Brushes.Red 
                || PyramidAngleOffset.Background == Brushes.Red)
            {
                pyramidAngleFeedback.Activate(pyramidAngleFeedback.InvalidValues);
                return;
            }

            double tiltAngle = Math.Round(Calc.Atan(offset / height), 4);
            PyramidAngleTiltAngle.Text = Convert.ToString(tiltAngle);

            pyramidAngleFeedback.Activate(pyramidAngleFeedback.TiltAngleCalculated);
        }

        /// <summary>
        /// Changes the textbox background to white and updates the feedback area.
        /// </summary>
        /// <param name="sender">The textbox that called the method.</param>
        /// <param name="e">The TextChangedEventArgs.</param>
        private void PyramidAngleInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var feedback = pyramidAngleFeedback;
            TextBox[] helper;

            senderTextBox.Background = Brushes.White;

            helper = new TextBox[] {
                PyramidAngleHeight,
                PyramidAngleThickness,
                PyramidAngleNumberOfSides,
                PyramidAngleBottomLine,
                PyramidAngleTiltAngle
            };

            if (!ATextBoxIsRed(helper))
                feedback.Deactivate(feedback.InvalidValues);

            if (feedback.Calculated.Active && (helper.Contains(senderTextBox)))
            {
                feedback.Activate(feedback.InputChanged);
                feedback.Deactivate(feedback.Calculated);
            }

            helper = new TextBox[] {
                PyramidAngleHeight,
                PyramidAngleOffset,
                PyramidAngleTiltAngle
            };

            if (feedback.TiltAngleCalculated.Active && (helper.Contains(senderTextBox)))
            {
                feedback.Activate(feedback.TiltAngleChanged);
                feedback.Deactivate(feedback.TiltAngleCalculated);
            }

            helper = new TextBox[] {
                PyramidAngleHeight,
                PyramidAngleTiltAngle
            };

            if (helper.Contains(senderTextBox))
            {
                PyramidAngleHeight.Background = Brushes.White;

                feedback.Deactivate(feedback.HeightNeeded);
                feedback.Deactivate(feedback.HeightLargerThanResulting);
            }
        }

        /// <summary>
        /// Resizes the columns of the grid so only the most right one gets bigger if the window gets bigger.
        /// </summary>
        /// <param name="sender">The grid to resize the columns in.</param>
        /// <param name="e">The SizeChangedEventArgs.</param>
        private void PyramidAngleGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            pyramidAngleColumnResize.ExpandMostRightIfBigger(sender, e);
        }

        /// <summary>
        /// Resizes the columns of the grid so only the one below the mouse is fully shown if the window gets smaller.
        /// </summary>
        /// <param name="sender">The grid to resize the columns in.</param>
        /// <param name="e">The MouseEventArgs.</param>
        private void PyramidAngleGrid_MouseMove(object sender, MouseEventArgs e)
        {
            pyramidAngleColumnResize.ShowFullyIfSmaller(sender, e);
        }

        #endregion

        #region Shared methods

        /// <summary>
        /// Shows the license.
        /// </summary>
        /// <param name="sender">The button.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void ButtonAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(@"Schifterschnitt V4
A program for joiners to calculate compound miters.
Copyright (C) 2020 Michael Pütz

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.", "Lizenz");
        }

        /// <summary>
        /// Checks if the input is valid.
        /// </summary>
        /// <param name="textBox">The textbox in which the input is.</param>
        /// <param name="number">The variable to store the input in if it is valid.</param>
        /// <returns>True if the input is valid.</returns>
        private bool InputValid(TextBox textBox, ref double number)
        {
            if (textBox.Text.Contains(".") || textBox.Text.Contains("NaN"))
                return false;

            if (!double.TryParse(textBox.Text, out number))
                return false;

            return true;
        }

        /// <summary>
        /// Checks if the input is valid.
        /// </summary>
        /// <param name="textBox">The textbox in which the input is.</param>
        /// <param name="number">The variable to store the input in if it is valid.</param>
        /// <returns>True if the input is valid.</returns>
        private bool InputValid(TextBox textBox, ref short number)
        {
            if (textBox.Text.Contains(".") || textBox.Text.Contains("NaN"))
                return false;

            if (!short.TryParse(textBox.Text, out number))
                return false;

            return true;
        }
        
        /// <summary>
        /// Loads all tabs after the window is loaded.
        /// </summary>
        /// <param name="sender">The window.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            int selected = tabControl.SelectedIndex;

            foreach (var item in tabControl.Items)
            {
                ((TabItem)item).IsSelected = true;
                ((TabItem)item).UpdateLayout();
            }

            tabControl.SelectedIndex = selected;
        }

        /// <summary>
        /// Calculates 90° - angle.
        /// </summary>
        /// <param name="textBox">The textbox with the input.</param>
        /// <param name="textBlock">The textblock to show the result.</param>
        private void AngleConversion(TextBox textBox, TextBlock textBlock)
        {
            double x = 0;

            if (textBox.Text == "")
            {
                textBox.Background = Brushes.White;
                textBlock.Text = "";
            }
            else if (InputValid(textBox, ref x))
            {
                textBox.Background = Brushes.White;
                textBlock.Text = Convert.ToString(90 - x) + "°";
            }
            else
            {
                textBox.Background = Brushes.Red;
                textBlock.Text = "";
            }
        }

        /// <summary>
        /// Checks if a textbox in an array has a red background.
        /// </summary>
        /// <param name="textBoxes">The textboxes to check.</param>
        /// <returns>True if the background of at least one textbox is red.</returns>
        private bool ATextBoxIsRed(TextBox[] textBoxes)
        {
            foreach (var textBox in textBoxes)
            {
                if (textBox.Background == Brushes.Red)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a textbox in an array is empty.
        /// </summary>
        /// <param name="textBoxes">The textboxes to check.</param>
        /// <returns>True if at least one textbox is empty.</returns>
        private bool ATextBoxIsEmpty(TextBox[] textBoxes)
        {
            foreach (var textBox in textBoxes)
            {
                if (textBox.Text == "")
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Checks if a result is too large to display and replaces it with an error message if so.
        /// </summary>
        /// <param name="number">The value to check.</param>
        /// <returns>The value rounded to 2 decimals if the value is small enough.</returns>
        /// <returns>A string saying "Error" if the value is too large to display.</returns>
        public static string ErrorIfTooLarge(double number)
        {
            if (number < 99000 && number > -99000)
                return Math.Round(number, 2) + " mm";
            else
                return "Error";
        }

        #region Graphic

        /// <summary>
        /// Changes the size of the graphic to fit the size of the viewport3D.
        /// </summary>
        /// <param name="sender">The viewport3D that shows the graphic.</param>
        /// <param name="e">The SizeChangedEventArgs.</param>
        private void AdjustGraphicSize(object sender, SizeChangedEventArgs e)
        {
            // Make sure we don't divide by zero when the window is loaded.
            if (e.PreviousSize.Width == 0 || e.PreviousSize.Height == 0)
                return;
            
            PerspectiveCamera camera = (PerspectiveCamera)((Viewport3D)sender).Camera;
            
            // The graphic should stay the same if the width gets bigger.
            double widthChangeValue = e.NewSize.Width / e.PreviousSize.Width;

            // The graphic should adjust its size if the height gets bigger.
            double heightChangeValue = e.PreviousSize.Height / e.NewSize.Height;

            Point3D position = camera.Position;

            position.X *= widthChangeValue * heightChangeValue;
            position.Y *= widthChangeValue * heightChangeValue;
            position.Z *= widthChangeValue * heightChangeValue;
            
            camera.SetValue(PerspectiveCamera.PositionProperty, position);

            var lookDirection = new Vector3D(position.X * -1, position.Y * -1, position.Z * -1);

            camera.SetValue(PerspectiveCamera.LookDirectionProperty, lookDirection);
        }

        /// <summary>
        /// Controls the zoom in the viewport3D.
        /// </summary>
        /// <param name="camera">The camera of the viewport3D.</param>
        /// <param name="cameraAngle">The angle of the camera looking up or down.</param>
        /// <param name="e">The MouseWheelEventArgs.</param>
        private void Zoom(PerspectiveCamera camera, double cameraAngle, MouseWheelEventArgs e)
        {
            var newPosition = camera.Position;
            double distanceFlat = Math.Abs(camera.Position.X) * Math.Sqrt(2);
            double distanceToZero = Math.Sqrt(Math.Pow(Math.Abs(camera.Position.Z), 2) + Math.Pow(distanceFlat, 2));
            var zoomSpeed = 0.7;

            if (e.Delta > 0 && distanceToZero > 2.5)
            {
                double newDistance = distanceToZero - zoomSpeed;
                double newXYPosition = Math.Abs(Calc.Cos(cameraAngle) * newDistance / Math.Sqrt(2)) * -1;
                newPosition.X = newXYPosition;
                newPosition.Y = newXYPosition;
                newPosition.Z = Calc.Sin(cameraAngle) * newDistance;
            }

            if (e.Delta < 0 && distanceToZero < 20)
            {
                double newDistance = distanceToZero + zoomSpeed;
                double newXYPosition = Math.Abs(Calc.Cos(cameraAngle) * newDistance / Math.Sqrt(2)) * -1;
                newPosition.X = newXYPosition;
                newPosition.Y = newXYPosition;
                newPosition.Z = Calc.Sin(cameraAngle) * newDistance;
            }

            camera.SetValue(PerspectiveCamera.PositionProperty, newPosition);

            var lookDirection = new Vector3D(newPosition.X * -1, newPosition.Y * -1, newPosition.Z * -1);

            camera.SetValue(PerspectiveCamera.LookDirectionProperty, lookDirection);
        }

        /// <summary>
        /// Saves the mouse position when the mouse button is pressed.
        /// </summary>
        /// <param name="sender">The grid on which the mouse is.</param>
        /// <param name="e">The MouseButtonEventArgs.</param>
        private void SaveMousePosition(object sender, MouseButtonEventArgs e)
        {
            mousePosition = e.GetPosition((Grid)sender);
        }

        /// <summary>
        /// Turns the 3D model based on the mouse movement when the left button is hold.
        /// </summary>
        /// <param name="rotation">The rotation to change.</param>
        /// <param name="cameraAngle">The angle of the camera looking up or down.</param>
        /// <param name="camera">The camera that looks on the 3D model.</param>
        /// <param name="sender">The grid on which the mouse is.</param>
        /// <param name="e">The MouseEventArgs.</param>
        private void Rotate3DModel(
            AxisAngleRotation3D rotation, 
            ref double cameraAngle, 
            PerspectiveCamera camera, 
            object sender, 
            MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            Point newMousePosition = e.GetPosition((Grid)sender);

            var sensitivity = 0.7;

            var movementLeftRight = newMousePosition.X - mousePosition.X;

            if (movementLeftRight > 0)
                rotation.Angle += movementLeftRight * sensitivity;
            if (movementLeftRight < 0)
                rotation.Angle += movementLeftRight * sensitivity;

            var movementUpDown = newMousePosition.Y - mousePosition.Y;

            if (movementUpDown > 0 && cameraAngle < 90)
                cameraAngle += movementUpDown * sensitivity;
            if (movementUpDown < 0 && cameraAngle > -90)
                cameraAngle += movementUpDown * sensitivity;

            if (cameraAngle < -90)
                cameraAngle = -90;
            if (cameraAngle > 90)
                cameraAngle = 90;

            double distanceFlat = Math.Abs(camera.Position.X) * Math.Sqrt(2);
            double distance = Math.Sqrt(Math.Pow(Math.Abs(camera.Position.Z), 2) + Math.Pow(distanceFlat, 2));
            double newXYPosition = Math.Abs(Calc.Cos(cameraAngle) * distance / Math.Sqrt(2)) * -1;

            var newPosition = new Point3D
            {
                Z = Calc.Sin(cameraAngle) * distance,
                X = newXYPosition,
                Y = newXYPosition
            };

            camera.Position = newPosition;
            camera.LookDirection = new Vector3D(newPosition.X * -1, newPosition.Y * -1, newPosition.Z * -1);
            mousePosition = newMousePosition;
        }

        #endregion

        #endregion
    }
}
