using Energy_printer.Models;
using PdfSharp;
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
        //  MAPEO DESDE DATA_LABEL
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
                ENERGY_LOGO = d.ENERGY_LOGO
            };
        }

        // ══════════════════════════════════════════════════════════════════════
        //  GENERACIÓN DE PÁGINA PDF
        // ══════════════════════════════════════════════════════════════════════
        public void AddUSAPage(PdfDocument doc, EnergyLabelDataUSA d, bool isLeft)
        {
            var page = doc.AddPage();

            // Si quieres EXACTO igual al HTML, usa 14cm x 19cm.
            // Si tu papel real necesita 14.5 x 19.1, cambia solo estas 2 líneas.
            page.Width = XUnit.FromCentimeter(14.0);
            page.Height = XUnit.FromCentimeter(19.0);
            page.Orientation = PageOrientation.Portrait;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                DrawUSALabel(gfx, d, page.Width.Point, page.Height.Point, isLeft);
            }
        }

        private void DrawUSALabel(XGraphics gfx, EnergyLabelDataUSA d, double W, double H, bool isLeft)
        {
            var white = XBrushes.White;
            var black = XBrushes.Black;
            var grayText = new XSolidBrush(XColor.FromArgb(110, 110, 110));

            double padT = Cm(0.8);
            double padB = Cm(0.6);
            double padL = isLeft ? Cm(0.6) : Cm(0.15);
            double padR = isLeft ? Cm(0.15) : Cm(0.6);
            double CW = W - padL - padR;

            // ── 1. Gov header (sin margin-top extra) ─────────────────────
            double govY = padT;

            gfx.DrawString(
                "U.S. Government",
                new XFont("Arial", 8, XFontStyleEx.Regular),
                black,
                new XRect(padL, govY, CW * 0.5, Px(12)),
                FmtTL);

            gfx.DrawString(
                "Federal law prohibits removal of this label before consumer purchase.",
                new XFont("Arial", 8, XFontStyleEx.Regular),
                black,
                new XRect(padL + CW * 0.25, govY, CW * 0.75, Px(12)),
                FmtTR);

            double y = govY + Px(14);

            // ── 2. Logo EnergyGuide ──────────────────────────────────────
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
                gfx.DrawString(
                    "EnergyGuide",
                    new XFont("Arial Black", 28, XFontStyleEx.Bold),
                    black,
                    new XRect(padL, y, CW, logoH),
                    FmtMC);
                logoBottom = y + logoH;
            }

            y = logoBottom - Px(30);

            // ── 3. Specs ─────────────────────────────────────────────────
            double specLineH = 8.5 * 1.2;
            var fSpec = new XFont("Arial", 8.5, XFontStyleEx.Bold);

            string[] left =
            {
                d.REF_TYPE,
                "• " + d.DEFROST_SYSTEM,
                "• " + d.DOORTYPE,
                "• " + d.ICE_SERVICE
            };

            string[] right =
            {
                d.CUST_NAME,
                "Model: " + d.MODEL,
                "Capacity: " + d.CAB_SIZE
            };

            double centerGap = Cm(0.05);
            double colW = (CW - centerGap) / 2;

            for (int i = 0; i < left.Length; i++)
            {
                gfx.DrawString(
                    left[i],
                    fSpec,
                    black,
                    new XRect(padL, y + i * specLineH, colW, specLineH),
                    FmtTL);
            }

            for (int i = 0; i < right.Length; i++)
            {
                gfx.DrawString(
                    right[i],
                    fSpec,
                    black,
                    new XRect(padL + colW + centerGap, y + i * specLineH, colW, specLineH),
                    FmtTR);
            }

            y += 4 * specLineH + Px(15);

            // ── 4. Compare box ───────────────────────────────────────────
            double cmpMain = 14 * 1.2;
            double cmpSub = 11 * 1.2;
            double cmpH = Px(10) + cmpMain + Px(3) + cmpSub + Px(10);

            gfx.DrawRectangle(black, padL, y, CW, cmpH);

            gfx.DrawString(
                "Compare ONLY to other labels with yellow numbers.",
                new XFont("Arial", 14, XFontStyleEx.Bold),
                white,
                new XRect(padL, y + Px(10), CW, cmpMain),
                FmtTC);

            gfx.DrawString(
                "Labels with yellow numbers are based on the same test procedures.",
                new XFont("Arial", 11, XFontStyleEx.Bold),
                white,
                new XRect(padL, y + Px(10) + cmpMain + Px(3), CW, cmpSub),
                FmtTC);

            y += cmpH + Px(10);

            // ── 5. Cost box ─────────────────────────────────────────────

            double costTitleH = 14 * 1.2;
            double costTopPad = Px(10);
            double costGap = Px(20);          // 5px margin-bottom + 5px margin-top
            double pointerAreaH = Px(50);
            double costH = costTopPad + costTitleH + costGap + pointerAreaH;

            gfx.DrawRectangle(black, padL, y, CW, costH);

            gfx.DrawString(
                "Estimated Yearly Energy Cost",
                new XFont("Arial", 14, XFontStyleEx.Bold),
                white,
                new XRect(padL, y + costTopPad, CW, costTitleH),
                FmtTC);

            double scaleLeft = padL + Px(100);
            double scaleW = CW - Px(110);

            int lowAmount = d.LOW_AMOUNT ?? 0;
            int lowSimilarModel = d.LOW_SIMILAR_MODEL ?? 0;
            int energyCost = d.ENERGY_COST ?? 0;
            int highAmount = d.HIGH_AMOUNT ?? 0;
            int highSimilarModel = d.HIGH_SIMILAR_MODEL ?? 0;

            double gMin = Math.Min(lowAmount, Math.Min(lowSimilarModel, energyCost));
            double gMax = Math.Max(highAmount, Math.Max(highSimilarModel, energyCost));

            double pct = (double)((gMax == gMin) ? 0.5 : (d.ENERGY_COST - gMin) / (double)(gMax - gMin));
            pct = Math.Max(0, Math.Min(1, pct));

            double cx = scaleLeft + pct * scaleW;

            // $ 24pt / número 36pt como el HTML
            var fDollar = new XFont("Arial", 35, XFontStyleEx.Bold);
            var fNum = new XFont("Arial", 45, XFontStyleEx.Bold);

            string numStr = d.ENERGY_COST.ToString();
            double wNum = gfx.MeasureString(numStr, fNum).Width;
            double wDol = gfx.MeasureString("$", fDollar).Width;

            // ── Ajuste fino del puntero (flecha) ────────────────────────────────
            double pointerTop = y + costTopPad + costTitleH + costGap;
            double pointerBottom = pointerTop + pointerAreaH;

            // Flecha un poco más chica
            double triH = Px(10);
            double triHalfW = Px(8);

            // Más aire entre número y flecha
            double numH = Px(28);
            double gap = Px(13);

            // Evitar que la flecha se recorte en los extremos
            cx = Math.Max(scaleLeft + triHalfW, Math.Min(scaleLeft + scaleW - triHalfW, cx));

            // Subir un poquito la flecha para que no quede tan pegada a la línea blanca
            double triApexY = pointerBottom - Px(6);
            double triTopY = triApexY - triH;

            // Colocar el número justo arriba de la flecha
            double valTopCost = triTopY - numH - gap;

            double gLeft = cx - (wDol + Px(2) + wNum) / 2;

            gfx.DrawString(
                "$",
                fDollar,
                white,
                new XRect(gLeft, valTopCost, wDol + Px(2), numH),
                FmtML);

            gfx.DrawString(
                numStr,
                fNum,
                white,
                new XRect(gLeft + wDol + Px(2), valTopCost, wNum + Px(4), numH),
                FmtML);

            // Flecha blanca apuntando hacia abajo
            gfx.DrawPolygon(
                white,
                new[]
                {
                    new XPoint(cx - triHalfW, triTopY),
                    new XPoint(cx + triHalfW, triTopY),
                    new XPoint(cx,            triApexY)
                },
                XFillMode.Winding);

            y += costH;

            // ── 6. Cost Ranges ────────────────────────────────────────────
            double rngH = Px(88);

            gfx.DrawRoundedRectangle(black, padL, y, CW, rngH, Px(10), Px(10));
            gfx.DrawRectangle(black, padL, y, CW, Px(12));

            double innerLeft = padL + Px(5);
            double innerTop = y + Px(8);
            double innerW = CW - Px(10);
            double innerH = rngH - Px(16);

            // sidebar "Cost Ranges"
            gfx.Save();
            double sbCx = innerLeft + Px(12.5);
            double sbCy = innerTop + innerH / 2;

            gfx.RotateAtTransform(-90, new XPoint(sbCx, sbCy));
            gfx.DrawString(
                "Cost Ranges",
                new XFont("Arial", 7.5, XFontStyleEx.Bold),
                white,
                new XRect(sbCx - Px(40), sbCy - Px(8), Px(80), Px(16)),
                FmtMC);
            gfx.Restore();

            // borde blanco interno
            double boxLeft = innerLeft + Px(25);
            double boxW = innerW - Px(25);

            gfx.DrawRoundedRectangle(
                new XPen(XColors.White, 1),
                boxLeft,
                innerTop,
                boxW,
                innerH,
                Px(10),
                Px(10));

            double labelW = Px(105);
            double trackLeft = boxLeft + labelW;
            double trackW = boxW - labelW - Px(12);
            double rowH = innerH / 2;
            double pillH = Px(18);

            // fila 1
            double r1cy = innerTop + rowH / 2;

            gfx.DrawString(
                "Models with",
                new XFont("Arial", 8, XFontStyleEx.Bold),
                white,
                new XRect(boxLeft + Px(6), r1cy - Px(11), labelW, Px(11)),
                FmtML);

            gfx.DrawString(
                "similar features",
                new XFont("Arial", 8, XFontStyleEx.Bold),
                white,
                new XRect(boxLeft + Px(6), r1cy + Px(1), labelW, Px(11)),
                FmtML);

            DrawPill(
                gfx,
                (int)d.LOW_SIMILAR_MODEL,
                (int)d.HIGH_SIMILAR_MODEL,
                trackLeft,
                r1cy - pillH / 2,
                trackW,
                pillH,
                gMin,
                gMax,
                true);

            // fila 2
            double r2cy = innerTop + rowH + rowH / 2;

            gfx.DrawString(
                "All models",
                new XFont("Arial", 8, XFontStyleEx.Bold),
                white,
                new XRect(boxLeft + Px(6), r2cy - Px(5.5), labelW, Px(11)),
                FmtML);

            DrawPill(
                gfx,
                (int)d.LOW_AMOUNT,
                (int)d.HIGH_AMOUNT,
                trackLeft,
                r2cy - pillH / 2,
                trackW,
                pillH,
                gMin,
                gMax,
                false);

            y += rngH + Px(20);

            // ── 7. kWh box ────────────────────────────────────────────────
            double kwhH = Px(94);
            double kwhBoxW = CW * 0.5;
            double kwhBoxX = padL + (CW - kwhBoxW) / 2;

            gfx.DrawRectangle(black, kwhBoxX, y, kwhBoxW, kwhH);

            var fKwh = new XFont("Arial", 42, XFontStyleEx.Bold);
            var fUnit = new XFont("Arial", 15, XFontStyleEx.Bold);

            string kwhStr = d.ELECTRICITY_USE.ToString();
            double wKwh = gfx.MeasureString(kwhStr, fKwh).Width;
            double wUnit = gfx.MeasureString("kWh", fUnit).Width;

            double grpLeft = kwhBoxX + (kwhBoxW - (wKwh + Px(5) + wUnit)) / 2;
            double valTop = y + Px(20);

            gfx.DrawString(
                kwhStr,
                fKwh,
                white,
                new XRect(grpLeft, valTop, wKwh + Px(4), Px(44)),
                FmtBL);

            gfx.DrawString(
                "kWh",
                fUnit,
                white,
                new XRect(grpLeft + wKwh + Px(5), valTop, wUnit + Px(4), Px(44)),
                FmtBL);

            gfx.DrawString(
                "Estimated Yearly Electricity Use",
                new XFont("Arial", 11, XFontStyleEx.Bold),
                white,
                new XRect(kwhBoxX, valTop + Px(44) + Px(5), kwhBoxW, Px(12)),
                FmtTC);

            y += kwhH;

            // ── 8. Footer dinámico + ftc.gov/energy ─────────────────────
            double ftcH = Px(20);
            double ftcY = H - padB - ftcH;

            gfx.DrawString(
                "ftc.gov/energy",
                new XFont("Arial", 12, XFontStyleEx.Regular),
                black,
                new XRect(padL, ftcY, CW, ftcH),
                FmtTC);

            // zona libre entre kWh y ftc.gov
            double notesTop = y + Px(6);
            double notesBottom = ftcY - Px(6);

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
                var fn = new XFont("Arial", 8, n.Bold ? XFontStyleEx.Bold : XFontStyleEx.Regular);

                gfx.DrawString(
                    "•",
                    new XFont("Arial", 8, XFontStyleEx.Bold),
                    black,
                    new XRect(padL, ny, Px(10), Px(11)),
                    FmtTL);

                int lineas = (int)Math.Ceiling((double)n.T.Length / 65.0);
                double alturaTexto = lineas * Px(11);

                tf.DrawString(
                    n.T,
                    fn,
                    black,
                    new XRect(padL + Px(12), ny, notesW - Px(12), alturaTexto + Px(10)));

                ny += alturaTexto + Px(4);
            }

            // logo + part no
            var logoPie = LoadImage("Logo_pie.png");
            double sH = starW;

            if (logoPie != null)
            {
                sH = starW * ((double)logoPie.PixelHeight / logoPie.PixelWidth);
            }

            double logoY = notesTop + (notesBottom - notesTop - sH - Px(14)) / 2;
            logoY = Math.Max(notesTop, logoY);

            if (!string.IsNullOrWhiteSpace(d.ENERGY_LOGO) &&
                d.ENERGY_LOGO.ToUpper() == "Y" &&
                logoPie != null)
            {
                gfx.DrawImage(logoPie, starX, logoY, starW, sH);
            }

            gfx.DrawString(
                "PART NO. " + d.PART_NUMBER,
                new XFont("Arial", 7, XFontStyleEx.Bold),
                black,
                new XRect(starX, logoY + sH + Px(4), starW + Px(50), Px(10)),
                FmtTL);
        }

        // ── Pill blanca con texto gris como en el HTML ajustado ───────────
        private void DrawPill(
            XGraphics gfx,
            int low,
            int high,
            double tX,
            double tY,
            double tW,
            double tH,
            double gMin,
            double gMax,
            bool sep)
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

            var f = new XFont("Arial", 11, XFontStyleEx.Bold);
            var grayText = XBrushes.Black;


            gfx.DrawString(
                "$" + low,
                f,
                grayText,
                new XRect(left + Px(6), tY, w / 2 - Px(6), tH),
                FmtML);

            gfx.DrawString(
                "$" + high,
                f,
                grayText,
                new XRect(left + w / 2, tY, w / 2 - Px(6), tH),
                FmtMR);

            if (sep)
            {
                gfx.DrawRectangle(XBrushes.Black, left, tY, Px(3), tH);
            }
        }
    }
}