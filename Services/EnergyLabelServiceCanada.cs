using Energy_printer.Models;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using PdfSharp;
using System.IO;

namespace Energy_printer.Services
{
    // ══════════════════════════════════════════════════════════════════════
    //  ETIQUETA CANADA (BLANCA / EnerGuide)
    // ══════════════════════════════════════════════════════════════════════
    public class EnergyLabelServiceCanada : EnergyLabelHelpersBase
    {
        public EnergyLabelServiceCanada(string contentPath) : base(contentPath)
        {
        }

        // ══════════════════════════════════════════════════════════════════════
        //  MAPEO DESDE DATA_LABEL (Entity Framework)
        // ══════════════════════════════════════════════════════════════════════

        public EnergyLabelDataCanada FromDataLabel(DATA_LABEL d)
        {
            return new EnergyLabelDataCanada
            {
                MODEL = d.MODEL,
                MODEL_KW = d.MODEL_KW,
                LOW_KW = d.LOW_KW,
                HIGH_KW = d.HIGH_KW,
                TYPE = d.TYPE,
                RANGE = d.RANGE,
                ENERGY_LOGO = d.ENERGY_LOGO,
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GENERACIÓN DE LA PÁGINA PDF
        // ══════════════════════════════════════════════════════════════════════

        public void AddCanadaPage(PdfDocument doc, EnergyLabelDataCanada d)
        {
            var page = doc.AddPage();
            page.Width = XUnit.FromInch(5.2);
            page.Height = XUnit.FromInch(7.15);

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                DrawCanadaLabel(gfx, d, page.Width.Point, page.Height.Point);
            }
        }

        private void DrawCanadaLabel(XGraphics gfx, EnergyLabelDataCanada d, double W, double H)
        {
            double pad = In(0.07);
            double padTop = In(0.19);

            // Fondo blanco y Borde
            gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
            gfx.DrawRectangle(new XPen(XColors.Black, 1.5), 0, 0, W, H);

            double innerH = In(5.80) + pad;
            gfx.DrawRectangle(new XPen(XColors.Black, 1), pad / 2, pad / 2, W - pad, innerH);

            double x = pad * 2;
            double cW = W - pad * 4;
            double y = pad + padTop;
            double logoW = 0;
            var logoEner = LoadImage("Energuide2.png");
            if (logoEner != null)
            {
                logoW = cW * 0.96;
                double logoH = logoW * (208.0 / 656.0);
                double logoX = pad + (cW - logoW) / 2 + pad;
                gfx.DrawImage(logoEner, logoX, y, logoW, logoH);
                y += logoH + In(0.12);
            }
            else
            {
                gfx.DrawRectangle(XBrushes.Black, x, y, cW, In(0.7));
                gfx.DrawString("ENERGUIDE", new XFont("Arial", 28, XFontStyleEx.Bold),
                    XBrushes.White, new XRect(x, y, cW, In(0.7)), FmtMC);
                y += In(0.8);
            }

            gfx.DrawString("Energy consumption / Consommation énergétique",
                new XFont("Arial", 10, XFontStyleEx.Bold), XBrushes.Black,
                new XRect(x, y, cW, In(0.25)), FmtMC);
            y += In(0.30);

            double kwhY = y;
            double kwhNumW = cW * 0.55;
            gfx.DrawString(d.MODEL_KW.ToString(),
                new XFont("Arial", 55, XFontStyleEx.Bold), XBrushes.Black,
                new XRect(x + cW * 0.1, kwhY, kwhNumW, In(0.85)), FmtBR);

            double unitX = x + cW * 0.1 + kwhNumW + In(0.1);
            gfx.DrawString("kWh",
                new XFont("Arial", 30, XFontStyleEx.Bold), XBrushes.Black,
                new XRect(unitX, kwhY + In(0.1), cW * 0.3, In(0.45)), FmtTL);
            gfx.DrawString("per year / par année",
                new XFont("Arial", 9, XFontStyleEx.Bold), XBrushes.Black,
                new XRect(unitX, kwhY + In(0.5), cW * 0.3, In(0.3)), FmtTL);
            y += In(0.95);

            double arrowPct = (double)((d.HIGH_KW - d.LOW_KW) <= 0 ? 0
                : (double)(d.MODEL_KW - d.LOW_KW) / (d.HIGH_KW - d.LOW_KW));
            arrowPct = Math.Max(0, Math.Min(1, arrowPct));

            double arrowSize = In(0.22);
            double arrowX = x + arrowPct * cW;
            gfx.DrawPolygon(XBrushes.Black, new[]
            {
                new XPoint(arrowX,                  y + arrowSize),
                new XPoint(arrowX - arrowSize * 0.6, y),
                new XPoint(arrowX + arrowSize * 0.6, y),
            }, XFillMode.Winding);

            gfx.DrawString("This Model / Ce Modèle",
                new XFont("Arial", 9, XFontStyleEx.Bold), XBrushes.Black,
                new XRect(arrowX + In(0.12), y, cW * 0.5, In(0.22)), FmtML);
            y += In(0.30);

            double barH = In(0.23);
            int steps = 20;
            double stepW = logoW / steps;
            for (int i = 0; i < steps; i++)
            {
                int gray = (int)(255.0 * i / (steps - 1));
                var brush = new XSolidBrush(XColor.FromArgb(255 - gray, 255 - gray, 255 - gray));
                gfx.DrawRectangle(brush, (pad + (cW - logoW) / 2 + pad) + i * stepW, y, stepW + 0.5, barH);
            }
            gfx.DrawRectangle(new XPen(XColors.Black, 2), pad + (cW - logoW) / 2 + pad, y, logoW, barH);
            y += barH;

            gfx.DrawString(d.LOW_KW + " kWh",
                new XFont("Arial", 7, XFontStyleEx.Bold), XBrushes.Black,
                new XRect((pad * 3 + (cW - logoW) / 2), y + In(0.04), cW / 2, In(0.2)), FmtTL);
            gfx.DrawString(d.HIGH_KW + " kWh",
                new XFont("Arial", 7, XFontStyleEx.Bold), XBrushes.Black,
                new XRect(x + cW / 2 - In(0.2), y + In(0.04), cW / 2, In(0.2)), FmtTR);
            y += In(0.28);

            double rowH = In(0.5);
            x = (pad + (cW - logoW) / 2 );

            // 1. Ule (Lado izquierdo)
            string textoUle = "Uses least energy /\nConsomme le moins d'énergie";
            XFont fuenteUle = new XFont("Arial", 10, XFontStyleEx.Bold);
            XRect rectanguloUle = new XRect(x, y, cW * 0.40, In(0.50));

            XTextFormatter tfUle = new XTextFormatter(gfx);
            tfUle.Alignment = XParagraphAlignment.Left;
            tfUle.DrawString(textoUle, fuenteUle, XBrushes.Black, rectanguloUle);

            // 2. TYPE (Centro)
            gfx.DrawString(d.TYPE.ToUpper(),
                new XFont("Arial", 16, XFontStyleEx.Bold), XBrushes.Black,
                new XRect(x + cW * 0.3, y - In(0.05), cW * 0.4, rowH), FmtMC);

            string textoUme = "Uses most energy /\nConsomme le plus d'énergie";
            XFont fuenteUme = new XFont("Arial", 10, XFontStyleEx.Bold);

            // Recorremos la coordenada X para que empiece en el 75% del ancho de tu contenedor
            XRect rectanguloUme = new XRect(x + (cW * 0.73) - (cW * 0.10), y, cW * 0.35, In(0.50));

            XTextFormatter tfUme = new XTextFormatter(gfx);
            // Lo alineamos a la derecha para respetar la estética de la etiqueta
            tfUme.Alignment = XParagraphAlignment.Right;
            tfUme.DrawString(textoUme, fuenteUme, XBrushes.Black, rectanguloUme);

            // AHORA SÍ, bajamos la coordenada "y" para preparar el siguiente elemento (el volumen)
            y += rowH + In(0.15);
            double rowSimilarH = In(0.55);

            // Laterales en fuente pequeña, solo una palabra clave
            string textoSimEn = "Similar models \ncompared";
            XFont fuenteSimEn = new XFont("Arial", 10, XFontStyleEx.Regular);
            XRect rectanguloSimEn = new XRect(x, y, cW * 0.25, rowSimilarH);

            XTextFormatter tfSimEn = new XTextFormatter(gfx);
            tfSimEn.Alignment = XParagraphAlignment.Left;
            tfSimEn.DrawString(textoSimEn, fuenteSimEn, XBrushes.Black, rectanguloSimEn);


            // 2. Textos centrales (Rango y Volumen)
            gfx.DrawString(d.RANGE.ToUpper(),
                new XFont("Arial", 10, XFontStyleEx.Bold), XBrushes.Black,
                new XRect(x + cW * 0.18, y, cW * 0.64, In(0.20)), FmtTC);

            gfx.DrawString("volume in ft.3 / volume en pi3",
                new XFont("Arial", 10, XFontStyleEx.Bold), XBrushes.Black,
                new XRect(x + cW * 0.18, y + In(0.20), cW * 0.64, In(0.25)), FmtTC);


            // 3. Texto derecho (Francés) en multilínea
            string textoSimFr = "Modèles similaires\ncomparés";
            XFont fuenteSimFr = new XFont("Arial", 10, XFontStyleEx.Regular);
            XRect rectanguloSimFr = new XRect(x + cW * 0.80 - (cW * 0.07), y, cW * 0.25, rowSimilarH);

            XTextFormatter tfSimFr = new XTextFormatter(gfx);
            tfSimFr.Alignment = XParagraphAlignment.Right; // Alineado a la derecha
            tfSimFr.DrawString(textoSimFr, fuenteSimFr, XBrushes.Black, rectanguloSimFr);

            // 4. Bajamos la coordenada "y" para la siguiente fila
            y += rowSimilarH;

            gfx.DrawString("Model number",
                new XFont("Arial", 10, XFontStyleEx.Regular), XBrushes.Black,
                new XRect(x, y + In(0.1), cW * 0.35, In(0.35)), FmtTL);
            gfx.DrawString(d.MODEL.ToUpper(),
                new XFont("Arial", 14, XFontStyleEx.Bold), XBrushes.Black,
                new XRect(x + cW * 0.2, y, cW * 0.6, In(0.35)), FmtMC);
            gfx.DrawString("Numéro du modèle",
                new XFont("Arial", 10, XFontStyleEx.Regular), XBrushes.Black,
                new XRect(x + cW * 0.63, y + In(0.1), cW * 0.35, In(0.35)), FmtTR);

            y += In(0.50);

            string removalLbel = "Removal of this label before first retail purchase is an offence (S.C. 1992, c. 36)\n" +
                "Enlever cette étiquette avant le premier achat au détail constitue une infraction (L.C. 1992, ch. 36)";
            XFont fuenteRl = new XFont("Arial", 7, XFontStyleEx.Regular);
            XRect rectanguloRl = new XRect(x, y, cW, In(0.35));

            XTextFormatter tfRl = new XTextFormatter(gfx);

            tfRl.Alignment = XParagraphAlignment.Center;
            tfRl.DrawString(removalLbel, fuenteRl, XBrushes.Black, rectanguloRl);

            if (d.ENERGY_LOGO != null && d.ENERGY_LOGO.ToUpper() == "Y")
            {
                double botY = pad + innerH + In(0.05);
                double botH = H - botY - pad;
                double starW = In(0.75);

                var canStar = LoadImage("CanStar2.jpg");
                if (canStar != null)
                {
                    double imgH = starW * (288.0 / 203.0);
                    gfx.DrawImage(canStar, In(0.23), botY + (botH - imgH) / 2, starW, imgH);
                }

                double textX = In(0.23) + starW + In(0.15);
                double textW = W - textX - pad - In(0.75);

                // Instanciamos la fuente y el formateador una sola vez para ambos párrafos
                XFont fuenteFooter = new XFont("Arial", 6.9, XFontStyleEx.Bold);
                XTextFormatter tfFooter = new XTextFormatter(gfx);
                tfFooter.Alignment = XParagraphAlignment.Left; // Equivalente a tu FmtTL

                // 1. Párrafo en Inglés
                string textoEnergyEn = "The Energy Star® mark on this EnerGuide label signifies that this is an energy-efficient " +
                                       "appliance. Its energy performance meets or exceeds the Government of Canada's high efficiency " +
                                       "levels. Use the EnerGuide rating to determine how this appliance compares to other similar models.";

                XRect rectanguloEn = new XRect(textX, botY, textW, botH * 0.55);
                tfFooter.DrawString(textoEnergyEn, fuenteFooter, XBrushes.Black, rectanguloEn);


                // 2. Párrafo en Francés
                string textoEnergyFr = "La marque Energy Star® sur cette étiquette Énerguide signifie que l'appareil est éconergétique " +
                                       "et que son rendement énergétique satisfait ou dépasse les niveaux de haute efficacité du " +
                                       "gouvernement du Canada. Utilisez la cote Énerguide afin de comparer le rendement de l'appareil " +
                                       "avec celui d'autres modèles similaires.";

                XRect rectanguloFr = new XRect(textX, botY + botH * 0.50, textW, botH * 0.6);
                tfFooter.DrawString(textoEnergyFr, fuenteFooter, XBrushes.Black, rectanguloFr);
            }
        }
    }
}
