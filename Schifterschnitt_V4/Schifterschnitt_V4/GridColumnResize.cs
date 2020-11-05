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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Schifterschnitt
{
    /// <summary>
    /// Allows resizing of the columns of a grid in a special way.
    /// </summary>
    class GridColumnResize
    {
        #region Fields

        /// <summary>
        /// The normal widths of the columns.
        /// </summary>
        private readonly List<double> columnWidths;

        /// <summary>
        /// The total width of the grid.
        /// </summary>
        private readonly double widthsSum = 0;

        #endregion

        #region ctor

        /// <summary>
        /// Create a new instance.
        /// </summary>
        /// <param name="widths">The normal widths of the columns.</param>
        public GridColumnResize(List<double> widths)
        {
            columnWidths = widths;

            foreach (var width in columnWidths)
                widthsSum += width;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Changes the column sizes if the window is bigger than normal so that only the last column expands.
        /// For use with a GridSizeChanged event.
        /// </summary>
        /// <param name="sender">The grid that contains the columns to change.</param>
        /// <param name="e">The SizeChangedEventArgs.</param>
        public void ExpandMostRightIfBigger(object sender, SizeChangedEventArgs e)
        {
            var senderGrid = (Grid)sender;
            int numberOfColumns = senderGrid.ColumnDefinitions.Count;

            if (senderGrid.ActualWidth < widthsSum)
            {
                for (int i = 0; i < numberOfColumns; i += 2)
                {
                    if (i == numberOfColumns - 1)
                    {
                        senderGrid.ColumnDefinitions[i].Width = new GridLength(columnWidths[i]);
                        break;
                    }
                    senderGrid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Star);
                }
            }
            else
            {
                for (int i = 0; i < numberOfColumns; i += 2)
                {
                    if (i == numberOfColumns - 1)
                    {
                        senderGrid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Star);
                        break;
                    }
                    senderGrid.ColumnDefinitions[i].Width = new GridLength(columnWidths[i]);
                }
            }
        }

        /// <summary>
        /// Changes the column sizes if the window is smaller than normal so the column the mouse is over is shown fully.
        /// For use with a MouseMove event.
        /// </summary>
        /// <param name="sender">The grid that contains the columns to change.</param>
        /// <param name="e">The MouseEventArgs.</param>
        public void ShowFullyIfSmaller(object sender, MouseEventArgs e)
        {
            var senderGrid = (Grid)sender;

            if (senderGrid.ActualWidth >= widthsSum)
                return;

            double positionX = e.GetPosition(senderGrid).X;
            int numberOfColumns = senderGrid.ColumnDefinitions.Count;
            var currentColumn = new ColumnDefinition();

            for (int i = 0; i < numberOfColumns; i += 2)
            {
                double leftStop = 0;

                for (int l = 0; l < i; l++)
                    leftStop += senderGrid.ColumnDefinitions[l].ActualWidth;

                double rightStop = leftStop + senderGrid.ColumnDefinitions[i].ActualWidth;

                if ((positionX >= leftStop) && (positionX <= rightStop))
                {
                    currentColumn = senderGrid.ColumnDefinitions[i];
                    break;
                }
            }

            for (int i = 0; i < numberOfColumns; i += 2)
            {
                var column = senderGrid.ColumnDefinitions[i];
                if (column == currentColumn)
                    column.Width = new GridLength(columnWidths[i]);
                else
                    column.Width = new GridLength(1, GridUnitType.Star);
            }
        }

        #endregion
    }
}
