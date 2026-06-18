using Energy_printer.Models;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;

namespace Energy_printer.Services
{
    // ══════════════════════════════════════════════════════════════════════
    //  ETIQUETA USA (EnergyGuide)
    // ══════════════════════════════════════════════════════════════════════
    public class EnergyLabelServiceUSA : EnergyLabelHelpersBase
    {
        public EnergyLabelServiceUSA(string contentPath) : base(contentPath)
        {
        }

        // ══════════════════════════════════════════════════════════════════════
        //  MAPEO DESDE DATA_LABEL (Entity Framework)
        // ══════════════════════════════════════════════════════════════════════

        public EnergyLabelDataUSA FromDataLabel(DATA_LABEL d)
        {
            return new EnergyLabelDataUSA
            {
                REF_TYPE = d.REF_TYPE,
                DEFROST_SYSTEM = d.DEFROST_SYSTEM,
                DOORTYPE = d.DOORTYPE,
                ICE_SERVICE = d.ICE_SERVICE,
                CUST_NAME = d.CUST_NAME,
                MODEL = d.MODEL,
                CAB_SIZE = d.CAB_SIZE,
                PART_NUMBER = d.PART_NUMBER,
                ENERGY_COST = d.ENERGY_COST,
                LOW_SIMILAR_MODEL = d.LOW_SIMILAR_MODEL,
                HIGH_SIMILAR_MODEL = d.HIGH_SIMILAR_MODEL,
                LOW_AMOUNT = d.LOW_AMOUNT,
                HIGH_AMOUNT = d.HIGH_AMOUNT,
                ELECTRICITY_USE = d.MODEL_KW,
                ENERGY_LOGO = d.ENERGY_LOGO,
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GENERACIÓN DE LA PÁGINA PDF
        // ══════════════════════════════════════════════════════════════════════

        public void AddUSAPage(PdfDocument doc, EnergyLabelDataUSA d)
        {
            var page = doc.AddPage();
            page.Width = XUnit.FromCentimeter(14.5);
            page.Height = XUnit.FromCentimeter(19.1);
            page.Orientation = PdfSharp.PageOrientation.Portrait;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                DrawUSALabel(gfx, d, page.Width.Point, page.Height.Point);
            }
        }

        private void DrawUSALabel(XGraphics gfx, EnergyLabelDataUSA d, double W, double H)
        {
            double pad = Cm(0.6);          // .usa-container padding
            double CW = W - pad * 2;      // ancho de contenido (100%)
            var white = XBrushes.White;
            var black = XBrushes.Black;

            // ── 1. Gov header  (margin-top 0.3cm, Arial 8pt) ─────────────────────
            double govY = pad + Cm(0.3);
            gfx.DrawString("U.S. Government",
                new XFont("Arial", 8, XFontStyleEx.Bold), black,
                new XRect(pad, govY, CW * 0.5, Px(12)), FmtTL);
            gfx.DrawString("Federal law prohibits removal of this label before consumer purchase.",
                new XFont("Arial", 8, XFontStyleEx.Regular), black,
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
                gfx.DrawString("EnergyGuide", new XFont("Arial Black", 28, XFontStyleEx.Bold),
                    black, new XRect(pad, logoTop, CW, logoH), FmtMC);
                logoBottom = logoTop + logoH;
            }
            // .usa-main-title margin-bottom:-45px  +  .specs-table padding-top:15px = -30px
            y = logoBottom - Px(30);

            // ── 3. Specs  (Arial 8.5pt bold, line-height 1.2) ────────────────────
            double specLineH = 8.5 * 1.2;
            var fSpec = new XFont("Arial", 8.5, XFontStyleEx.Bold);
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
                new XFont("Arial Black", 12, XFontStyleEx.Bold), white,
                new XRect(pad, y + Px(10), CW, cmpMain), FmtTC);
            gfx.DrawString("Labels with yellow numbers are based on the same test procedures.",
                new XFont("Arial", 9, XFontStyleEx.Bold), white,
                new XRect(pad, y + Px(10) + cmpMain + Px(3), CW, cmpSub), FmtTC);
            y += cmpH + Px(10);   // margin-bottom 10px

            // ── 5. Cost box  (más aire, números Arial Bold, título separado) ──
            double costTitleH = 14 * 1.2;
            double gapTitle = Px(25);                 // separación título ↔ número
            double numH = Px(20);                     // banda del número
            double costH = Px(12) + costTitleH + gapTitle + numH + Px(6) + Px(12) + Px(8) + Px(10);
            gfx.DrawRectangle(black, pad, y, CW, costH);
            gfx.DrawString("Estimated Yearly Energy Cost",
                new XFont("Arial Black", 18, XFontStyleEx.Bold), white,
                new XRect(pad, y + Px(12), CW, costTitleH), FmtTC);

            // posición horizontal según el pct del costo
            double costInnerW = CW - 2 * Px(15);
            double scaleLeft = pad + Px(15) + Px(100);
            double scaleW = costInnerW - Px(110);
            double gMin = Math.Min((sbyte) d.LOW_AMOUNT, Math.Min((sbyte) d.LOW_SIMILAR_MODEL, (sbyte) d.ENERGY_COST));
            double gMax = Math.Max((sbyte) d.HIGH_AMOUNT, Math.Max((sbyte) d.HIGH_SIMILAR_MODEL, (sbyte) d.ENERGY_COST));
            double pct = (gMax == gMin) ? 0.5 : (double) (d.ENERGY_COST - gMin) / (double)(gMax - gMin);
            pct = Math.Max(0, Math.Min(1, pct));
            double cx = scaleLeft + pct * scaleW;

            // valor DEBAJO del título — Arial Bold (no Arial Black)
            double valTopCost = y + Px(12) + costTitleH + gapTitle;
            var fDollar = new XFont("Arial Black", 26, XFontStyleEx.Bold);
            var fNum = new XFont("Arial Black", 38, XFontStyleEx.Bold);
            string numStr = d.ENERGY_COST.ToString();
            double wNum = gfx.MeasureString(numStr, fNum).Width;
            double wDol = gfx.MeasureString("$", fDollar).Width;
            double gap = Px(4);
            double gLeft = cx - (wDol + gap + wNum) / 2;
            gfx.DrawString(numStr, fNum, white,
                new XRect((double) gLeft + wDol + gap, valTopCost, wNum + Px(4), numH), FmtML);
            gfx.DrawString("$", fDollar, white,
                new XRect((double) gLeft, valTopCost, wDol + Px(2), numH), FmtML);

            // triángulo abajo del número
            double triTopY = valTopCost + numH + 15;
            double triApexY = triTopY + Px(12);
            gfx.DrawPolygon(white, new[]
            {
                new XPoint((double) cx - Px(10), triTopY),
                new XPoint((double) cx + Px(10), triTopY),
                new XPoint((double) cx,          triApexY),
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
            gfx.DrawString("Cost Ranges", new XFont("Arial", 9, XFontStyleEx.Regular), white,
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
            gfx.DrawString("Models with", new XFont("Arial", 9, XFontStyleEx.Regular), white,
                new XRect(boxLeft + Px(6), r1cy - Px(11), labelW, Px(11)), FmtML);
            gfx.DrawString("similar features", new XFont("Arial", 9, XFontStyleEx.Regular), white,
                new XRect(boxLeft + Px(6), r1cy + Px(1), labelW, Px(11)), FmtML);
            DrawPill(gfx, (sbyte) d.LOW_SIMILAR_MODEL, (sbyte) d.HIGH_SIMILAR_MODEL,
                     trackLeft, r1cy - pillH / 2, trackW, pillH, gMin, gMax, true);

            // Fila 2: All models
            double r2cy = innerTop + rowH + rowH / 2;
            gfx.DrawString("All models", new XFont("Arial", 10, XFontStyleEx.Regular), white,
                new XRect(boxLeft + Px(6), r2cy - Px(5.5), labelW, Px(11)), FmtML);
            DrawPill(gfx, (sbyte) d.LOW_AMOUNT, (sbyte) d.HIGH_AMOUNT,
                     trackLeft, r2cy - pillH / 2, trackW, pillH, gMin, gMax, false);

            y += rngH + Px(20);   // margin-bottom 20px

            // ── 7. kWh box  (negro, width 50%, centrado) ─────────────────────────
            double kwhH = Px(94);
            double kwhBoxW = CW * 0.5;
            double kwhBoxX = pad + (CW - kwhBoxW) / 2;
            gfx.DrawRectangle(black, kwhBoxX, y, kwhBoxW, kwhH);

            var fKwh = new XFont("Arial Black", 34, XFontStyleEx.Bold);
            var fUnit = new XFont("Arial Black", 14, XFontStyleEx.Bold);
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
                new XFont("Arial", 9, XFontStyleEx.Bold), white,
                new XRect(kwhBoxX, valTop + Px(44) + Px(5), kwhBoxW, Px(12)), FmtTC);
            y += kwhH + Px(15);

            // ── 8. Footer + ftc.gov/energy  (anclados al fondo) ──────────────────
            double ftcH = Px(20);
            double ftcY = H - pad - ftcH;

            gfx.DrawString("ftc.gov/energy",
                new XFont("Arial", 12, XFontStyleEx.Regular),
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
                var fn = new XFont("Arial", 8, n.Bold ? XFontStyleEx.Bold : XFontStyleEx.Regular);

                // Viñeta (siempre en negrita para que parezca un bullet point real)
                gfx.DrawString("•", new XFont("Arial", 8, XFontStyleEx.Bold), black,
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
                new XFont("Arial", 7, XFontStyleEx.Bold),
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

            var f = new XFont("Arial Black", 10, XFontStyleEx.Bold);
            gfx.DrawString("$" + low, f, XBrushes.Black,
                new XRect(left + Px(6), tY, w * 0.5, tH), FmtML);
            gfx.DrawString("$" + high, f, XBrushes.Black,
                new XRect(left + w - Px(6) - Cm(1), tY, Cm(1), tH), FmtMR);

            if (sep)   // .separador-sim (3px negro en el borde izquierdo)
                gfx.DrawRectangle(XBrushes.Black, left, tY, Px(3), tH);
        }
    }
}
