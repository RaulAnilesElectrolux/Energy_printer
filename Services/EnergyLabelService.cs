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
            AddUSAPage(doc, usa, isLeft: true);   // padL=0.6cm padR=0.15cm
            AddUSAPage(doc, usa, isLeft: false);  // padL=0.15cm padR=0.6cm

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

            double innerH = In(5.80);
            gfx.DrawRectangle(new XPen(XColors.Black, 2), pad / 2, pad / 2, W - pad, innerH);

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
                new XFont("Arial", 55, XFontStyle.Bold), XBrushes.Black,
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

            // 1. Ule (Lado izquierdo)
            string textoUle = "Uses least energy /\nConsomme le moins d'énergie";
            XFont fuenteUle = new XFont("Arial", 10, XFontStyle.Bold);
            XRect rectanguloUle = new XRect(x, y, cW * 0.40, In(0.50));

            XTextFormatter tfUle = new XTextFormatter(gfx);
            tfUle.Alignment = XParagraphAlignment.Left;
            tfUle.DrawString(textoUle, fuenteUle, XBrushes.Black, rectanguloUle);

            // 2. TYPE (Centro)
            gfx.DrawString(d.TYPE.ToUpper(),
                new XFont("Arial", 16, XFontStyle.Bold), XBrushes.Black,
                new XRect(x + cW * 0.3, y - In(0.05), cW * 0.4, rowH), FmtMC);

            // 3. Ume (Lado derecho)
            // (Por cierto, corregí "least" por "most" en el texto en inglés)
            string textoUme = "Uses most energy /\nConsomme le plus d'énergie";
            XFont fuenteUme = new XFont("Arial", 10, XFontStyle.Bold);

            // Recorremos la coordenada X para que empiece en el 75% del ancho de tu contenedor
            XRect rectanguloUme = new XRect(x + (cW * 0.75) - (cW * 0.10), y, cW * 0.35, In(0.50));

            XTextFormatter tfUme = new XTextFormatter(gfx);
            // Lo alineamos a la derecha para respetar la estética de la etiqueta
            tfUme.Alignment = XParagraphAlignment.Right;
            tfUme.DrawString(textoUme, fuenteUme, XBrushes.Black, rectanguloUme);

            // AHORA SÍ, bajamos la coordenada "y" para preparar el siguiente elemento (el volumen)
            y += rowH + In(0.15);
            double rowSimilarH = In(0.55);

            // Laterales en fuente pequeña, solo una palabra clave
            string textoSimEn = "Similar models \ncompared";
            XFont fuenteSimEn = new XFont("Arial", 10, XFontStyle.Regular);
            XRect rectanguloSimEn = new XRect(x, y, cW * 0.25, rowSimilarH);

            XTextFormatter tfSimEn = new XTextFormatter(gfx);
            tfSimEn.Alignment = XParagraphAlignment.Left;
            tfSimEn.DrawString(textoSimEn, fuenteSimEn, XBrushes.Black, rectanguloSimEn);


            // 2. Textos centrales (Rango y Volumen)
            gfx.DrawString(d.RANGE.ToUpper(),
                new XFont("Arial", 10, XFontStyle.Bold), XBrushes.Black,
                new XRect(x + cW * 0.18, y, cW * 0.64, In(0.20)), FmtTC);

            gfx.DrawString("volume in ft.3 / volume en pi3",
                new XFont("Arial", 10, XFontStyle.Bold), XBrushes.Black,
                new XRect(x + cW * 0.18, y + In(0.20), cW * 0.64, In(0.25)), FmtTC);


            // 3. Texto derecho (Francés) en multilínea
            string textoSimFr = "Modèles similaires\ncomparés";
            XFont fuenteSimFr = new XFont("Arial", 10, XFontStyle.Regular);
            XRect rectanguloSimFr = new XRect(x + cW * 0.82 - (cW * 0.07), y, cW * 0.25, rowSimilarH);

            XTextFormatter tfSimFr = new XTextFormatter(gfx);
            tfSimFr.Alignment = XParagraphAlignment.Right; // Alineado a la derecha
            tfSimFr.DrawString(textoSimFr, fuenteSimFr, XBrushes.Black, rectanguloSimFr);

            // 4. Bajamos la coordenada "y" para la siguiente fila
            y += rowSimilarH;

            gfx.DrawString("Model number",
                new XFont("Arial", 10, XFontStyle.Regular), XBrushes.Black,
                new XRect(x, y + In(0.1), cW * 0.35, In(0.35)), FmtTL);
            gfx.DrawString(d.MODEL.ToUpper(),
                new XFont("Arial", 14, XFontStyle.Bold), XBrushes.Black,
                new XRect(x + cW * 0.2, y, cW * 0.6, In(0.35)), FmtMC);
            gfx.DrawString("Numéro du modèle",
                new XFont("Arial", 10, XFontStyle.Regular), XBrushes.Black,
                new XRect(x + cW * 0.65, y + In(0.1), cW * 0.35, In(0.35)), FmtTR);

            y += In(0.50);

            string removalLbel = "Removal of this label before first retail purchase is an offence (S.C. 1992, c. 36)\n" +
                "Enlever cette étiquette avant le premier achat au détail constitue une infraction (L.C. 1992, ch. 36)";
            XFont fuenteRl = new XFont("Arial", 7, XFontStyle.Regular);
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
                XFont fuenteFooter = new XFont("Arial", 6.9, XFontStyle.Bold);
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


        // ══════════════════════════════════════════════════════════════════════
        //  ETIQUETA USA (TRANSPARENTE / SIN AMARILLO)
        // ══════════════════════════════════════════════════════════════════════

        private void AddUSAPage(PdfDocument doc, EnergyLabelDataUSA d, bool isLeft)
        {
            var page = doc.AddPage();
            page.Width = XUnit.FromCentimeter(14.5);
            page.Height = XUnit.FromCentimeter(19.1);
            page.Orientation = PageOrientation.Portrait;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                DrawUSALabel(gfx, d, page.Width.Point, page.Height.Point, isLeft);
            }
        }
        private static double Px(double px) { return px * 0.75; }   // 96px = 72pt

        private void DrawUSALabel(XGraphics gfx, EnergyLabelDataUSA d, double W, double H,
                           bool isLeft = true)
        {
            // ── Paddings idénticos al HTML ──────────────────────────────────────────
            // first-child: padL=0.6cm  padR=0.15cm
            // last-child:  padL=0.15cm padR=0.6cm
            double padL = isLeft ? Cm(0.6) : Cm(0.15);
            double padR = isLeft ? Cm(0.15) : Cm(0.6);
            double padT = In(0.23);
            double padB = Cm(0.6);
            double CW = W - padL - padR;

            var white = XBrushes.White;
            var black = XBrushes.Black;

            // ── 1. Gov header ───────────────────────────────────────────────────────
            double govY = padT;
            gfx.DrawString("U.S. Government",
                new XFont("Arial", 8, XFontStyle.Bold), black,
                new XRect(padL, govY, CW * 0.5, Px(12)), FmtTL);
            gfx.DrawString("Federal law prohibits removal of this label before consumer purchase.",
                new XFont("Arial", 8, XFontStyle.Regular), black,
                new XRect(padL + CW * 0.25, govY, CW * 0.75, Px(12)), FmtTR);
            double y = govY + Px(14);

            // ── 2. Logo EnergyGuide ─────────────────────────────────────────────────
            var logoTit = LoadImage("Logo_titulo.png");
            double logoBottom;
            if (logoTit != null)
            {
                double logoH = CW * ((double)logoTit.PixelHeight / logoTit.PixelWidth);
                gfx.DrawImage(logoTit, padL, y, CW, logoH);
                logoBottom = y + logoH;
            }
            else
            {
                double logoH = Cm(1.6);
                gfx.DrawString("EnergyGuide", new XFont("Arial Black", 28, XFontStyle.Bold),
                    black, new XRect(padL, y, CW, logoH), FmtMC);
                logoBottom = y + logoH;
            }
            // margin-bottom:-45px + padding-top:15px = -30px
            y = logoBottom - Px(30);

            // ── 3. Specs ────────────────────────────────────────────────────────────
            double specLineH = 8.5 * 1.2;
            var fSpec = new XFont("Arial", 8.5, XFontStyle.Bold);
            string[] left = { d.REF_TYPE, "• " + d.DEFROST_SYSTEM, "• " + d.DOORTYPE, "• " + d.ICE_SERVICE };
            string[] right = { d.CUST_NAME, "Model: " + d.MODEL, "Capacity: " + d.CAB_SIZE };
            for (int i = 0; i < left.Length; i++)
                gfx.DrawString(left[i], fSpec, black, new XRect(padL, y + i * specLineH, CW * 0.5, specLineH), FmtTL);
            for (int i = 0; i < right.Length; i++)
                gfx.DrawString(right[i], fSpec, black, new XRect(padL + CW * 0.5, y + i * specLineH, CW * 0.5, specLineH), FmtTR);
            y += 4 * specLineH + Px(15);

            // ── 4. Compare box ──────────────────────────────────────────────────────
            double cmpMain = 12 * 1.2, cmpSub = 9 * 1.2;
            double cmpH = Px(10) + cmpMain + Px(3) + cmpSub + Px(10);
            gfx.DrawRectangle(black, padL, y, CW, cmpH);
            gfx.DrawString("Compare ONLY to other labels with yellow numbers.",
                new XFont("Arial Black", 12, XFontStyle.Bold), white,
                new XRect(padL, y + Px(10), CW, cmpMain), FmtTC);
            gfx.DrawString("Labels with yellow numbers are based on the same test procedures.",
                new XFont("Arial", 9, XFontStyle.Bold), white,
                new XRect(padL, y + Px(10) + cmpMain + Px(3), CW, cmpSub), FmtTC);
            y += cmpH + Px(10);

            // ── 5. Cost box ─────────────────────────────────────────────────────────

            // CAMBIO: costH debe incluir espacio para el triángulo DENTRO del rectángulo negro
            double costTitleH = 14 * 1.2;
            double gapTitle = Px(10);          // menos gap que antes
            double numH = Px(44);          // altura real del número (36pt ≈ 48px → 36pt)
            double triH = Px(14);          // altura del triángulo
            double costH = Px(10) + costTitleH + gapTitle + numH + Px(4) + triH + Px(10);

            gfx.DrawRectangle(black, padL, y, CW, costH);

            gfx.DrawString("Estimated Yearly Energy Cost",
                new XFont("Arial Black", 14, XFontStyle.Bold), white,
                new XRect(padL, y + Px(10), CW, costTitleH), FmtTC);

            // Posición horizontal del $91 (margin-left:100px del HTML)
            double scaleLeft = padL + Px(100);
            double scaleW = CW - Px(100) - Px(10);
            double gMin = Math.Min(d.LOW_AMOUNT, Math.Min(d.LOW_SIMILAR_MODEL, d.ENERGY_COST));
            double gMax = Math.Max(d.HIGH_AMOUNT, Math.Max(d.HIGH_SIMILAR_MODEL, d.ENERGY_COST));
            double pct = (gMax == gMin) ? 0.5 : (d.ENERGY_COST - gMin) / (double)(gMax - gMin);
            pct = Math.Max(0, Math.Min(1, pct));
            double cx = scaleLeft + pct * scaleW;

            double valTopCost = y + Px(10) + costTitleH + gapTitle;
            var fDollar = new XFont("Arial Black", 24, XFontStyle.Bold);
            var fNum = new XFont("Arial Black", 36, XFontStyle.Bold);
            string numStr = d.ENERGY_COST.ToString();
            double wNum = gfx.MeasureString(numStr, fNum).Width;
            double wDol = gfx.MeasureString("$", fDollar).Width;
            double gLeft = cx - (wDol + Px(2) + wNum) / 2;

            // $ y número alineados al centro vertical del numH
            gfx.DrawString("$", fDollar, white,
                new XRect(gLeft, valTopCost, wDol + Px(2), numH), FmtML);
            gfx.DrawString(numStr, fNum, white,
                new XRect(gLeft + wDol + Px(2), valTopCost, wNum + Px(4), numH), FmtML);

            // Triángulo — DENTRO del rectángulo negro, justo bajo el número
            double triTopY = valTopCost + numH + Px(4);   // Px(4) gap mínimo
            double triApexY = triTopY + triH;
            gfx.DrawPolygon(white, new[]
            {
    new XPoint(cx - Px(10), triTopY),
    new XPoint(cx + Px(10), triTopY),
    new XPoint(cx,          triApexY),
}, XFillMode.Winding);

            y += costH;   // SIN margen extra — ranges va pegado

            // ── 6. Cost Ranges ──────────────────────────────────────────────────────
            double rngH = Px(88);
            gfx.DrawRoundedRectangle(black, padL, y, CW, rngH, Px(10), Px(10));
            gfx.DrawRectangle(black, padL, y, CW, Px(12));

            double innerLeft = padL + Px(5);
            double innerTop = y + Px(8);
            double innerW = CW - Px(10);
            double innerH2 = rngH - Px(16);

            gfx.Save();
            double sbCx = innerLeft + Px(12.5);
            double sbCy = innerTop + innerH2 / 2;
            gfx.RotateAtTransform(-90, new XPoint(sbCx, sbCy));
            gfx.DrawString("Cost Ranges", new XFont("Arial", 9, XFontStyle.Regular), white,
                new XRect(sbCx - Px(40), sbCy - Px(8), Px(80), Px(16)), FmtMC);
            gfx.Restore();

            double boxLeft = innerLeft + Px(25);
            double boxW = innerW - Px(25);
            gfx.DrawRoundedRectangle(new XPen(XColors.White, 1), boxLeft, innerTop, boxW, innerH2, Px(10), Px(10));

            double labelW = Px(105);
            double trackLeft = boxLeft + labelW;
            double trackW = boxW - labelW - Px(12);
            double rowH = innerH2 / 2;
            double pillH = Px(18);

            double r1cy = innerTop + rowH / 2;
            gfx.DrawString("Models with", new XFont("Arial", 9, XFontStyle.Regular), white,
                new XRect(boxLeft + Px(6), r1cy - Px(11), labelW, Px(11)), FmtML);
            gfx.DrawString("similar features", new XFont("Arial", 9, XFontStyle.Regular), white,
                new XRect(boxLeft + Px(6), r1cy + Px(1), labelW, Px(11)), FmtML);
            DrawPill(gfx, d.LOW_SIMILAR_MODEL, d.HIGH_SIMILAR_MODEL,
                     trackLeft, r1cy - pillH / 2, trackW, pillH, gMin, gMax, true);

            double r2cy = innerTop + rowH + rowH / 2;
            gfx.DrawString("All models", new XFont("Arial", 10, XFontStyle.Regular), white,
                new XRect(boxLeft + Px(6), r2cy - Px(5.5), labelW, Px(11)), FmtML);
            DrawPill(gfx, d.LOW_AMOUNT, d.HIGH_AMOUNT,
                     trackLeft, r2cy - pillH / 2, trackW, pillH, gMin, gMax, false);

            y += rngH + Px(20);

            // ── 7. kWh box ──────────────────────────────────────────────────────────
            double kwhH = Px(94);
            double kwhBoxW = CW * 0.5;
            double kwhBoxX = padL + (CW - kwhBoxW) / 2;
            gfx.DrawRectangle(black, kwhBoxX, y, kwhBoxW, kwhH);

            var fKwh = new XFont("Arial Black", 34, XFontStyle.Bold);
            var fUnit = new XFont("Arial Black", 14, XFontStyle.Bold);
            string kwhStr = d.ELECTRICITY_USE.ToString();
            double wKwh = gfx.MeasureString(kwhStr, fKwh).Width;
            double wUnit = gfx.MeasureString("kWh", fUnit).Width;
            double grpLeft = kwhBoxX + (kwhBoxW - (wKwh + Px(5) + wUnit)) / 2;
            double valTop = y + Px(20);
            gfx.DrawString(kwhStr, fKwh, white,
                new XRect(grpLeft, valTop, wKwh + Px(4), Px(44)), FmtBL);
            gfx.DrawString("kWh", fUnit, white,
                new XRect(grpLeft + wKwh + Px(5), valTop, wUnit + Px(4), Px(44)), FmtBL);
            gfx.DrawString("Estimated Yearly Electricity Use",
                new XFont("Arial", 9, XFontStyle.Bold), white,
                new XRect(kwhBoxX, valTop + Px(44) + Px(5), kwhBoxW, Px(12)), FmtTC);
            y += kwhH;   // sin margen extra — el footer toma el resto

            // ── 8. Footer dinámico (notas desde y, ftc.gov anclado al fondo) ────────
            double ftcH2 = Px(20);
            double ftcY = H - padB - ftcH2 + Cm(.2);

            gfx.DrawString("ftc.gov/energy",
                new XFont("Arial", 12, XFontStyle.Regular), black,
                new XRect(padL, ftcY, CW, ftcH2), FmtTC);

            // Zona de notas: entre el kWh y el ftc link
            double notesTop = y + Px(6);
            double notesW = CW * 0.72;
            double starW = Px(85);
            double starX = padL + CW - starW;

            var tf = new XTextFormatter(gfx);
            var notes = new (bool Bold, string T)[]
            {
        (true,  "Your cost will depend on your utility rates and use."),
        (false, "Both cost ranges based on models of similar size capacity."),
        (false, "Models with similar features have automatic defrost, side-mounted freezer, and through-the-door ice."),
        (false, "Estimated energy cost is based on a national average electricity cost of 14 cents per kWh.")
            };

            double ny = notesTop;
            foreach (var n in notes)
            {
                var fn = new XFont("Arial", 8, n.Bold ? XFontStyle.Bold : XFontStyle.Regular);
                gfx.DrawString("•", new XFont("Arial", 8, XFontStyle.Bold), black,
                    new XRect(padL, ny, Px(10), Px(11)), FmtTL);
                int lineas = (int)Math.Ceiling((double)n.T.Length / 65.0);
                double altH = lineas * Px(11);
                tf.DrawString(n.T, fn, black,
                    new XRect(padL + Px(12), ny, notesW - Px(12), altH + Px(10)));
                ny += altH + Px(4);
            }

            // Logo y PART NO. alineados en la esquina derecha del área de notas
            var logoPie = LoadImage("Logo_pie.png");
            double sH = starW;
            if (logoPie != null)
                sH = starW * ((double)logoPie.PixelHeight / logoPie.PixelWidth);

            // Centrar verticalmente entre notesTop y ftcY
            double logoY = notesTop + (ftcY - notesTop - sH - Px(14)) / 2;
            logoY = Math.Max(notesTop, logoY);   // nunca subir del borde de notas

            if (d.ENERGY_LOGO != null && d.ENERGY_LOGO.ToUpper() == "Y" && logoPie != null)
                gfx.DrawImage(logoPie, starX, logoY, starW, sH);

            gfx.DrawString("PART NO. " + d.PART_NUMBER,
                new XFont("Arial", 7, XFontStyle.Bold), black,
                new XRect(starX, logoY + sH + Px(4), starW + Px(50), Px(10)), FmtTL);
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

            double minW = Cm(1.5);
            if (w < minW) w = minW;
            if (left + w > tX + tW) left = tX + tW - w;

            gfx.DrawRoundedRectangle(XBrushes.White, left, tY, w, tH, tH, tH);

            var f = new XFont("Arial", 9, XFontStyle.Bold);   // un poco menor para que entre

            // $low — izquierda de la pill con padding interno
            gfx.DrawString("$" + low, f, XBrushes.Black,
                new XRect(left + Px(5), tY, w / 2 - Px(5), tH), FmtML);

            // $high — derecha de la pill con padding interno
            gfx.DrawString("$" + high, f, XBrushes.Black,
                new XRect(left + w / 2, tY, w / 2 - Px(5), tH), FmtMR);

            if (sep)
                gfx.DrawRectangle(XBrushes.Black, left, tY, Px(3), tH);
        }
    }
}