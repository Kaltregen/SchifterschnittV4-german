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

            eckeEingaben = new TextBox[] { 
                textBoxEckeHoehe, 
                textBoxEckeMaterialstaerkeEins, 
                textBoxEckeMaterialstaerkeZwei, 
                textBoxEckeWinkelAlphaEins,
                textBoxEckeWinkelAlphaZwei, 
                textBoxEckeWinkelBeta, 
                textBoxEckeBreitenversatzEins, 
                textBoxEckeBreitenversatzZwei, 
                textBoxEckeAnzahlSeiten,
                CornerLineYFirst, 
                CornerLineYSecond, 
                CornerLineXFirst, 
                CornerLineXSecond 
            };

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
        /// <param name="sender">Das Eingabefeld der Winkelumrechnung.</param>
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
                textBlockEckeWinkelQueranschlagEins,
                textBlockEckeWinkelQueranschlagZwei,
                textBlockEckeWinkelSaegeblattEins,
                textBlockEckeWinkelSaegeblattZwei,
                textBlockEckeBreiteEins,
                textBlockEckeBreiteZwei,
                textBlockEckeBreiteMitSchraegeEins,
                textBlockEckeBreiteMitSchraegeZwei,
                textBlockEckeFlächenwinkel,
                textBlockEckeBreitenversatzEinsErgebnis,
                textBlockEckeBreitenversatzZweiErgebnis,
                textBlockEckeSchraegeSEins,
                textBlockEckeSchraegeSZwei,
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

            for (int i = 0; i < eckeEingaben.Length; i++)
            {
                eckeEingaben[i].Text = "";
                eckeEingaben[i].Background = Brushes.White;
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

            cornerFeedback.Deactivate(cornerFeedback.InputChanged);

            double höhe = 0;
            double materialstärkeEins = 0;
            double materialstärkeZwei = 0;
            double winkelAlphaEins = 0;
            double winkelAlphaZwei = 0;
            double winkelBeta = 0;

            if (ATextBoxIsEmpty(eckeEingaben, 0, 5))
            {
                CornerResultReset();
                modelVisual3dEcke.Content = new Model3DGroup();
                cornerFeedback.Deactivate(cornerFeedback.Calculated);
                cornerFeedback.Activate(cornerFeedback.EnterValues);

                return;
            }

            if (!InputValid(textBoxEckeHoehe, ref höhe) 
                || höhe <= 0)
                textBoxEckeHoehe.Background = Brushes.Red;

            if (!InputValid(textBoxEckeMaterialstaerkeEins, ref materialstärkeEins) 
                || materialstärkeEins <= 0)
                textBoxEckeMaterialstaerkeEins.Background = Brushes.Red;

            if (!InputValid(textBoxEckeMaterialstaerkeZwei, ref materialstärkeZwei) 
                || materialstärkeZwei <= 0)
                textBoxEckeMaterialstaerkeZwei.Background = Brushes.Red;

            if (!InputValid(textBoxEckeWinkelAlphaEins, ref winkelAlphaEins) 
                || winkelAlphaEins < -90 || winkelAlphaEins > 90)
                textBoxEckeWinkelAlphaEins.Background = Brushes.Red;

            if (!InputValid(textBoxEckeWinkelAlphaZwei, ref winkelAlphaZwei) 
                || winkelAlphaZwei < -90 || winkelAlphaZwei > 90)
                textBoxEckeWinkelAlphaZwei.Background = Brushes.Red;

            if (!InputValid(textBoxEckeWinkelBeta, ref winkelBeta) 
                || winkelBeta <= 0 || winkelBeta >= 180)
                textBoxEckeWinkelBeta.Background = Brushes.Red;
            
            if (ATextBoxIsRed(eckeEingaben, 0, 5))
            {
                CornerResultReset();
                modelVisual3dEcke.Content = new Model3DGroup();
                cornerFeedback.Deactivate(cornerFeedback.Calculated);
                cornerFeedback.Activate(cornerFeedback.InvalidValues);
                cornerFeedback.Activate(cornerFeedback.EnterValues);

                return;
            }

            corner.Height = höhe;
            corner.ThicknessFirstBoard = materialstärkeEins;
            corner.ThicknessSecondBoard = materialstärkeZwei;
            corner.AngleAlphaFirstBoard = winkelAlphaEins;
            corner.AngleAlphaSecondBoard = winkelAlphaZwei;
            corner.AngleBeta = winkelBeta;
            
            corner.MiterJoint = checkBoxEcke.IsChecked.Value;

            corner.Calculation();

            textBlockEckeWinkelQueranschlagEins.Text = Math.Round(corner.AngleCrossCutFirstBoard, 2) + "°";
            textBlockEckeWinkelQueranschlagZwei.Text = Math.Round(corner.AngleCrossCutSecondBoard, 2) + "°";
            textBlockEckeWinkelSaegeblattEins.Text = Math.Round(corner.AngleSawBladeTiltFirstBoard, 2) + "°";
            textBlockEckeWinkelSaegeblattZwei.Text = Math.Round(corner.AngleSawBladeTiltSecondBoard, 2) + "°";
            textBlockEckeBreiteEins.Text = corner.AngleAlphaFirstBoard == 90 || corner.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(corner.WidthFirstBoard, 2).ToString() + " mm";
            textBlockEckeBreiteZwei.Text = corner.AngleAlphaSecondBoard == 90 || corner.AngleAlphaSecondBoard == -90 ? "Error" : Math.Round(corner.WidthSecondBoard, 2).ToString() + " mm";
            textBlockEckeBreiteMitSchraegeEins.Text = corner.AngleAlphaFirstBoard == 90 || corner.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(corner.WidthWithSlantFirstBoard, 2) + " mm";
            textBlockEckeBreiteMitSchraegeZwei.Text = corner.AngleAlphaSecondBoard == 90 || corner.AngleAlphaSecondBoard == -90 ? "Error" : Math.Round(corner.WidhtWithSlantSecondBoard, 2) + " mm";
            textBlockEckeFlächenwinkel.Text = Math.Round(corner.AngleDihedral, 2) + "°";

            if (corner.AngleAlphaFirstBoard == 90 || corner.AngleAlphaSecondBoard == 90 || corner.AngleAlphaFirstBoard == -90 || corner.AngleAlphaSecondBoard == -90)
            {
                textBlockEckeBreitenversatzEinsErgebnis.Text = "Error";
                textBlockEckeBreitenversatzZweiErgebnis.Text = "Error";
                textBlockEckeSchraegeSEins.Text = "Error";
                textBlockEckeSchraegeSZwei.Text = "Error";
            }
            else
            {
                textBlockEckeBreitenversatzEinsErgebnis.Text = Convert.ToString(Math.Round(Calc.Sin(corner.AngleAlphaFirstBoard) * corner.WidthFirstBoard, 2)) + " mm";
                textBlockEckeBreitenversatzZweiErgebnis.Text = Convert.ToString(Math.Round(Calc.Sin(corner.AngleAlphaSecondBoard) * corner.WidthSecondBoard, 2)) + " mm";
                textBlockEckeSchraegeSEins.Text = Convert.ToString(Math.Round(corner.ThicknessFirstBoard / Calc.Cos(corner.AngleAlphaFirstBoard), 2)) + " mm";
                textBlockEckeSchraegeSZwei.Text = Convert.ToString(Math.Round(corner.ThicknessSecondBoard / Calc.Cos(corner.AngleAlphaSecondBoard), 2)) + " mm";
            }

            corner.CreateModel(modelVisual3dEcke);

            cornerFeedback.Deactivate(cornerFeedback.EnterValues);
            cornerFeedback.Activate(cornerFeedback.Calculated);
        }

        /// <summary>
        /// Berechnet die Linien XY.
        /// </summary>
        /// <param name="sender">Der Button Linie XY Berechnen.</param>
        /// <param name="e"></param>
        private void ButtonEckeLiniexy_Click(object sender, RoutedEventArgs e)
        {
            double LinieYEins = 0;
            double LinieYZwei = 0;
            double LinieXEins = 0;
            double LinieXZwei = 0;

            for (int i = 9; i < 13; i++)
                eckeEingaben[i].Background = Brushes.White;

            if (CornerLineYFirst.Text != "" && !InputValid(CornerLineYFirst, ref LinieYEins))
                CornerLineYFirst.Background = Brushes.Red;

            if (CornerLineYSecond.Text != "" && !InputValid(CornerLineYSecond, ref LinieYZwei))
                CornerLineYSecond.Background = Brushes.Red;

            if (CornerLineXFirst.Text != "" && !InputValid(CornerLineXFirst, ref LinieXEins))
                CornerLineXFirst.Background = Brushes.Red;

            if (CornerLineXSecond.Text != "" && !InputValid(CornerLineXSecond, ref LinieXZwei))
                CornerLineXSecond.Background = Brushes.Red;

            if (ATextBoxIsRed(eckeEingaben, 9, 12))
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
            
            if (cornerFeedback.Calculated.Active && !ATextBoxIsRed(eckeEingaben, 9, 12))
            {
                double zusatzEins = Calc.Tan(corner.AngleCrossCutFirstBoard) * corner.WidthFirstBoard;
                double zusatzZwei = Calc.Tan(corner.AngleCrossCutSecondBoard) * corner.WidthSecondBoard;

                if (CornerLineYFirst.Text != "" && linexyFirstCalculated == false)
                {
                    CornerLineXFirst.Text = Convert.ToString(Math.Round(LinieYEins + (2 * zusatzEins), 2));
                    linexyFirstCalculated = true;
                }
                else if (CornerLineXFirst.Text != "" && linexyFirstCalculated == false)
                {
                    CornerLineYFirst.Text = Convert.ToString(Math.Round(LinieXEins - (2 * zusatzEins), 2));
                    linexyFirstCalculated = true;
                }

                if (CornerLineYSecond.Text != "" && linexySecondCalculated == false)
                {
                    CornerLineXSecond.Text = Convert.ToString(Math.Round(LinieYZwei + (2 * zusatzZwei), 2));
                    linexySecondCalculated = true;
                }
                else if (CornerLineXSecond.Text != "" && linexySecondCalculated == false)
                {
                    CornerLineYSecond.Text = Convert.ToString(Math.Round(LinieXZwei - (2 * zusatzZwei), 2));
                    linexySecondCalculated = true;
                }
            }
        }

        /// <summary>
        /// Berechnet den Winkel Alpha.
        /// </summary>
        /// <param name="sender">Der Button Winkel Alpha Berechnen.</param>
        /// <param name="e"></param>
        private void ButtonEckeWinkelAlpha_Click(object sender, RoutedEventArgs e)
        {
            cornerFeedback.Deactivate(cornerFeedback.AlphaChanged);

            double höhe = 0;
            double breitenversatzEins = 0;
            double breitenversatzZwei = 0;

            if (textBoxEckeHoehe.Text != "" && (!InputValid(textBoxEckeHoehe, ref höhe) || höhe <= 0))
                textBoxEckeHoehe.Background = Brushes.Red;

            if (textBoxEckeBreitenversatzEins.Text != "" && (!InputValid(textBoxEckeBreitenversatzEins, ref breitenversatzEins)))
                textBoxEckeBreitenversatzEins.Background = Brushes.Red;

            if (textBoxEckeBreitenversatzZwei.Text != "" && (!InputValid(textBoxEckeBreitenversatzZwei, ref breitenversatzZwei)))
                textBoxEckeBreitenversatzZwei.Background = Brushes.Red;

            if (textBoxEckeHoehe.Background == Brushes.Red || ATextBoxIsRed(eckeEingaben, 6, 7))
                cornerFeedback.Activate(cornerFeedback.InvalidValues);

            var x = false;

            if (textBoxEckeHoehe.Background == Brushes.White && textBoxEckeHoehe.Text != "" && textBoxEckeBreitenversatzEins.Background == Brushes.White && textBoxEckeBreitenversatzEins.Text != "")
            {
                textBoxEckeWinkelAlphaEins.Text = Convert.ToString(Math.Round(Calc.Atan(breitenversatzEins / höhe), 4));

                x = true;
            }

            if (textBoxEckeHoehe.Background == Brushes.White && textBoxEckeHoehe.Text != "" && textBoxEckeBreitenversatzZwei.Background == Brushes.White && textBoxEckeBreitenversatzZwei.Text != "")
            {
                textBoxEckeWinkelAlphaZwei.Text = Convert.ToString(Math.Round(Calc.Atan(breitenversatzZwei / höhe), 4));

                x = true;
            }

            if (x)
                cornerFeedback.Activate(cornerFeedback.AlphaCalculated);
        }

        /// <summary>
        /// Berechnet den Winkel Beta.
        /// </summary>
        /// <param name="sender">Der Button Winkel Beta Berechnen.</param>
        /// <param name="e"></param>
        private void ButtonEckeWinkelBeta_Click(object sender, RoutedEventArgs e)
        {
            cornerFeedback.Deactivate(cornerFeedback.BetaChanged);

            short anzahlSeiten = 0;

            if (textBoxEckeAnzahlSeiten.Text == "")
                return;

            if (!InputValid(textBoxEckeAnzahlSeiten, ref anzahlSeiten) || anzahlSeiten < 3 || anzahlSeiten > 100)
            {
                textBoxEckeAnzahlSeiten.Background = Brushes.Red;
                cornerFeedback.Activate(cornerFeedback.InvalidValues);

                return;
            }

            textBoxEckeWinkelBeta.Text = Convert.ToString(Math.Round(Convert.ToDouble((anzahlSeiten - 2.0) * 180.0 / anzahlSeiten), 4));

            cornerFeedback.Activate(cornerFeedback.BetaCalculated);
        }

        /// <summary>
        /// Stellt den Hintergrund eines veränderten Eingabefeldes weiß und aktualisiert die Feedbackleiste.
        /// </summary>
        /// <param name="sender">Das Eingabefeld das die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void EckeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).Background = Brushes.White;

            if (!ATextBoxIsRed(eckeEingaben, 0, 8))
            {
                cornerFeedback.Deactivate(cornerFeedback.InvalidValues);
            }

            for (int i = 0; i <= 5; i++)
            {
                if (cornerFeedback.Calculated.Active && (TextBox)sender == eckeEingaben[i])
                {
                    cornerFeedback.Activate(cornerFeedback.InputChanged);
                    cornerFeedback.Deactivate(cornerFeedback.Calculated);
                }
            }

            for (int i = 6; i <= 7; i++)
            {
                if (cornerFeedback.AlphaCalculated.Active && ((TextBox)sender == eckeEingaben[i] || (TextBox)sender == textBoxEckeWinkelAlphaEins || 
                    (TextBox)sender == textBoxEckeWinkelAlphaZwei || (TextBox)sender == textBoxEckeHoehe))
                {
                    cornerFeedback.Activate(cornerFeedback.AlphaChanged);
                    cornerFeedback.Deactivate(cornerFeedback.AlphaCalculated);
                }
            }

            if (cornerFeedback.BetaCalculated.Active && ((TextBox)sender == textBoxEckeAnzahlSeiten || (TextBox)sender == textBoxEckeWinkelBeta))
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
            double x = 0;

            if (InputValid(textBox, ref x) && x >= 0)
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

            double schrägeS = pyramidLine.ThicknessFirstBoard / Calc.Cos(pyramidLine.AngleAlphaFirstBoard);

            textBlockPyramideLinieWinkelQueranschlag.Text = Math.Round(pyramidLine.AngleCrossCutFirstBoard, 2) + "°";
            textBlockPyramideLinieWinkelSaegeblatt.Text = Math.Round(pyramidLine.AngleSawBladeTiltFirstBoard, 2) + "°";
            textBlockPyramideLinieBreite.Text = pyramidLine.AngleAlphaFirstBoard == 90 || pyramidLine.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(pyramidLine.WidthFirstBoard, 2).ToString() + " mm";
            textBlockPyramideLinieBreiteMitSchraege.Text = pyramidLine.AngleAlphaFirstBoard == 90 || pyramidLine.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(pyramidLine.WidthWithSlantFirstBoard, 2) + " mm";

            textBlockPyramideLinieFlächenwinkel.Text = Math.Round(pyramidLine.AngleDihedral, 2) + "°";
            textBlockPyramideLinieBreitenversatz.Text = Convert.ToString(Math.Round(Calc.Sin(pyramidLine.AngleAlphaFirstBoard) * pyramidLine.WidthFirstBoard, 2)) + " mm";
            textBlockPyramideLinieSchraegeS.Text = Convert.ToString(Math.Round(schrägeS, 2)) + " mm";
            textBlockPyramideLinieNeigungswinkel.Text = Convert.ToString(Math.Round(pyramidLine.AngleAlphaFirstBoard, 2)) + " °";
            textBlockPyramideLinieInkreisradiusOA.Text = Convert.ToString(Math.Round(Calc.InscribedCircleRadius(pyramidLine.TopSideLength, pyramidLine.NumberOfSides), 2)) + " mm";
            textBlockPyramideLinieInkreisradiusOI.Text = Convert.ToString(Math.Round(Calc.InscribedCircleRadius(pyramidLine.TopSideLength, pyramidLine.NumberOfSides) - schrägeS, 2)) + " mm";
            textBlockPyramideLinieInkreisradiusUA.Text = Convert.ToString(Math.Round(Calc.InscribedCircleRadius(pyramidLine.BottomSideLength, pyramidLine.NumberOfSides), 2)) + " mm";
            textBlockPyramideLinieInkreisradiusUI.Text = Convert.ToString(Math.Round(Calc.InscribedCircleRadius(pyramidLine.BottomSideLength, pyramidLine.NumberOfSides) - schrägeS, 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusOA.Text = Convert.ToString(Math.Round(Calc.CircumscribedCircleRadius(pyramidLine.TopSideLength, pyramidLine.NumberOfSides), 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusOI.Text = Convert.ToString(Math.Round(Calc.CircumscribedCircleRadius(pyramidLine.TopSideLength, pyramidLine.NumberOfSides) - schrägeS /
                Calc.Sin(pyramidLine.AngleBeta / 2.0), 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusUA.Text = Convert.ToString(Math.Round(Calc.CircumscribedCircleRadius(pyramidLine.BottomSideLength, pyramidLine.NumberOfSides), 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusUI.Text = Convert.ToString(Math.Round(Calc.CircumscribedCircleRadius(pyramidLine.BottomSideLength, pyramidLine.NumberOfSides) - schrägeS /
                Calc.Sin(pyramidLine.AngleBeta / 2.0), 2)) + " mm";

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

            double schrägeS = pyramidAngle.ThicknessFirstBoard / Calc.Cos(pyramidAngle.AngleAlphaFirstBoard);

            textBlockPyramideWinkelWinkelQueranschlag.Text = Math.Round(pyramidAngle.AngleCrossCutFirstBoard, 2) + "°";
            textBlockPyramideWinkelWinkelSaegeblatt.Text = Math.Round(pyramidAngle.AngleSawBladeTiltFirstBoard, 2) + "°";
            textBlockPyramideWinkelBreite.Text = pyramidAngle.AngleAlphaFirstBoard == 90 || pyramidAngle.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(pyramidAngle.WidthFirstBoard, 2).ToString() + " mm";
            textBlockPyramideWinkelBreiteMitSchraege.Text = pyramidAngle.AngleAlphaFirstBoard == 90 || pyramidAngle.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(pyramidAngle.WidthWithSlantFirstBoard, 2) + " mm";

            pyramidAngle.TopSideLength = ((pyramidAngle.BottomSideLength / (2 * Calc.Tan(180.0 / pyramidAngle.NumberOfSides))) -
                Calc.Sin(pyramidAngle.AngleAlphaFirstBoard) * pyramidAngle.WidthFirstBoard) * (2 * Calc.Tan(180.0 / pyramidAngle.NumberOfSides));

            textBlockPyramideWinkelFlächenwinkel.Text = Math.Round(pyramidAngle.AngleDihedral, 2) + "°";
            textBlockPyramideWinkelInkreisradiusUA.Text = Convert.ToString(Math.Round(Calc.InscribedCircleRadius(pyramidAngle.BottomSideLength, pyramidAngle.NumberOfSides), 2)) + " mm";
            textBlockPyramideWinkelUmkreisradiusUA.Text = Convert.ToString(Math.Round(Calc.CircumscribedCircleRadius(pyramidAngle.BottomSideLength, pyramidAngle.NumberOfSides), 2)) + " mm";

            if (pyramidAngle.AngleAlphaFirstBoard == 90 || pyramidAngle.AngleAlphaFirstBoard == -90)
            {
                textBlockPyramideWinkelBreitenversatzErgebnis.Text = "Error";
                textBlockPyramideWinkelSchraegeS.Text = "Error";
                textBlockPyramideWinkelOberlinie.Text = "Error";
                textBlockPyramideWinkelUmkreisradiusOA.Text = "Error";
                textBlockPyramideWinkelInkreisradiusOA.Text = "Error";
                textBlockPyramideWinkelInkreisradiusOI.Text = "Error";
                textBlockPyramideWinkelInkreisradiusUI.Text = "Error";
                textBlockPyramideWinkelUmkreisradiusOI.Text = "Error";
                textBlockPyramideWinkelUmkreisradiusUI.Text = "Error";
            }
            else
            {
                textBlockPyramideWinkelSchraegeS.Text = Convert.ToString(Math.Round(schrägeS, 2)) + " mm";
                textBlockPyramideWinkelBreitenversatzErgebnis.Text = Convert.ToString(Math.Round(Calc.Sin(pyramidAngle.AngleAlphaFirstBoard) * pyramidAngle.WidthFirstBoard, 2)) + " mm";
                textBlockPyramideWinkelOberlinie.Text = Convert.ToString(Math.Round(pyramidAngle.TopSideLength, 2)) + " mm";
                textBlockPyramideWinkelUmkreisradiusOA.Text = Convert.ToString(Math.Round(Calc.CircumscribedCircleRadius(pyramidAngle.TopSideLength, pyramidAngle.NumberOfSides), 2)) + " mm";
                textBlockPyramideWinkelInkreisradiusOA.Text = Convert.ToString(Math.Round(Calc.InscribedCircleRadius(pyramidAngle.TopSideLength, pyramidAngle.NumberOfSides), 2)) + " mm";
                textBlockPyramideWinkelInkreisradiusOI.Text = Convert.ToString(Math.Round(Calc.InscribedCircleRadius(pyramidAngle.TopSideLength, pyramidAngle.NumberOfSides) - schrägeS, 2)) + " mm";
                textBlockPyramideWinkelInkreisradiusUI.Text = Convert.ToString(Math.Round(Calc.InscribedCircleRadius(pyramidAngle.BottomSideLength, pyramidAngle.NumberOfSides) - schrägeS, 2)) + " mm";
                textBlockPyramideWinkelUmkreisradiusOI.Text = Convert.ToString(Math.Round(Calc.CircumscribedCircleRadius(pyramidAngle.TopSideLength, pyramidAngle.NumberOfSides) - schrägeS / Calc.Sin(pyramidAngle.AngleBeta / 2.0), 2)) + " mm";
                textBlockPyramideWinkelUmkreisradiusUI.Text = Convert.ToString(Math.Round(Calc.CircumscribedCircleRadius(pyramidAngle.BottomSideLength, pyramidAngle.NumberOfSides) - schrägeS / Calc.Sin(pyramidAngle.AngleBeta / 2.0), 2)) + " mm";
            }

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
