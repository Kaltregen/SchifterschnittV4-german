using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace Schifterschnitt.Objekt
{
    /// <summary>
    /// Eine Ecke oder Pyramide mit Schifterschnitt mit Methoden zum Berechnen des Schifterschnitts und zum Erzeugen eines 3D-Modells.
    /// </summary>
    public abstract class SchifterObjekt
    {
        #region Eigenschaften

        /// <summary>
        /// Die Höhe der Ecke oder Pyramide.
        /// </summary>
        public double Hoehe { get; set; }

        /// <summary>
        /// Die Materialstärke des ersten Brettes.
        /// </summary>
        public double MaterialstaerkeEins { get; set; }

        /// <summary>
        /// Die Materialstärke des zweiten Brettes.
        /// </summary>
        public double MaterialstaerkeZwei { get; set; }

        /// <summary>
        /// Der Neigungswinkel des ersten Brettes.
        /// </summary>
        public double WinkelAlphaEins { get; set; }

        /// <summary>
        /// Der Neigungswinkel des zweiten Brettes.
        /// </summary>
        public double WinkelAlphaZwei { get; set; }

        /// <summary>
        /// Der Innenwinkel der Ecke oder Pyramide.
        /// </summary>
        public double WinkelBeta { get; set; }
        
        /// <summary>
        /// Gibt an ob eine Gehrung vorhanden ist.
        /// </summary>
        public bool Gehrung { get; set; }
        
        /// <summary>
        /// Der Winkel des Queranschlags beim ersten Brett.
        /// </summary>
        public double WinkelQueranschlagEins { get; set; }

        /// <summary>
        /// Der Winkel des Queranschlags beim zweiten Brett.
        /// </summary>
        public double WinkelQueranschlagZwei { get; set; }

        /// <summary>
        /// Der Winkel des Sägeblatts beim ersten Brett.
        /// </summary>
        public double WinkelSägeblattEins { get; set; }

        /// <summary>
        /// Der Winkel des Sägeblatts beim zweiten Brett.
        /// </summary>
        public double WinkelSägeblattZwei { get; set; }

        /// <summary>
        /// Die Breite des ersten Bretts ohne Schräge.
        /// </summary>
        public double BreiteEins { get; set; }

        /// <summary>
        /// Die Breite des zweiten Bretts ohne Schräge.
        /// </summary>
        public double BreiteZwei { get; set; }

        /// <summary>
        /// Die Breite des ersten Bretts mit Schräge.
        /// </summary>
        public double BreiteMitSchrägeEins { get; set; }

        /// <summary>
        /// Die Breite des zweiten Bretts mit Schräge.
        /// </summary>
        public double BreiteMitSchrägeZwei { get; set; }

        /// <summary>
        /// Der Flächenwinkel zwischen den Brettern.
        /// </summary>
        public double Flächenwinkel { get; set; }

        #endregion

        #region Variablen

        /// <summary>
        /// Das Material für die Grafik.
        /// </summary>
        DiffuseMaterial holzmaterial;

        /// <summary>
        /// Der ImageBrush für das Material.
        /// </summary>
        ImageBrush holz = new ImageBrush();

        #endregion

        #region ctor

        /// <summary>
        /// ctor
        /// </summary>
        public SchifterObjekt()
        {
            // Dem ImageBrush ein Bild zuweisen und diesen dem Holzmaterial zuweisen.
            holz.ImageSource = new BitmapImage(new Uri("Images/Holz.bmp", UriKind.Relative));
            holzmaterial = new DiffuseMaterial(holz);
        }

        #endregion

        #region Methoden

        /// <summary>
        /// Berechnet den Schifterschnitt.
        /// </summary>
        public void Berechnung()
        {
            double winkelAlphaEinsRadian = Rechne.DegreeToRadian(WinkelAlphaEins);
            double winkelAlphaZweiRadian = Rechne.DegreeToRadian(WinkelAlphaZwei);
            double winkelBetaRadian = Rechne.DegreeToRadian(WinkelBeta);

            Vector3D vektorEins = new Vector3D()
            {
                X = Math.Sin(winkelBetaRadian - Rechne.DegreeToRadian(90)) * Math.Tan(winkelAlphaEinsRadian),
                Y = Math.Cos(winkelBetaRadian - Rechne.DegreeToRadian(90)) * Math.Tan(winkelAlphaEinsRadian)
            };

            Vector3D vektorZwei = new Vector3D()
            {
                X = Math.Tan(winkelAlphaZweiRadian)
            };

            Vector3D vektorDrei = new Vector3D()
            {
                X = Math.Tan(winkelAlphaZweiRadian),
                Y = Math.Cos(winkelBetaRadian - Rechne.DegreeToRadian(90)) * Math.Tan(winkelAlphaEinsRadian) -
                (Math.Tan(winkelBetaRadian - Rechne.DegreeToRadian(90)) * (Math.Tan(winkelAlphaZweiRadian) - Math.Sin(winkelBetaRadian -
                Rechne.DegreeToRadian(90)) * Math.Tan(winkelAlphaEinsRadian)))
            };
            
            // Berechnung der Queranschlagswinkel.
            double winkelQueranschlagEinsRadian = Math.Acos((vektorEins.X * vektorDrei.X + vektorEins.Y * vektorDrei.Y + 1) /
                (Math.Sqrt(Math.Pow(vektorEins.X, 2) + Math.Pow(vektorEins.Y, 2) + 1) * Math.Sqrt(Math.Pow(vektorDrei.X, 2) + Math.Pow(vektorDrei.Y, 2) + 1)));
            double winkelQueranschlagZweiRadian = Math.Acos((vektorZwei.X * vektorDrei.X + 1) / (Math.Sqrt(Math.Pow(vektorZwei.X, 2) + 1) *
                Math.Sqrt(Math.Pow(vektorDrei.X, 2) + Math.Pow(vektorDrei.Y, 2) + 1)));
            
            // Setzt den Queranschlagswinkel des Teil zwei wenn nötig ins negative.
            if (vektorDrei.Y < 0)
                winkelQueranschlagZweiRadian *= -1;

            // Setzt den Queranschlagswinkel des Teil eins wenn nötig ins negative.
            if (360 - WinkelBeta - 180 <= 90)
            {
                // Vektor 3 Oben Links immer negativ
                if (vektorDrei.X < 0 && vektorDrei.Y >= 0)
                    winkelQueranschlagEinsRadian *= -1;

                // Vektor 3 Oben Rechts
                if (vektorDrei.X >= 0 && vektorDrei.Y >= 0 && (vektorDrei.Y / vektorDrei.X) > (vektorEins.Y / vektorEins.X))
                    winkelQueranschlagEinsRadian *= -1;

                // Vektor 3 Unten Rechts immer positiv

                // Vektor 3 Unten Links
                if (vektorDrei.X < 0 && vektorDrei.Y < 0 && (Math.Abs(vektorDrei.Y) / Math.Abs(vektorDrei.X)) < (vektorEins.Y / vektorEins.X))
                    winkelQueranschlagEinsRadian *= -1;
            }
            else
            {
                // Vektor 3 Oben Links
                if (vektorDrei.X < 0 && vektorDrei.Y >= 0 && (Math.Abs(vektorDrei.Y) / Math.Abs(vektorDrei.X)) < (Math.Abs(vektorEins.Y) / Math.Abs(vektorEins.X)))
                    winkelQueranschlagEinsRadian *= -1;

                // Vektor 3 Oben Rechts immer positiv

                // Vektor 3 Unten Rechts
                if (vektorDrei.X >= 0 && vektorDrei.Y < 0 && (Math.Abs(vektorDrei.Y) / Math.Abs(vektorDrei.X)) > (Math.Abs(vektorEins.Y) / Math.Abs(vektorEins.X)))
                    winkelQueranschlagEinsRadian *= -1;

                // Vektor 3 Unten Links immer negativ
                if (vektorDrei.X < 0 && vektorDrei.Y < 0)
                    winkelQueranschlagEinsRadian *= -1;
            }

            // Setzt die Queranschlagswinkel auf Null wenn sie keine Zahl sind.
            if (double.IsNaN(WinkelQueranschlagEins))
                winkelQueranschlagEinsRadian = 0;

            if (double.IsNaN(WinkelQueranschlagZwei))
                winkelQueranschlagZweiRadian = 0;

            // Zuweisung der Queranschlagswinkel zu den Eigenschaften.
            WinkelQueranschlagEins = Rechne.RadianToDegree(winkelQueranschlagEinsRadian);
            WinkelQueranschlagZwei = Rechne.RadianToDegree(winkelQueranschlagZweiRadian);

            // Berechnung der Werte der Vektoren für den Flächenwinkel.
            Vector3D vektorVier = new Vector3D();
            Vector3D vektorFuenf = new Vector3D();

            vektorVier.Z = Math.Cos(winkelAlphaEinsRadian) * Math.Sin(winkelQueranschlagEinsRadian);

            var winkelBodenlinieRadian = Math.Atan(Math.Sin(winkelAlphaEinsRadian) * Math.Sin(winkelQueranschlagEinsRadian) / Math.Cos(winkelQueranschlagEinsRadian));
            var bodenlinie = Math.Cos(winkelQueranschlagEinsRadian) / Math.Cos(winkelBodenlinieRadian);

            vektorVier.X = -1 * Math.Cos(Rechne.DegreeToRadian(WinkelBeta - 90 + Rechne.RadianToDegree(winkelBodenlinieRadian))) * bodenlinie;
            vektorVier.Y = Math.Sin(Rechne.DegreeToRadian(WinkelBeta - 90 + Rechne.RadianToDegree(winkelBodenlinieRadian))) * bodenlinie;

            vektorFuenf.Z = Math.Cos(winkelAlphaZweiRadian) * Math.Sin(winkelQueranschlagZweiRadian);
            vektorFuenf.X = Math.Sin(winkelAlphaZweiRadian) * Math.Sin(winkelQueranschlagZweiRadian);
            vektorFuenf.Y = -1 * Math.Cos(winkelQueranschlagZweiRadian);

            // Berechnung des Flächenwinkels.
            Flächenwinkel = Vector3D.AngleBetween(vektorVier, vektorFuenf);

            // Setzt den Flächenwinkel auf 90° wenn er keine Zahl ist.
            if (System.Double.IsNaN(Flächenwinkel))
                Flächenwinkel = 90;

            // Berechnung der Sägeblattwinkel.
            double linie = Math.Sqrt(Math.Pow(MaterialstaerkeEins, 2) + Math.Pow(MaterialstaerkeZwei, 2) - 2 * MaterialstaerkeEins * MaterialstaerkeZwei *
                Math.Cos(Rechne.DegreeToRadian(360 - Flächenwinkel - 180)));
            double winkelgruenEins = Rechne.RadianToDegree(Math.Acos((Math.Pow(linie, 2) + Math.Pow(MaterialstaerkeEins, 2) - Math.Pow(MaterialstaerkeZwei, 2)) /
                (2 * linie * MaterialstaerkeEins)));
            double winkelgruenZwei = Rechne.RadianToDegree(Math.Acos((Math.Pow(linie, 2) + Math.Pow(MaterialstaerkeZwei, 2) - Math.Pow(MaterialstaerkeEins, 2)) /
                (2 * linie * MaterialstaerkeZwei)));
            double winkelgelbEins = 0;
            double winkelgelbZwei = 0;

            if (winkelgruenZwei > 90)
                winkelgelbEins = 90 + winkelgruenEins;
            else
                winkelgelbEins = 90 - winkelgruenEins;

            if (winkelgruenEins > 90)
                winkelgelbZwei = 90 + winkelgruenZwei;
            else
                winkelgelbZwei = 90 - winkelgruenZwei;

            winkelgelbEins = Math.Abs(winkelgelbEins);
            winkelgelbZwei = Math.Abs(winkelgelbZwei);

            WinkelSägeblattEins = Rechne.RadianToDegree(Math.Atan(linie / Math.Sin(Rechne.DegreeToRadian(180 - winkelgelbEins - winkelgelbZwei)) *
                Math.Sin(Rechne.DegreeToRadian(winkelgelbZwei)) / MaterialstaerkeEins));
            WinkelSägeblattZwei = Rechne.RadianToDegree(Math.Atan(linie / Math.Sin(Rechne.DegreeToRadian(180 - winkelgelbEins - winkelgelbZwei)) *
                Math.Sin(Rechne.DegreeToRadian(winkelgelbEins)) / MaterialstaerkeZwei));

            // Setzt die Sägeblattwinkel auf Null wenn sie keine Zahl sind.
            if (System.Double.IsNaN(WinkelSägeblattEins))
                WinkelSägeblattEins = 0;

            if (System.Double.IsNaN(WinkelSägeblattZwei))
                WinkelSägeblattZwei = 0;

            // Setzt die Sägeblattwinkel ins negative wenn nötig.
            if (winkelgruenEins > 90)
                WinkelSägeblattZwei *= -1;

            if (winkelgruenZwei > 90)
                WinkelSägeblattEins *= -1;

            // Wenn keine Gehrung geschnitten werden soll.
            if (!Gehrung)
            {
                // Die Sägeblattwinkel anpassen.
                WinkelSägeblattEins = (90 - (360 - Flächenwinkel - 180)) * -1;
                WinkelSägeblattZwei = (90 - (360 - Flächenwinkel - 180)) * -1;
            }

            // Berechnet die Breiten der Teile und weist sie der Eigenschaft zu.
            BreiteEins = Hoehe / Math.Cos(winkelAlphaEinsRadian);
            BreiteZwei = Hoehe / Math.Cos(winkelAlphaZweiRadian);

            // Berechnet die Breiten mit Schräge der Teile und weist sie der Eigenschaft zu.
            BreiteMitSchrägeEins = BreiteEins + Math.Abs(Math.Tan(winkelAlphaEinsRadian)) * MaterialstaerkeEins;
            BreiteMitSchrägeZwei = BreiteZwei + Math.Abs(Math.Tan(winkelAlphaZweiRadian)) * MaterialstaerkeZwei;
        }

        /// <summary>
        /// Erzeugt ein 3D-Modell des Objekts.
        /// </summary>
        public abstract void ModellErzeugen(ModelVisual3D modell);

        /// <summary>
        /// Erstellt ein Viereck und weist ihm ein Material mit der Holztextur zu.
        /// </summary>
        /// <param name="punktEins">Der erste Punkt des Vierecks.</param>
        /// <param name="punktZwei">Der zweite Punkt des Vierecks.</param>
        /// <param name="punktDrei">Der dritte Punkt des Vierecks.</param>
        /// <param name="punktVier">Der vierte Punkt des Vierecks.</param>
        /// <returns>Ein Viereck als Geometrie.</returns>
        public GeometryModel3D Viereck(Point3D punktEins, Point3D punktZwei, Point3D punktDrei, Point3D punktVier)
        {
            MeshGeometry3D m = new MeshGeometry3D();

            m.Positions.Add(punktEins);
            m.Positions.Add(punktZwei);
            m.Positions.Add(punktDrei);
            m.Positions.Add(punktVier);

            m.TextureCoordinates.Add(new Point(0, 0));
            m.TextureCoordinates.Add(new Point(1, 0));
            m.TextureCoordinates.Add(new Point(0, 1));

            m.TriangleIndices.Add(0);
            m.TriangleIndices.Add(1);
            m.TriangleIndices.Add(2);

            m.TextureCoordinates.Add(new Point(1, 1));
            m.TextureCoordinates.Add(new Point(1, 0));
            m.TextureCoordinates.Add(new Point(0, 1));

            m.TriangleIndices.Add(2);
            m.TriangleIndices.Add(3);
            m.TriangleIndices.Add(0);

            return new GeometryModel3D(m, holzmaterial);
        }

        /// <summary>
        /// Erstellt ein Dreieck und weist ihm ein Material mit der Holztextur zu.
        /// </summary>
        /// <param name="punktEins">Der erste Punkt des Dreiecks.</param>
        /// <param name="punktZwei">Der zweite Punkt des Dreiecks.</param>
        /// <param name="punktDrei">Der dritte Punkt des Dreiecks.</param>
        /// <returns>Ein Dreieck als Geometrie.</returns>
        public GeometryModel3D Dreieck(Point3D punktEins, Point3D punktZwei, Point3D punktDrei)
        {
            MeshGeometry3D m = new MeshGeometry3D();

            m.Positions.Add(punktEins);
            m.Positions.Add(punktZwei);
            m.Positions.Add(punktDrei);

            m.TextureCoordinates.Add(new Point(0, 0));
            m.TextureCoordinates.Add(new Point(1, 0));
            m.TextureCoordinates.Add(new Point(0, 1));

            m.TriangleIndices.Add(0);
            m.TriangleIndices.Add(1);
            m.TriangleIndices.Add(2);

            return new GeometryModel3D(m, holzmaterial);
        }

        #endregion
    }
}
