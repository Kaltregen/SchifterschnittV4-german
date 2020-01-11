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
