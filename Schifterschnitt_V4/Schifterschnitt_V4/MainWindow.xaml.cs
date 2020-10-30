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
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        Corner ecke = new Corner();
        Pyramid pyramideLinie = new Pyramid();
        Pyramid pyramideWinkel = new Pyramid();

        // Variablen die anzeigen ob bei der Linie XY ein Teil berechnet ist.
        bool liniexyTeilEinsBerechnet = false;
        bool liniexyTeilZweiBerechnet = false;

        // Rotationen erstellen.
        AxisAngleRotation3D eckeRotation;
        AxisAngleRotation3D pyramideLinieRotation;
        AxisAngleRotation3D pyramideWinkelRotation;

        // Einen Punkt für die Position der Maus auf der Grafik anlegen.
        Point mousePosition = new Point(0, 0);

        // Die Winkel für die Kamera in der Grafik anlegen.
        double eckeKameraWinkel = new double();
        double pyramideLinieKameraWinkel = new double();
        double pyramideWinkelKameraWinkel = new double();

        // Die Feedbackleisten erstellen.
        FeedbackArea eckeFeedback;
        FeedbackArea pyramideLinieFeedback;
        FeedbackArea pyramideWinkelFeedback;

        // Arrays für die Eingabefelder erstellen.
        TextBox[] eckeEingaben;
        TextBox[] pyramideLinieEingaben;
        TextBox[] pyramideWinkelEingaben;

        GridColumnResize cornerColumnResize;
        GridColumnResize pyramidLineColumnResize;
        GridColumnResize pyramidAngleColumnResize;

        #endregion

        #region ctor

        /// <summary>
        /// ctor
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

            // Neue Transformationen für die Grafiken erzeugen.
            eckeRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 1);
            RotateTransform3D eckeTransformation = new RotateTransform3D(eckeRotation);
            modelVisual3dEcke.Transform = eckeTransformation;

            pyramideLinieRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 1);
            RotateTransform3D pyramideLinieTransformation = new RotateTransform3D(pyramideLinieRotation);
            modelVisual3dPyramideLinie.Transform = pyramideLinieTransformation;

            pyramideWinkelRotation = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 1);
            RotateTransform3D pyramideWinkelTransformation = new RotateTransform3D(pyramideWinkelRotation);
            modelVisual3dPyramideWinkel.Transform = pyramideWinkelTransformation;
            
            // Die Winkel für die Kamera in der Grafik berechnen.
            eckeKameraWinkel = Calculate.RadianToDegree(Math.Atan(5 / Math.Sqrt(Math.Pow(5, 2) + Math.Pow(5, 2))));
            pyramideLinieKameraWinkel = Calculate.RadianToDegree(Math.Atan(5 / Math.Sqrt(Math.Pow(5, 2) + Math.Pow(5, 2))));
            pyramideWinkelKameraWinkel = Calculate.RadianToDegree(Math.Atan(5 / Math.Sqrt(Math.Pow(5, 2) + Math.Pow(5, 2))));
            
            // Die Feedbackleisten initialisieren und den Grids im Fenster zuweisen.
            eckeFeedback = new FeedbackArea(gridEckeFeedback);
            pyramideLinieFeedback = new FeedbackArea(gridPyramideLinieFeedback);
            pyramideWinkelFeedback = new FeedbackArea(gridPyramideWinkelFeedback);

            // Die Arrays für die Eingabefelder füllen.
            eckeEingaben = new TextBox[] { textBoxEckeHoehe, textBoxEckeMaterialstaerkeEins, textBoxEckeMaterialstaerkeZwei, textBoxEckeWinkelAlphaEins,
                textBoxEckeWinkelAlphaZwei, textBoxEckeWinkelBeta, textBoxEckeBreitenversatzEins, textBoxEckeBreitenversatzZwei, textBoxEckeAnzahlSeiten,
                textBoxEckeLinieYEins, textBoxEckeLinieYZwei, textBoxEckeLinieXEins, textBoxEckeLinieXZwei };

            pyramideLinieEingaben = new TextBox[] { textBoxPyramideLinieHoehe, textBoxPyramideLinieStaerke, textBoxPyramideLinieAnzahlSeiten,
                textBoxPyramideLinieGrundlinie, textBoxPyramideLinieOberlinie };

            pyramideWinkelEingaben = new TextBox[] { textBoxPyramideWinkelHoehe, textBoxPyramideWinkelStaerke, textBoxPyramideWinkelAnzahlSeiten,
                textBoxPyramideWinkelGrundlinie, textBoxPyramideWinkelNeigungswinkel, textBoxPyramideWinkelBreitenversatz };
            
            // Für alle Tabs die Meldung Eingabewerte eingeben aktivieren.
            eckeFeedback.Activate(eckeFeedback.EnterValues);
            pyramideLinieFeedback.Activate(pyramideLinieFeedback.EnterValues);
            pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.EnterValues);
        }

        #endregion

        #region Methoden Ecke
        
        /// <summary>
        /// Steuert den Zoom durch verändern der Kameraposition und Blickrichtung beim Scrollen über der Grafik.
        /// </summary>
        /// <param name="sender">Das Grid in der die Grafik ist.</param>
        /// <param name="e"></param>
        private void GridEckeGrafik_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Viewport3D_MouseWheel(perspectiveCameraEcke, eckeKameraWinkel, e);
        }

        /// <summary>
        /// Dreht das 3D-Modell bei Bewegung der Maus nach links oder rechts.
        /// </summary>
        /// <param name="sender">Das Grid in der die Grafik ist.</param>
        /// <param name="e"></param>
        private void GridEckeGrafik_MouseMove(object sender, MouseEventArgs e)
        {
            Grid_MouseMove(eckeRotation, ref eckeKameraWinkel, perspectiveCameraEcke, sender, e);
        }

        /// <summary>
        /// Rechnet den Winkel um und zeigt das Ergebnis im Ergebnisfeld an.
        /// </summary>
        /// <param name="sender">Das Eingabefeld der Winkelumrechnung.</param>
        /// <param name="e"></param>
        private void TextBoxEckeWinkelumrechnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            AngleConversion(textBoxEckeWinkelumrechnung, textBlockEckeWinkelumrechnung);
        }

        /// <summary>
        /// Setzt die Ergebnisfelder zurück.
        /// </summary>
        private void EckeErgebnisReset()
        {
            textBlockEckeWinkelQueranschlagEins.Text = "";
            textBlockEckeWinkelQueranschlagZwei.Text = "";
            textBlockEckeWinkelSaegeblattEins.Text = "";
            textBlockEckeWinkelSaegeblattZwei.Text = "";
            textBlockEckeBreiteEins.Text = "";
            textBlockEckeBreiteZwei.Text = "";
            textBlockEckeBreiteMitSchraegeEins.Text = "";
            textBlockEckeBreiteMitSchraegeZwei.Text = "";
            textBlockEckeFlächenwinkel.Text = "";
            textBlockEckeBreitenversatzEinsErgebnis.Text = "";
            textBlockEckeBreitenversatzZweiErgebnis.Text = "";
            textBlockEckeSchraegeSEins.Text = "";
            textBlockEckeSchraegeSZwei.Text = "";
        }

        /// <summary>
        /// Setzt alle Felder zurück, leert die Grafik und aktualisiert die Feedbackleiste.
        /// </summary>
        /// <param name="sender">Der Button Zurücksetzen.</param>
        /// <param name="e"></param>
        private void ButtonEckeReset_Click(object sender, RoutedEventArgs e)
        {
            // Die Ergebnisfelder zurücksetzen.
            EckeErgebnisReset();

            // Die Checkbox auf nicht aktiv setzen.
            checkBoxEcke.IsChecked = false;

            // Alle Eingabefelder zurücksetzen.
            for (int i = 0; i < eckeEingaben.Length; i++)
            {
                eckeEingaben[i].Text = "";
                eckeEingaben[i].Background = Brushes.White;
            }

            // Die Grafik leeren.
            modelVisual3dEcke.Content = new Model3DGroup();
            
            // Deaktiviert die Meldungen [Berechnet, EingabeGeändert, AlphaBerechnet, AlphaGeändert, BetaBerechnet, BetaGeändert, UngültigeWerte, LiniexyUngültigeWerte, LiniexyZuVieleEingaben].
            eckeFeedback.Deactivate(eckeFeedback.Calculated, eckeFeedback.InputChanged, eckeFeedback.AlphaCalculated, eckeFeedback.AlphaChanged, eckeFeedback.BetaCalculated, 
                eckeFeedback.BetaChanged, eckeFeedback.InvalidValues, eckeFeedback.LineXYInvalidValues, eckeFeedback.LineXYToManyValues);

            // Aktiviert die Meldungen [EingabewerteEingeben].
            eckeFeedback.Activate(eckeFeedback.EnterValues);
        }

        /// <summary>
        /// Startet die Berechnung und weist den Ergebnisfeldern die Ergebnisse zu.
        /// </summary>
        /// <param name="sender">Der Button Berechnen.</param>
        /// <param name="e"></param>
        private void ButtonEckeBerechnung_Click(object sender, RoutedEventArgs e)
        {
            // Die Eingabefelder der Funktion Linie XY leeren.
            for (int i = 9; i < 13; i++)
            {
                eckeEingaben[i].Text = "";
            }

            // Deaktiviert die Meldung [EingabeGeändert].
            eckeFeedback.Deactivate(eckeFeedback.InputChanged);

            // Hilfsvariablen erstellen.
            double höhe = 0;
            double materialstärkeEins = 0;
            double materialstärkeZwei = 0;
            double winkelAlphaEins = 0;
            double winkelAlphaZwei = 0;
            double winkelBeta = 0;

            // Überprüfen ob ein Eingabefeld leer ist.
            if (TextBoxIsEmpty(eckeEingaben, 0, 5))
            {
                // Die Ergebnisfelder zurücksetzen.
                EckeErgebnisReset();
                
                // Die Grafik leeren.
                modelVisual3dEcke.Content = new Model3DGroup();

                // Deaktiviert die Meldung [Berechnet].
                eckeFeedback.Deactivate(eckeFeedback.Calculated);

                // Aktiviert die Meldung [EingabewerteEingeben].
                eckeFeedback.Activate(eckeFeedback.EnterValues);

                return;
            }

            // Überprüfen ob die Eingaben in den Eingabefeldern gültig sind und wenn nicht den Hintergrund rot setzen.
            if (!InputValid(textBoxEckeHoehe, ref höhe) || höhe <= 0)
                textBoxEckeHoehe.Background = Brushes.Red;

            if (!InputValid(textBoxEckeMaterialstaerkeEins, ref materialstärkeEins) || materialstärkeEins <= 0)
                textBoxEckeMaterialstaerkeEins.Background = Brushes.Red;

            if (!InputValid(textBoxEckeMaterialstaerkeZwei, ref materialstärkeZwei) || materialstärkeZwei <= 0)
                textBoxEckeMaterialstaerkeZwei.Background = Brushes.Red;

            if (!InputValid(textBoxEckeWinkelAlphaEins, ref winkelAlphaEins) || winkelAlphaEins < -90 || winkelAlphaEins > 90)
                textBoxEckeWinkelAlphaEins.Background = Brushes.Red;

            if (!InputValid(textBoxEckeWinkelAlphaZwei, ref winkelAlphaZwei) || winkelAlphaZwei < -90 || winkelAlphaZwei > 90)
                textBoxEckeWinkelAlphaZwei.Background = Brushes.Red;

            if (!InputValid(textBoxEckeWinkelBeta, ref winkelBeta) || winkelBeta <= 0 || winkelBeta >= 180)
                textBoxEckeWinkelBeta.Background = Brushes.Red;
            
            // Überprüfen ob der Hintergrund eines Eingabefeldes rot ist.
            if (TextBoxIsRed(eckeEingaben, 0, 5))
            {
                // Die Ergebnisfelder zurücksetzen.
                EckeErgebnisReset();

                // Die Grafik leeren.
                modelVisual3dEcke.Content = new Model3DGroup();
                
                // Deaktiviert die Meldung [Berechnet].
                eckeFeedback.Deactivate(eckeFeedback.Calculated);

                // Aktivert die Meldung [UngültigeWerte].
                eckeFeedback.Activate(eckeFeedback.InvalidValues);

                // Aktiviert die Meldung [EingabewerteEingeben].
                eckeFeedback.Activate(eckeFeedback.EnterValues);

                return;
            }

            // Die Werte aus den Eingabefeldern den Eigenschaften der Ecke zuweisen.
            ecke.Height = höhe;
            ecke.ThicknessFirstBoard = materialstärkeEins;
            ecke.ThicknessSecondBoard = materialstärkeZwei;
            ecke.AngleAlphaFirstBoard = winkelAlphaEins;
            ecke.AngleAlphaSecondBoard = winkelAlphaZwei;
            ecke.AngleBeta = winkelBeta;
            
            ecke.MiterJoint = checkBoxEcke.IsChecked.Value;

            // Den Schifterschnitt berechnen.
            ecke.Calculation();

            // Die Ergebnisse den Ergebnisfeldern zuweisen.
            textBlockEckeWinkelQueranschlagEins.Text = Math.Round(ecke.AngleCrossCutFirstBoard, 2) + "°";
            textBlockEckeWinkelQueranschlagZwei.Text = Math.Round(ecke.AngleCrossCutSecondBoard, 2) + "°";
            textBlockEckeWinkelSaegeblattEins.Text = Math.Round(ecke.AngleSawBladeTiltFirstBoard, 2) + "°";
            textBlockEckeWinkelSaegeblattZwei.Text = Math.Round(ecke.AngleSawBladeTiltSecondBoard, 2) + "°";
            textBlockEckeBreiteEins.Text = ecke.AngleAlphaFirstBoard == 90 || ecke.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(ecke.WidthFirstBoard, 2).ToString() + " mm";
            textBlockEckeBreiteZwei.Text = ecke.AngleAlphaSecondBoard == 90 || ecke.AngleAlphaSecondBoard == -90 ? "Error" : Math.Round(ecke.WidthSecondBoard, 2).ToString() + " mm";
            textBlockEckeBreiteMitSchraegeEins.Text = ecke.AngleAlphaFirstBoard == 90 || ecke.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(ecke.WidthWithSlantFirstBoard, 2) + " mm";
            textBlockEckeBreiteMitSchraegeZwei.Text = ecke.AngleAlphaSecondBoard == 90 || ecke.AngleAlphaSecondBoard == -90 ? "Error" : Math.Round(ecke.WidhtWithSlantSecondBoard, 2) + " mm";
            textBlockEckeFlächenwinkel.Text = Math.Round(ecke.AngleDihedral, 2) + "°";

            if (ecke.AngleAlphaFirstBoard == 90 || ecke.AngleAlphaSecondBoard == 90 || ecke.AngleAlphaFirstBoard == -90 || ecke.AngleAlphaSecondBoard == -90)
            {
                textBlockEckeBreitenversatzEinsErgebnis.Text = "Error";
                textBlockEckeBreitenversatzZweiErgebnis.Text = "Error";
                textBlockEckeSchraegeSEins.Text = "Error";
                textBlockEckeSchraegeSZwei.Text = "Error";
            }
            else
            {
                textBlockEckeBreitenversatzEinsErgebnis.Text = Convert.ToString(Math.Round(Math.Sin(Calculate.DegreeToRadian(ecke.AngleAlphaFirstBoard)) * ecke.WidthFirstBoard, 2)) + " mm";
                textBlockEckeBreitenversatzZweiErgebnis.Text = Convert.ToString(Math.Round(Math.Sin(Calculate.DegreeToRadian(ecke.AngleAlphaSecondBoard)) * ecke.WidthSecondBoard, 2)) + " mm";
                textBlockEckeSchraegeSEins.Text = Convert.ToString(Math.Round(ecke.ThicknessFirstBoard / Math.Cos(Calculate.DegreeToRadian(ecke.AngleAlphaFirstBoard)), 2)) + " mm";
                textBlockEckeSchraegeSZwei.Text = Convert.ToString(Math.Round(ecke.ThicknessSecondBoard / Math.Cos(Calculate.DegreeToRadian(ecke.AngleAlphaSecondBoard)), 2)) + " mm";
            }

            // Ein 3D-Modell der Pyramide erzeugen und der Grafik zuweisen.
            ecke.CreateModel(modelVisual3dEcke);

            // Deaktiviert die Meldung [EingabewerteEingeben].
            eckeFeedback.Deactivate(eckeFeedback.EnterValues);

            // Aktiviert die Meldung [Berechnet].
            eckeFeedback.Activate(eckeFeedback.Calculated);
        }

        /// <summary>
        /// Berechnet die Linien XY.
        /// </summary>
        /// <param name="sender">Der Button Linie XY Berechnen.</param>
        /// <param name="e"></param>
        private void ButtonEckeLiniexy_Click(object sender, RoutedEventArgs e)
        {
            // Hilfsvariablen erstellen.
            double LinieYEins = 0;
            double LinieYZwei = 0;
            double LinieXEins = 0;
            double LinieXZwei = 0;

            // Den Hintergrund der Eingabefelder auf weiß setzen.
            for (int i = 9; i < 13; i++)
                eckeEingaben[i].Background = Brushes.White;

            // Überprüfen ob die Eingaben gültig sind.
            if (textBoxEckeLinieYEins.Text != "" && !InputValid(textBoxEckeLinieYEins, ref LinieYEins))
                textBoxEckeLinieYEins.Background = Brushes.Red;

            if (textBoxEckeLinieYZwei.Text != "" && !InputValid(textBoxEckeLinieYZwei, ref LinieYZwei))
                textBoxEckeLinieYZwei.Background = Brushes.Red;

            if (textBoxEckeLinieXEins.Text != "" && !InputValid(textBoxEckeLinieXEins, ref LinieXEins))
                textBoxEckeLinieXEins.Background = Brushes.Red;

            if (textBoxEckeLinieXZwei.Text != "" && !InputValid(textBoxEckeLinieXZwei, ref LinieXZwei))
                textBoxEckeLinieXZwei.Background = Brushes.Red;

            // Wenn der Hintergrund einer der Eingabefelder rot ist.
            if (TextBoxIsRed(eckeEingaben, 9, 12))
            {
                // Aktiviert die Meldung [LiniexyUngültigeWerte].
                eckeFeedback.Activate(eckeFeedback.LineXYInvalidValues);
            }

            // Überprüft ob es bei einem Teil mehrere Eingaben gibt und färbt diese Felder rot.
            if (textBoxEckeLinieYEins.Text != "" && textBoxEckeLinieXEins.Text != "" && liniexyTeilEinsBerechnet == false)
            {
                textBoxEckeLinieYEins.Background = Brushes.Red;
                textBoxEckeLinieXEins.Background = Brushes.Red;

                // Aktiviert die Meldung [LiniexyZuVieleEingaben].
                eckeFeedback.Activate(eckeFeedback.LineXYToManyValues);
            }

            if (textBoxEckeLinieYZwei.Text != "" && textBoxEckeLinieXZwei.Text != "" && liniexyTeilZweiBerechnet == false)
            {
                textBoxEckeLinieYZwei.Background = Brushes.Red;
                textBoxEckeLinieXZwei.Background = Brushes.Red;

                // Aktiviert die Meldung [LiniexyZuVieleEingaben].
                eckeFeedback.Activate(eckeFeedback.LineXYToManyValues);
            }
            
            // Wenn die Ecke erfolgreich berechnet wurde und kein Eingabefeld rot ist.
            if (eckeFeedback.Calculated.Active && !TextBoxIsRed(eckeEingaben, 9, 12))
            {
                // Die Zusätze berechnen.
                double zusatzEins = Math.Tan(Calculate.DegreeToRadian(ecke.AngleCrossCutFirstBoard)) * ecke.WidthFirstBoard;
                double zusatzZwei = Math.Tan(Calculate.DegreeToRadian(ecke.AngleCrossCutSecondBoard)) * ecke.WidthSecondBoard;

                // Wenn es bei einer Linie eine Eingabe gibt die andere Linie berechnen und dem anderen Eingabefeld zuweisen.
                if (textBoxEckeLinieYEins.Text != "" && liniexyTeilEinsBerechnet == false)
                {
                    textBoxEckeLinieXEins.Text = Convert.ToString(Math.Round(LinieYEins + (2 * zusatzEins), 2));
                    liniexyTeilEinsBerechnet = true;
                }
                else if (textBoxEckeLinieXEins.Text != "" && liniexyTeilEinsBerechnet == false)
                {
                    textBoxEckeLinieYEins.Text = Convert.ToString(Math.Round(LinieXEins - (2 * zusatzEins), 2));
                    liniexyTeilEinsBerechnet = true;
                }

                if (textBoxEckeLinieYZwei.Text != "" && liniexyTeilZweiBerechnet == false)
                {
                    textBoxEckeLinieXZwei.Text = Convert.ToString(Math.Round(LinieYZwei + (2 * zusatzZwei), 2));
                    liniexyTeilZweiBerechnet = true;
                }
                else if (textBoxEckeLinieXZwei.Text != "" && liniexyTeilZweiBerechnet == false)
                {
                    textBoxEckeLinieYZwei.Text = Convert.ToString(Math.Round(LinieXZwei - (2 * zusatzZwei), 2));
                    liniexyTeilZweiBerechnet = true;
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
            // Deaktiviert die Meldung [AlphaGeändert].
            eckeFeedback.Deactivate(eckeFeedback.AlphaChanged);

            // Hilfsvariablen erstellen.
            double höhe = 0;
            double breitenversatzEins = 0;
            double breitenversatzZwei = 0;

            // Überprüfen ob die Eingaben in den Eingabefeldern gültig sind und wenn nicht den Hintergrund rot setzen.
            if (textBoxEckeHoehe.Text != "" && (!InputValid(textBoxEckeHoehe, ref höhe) || höhe <= 0))
                textBoxEckeHoehe.Background = Brushes.Red;

            if (textBoxEckeBreitenversatzEins.Text != "" && (!InputValid(textBoxEckeBreitenversatzEins, ref breitenversatzEins)))
                textBoxEckeBreitenversatzEins.Background = Brushes.Red;

            if (textBoxEckeBreitenversatzZwei.Text != "" && (!InputValid(textBoxEckeBreitenversatzZwei, ref breitenversatzZwei)))
                textBoxEckeBreitenversatzZwei.Background = Brushes.Red;

            // Überprüfen ob ein Eingabefeld rot ist.
            if (textBoxEckeHoehe.Background == Brushes.Red || TextBoxIsRed(eckeEingaben, 6, 7))
            {
                // Aktiviert die Meldung [UngültigeWerte].
                eckeFeedback.Activate(eckeFeedback.InvalidValues);
            }

            // Hilfsvariable erstellen.
            var x = false;

            // Wenn alle Eingaben gültig sind und die Eingabefelder nicht leer sind die Winkel Alpha berechnen und zuweisen.
            if (textBoxEckeHoehe.Background == Brushes.White && textBoxEckeHoehe.Text != "" && textBoxEckeBreitenversatzEins.Background == Brushes.White && textBoxEckeBreitenversatzEins.Text != "")
            {
                textBoxEckeWinkelAlphaEins.Text = Convert.ToString(Math.Round(Calculate.RadianToDegree(Math.Atan(breitenversatzEins / höhe)), 4));

                x = true;
            }

            if (textBoxEckeHoehe.Background == Brushes.White && textBoxEckeHoehe.Text != "" && textBoxEckeBreitenversatzZwei.Background == Brushes.White && textBoxEckeBreitenversatzZwei.Text != "")
            {
                textBoxEckeWinkelAlphaZwei.Text = Convert.ToString(Math.Round(Calculate.RadianToDegree(Math.Atan(breitenversatzZwei / höhe)), 4));

                x = true;
            }

            // Wenn eine Berechnung erfolgt ist.
            if (x)
            {
                // Aktiviert die Meldung [AlphaBerechnet].
                eckeFeedback.Activate(eckeFeedback.AlphaCalculated);
            }
        }

        /// <summary>
        /// Berechnet den Winkel Beta.
        /// </summary>
        /// <param name="sender">Der Button Winkel Beta Berechnen.</param>
        /// <param name="e"></param>
        private void ButtonEckeWinkelBeta_Click(object sender, RoutedEventArgs e)
        {
            // Deaktiviert die Meldung [BetaGeändert].
            eckeFeedback.Deactivate(eckeFeedback.BetaChanged);

            // Hilfsvariable erstellen.
            short anzahlSeiten = 0;

            // Überprüfen ob das Eingabefeld leer ist.
            if (textBoxEckeAnzahlSeiten.Text == "")
                return;

            // Überprüfen ob die Eingabe in dem Eingabefeld gültig ist und wenn nicht den Hintergrund rot setzen.
            if (!InputValid(textBoxEckeAnzahlSeiten, ref anzahlSeiten) || anzahlSeiten < 3 || anzahlSeiten > 100)
            {
                textBoxEckeAnzahlSeiten.Background = Brushes.Red;

                // Aktiviert die Meldung [UngültigeWerte].
                eckeFeedback.Activate(eckeFeedback.InvalidValues);

                return;
            }

            // Den Winkel Beta berechnen und zuweisen.
            textBoxEckeWinkelBeta.Text = Convert.ToString(Math.Round(Convert.ToDouble((anzahlSeiten - 2.0) * 180.0 / anzahlSeiten), 4));

            // Aktiviert die Meldung [BetaBerechnet].
            eckeFeedback.Activate(eckeFeedback.BetaCalculated);
        }

        /// <summary>
        /// Stellt den Hintergrund eines veränderten Eingabefeldes weiß und aktualisiert die Feedbackleiste.
        /// </summary>
        /// <param name="sender">Das Eingabefeld das die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void EckeInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Den Hintergrund des Eingabefeldes, das die Methode ausgelöst hat, weiß setzen.
            ((TextBox)sender).Background = Brushes.White;

            // Wenn kein Hintergrund der Eingabefelder rot ist.
            if (!TextBoxIsRed(eckeEingaben, 0, 8))
            {
                // Deaktivert die Meldung [UngültigeWerte].
                eckeFeedback.Deactivate(eckeFeedback.InvalidValues);
            }

            // Wenn das Eingabefeld, das die Methode ausgelöst hat, keines der Funktionen ist und die Ecke erfolgreich berechnet wurde.
            for (int i = 0; i <= 5; i++)
            {
                if (eckeFeedback.Calculated.Active && (TextBox)sender == eckeEingaben[i])
                {
                    // Aktiviert die Meldung [EingabeGeändert].
                    eckeFeedback.Activate(eckeFeedback.InputChanged);

                    // Deaktiviert die Meldung [Berechnet].
                    eckeFeedback.Deactivate(eckeFeedback.Calculated);
                }
            }

            // Wenn das Eingabefeld, das die Methode ausgelöst hat, eine der Funktion Winkel Alpha Berechnen ist und diese erfolgreich berechnet wurde.
            for (int i = 6; i <= 7; i++)
            {
                if (eckeFeedback.AlphaCalculated.Active && ((TextBox)sender == eckeEingaben[i] || (TextBox)sender == textBoxEckeWinkelAlphaEins || 
                    (TextBox)sender == textBoxEckeWinkelAlphaZwei || (TextBox)sender == textBoxEckeHoehe))
                {
                    // Aktiviert die Meldung [AlphaGeändert].
                    eckeFeedback.Activate(eckeFeedback.AlphaChanged);

                    // Deaktiviert die Meldung [AlphaBerechnet].
                    eckeFeedback.Deactivate(eckeFeedback.AlphaCalculated);
                }
            }

            // Wenn das Eingabefeld, das die Methode ausgelöst hat, das der Funktion Winkel Beta Berechnen ist und diese erfolgreich berechnet wurde.
            if (eckeFeedback.BetaCalculated.Active && ((TextBox)sender == textBoxEckeAnzahlSeiten || (TextBox)sender == textBoxEckeWinkelBeta))
            {
                // Aktiviert die Meldung [BetaGeändert].
                eckeFeedback.Activate(eckeFeedback.BetaChanged);

                // Deaktiviert die Meldung [BetaBerechnet].
                eckeFeedback.Deactivate(eckeFeedback.BetaCalculated);
            }
        }

        /// <summary>
        /// Aktualisiert die Feedbackleiste.
        /// </summary>
        /// <param name="sender">Die Checkbox die die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void EckeCheckbox_Click(object sender, RoutedEventArgs e)
        {
            // Wenn die Ecke erfolgreich berechnet wurde.
            if (eckeFeedback.Calculated.Active)
            {
                // Aktiviert die Meldung [EingabeGeändert].
                eckeFeedback.Activate(eckeFeedback.InputChanged);

                // Deaktiviert die Meldung [Berechnet].
                eckeFeedback.Deactivate(eckeFeedback.Calculated);
            }
        }

        /// <summary>
        /// Stellt den Hintergrund eines veränderten Eingabefeldes weiß und aktualisiert die Feedbackleiste.
        /// </summary>
        /// <param name="sender">Ein Eingabefeld der Funktion Linie XY das die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void EckeInputLiniexy_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Setzt den Hintergrund des Eingabefeldes, das die Methode aufgerufen hat, auf weiß.
            ((TextBox)sender).Background = Brushes.White;

            // Wenn das Eingabefeld vom Teil Eins ist die Berechnet-Variable des Teil Eins auf false setzen.
            if (((TextBox)sender) == textBoxEckeLinieXEins || ((TextBox)sender) == textBoxEckeLinieYEins)
                liniexyTeilEinsBerechnet = false;

            // Wenn das Eingabefeld vom Teil Zwei ist die Berechnet-Variable des Teil Zwei auf false setzen.
            if (((TextBox)sender) == textBoxEckeLinieXZwei || ((TextBox)sender) == textBoxEckeLinieYZwei)
                liniexyTeilZweiBerechnet = false;

            // Wenn bei der Funktion Linie XY beim Teil Eins nicht beide Eingabefelder gleichzeitig rot sind.
            if (!(textBoxEckeLinieYEins.Background == Brushes.Red && textBoxEckeLinieXEins.Background == Brushes.Red))
            {
                // Den Hintergrund der Eingabefelder weiß setzen wenn sie gültig sind.
                WhiteIfValid(textBoxEckeLinieYEins);
                WhiteIfValid(textBoxEckeLinieXEins);
            }

            // Wenn bei der Funktion Linie XY beim Teil Zwei nicht beide Eingabefelder gleichzeitig rot sind.
            if (!(textBoxEckeLinieYZwei.Background == Brushes.Red && textBoxEckeLinieXZwei.Background == Brushes.Red))
            {
                // Den Hintergrund der Eingabefelder weiß setzen wenn sie gültig sind.
                WhiteIfValid(textBoxEckeLinieYZwei);
                WhiteIfValid(textBoxEckeLinieXZwei);
            }

            // Wenn bei der Funktion Linie XY bei beiden Teilen nicht beide Eingabefelder gleichzeitig rot sind.
            if (!(textBoxEckeLinieYEins.Background == Brushes.Red && textBoxEckeLinieXEins.Background == Brushes.Red) && 
                !(textBoxEckeLinieYZwei.Background == Brushes.Red && textBoxEckeLinieXZwei.Background == Brushes.Red))
            {
                // Deaktivert die Meldung [LiniexyZuVieleEingaben].
                eckeFeedback.Deactivate(eckeFeedback.LineXYToManyValues);
            }

            // Geht die Eingabefelder der Funktion Linie XY durch.
            for (int i = 9; i < 13; i++)
            {
                // Hilfsvariable erstellen.
                double x = 0;
                
                // Wenn eine Eingabe ungültig ist.
                if ((eckeEingaben[i].Background == Brushes.Red) && (eckeEingaben[i].Text != "") && (!InputValid(eckeEingaben[i], ref x) || x < 0))
                {
                    return;
                }
            }

            // Deaktiviert die Meldung [LiniexyUngültigeWerte].
            eckeFeedback.Deactivate(eckeFeedback.LineXYInvalidValues);
        }

        /// <summary>
        /// Resizes the columns of the grid so only the most right one gets bigger if the window gets bigger.
        /// </summary>
        /// <param name="sender">The grid to resize the columns in.</param>
        /// <param name="e"></param>
        private void CornerGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            cornerColumnResize.ExpandMostRightIfBigger(sender, e);
        }

        /// <summary>
        /// Resizes the columns of the grid so only the one below the mouse is fully shown if the window gets smaller.
        /// </summary>
        /// <param name="sender">The grid to resize the columns in.</param>
        /// <param name="e"></param>
        private void CornerGrid_MouseMove(object sender, MouseEventArgs e)
        {
            cornerColumnResize.ShowFullyIfSmaller(sender, e);
        }

        #endregion

        #region Methoden Pyramide mit Grund- und Oberlinie
        
        /// <summary>
        /// Steuert den Zoom durch verändern der Kameraposition und Blickrichtung beim Scrollen über der Grafik.
        /// </summary>
        /// <param name="sender">Das Grid in der die Grafik ist.</param>
        /// <param name="e"></param>
        private void GridPyramideLinieGrafik_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Viewport3D_MouseWheel(perspectiveCameraPyramideLinie, pyramideLinieKameraWinkel, e);
        }

        /// <summary>
        /// Dreht das 3D-Modell bei Bewegung der Maus nach links oder rechts.
        /// </summary>
        /// <param name="sender">Das Grid in der die Grafik ist.</param>
        /// <param name="e"></param>
        private void GridPyramideLinieGrafik_MouseMove(object sender, MouseEventArgs e)
        {
            Grid_MouseMove(pyramideLinieRotation, ref pyramideLinieKameraWinkel, perspectiveCameraPyramideLinie, sender, e);
        }

        /// <summary>
        /// Rechnet den Winkel um und zeigt das Ergebnis im Ergebnisfeld an.
        /// </summary>
        /// <param name="sender">Das Eingabefeld der Winkelumrechnung.</param>
        /// <param name="e"></param>
        private void TextBoxPyramideLinieWinkelumrechnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            AngleConversion(textBoxPyramideLinieWinkelumrechnung, textBlockPyramideLinieWinkelumrechnung);
        }

        /// <summary>
        /// Setzt die Ergebnisfelder zurück und setzt die Variable pyramideLinieBerechnet auf false.
        /// </summary>
        private void PyramideLinieErgebnisReset()
        {
            textBlockPyramideLinieWinkelQueranschlag.Text = "";
            textBlockPyramideLinieWinkelSaegeblatt.Text = "";
            textBlockPyramideLinieBreite.Text = "";
            textBlockPyramideLinieBreiteMitSchraege.Text = "";
            textBlockPyramideLinieFlächenwinkel.Text = "";
            textBlockPyramideLinieBreitenversatz.Text = "";
            textBlockPyramideLinieSchraegeS.Text = "";
            textBlockPyramideLinieNeigungswinkel.Text = "";
            textBlockPyramideLinieInkreisradiusOA.Text = "";
            textBlockPyramideLinieInkreisradiusOI.Text = "";
            textBlockPyramideLinieInkreisradiusUA.Text = "";
            textBlockPyramideLinieInkreisradiusUI.Text = "";
            textBlockPyramideLinieUmkreisradiusOA.Text = "";
            textBlockPyramideLinieUmkreisradiusOI.Text = "";
            textBlockPyramideLinieUmkreisradiusUA.Text = "";
            textBlockPyramideLinieUmkreisradiusUI.Text = "";
        }

        /// <summary>
        /// Setzt alle Felder zurück, leert die Grafik und aktualisiert die Feedbackleiste.
        /// </summary>
        /// <param name="sender">Der Button Zurücksetzen.</param>
        /// <param name="e"></param>
        private void ButtonPyramideLinieReset_Click(object sender, RoutedEventArgs e)
        {
            // Die Ergebnisfelder zurücksetzen.
            PyramideLinieErgebnisReset();

            // Alle Eingabefelder zurücksetzen.
            for (int i = 0; i < pyramideLinieEingaben.Length; i++)
            {
                pyramideLinieEingaben[i].Text = "";
                pyramideLinieEingaben[i].Background = Brushes.White;
            }

            // Die Grafik leeren.
            modelVisual3dPyramideLinie.Content = new Model3DGroup();
            
            // Deaktiviert die Meldungen [UngültigeWerte, EingabeGeändert, Berechnet].
            pyramideLinieFeedback.Deactivate(pyramideLinieFeedback.InvalidValues, pyramideLinieFeedback.InputChanged, pyramideLinieFeedback.Calculated);

            // Aktiviert die Meldung [EingabewerteEingeben].
            pyramideLinieFeedback.Activate(pyramideLinieFeedback.EnterValues);
        }

        /// <summary>
        /// Startet die Berechnung und weist den Ergebnisfeldern die Ergebnisse zu.
        /// </summary>
        /// <param name="sender">Der Button Berechnen.</param>
        /// <param name="e"></param>
        private void ButtonPyramideLinieBerechnung_Click(object sender, RoutedEventArgs e)
        {
            // Deaktiviert die Meldung [EingabeGeändert].
            pyramideLinieFeedback.Deactivate(pyramideLinieFeedback.InputChanged);

            // Hilfsvariablen erstellen.
            double höhe = 0;
            double materialstärke = 0;
            short anzahlSeiten = 0;
            double grundlinie = 0;
            double oberlinie = 0;

            // Überprüfen ob ein Eingabefeld leer ist.
            if (TextBoxIsEmpty(pyramideLinieEingaben, 0, 4))
            {
                // Die Ergebnisfelder zurücksetzen.
                PyramideLinieErgebnisReset();

                // Die Grafik leeren.
                modelVisual3dPyramideLinie.Content = new Model3DGroup();

                // Deaktiviert die Meldung [Berechnet].
                pyramideLinieFeedback.Deactivate(pyramideLinieFeedback.Calculated);

                // Aktiviert die Meldung [EingabewerteEingeben].
                pyramideLinieFeedback.Activate(pyramideLinieFeedback.EnterValues);

                return;
            }

            // Prüfen ob die Eingaben in den Eingabefeldern gültig sind und wenn nicht den Hintergrund rot setzen.
            if (!InputValid(textBoxPyramideLinieHoehe, ref höhe) || höhe <= 0)
                textBoxPyramideLinieHoehe.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideLinieStaerke, ref materialstärke) || materialstärke <= 0)
                textBoxPyramideLinieStaerke.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideLinieAnzahlSeiten, ref anzahlSeiten) || anzahlSeiten < 3 || anzahlSeiten > 100)
                textBoxPyramideLinieAnzahlSeiten.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideLinieGrundlinie, ref grundlinie) || grundlinie < 0)
                textBoxPyramideLinieGrundlinie.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideLinieOberlinie, ref oberlinie) || oberlinie < 0)
                textBoxPyramideLinieOberlinie.Background = Brushes.Red;

            // Überprüfen ob der Hintergrund eines Eingabefeldes rot ist.
            if (TextBoxIsRed(pyramideLinieEingaben, 0, 4))
            {
                // Die Ergebnisfelder zurücksetzen.
                PyramideLinieErgebnisReset();

                // Die Grafik leeren.
                modelVisual3dPyramideLinie.Content = new Model3DGroup();

                // Deaktiviert die Meldung [Berechnet].
                pyramideLinieFeedback.Deactivate(pyramideLinieFeedback.Calculated);

                // Aktiviert die Meldung [UngültigeWerte].
                pyramideLinieFeedback.Activate(pyramideLinieFeedback.InvalidValues);

                // Aktiviert die Meldung [EingabewerteEingeben].
                pyramideLinieFeedback.Activate(pyramideLinieFeedback.EnterValues);

                return;
            }

            // Die Werte aus den Eingabefeldern den Eigenschaften der Pyramide zuweisen.
            pyramideLinie.Height = höhe;
            pyramideLinie.ThicknessFirstBoard = materialstärke;
            pyramideLinie.ThicknessSecondBoard = materialstärke;
            pyramideLinie.NumberOfSides = anzahlSeiten;
            pyramideLinie.BottomSideLength = grundlinie;
            pyramideLinie.TopSideLength = oberlinie;

            // Berechnung und setzen der Eigenschaften für die Hauptberechnung.
            pyramideLinie.AngleBeta = Math.Round(Convert.ToDouble((pyramideLinie.NumberOfSides - 2.0) * 180.0 / pyramideLinie.NumberOfSides), 4);

            double alpha = Calculate.RadianToDegree(Math.Atan(((pyramideLinie.BottomSideLength / (2 * Math.Tan(Calculate.DegreeToRadian(180.0) / pyramideLinie.NumberOfSides))) -
                (pyramideLinie.TopSideLength / (2 * Math.Tan(Calculate.DegreeToRadian(180.0) / pyramideLinie.NumberOfSides)))) / pyramideLinie.Height));

            pyramideLinie.AngleAlphaFirstBoard = alpha;
            pyramideLinie.AngleAlphaSecondBoard = alpha;

            pyramideLinie.MiterJoint = true;

            // Den Schifterschnitt berechnen.
            pyramideLinie.Calculation();

            // Die Länge der Schräge S berechnen.
            double schrägeS = pyramideLinie.ThicknessFirstBoard / Math.Cos(Calculate.DegreeToRadian(pyramideLinie.AngleAlphaFirstBoard));

            // Die Ergebnisse den Ergebnisfeldern zuweisen.
            textBlockPyramideLinieWinkelQueranschlag.Text = Math.Round(pyramideLinie.AngleCrossCutFirstBoard, 2) + "°";
            textBlockPyramideLinieWinkelSaegeblatt.Text = Math.Round(pyramideLinie.AngleSawBladeTiltFirstBoard, 2) + "°";
            textBlockPyramideLinieBreite.Text = pyramideLinie.AngleAlphaFirstBoard == 90 || pyramideLinie.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(pyramideLinie.WidthFirstBoard, 2).ToString() + " mm";
            textBlockPyramideLinieBreiteMitSchraege.Text = pyramideLinie.AngleAlphaFirstBoard == 90 || pyramideLinie.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(pyramideLinie.WidthWithSlantFirstBoard, 2) + " mm";

            // Berechnung der weiteren Ergebnisse und Zuweisung zu den Ergebnisfeldern.
            textBlockPyramideLinieFlächenwinkel.Text = Math.Round(pyramideLinie.AngleDihedral, 2) + "°";
            textBlockPyramideLinieBreitenversatz.Text = Convert.ToString(Math.Round(Math.Sin(Calculate.DegreeToRadian(pyramideLinie.AngleAlphaFirstBoard)) * pyramideLinie.WidthFirstBoard, 2)) + " mm";
            textBlockPyramideLinieSchraegeS.Text = Convert.ToString(Math.Round(schrägeS, 2)) + " mm";
            textBlockPyramideLinieNeigungswinkel.Text = Convert.ToString(Math.Round(pyramideLinie.AngleAlphaFirstBoard, 2)) + " °";
            textBlockPyramideLinieInkreisradiusOA.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideLinie.TopSideLength, pyramideLinie.NumberOfSides), 2)) + " mm";
            textBlockPyramideLinieInkreisradiusOI.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideLinie.TopSideLength, pyramideLinie.NumberOfSides) - schrägeS, 2)) + " mm";
            textBlockPyramideLinieInkreisradiusUA.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideLinie.BottomSideLength, pyramideLinie.NumberOfSides), 2)) + " mm";
            textBlockPyramideLinieInkreisradiusUI.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideLinie.BottomSideLength, pyramideLinie.NumberOfSides) - schrägeS, 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusOA.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideLinie.TopSideLength, pyramideLinie.NumberOfSides), 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusOI.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideLinie.TopSideLength, pyramideLinie.NumberOfSides) - schrägeS /
                Math.Sin(Calculate.DegreeToRadian(pyramideLinie.AngleBeta / 2.0)), 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusUA.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideLinie.BottomSideLength, pyramideLinie.NumberOfSides), 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusUI.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideLinie.BottomSideLength, pyramideLinie.NumberOfSides) - schrägeS /
                Math.Sin(Calculate.DegreeToRadian(pyramideLinie.AngleBeta / 2.0)), 2)) + " mm";

            // Ein 3D-Modell der Pyramide erzeugen und der Grafik zuweisen.
            pyramideLinie.CreateModel(modelVisual3dPyramideLinie);

            // Deaktiviert die Meldungen [EingabewerteEingeben].
            pyramideLinieFeedback.Deactivate(pyramideLinieFeedback.EnterValues);

            // Aktiviert die Meldung [Berechnet].
            pyramideLinieFeedback.Activate(pyramideLinieFeedback.Calculated);
        }

        /// <summary>
        /// Stellt den Hintergrund eines veränderten Eingabefeldes weiß und aktualisiert die Feedbackleiste.
        /// </summary>
        /// <param name="sender">Das Eingabefeld das die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void PyramideLinieInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Den Hintergrund des Eingabefeldes, das die Methode ausgelöst hat, weiß setzen.
            ((TextBox)sender).Background = Brushes.White;

            // Wenn bei keinem Eingabefeld der Hintergrund rot ist.
            if (!TextBoxIsRed(pyramideLinieEingaben, 0, 4))
            {
                // Deaktiviert die Meldung [UngültigeWerte].
                pyramideLinieFeedback.Deactivate(pyramideLinieFeedback.InvalidValues);
            }

            // Wenn die Pyramide erfolgreich brechnet wurde.
            if (pyramideLinieFeedback.Calculated.Active)
            {
                // Aktiviert die Meldung [EingabeGeändert].
                pyramideLinieFeedback.Activate(pyramideLinieFeedback.InputChanged);

                // Deaktiviert die Meldung [Berechnet].
                pyramideLinieFeedback.Deactivate(pyramideLinieFeedback.Calculated);
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
        /// Steuert den Zoom durch verändern der Kameraposition und Blickrichtung beim Scrollen über der Grafik.
        /// </summary>
        /// <param name="sender">Das Grid in der die Grafik ist.</param>
        /// <param name="e"></param>
        private void GridPyramideWinkel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Viewport3D_MouseWheel(perspectiveCameraPyramideWinkel, pyramideWinkelKameraWinkel, e);
        }

        /// <summary>
        /// Dreht das 3D-Modell bei Bewegung der Maus nach links oder rechts.
        /// </summary>
        /// <param name="sender">Das Grid in der die Grafik ist.</param>
        /// <param name="e"></param>
        private void GridPyramideWinkel_MouseMove(object sender, MouseEventArgs e)
        {
            Grid_MouseMove(pyramideWinkelRotation, ref pyramideWinkelKameraWinkel, perspectiveCameraPyramideWinkel, sender, e);
        }

        /// <summary>
        /// Rechnet den Winkel um und zeigt das Ergebnis im Ergebnisfeld an.
        /// </summary>
        /// <param name="sender">Das Eingabefeld der Winkelumrechnung.</param>
        /// <param name="e"></param>
        private void TextBoxPyramideWinkelWinkelumrechnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            AngleConversion(textBoxPyramideWinkelWinkelumrechnung, textBlockPyramideWinkelWinkelumrechnung);
        }

        /// <summary>
        /// Setzt die Ergebnisfelder zurück.
        /// </summary>
        private void PyramideWinkelErgebnisReset()
        {
            textBlockPyramideWinkelWinkelQueranschlag.Text = "";
            textBlockPyramideWinkelWinkelSaegeblatt.Text = "";
            textBlockPyramideWinkelBreite.Text = "";
            textBlockPyramideWinkelBreiteMitSchraege.Text = "";
            textBlockPyramideWinkelFlächenwinkel.Text = "";
            textBlockPyramideWinkelBreitenversatzErgebnis.Text = "";
            textBlockPyramideWinkelSchraegeS.Text = "";
            textBlockPyramideWinkelOberlinie.Text = "";
            textBlockPyramideWinkelInkreisradiusOA.Text = "";
            textBlockPyramideWinkelInkreisradiusOI.Text = "";
            textBlockPyramideWinkelInkreisradiusUA.Text = "";
            textBlockPyramideWinkelInkreisradiusUI.Text = "";
            textBlockPyramideWinkelUmkreisradiusOA.Text = "";
            textBlockPyramideWinkelUmkreisradiusOI.Text = "";
            textBlockPyramideWinkelUmkreisradiusUA.Text = "";
            textBlockPyramideWinkelUmkreisradiusUI.Text = "";
        }

        /// <summary>
        /// Setzt alle Felder zurück, leert die Grafik und aktualisiert die Feedbackleiste.
        /// </summary>
        /// <param name="sender">Der Button Zurücksetzen.</param>
        /// <param name="e"></param>
        private void ButtonPyramideWinkelReset_Click(object sender, RoutedEventArgs e)
        {
            // Die Ergebnisfelder zurücksetzen.
            PyramideWinkelErgebnisReset();

            // Alle Eingabefelder zurücksetzen.
            for (int i = 0; i < pyramideWinkelEingaben.Length; i++)
            {
                pyramideWinkelEingaben[i].Text = "";
                pyramideWinkelEingaben[i].Background = Brushes.White;
            }

            // Die Grafik leeren.
            modelVisual3dPyramideWinkel.Content = new Model3DGroup();

            // Deaktiviert die Meldungen [UngültigeWerte, EingabeGeändert, Berechnet, NeigungswinkelBerechnet, NeigungswinkelGeändert, HöheGrößerAlsErgebend, HöheErforderlich].
            pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.InvalidValues, pyramideWinkelFeedback.InputChanged, pyramideWinkelFeedback.Calculated, 
                pyramideWinkelFeedback.TiltAngleCalculated, pyramideWinkelFeedback.TiltAngleChanged, pyramideWinkelFeedback.HeightLargerThanResulting, 
                pyramideWinkelFeedback.HeightNeeded);

            // Aktiviert die Meldung [EingabewerteEingeben].
            pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.EnterValues);
        }

        /// <summary>
        /// Startet die Berechnung und weist den Ergebnisfeldern die Ergebnisse zu.
        /// </summary>
        /// <param name="sender">Der Button Berechnen.</param>
        /// <param name="e"></param>
        private void ButtonPyramideWinkelBerechnung_Click(object sender, RoutedEventArgs e)
        {
            // Deaktiviert die Meldung [EingabeGeändert].
            pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.InputChanged);

            // Hilfsvariablen erstellen.
            short anzahlSeiten = 0;
            double materialstärke = 0;
            double grundlinie = 0;
            double neigungswinkel = 0;
            double höhe = 0;
            double höheErgebend = 0;

            // Überprüfen ob ein Eingabefeld leer ist.
            if (TextBoxIsEmpty(pyramideWinkelEingaben, 1, 4))
            {
                // Die Ergebnisfelder zurücksetzen.
                PyramideWinkelErgebnisReset();

                // Die Grafik leeren.
                modelVisual3dPyramideWinkel.Content = new Model3DGroup();

                // Deaktiviert die Meldung [Berechnet].
                pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.Calculated);

                // Aktiviert die Meldung [EingabewerteEingeben].
                pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.EnterValues);

                return;
            }

            // Überprüfen ob die Eingaben in den Eingabefeldern gültig sind und wenn nicht den Hintergrund rot setzen.
            if (!InputValid(textBoxPyramideWinkelAnzahlSeiten, ref anzahlSeiten) || anzahlSeiten < 3 || anzahlSeiten > 100)
                textBoxPyramideWinkelAnzahlSeiten.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideWinkelStaerke, ref materialstärke) || materialstärke <= 0)
                textBoxPyramideWinkelStaerke.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideWinkelGrundlinie, ref grundlinie) || grundlinie < 0)
                textBoxPyramideWinkelGrundlinie.Background = Brushes.Red;

            if (!InputValid(textBoxPyramideWinkelNeigungswinkel, ref neigungswinkel) || neigungswinkel < -90 || neigungswinkel > 90)
                textBoxPyramideWinkelNeigungswinkel.Background = Brushes.Red;

            if ((textBoxPyramideWinkelHoehe.Text != "") && (!InputValid(textBoxPyramideWinkelHoehe, ref höhe) || höhe <= 0))
                textBoxPyramideWinkelHoehe.Background = Brushes.Red;

            // Überprüfen ob der Hintergrund eines Eingabefeldes rot ist.
            if (TextBoxIsRed(pyramideWinkelEingaben, 0, 4))
            {
                // Die Ergebnisfelder zurücksetzen.
                PyramideWinkelErgebnisReset();

                // Die Grafik leeren.
                modelVisual3dPyramideWinkel.Content = new Model3DGroup();

                // Deaktiviert die Meldung [Berechnet].
                pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.Calculated);

                // Aktiviert die Meldung [UngültigeWerte].
                pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.InvalidValues);

                // Aktiviert die Meldung [EingabewerteEingeben].
                pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.EnterValues);
                
                return;
            }

            // Die Werte aus den Eingabefeldern den Eigenschaften der Pyramide zuweisen.
            pyramideWinkel.NumberOfSides = anzahlSeiten;
            pyramideWinkel.ThicknessFirstBoard = materialstärke;
            pyramideWinkel.ThicknessSecondBoard = materialstärke;
            pyramideWinkel.BottomSideLength = grundlinie;
            pyramideWinkel.AngleAlphaFirstBoard = neigungswinkel;
            pyramideWinkel.AngleAlphaSecondBoard = neigungswinkel;

            // Die Höhe der Pyramide berechnen.
            höheErgebend = Math.Tan(Calculate.DegreeToRadian(90.0 - pyramideWinkel.AngleAlphaFirstBoard)) * (pyramideWinkel.BottomSideLength / (2 * Math.Tan(Calculate.DegreeToRadian(180.0 /
                pyramideWinkel.NumberOfSides))));

            // Wenn der Neigungswinkel positiv ist.
            if (pyramideWinkel.AngleAlphaFirstBoard > 0)
            {
                // Prüft ob eine Höhe eingegeben wurde und ob diese größer als die sich ergebende ist.
                if ((textBoxPyramideWinkelHoehe.Text != "") && höhe > höheErgebend)
                {
                    textBoxPyramideWinkelHoehe.Background = Brushes.Red;

                    // Deaktiviert die Meldung [Berechnet].
                    pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.Calculated);

                    // Aktiviert die Meldung [HöheGrößerAlsErgebend].
                    pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.HeightLargerThanResulting);

                    // Aktiviert die Meldung [EingabewerteEingeben].
                    pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.EnterValues);
                }
                else
                {
                    // Setzt die Höhe in die Eigenschaft der Pyramide ein.
                    pyramideWinkel.Height = höhe;
                }

                // Setzt die Höhe in das Eingabefeld ein wenn das Eingabefeld leer ist.
                if (textBoxPyramideWinkelHoehe.Text == "")
                {
                    textBoxPyramideWinkelHoehe.Text = Convert.ToString(Math.Round(höheErgebend - 0.01, 2));
                    pyramideWinkel.Height = double.Parse(textBoxPyramideWinkelHoehe.Text);
                }
            }
            // Wenn der Neigungswinkel negativ oder Null ist.
            else
            {
                // Wenn das Eingabefeld Höhe leer ist den Hintergrund rot setzen und die Feedbackleiste aktualisieren.
                if (textBoxPyramideWinkelHoehe.Text == "")
                {
                    textBoxPyramideWinkelHoehe.Background = Brushes.Red;

                    // Aktiviert die Meldung [HöheErforderlich].
                    pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.HeightNeeded);

                    // Deaktiviert die Meldung [Berechnet].
                    pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.Calculated);

                    // Aktiviert die Meldung [EingabewerteEingeben].
                    pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.EnterValues);
                }
                else
                {
                    // Setzt die Höhe in die Eigenschaft der Pyramide ein.
                    pyramideWinkel.Height = höhe;
                }
            }

            // Überprüfen ob der Hintergrund des Eingabefeldes Höhe rot ist.
            if (textBoxPyramideWinkelHoehe.Background == Brushes.Red)
            {
                // Die Ergebnisfelder leer setzen.
                PyramideWinkelErgebnisReset();

                // Ein leeres 3D-Modell erzeugen und der Grafik zuweisen.
                modelVisual3dPyramideWinkel.Content = new Model3DGroup();

                return;
            }

            // Berechnung und setzen der Eigenschaften für die Hauptberechnung.
            pyramideWinkel.AngleBeta = Convert.ToDouble((Convert.ToDouble(pyramideWinkel.NumberOfSides) - 2) * 180.0 / Convert.ToDouble(pyramideWinkel.NumberOfSides));

            pyramideWinkel.MiterJoint = true;

            // Den Schifterschnitt berechnen.
            pyramideWinkel.Calculation();

            // Die Länge der Schräge S berechnen.
            double schrägeS = pyramideWinkel.ThicknessFirstBoard / Math.Cos(Calculate.DegreeToRadian(pyramideWinkel.AngleAlphaFirstBoard));

            // Zuweisen der Ergebnisse zu den Ergebnisfeldern.
            textBlockPyramideWinkelWinkelQueranschlag.Text = Math.Round(pyramideWinkel.AngleCrossCutFirstBoard, 2) + "°";
            textBlockPyramideWinkelWinkelSaegeblatt.Text = Math.Round(pyramideWinkel.AngleSawBladeTiltFirstBoard, 2) + "°";
            textBlockPyramideWinkelBreite.Text = pyramideWinkel.AngleAlphaFirstBoard == 90 || pyramideWinkel.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(pyramideWinkel.WidthFirstBoard, 2).ToString() + " mm";
            textBlockPyramideWinkelBreiteMitSchraege.Text = pyramideWinkel.AngleAlphaFirstBoard == 90 || pyramideWinkel.AngleAlphaFirstBoard == -90 ? "Error" : Math.Round(pyramideWinkel.WidthWithSlantFirstBoard, 2) + " mm";

            // Berechnung der weiteren Ergebnisse und Zuweisung zu den Ergebnisfeldern.
            pyramideWinkel.TopSideLength = ((pyramideWinkel.BottomSideLength / (2 * Math.Tan(Calculate.DegreeToRadian(180.0 / pyramideWinkel.NumberOfSides)))) -
                Math.Sin(Calculate.DegreeToRadian(pyramideWinkel.AngleAlphaFirstBoard)) * pyramideWinkel.WidthFirstBoard) * (2 * Math.Tan(Calculate.DegreeToRadian(180.0 / pyramideWinkel.NumberOfSides)));

            textBlockPyramideWinkelFlächenwinkel.Text = Math.Round(pyramideWinkel.AngleDihedral, 2) + "°";
            textBlockPyramideWinkelInkreisradiusUA.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideWinkel.BottomSideLength, pyramideWinkel.NumberOfSides), 2)) + " mm";
            textBlockPyramideWinkelUmkreisradiusUA.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideWinkel.BottomSideLength, pyramideWinkel.NumberOfSides), 2)) + " mm";

            if (pyramideWinkel.AngleAlphaFirstBoard == 90 || pyramideWinkel.AngleAlphaFirstBoard == -90)
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
                textBlockPyramideWinkelBreitenversatzErgebnis.Text = Convert.ToString(Math.Round(Math.Sin(Calculate.DegreeToRadian(pyramideWinkel.AngleAlphaFirstBoard)) * pyramideWinkel.WidthFirstBoard, 2)) + " mm";
                textBlockPyramideWinkelOberlinie.Text = Convert.ToString(Math.Round(pyramideWinkel.TopSideLength, 2)) + " mm";
                textBlockPyramideWinkelUmkreisradiusOA.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideWinkel.TopSideLength, pyramideWinkel.NumberOfSides), 2)) + " mm";
                textBlockPyramideWinkelInkreisradiusOA.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideWinkel.TopSideLength, pyramideWinkel.NumberOfSides), 2)) + " mm";
                textBlockPyramideWinkelInkreisradiusOI.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideWinkel.TopSideLength, pyramideWinkel.NumberOfSides) - schrägeS, 2)) + " mm";
                textBlockPyramideWinkelInkreisradiusUI.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideWinkel.BottomSideLength, pyramideWinkel.NumberOfSides) - schrägeS, 2)) + " mm";
                textBlockPyramideWinkelUmkreisradiusOI.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideWinkel.TopSideLength, pyramideWinkel.NumberOfSides) - schrägeS / Math.Sin(Calculate.DegreeToRadian(pyramideWinkel.AngleBeta / 2.0)), 2)) + " mm";
                textBlockPyramideWinkelUmkreisradiusUI.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideWinkel.BottomSideLength, pyramideWinkel.NumberOfSides) - schrägeS / Math.Sin(Calculate.DegreeToRadian(pyramideWinkel.AngleBeta / 2.0)), 2)) + " mm";
            }

            // Ein neues 3D-Modell der Pyramide erzeugen und der Grafik zuweisen.
            pyramideWinkel.CreateModel(modelVisual3dPyramideWinkel);

            // Deaktiviert die Meldungen [EingabewerteEingeben].
            pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.EnterValues);

            // Aktiviert die Meldung [Berechnet].
            pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.Calculated);
        }

        /// <summary>
        /// Berechnet den Neigungswinkel und setzt ihn ein.
        /// </summary>
        /// <param name="sender">Der Button Neigungswinkel Berechnen.</param>
        /// <param name="e"></param>
        private void ButtonPyramideWinkelNeigungswinkel_Click(object sender, RoutedEventArgs e)
        {
            // Deaktiviert die Meldung [NeigungswinkelGeändert].
            pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.TiltAngleChanged);

            // Hilfsvariablen erstellen.
            double höhe = 0;
            double breitenversatz = 0;

            // Überprüfen ob ein Eingabefeld leer ist.
            if (textBoxPyramideWinkelHoehe.Text == "" || textBoxPyramideWinkelBreitenversatz.Text == "")
                return;

            // Überprüfen ob die Eingaben in den Eingabefeldern gültig sind und wenn nicht den Hintergrund rot setzen.
            if (textBoxPyramideWinkelHoehe.Text != "" && (!InputValid(textBoxPyramideWinkelHoehe, ref höhe) || höhe <= 0))
                textBoxPyramideWinkelHoehe.Background = Brushes.Red;

            if (textBoxPyramideWinkelBreitenversatz.Text != "" && (!InputValid(textBoxPyramideWinkelBreitenversatz, ref breitenversatz)))
                textBoxPyramideWinkelBreitenversatz.Background = Brushes.Red;

            // Überprüfen ob ein Eingabefeld rot ist.
            if (textBoxPyramideWinkelHoehe.Background == Brushes.Red || textBoxPyramideWinkelBreitenversatz.Background == Brushes.Red)
            {
                // Aktiviert die Meldung [UngültigeWerte].
                pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.InvalidValues);

                return;
            }

            // Den Neigungswinkel berechnen und einsetzen.
            textBoxPyramideWinkelNeigungswinkel.Text = Convert.ToString(Math.Round(Calculate.RadianToDegree(Math.Atan(breitenversatz / höhe)), 4));

            // Aktiviert die Meldung [NeigungswinkelBerechnet].
            pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.TiltAngleCalculated);
        }

        /// <summary>
        /// Changes the textbox background to white and updates the feedback area.
        /// </summary>
        /// <param name="sender">The textbox that called the method.</param>
        /// <param name="e"></param>
        private void PyramideWinkelInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var senderTextBox = (TextBox)sender;
            var feedback = pyramideWinkelFeedback;
            TextBox[] helper;

            senderTextBox.Background = Brushes.White;

            if (!TextBoxIsRed(pyramideWinkelEingaben, 0, 5))
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
        private bool TextBoxIsRed(TextBox[] textBoxes, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                if (textBoxes[i].Background == Brushes.Red)
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
        private bool TextBoxIsEmpty(TextBox[] textBoxes, int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                if (textBoxes[i].Text == "")
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the background of a textbox to white if the input is valid.
        /// </summary>
        /// <param name="textBox">The textbox.</param>
        private void WhiteIfValid(TextBox textBox)
        {
            double x = 0;

            if (InputValid(textBox, ref x) && x >= 0)
                textBox.Background = Brushes.White;
        }

        #region Graphic

        /// <summary>
        /// Changes the size of the graphic to fit the size of the viewmodel.
        /// </summary>
        /// <param name="sender">The viewport3D that shows the graphic.</param>
        /// <param name="e"></param>
        private void Viewport3D_SizeChanged(object sender, SizeChangedEventArgs e)
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
        /// <param name="e"></param>
        private void Viewport3D_MouseWheel(PerspectiveCamera camera, double cameraAngle, MouseWheelEventArgs e)
        {
            var newPosition = camera.Position;
            double distanceFlat = Math.Abs(camera.Position.X) * Math.Sqrt(2);
            double distanceToZero = Math.Sqrt(Math.Pow(Math.Abs(camera.Position.Z), 2) + Math.Pow(distanceFlat, 2));
            double cameraAngleRadian = Calculate.DegreeToRadian(cameraAngle);
            var zoomSpeed = 0.7;

            if (e.Delta > 0 && distanceToZero > 2.5)
            {
                double newDistance = distanceToZero - zoomSpeed;
                double newXYPosition = Math.Abs(Math.Cos(cameraAngleRadian) * newDistance / Math.Sqrt(2)) * -1;
                newPosition.X = newXYPosition;
                newPosition.Y = newXYPosition;
                newPosition.Z = Math.Sin(cameraAngleRadian) * newDistance;
            }

            if (e.Delta < 0 && distanceToZero < 20)
            {
                double newDistance = distanceToZero + zoomSpeed;
                double newXYPosition = Math.Abs(Math.Cos(cameraAngleRadian) * newDistance / Math.Sqrt(2)) * -1;
                newPosition.X = newXYPosition;
                newPosition.Y = newXYPosition;
                newPosition.Z = Math.Sin(cameraAngleRadian) * newDistance;
            }

            camera.SetValue(PerspectiveCamera.PositionProperty, newPosition);

            var lookDirection = new Vector3D(newPosition.X * -1, newPosition.Y * -1, newPosition.Z * -1);

            camera.SetValue(PerspectiveCamera.LookDirectionProperty, lookDirection);
        }

        /// <summary>
        /// Saves the mouse position when the mouse button is clicked.
        /// </summary>
        /// <param name="sender">The viewport3D which the mouse is over.</param>
        /// <param name="e"></param>
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mousePosition = e.GetPosition((Grid)sender);
        }

        /// <summary>
        /// Turns the 3D model based on the mouse movement when the left button is clicked.
        /// </summary>
        /// <param name="rotation">The rotation to change.</param>
        /// <param name="sender">The grid on which the mouse is moved.</param>
        /// <param name="e"></param>
        private void Grid_MouseMove(
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
            double cameraAngleRadian = Calculate.DegreeToRadian(cameraAngle);
            double newXYPosition = Math.Abs(Math.Cos(cameraAngleRadian) * distance / Math.Sqrt(2)) * -1;

            var newPosition = new Point3D
            {
                Z = Math.Sin(Calculate.DegreeToRadian(cameraAngle)) * distance,
                X = newXYPosition,
                Y = newXYPosition
            };

            camera.Position = newPosition;

            var newLookDirection = new Vector3D(newPosition.X * -1, newPosition.Y * -1, newPosition.Z * -1);

            camera.LookDirection = newLookDirection;

            mousePosition = newMousePosition;
        }

        #endregion

        #endregion
    }
}
