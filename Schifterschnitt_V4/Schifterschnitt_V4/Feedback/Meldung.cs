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

namespace Schifterschnitt.Feedback
{
    /// <summary>
    /// Eine Meldung die in der Feedbackleiste angezeigt werden kann.
    /// </summary>
    public class Meldung
    {
        #region Eigenschaften

        /// <summary>
        /// Die Hintergrundfarbe des Grid-Felds der Meldung.
        /// </summary>
        public SolidColorBrush Hintergrundfarbe { get; set; }

        /// <summary>
        /// Der Text der Meldung.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Bestimmt ob die Meldung aktiv ist.
        /// </summary>
        public bool Aktiv { get; set; } = false;

        #endregion

        #region ctor

        /// <summary>
        ///  Legt die Defaultwerte für die Eigenschaften fest.
        /// </summary>
        public Meldung()
        {
            // Defaultwerte für die Eigenschaften festlegen.
            Hintergrundfarbe = Brushes.White;
            Text = "";
            Aktiv = false;
        }

        #endregion
    }
}
