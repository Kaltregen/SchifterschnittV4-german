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

        TextBox[] eckeEingaben;
        TextBox[] pyramideLinieEingaben;
        TextBox[] pyramideWinkelEingaben;

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
            modelVisual3dEcke.Transform = eckeTransformation;

            pyramidLineRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 1);
            RotateTransform3D pyramideLinieTransformation = new RotateTransform3D(pyramidLineRotation);
            modelVisual3dPyramideLinie.Transform = pyramideLinieTransformation;

            pyramidAngleRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 1);
            RotateTransform3D pyramideWinkelTransformation = new RotateTransform3D(pyramidAngleRotation);
            modelVisual3dPyramideWinkel.Transform = pyramideWinkelTransformation;
            
            cornerCameraAngle = Calc.Atan(5 / Math.Sqrt(Math.Pow(5, 2) + Math.Pow(5, 2)));
            pyramidLineCameraAngle = Calc.Atan(5 / Math.Sqrt(Math.Pow(5, 2) + Math.Pow(5, 2)));
            pyramidAngleCameraAngle = Calc.Atan(5 / Math.Sqrt(Math.Pow(5, 2) + Math.Pow(5, 2)));
            
            cornerFeedback = new FeedbackArea(gridEckeFeedback);
            pyramidLineFeedback = new FeedbackArea(gridPyramideLinieFeedback);
            pyramidAngleFeedback = new FeedbackArea(gridPyramideWinkelFeedback);

            cornerFeedback.Activate(cornerFeedback.EnterValues);
            pyramidLineFeedback.Activate(pyramidLineFeedback.EnterValues);
            pyramidAngleFeedback.Activate(pyramidAngleFeedback.EnterValues);

            pyramideLinieEingaben = new TextBox[] { 
                textBoxPyramideLinieHoehe, 
                textBoxPyramideLinieStaerke, 
                textBoxPyramideLinieAnzahlSeiten,
                textBoxPyramideLinieGrundlinie, 
                textBoxPyramideLinieOberlinie 
            };

            pyramideWinkelEingaben = new TextBox[] { 
                textBoxPyramideWinkelHoehe, 
                textBoxPyramideWinkelStaerke, 
                textBoxPyramideWinkelAnzahlSeiten,
                textBoxPyramideWinkelGrundlinie, 
                textBoxPyramideWinkelNeigungswinkel, 
                textBoxPyramideWinkelBreitenversatz 
            };
        }

        #endregion

        #region Methods corner
        
        /// <summary>
        /// Controls the zoom by changing the camera position and look direction.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D.</param>
        /// <param name="e">The MouseWheelEventArgs.</param>
        private void GridEckeGrafik_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Zoom(perspectiveCameraEcke, cornerCameraAngle, e);
        }

        /// <summary>
        /// Rotates the 3D model on left or right mouse movement when the left button is hold.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D.</param>
        /// <param name="e">The MouseEventArgs.</param>
        private void GridEckeGrafik_MouseMove(object sender, MouseEventArgs e)
        {
            Rotate3DModel(cornerRotation, ref cornerCameraAngle, perspectiveCameraEcke, sender, e);
        }

        /// <summary>
        /// Converts the angle if the input is valid and shows the result.
        /// </summary>
        /// <param name="sender">The textbox for angle conversion.</param>
        /// <param name="e">The TextChangedEventArgs.</param>
        private void TextBoxEckeWinkelumrechnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            AngleConversion(textBoxEckeWinkelumrechnung, textBlockEckeWinkelumrechnung);
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
        private void ButtonEckeReset_Click(object sender, RoutedEventArgs e)
        {
            CornerResultReset();

            checkBoxEcke.IsChecked = false;

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

            modelVisual3dEcke.Content = new Model3DGroup();
            
            cornerFeedback.Deactivate(
                cornerFeedback.Calculated, 
                cornerFeedback.InputChanged, 
                cornerFeedback.AlphaCalculated, 
                cornerFeedback.AlphaChanged, 
                cornerFeedback.BetaCalculated, 
                cornerFeedback.BetaChanged, 
                cornerFeedback.InvalidValues, 
                cornerFeedback.LineXYInvalidValues, 
                cornerFeedback.LineXYToManyValues);

            cornerFeedback.Activate(cornerFeedback.EnterValues);
        }

        /// <summary>
        /// Calculates the compound miter and shows the results.
        /// </summary>
        /// <param name="sender">The button for the calculation.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void ButtonEckeBerechnung_Click(object sender, RoutedEventArgs e)
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
                modelVisual3dEcke.Content = new Model3DGroup();
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
                modelVisual3dEcke.Content = new Model3DGroup();
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
            
            corner.MiterJoint = checkBoxEcke.IsChecked.Value;

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

            corner.CreateModel(modelVisual3dEcke);

            cornerFeedback.Deactivate(cornerFeedback.EnterValues);
            cornerFeedback.Activate(cornerFeedback.Calculated);
        }

        /// <summary>
        /// Calculates the lines x and y.
        /// </summary>
        /// <param name="sender">The button to calculate the lines.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void ButtonEckeLiniexy_Click(object sender, RoutedEventArgs e)
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

                cornerFeedback.Activate(cornerFeedback.LineXYToManyValues);
            }

            if (CornerLineYSecond.Text != "" && CornerLineXSecond.Text != "" && linexySecondCalculated == false)
            {
                CornerLineYSecond.Background = Brushes.Red;
                CornerLineXSecond.Background = Brushes.Red;

                cornerFeedback.Activate(cornerFeedback.LineXYToManyValues);
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
        private void ButtonEckeWinkelAlpha_Click(object sender, RoutedEventArgs e)
        {
            cornerFeedback.Deactivate(cornerFeedback.AlphaChanged);

            double höhe = 0;
            double breitenversatzEins = 0;
            double breitenversatzZwei = 0;

            var inputTextBoxes = new TextBox[]
            {
                CornerOffsetFirst,
                CornerOffsetSecond
            };

            if (CornerHeight.Text != "" && (!InputValid(CornerHeight, ref höhe) || höhe <= 0))
                CornerHeight.Background = Brushes.Red;

            if (CornerOffsetFirst.Text != "" && (!InputValid(CornerOffsetFirst, ref breitenversatzEins)))
                CornerOffsetFirst.Background = Brushes.Red;

            if (CornerOffsetSecond.Text != "" && (!InputValid(CornerOffsetSecond, ref breitenversatzZwei)))
                CornerOffsetSecond.Background = Brushes.Red;

            if (CornerHeight.Background == Brushes.Red || ATextBoxIsRed(inputTextBoxes))
                cornerFeedback.Activate(cornerFeedback.InvalidValues);

            var x = false;

            if (CornerHeight.Background == Brushes.White && CornerHeight.Text != "" 
                && CornerOffsetFirst.Background == Brushes.White && CornerOffsetFirst.Text != "")
            {
                CornerAngleAlphaFirst.Text = Convert.ToString(Math.Round(Calc.Atan(breitenversatzEins / höhe), 4));

                x = true;
            }

            if (CornerHeight.Background == Brushes.White && CornerHeight.Text != "" 
                && CornerOffsetSecond.Background == Brushes.White && CornerOffsetSecond.Text != "")
            {
                CornerAngleAlphaSecond.Text = Convert.ToString(Math.Round(Calc.Atan(breitenversatzZwei / höhe), 4));

                x = true;
            }

            if (x)
                cornerFeedback.Activate(cornerFeedback.AlphaCalculated);
        }

        /// <summary>
        /// Calculates the angle beta.
        /// </summary>
        /// <param name="sender">The button to calculate angle beta.</param>
        /// <param name="e">The RoutedEventArgs.</param>
        private void ButtonEckeWinkelBeta_Click(object sender, RoutedEventArgs e)
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
        private void EckeInput_TextChanged(object sender, TextChangedEventArgs e)
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
                CornerOffsetFirst,
                CornerOffsetSecond,
                CornerNumberOfSides
            };

            if (!ATextBoxIsRed(helper))
            {
                cornerFeedback.Deactivate(cornerFeedback.InvalidValues);
            }

            helper = new TextBox[]
            {
                CornerHeight,
                CornerThicknessFirst,
                CornerThicknessSecond,
                CornerAngleAlphaFirst,
                CornerAngleAlphaSecond,
                CornerAngleBeta
            };

            foreach (var textbox in helper)
            {
                if (cornerFeedback.Calculated.Active && senderTextBox == textbox)
                {
                    cornerFeedback.Activate(cornerFeedback.InputChanged);
                    cornerFeedback.Deactivate(cornerFeedback.Calculated);
                }
            }

            helper = new TextBox[]
            {
                CornerOffsetFirst,
                CornerOffsetSecond,
                CornerAngleAlphaFirst,
                CornerAngleAlphaSecond,
                CornerHeight
            };

            foreach (var textbox in helper)
            {
                if (cornerFeedback.AlphaCalculated.Active && senderTextBox == textbox)
                {
                    cornerFeedback.Activate(cornerFeedback.AlphaChanged);
                    cornerFeedback.Deactivate(cornerFeedback.AlphaCalculated);
                }
            }

            helper = new TextBox[]
            {
                CornerNumberOfSides,
                CornerAngleBeta
            };

            foreach (var textbox in helper)
            {
                if (cornerFeedback.BetaCalculated.Active && senderTextBox == textbox)
                {
                    cornerFeedback.Activate(cornerFeedback.BetaChanged);
                    cornerFeedback.Deactivate(cornerFeedback.BetaCalculated);
                }
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
        private void CornerInputLinexy_TextChanged(object sender, TextChangedEventArgs e)
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
                cornerFeedback.Deactivate(cornerFeedback.LineXYToManyValues);
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

        #region Methoden Pyramide mit Grund- und Oberlinie
        
        /// <summary>
        /// Controls the zoom by changing camera position and look direction.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D.</param>
        /// <param name="e"></param>
        private void GridPyramideLinieGrafik_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Zoom(perspectiveCameraPyramideLinie, pyramidLineCameraAngle, e);
        }

        /// <summary>
        /// Rotates the 3D model when the mouse is moved and the left mouse button is hold.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D.</param>
        /// <param name="e"></param>
        private void GridPyramidLineGraphic_MouseMove(object sender, MouseEventArgs e)
        {
            Rotate3DModel(
                pyramidLineRotation, 
                ref pyramidLineCameraAngle, 
                perspectiveCameraPyramideLinie, 
                sender, 
                e);
        }

        /// <summary>
        /// Converts the angle if the input is valid and shows the result.
        /// </summary>
        /// <param name="sender">The textbox for angle conversion.</param>
        /// <param name="e"></param>
        private void TextBoxPyramideLinieWinkelumrechnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            AngleConversion(textBoxPyramideLinieWinkelumrechnung, textBlockPyramideLinieWinkelumrechnung);
        }

        /// <summary>
        /// Resets the result textblocks.
        /// </summary>
        private void PyramidLineResultReset()
        {
            var resultTextblocks = new TextBlock[]
            {
                textBlockPyramideLinieWinkelQueranschlag,
                textBlockPyramideLinieWinkelSaegeblatt,
                textBlockPyramideLinieBreite,
                textBlockPyramideLinieBreiteMitSchraege,
                textBlockPyramideLinieFlächenwinkel,
                textBlockPyramideLinieBreitenversatz,
                textBlockPyramideLinieSchraegeS,
                textBlockPyramideLinieNeigungswinkel,
                textBlockPyramideLinieInkreisradiusOA,
                textBlockPyramideLinieInkreisradiusOI,
                textBlockPyramideLinieInkreisradiusUA,
                textBlockPyramideLinieInkreisradiusUI,
                textBlockPyramideLinieUmkreisradiusOA,
                textBlockPyramideLinieUmkreisradiusOI,
                textBlockPyramideLinieUmkreisradiusUA,
                textBlockPyramideLinieUmkreisradiusUI,
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
        /// <param name="e"></param>
        private void ButtonPyramideLinieReset_Click(object sender, RoutedEventArgs e)
        {
            PyramidLineResultReset();

            var inputTextboxes = new TextBox[] { 
                textBoxPyramideLinieHoehe, 
                textBoxPyramideLinieStaerke, 
                textBoxPyramideLinieAnzahlSeiten,
                textBoxPyramideLinieGrundlinie, 
                textBoxPyramideLinieOberlinie 
            };

            foreach (var textbox in inputTextboxes)
            {
                textbox.Text = "";
                textbox.Background = Brushes.White;
            }

            modelVisual3dPyramideLinie.Content = new Model3DGroup();
            
            pyramidLineFeedback.Deactivate(
                pyramidLineFeedback.InvalidValues, 
                pyramidLineFeedback.InputChanged, 
                pyramidLineFeedback.Calculated);

            pyramidLineFeedback.Activate(pyramidLineFeedback.EnterValues);
        }

        /// <summary>
        /// Startet die Berechnung und weist den Ergebnisfeldern die Ergebnisse zu.
        /// </summary>
        /// <param name="sender">Der Button Berechnen.</param>
        /// <param name="e"></param>
        private void ButtonPyramideLinieBerechnung_Click(object sender, RoutedEventArgs e)
        {
            pyramidLineFeedback.Deactivate(pyramidLineFeedback.InputChanged);

            double höhe = 0;
            double materialstärke = 0;
            short anzahlSeiten = 0;
            double grundlinie = 0;
            double oberlinie = 0;

            if (ATextBoxIsEmpty(pyramideLinieEingaben, 0, 4))
            {
                PyramidLineResultReset();
                modelVisual3dPyramideLinie.Content = new Model3DGroup();
                pyramidLineFeedback.Deactivate(pyramidLineFeedback.Calculated);
                pyramidLineFeedback.Activate(pyramidLineFeedback.EnterValues);

                return;
            }

            if (!InputValid(textBoxPyramideLinieHoehe, ref höhe) 
                || höhe <= 0)
                textBoxPyramideLinieHoehe.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideLinieStaerke, ref materialstärke) 
                || materialstärke <= 0)
                textBoxPyramideLinieStaerke.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideLinieAnzahlSeiten, ref anzahlSeiten) 
                || anzahlSeiten < 3 
                || anzahlSeiten > 100)
                textBoxPyramideLinieAnzahlSeiten.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideLinieGrundlinie, ref grundlinie) 
                || grundlinie < 0)
                textBoxPyramideLinieGrundlinie.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideLinieOberlinie, ref oberlinie) 
                || oberlinie < 0)
                textBoxPyramideLinieOberlinie.Background = Brushes.Red;

            if (ATextBoxIsRed(pyramideLinieEingaben, 0, 4))
            {
                PyramidLineResultReset();
                modelVisual3dPyramideLinie.Content = new Model3DGroup();
                pyramidLineFeedback.Deactivate(pyramidLineFeedback.Calculated);
                pyramidLineFeedback.Activate(pyramidLineFeedback.InvalidValues);
                pyramidLineFeedback.Activate(pyramidLineFeedback.EnterValues);

                return;
            }

            pyramidLine.Height = höhe;
            pyramidLine.ThicknessFirstBoard = materialstärke;
            pyramidLine.ThicknessSecondBoard = materialstärke;
            pyramidLine.NumberOfSides = anzahlSeiten;
            pyramidLine.BottomSideLength = grundlinie;
            pyramidLine.TopSideLength = oberlinie;

            pyramidLine.AngleBeta = Math.Round(Convert.ToDouble((pyramidLine.NumberOfSides - 2.0) * 180.0 / pyramidLine.NumberOfSides), 4);

            double alpha = Calc.Atan(((pyramidLine.BottomSideLength / (2 * Calc.Tan(180.0 / pyramidLine.NumberOfSides))) -
                (pyramidLine.TopSideLength / (2 * Calc.Tan(180.0 / pyramidLine.NumberOfSides)))) / pyramidLine.Height);

            pyramidLine.AngleAlphaFirstBoard = alpha;
            pyramidLine.AngleAlphaSecondBoard = alpha;

            pyramidLine.MiterJoint = true;

            pyramidLine.Calculation();

            textBlockPyramideLinieWinkelQueranschlag.Text = Math.Round(pyramidLine.AngleCrossCutFirstBoard, 2) + "°";
            textBlockPyramideLinieWinkelSaegeblatt.Text = Math.Round(pyramidLine.AngleSawBladeTiltFirstBoard, 2) + "°";
            textBlockPyramideLinieBreite.Text = ErrorIfTooLarge(pyramidLine.WidthFirstBoard);
            textBlockPyramideLinieBreiteMitSchraege.Text = ErrorIfTooLarge(pyramidLine.WidthWithSlantFirstBoard);

            textBlockPyramideLinieFlächenwinkel.Text = Math.Round(pyramidLine.AngleDihedral, 2) + "°";

            double offset = Calc.Sin(pyramidLine.AngleAlphaFirstBoard) * pyramidLine.WidthFirstBoard;
            double slantS = pyramidLine.ThicknessFirstBoard / Calc.Cos(pyramidLine.AngleAlphaFirstBoard);

            textBlockPyramideLinieBreitenversatz.Text = ErrorIfTooLarge(offset);
            textBlockPyramideLinieSchraegeS.Text = ErrorIfTooLarge(slantS);

            textBlockPyramideLinieNeigungswinkel.Text = Math.Round(pyramidLine.AngleAlphaFirstBoard, 2) + " °";

            double inscribedTopOuter = Calc.InscribedCircleRadius(pyramidLine.TopSideLength, pyramidLine.NumberOfSides);
            double circumscribedTopOuter = Calc.CircumscribedCircleRadius(pyramidLine.TopSideLength, pyramidLine.NumberOfSides);
            double inscribedBottomOuter = Calc.InscribedCircleRadius(pyramidLine.BottomSideLength, pyramidLine.NumberOfSides);
            double circumscribedBottomOuter = Calc.CircumscribedCircleRadius(pyramidLine.BottomSideLength, pyramidLine.NumberOfSides);

            double inscribedTopInner = Calc.InscribedCircleRadius(pyramidLine.TopSideLength, pyramidLine.NumberOfSides) - slantS;
            double circumscribedTopInner = Calc.CircumscribedCircleRadius(pyramidLine.TopSideLength, pyramidLine.NumberOfSides) - slantS / Calc.Sin(pyramidLine.AngleBeta / 2.0);
            double inscribedBottomInner = Calc.InscribedCircleRadius(pyramidLine.BottomSideLength, pyramidLine.NumberOfSides) - slantS;
            double circumscribedBottomInner = Calc.CircumscribedCircleRadius(pyramidLine.BottomSideLength, pyramidLine.NumberOfSides) - slantS / Calc.Sin(pyramidLine.AngleBeta / 2.0);


            textBlockPyramideLinieInkreisradiusOA.Text = ErrorIfTooLarge(inscribedTopOuter);
            textBlockPyramideLinieUmkreisradiusOA.Text = ErrorIfTooLarge(circumscribedTopOuter);
            textBlockPyramideLinieInkreisradiusUA.Text = ErrorIfTooLarge(inscribedBottomOuter);
            textBlockPyramideLinieUmkreisradiusUA.Text = ErrorIfTooLarge(circumscribedBottomOuter);

            textBlockPyramideLinieInkreisradiusOI.Text = ErrorIfTooLarge(inscribedTopInner);
            textBlockPyramideLinieUmkreisradiusOI.Text = ErrorIfTooLarge(circumscribedTopInner);
            textBlockPyramideLinieInkreisradiusUI.Text = ErrorIfTooLarge(inscribedBottomInner);
            textBlockPyramideLinieUmkreisradiusUI.Text = ErrorIfTooLarge(circumscribedBottomInner);

            pyramidLine.CreateModel(modelVisual3dPyramideLinie);

            pyramidLineFeedback.Deactivate(pyramidLineFeedback.EnterValues);
            pyramidLineFeedback.Activate(pyramidLineFeedback.Calculated);
        }

        /// <summary>
        /// Stellt den Hintergrund eines veränderten Eingabefeldes weiß und aktualisiert die Feedbackleiste.
        /// </summary>
        /// <param name="sender">Das Eingabefeld das die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void PyramideLinieInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).Background = Brushes.White;

            if (!ATextBoxIsRed(pyramideLinieEingaben, 0, 4))
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
        /// <param name="e"></param>
        private void PyramidLineGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            pyramidLineColumnResize.ExpandMostRightIfBigger(sender, e);
        }

        /// <summary>
        /// Resizes the columns of the grid so only the one below the mouse is fully shown if the window gets smaller.
        /// </summary>
        /// <param name="sender">The grid to resize the columns in.</param>
        /// <param name="e"></param>
        private void PyramidLineGrid_MouseMove(object sender, MouseEventArgs e)
        {
            pyramidLineColumnResize.ShowFullyIfSmaller(sender, e);
        }

        #endregion

        #region Methoden Pyramide mit Neigungswinkel

        /// <summary>
        /// Controls the zoom by changing the camera position and lookdirection.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D</param>
        /// <param name="e"></param>
        private void GridPyramidAngle_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Zoom(perspectiveCameraPyramideWinkel, pyramidAngleCameraAngle, e);
        }

        /// <summary>
        /// Rotates the 3D model on mouse movement if the left button is hold.
        /// </summary>
        /// <param name="sender">The grid with the viewport3D.</param>
        /// <param name="e"></param>
        private void GridPyramidAngle_MouseMove(object sender, MouseEventArgs e)
        {
            Rotate3DModel(
                pyramidAngleRotation, 
                ref pyramidAngleCameraAngle, 
                perspectiveCameraPyramideWinkel, 
                sender, 
                e);
        }

        /// <summary>
        /// Converts the angle if the input is valid and shows the result.
        /// </summary>
        /// <param name="sender">The textbox for angle conversion.</param>
        /// <param name="e"></param>
        private void TextBoxPyramidAngleAngleConversion_TextChanged(object sender, TextChangedEventArgs e)
        {
            AngleConversion(textBoxPyramideWinkelWinkelumrechnung, textBlockPyramideWinkelWinkelumrechnung);
        }

        /// <summary>
        /// Resets the textboxes for the results.
        /// </summary>
        private void PyramidAngleResultReset()
        {
            var resultTextblocks = new TextBlock[]
            {
                textBlockPyramideWinkelWinkelQueranschlag,
                textBlockPyramideWinkelWinkelSaegeblatt,
                textBlockPyramideWinkelBreite,
                textBlockPyramideWinkelBreiteMitSchraege,
                textBlockPyramideWinkelFlächenwinkel,
                textBlockPyramideWinkelBreitenversatzErgebnis,
                textBlockPyramideWinkelSchraegeS,
                textBlockPyramideWinkelOberlinie,
                textBlockPyramideWinkelInkreisradiusOA,
                textBlockPyramideWinkelInkreisradiusOI,
                textBlockPyramideWinkelInkreisradiusUA,
                textBlockPyramideWinkelInkreisradiusUI,
                textBlockPyramideWinkelUmkreisradiusOA,
                textBlockPyramideWinkelUmkreisradiusOI,
                textBlockPyramideWinkelUmkreisradiusUA,
                textBlockPyramideWinkelUmkreisradiusUI,
            };

            foreach (var textBlock in resultTextblocks)
                textBlock.Text = "";
        }

        /// <summary>
        /// Resets everything.
        /// </summary>
        /// <param name="sender">The button to reset everything.</param>
        /// <param name="e"></param>
        private void ButtonPyramidAngleReset_Click(object sender, RoutedEventArgs e)
        {
            PyramidAngleResultReset();

            var inputTextboxes = new TextBox[] 
            {
                textBoxPyramideWinkelHoehe, 
                textBoxPyramideWinkelStaerke, 
                textBoxPyramideWinkelAnzahlSeiten,
                textBoxPyramideWinkelGrundlinie, 
                textBoxPyramideWinkelNeigungswinkel, 
                textBoxPyramideWinkelBreitenversatz 
            };

            foreach (var textbox in inputTextboxes)
            {
                textbox.Text = "";
                textbox.Background = Brushes.White;
            }

            modelVisual3dPyramideWinkel.Content = new Model3DGroup();

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
        /// Startet die Berechnung und weist den Ergebnisfeldern die Ergebnisse zu.
        /// </summary>
        /// <param name="sender">Der Button Berechnen.</param>
        /// <param name="e"></param>
        private void ButtonPyramideWinkelBerechnung_Click(object sender, RoutedEventArgs e)
        {
            pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.InputChanged);

            short anzahlSeiten = 0;
            double materialstärke = 0;
            double grundlinie = 0;
            double neigungswinkel = 0;
            double höhe = 0;
            double höheErgebend = 0;

            if (ATextBoxIsEmpty(pyramideWinkelEingaben, 1, 4))
            {
                PyramidAngleResultReset();
                modelVisual3dPyramideWinkel.Content = new Model3DGroup();
                pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.Calculated);
                pyramidAngleFeedback.Activate(pyramidAngleFeedback.EnterValues);

                return;
            }

            if (!InputValid(textBoxPyramideWinkelAnzahlSeiten, ref anzahlSeiten) 
                || anzahlSeiten < 3 || anzahlSeiten > 100)
                textBoxPyramideWinkelAnzahlSeiten.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideWinkelStaerke, ref materialstärke) 
                || materialstärke <= 0)
                textBoxPyramideWinkelStaerke.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideWinkelGrundlinie, ref grundlinie) 
                || grundlinie < 0)
                textBoxPyramideWinkelGrundlinie.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideWinkelNeigungswinkel, ref neigungswinkel) 
                || neigungswinkel < -90 || neigungswinkel > 90)
                textBoxPyramideWinkelNeigungswinkel.Background = Brushes.Red;

            if ((textBoxPyramideWinkelHoehe.Text != "") && (!InputValid(textBoxPyramideWinkelHoehe, ref höhe) 
                || höhe <= 0))
                textBoxPyramideWinkelHoehe.Background = Brushes.Red;

            if (ATextBoxIsRed(pyramideWinkelEingaben, 0, 4))
            {
                PyramidAngleResultReset();
                modelVisual3dPyramideWinkel.Content = new Model3DGroup();
                pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.Calculated);
                pyramidAngleFeedback.Activate(pyramidAngleFeedback.InvalidValues);
                pyramidAngleFeedback.Activate(pyramidAngleFeedback.EnterValues);
                
                return;
            }

            pyramidAngle.NumberOfSides = anzahlSeiten;
            pyramidAngle.ThicknessFirstBoard = materialstärke;
            pyramidAngle.ThicknessSecondBoard = materialstärke;
            pyramidAngle.BottomSideLength = grundlinie;
            pyramidAngle.AngleAlphaFirstBoard = neigungswinkel;
            pyramidAngle.AngleAlphaSecondBoard = neigungswinkel;

            höheErgebend = Calc.Tan(90.0 - pyramidAngle.AngleAlphaFirstBoard) * (pyramidAngle.BottomSideLength / (2 * Calc.Tan(180.0 /
                pyramidAngle.NumberOfSides)));

            if (pyramidAngle.AngleAlphaFirstBoard > 0)
            {
                if ((textBoxPyramideWinkelHoehe.Text != "") && höhe > höheErgebend)
                {
                    textBoxPyramideWinkelHoehe.Background = Brushes.Red;

                    pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.Calculated);
                    pyramidAngleFeedback.Activate(pyramidAngleFeedback.HeightLargerThanResulting);
                    pyramidAngleFeedback.Activate(pyramidAngleFeedback.EnterValues);
                }
                else
                {
                    pyramidAngle.Height = höhe;
                }

                if (textBoxPyramideWinkelHoehe.Text == "")
                {
                    textBoxPyramideWinkelHoehe.Text = Convert.ToString(Math.Round(höheErgebend - 0.01, 2));
                    pyramidAngle.Height = double.Parse(textBoxPyramideWinkelHoehe.Text);
                }
            }
            else
            {
                if (textBoxPyramideWinkelHoehe.Text == "")
                {
                    textBoxPyramideWinkelHoehe.Background = Brushes.Red;

                    pyramidAngleFeedback.Activate(pyramidAngleFeedback.HeightNeeded);
                    pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.Calculated);
                    pyramidAngleFeedback.Activate(pyramidAngleFeedback.EnterValues);
                }
                else
                {
                    pyramidAngle.Height = höhe;
                }
            }

            if (textBoxPyramideWinkelHoehe.Background == Brushes.Red)
            {
                PyramidAngleResultReset();
                modelVisual3dPyramideWinkel.Content = new Model3DGroup();

                return;
            }

            pyramidAngle.AngleBeta = Convert.ToDouble((Convert.ToDouble(pyramidAngle.NumberOfSides) - 2) * 180.0 / Convert.ToDouble(pyramidAngle.NumberOfSides));

            pyramidAngle.MiterJoint = true;

            pyramidAngle.Calculation();

            textBlockPyramideWinkelWinkelQueranschlag.Text = Math.Round(pyramidAngle.AngleCrossCutFirstBoard, 2) + "°";
            textBlockPyramideWinkelWinkelSaegeblatt.Text = Math.Round(pyramidAngle.AngleSawBladeTiltFirstBoard, 2) + "°";
            textBlockPyramideWinkelBreite.Text = ErrorIfTooLarge(pyramidAngle.WidthFirstBoard);
            textBlockPyramideWinkelBreiteMitSchraege.Text = ErrorIfTooLarge(pyramidAngle.WidthWithSlantFirstBoard);

            pyramidAngle.TopSideLength = ((pyramidAngle.BottomSideLength / (2 * Calc.Tan(180.0 / pyramidAngle.NumberOfSides))) -
                Calc.Sin(pyramidAngle.AngleAlphaFirstBoard) * pyramidAngle.WidthFirstBoard) * (2 * Calc.Tan(180.0 / pyramidAngle.NumberOfSides));

            textBlockPyramideWinkelFlächenwinkel.Text = Math.Round(pyramidAngle.AngleDihedral, 2) + "°";

            double slantS = pyramidAngle.ThicknessFirstBoard / Calc.Cos(pyramidAngle.AngleAlphaFirstBoard);
            double offset = Calc.Sin(pyramidAngle.AngleAlphaFirstBoard) * pyramidAngle.WidthFirstBoard;

            textBlockPyramideWinkelSchraegeS.Text = ErrorIfTooLarge(slantS);
            textBlockPyramideWinkelBreitenversatzErgebnis.Text = ErrorIfTooLarge(offset);
            textBlockPyramideWinkelOberlinie.Text = ErrorIfTooLarge(pyramidAngle.TopSideLength);

            double inscribedTopOuter = Calc.InscribedCircleRadius(pyramidAngle.TopSideLength, pyramidAngle.NumberOfSides);
            double circumscribedTopOuter = Calc.CircumscribedCircleRadius(pyramidAngle.TopSideLength, pyramidAngle.NumberOfSides);
            double inscribedBottomOuter = Calc.InscribedCircleRadius(pyramidAngle.BottomSideLength, pyramidAngle.NumberOfSides);
            double circumscribedBottomOuter = Calc.CircumscribedCircleRadius(pyramidAngle.BottomSideLength, pyramidAngle.NumberOfSides);
            
            double inscribedTopInner = Calc.InscribedCircleRadius(pyramidAngle.TopSideLength, pyramidAngle.NumberOfSides) - slantS;
            double circumscribedTopInner = Calc.CircumscribedCircleRadius(pyramidAngle.TopSideLength, pyramidAngle.NumberOfSides) - slantS / Calc.Sin(pyramidAngle.AngleBeta / 2.0);
            double inscribedBottomInner = Calc.InscribedCircleRadius(pyramidAngle.BottomSideLength, pyramidAngle.NumberOfSides) - slantS;
            double circumscribedBottomInner = Calc.CircumscribedCircleRadius(pyramidAngle.BottomSideLength, pyramidAngle.NumberOfSides) - slantS / Calc.Sin(pyramidAngle.AngleBeta / 2.0);

            textBlockPyramideWinkelInkreisradiusOA.Text = ErrorIfTooLarge(inscribedTopOuter);
            textBlockPyramideWinkelUmkreisradiusOA.Text = ErrorIfTooLarge(circumscribedTopOuter);
            textBlockPyramideWinkelInkreisradiusUA.Text = ErrorIfTooLarge(inscribedBottomOuter);
            textBlockPyramideWinkelUmkreisradiusUA.Text = ErrorIfTooLarge(circumscribedBottomOuter);

            textBlockPyramideWinkelInkreisradiusOI.Text = ErrorIfTooLarge(inscribedTopInner);
            textBlockPyramideWinkelUmkreisradiusOI.Text = ErrorIfTooLarge(circumscribedTopInner);
            textBlockPyramideWinkelInkreisradiusUI.Text = ErrorIfTooLarge(inscribedBottomInner);
            textBlockPyramideWinkelUmkreisradiusUI.Text = ErrorIfTooLarge(circumscribedBottomInner);


            pyramidAngle.CreateModel(modelVisual3dPyramideWinkel);

            pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.EnterValues);
            pyramidAngleFeedback.Activate(pyramidAngleFeedback.Calculated);
        }

        /// <summary>
        /// Calculates the tilt angle if input is valid.
        /// </summary>
        /// <param name="sender">The button to calculate the tilt angle.</param>
        /// <param name="e"></param>
        private void ButtonPyramidAngleTiltAngle_Click(object sender, RoutedEventArgs e)
        {
            pyramidAngleFeedback.Deactivate(pyramidAngleFeedback.TiltAngleChanged);

            double height = 0;
            double offset = 0;

            if (textBoxPyramideWinkelHoehe.Text == "" || textBoxPyramideWinkelBreitenversatz.Text == "")
                return;

            if (!InputValid(textBoxPyramideWinkelHoehe, ref height) || height <= 0)
                textBoxPyramideWinkelHoehe.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideWinkelBreitenversatz, ref offset))
                textBoxPyramideWinkelBreitenversatz.Background = Brushes.Red;

            if (textBoxPyramideWinkelHoehe.Background == Brushes.Red 
                || textBoxPyramideWinkelBreitenversatz.Background == Brushes.Red)
            {
                pyramidAngleFeedback.Activate(pyramidAngleFeedback.InvalidValues);
                return;
            }

            double tiltAngle = Math.Round(Calc.Atan(offset / height), 4);
            textBoxPyramideWinkelNeigungswinkel.Text = Convert.ToString(tiltAngle);

            pyramidAngleFeedback.Activate(pyramidAngleFeedback.TiltAngleCalculated);
        }

        /// <summary>
        /// Changes the textbox background to white and updates the feedback area.
        /// </summary>
        /// <param name="sender">The textbox that called the method.</param>
        /// <param name="e"></param>
        private void PyramidAngleInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var feedback = pyramidAngleFeedback;
            TextBox[] helper;

            senderTextBox.Background = Brushes.White;

            if (!ATextBoxIsRed(pyramideWinkelEingaben, 0, 5))
                feedback.Deactivate(feedback.InvalidValues);

            helper = new TextBox[] {
                textBoxPyramideWinkelHoehe,
                textBoxPyramideWinkelStaerke,
                textBoxPyramideWinkelAnzahlSeiten,
                textBoxPyramideWinkelGrundlinie
            };

            if (feedback.Calculated.Active && (helper.Contains(senderTextBox)))
            {
                feedback.Activate(feedback.InputChanged);
                feedback.Deactivate(feedback.Calculated);
            }

            helper = new TextBox[] {
                textBoxPyramideWinkelHoehe,
                textBoxPyramideWinkelBreitenversatz,
                textBoxPyramideWinkelNeigungswinkel
            };

            if (feedback.TiltAngleCalculated.Active && (helper.Contains(senderTextBox)))
            {
                feedback.Activate(feedback.TiltAngleChanged);
                feedback.Deactivate(feedback.TiltAngleCalculated);
            }

            helper = new TextBox[] {
                textBoxPyramideWinkelHoehe,
                textBoxPyramideWinkelNeigungswinkel
            };

            if (helper.Contains(senderTextBox))
            {
                textBoxPyramideWinkelHoehe.Background = Brushes.White;

                feedback.Deactivate(feedback.HeightNeeded);
                feedback.Deactivate(feedback.HeightLargerThanResulting);
            }
        }

        /// <summary>
        /// Resizes the columns of the grid so only the most right one gets bigger if the window gets bigger.
        /// </summary>
        /// <param name="sender">The grid to resize the columns in.</param>
        /// <param name="e"></param>
        private void PyramidAngleGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            pyramidAngleColumnResize.ExpandMostRightIfBigger(sender, e);
        }

        /// <summary>
        /// Resizes the columns of the grid so only the one below the mouse is fully shown if the window gets smaller.
        /// </summary>
        /// <param name="sender">The grid to resize the columns in.</param>
        /// <param name="e"></param>
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
        /// <param name="e"></param>
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
        /// <param name="e"></param>
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
        /// <param name="start">The start index in the array.</param>
        /// <param name="end">The end index in the array (included).</param>
        /// <returns>True if the background of at least one textbox is red.</returns>
        private bool ATextBoxIsRed(TextBox[] textBoxes, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                if (textBoxes[i].Background == Brushes.Red)
                    return true;
            }
            return false;
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
        /// <param name="start">The start index in the array.</param>
        /// <param name="end">The end index in the array (included).</param>
        /// <returns>True if at least one textbox is empty.</returns>
        private bool ATextBoxIsEmpty(TextBox[] textBoxes, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                if (textBoxes[i].Text == "")
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
            if (number < 10000 && number > -10000)
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
