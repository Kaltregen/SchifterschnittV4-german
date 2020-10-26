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

using System.Windows.Media;

namespace Schifterschnitt
{
    /// <summary>
    /// A report that can be shown in the feedback area.
    /// </summary>
    public class Report
    {
        #region Properties

        /// <summary>
        /// The background color of the report.
        /// </summary>
        public SolidColorBrush BackgroundColor { get; set; }

        /// <summary>
        /// The text the report is showing.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Sets a report as active or inactive.
        /// </summary>
        public bool Active { get; set; } = false;

        #endregion

        #region ctor

        /// <summary>
        /// Creates a new report.
        /// </summary>
        public Report()
        {
            BackgroundColor = Brushes.White;
            Text = "";
            Active = false;
        }

        #endregion
    }
}
