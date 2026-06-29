using Energy_printer.Models;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Energy_printer.Services
{
    public class EnergyLabelServiceUSA : EnergyLabelHelpersBase
    {
        public EnergyLabelServiceUSA(string contentPath) : base(contentPath)
        {
        }

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

        public void AddUSASheet(PdfDocument doc, EnergyLabelDataUSA d, object config)
        {
            object usaConfig = GetUsaConfig(config);

            var page = doc.AddPage();
            page.Width = XUnit.FromCentimeter(27.8);
            page.Height = XUnit.FromCentimeter(21.5);

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                double labelW = Cm(14.0);
                double labelH = Cm(21.5);

                DrawUSALabel(
                    gfx,
                    d,
                    usaConfig,
                    x: Cm(0),
                    y: Cm(0),
                    w: labelW,
                    h: labelH,
                    labelIndex: 1
                );

                DrawUSALabel(
                    gfx,
                    d,
                    usaConfig,
                    x: Cm(14.0),
                    y: Cm(0),
                    w: labelW,
                    h: labelH,
                    labelIndex: 2
                );
            }
        }

        private void DrawUSALabel(
            XGraphics gfx,
            EnergyLabelDataUSA d,
            object cfg,
            double x,
            double y,
            double w,
            double h,
            int labelIndex)
        {
            double mt = Cm(labelIndex == 1 ? GetCfg(cfg, "MARG_TOP_1", 3.9) : GetCfg(cfg, "MARG_TOP_2", 3.9));
            double mb = Cm(labelIndex == 1 ? GetCfg(cfg, "MARG_BOTTOM_1", 0.4) : GetCfg(cfg, "MARG_BOTTOM_2", 0.4));
            double ml = Cm(labelIndex == 1 ? GetCfg(cfg, "MARG_LEFT_1", 0.4) : GetCfg(cfg, "MARG_LEFT_2", 0.5));
            double mr = Cm(labelIndex == 1 ? GetCfg(cfg, "MARG_RIGHT_1", 0.2) : GetCfg(cfg, "MARG_RIGHT_2", 0.4));

            double X = x + ml;
            double Y = y + mt;
            double CW = w - ml - mr;
            double CH = h - mt - mb;

            XBrush black = XBrushes.Black;
            XBrush white = XBrushes.White;

            XFont fGovLeft = new XFont("Arial Narrow", 10, XFontStyleEx.Regular);
            XFont fGovRight = new XFont("Arial Narrow", 9.5, XFontStyleEx.Regular);
            XFont fSpec = new XFont("Arial Narrow", 9.5, XFontStyleEx.Bold);
            XFont fCompareMain = new XFont("Arial Narrow", 15.6, XFontStyleEx.Bold);
            XFont fCompareSub = new XFont("Arial Narrow", 13.1, XFontStyleEx.Bold);
            XFont fCostTitle = new XFont("Arial Narrow", 16, XFontStyleEx.Bold);
            XFont fDollar = new XFont("Arial Black", 38, XFontStyleEx.Bold);
            XFont fCost = new XFont("Arial Black", 50, XFontStyleEx.Bold);
            XFont fRangeLabel = new XFont("Arial Narrow", 9.5, XFontStyleEx.Bold);
            XFont fRangeSide = new XFont("Arial Narrow", 9, XFontStyleEx.Bold);
            XFont fTrack = new XFont("Arial Narrow", 14.5, XFontStyleEx.Bold);
            XFont fKwh = new XFont("Arial Black", 36, XFontStyleEx.Bold);
            XFont fKwhUnit = new XFont("Arial Narrow", 13, XFontStyleEx.Bold);
            XFont fKwhSub = new XFont("Arial Narrow", 11, XFontStyleEx.Bold);
            XFont fFooter = new XFont("Arial Narrow", 9.3, XFontStyleEx.Regular);
            XFont fFtc = new XFont("Arial Narrow", 15, XFontStyleEx.Regular);
            XFont fPart = new XFont("Arial Narrow", 8.5, XFontStyleEx.Bold);

            double currentY = Y;

            DrawGovernmentHeader(gfx, X, currentY, CW, fGovLeft, fGovRight, black);
            currentY += Cm(0.55);

            DrawEnergyGuideLogo(gfx, X, currentY, CW);
            currentY += Cm(1.35);

            currentY -= Px(46);

            DrawSpecs(gfx, d, X, currentY, CW, fSpec, black);
            currentY += Cm(1.62);

            DrawCompareBox(gfx, X, currentY, CW, fCompareMain, fCompareSub, black, white);
            currentY += Cm(1.8);

            DrawCostBox(gfx, d, X, currentY, CW, fCostTitle, fDollar, fCost, black, white);
            currentY += Cm(2.9);

            DrawRanges(gfx, d, X, currentY, CW, fRangeSide, fRangeLabel, fTrack, black, white);
            currentY += Cm(2.39);

            DrawKwhBox(gfx, d, X, currentY, CW, fKwh, fKwhUnit, fKwhSub, black, white);
            currentY += Cm(2.72);

            DrawFooter(gfx, d, X, Y, CW, CH, fFooter, fFtc, fPart, black);
        }

        private object GetUsaConfig(object config)
        {
            if (config == null)
                return null;

            var enumerable = config as System.Collections.IEnumerable;

            if (enumerable == null)
                return null;

            foreach (object item in enumerable)
            {
                if (item == null)
                    continue;

                var prop = item.GetType().GetProperty("LABEL_TYPE");

                if (prop == null)
                    continue;

                object value = prop.GetValue(item, null);

                if (value == null)
                    continue;

                string labelType = Convert.ToString(value);

                if (labelType.Equals("USA", StringComparison.OrdinalIgnoreCase))
                    return item;
            }

            return null;
        }

        private double GetCfg(object cfg, string propertyName, double fallback)
        {
            if (cfg == null)
                return fallback;

            var prop = cfg.GetType().GetProperty(propertyName);

            if (prop == null)
                return fallback;

            object value = prop.GetValue(cfg, null);

            return ToDoubleSafe(value, fallback);
        }

        private void DrawGovernmentHeader(
            XGraphics gfx,
            double x,
            double y,
            double w,
            XFont fLeft,
            XFont fRight,
            XBrush black)
        {
            gfx.DrawString(
                "U.S. Government",
                fLeft,
                black,
                new XRect(x + Cm(0.3), y, w * 0.45, Cm(0.35)),
                FmtTL
            );

            gfx.DrawString(
                "Federal law prohibits removal of this label before consumer purchase.",
                fRight,
                black,
                new XRect(x + w * 0.25, y + Cm(0.05), w * 0.72, Cm(0.35)),
                FmtTR
            );
        }

        private void DrawEnergyGuideLogo(XGraphics gfx, double x, double y, double w)
        {
            XImage logo = LoadImage("Logo_titulo.png");

            double logoW = Cm(13.2);
            double logoX = x + Cm(0.1);

            if (logo != null)
            {
                double logoH = logoW * ((double)logo.PixelHeight / logo.PixelWidth);
                gfx.DrawImage(logo, logoX, y, logoW, logoH);
            }
            else
            {
                gfx.DrawString(
                    "EnergyGuide",
                    new XFont("Arial Black", 38, XFontStyleEx.Bold),
                    XBrushes.Black,
                    new XRect(logoX, y, logoW, Cm(1.5)),
                    FmtMC
                );
            }
        }

        private void DrawSpecs(
            XGraphics gfx,
            EnergyLabelDataUSA d,
            double x,
            double y,
            double w,
            XFont font,
            XBrush black)
        {
            double lineH = Cm(0.39);

            string[] left =
            {
                Safe(d.REF_TYPE),
                "• " + Safe(d.DEFROST_SYSTEM),
                "• " + Safe(d.DOORTYPE),
                "• " + Safe(d.ICE_SERVICE)
            };

            string[] right =
            {
                Safe(d.CUST_NAME),
                "Model: " + Safe(d.MODEL),
                "Capacity: " + Safe(d.CAB_SIZE)
            };

            for (int i = 0; i < left.Length; i++)
            {
                gfx.DrawString(
                    left[i],
                    font,
                    black,
                    new XRect(x + Cm(0.1), y + i * lineH, w * 0.52, lineH),
                    FmtTL
                );
            }

            for (int i = 0; i < right.Length; i++)
            {
                gfx.DrawString(
                    right[i],
                    font,
                    black,
                    new XRect(x + w * 0.50, y + i * lineH, w * 0.48 - Cm(0.2), lineH),
                    FmtTR
                );
            }
        }

        private void DrawCompareBox(
            XGraphics gfx,
            double x,
            double y,
            double w,
            XFont fMain,
            XFont fSub,
            XBrush black,
            XBrush white)
        {
            double boxW = Cm(13.3);
            double boxH = Cm(1.8);
            double boxX = x;

            gfx.DrawRectangle(black, boxX, y, boxW, boxH);

            gfx.DrawString(
                "Compare ONLY to other labels with yellow numbers.",
                fMain,
                white,
                new XRect(boxX + Cm(0.6), y + Cm(0.28), boxW - Cm(1.3), Cm(0.55)),
                FmtTC
            );

            gfx.DrawString(
                "Labels with yellow numbers are based on the same test procedures.",
                fSub,
                white,
                new XRect(boxX + Cm(0.4), y + Cm(0.93), boxW - Cm(0.7), Cm(0.5)),
                FmtTC
            );
        }

        private void DrawCostBox(
            XGraphics gfx,
            EnergyLabelDataUSA d,
            double x,
            double y,
            double w,
            XFont fTitle,
            XFont fDollar,
            XFont fCost,
            XBrush black,
            XBrush white)
        {
            double boxW = Cm(13.3);
            double boxH = Cm(2.9);
            double boxX = x;

            gfx.DrawRectangle(black, boxX, y, boxW, boxH);

            gfx.DrawString(
                "Estimated Yearly Energy Cost",
                fTitle,
                white,
                new XRect(boxX + Cm(3.15), y + Cm(0.1), boxW - Cm(6.3), Cm(0.7)),
                FmtTC
            );

            double lowAmount = ToDoubleSafe(d.LOW_AMOUNT);
            double highAmount = ToDoubleSafe(d.HIGH_AMOUNT);
            double lowSimilar = ToDoubleSafe(d.LOW_SIMILAR_MODEL);
            double highSimilar = ToDoubleSafe(d.HIGH_SIMILAR_MODEL);
            double energyCost = ToDoubleSafe(d.ENERGY_COST);

            double globalMin = Math.Min(lowAmount, lowSimilar);
            double globalMax = Math.Max(highAmount, highSimilar);
            double range = globalMax - globalMin;

            double pct = range == 0 ? 0 : ((energyCost - globalMin) / range);
            pct = Clamp01(pct);

            double scaleLeft = boxX + Px(100);
            double scaleW = boxW - Px(110);
            double trackW = Cm(9.8);
            double trackLeft = scaleLeft;
            double cx = trackLeft + pct * trackW;

            string costText = FormatNumber(d.ENERGY_COST);

            double dollarW = gfx.MeasureString("$", fDollar).Width;
            double costW = gfx.MeasureString(costText, fCost).Width;
            double totalW = dollarW + Cm(0.25) + costW;

            double valueY = y + Cm(0.9);
            double valueX = cx - totalW / 2;

            if (valueX < boxX + Cm(0.1))
                valueX = boxX + Cm(0.1);

            if (valueX + totalW > boxX + boxW - Cm(0.1))
                valueX = boxX + boxW - Cm(0.1) - totalW;

            gfx.DrawString(
                "$",
                fDollar,
                white,
                new XRect(valueX, valueY + Cm(0.18), dollarW, Cm(1.2)),
                FmtML
            );

            gfx.DrawString(
                costText,
                fCost,
                white,
                new XRect(valueX + dollarW + Cm(0.25), valueY, costW + Cm(0.2), Cm(1.4)),
                FmtML
            );

            double triTop = y + Cm(2.15);
            double triH = Px(15);
            double triW = Px(20);

            gfx.DrawPolygon(
                white,
                new[]
                {
                    new XPoint(cx - triW / 2, triTop),
                    new XPoint(cx + triW / 2, triTop),
                    new XPoint(cx, triTop + triH)
                },
                XFillMode.Winding
            );
        }

        private void DrawRanges(
            XGraphics gfx,
            EnergyLabelDataUSA d,
            double x,
            double y,
            double w,
            XFont fSide,
            XFont fLabel,
            XFont fTrack,
            XBrush black,
            XBrush white)
        {
            double boxW = Cm(13.3);
            double boxH = Cm(2.0);
            double boxX = x;

            gfx.DrawRectangle(black, boxX, y, boxW, boxH);

            double innerX = boxX + Cm(0.2);
            double innerY = y + Cm(0.15);
            double innerW = Cm(12.5);
            double innerH = Cm(1.55);

            gfx.DrawRoundedRectangle(
                new XPen(XColors.White, 1),
                innerX,
                innerY,
                innerW,
                innerH,
                Cm(0.18),
                Cm(0.18)
            );

            gfx.Save();
            double sideCX = innerX + Cm(0.35);
            double sideCY = innerY + innerH / 2;
            gfx.RotateAtTransform(-90, new XPoint(sideCX, sideCY));
            gfx.DrawString(
                "Cost Ranges",
                fSide,
                white,
                new XRect(sideCX - Cm(1.5), sideCY - Cm(0.18), Cm(3), Cm(0.36)),
                FmtMC
            );
            gfx.Restore();

            double labelX = innerX + Cm(0.85);
            double labelW = Cm(2.45);
            double trackX = innerX + Cm(3.4);
            double trackW = Cm(9.8);
            double trackH = Cm(0.6);

            double row1Y = innerY + Cm(0.18);
            double row2Y = innerY + Cm(0.9);

            gfx.DrawString(
                "Models with",
                fLabel,
                white,
                new XRect(labelX, row1Y - Cm(0.02), labelW, Cm(0.3)),
                FmtTL
            );

            gfx.DrawString(
                "similar features",
                fLabel,
                white,
                new XRect(labelX, row1Y + Cm(0.28), labelW, Cm(0.3)),
                FmtTL
            );

            gfx.DrawString(
                "All models",
                fLabel,
                white,
                new XRect(labelX, row2Y + Cm(0.08), labelW, Cm(0.35)),
                FmtTL
            );

            double lowAmount = ToDoubleSafe(d.LOW_AMOUNT);
            double highAmount = ToDoubleSafe(d.HIGH_AMOUNT);
            double lowSimilar = ToDoubleSafe(d.LOW_SIMILAR_MODEL);
            double highSimilar = ToDoubleSafe(d.HIGH_SIMILAR_MODEL);

            double globalMin = Math.Min(lowAmount, lowSimilar);
            double globalMax = Math.Max(highAmount, highSimilar);

            DrawTrack(
                gfx,
                lowSimilar,
                highSimilar,
                trackX,
                row1Y,
                trackW,
                trackH,
                globalMin,
                globalMax,
                true,
                fTrack
            );

            DrawTrack(
                gfx,
                lowAmount,
                highAmount,
                trackX,
                row2Y,
                trackW,
                trackH,
                globalMin,
                globalMax,
                false,
                fTrack
            );
        }

        private void DrawTrack(
            XGraphics gfx,
            double low,
            double high,
            double x,
            double y,
            double w,
            double h,
            double globalMin,
            double globalMax,
            bool separator,
            XFont font)
        {
            XBrush black = XBrushes.Black;
            XBrush white = XBrushes.White;

            gfx.DrawRoundedRectangle(black, x, y, w, h, Cm(0.16), Cm(0.16));

            double range = globalMax - globalMin;

            double leftPct = range == 0 ? 0 : (low - globalMin) / range;
            double rightPct = range == 0 ? 1 : (high - globalMin) / range;

            leftPct = Clamp01(leftPct);
            rightPct = Clamp01(rightPct);

            double fillX = x + leftPct * w;
            double fillW = Math.Max(Cm(1.2), (rightPct - leftPct) * w);

            if (fillX + fillW > x + w)
                fillX = x + w - fillW;

            gfx.DrawRoundedRectangle(white, fillX, y, fillW, h, Cm(0.12), Cm(0.12));

            if (separator)
                gfx.DrawRectangle(black, fillX, y, Px(3), h);

            gfx.DrawString(
                "$" + FormatNumber(low),
                font,
                black,
                new XRect(fillX + Px(6), y, fillW / 2, h),
                FmtML
            );

            gfx.DrawString(
                "$" + FormatNumber(high),
                font,
                black,
                new XRect(fillX + fillW / 2, y, fillW / 2 - Px(6), h),
                FmtMR
            );
        }

        private void DrawKwhBox(
            XGraphics gfx,
            EnergyLabelDataUSA d,
            double x,
            double y,
            double w,
            XFont fKwh,
            XFont fUnit,
            XFont fSub,
            XBrush black,
            XBrush white)
        {
            double boxW = Cm(6.4);
            double boxH = Cm(2.22);
            double boxX = x + Cm(3.7);

            gfx.DrawRectangle(black, boxX, y, boxW, boxH);

            string kwh = FormatNumber(d.ELECTRICITY_USE);

            double kwhW = gfx.MeasureString(kwh, fKwh).Width;
            double unitW = gfx.MeasureString("kWh", fUnit).Width;
            double totalW = kwhW + Cm(0.5) + unitW;

            double groupX = boxX + (boxW - totalW) / 2;
            double valueY = y + Cm(0.25);

            gfx.DrawString(
                kwh,
                fKwh,
                white,
                new XRect(groupX, valueY, kwhW + Cm(0.1), Cm(1.0)),
                FmtML
            );

            gfx.DrawString(
                "kWh",
                fUnit,
                white,
                new XRect(groupX + kwhW + Cm(0.5), valueY + Cm(0.11), unitW + Cm(0.2), Cm(0.8)),
                FmtML
            );

            gfx.DrawString(
                "Estimated Yearly Electricity Use",
                fSub,
                white,
                new XRect(boxX, y + Cm(1.48), boxW, Cm(0.45)),
                FmtTC
            );
        }

        private void DrawFooter(
            XGraphics gfx,
            EnergyLabelDataUSA d,
            double x,
            double y,
            double w,
            double h,
            XFont fFooter,
            XFont fFtc,
            XFont fPart,
            XBrush black)
        {
            double footerY = y + h - Cm(3.25);

            double notesX = x + Cm(0.1);
            double notesW = Cm(9.2);

            var tf = new XTextFormatter(gfx);

            string[] notes =
            {
                "Your cost will depend on your utility rates and use.",
                "Both cost ranges based on models of similar size capacity.",
                "Models with similar features have automatic defrost, side-mounted freezer, and through-the-door ice.",
                "Estimated energy cost is based on a national average electricity cost of 14 cents per kWh."
            };

            double lineY = footerY;

            foreach (string note in notes)
            {
                gfx.DrawString(
                    "•",
                    new XFont("Arial Narrow", 9.3, XFontStyleEx.Bold),
                    black,
                    new XRect(notesX, lineY, Cm(0.2), Cm(0.35)),
                    FmtTL
                );

                tf.DrawString(
                    note,
                    fFooter,
                    black,
                    new XRect(notesX + Cm(0.25), lineY, notesW - Cm(0.25), Cm(0.6)),
                    XStringFormats.TopLeft
                );

                lineY += Cm(0.43);
            }

            gfx.DrawString(
                "ftc.gov/energy",
                fFtc,
                black,
                new XRect(x + Cm(3.2), y + h - Cm(0.65), w - Cm(3.2), Cm(0.5)),
                FmtTC
            );

            double logoX = x + w - Cm(3.15);
            double logoY = footerY - Cm(0.1);
            double logoW = Cm(2.3);
            double logoH = Cm(2.4);

            string energyLogo = Safe(d.ENERGY_LOGO).ToUpper();

            if (energyLogo == "Y")
            {
                XImage logoPie = LoadImage("Logo_pie.png");

                if (logoPie != null)
                    gfx.DrawImage(logoPie, logoX, logoY, logoW, logoH);
            }

            gfx.DrawString(
                "PART NO. " + Safe(d.PART_NUMBER),
                fPart,
                black,
                new XRect(logoX - Cm(0.2), logoY + logoH + Cm(0.1), Cm(4), Cm(0.35)),
                FmtTR
            );
        }

        private static double Clamp01(double value)
        {
            return Math.Max(0, Math.Min(1, value));
        }

        private static string Safe(object value)
        {
            return value == null ? string.Empty : Convert.ToString(value);
        }

        private static string FormatNumber(object value)
        {
            double n = ToDoubleSafe(value);

            if (Math.Abs(n - Math.Round(n)) < 0.00001)
                return ((int)Math.Round(n)).ToString();

            return n.ToString("0.##");
        }
    }
}