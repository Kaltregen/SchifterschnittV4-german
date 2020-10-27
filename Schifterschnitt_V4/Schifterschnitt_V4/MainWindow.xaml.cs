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
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Input;

namespace Schifterschnitt
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Variablen

        // Die Objekte erstellen.
        Ecke ecke = new Ecke();
        Pyramide pyramideLinie = new Pyramide();
        Pyramide pyramideWinkel = new Pyramide();

        // Variablen die anzeigen ob bei der Linie XY ein Teil berechnet ist.
        bool liniexyTeilEinsBerechnet = false;
        bool liniexyTeilZweiBerechnet = false;

        // Rotationen erstellen.
        AxisAngleRotation3D eckeRotation;
        AxisAngleRotation3D pyramideLinieRotation;
        AxisAngleRotation3D pyramideWinkelRotation;

        // Einen Punkt für die Position der Maus auf der Grafik anlegen.
        Point mausPosition = new Point(0, 0);

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

        #endregion

        #region ctor

        /// <summary>
        /// ctor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

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
            MausRad(perspectiveCameraEcke, eckeKameraWinkel, e);
        }

        /// <summary>
        /// Dreht das 3D-Modell bei Bewegung der Maus nach links oder rechts.
        /// </summary>
        /// <param name="sender">Das Grid in der die Grafik ist.</param>
        /// <param name="e"></param>
        private void GridEckeGrafik_MouseMove(object sender, MouseEventArgs e)
        {
            MausBewegung(eckeRotation, ref eckeKameraWinkel, perspectiveCameraEcke, sender, e);
        }

        /// <summary>
        /// Rechnet den Winkel um und zeigt das Ergebnis im Ergebnisfeld an.
        /// </summary>
        /// <param name="sender">Das Eingabefeld der Winkelumrechnung.</param>
        /// <param name="e"></param>
        private void TextBoxEckeWinkelumrechnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            Winkelumrechnung(textBoxEckeWinkelumrechnung, textBlockEckeWinkelumrechnung);
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
            if (TextBoxIstLeer(eckeEingaben, 0, 5))
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
            if (!EingabeGültig(textBoxEckeHoehe, ref höhe) || höhe <= 0)
                textBoxEckeHoehe.Background = Brushes.Red;

            if (!EingabeGültig(textBoxEckeMaterialstaerkeEins, ref materialstärkeEins) || materialstärkeEins <= 0)
                textBoxEckeMaterialstaerkeEins.Background = Brushes.Red;

            if (!EingabeGültig(textBoxEckeMaterialstaerkeZwei, ref materialstärkeZwei) || materialstärkeZwei <= 0)
                textBoxEckeMaterialstaerkeZwei.Background = Brushes.Red;

            if (!EingabeGültig(textBoxEckeWinkelAlphaEins, ref winkelAlphaEins) || winkelAlphaEins < -90 || winkelAlphaEins > 90)
                textBoxEckeWinkelAlphaEins.Background = Brushes.Red;

            if (!EingabeGültig(textBoxEckeWinkelAlphaZwei, ref winkelAlphaZwei) || winkelAlphaZwei < -90 || winkelAlphaZwei > 90)
                textBoxEckeWinkelAlphaZwei.Background = Brushes.Red;

            if (!EingabeGültig(textBoxEckeWinkelBeta, ref winkelBeta) || winkelBeta <= 0 || winkelBeta >= 180)
                textBoxEckeWinkelBeta.Background = Brushes.Red;
            
            // Überprüfen ob der Hintergrund eines Eingabefeldes rot ist.
            if (TextBoxIstRot(eckeEingaben, 0, 5))
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
            ecke.ModellErzeugen(modelVisual3dEcke);

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
            if (textBoxEckeLinieYEins.Text != "" && !EingabeGültig(textBoxEckeLinieYEins, ref LinieYEins))
                textBoxEckeLinieYEins.Background = Brushes.Red;

            if (textBoxEckeLinieYZwei.Text != "" && !EingabeGültig(textBoxEckeLinieYZwei, ref LinieYZwei))
                textBoxEckeLinieYZwei.Background = Brushes.Red;

            if (textBoxEckeLinieXEins.Text != "" && !EingabeGültig(textBoxEckeLinieXEins, ref LinieXEins))
                textBoxEckeLinieXEins.Background = Brushes.Red;

            if (textBoxEckeLinieXZwei.Text != "" && !EingabeGültig(textBoxEckeLinieXZwei, ref LinieXZwei))
                textBoxEckeLinieXZwei.Background = Brushes.Red;

            // Wenn der Hintergrund einer der Eingabefelder rot ist.
            if (TextBoxIstRot(eckeEingaben, 9, 12))
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
            if (eckeFeedback.Calculated.Active && !TextBoxIstRot(eckeEingaben, 9, 12))
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
            if (textBoxEckeHoehe.Text != "" && (!EingabeGültig(textBoxEckeHoehe, ref höhe) || höhe <= 0))
                textBoxEckeHoehe.Background = Brushes.Red;

            if (textBoxEckeBreitenversatzEins.Text != "" && (!EingabeGültig(textBoxEckeBreitenversatzEins, ref breitenversatzEins)))
                textBoxEckeBreitenversatzEins.Background = Brushes.Red;

            if (textBoxEckeBreitenversatzZwei.Text != "" && (!EingabeGültig(textBoxEckeBreitenversatzZwei, ref breitenversatzZwei)))
                textBoxEckeBreitenversatzZwei.Background = Brushes.Red;

            // Überprüfen ob ein Eingabefeld rot ist.
            if (textBoxEckeHoehe.Background == Brushes.Red || TextBoxIstRot(eckeEingaben, 6, 7))
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
            if (!EingabeGültig(textBoxEckeAnzahlSeiten, ref anzahlSeiten) || anzahlSeiten < 3 || anzahlSeiten > 100)
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
            if (!TextBoxIstRot(eckeEingaben, 0, 8))
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
                WeißWennGültig(textBoxEckeLinieYEins);
                WeißWennGültig(textBoxEckeLinieXEins);
            }

            // Wenn bei der Funktion Linie XY beim Teil Zwei nicht beide Eingabefelder gleichzeitig rot sind.
            if (!(textBoxEckeLinieYZwei.Background == Brushes.Red && textBoxEckeLinieXZwei.Background == Brushes.Red))
            {
                // Den Hintergrund der Eingabefelder weiß setzen wenn sie gültig sind.
                WeißWennGültig(textBoxEckeLinieYZwei);
                WeißWennGültig(textBoxEckeLinieXZwei);
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
                if ((eckeEingaben[i].Background == Brushes.Red) && (eckeEingaben[i].Text != "") && (!EingabeGültig(eckeEingaben[i], ref x) || x < 0))
                {
                    return;
                }
            }

            // Deaktiviert die Meldung [LiniexyUngültigeWerte].
            eckeFeedback.Deactivate(eckeFeedback.LineXYInvalidValues);
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
            MausRad(perspectiveCameraPyramideLinie, pyramideLinieKameraWinkel, e);
        }

        /// <summary>
        /// Dreht das 3D-Modell bei Bewegung der Maus nach links oder rechts.
        /// </summary>
        /// <param name="sender">Das Grid in der die Grafik ist.</param>
        /// <param name="e"></param>
        private void GridPyramideLinieGrafik_MouseMove(object sender, MouseEventArgs e)
        {
            MausBewegung(pyramideLinieRotation, ref pyramideLinieKameraWinkel, perspectiveCameraPyramideLinie, sender, e);
        }

        /// <summary>
        /// Rechnet den Winkel um und zeigt das Ergebnis im Ergebnisfeld an.
        /// </summary>
        /// <param name="sender">Das Eingabefeld der Winkelumrechnung.</param>
        /// <param name="e"></param>
        private void TextBoxPyramideLinieWinkelumrechnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            Winkelumrechnung(textBoxPyramideLinieWinkelumrechnung, textBlockPyramideLinieWinkelumrechnung);
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
            if (TextBoxIstLeer(pyramideLinieEingaben, 0, 4))
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
            if (!EingabeGültig(textBoxPyramideLinieHoehe, ref höhe) || höhe <= 0)
                textBoxPyramideLinieHoehe.Background = Brushes.Red;

            if (!EingabeGültig(textBoxPyramideLinieStaerke, ref materialstärke) || materialstärke <= 0)
                textBoxPyramideLinieStaerke.Background = Brushes.Red;

            if (!EingabeGültig(textBoxPyramideLinieAnzahlSeiten, ref anzahlSeiten) || anzahlSeiten < 3 || anzahlSeiten > 100)
                textBoxPyramideLinieAnzahlSeiten.Background = Brushes.Red;

            if (!EingabeGültig(textBoxPyramideLinieGrundlinie, ref grundlinie) || grundlinie < 0)
                textBoxPyramideLinieGrundlinie.Background = Brushes.Red;

            if (!EingabeGültig(textBoxPyramideLinieOberlinie, ref oberlinie) || oberlinie < 0)
                textBoxPyramideLinieOberlinie.Background = Brushes.Red;

            // Überprüfen ob der Hintergrund eines Eingabefeldes rot ist.
            if (TextBoxIstRot(pyramideLinieEingaben, 0, 4))
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
            pyramideLinie.AnzahlSeiten = anzahlSeiten;
            pyramideLinie.Grundlinie = grundlinie;
            pyramideLinie.Oberlinie = oberlinie;

            // Berechnung und setzen der Eigenschaften für die Hauptberechnung.
            pyramideLinie.AngleBeta = Math.Round(Convert.ToDouble((pyramideLinie.AnzahlSeiten - 2.0) * 180.0 / pyramideLinie.AnzahlSeiten), 4);

            double alpha = Calculate.RadianToDegree(Math.Atan(((pyramideLinie.Grundlinie / (2 * Math.Tan(Calculate.DegreeToRadian(180.0) / pyramideLinie.AnzahlSeiten))) -
                (pyramideLinie.Oberlinie / (2 * Math.Tan(Calculate.DegreeToRadian(180.0) / pyramideLinie.AnzahlSeiten)))) / pyramideLinie.Height));

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
            textBlockPyramideLinieInkreisradiusOA.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideLinie.Oberlinie, pyramideLinie.AnzahlSeiten), 2)) + " mm";
            textBlockPyramideLinieInkreisradiusOI.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideLinie.Oberlinie, pyramideLinie.AnzahlSeiten) - schrägeS, 2)) + " mm";
            textBlockPyramideLinieInkreisradiusUA.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideLinie.Grundlinie, pyramideLinie.AnzahlSeiten), 2)) + " mm";
            textBlockPyramideLinieInkreisradiusUI.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideLinie.Grundlinie, pyramideLinie.AnzahlSeiten) - schrägeS, 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusOA.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideLinie.Oberlinie, pyramideLinie.AnzahlSeiten), 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusOI.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideLinie.Oberlinie, pyramideLinie.AnzahlSeiten) - schrägeS /
                Math.Sin(Calculate.DegreeToRadian(pyramideLinie.AngleBeta / 2.0)), 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusUA.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideLinie.Grundlinie, pyramideLinie.AnzahlSeiten), 2)) + " mm";
            textBlockPyramideLinieUmkreisradiusUI.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideLinie.Grundlinie, pyramideLinie.AnzahlSeiten) - schrägeS /
                Math.Sin(Calculate.DegreeToRadian(pyramideLinie.AngleBeta / 2.0)), 2)) + " mm";

            // Ein 3D-Modell der Pyramide erzeugen und der Grafik zuweisen.
            pyramideLinie.ModellErzeugen(modelVisual3dPyramideLinie);

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
            if (!TextBoxIstRot(pyramideLinieEingaben, 0, 4))
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

        #endregion

        #region Methoden Pyramide mit Neigungswinkel
        
        /// <summary>
        /// Steuert den Zoom durch verändern der Kameraposition und Blickrichtung beim Scrollen über der Grafik.
        /// </summary>
        /// <param name="sender">Das Grid in der die Grafik ist.</param>
        /// <param name="e"></param>
        private void GridPyramideWinkel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            MausRad(perspectiveCameraPyramideWinkel, pyramideWinkelKameraWinkel, e);
        }

        /// <summary>
        /// Dreht das 3D-Modell bei Bewegung der Maus nach links oder rechts.
        /// </summary>
        /// <param name="sender">Das Grid in der die Grafik ist.</param>
        /// <param name="e"></param>
        private void GridPyramideWinkel_MouseMove(object sender, MouseEventArgs e)
        {
            MausBewegung(pyramideWinkelRotation, ref pyramideWinkelKameraWinkel, perspectiveCameraPyramideWinkel, sender, e);
        }

        /// <summary>
        /// Rechnet den Winkel um und zeigt das Ergebnis im Ergebnisfeld an.
        /// </summary>
        /// <param name="sender">Das Eingabefeld der Winkelumrechnung.</param>
        /// <param name="e"></param>
        private void TextBoxPyramideWinkelWinkelumrechnung_TextChanged(object sender, TextChangedEventArgs e)
        {
            Winkelumrechnung(textBoxPyramideWinkelWinkelumrechnung, textBlockPyramideWinkelWinkelumrechnung);
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
            if (TextBoxIstLeer(pyramideWinkelEingaben, 1, 4))
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
            if (!EingabeGültig(textBoxPyramideWinkelAnzahlSeiten, ref anzahlSeiten) || anzahlSeiten < 3 || anzahlSeiten > 100)
                textBoxPyramideWinkelAnzahlSeiten.Background = Brushes.Red;

            if (!EingabeGültig(textBoxPyramideWinkelStaerke, ref materialstärke) || materialstärke <= 0)
                textBoxPyramideWinkelStaerke.Background = Brushes.Red;

            if (!EingabeGültig(textBoxPyramideWinkelGrundlinie, ref grundlinie) || grundlinie < 0)
                textBoxPyramideWinkelGrundlinie.Background = Brushes.Red;

            if (!EingabeGültig(textBoxPyramideWinkelNeigungswinkel, ref neigungswinkel) || neigungswinkel < -90 || neigungswinkel > 90)
                textBoxPyramideWinkelNeigungswinkel.Background = Brushes.Red;

            if ((textBoxPyramideWinkelHoehe.Text != "") && (!EingabeGültig(textBoxPyramideWinkelHoehe, ref höhe) || höhe <= 0))
                textBoxPyramideWinkelHoehe.Background = Brushes.Red;

            // Überprüfen ob der Hintergrund eines Eingabefeldes rot ist.
            if (TextBoxIstRot(pyramideWinkelEingaben, 0, 4))
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
            pyramideWinkel.AnzahlSeiten = anzahlSeiten;
            pyramideWinkel.ThicknessFirstBoard = materialstärke;
            pyramideWinkel.ThicknessSecondBoard = materialstärke;
            pyramideWinkel.Grundlinie = grundlinie;
            pyramideWinkel.AngleAlphaFirstBoard = neigungswinkel;
            pyramideWinkel.AngleAlphaSecondBoard = neigungswinkel;

            // Die Höhe der Pyramide berechnen.
            höheErgebend = Math.Tan(Calculate.DegreeToRadian(90.0 - pyramideWinkel.AngleAlphaFirstBoard)) * (pyramideWinkel.Grundlinie / (2 * Math.Tan(Calculate.DegreeToRadian(180.0 /
                pyramideWinkel.AnzahlSeiten))));

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
            pyramideWinkel.AngleBeta = Convert.ToDouble((Convert.ToDouble(pyramideWinkel.AnzahlSeiten) - 2) * 180.0 / Convert.ToDouble(pyramideWinkel.AnzahlSeiten));

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
            pyramideWinkel.Oberlinie = ((pyramideWinkel.Grundlinie / (2 * Math.Tan(Calculate.DegreeToRadian(180.0 / pyramideWinkel.AnzahlSeiten)))) -
                Math.Sin(Calculate.DegreeToRadian(pyramideWinkel.AngleAlphaFirstBoard)) * pyramideWinkel.WidthFirstBoard) * (2 * Math.Tan(Calculate.DegreeToRadian(180.0 / pyramideWinkel.AnzahlSeiten)));

            textBlockPyramideWinkelFlächenwinkel.Text = Math.Round(pyramideWinkel.AngleDihedral, 2) + "°";
            textBlockPyramideWinkelInkreisradiusUA.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideWinkel.Grundlinie, pyramideWinkel.AnzahlSeiten), 2)) + " mm";
            textBlockPyramideWinkelUmkreisradiusUA.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideWinkel.Grundlinie, pyramideWinkel.AnzahlSeiten), 2)) + " mm";

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
                textBlockPyramideWinkelOberlinie.Text = Convert.ToString(Math.Round(pyramideWinkel.Oberlinie, 2)) + " mm";
                textBlockPyramideWinkelUmkreisradiusOA.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideWinkel.Oberlinie, pyramideWinkel.AnzahlSeiten), 2)) + " mm";
                textBlockPyramideWinkelInkreisradiusOA.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideWinkel.Oberlinie, pyramideWinkel.AnzahlSeiten), 2)) + " mm";
                textBlockPyramideWinkelInkreisradiusOI.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideWinkel.Oberlinie, pyramideWinkel.AnzahlSeiten) - schrägeS, 2)) + " mm";
                textBlockPyramideWinkelInkreisradiusUI.Text = Convert.ToString(Math.Round(Calculate.InscribedCircleRadius(pyramideWinkel.Grundlinie, pyramideWinkel.AnzahlSeiten) - schrägeS, 2)) + " mm";
                textBlockPyramideWinkelUmkreisradiusOI.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideWinkel.Oberlinie, pyramideWinkel.AnzahlSeiten) - schrägeS / Math.Sin(Calculate.DegreeToRadian(pyramideWinkel.AngleBeta / 2.0)), 2)) + " mm";
                textBlockPyramideWinkelUmkreisradiusUI.Text = Convert.ToString(Math.Round(Calculate.CircumscribedCircleRadius(pyramideWinkel.Grundlinie, pyramideWinkel.AnzahlSeiten) - schrägeS / Math.Sin(Calculate.DegreeToRadian(pyramideWinkel.AngleBeta / 2.0)), 2)) + " mm";
            }

            // Ein neues 3D-Modell der Pyramide erzeugen und der Grafik zuweisen.
            pyramideWinkel.ModellErzeugen(modelVisual3dPyramideWinkel);

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
            if (textBoxPyramideWinkelHoehe.Text != "" && (!EingabeGültig(textBoxPyramideWinkelHoehe, ref höhe) || höhe <= 0))
                textBoxPyramideWinkelHoehe.Background = Brushes.Red;

            if (textBoxPyramideWinkelBreitenversatz.Text != "" && (!EingabeGültig(textBoxPyramideWinkelBreitenversatz, ref breitenversatz)))
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
        /// Stellt den Hintergrund eines veränderten Eingabefeldes weiß und aktualisiert die Feedbackleiste.
        /// </summary>
        /// <param name="sender">Das Eingabefeld das die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void PyramideWinkelInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Den Hintergrund des Eingabefeldes, das die Methode ausgelöst hat, weiß setzen.
            ((TextBox)sender).Background = Brushes.White;

            // Wenn der Hintergrund von keinem Eingabefeld rot ist.
            if (!TextBoxIstRot(pyramideWinkelEingaben, 0, 5))
            {
                // Deaktiviert die Meldung [UngültigeWerte].
                pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.InvalidValues);
            }

            // Wenn das Eingabefeld, das die Methode ausgelöst hat, keines der Funktionen ist und die Ecke erfolgreich berechnet wurde.
            for (int i = 0; i <= 4; i++)
            {
                if (pyramideWinkelFeedback.Calculated.Active && (TextBox)sender == pyramideWinkelEingaben[i])
                {
                    // Aktiviert die Meldung [EingabeGeändert].
                    pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.InputChanged);

                    // Deaktiviert die Meldung [Berechnet].
                    pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.Calculated);
                }
            }

            // Wenn das Eingabefeld, das die Methode ausgelöst hat, eine der Funktion Neigungswinkel Berechnen ist und diese erfolgreich berechnet wurde.
            if (pyramideWinkelFeedback.TiltAngleCalculated.Active && ((TextBox)sender == textBoxPyramideWinkelHoehe || (TextBox)sender == textBoxPyramideWinkelBreitenversatz || 
                (TextBox)sender == textBoxPyramideWinkelNeigungswinkel))
            {
                // Aktiviert die Meldung [NeigungswinkelGeändert].
                pyramideWinkelFeedback.Activate(pyramideWinkelFeedback.TiltAngleChanged);

                // Deaktiviert die Meldung [NeigungswinkelBerechnet].
                pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.TiltAngleCalculated);
            }

            // Wenn der sender das Eingabefeld Höhe oder Neigungswinkel ist den Hintergrund des Eingabefelds Höhe weiß setzen und die Feedbackleiste aktualisieren.
            if (((TextBox)sender) == textBoxPyramideWinkelHoehe || ((TextBox)sender) == textBoxPyramideWinkelNeigungswinkel)
            {
                textBoxPyramideWinkelHoehe.Background = Brushes.White;

                // Deaktiviert die Meldung [HöheErforderlich].
                pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.HeightNeeded);

                // Deaktiviert die Meldung [HöheGrößerAlsErgebend].
                pyramideWinkelFeedback.Deactivate(pyramideWinkelFeedback.HeightLargerThanResulting);
            }
        }

        #endregion

        #region Gemeinsame Methoden

        /// <summary>
        /// Zeigt die Lizenz an.
        /// </summary>
        /// <param name="sender">Der About-Button.</param>
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
        /// Prüft ob die Eingabe in einer TextBox gültig ist und in eine Zahl konvertiert werden kann.
        /// </summary>
        /// <param name="textBox">Die TextBox deren Text geprüft werden soll.</param>
        /// <param name="zahl">Die Variable der die Zahl aus der Konvertierung zugewiesen werden soll.</param>
        /// <returns>True wenn der Text gültig ist und in eine Zahl konvertiert werden kann.</returns>
        private bool EingabeGültig(TextBox textBox, ref double zahl)
        {
            // Wenn der Text nicht gültig ist false zurückgeben.
            if (!TextGültig(textBox))
                return false;

            // Wenn der Text nicht in eine Zahl konvertiert werden kann false zurückgeben.
            if (!double.TryParse(textBox.Text, out zahl))
                return false;

            // Ansonsten true zurückgeben.
            return true;
        }

        /// <summary>
        /// Prüft ob die Eingabe in einer TextBox gültig ist und in eine Zahl konvertiert werden kann.
        /// </summary>
        /// <param name="textBox">Die TextBox deren Text geprüft werden soll.</param>
        /// <param name="zahl">Die Variable der die Zahl aus der Konvertierung zugewiesen werden soll.</param>
        /// <returns>True wenn der Text gültig ist und in eine Zahl konvertiert werden kann.</returns>
        private bool EingabeGültig(TextBox textBox, ref short zahl)
        {
            // Wenn der Text nicht gültig ist false zurückgeben.
            if (!TextGültig(textBox))
                return false;

            // Wenn der Text nicht in eine Zahl konvertiert werden kann false zurückgeben.
            if (!short.TryParse(textBox.Text, out zahl))
                return false;

            // Ansonsten true zurückgeben.
            return true;
        }
        
        /// <summary>
        /// Lädt alle Tabs nachdem das Fenster geladen ist.
        /// </summary>
        /// <param name="sender">Das Fenster.</param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Den Index des aktiven Tabs einer Variablen zuweisen.
            var ausgewählt = tabControl.SelectedIndex;

            // Sicherstellen, dass für die Viewport3D-Elemente in allen Tabs die Größe geladen ist.
            foreach (var item in tabControl.Items)
            {
                // Den Tab anwählen und laden.
                ((TabItem)item).IsSelected = true;
                ((TabItem)item).UpdateLayout();
            }

            // Den Index des ursprünglich aktiven Tabs an die TabControl geben.
            tabControl.SelectedIndex = ausgewählt;
        }

        /// <summary>
        /// Passt die Spaltenbreite an wenn die Größe des Grids verändert wird.
        /// </summary>
        /// <param name="sender">Das Grid das die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Wenn die Breite des Grids kleiner als 1112 ist sollen alle Spalten gleich groß sein.
            if (((Grid)sender).ActualWidth < 1112)
            {
                ((Grid)sender).ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                ((Grid)sender).ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                ((Grid)sender).ColumnDefinitions[4].Width = new GridLength(370.0);
            }
            // Ansonsten sollen die beiden linken Spalten auf 370 bleiben.
            else
            {
                ((Grid)sender).ColumnDefinitions[0].Width = new GridLength(370.0);
                ((Grid)sender).ColumnDefinitions[2].Width = new GridLength(370.0);
                ((Grid)sender).ColumnDefinitions[4].Width = new GridLength(1, GridUnitType.Star);
            }
        }

        /// <summary>
        /// Stellt die Breite der Spalte auf der die Maus ist breiter wenn die Breite des Grids kleiner als 1112 ist.
        /// </summary>
        /// <param name="sender">Die Spalte die die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void Spalte_MouseEnter(object sender, MouseEventArgs e)
        {
            // Wenn der sender ein ScrollViewer ist.
            if (sender.GetType() == typeof(ScrollViewer))
            {
                // Wenn die Breite des Grids größer gleich 1112 ist return.
                if (((Grid)((ScrollViewer)sender).Parent).ActualWidth >= 1112)
                    return;

                // Für alle Columndefinitions der anderen Spalten die Breite auf Stern setzen und für die des senders auf 370.
                foreach (var columnDefinition in ((Grid)((ScrollViewer)sender).Parent).ColumnDefinitions)
                {
                    if (columnDefinition == ((Grid)((ScrollViewer)sender).Parent).ColumnDefinitions[1] || columnDefinition == ((Grid)((ScrollViewer)sender).Parent).ColumnDefinitions[3])
                        continue;

                    if (!(columnDefinition == ((Grid)((ScrollViewer)sender).Parent).ColumnDefinitions[Grid.GetColumn(((ScrollViewer)sender))]))
                        columnDefinition.Width = new GridLength(1, GridUnitType.Star);
                    else
                        columnDefinition.Width = new GridLength(370.0);
                }
            }

            // Wenn der sender ein Grid ist.
            if (sender.GetType() == typeof(Grid))
            {
                // Wenn die Breite des Grids größer gleich 1112 ist return.
                if (((Grid)((Grid)sender).Parent).ActualWidth >= 1112)
                    return;

                // Für alle Columndefinitions der anderen Spalten die Breite auf Stern setzen und für die des senders auf 370.
                foreach (var columnDefinition in ((Grid)((Grid)sender).Parent).ColumnDefinitions)
                {
                    if (columnDefinition == ((Grid)((Grid)sender).Parent).ColumnDefinitions[1] || columnDefinition == ((Grid)((Grid)sender).Parent).ColumnDefinitions[3])
                        continue;

                    if (!(columnDefinition == ((Grid)((Grid)sender).Parent).ColumnDefinitions[Grid.GetColumn(((Grid)sender))]))
                        columnDefinition.Width = new GridLength(1, GridUnitType.Star);
                    else
                        columnDefinition.Width = new GridLength(370.0);
                }
            }
        }

        #region Grafik

        /// <summary>
        /// Passt die Größe der Grafik an die Größe des Viewport3D an.
        /// </summary>
        /// <param name="sender">Das Viewport3D das die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void Viewport3D_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Stellt sicher dass beim Initialisieren nicht durch null geteilt wird.
            if (e.PreviousSize.Width == 0 || e.PreviousSize.Height == 0)
                return;
            
            // Die Kamera des senders einer Variablen zuweisen.
            PerspectiveCamera kamera = (PerspectiveCamera)((Viewport3D)sender).Camera;
            
            // Das Verhältnis von der aktuellen Breite zur vorherigen Breite herausfinden.
            // Die Grafik soll bei Breitenänderung gleich groß bleiben.
            double verhältnisBreite = e.NewSize.Width / e.PreviousSize.Width;

            // Das Verhältnis von der vorherigen Höhe zur aktuellen Höhe herausfinden.
            // Die Grafik soll bei Höhenänderung größer oder kleiner werden.
            double verhältnisHöhe = e.PreviousSize.Height / e.NewSize.Height;

            // Einen 3D-Punkt für die aktuelle Position der Kamera festlegen.
            Point3D position = kamera.Position;

            // Die Position entsprechend ändern.
            position.X *= verhältnisBreite * verhältnisHöhe;
            position.Y *= verhältnisBreite * verhältnisHöhe;
            position.Z *= verhältnisBreite * verhältnisHöhe;
            
            // Die neue Position an die Kamera weitergeben.
            kamera.SetValue(PerspectiveCamera.PositionProperty, position);

            // Einen Vektor für die Blickrichtung der Kamera erstellen.
            Vector3D blickrichtung = new Vector3D(position.X * -1, position.Y * -1, position.Z * -1);

            // Die Blickrichtung der Kamera einstellen.
            kamera.SetValue(PerspectiveCamera.LookDirectionProperty, blickrichtung);
        }

        /// <summary>
        /// Steuert den Zoom durch verändern der Kameraposition und Blickrichtung beim Scrollen über der Grafik.
        /// </summary>
        /// <param name="kamera">Die Kamera der Grafik.</param>
        /// <param name="e"></param>
        private void MausRad(PerspectiveCamera kamera, double kameraWinkel, MouseWheelEventArgs e)
        {
            // Die aktuelle Position der Kamera in einem 3D-Punkt speichern.
            Point3D actualPosition = kamera.Position;

            // Die Länge der Entfernung von der Kamera zum Nullpunkt herausfinden.
            double entfernung = Math.Sqrt(Math.Pow(Math.Abs(kamera.Position.Z), 2) + Math.Pow(Math.Abs(kamera.Position.X) * Math.Sqrt(2), 2));

            // Wenn das Scrollrad nach oben bewegt wurde und die Position weit genug vom Nullpunkt weg ist die Position der Kamera an den Mittelpunkt annähern.
            if (e.Delta > 0 && entfernung > 2.5)
            {
                actualPosition.X = Math.Abs(Math.Cos(Calculate.DegreeToRadian(kameraWinkel)) * (entfernung - 0.7) / Math.Sqrt(2)) * -1;
                actualPosition.Y = Math.Abs(Math.Cos(Calculate.DegreeToRadian(kameraWinkel)) * (entfernung - 0.7) / Math.Sqrt(2)) * -1;
                actualPosition.Z = Math.Sin(Calculate.DegreeToRadian(kameraWinkel)) * (entfernung - 0.7);
            }

            // Wenn das Scrollrad nach unten bewegt wurde und die Position nah genug am Nullpunkt ist die Position der Kamera vom Mittelpunkt entfernen.
            if (e.Delta < 0 && entfernung < 20)
            {
                actualPosition.X = Math.Abs(Math.Cos(Calculate.DegreeToRadian(kameraWinkel)) * (entfernung + 0.7) / Math.Sqrt(2)) * -1;
                actualPosition.Y = Math.Abs(Math.Cos(Calculate.DegreeToRadian(kameraWinkel)) * (entfernung + 0.7) / Math.Sqrt(2)) * -1;
                actualPosition.Z = Math.Sin(Calculate.DegreeToRadian(kameraWinkel)) * (entfernung + 0.7);
            }

            // Die neue Position der Kamera zuweisen.
            kamera.SetValue(PerspectiveCamera.PositionProperty, actualPosition);

            // Einen Vektor für die Blickrichtung der Kamera erstellen.
            Vector3D blickrichtung = new Vector3D(actualPosition.X * -1, actualPosition.Y * -1, actualPosition.Z * -1);

            // Die Blickrichtung der Kamera einstellen.
            kamera.SetValue(PerspectiveCamera.LookDirectionProperty, blickrichtung);
        }

        /// <summary>
        /// Speichert die Mausposition wenn die linke Maustaste gedrückt wurde.
        /// </summary>
        /// <param name="sender">Das Viewport3D das die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mausPosition = e.GetPosition((Grid)sender);
        }

        /// <summary>
        /// Dreht das 3D-Modell bei Bewegung der Maus in die entsprechende Richtung.
        /// </summary>
        /// <param name="rotation">Die Rotation die verändert werden soll.</param>
        /// <param name="sender">Das Grid das die Methode ausgelöst hat.</param>
        /// <param name="e"></param>
        private void MausBewegung(AxisAngleRotation3D rotation, ref double kameraWinkel, PerspectiveCamera kamera, object sender, MouseEventArgs e)
        {
            // Nur wenn die linke Maustaste gedrückt ist weiter machen.
            if (e.LeftButton == MouseButtonState.Released)
                return;

            // Die neue Mausposition speichern.
            Point neueMausPosition = e.GetPosition((Grid)sender);

            // Die Sensitivität der Mausbewegung in einer Variablen speichern.
            var sensitivität = 0.7;

            // Die Länge der Bewegung nach links oder rechts einer Variablen zuweisen.
            var versatzLinksRechts = neueMausPosition.X - mausPosition.X;

            // Wenn die Maus nach links oder rechts bewegt wurde den Winkel der Rotation anpassen.
            if (versatzLinksRechts > 0)
                rotation.Angle += versatzLinksRechts * sensitivität;
            else if (versatzLinksRechts < 0)
                rotation.Angle += versatzLinksRechts * sensitivität;

            // Die Länge der Bewegung nach oben oder unten einer Variablen zuweisen.
            var versatzObenUnten = neueMausPosition.Y - mausPosition.Y;

            // Wenn die Maus nach oben oder unten bewegt wurde den Winkel der Kamera anpassen.
            if (versatzObenUnten > 0 && kameraWinkel < 90)
                kameraWinkel += versatzObenUnten * sensitivität;
            else if (versatzObenUnten < 0 && kameraWinkel > -90)
                kameraWinkel += versatzObenUnten * sensitivität;

            // Den Winkel der Kamera auf den Grenzwert setzen wenn dieser überschritten wurde.
            if (kameraWinkel < -90)
                kameraWinkel = -90;
            else if (kameraWinkel > 90)
                kameraWinkel = 90;
            
            // Die Länge der Entfernung von der Kamera zum Nullpunkt herausfinden.
            double entfernung = Math.Sqrt(Math.Pow(Math.Abs(kamera.Position.Z), 2) + Math.Pow(Math.Abs(kamera.Position.X) * Math.Sqrt(2), 2));

            // Variable für die neue Kameraposition festlegen.
            Point3D kameraPositionNeu = new Point3D
            {
                // Die neuen Werte für die Position berechnen.
                Z = Math.Sin(Calculate.DegreeToRadian(kameraWinkel)) * entfernung,
                X = Math.Abs(Math.Cos(Calculate.DegreeToRadian(kameraWinkel)) * entfernung / Math.Sqrt(2)) * -1,
                Y = Math.Abs(Math.Cos(Calculate.DegreeToRadian(kameraWinkel)) * entfernung / Math.Sqrt(2)) * -1
            };

            // Die neue Position der Kamera zuweisen.
            kamera.Position = kameraPositionNeu;

            // Variable für die neue Blickrichtung der Kamera festlegen.
            Vector3D kameraBlickrichtungNeu = new Vector3D(kameraPositionNeu.X * -1, kameraPositionNeu.Y * -1, kameraPositionNeu.Z * -1);

            // Die neue Blickrichtung der Kamera zuweisen.
            kamera.LookDirection = kameraBlickrichtungNeu;

            // Die Mausposition auf die neue Mausposition setzen.
            mausPosition = neueMausPosition;
        }

        #endregion

        #endregion

        #region Private Helfer

        /// <summary>
        /// Rechnet einen Winkel um nach der Formel: 90° - Winkel.
        /// </summary>
        /// <param name="textBox">Das Eingabefeld.</param>
        /// <param name="textBlock">Das Ergebnisfeld.</param>
        private void Winkelumrechnung(TextBox textBox, TextBlock textBlock)
        {
            // Hilfsvariable erstellen.
            double x = 0;
            
            // Wenn das Eingabefeld leer ist.
            if (textBox.Text == "")
            {
                // Den Hintergrund des Eingabefeldes weiß setzen und das Ergebnisfeld leer setzen.
                textBox.Background = Brushes.White;
                textBlock.Text = "";
            }
            // Wenn man die Eingabe in eine Zahl umwandeln kann.
            else if (EingabeGültig(textBox, ref x))
            {
                // Den Hintergrund des Eingabefelds weiß setzen, die Berechnung durchführen und in das Ergebnisfeld einsetzen.
                textBox.Background = Brushes.White;
                textBlock.Text = Convert.ToString(90 - x) + "°";
            }
            // Sonst.
            else
            {
                // Den Hintergrund des Eingabefelds rot setzen und das Ergebnisfeld leer setzen.
                textBox.Background = Brushes.Red;
                textBlock.Text = "";
            }
        }

        /// <summary>
        /// Überprüft ob der Hintergrund von einer der übergebenen TextBoxen rot ist.
        /// </summary>
        /// <param name="textBoxes">Das Array der TextBoxen.</param>
        /// <param name="start">Der Startindex in dem Array.</param>
        /// <param name="ende">Der Endindex in dem Array inklusive.</param>
        /// <returns>True wenn der Hintergrund einer TextBox rot ist.</returns>
        private bool TextBoxIstRot(TextBox[] textBoxes, int start, int ende)
        {
            // Die TextBoxen durchlaufen. 
            for (int i = start; i <= ende; i++)
            {
                // Prüfen ob der Hintergrund rot ist.
                if (textBoxes[i].Background == Brushes.Red)
                {
                    // True ausgeben.
                    return true;
                }
            }
            // False ausgeben.
            return false;
        }

        /// <summary>
        /// Überprüft ob der Text von einer der übergebenen TextBoxen leer ist.
        /// </summary>
        /// <param name="textBoxes">Das Array der TextBoxen.</param>
        /// <param name="start">Der Startindex in dem Array.</param>
        /// <param name="ende">Der Endindex in dem Array inklusive.</param>
        /// <returns>True wenn der Text einer TextBox leer ist.</returns>
        private bool TextBoxIstLeer(TextBox[] textBoxes, int start, int ende)
        {
            // Die TextBoxen durchlaufen. 
            for (int i = start; i <= ende; i++)
            {
                // Prüfen ob der Text leer ist.
                if (textBoxes[i].Text == "")
                {
                    // True ausgeben.
                    return true;
                }
            }
            // False ausgeben.
            return false;
        }

        /// <summary>
        /// Setzt den Hintergrund einer TextBox weiß wenn die Eingabe gültig und größer als oder gleich Null ist.
        /// </summary>
        /// <param name="textBox">Die TextBox.</param>
        private void WeißWennGültig(TextBox textBox)
        {
            double x = 0;

            if (EingabeGültig(textBox, ref x) && x >= 0)
            {
                textBox.Background = Brushes.White;
            }
        }

        /// <summary>
        /// Prüft ob der Text einer Textbox Eingaben enthält die ein Problem bei der Konvertierung in eine Zahl machen könnten.
        /// </summary>
        /// <param name="textBox">Die TextBox deren Text geprüft werden soll.</param>
        /// <returns>True wenn keine problematische Eingabe vorhanden ist.</returns>
        private bool TextGültig(TextBox textBox)
        {
            // Wenn der Text eine problematische Eingabe enthält false zurückgeben.
            if (textBox.Text.Contains(".") || textBox.Text.Contains("NaN"))
                return false;

            // Ansonsten true zurück geben.
            return true;
        }


        #endregion
    }
}
