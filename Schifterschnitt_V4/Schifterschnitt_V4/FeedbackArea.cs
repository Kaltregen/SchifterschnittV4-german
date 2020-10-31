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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Schifterschnitt
{
    /// <summary>
    /// Controls the feedback area.
    /// </summary>
    public class FeedbackArea
    {
        #region Reports

        /// <summary>
        /// Shows that the user needs to enter values.
        /// </summary>
        public Report EnterValues { get; set; } = new Report();

        /// <summary>
        /// Shows that the calculation ended successfully.
        /// </summary>
        public Report Calculated { get; set; } = new Report();

        /// <summary>
        /// Shows that input values have been changed.
        /// </summary>
        public Report InputChanged { get; set; } = new Report();

        /// <summary>
        /// Shows that the calculation of angle alpha ended successfully.
        /// </summary>
        public Report AlphaCalculated { get; set; } = new Report();

        /// <summary>
        /// Shows that the values of function angle alpha changed.
        /// </summary>
        public Report AlphaChanged { get; set; } = new Report();

        /// <summary>
        /// Shows that the calculation of tilt angle ended successfully.
        /// </summary>
        public Report TiltAngleCalculated { get; set; } = new Report();

        /// <summary>
        /// Shows that the values of function tilt angle changed.
        /// </summary>
        public Report TiltAngleChanged { get; set; } = new Report();

        /// <summary>
        /// Shows that the calculation of angle beta ended successfully.
        /// </summary>
        public Report BetaCalculated { get; set; } = new Report();

        /// <summary>
        /// Shows that the values of function angle beta changed.
        /// </summary>
        public Report BetaChanged { get; set; } = new Report();

        /// <summary>
        /// Shows that the values are invalid.
        /// </summary>
        public Report InvalidValues { get; set; } = new Report();

        /// <summary>
        /// Shows that the values of function line xy are invalid.
        /// </summary>
        public Report LineXYInvalidValues { get; set; } = new Report();

        /// <summary>
        /// Shows that there are to many values in function line xy.
        /// </summary>
        public Report LineXYToManyValues { get; set; } = new Report();

        /// <summary>
        /// Shows that in tab pyramid with tilt angle there is a negative tilt angle but no height.
        /// </summary>
        public Report HeightNeeded { get; set; } = new Report();

        /// <summary>
        /// Shows that the height of the pyramid is larger than the resulting one.
        /// </summary>
        public Report HeightLargerThanResulting { get; set; } = new Report();

        #endregion

        #region Feedback area

        /// <summary>
        /// The Feedback area.
        /// </summary>
        private Grid Area;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new feedback area.
        /// </summary>
        public FeedbackArea(Grid grid)
        {
            Area = grid;

            EnterValues.Text = "Eingabewerte eingeben \n Auf Berechnen klicken";
            EnterValues.BackgroundColor = Brushes.White;

            Calculated.Text = "Berechnet";
            Calculated.BackgroundColor = Brushes.Green;

            InputChanged.Text = "Eingabewerte geändert";
            InputChanged.BackgroundColor = Brushes.Yellow;

            AlphaCalculated.Text = "Alpha Berechnen \n Berechnet";
            AlphaCalculated.BackgroundColor = Brushes.Green;

            AlphaChanged.Text = "Alpha Berechnen \n Eingabewerte geändert";
            AlphaChanged.BackgroundColor = Brushes.Yellow;

            TiltAngleCalculated.Text = "Neigungswinkel Berechnen \n Berechnet";
            TiltAngleCalculated.BackgroundColor = Brushes.Green;

            TiltAngleChanged.Text = "Neigungswinkel Berechnen \n Eingabewerte geändert";
            TiltAngleChanged.BackgroundColor = Brushes.Yellow;

            BetaCalculated.Text = "Beta Berechnen \n Berechnet";
            BetaCalculated.BackgroundColor = Brushes.Green;

            BetaChanged.Text = "Beta Berechnen \n Eingabewerte geändert";
            BetaChanged.BackgroundColor = Brushes.Yellow;

            InvalidValues.Text = "Ungültige Eingabewerte";
            InvalidValues.BackgroundColor = Brushes.Red;

            LineXYInvalidValues.Text = "Linie X / Y \n Ungültige Werte";
            LineXYInvalidValues.BackgroundColor = Brushes.Red;

            LineXYToManyValues.Text = "Linie X / Y \n Zu viele Eingaben";
            LineXYToManyValues.BackgroundColor = Brushes.Red;

            HeightNeeded.Text = "Negativer Neigungswinkel \n Höhe erforderlich";
            HeightNeeded.BackgroundColor = Brushes.Red;

            HeightLargerThanResulting.Text = "Höhe größer als \n die sich ergebende";
            HeightLargerThanResulting.BackgroundColor = Brushes.Red;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears the feedback area and builds the new reports.
        /// </summary>
        private void Update()
        {
            Area.Children.Clear();
            Area.ColumnDefinitions.Clear();
            Area.Background = Brushes.White;

            var reports = new Report[] { 
                EnterValues, 
                Calculated, 
                InputChanged, 
                AlphaCalculated, 
                AlphaChanged, 
                TiltAngleCalculated, 
                TiltAngleChanged,
                BetaCalculated, 
                BetaChanged, 
                InvalidValues, 
                LineXYInvalidValues, 
                LineXYToManyValues, 
                HeightNeeded, 
                HeightLargerThanResulting 
            };

            var column = 0;

            foreach (var report in reports)
            {
                if (report.Active == false)
                    continue;

                var columnDefiniton = new ColumnDefinition();
                columnDefiniton.Width = new GridLength(1, GridUnitType.Star);
                Area.ColumnDefinitions.Add(columnDefiniton);

                var subarea = new Grid();
                subarea.Background = report.BackgroundColor;
                subarea.SetValue(Grid.ColumnProperty, column);
                Area.Children.Add(subarea);

                var textblock = new TextBlock();
                textblock.SetValue(TextBlock.TextProperty, report.Text);
                textblock.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
                textblock.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                textblock.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
                subarea.Children.Add(textblock);

                column++;
            }
        }

        /// <summary>
        /// Activates reports.
        /// </summary>
        /// <param name="reports">The reports to be activated.</param>
        public void Activate(params Report[] reports)
        {
            foreach (var report in reports)
                report.Active = true;

            Update();
        }

        /// <summary>
        /// Deactivates reports.
        /// </summary>
        /// <param name="reports">The reports to be deactivated.</param>
        public void Deactivate(params Report[] reports)
        {
            foreach (var report in reports)
                report.Active = false;

            Update();
        }

        #endregion
    }
}
