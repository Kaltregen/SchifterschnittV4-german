using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Schifterschnitt.Objekt
{
    /// <summary>
    /// Eine Pyramide mit Schifterschnitt.
    /// </summary>
    class Pyramide : SchifterObjekt
    {
        #region Eigenschaften

        /// <summary>
        /// Die Grundlinie bei einer Pyramide.
        /// </summary>
        public double Grundlinie { get; set; }

        /// <summary>
        /// Die Oberlinie bei einer Pyramide.
        /// </summary>
        public double Oberlinie { get; set; }

        /// <summary>
        /// Die Anzahl der Seiten bei einer Pyramide.
        /// </summary>
        public short AnzahlSeiten { get; set; }

        #endregion

        #region Methoden

        /// <summary>
        /// Erzeugt ein 3D-Modell einer Pyramide.
        /// </summary>
        /// <param name="modell">Das 3D-Modell in dem die Pyramide gebaut werden soll.</param>
        public override void ModellErzeugen(ModelVisual3D modell)
        {
            double schrägeS = MaterialstaerkeEins / Math.Cos(Rechne.DegreeToRadian(WinkelAlphaEins));

            // Berechnet die Radien der Umkreise und fügt sie einem Array hinzu.
            double umkreisradiusUnten = Rechne.Umkreis(Grundlinie, AnzahlSeiten);
            double umkreisradiusOben = Rechne.Umkreis(Oberlinie, AnzahlSeiten);
            double umkreisradiusInnenUnten = Rechne.Umkreis(Grundlinie, AnzahlSeiten) - (schrägeS / Math.Sin(Rechne.DegreeToRadian((AnzahlSeiten - 2.0) / AnzahlSeiten * 180.0 / 2)));
            double umkreisradiusInnenOben = Rechne.Umkreis(Oberlinie, AnzahlSeiten) - (schrägeS / Math.Sin(Rechne.DegreeToRadian((AnzahlSeiten - 2.0) / AnzahlSeiten * 180.0 / 2)));
            double[] umkreise = { umkreisradiusUnten, umkreisradiusOben, umkreisradiusInnenUnten, umkreisradiusInnenOben };

            // Erstellt eine Referenzgröße.
            double referenz;

            // Die größte Größe in der Pyramide herausfinden und der Referenz zuweisen.
            if (Hoehe > umkreisradiusUnten && Hoehe > umkreisradiusOben)
                referenz = Hoehe;
            else if (umkreisradiusUnten > Hoehe && umkreisradiusUnten > umkreisradiusOben)
                referenz = umkreisradiusUnten;
            else if (umkreisradiusOben > Hoehe && umkreisradiusOben > umkreisradiusUnten)
                referenz = umkreisradiusOben;
            else
                referenz = Hoehe;

            // Erstellt Listen für 3D-Punkte und fügt sie einem Array hinzu.
            List<Point3D> punkteUnten = new List<Point3D>();
            List<Point3D> punkteOben = new List<Point3D>();
            List<Point3D> punkteInnenUnten = new List<Point3D>();
            List<Point3D> punkteInnenOben = new List<Point3D>();
            List<Point3D>[] punkteListen = { punkteUnten, punkteOben, punkteInnenUnten, punkteInnenOben };

            // Erstellt ein Array für die For-Schleife.
            double[] schleifenWerte = { Hoehe / referenz * -1, Hoehe / referenz, Hoehe / referenz * -1, Hoehe / referenz };

            // Berechnet die 3D-Punkte und fügt sie den Listen hinzu.
            double w = 360.0 / AnzahlSeiten / 2.0;
            double x;
            double y;

            for (int i = 0; i < 4; i++)
            {
                double item = umkreise[i];
                for (int j = 0; j < AnzahlSeiten; j++)
                {
                    if (w < 90)
                    {
                        x = -1 * Math.Sin(Rechne.DegreeToRadian(w)) * (item / referenz) * 2;
                        y = -1 * Math.Cos(Rechne.DegreeToRadian(w)) * (item / referenz) * 2;
                    }
                    else if (w >= 90 && w < 180)
                    {
                        x = -1 * Math.Cos(Rechne.DegreeToRadian(w - 90)) * (item / referenz) * 2;
                        y = Math.Sin(Rechne.DegreeToRadian(w - 90)) * (item / referenz) * 2;
                    }
                    else if (w >= 180 && w < 270)
                    {
                        x = Math.Sin(Rechne.DegreeToRadian(w - 180)) * (item / referenz) * 2;
                        y = Math.Cos(Rechne.DegreeToRadian(w - 180)) * (item / referenz) * 2;
                    }
                    else
                    {
                        x = Math.Cos(Rechne.DegreeToRadian(w - 270)) * (item / referenz) * 2;
                        y = -1 * Math.Sin(Rechne.DegreeToRadian(w - 270)) * (item / referenz) * 2;
                    }

                    punkteListen[i].Add(new Point3D(x, y, schleifenWerte[i]));

                    w += (360.0 / AnzahlSeiten);
                }
                w = 360.0 / AnzahlSeiten / 2.0;
            }

            // Einen Punkt unten in der Mitte erstellen, falls der Neigungswinkel negativ und die Grundlinie sehr klein ist.
            Point3D innenMitteUnten = new Point3D(0, 0, Hoehe / referenz * -1 + (Math.Tan(Rechne.DegreeToRadian(90 - Math.Abs(WinkelAlphaEins))) * Math.Abs(umkreisradiusInnenUnten) / referenz * 2));
            Point3D innenMitteOben = new Point3D(0, 0, Hoehe / referenz - (Math.Tan(Rechne.DegreeToRadian(90 - Math.Abs(WinkelAlphaEins))) * Math.Abs(umkreisradiusInnenOben) / referenz * 2));

            Model3DGroup group = new Model3DGroup();

            // Verbindet die Punkte und füllt die Flächen.

            // Füllt die äußeren Flächen.
            group.Children.Add(Viereck(punkteUnten[0], punkteUnten[AnzahlSeiten - 1], punkteOben[AnzahlSeiten - 1], punkteOben[0]));

            for (int i = 0; i < AnzahlSeiten - 1; i++)
                group.Children.Add(Viereck(punkteUnten[i + 1], punkteUnten[i], punkteOben[i], punkteOben[i + 1]));

            // Füllt die inneren Flächen wenn oben und unten ein Loch entsteht.
            if (umkreisradiusInnenOben > 0 && umkreisradiusInnenUnten > 0)
            {
                group.Children.Add(Viereck(punkteInnenUnten[AnzahlSeiten - 1], punkteInnenUnten[0], punkteInnenOben[0], punkteInnenOben[AnzahlSeiten - 1]));

                for (int i = 0; i < AnzahlSeiten - 1; i++)
                    group.Children.Add(Viereck(punkteInnenUnten[i], punkteInnenUnten[i + 1], punkteInnenOben[i + 1], punkteInnenOben[i]));
            }

            // Füllt die inneren Flächen wenn nur oben ein Loch entsteht.
            if (umkreisradiusInnenOben > 0 && umkreisradiusInnenUnten <= 0)
            {
                group.Children.Add(Dreieck(punkteInnenOben[0], punkteInnenOben[AnzahlSeiten - 1], innenMitteUnten));

                for (int i = 0; i < AnzahlSeiten - 1; i++)
                    group.Children.Add(Dreieck(punkteInnenOben[i + 1], punkteInnenOben[i], innenMitteUnten));
            }

            // Füllt die inneren Flächen wenn nur unten ein Loch entsteht.
            if (umkreisradiusInnenUnten > 0 && umkreisradiusInnenOben <= 0)
            {
                group.Children.Add(Dreieck(punkteInnenUnten[AnzahlSeiten - 1], punkteInnenUnten[0], innenMitteOben));

                for (int i = 0; i < AnzahlSeiten - 1; i++)
                    group.Children.Add(Dreieck(punkteInnenUnten[i], punkteInnenUnten[i + 1], innenMitteOben));
            }

            // Wenn oben kein Loch entsteht.
            if (umkreisradiusInnenOben <= 0)
            {
                // Füllt die obere Fläche mit Dreiecken.
                Point3D middle = new Point3D(0, 0, Hoehe / referenz);

                group.Children.Add(Dreieck(punkteOben[0], punkteOben[AnzahlSeiten - 1], middle));

                for (int i = 0; i < AnzahlSeiten - 1; i++)
                    group.Children.Add(Dreieck(punkteOben[i + 1], punkteOben[i], middle));
            }
            else
            {
                // Füllt die oberen Flächen.
                group.Children.Add(Viereck(punkteOben[0], punkteOben[AnzahlSeiten - 1], punkteInnenOben[AnzahlSeiten - 1], punkteInnenOben[0]));

                for (int i = 0; i < AnzahlSeiten - 1; i++)
                    group.Children.Add(Viereck(punkteOben[i + 1], punkteOben[i], punkteInnenOben[i], punkteInnenOben[i + 1]));
            }

            // Wenn unten kein Loch entsteht.
            if (umkreisradiusInnenUnten <= 0)
            {
                // Füllt die untere Fläche mit Dreiecken.
                Point3D middle = new Point3D(0, 0, Hoehe / referenz * -1);

                group.Children.Add(Dreieck(punkteUnten[AnzahlSeiten - 1], punkteUnten[0], middle));

                for (int i = 0; i < AnzahlSeiten - 1; i++)
                    group.Children.Add(Dreieck(punkteUnten[i], punkteUnten[i + 1], middle));
            }
            else
            {
                // Füllt die unteren Flächen.
                group.Children.Add(Viereck(punkteUnten[AnzahlSeiten - 1], punkteUnten[0], punkteInnenUnten[0], punkteInnenUnten[AnzahlSeiten - 1]));

                for (int i = 0; i < AnzahlSeiten - 1; i++)
                    group.Children.Add(Viereck(punkteUnten[i], punkteUnten[i + 1], punkteInnenUnten[i + 1], punkteInnenUnten[i]));
            }

            // Lichtquellen hinzufügen.
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(2, 1, -1)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-2, 1, -1)));
            group.Children.Add(new DirectionalLight(Colors.White, new Vector3D(0, -2, -1)));

            modell.Content = group;
        }

        #endregion
    }
}
