// Schifterschnitt_V4 - A program for joiners to calculate compound miters.
// Copyright (C) 2020 Michael Pütz

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Schifterschnitt.Feedback
{
    /// <summary>
    /// Steuert die Feedbackleiste.
    /// </summary>
    public class Feedbackleiste
    {
        #region Meldungen

        /// <summary>
        /// Zeigt an dass man Eingabewerte eingeben soll.
        /// </summary>
        public Meldung EingabewerteEingeben { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass die Berechnung erfolgreich ausgeführt wurde.
        /// </summary>
        public Meldung Berechnet { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass die Eingabewerte geändert wurden.
        /// </summary>
        public Meldung EingabeGeändert { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass die Berechnung bei der Funktion Winkel Alpha Berechnen erfolgreich ausgeführt wurde.
        /// </summary>
        public Meldung AlphaBerechnet { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass die Eingabewerte bei der Funktion Winkel Alpha Berechnen geändert wurden.
        /// </summary>
        public Meldung AlphaGeändert { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass die Berechnung bei der Funktion Neigungswinkel Berechnen erfolgreich ausgeführt wurde.
        /// </summary>
        public Meldung NeigungswinkelBerechnet { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass die Eingabewerte bei der Funktion Neigungswinkel Berechnen geändert wurden.
        /// </summary>
        public Meldung NeigungswinkelGeändert { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass die Berechnung bei der Funktion Winkel Beta Berechnen erfolgreich ausgeführt wurde.
        /// </summary>
        public Meldung BetaBerechnet { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass die Eingabewerte bei der Funktion Winkel Beta Berechnen geändert wurden.
        /// </summary>
        public Meldung BetaGeändert { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass Eingabewerte ungültig sind.
        /// </summary>
        public Meldung UngültigeWerte { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass die Eingabewerte bei der Funktion LinieXY ungültig sind.
        /// </summary>
        public Meldung LiniexyUngültigeWerte { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass es zu viele Eingaben bei der Funktion LinieXY gibt.
        /// </summary>
        public Meldung LiniexyZuVieleEingaben { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass bei der Pyramide mit Neigungswinkel bei negativem Neigungswinkel keine Höhe eingegeben wurde.
        /// </summary>
        public Meldung HöheErforderlich { get; set; } = new Meldung();

        /// <summary>
        /// Zeigt an dass die Höhe größer als die sich ergebende ist.
        /// </summary>
        public Meldung HöheGrößerAlsErgebend { get; set; } = new Meldung();

        #endregion

        #region Ausgabefeld

        /// <summary>
        /// Die Ausgabefläche für die Meldungen.
        /// </summary>
        private Grid Feld;

        #endregion

        #region ctor

        /// <summary>
        /// Konstruktor - Weist den Fehlermeldungen Eigenschaften zu.
        /// </summary>
        public Feedbackleiste(Grid grid)
        {
            // Das übergebene Grid zuweisen.
            Feld = grid;

            // Die Eigenschaften der Meldungen festlegen.
            EingabewerteEingeben.Text = "Eingabewerte eingeben \n Auf Berechnen klicken";
            EingabewerteEingeben.Hintergrundfarbe = Brushes.White;

            Berechnet.Text = "Berechnet";
            Berechnet.Hintergrundfarbe = Brushes.Green;

            EingabeGeändert.Text = "Eingabewerte geändert";
            EingabeGeändert.Hintergrundfarbe = Brushes.Yellow;

            AlphaBerechnet.Text = "Alpha Berechnen \n Berechnet";
            AlphaBerechnet.Hintergrundfarbe = Brushes.Green;

            AlphaGeändert.Text = "Alpha Berechnen \n Eingabewerte geändert";
            AlphaGeändert.Hintergrundfarbe = Brushes.Yellow;

            NeigungswinkelBerechnet.Text = "Neigungswinkel Berechnen \n Berechnet";
            NeigungswinkelBerechnet.Hintergrundfarbe = Brushes.Green;

            NeigungswinkelGeändert.Text = "Neigungswinkel Berechnen \n Eingabewerte geändert";
            NeigungswinkelGeändert.Hintergrundfarbe = Brushes.Yellow;

            BetaBerechnet.Text = "Beta Berechnen \n Berechnet";
            BetaBerechnet.Hintergrundfarbe = Brushes.Green;

            BetaGeändert.Text = "Beta Berechnen \n Eingabewerte geändert";
            BetaGeändert.Hintergrundfarbe = Brushes.Yellow;

            UngültigeWerte.Text = "Ungültige Eingabewerte";
            UngültigeWerte.Hintergrundfarbe = Brushes.Red;

            LiniexyUngültigeWerte.Text = "Linie X / Y \n Ungültige Werte";
            LiniexyUngültigeWerte.Hintergrundfarbe = Brushes.Red;

            LiniexyZuVieleEingaben.Text = "Linie X / Y \n Zu viele Eingaben";
            LiniexyZuVieleEingaben.Hintergrundfarbe = Brushes.Red;

            HöheErforderlich.Text = "Negativer Neigungswinkel \n Höhe erforderlich";
            HöheErforderlich.Hintergrundfarbe = Brushes.Red;

            HöheGrößerAlsErgebend.Text = "Höhe größer als \n die sich ergebende";
            HöheGrößerAlsErgebend.Hintergrundfarbe = Brushes.Red;
        }

        #endregion

        #region Methoden

        /// <summary>
        /// Baut eine neue Feedbackleiste mit den aktuellen Meldungen und ersetzt die alte.
        /// </summary>
        private void Update()
        {
            // Zurücksetzen des Grids auf einen leeren Zustand.
            Feld.Children.Clear();
            Feld.ColumnDefinitions.Clear();
            Feld.Background = Brushes.White;

            // Ein Array erstellen um die Meldungen in einer For-Schleife durchzugehen.
            Meldung[] vs = new Meldung[] { EingabewerteEingeben, Berechnet, EingabeGeändert, AlphaBerechnet, AlphaGeändert, NeigungswinkelBerechnet, NeigungswinkelGeändert,
                    BetaBerechnet, BetaGeändert, UngültigeWerte, LiniexyUngültigeWerte, LiniexyZuVieleEingaben, HöheErforderlich, HöheGrößerAlsErgebend };

            // Variable für den Spaltenindex anlegen.
            var spalte = 0;

            // Für jede Meldung eine Spalte und einen TextBlock anlegen.
            foreach (var m in vs)
            {
                // Wenn die Meldung aktiv ist.
                if (m.Aktiv == true)
                {
                    // Eine Spalte anlegen.
                    var c = new ColumnDefinition();
                    c.Width = new GridLength(1, GridUnitType.Star);
                    Feld.ColumnDefinitions.Add(c);

                    // In die Spalte ein Grid legen und ihm die Farbe der Meldung geben.
                    Grid unterfeld = new Grid();
                    unterfeld.Background = m.Hintergrundfarbe;
                    unterfeld.SetValue(Grid.ColumnProperty, spalte);
                    Feld.Children.Add(unterfeld);

                    // Einen TextBlock anlegen, in die richtige Spalte setzen, ausrichten und den Meldungstext zuweisen.
                    var t = new TextBlock();
                    t.SetValue(TextBlock.TextProperty, m.Text);
                    t.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                    t.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                    t.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                    unterfeld.Children.Add(t);

                    // Den Spaltenindex erhöhen.
                    spalte++;
                }
            }
        }

        /// <summary>
        /// Aktiviert alle übergebenen Meldungen.
        /// </summary>
        /// <param name="meldungen">Die Meldungen die aktiviert werden sollen.</param>
        public void Aktivieren(params Meldung[] meldungen)
        {
            // Alle übergebenen Meldungen aktivieren.
            foreach (var item in meldungen)
            {
                item.Aktiv = true;
            }

            // Die Feedbackleiste aktualisieren.
            Update();
        }

        /// <summary>
        /// Deaktiviert alle übergebenen Meldungen.
        /// </summary>
        /// <param name="meldungen">Die Meldungen die deaktivert werden sollen.</param>
        public void Deaktivieren(params Meldung[] meldungen)
        {
            // Alle übergebenen Meldungen deaktivieren.
            foreach (var item in meldungen)
            {
                item.Aktiv = false;
            }

            // Die Feedbackleiste aktualisieren.
            Update();
        }

        #endregion
    }
}
