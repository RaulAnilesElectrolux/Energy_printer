using Energy_printer.Models;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.IO;

namespace Energy_printer.Services
{
    public class EnergyLabelService
    {
        private readonly string _contentPath;

        public EnergyLabelService(string contentPath)
        {
            _contentPath = contentPath;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  1. DATOS SIMULADOS
        // ══════════════════════════════════════════════════════════════════════

        public EnergyLabelDataUSA GetLabelDataUSA(string model = null)
        {
            return new EnergyLabelDataUSA
            {
                REF_TYPE = "Refrigerator-Freezer",
                DEFROST_SYSTEM = "Automatic Defrost",
                DOORTYPE = "Side-Mounted Freezer",
                ICE_SERVICE = "Through-the-Door Ice Service",
                CUST_NAME = "Electrolux",
                MODEL = model ?? "FRSS2623A*",
                CAB_SIZE = "25.6 Cubic Feet",
                PART_NUMBER = "A24876741",
                ENERGY_COST = 91,
                LOW_SIMILAR_MODEL = 81,
                HIGH_SIMILAR_MODEL = 116,
                LOW_AMOUNT = 66,
                HIGH_AMOUNT = 116,
                ELECTRICITY_USE = 647,
                ENERGY_LOGO = "N",
            };
        }

        public EnergyLabelDataCanada GetLabelDataCanada(string model = null)
        {
            return new EnergyLabelDataCanada
            {
                MODEL = model ?? "fghb2868T*",
                MODEL_KW = 647,
                LOW_KW = 640,
                HIGH_KW = 716,
                TYPE = "Type 7",
                RANGE = "24.5 TO 26.4 CU. FT.",
                ENERGY_LOGO = "Y",
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        //  2. GENERACIÓN DE PDF
        // ══════════════════════════════════════════════════════════════════════

        public byte[] GeneratePdf(EnergyLabelDataUSA usa, EnergyLabelDataCanada can)
        {
            var doc = new PdfDocument();
            doc.Info.Title = "Energy Labels - Electrolux";
            doc.Info.Author = "Electrolux Label Tool";

            // Páginas Canada (5.2in x 7.15in)
            AddCanadaPage(doc, can);
            AddCanadaPage(doc, can);

            // Páginas USA (14.5cm x 19.1cm)
            AddUSAPage(doc, usa);
            AddUSAPage(doc, usa);

            using (var ms = new MemoryStream())
            {
                doc.Save(ms, false);
                return ms.ToArray();
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS GLOBALES
        // ══════════════════════════════════════════════════════════════════════

        private static double Cm(double cm) { return cm * 28.3465; }
        private static double In(double inch) { return inch * 72.0; }

        private static XStringFormat Fmt(XStringAlignment h, XLineAlignment v)
        {
            return new XStringFormat { Alignment = h, LineAlignment = v };
        }

        private static readonly XStringFormat FmtTL = XStringFormats.TopLeft;
        private static readonly XStringFormat FmtTR = XStringFormats.TopRight;
        private static readonly XStringFormat FmtC = XStringFormats.Center;
        private static readonly XStringFormat FmtML = Fmt(XStringAlignment.Near, XLineAlignment.Center);
        private static readonly XStringFormat FmtMR = Fmt(XStringAlignment.Far, XLineAlignment.Center);
        private static readonly XStringFormat FmtMC = Fmt(XStringAlignment.Center, XLineAlignment.Center);
        private static readonly XStringFormat FmtBL = Fmt(XStringAlignment.Near, XLineAlignment.Far);
        private static readonly XStringFormat FmtBR = Fmt(XStringAlignment.Far, XLineAlignment.Far);
        private static readonly XStringFormat FmtBC = Fmt(XStringAlignment.Center, XLineAlignment.Far);
        private static readonly XStringFormat FmtTC = Fmt(XStringAlignment.Center, XLineAlignment.Near);

        private XImage LoadImage(string filename)
        {
            string path = Path.Combine(_contentPath, filename);
            return File.Exists(path) ? XImage.FromFile(path) : null;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  ETIQUETA CANADA (BLANCA)
        // ══════════════════════════════════════════════════════════════════════

        private void AddCanadaPage(PdfDocument doc, EnergyLabelDataCanada d)
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
            double pad = In(0.11);
            double padTop = In(0.19);

            // Fondo blanco y Borde
            gfx.DrawRectangle(XBrushes.White, 0, 0, W, H);
            gfx.DrawRectangle(new XPen(XColors.Black, 2), 0, 0, W, H);

            double innerH = In(5.86);
            gfx.DrawRectangle(new XPen(XColors.Black, 2), pad, pad, W - pad * 2, innerH);

            double x = pad * 2;
            double cW = W - pad * 4;
            double y = pad + padTop;

            var logoEner = LoadImage("Energuide2.png");
            if (logoEner != null)
            {
                double logoW = cW * 0.95;
                double logoH = logoW * (208.0 / 656.0);
                double logoX = pad + (cW - logoW) / 2 + pad;
                gfx.DrawImage(logoEner, logoX, y, logoW, logoH);
                y += logoH + In(0.12);
            }
            else
            {
                gfx.DrawRectangle(XBrushes.Black, x, y, cW, In(0.7));
                gfx.DrawString("ENERGUIDE", new XFont("Arial", 28, XFontStyle.Bold),
                    XBrushes.White, new XRect(x, y, cW, In(0.7)), FmtMC);
                y += In(0.8);
            }

            gfx.DrawString("Energy consumption / Consommation énergétique",
                new XFont("Arial", 10, XFontStyle.Bold), XBrushes.Black,
                new XRect(x, y, cW, In(0.25)), FmtMC);
            y += In(0.30);

            double kwhY = y;
            double kwhNumW = cW * 0.55;
            gfx.DrawString(d.MODEL_KW.ToString(),
                new XFont("Arial Black", 55, XFontStyle.Bold), XBrushes.Black,
                new XRect(x + cW * 0.1, kwhY, kwhNumW, In(0.85)), FmtBR);

            double unitX = x + cW * 0.1 + kwhNumW + In(0.1);
            gfx.DrawString("kWh",
                new XFont("Arial", 30, XFontStyle.Bold), XBrushes.Black,
                new XRect(unitX, kwhY + In(0.1), cW * 0.3, In(0.45)), FmtTL);
            gfx.DrawString("per year / par année",
                new XFont("Arial", 9, XFontStyle.Bold), XBrushes.Black,
                new XRect(unitX, kwhY + In(0.5), cW * 0.3, In(0.3)), FmtTL);
            y += In(0.95);

            double arrowPct = (d.HIGH_KW - d.LOW_KW) == 0 ? 0
                : (double)(d.MODEL_KW - d.LOW_KW) / (d.HIGH_KW - d.LOW_KW);
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
                new XFont("Arial", 9, XFontStyle.Bold), XBrushes.Black,
                new XRect(arrowX + In(0.12), y, cW * 0.5, In(0.22)), FmtML);
            y += In(0.30);

            double barH = In(0.23);
            int steps = 20;
            double stepW = cW / steps;
            for (int i = 0; i < steps; i++)
            {
                int gray = (int)(255.0 * i / (steps - 1));
                var brush = new XSolidBrush(XColor.FromArgb(255 - gray, 255 - gray, 255 - gray));
                gfx.DrawRectangle(brush, x + i * stepW, y, stepW + 0.5, barH);
            }
            gfx.DrawRectangle(new XPen(XColors.Black, 2), x, y, cW, barH);
            y += barH;

            gfx.DrawString(d.LOW_KW + " kWh",
                new XFont("Arial", 8, XFontStyle.Bold), XBrushes.Black,
                new XRect(x, y + In(0.04), cW / 2, In(0.2)), FmtTL);
            gfx.DrawString(d.HIGH_KW + " kWh",
                new XFont("Arial", 8, XFontStyle.Bold), XBrushes.Black,
                new XRect(x + cW / 2, y + In(0.04), cW / 2, In(0.2)), FmtTR);
            y += In(0.28);

            double rowH = In(0.5);
            gfx.DrawString("Uses least energy",
    new XFont("Arial", 8, XFontStyle.Bold),
    XBrushes.Black,
    new XRect(x, y, cW * 0.25, In(0.18)),
    FmtMC);
            gfx.DrawString(d.TYPE.ToUpper(),
                new XFont("Arial", 16, XFontStyle.Bold), XBrushes.Black,
                new XRect(x + cW * 0.3, y, cW * 0.4, rowH), FmtMC);
            gfx.DrawString("Consomme le moins d'énergie",
    new XFont("Arial", 6.5, XFontStyle.Regular),
    XBrushes.Black,
    new XRect(x, y + In(0.16), cW * 0.25, In(0.18)),
    FmtMC);
            y += rowH + In(0.08);
            double rowSimilarH = In(0.55);

            // Laterales en fuente pequeña, solo una palabra clave
            gfx.DrawString("Similar models compared",
    new XFont("Arial", 6.5, XFontStyle.Regular), XBrushes.Black,
    new XRect(x, y, cW * 0.18, rowSimilarH), FmtTL);

            gfx.DrawString(d.RANGE.ToUpper(),
                new XFont("Arial", 8, XFontStyle.Bold), XBrushes.Black,
                new XRect(x + cW * 0.18, y, cW * 0.64, In(0.27)), FmtTC);
            gfx.DrawString("volume in ft.3 / volume en pi3",
    new XFont("Arial", 7, XFontStyle.Bold), XBrushes.Black,
    new XRect(x + cW * 0.18, y + In(0.27), cW * 0.64, In(0.25)), FmtTC);

            gfx.DrawString("Modèles similaires\ncomparés",
                new XFont("Arial", 6.5, XFontStyle.Regular), XBrushes.Black,
                new XRect(x + cW * 0.82, y, cW * 0.18, rowSimilarH), FmtTR);

            y += rowSimilarH;

            gfx.DrawString("Model number",
                new XFont("Arial", 10, XFontStyle.Regular), XBrushes.Black,
                new XRect(x, y, cW * 0.35, In(0.35)), FmtTL);
            gfx.DrawString(d.MODEL.ToUpper(),
                new XFont("Arial", 14, XFontStyle.Bold), XBrushes.Black,
                new XRect(x + cW * 0.2, y, cW * 0.6, In(0.35)), FmtMC);
            gfx.DrawString("Numéro du modèle",
                new XFont("Arial", 10, XFontStyle.Regular), XBrushes.Black,
                new XRect(x + cW * 0.65, y, cW * 0.35, In(0.35)), FmtTR);
            y += In(0.42);

            gfx.DrawString(
                "Removal of this label before first retail purchase is an offence (S.C. 1992, c. 36)\n" +
                "Enlever cette étiquette avant le premier achat au détail constitue une infraction (L.C. 1992, ch. 36)",
                new XFont("Arial", 7, XFontStyle.Regular), XBrushes.Black,
                new XRect(x, y, cW, In(0.35)), FmtTC);

            if (d.ENERGY_LOGO != null && d.ENERGY_LOGO.ToUpper() == "Y")
            {
                double botY = pad + innerH + In(0.05);
                double botH = H - botY - pad;
                double starW = In(0.75);

                var canStar = LoadImage("CanStar2.png");
                if (canStar != null)
                {
                    double imgH = starW * (288.0 / 203.0);
                    gfx.DrawImage(canStar, In(0.23), botY + (botH - imgH) / 2, starW, imgH);
                }

                double textX = In(0.23) + starW + In(0.15);
                double textW = W - textX - pad;

                gfx.DrawString(
                    "The Energy Star® mark on this EnerGuide label signifies that this is an energy-efficient " +
                    "appliance. Its energy performance meets or exceeds the Government of Canada's high efficiency " +
                    "levels. Use the EnerGuide rating to determine how this appliance compares to other similar models.",
                    new XFont("Arial", 7.5, XFontStyle.Bold), XBrushes.Black,
                    new XRect(textX, botY, textW, botH * 0.5), FmtTL);

                gfx.DrawString(
                    "La marque Energy Star® sur cette étiquette Énerguide signifie que l'appareil est éconergétique " +
                    "et que son rendement énergétique satisfait ou dépasse les niveaux de haute efficacité du " +
                    "gouvernement du Canada. Utilisez la cote Énerguide afin de comparer le rendement de l'appareil " +
                    "avec celui d'autres modèles similaires.",
                    new XFont("Arial", 7.5, XFontStyle.Bold), XBrushes.Black,
                    new XRect(textX, botY + botH * 0.5, textW, botH * 0.5), FmtTL);
            }
        }


        // ══════════════════════════════════════════════════════════════════════
        //  ETIQUETA USA (TRANSPARENTE / SIN AMARILLO)
        // ══════════════════════════════════════════════════════════════════════

        private void AddUSAPage(PdfDocument doc, EnergyLabelDataUSA d)
        {
            var page = doc.AddPage();
            page.Width = XUnit.FromCentimeter(14.5);
            page.Height = XUnit.FromCentimeter(19.1);
            page.Orientation = PageOrientation.Portrait;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                DrawUSALabel(gfx, d, page.Width.Point, page.Height.Point);
            }
        }
        private static double Px(double px) { return px * 0.75; }   // 96px = 72pt

        private void DrawUSALabel(XGraphics gfx, EnergyLabelDataUSA d, double W, double H)
        {
            double pad = Cm(0.6);          // .usa-container padding
            double CW = W - pad * 2;      // ancho de contenido (100%)
            var white = XBrushes.White;
            var black = XBrushes.Black;

            // ── 1. Gov header  (margin-top 0.3cm, Arial 8pt) ─────────────────────
            double govY = pad + Cm(0.3);
            gfx.DrawString("U.S. Government",
                new XFont("Arial", 8, XFontStyle.Bold), black,
                new XRect(pad, govY, CW * 0.5, Px(12)), FmtTL);
            gfx.DrawString("Federal law prohibits removal of this label before consumer purchase.",
                new XFont("Arial", 8, XFontStyle.Regular), black,
                new XRect(pad + CW * 0.25, govY, CW * 0.75, Px(12)), FmtTR);
            double y = govY + Px(14);

            // ── 2. Logo EnergyGuide  (SIN caja negra; imagen sola, aspect real) ──
            double logoTop = y, logoBottom = y;
            var logoTit = LoadImage("Logo_titulo.png");
            if (logoTit != null)
            {
                double logoH = CW * ((double)logoTit.PixelHeight / logoTit.PixelWidth);
                gfx.DrawImage(logoTit, pad, logoTop, CW, logoH);
                logoBottom = logoTop + logoH;
            }
            else
            {
                double logoH = Cm(1.6);
                gfx.DrawString("EnergyGuide", new XFont("Arial Black", 28, XFontStyle.Bold),
                    black, new XRect(pad, logoTop, CW, logoH), FmtMC);
                logoBottom = logoTop + logoH;
            }
            // .usa-main-title margin-bottom:-45px  +  .specs-table padding-top:15px = -30px
            y = logoBottom - Px(30);

            // ── 3. Specs  (Arial 8.5pt bold, line-height 1.2) ────────────────────
            double specLineH = 8.5 * 1.2;
            var fSpec = new XFont("Arial", 8.5, XFontStyle.Bold);
            string[] left = { d.REF_TYPE, "• " + d.DEFROST_SYSTEM, "• " + d.DOORTYPE, "• " + d.ICE_SERVICE };
            string[] right = { d.CUST_NAME, "Model: " + d.MODEL, "Capacity: " + d.CAB_SIZE };
            for (int i = 0; i < left.Length; i++)
                gfx.DrawString(left[i], fSpec, black,
                    new XRect(pad, y + i * specLineH, CW * 0.5, specLineH), FmtTL);
            for (int i = 0; i < right.Length; i++)
                gfx.DrawString(right[i], fSpec, black,
                    new XRect(pad + CW * 0.5, y + i * specLineH, CW * 0.5, specLineH), FmtTR);
            y += 4 * specLineH + Px(15);   // margin-bottom 15px

            // ── 4. Compare box  (negro, texto blanco, padding 10px) ──────────────
            double cmpMain = 12 * 1.2, cmpSub = 9 * 1.2;
            double cmpH = Px(10) + cmpMain + Px(3) + cmpSub + Px(10);
            gfx.DrawRectangle(black, pad, y, CW, cmpH);
            gfx.DrawString("Compare ONLY to other labels with yellow numbers.",
                new XFont("Arial Black", 12, XFontStyle.Bold), white,
                new XRect(pad, y + Px(10), CW, cmpMain), FmtTC);
            gfx.DrawString("Labels with yellow numbers are based on the same test procedures.",
                new XFont("Arial", 9, XFontStyle.Bold), white,
                new XRect(pad, y + Px(10) + cmpMain + Px(3), CW, cmpSub), FmtTC);
            y += cmpH + Px(10);   // margin-bottom 10px

            // ── 5. Cost box  (más aire, números Arial Bold, título separado) ──
            double costTitleH = 14 * 1.2;
            double gapTitle = Px(25);                 // separación título ↔ número
            double numH = Px(20);                     // banda del número
            double costH = Px(12) + costTitleH + gapTitle + numH + Px(6) + Px(12) + Px(8) + Px(10);
            gfx.DrawRectangle(black, pad, y, CW, costH);
            gfx.DrawString("Estimated Yearly Energy Cost",
                new XFont("Arial Black", 18, XFontStyle.Bold), white,
                new XRect(pad, y + Px(12), CW, costTitleH), FmtTC);

            // posición horizontal según el pct del costo
            double costInnerW = CW - 2 * Px(15);
            double scaleLeft = pad + Px(15) + Px(100);
            double scaleW = costInnerW - Px(110);
            double gMin = Math.Min(d.LOW_AMOUNT, Math.Min(d.LOW_SIMILAR_MODEL, d.ENERGY_COST));
            double gMax = Math.Max(d.HIGH_AMOUNT, Math.Max(d.HIGH_SIMILAR_MODEL, d.ENERGY_COST));
            double pct = (gMax == gMin) ? 0.5 : (d.ENERGY_COST - gMin) / (double)(gMax - gMin);
            pct = Math.Max(0, Math.Min(1, pct));
            double cx = scaleLeft + pct * scaleW;

            // valor DEBAJO del título — Arial Bold (no Arial Black)
            double valTopCost = y + Px(12) + costTitleH + gapTitle;
            var fDollar = new XFont("Arial Black", 26, XFontStyle.Bold);
            var fNum = new XFont("Arial Black", 38, XFontStyle.Bold);
            string numStr = d.ENERGY_COST.ToString();
            double wNum = gfx.MeasureString(numStr, fNum).Width;
            double wDol = gfx.MeasureString("$", fDollar).Width;
            double gap = Px(4);
            double gLeft = cx - (wDol + gap + wNum) / 2;
            gfx.DrawString(numStr, fNum, white,
                new XRect(gLeft + wDol + gap, valTopCost, wNum + Px(4), numH), FmtML);
            gfx.DrawString("$", fDollar, white,
                new XRect(gLeft, valTopCost, wDol + Px(2), numH), FmtML);

            // triángulo abajo del número
            double triTopY = valTopCost + numH + 15;
            double triApexY = triTopY + Px(12);
            gfx.DrawPolygon(white, new[]
            {
                new XPoint(cx - Px(10), triTopY),
                new XPoint(cx + Px(10), triTopY),
                new XPoint(cx,          triApexY),
            }, XFillMode.Winding);

            y += costH;

            // ── 6. Cost Ranges  (mismo bloque negro, esquinas inferiores redondas) ─
            double rngH = Px(88);
            gfx.DrawRoundedRectangle(black, pad, y, CW, rngH, Px(10), Px(10));
            gfx.DrawRectangle(black, pad, y, CW, Px(12));   // cuadra el borde superior (flush con cost-box)

            double innerLeft = pad + Px(5);
            double innerTop = y + Px(8);
            double innerW = CW - 2 * Px(5);
            double innerH = rngH - 2 * Px(8);

            // "Cost Ranges" rotado en la columna izquierda (25px)
            gfx.Save();
            double sbCx = innerLeft + Px(12.5);
            double sbCy = innerTop + innerH / 2;
            gfx.RotateAtTransform(-90, new XPoint(sbCx, sbCy));
            gfx.DrawString("Cost Ranges", new XFont("Arial", 9, XFontStyle.Regular), white,
                new XRect(sbCx - Px(40), sbCy - Px(8), Px(80), Px(16)), FmtMC);
            gfx.Restore();

            // Caja con borde BLANCO alrededor de las dos barras
            double boxLeft = innerLeft + Px(25);
            double boxW = innerW - Px(25);
            gfx.DrawRoundedRectangle(new XPen(XColors.White, 1), boxLeft, innerTop, boxW, innerH, Px(10), Px(10));

            double labelW = Px(105);
            double trackLeft = boxLeft + labelW;
            double trackW = boxW - labelW - Px(12);
            double rowH = innerH / 2;
            double pillH = Px(18);

            // Fila 1: Models with similar features  (con notch)
            double r1cy = innerTop + rowH / 2;
            gfx.DrawString("Models with", new XFont("Arial", 9, XFontStyle.Regular), white,
                new XRect(boxLeft + Px(6), r1cy - Px(11), labelW, Px(11)), FmtML);
            gfx.DrawString("similar features", new XFont("Arial", 9, XFontStyle.Regular), white,
                new XRect(boxLeft + Px(6), r1cy + Px(1), labelW, Px(11)), FmtML);
            DrawPill(gfx, d.LOW_SIMILAR_MODEL, d.HIGH_SIMILAR_MODEL,
                     trackLeft, r1cy - pillH / 2, trackW, pillH, gMin, gMax, true);

            // Fila 2: All models
            double r2cy = innerTop + rowH + rowH / 2;
            gfx.DrawString("All models", new XFont("Arial", 10, XFontStyle.Regular), white,
                new XRect(boxLeft + Px(6), r2cy - Px(5.5), labelW, Px(11)), FmtML);
            DrawPill(gfx, d.LOW_AMOUNT, d.HIGH_AMOUNT,
                     trackLeft, r2cy - pillH / 2, trackW, pillH, gMin, gMax, false);

            y += rngH + Px(20);   // margin-bottom 20px

            // ── 7. kWh box  (negro, width 50%, centrado) ─────────────────────────
            double kwhH = Px(94);
            double kwhBoxW = CW * 0.5;
            double kwhBoxX = pad + (CW - kwhBoxW) / 2;
            gfx.DrawRectangle(black, kwhBoxX, y, kwhBoxW, kwhH);

            var fKwh = new XFont("Arial Black", 34, XFontStyle.Bold);
            var fUnit = new XFont("Arial Black", 14, XFontStyle.Bold);
            string kwhStr = d.ELECTRICITY_USE.ToString();
            double wKwh = gfx.MeasureString(kwhStr, fKwh).Width;
            double wUnit = gfx.MeasureString("kWh", fUnit).Width;
            double kg = Px(5);
            double grpLeft = kwhBoxX + (kwhBoxW - (wKwh + kg + wUnit)) / 2;
            double valTop = y + Px(20);
            gfx.DrawString(kwhStr, fKwh, white,
                new XRect(grpLeft, valTop, wKwh + Px(4), Px(44)), FmtBL);
            gfx.DrawString("kWh", fUnit, white,
                new XRect(grpLeft + wKwh + kg, valTop, wUnit + Px(4), Px(44)), FmtBL);
            gfx.DrawString("Estimated Yearly Electricity Use",
                new XFont("Arial", 9, XFontStyle.Bold), white,
                new XRect(kwhBoxX, valTop + Px(44) + Px(5), kwhBoxW, Px(12)), FmtTC);
            y += kwhH + Px(15);

            // ── 8. Footer + ftc.gov/energy  (anclados al fondo) ──────────────────
            double ftcH = Px(20);
            double ftcY = H - pad - ftcH;

            gfx.DrawString("ftc.gov/energy",
                new XFont("Arial", 12, XFontStyle.Regular),
                black,
                new XRect(pad, ftcY, CW, ftcH),
                FmtTC);

            // Ancho para las notas y el logo
            double notesW = CW * 0.72;
            double starW = Px(85);
            double starX = pad + CW - starW;

            // Ajustamos la posición inicial para que el texto y el logo no pisen el enlace
            double blockPaddingTop = Px(15);
            double blockTop = ftcY - blockPaddingTop - Px(85);

            // Formatter para multilínea
            var tf = new XTextFormatter(gfx);

            var notes = new (bool Bold, string T)[]
            {
            (true,  "Your cost will depend on your utility rates and use."),
            (false, "Both cost ranges based on models of similar size capacity."),
            (false, "Models with similar features have automatic defrost, side-mounted freezer, and through-the-door ice."),
            (false, "Estimated energy cost is based on a national average electricity cost of 14 cents per kWh.")
            };

            // Súmale los pixeles que necesites para empujar el texto hacia abajo
            double ny = blockTop + Px(25);

            foreach (var n in notes)
            {
                var fn = new XFont("Arial", 8, n.Bold ? XFontStyle.Bold : XFontStyle.Regular);

                // Viñeta (siempre en negrita para que parezca un bullet point real)
                gfx.DrawString("•", new XFont("Arial", 8, XFontStyle.Bold), black,
                    new XRect(pad, ny, Px(10), Px(11)), FmtTL);

                // Estimación dinámica de líneas: asumimos que en ese ancho caben unos 65 caracteres por línea.
                int lineas = (int)Math.Ceiling((double)n.T.Length / 65.0);
                double alturaTexto = lineas * Px(11); // Px(11) equivale a la altura de línea de la fuente

                tf.DrawString(n.T, fn, black,
                    new XRect(pad + Px(12), ny, notesW - Px(12), alturaTexto + Px(10)));

                // Salto dinámico + margen inferior (equivale al margin-bottom: 2px del HTML)
                ny += alturaTexto + Px(4);
            }

            // Calculamos la altura del logo SIEMPRE, para reservar su espacio
            double sH = starW; // Valor cuadrado por defecto por si no encuentra la imagen
            var logoPie = LoadImage("Logo_pie.png");
            if (logoPie != null)
            {
                sH = starW * ((double)logoPie.PixelHeight / logoPie.PixelWidth);
            }

            // Dibujamos la imagen SOLO si lleva el logo
            if (d.ENERGY_LOGO != null && d.ENERGY_LOGO.ToUpper() == "Y")
            {
                if (logoPie != null)
                {
                    gfx.DrawImage(logoPie, starX, blockTop, starW, sH);
                }
            }

            // Dibujamos el número de parte SIEMPRE, posicionado justo debajo del espacio del logo
            gfx.DrawString("PART NO. " + d.PART_NUMBER,
                new XFont("Arial", 7, XFontStyle.Bold),
                black,
                new XRect(starX, blockTop + sH + Px(4), starW + Px(50), Px(10)),
                FmtTL);
        
        }
        // ── Pill blanca con $low / $high adentro  (+ notch negro opcional) ──────
        private void DrawPill(XGraphics gfx, int low, int high,
                              double tX, double tY, double tW, double tH,
                              double gMin, double gMax, bool sep)
        {
            double range = gMax - gMin;
            double pL = range == 0 ? 0 : (low - gMin) / range;
            double pH = range == 0 ? 1 : (high - gMin) / range;
            double left = tX + pL * tW;
            double w = (pH - pL) * tW;

            double minW = Cm(1.2);                      // mínimo para que quepan los textos
            if (w < minW) w = minW;
            if (left + w > tX + tW) left = tX + tW - w;

            gfx.DrawRoundedRectangle(XBrushes.White, left, tY, w, tH, tH, tH);

            var f = new XFont("Arial Black", 10, XFontStyle.Bold);
            gfx.DrawString("$" + low, f, XBrushes.Black,
                new XRect(left + Px(6), tY, w * 0.5, tH), FmtML);
            gfx.DrawString("$" + high, f, XBrushes.Black,
                new XRect(left + w - Px(6) - Cm(1), tY, Cm(1), tH), FmtMR);

            if (sep)   // .separador-sim (3px negro en el borde izquierdo)
                gfx.DrawRectangle(XBrushes.Black, left, tY, Px(3), tH);
        }
    }
}