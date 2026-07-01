using Energy_printer.Models;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;

namespace Energy_printer.Services
{
    public class EnergyLabelServiceUSA : EnergyLabelHelpersBase
    {
        public EnergyLabelServiceUSA(string contentPath) : base(contentPath)
        {
        }

        private class PdfMargins
        {
            public double Left { get; set; }
            public double Right { get; set; }
            public double Top { get; set; }
            public double Bottom { get; set; }
        }

        // ─────────────────────────────────────────────────────────────
        // DATA MAPPING
        // ─────────────────────────────────────────────────────────────
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

        // ─────────────────────────────────────────────────────────────
        // MEASURES
        // ─────────────────────────────────────────────────────────────
        private static readonly double BoxW = Cm(13.3);
        private static readonly double LogoW = Cm(13.2);
        private static readonly double CompareH = Cm(1.78);
        private static readonly double CostH = Cm(3);
        private static readonly double RangesH = Cm(1.78);
        private static readonly double KwhBoxW = Cm(6.4);
        private static readonly double KwhBoxH = Cm(2.22);
        private static readonly double TrackH = Cm(0.58);
        private static readonly double RangeSidebarW = Cm(1.15);
        private static readonly double RangeLabelW = Cm(2.45);

        // ─────────────────────────────────────────────────────────────
        // FONTS
        // ─────────────────────────────────────────────────────────────
        private XFont FontArial(double size, XFontStyleEx style = XFontStyleEx.Regular)
        {
            return new XFont("Arial", size, style);
        }

        private XFont FontArialNarrow(double size, XFontStyleEx style = XFontStyleEx.Regular)
        {
            return new XFont("Arial Narrow", size, style);
        }

        private XFont FontArialBlack(double size)
        {
            return new XFont("Arial Black", size, XFontStyleEx.Bold);
        }

        // ─────────────────────────────────────────────────────────────
        // PDF PAGE GENERATION
        // ─────────────────────────────────────────────────────────────
        public void AddUSAPage(PdfDocument doc, EnergyLabelDataUSA d, CONFIG_DATA_LABEL config, int copyNumber)
        {
            var page = doc.AddPage();

            page.Width = XUnit.FromCentimeter(14);
            page.Height = XUnit.FromCentimeter(21.5);
            page.Orientation = PdfSharp.PageOrientation.Portrait;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                var margins = GetUSAMargins(config, copyNumber);
                DrawUSALabel(gfx, d, 0, page.Width.Point, page.Height.Point, margins);
            }
        }

        public void AddUSASheet(PdfDocument doc, EnergyLabelDataUSA d, CONFIG_DATA_LABEL config)
        {
            var page = doc.AddPage();

            page.Width = XUnit.FromCentimeter(28.0);
            page.Height = XUnit.FromCentimeter(21.5);
            page.Orientation = PdfSharp.PageOrientation.Landscape;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                double labelW = Cm(14.0);
                double labelH = page.Height.Point;

                var margins1 = GetUSAMargins(config, 1);
                var margins2 = GetUSAMargins(config, 2);

                DrawUSALabel(gfx, d, 0, labelW, labelH, margins1);
                DrawUSALabel(gfx, d, Cm(14.0), labelW, labelH, margins2);
            }
        }

        // ─────────────────────────────────────────────────────────────
        // MARGINS FROM DATABASE
        // ─────────────────────────────────────────────────────────────
        private PdfMargins GetUSAMargins(CONFIG_DATA_LABEL config, int copyNumber)
        {
            string unit = "cm";

            if (config != null && !string.IsNullOrWhiteSpace(config.UNIT))
            {
                unit = config.UNIT.Trim().ToLower();
            }

            double left;
            double right;
            double top;
            double bottom;

            if (copyNumber == 1)
            {
                left = ToDouble(config?.MARG_LEFT_1, 0.400);
                right = ToDouble(config?.MARG_RIGHT_1, 0.200);
                top = ToDouble(config?.MARG_TOP_1, 3.900);
                bottom = ToDouble(config?.MARG_BOTTOM_1, 0.400);
            }
            else
            {
                left = ToDouble(config?.MARG_LEFT_2, 0.500);
                right = ToDouble(config?.MARG_RIGHT_2, 0.400);
                top = ToDouble(config?.MARG_TOP_2, 3.900);
                bottom = ToDouble(config?.MARG_BOTTOM_2, 0.400);
            }

            return new PdfMargins
            {
                Left = ConvertUnitToPoints(left, unit),
                Right = ConvertUnitToPoints(right, unit),
                Top = ConvertUnitToPoints(top, unit),
                Bottom = ConvertUnitToPoints(bottom, unit)
            };
        }

        // ─────────────────────────────────────────────────────────────
        // DRAW USA LABEL
        // ─────────────────────────────────────────────────────────────
        private void DrawUSALabel(XGraphics gfx, EnergyLabelDataUSA d, double originX, double W, double H, PdfMargins margins)
        {
            double padLeft = originX + margins.Left;
            double padTop = margins.Top;
            double padBottom = margins.Bottom;
            double CW = W - margins.Left - margins.Right;

            double boxX = padLeft;
            double boxW = BoxW;

            var white = XBrushes.White;
            var black = XBrushes.Black;

            double lowAmount = ToDouble(d.LOW_AMOUNT, 0);
            double highAmount = ToDouble(d.HIGH_AMOUNT, 0);
            double lowSimilar = ToDouble(d.LOW_SIMILAR_MODEL, 0);
            double highSimilar = ToDouble(d.HIGH_SIMILAR_MODEL, 0);
            double energyCost = ToDouble(d.ENERGY_COST, 0);

            double globalMin = Math.Min(lowAmount, lowSimilar);
            double globalMax = Math.Max(highAmount, highSimilar);
            double globalRange = globalMax - globalMin;

            Func<double, double> pct01 = value =>
            {
                if (globalRange == 0)
                    return 0;

                return Clamp01((value - globalMin) / globalRange);
            };

            // ─────────────────────────────────────────────────────────
            // 1. HEADER
            // ─────────────────────────────────────────────────────────
            double govY = padTop;
            double headerX = padLeft;
            double headerW = CW;

            gfx.DrawString(
                "U.S. Government",
                FontArialNarrow(10, XFontStyleEx.Regular),
                black,
                new XRect(headerX + Cm(0.3), govY, headerW * 0.35, Cm(0.38)),
                FmtTL
            );

            gfx.DrawString(
                "Federal law prohibits removal of this label before consumer purchase.",
                FontArialNarrow(9.5, XFontStyleEx.Regular),
                black,
                new XRect(headerX, govY + Cm(0.05), headerW - Cm(0.35), Cm(0.38)),
                FmtTR
            );

            double y = govY + Cm(0.5);

            // ─────────────────────────────────────────────────────────
            // 2. LOGO
            // ─────────────────────────────────────────────────────────
            double logoTop = y;
            double logoBottom = y;

            var logoTit = LoadImageHighContrastBlack("Logo_titulo.png", 210);

            if (logoTit != null)
            {
                double logoX = boxX + Cm(0.1);
                double logoW = LogoW;
                double logoH = logoW * ((double)logoTit.PixelHeight / logoTit.PixelWidth);

                gfx.DrawImage(logoTit, logoX, logoTop, logoW, logoH);
                logoBottom = logoTop + logoH;
            }
            else
            {
                double logoH = Cm(1.6);

                gfx.DrawString(
                    "EnergyGuide",
                    FontArialBlack(28),
                    black,
                    new XRect(boxX + Cm(0.1), logoTop, LogoW, logoH),
                    FmtMC
                );

                logoBottom = logoTop + logoH;
            }

            y = logoBottom - Px(43);

            // ─────────────────────────────────────────────────────────
            // 3. SPECIFICATIONS
            // ─────────────────────────────────────────────────────────
            double specTableX = boxX;
            double specTableW = Cm(13.3);
            double specPaddingLeft = Cm(0.1);
            double specPaddingRight = Cm(0.1);
            double specInsetFix = Cm(0.05);

            double specContentX = specTableX + specPaddingLeft + specInsetFix;
            double specContentW = specTableW - specPaddingLeft - specPaddingRight - (specInsetFix * 2);

            double specLeftX = specContentX;
            double specLeftW = specContentW * 0.50;
            double specBulletX = specLeftX + Cm(0.3);

            double specRightX = specContentX + specContentW * 0.50;
            double specRightW = specContentW * 0.50;
            double specRightPadding = Cm(0.1);

            double specFontSize = 9.5;
            double specLineH = specFontSize * 1.05;

            var fSpec = FontArialNarrow(specFontSize, XFontStyleEx.Bold);

            gfx.DrawString(
                Safe(d.REF_TYPE),
                fSpec,
                black,
                new XRect(specLeftX, y, specLeftW, specLineH),
                FmtTL
            );

            gfx.DrawString(
                "• " + Safe(d.DEFROST_SYSTEM),
                fSpec,
                black,
                new XRect(specBulletX, y + specLineH, specLeftW - Cm(0.3), specLineH),
                FmtTL
            );

            gfx.DrawString(
                "• " + Safe(d.DOORTYPE),
                fSpec,
                black,
                new XRect(specBulletX, y + specLineH * 2, specLeftW - Cm(0.3), specLineH),
                FmtTL
            );

            gfx.DrawString(
                "• " + Safe(d.ICE_SERVICE),
                fSpec,
                black,
                new XRect(specBulletX, y + specLineH * 3, specLeftW - Cm(0.3), specLineH),
                FmtTL
            );

            gfx.DrawString(
                Safe(d.CUST_NAME),
                fSpec,
                black,
                new XRect(specRightX, y, specRightW, specLineH),
                FmtTR
            );

            gfx.DrawString(
                Safe(d.MODEL),
                fSpec,
                black,
                new XRect(specRightX, y + specLineH + Cm(0.05), specRightW - specRightPadding, specLineH),
                FmtTR
            );

            gfx.DrawString(
                "Capacity: " + Safe(d.CAB_SIZE),
                fSpec,
                black,
                new XRect(specRightX, y + specLineH * 2 + Cm(0.1), specRightW - specRightPadding, specLineH),
                FmtTR
            );

            y += specLineH * 4 + Cm(0.19);

            // ─────────────────────────────────────────────────────────
            // 4. COMPARE BOX
            // ─────────────────────────────────────────────────────────
            gfx.DrawRectangle(black, boxX, y, boxW, CompareH);

            var fCompareMain = FontArialNarrow(16.2, XFontStyleEx.Bold);
            var fCompareSub = FontArialNarrow(13.2, XFontStyleEx.Bold);

            double compareMainX = boxX + Cm(0.6);
            double compareMainW = boxW - Cm(1.3);
            double compareSubX = boxX + Cm(0.4);
            double compareSubW = boxW - Cm(0.7);

            double compareMainY = y + Cm(0.27);
            double compareSubY = compareMainY + Px(15.6 * 1.20) + Cm(0.22);

            gfx.DrawString(
                "Compare ONLY to other labels with yellow numbers.",
                fCompareMain,
                white,
                new XRect(compareMainX, compareMainY, compareMainW, Px(15.6 * 1.20)),
                FmtTC
            );

            gfx.DrawString(
                "Labels with yellow numbers are based on the same test procedures.",
                fCompareSub,
                white,
                new XRect(compareSubX, compareSubY, compareSubW, Px(13.1 * 1.20)),
                FmtTC
            );

            y += CompareH + Cm(0.3);

            // ─────────────────────────────────────────────────────────
            // 5. COST BOX + COST RANGES
            // ─────────────────────────────────────────────────────────
            double costY = y;
            double rangesY = costY + CostH;

            double rangesInnerLeft = boxX + Cm(0.2);
            double rangesInnerTop = rangesY - Cm(.1);
            double rangesInnerH = RangesH - Cm(0.10);

            double rangesBoxX = rangesInnerLeft + RangeSidebarW - Cm(0.7);

            double labelX = rangesBoxX;
            double labelW = RangeLabelW;

            double trackX = rangesBoxX + labelW + Cm(0.05);
            double trackH = TrackH;

            double rangesRightLimit = boxX + boxW - Cm(0.20);

            // Borde exterior crece hacia la izquierda, pero el contenido se queda igual
            double rangesBorderX = rangesBoxX - Cm(0.02);
            double rangesBorderW = rangesRightLimit - rangesBorderX;

            // Contenido interno mantiene su misma posición
            double rangesBoxW = rangesRightLimit - rangesBoxX;
            double trackW = rangesRightLimit - trackX - Cm(0.05);

            if (trackW < Cm(1.0))
            {
                trackW = Cm(1.0);
            }

            double desiredGapBetweenPills = Cm(0.25);

            double remainingVerticalSpace = rangesInnerH - (trackH * 2) - desiredGapBetweenPills;

            if (remainingVerticalSpace < 0)
            {
                remainingVerticalSpace = 0;
            }

            double topPaddingInsideRanges = remainingVerticalSpace / 2.0;

            double row1Y = rangesInnerTop + topPaddingInsideRanges;
            double row2Y = row1Y + trackH + desiredGapBetweenPills;

            gfx.DrawRectangle(black, boxX, costY, boxW, CostH);

            gfx.DrawString(
                "Estimated Yearly Energy Cost",
                FontArialNarrow(16, XFontStyleEx.Bold),
                white,
                new XRect(boxX + Cm(2), costY + Cm(0.1), boxW - Cm(4), Cm(0.55)),
                FmtTC
            );

            double rawCostPct = globalRange == 0 ? 0 : (energyCost - globalMin) / globalRange;

            double pointerLeftX = trackX;
            double pointerRightX = trackX + trackW - Cm(0.18);
            double pointerW = pointerRightX - pointerLeftX;

            double cx;

            if (energyCost < globalMin)
            {
                cx = trackX;
            }
            else if (energyCost > globalMax)
            {
                cx = trackX + trackW;
            }
            else
            {
                cx = pointerLeftX + Clamp01(rawCostPct) * pointerW;
            }

            var fDollar = FontArialBlack(38);
            var fNum = FontArialBlack(50);

            string costText = Safe(d.ENERGY_COST);

            double dollarW = gfx.MeasureString("$", fDollar).Width;
            double numberW = gfx.MeasureString(costText, fNum).Width;
            double dollarGap = Cm(0.25) + Px(2);

            double groupW = dollarW + dollarGap + numberW;

            double costValueX = cx - groupW / 2.0;
            double costValueY = costY + Cm(0.81);//82

            double minValueX = boxX + Cm(0.1);
            double maxValueX = boxX + boxW - Cm(0.1) - groupW;

            if (costValueX < minValueX)
                costValueX = minValueX;

            if (costValueX > maxValueX)
                costValueX = maxValueX;

            gfx.DrawString(
                "$",
                fDollar,
                white,
                new XRect(costValueX, costValueY, dollarW, Cm(1.4)),
                FmtML
            );

            gfx.DrawString(
                costText,
                fNum,
                white,
                new XRect(costValueX + dollarW + dollarGap, costValueY, numberW + Cm(0.15), Cm(1.4)),
                FmtML
            );

            double triTopY = costY + Cm(2.4);
            double triApexY = triTopY + Px(15);

            gfx.DrawPolygon(
                white,
                new[]
                {
                    new XPoint(cx - Px(10), triTopY),
                    new XPoint(cx + Px(10), triTopY),
                    new XPoint(cx, triApexY)
                },
                XFillMode.Winding
            );

            y = rangesY;

            gfx.DrawRectangle(black, boxX, y, boxW, RangesH);

            gfx.Save();

            double sidebarX = rangesInnerLeft;
            double sidebarW = RangeSidebarW;
            double sidebarCx = sidebarX + sidebarW / 2.0 - Cm(0.32);
            double sidebarCy = rangesInnerTop + rangesInnerH / 2.0;

            gfx.RotateAtTransform(-90, new XPoint(sidebarCx, sidebarCy));

            gfx.DrawString(
                "Cost Ranges",
                FontArialNarrow(9, XFontStyleEx.Bold),
                white,
                new XRect(sidebarCx - Cm(1.2), sidebarCy - Cm(0.2), Cm(2.4), Cm(0.4)),
                FmtMC
            );

            gfx.Restore();

            gfx.DrawRoundedRectangle(
                new XPen(XColors.White, .6),
                rangesBorderX,
                rangesInnerTop,
                rangesBorderW,
                rangesInnerH,
                Px(18),
                Px(18)
            );

            gfx.DrawString(
                "Models with",
                FontArialNarrow(9.5, XFontStyleEx.Bold),
                white,
                new XRect(labelX + Cm(0.3), row1Y - Cm(0.02), labelW, Cm(0.3)),
                FmtTL
            );

            gfx.DrawString(
                "similar features",
                FontArialNarrow(9.5, XFontStyleEx.Bold),
                white,
                new XRect(labelX + Cm(0.3), row1Y + Cm(0.38), labelW, Cm(0.3)),
                FmtTL
            );

            gfx.DrawString(
                "All models",
                FontArialNarrow(9.5, XFontStyleEx.Bold),
                white,
                new XRect(labelX + Cm(0.3), row2Y + Cm(0.08), labelW, Cm(0.3)),
                FmtTL
            );

            DrawRangeBarByJsLogic(
                gfx,
                lowSimilar,
                highSimilar,
                trackX,
                row1Y,
                trackW,
                trackH,
                globalMin,
                globalMax
            );

            DrawRangeBarByJsLogic(
                gfx,
                lowAmount,
                highAmount,
                trackX,
                row2Y,
                trackW,
                trackH,
                globalMin,
                globalMax
            );
            // Redibuja el borde exterior encima de las píldoras
            gfx.DrawRoundedRectangle(
                new XPen(XColors.White, 1.1),
                rangesBorderX,
                rangesInnerTop,
                rangesBorderW,
                rangesInnerH,
                Px(18),
                Px(18)
            );
            y += RangesH + Cm(0.39);

            // ─────────────────────────────────────────────────────────
            // 6. KWH BOX
            // ─────────────────────────────────────────────────────────
            double kwhBoxX = boxX + Cm(3.7);

            gfx.DrawRectangle(black, kwhBoxX, y, KwhBoxW, KwhBoxH);

            var fKwh = FontArialBlack(36);
            var fUnit = FontArialNarrow(13, XFontStyleEx.Bold);

            string kwhStr = Safe(d.ELECTRICITY_USE);

            double wKwh = gfx.MeasureString(kwhStr, fKwh).Width;
            double wUnit = gfx.MeasureString("kWh", fUnit).Width;
            double kg = Cm(0.5);

            double grpLeft = kwhBoxX + (KwhBoxW - (wKwh + kg + wUnit)) / 2.0;
            double valTop = y + Cm(0.35);

            gfx.DrawString(
                kwhStr,
                fKwh,
                white,
                new XRect(grpLeft, valTop + Cm(0.18), wKwh + Px(4), Px(44)),
                FmtBL
            );

            gfx.DrawString(
                "kWh",
                fUnit,
                white,
                new XRect(grpLeft + wKwh + kg, valTop - Cm(0.08), wUnit + Px(4), Px(44)),
                FmtBL
            );

            gfx.DrawString(
                "Estimated Yearly Electricity Use",
                FontArialNarrow(12, XFontStyleEx.Bold),
                white,
                new XRect(kwhBoxX, y + Cm(1.55), KwhBoxW, Cm(0.4)),
                FmtTC
            );

            y += KwhBoxH + Px(15);

            // ─────────────────────────────────────────────────────────
            // 7. FOOTER
            // ─────────────────────────────────────────────────────────
            double ftcH = Px(20);
            double ftcY = H - padBottom - ftcH - Cm(0.5); ;

            gfx.DrawString(
                "ftc.gov/energy",
                FontArialNarrow(15, XFontStyleEx.Regular),
                black,
                new XRect(boxX + Cm(5), ftcY, boxW, ftcH),
                FmtTL
            );

            double notesW = Cm(9.4);
            double notesX = boxX + Cm(0.40);
            double bulletX = notesX - Px(7);

            double starW = Cm(2.3);
            double starH = Cm(2.4);
            double starX = boxX + boxW - Cm(2.8);

            double blockTop = H - padBottom - Cm(3.05);

            var fFooterBold = FontArialNarrow(10.2, XFontStyleEx.Bold);
            var fFooter = FontArialNarrow(9.3, XFontStyleEx.Regular);
            var fBullet = FontArialNarrow(9.3, XFontStyleEx.Bold);

            double footerLineH = Px(14);
            double footerGap = Px(0);
            double ny = blockTop + Cm(0.08);

            DrawFooterBulletLines(
                gfx,
                new[] { "Your cost will depend on your utility rates and use." },
                fFooterBold,
                fBullet,
                black,
                bulletX,
                notesX,
                notesW,
                ref ny,
                footerLineH,
                footerGap
            );

            DrawFooterBulletLines(
                gfx,
                new[] { "Both cost ranges based on models of similar size capacity." },
                fFooter,
                fBullet,
                black,
                bulletX,
                notesX,
                notesW,
                ref ny,
                footerLineH,
                footerGap
            );

            DrawFooterBulletLines(
                gfx,
                new[]
                {
        "Models with similar features have automatic defrost, side-mounted freezer,",
        "and through-the-door ice."
                },
                fFooter,
                fBullet,
                black,
                bulletX,
                notesX,
                notesW,
                ref ny,
                footerLineH,
                footerGap
            );

            DrawFooterBulletLines(
                gfx,
                new[]
                {
        "Estimated energy cost is based on a national average electricity cost",
        "of 14 cents per kWh."
                },
                fFooter,
                fBullet,
                black,
                bulletX,
                notesX,
                notesW,
                ref ny,
                footerLineH,
                footerGap
            );

            var logoPie = LoadImageHighContrastBlack("Logo_pie.png", 210);

            if (d.ENERGY_LOGO != null && d.ENERGY_LOGO.ToUpper() == "Y")
            {
                if (logoPie != null)
                {
                    gfx.DrawImage(logoPie, starX, blockTop, starW, starH);
                }
            }

            gfx.DrawString(
                "PART NO. " + Safe(d.PART_NUMBER),
                FontArialNarrow(8.5, XFontStyleEx.Bold),
                black,
                new XRect(starX - Cm(0.30), blockTop + starH + Px(4), starW + Px(50), Px(12)),
                FmtTL
            );
        }
// ─────────────────────────────────────────────────────────────
// FOOTER BULLET DRAWING
// ─────────────────────────────────────────────────────────────
private void DrawFooterBulletLines(
    XGraphics gfx,
    string[] lines,
    XFont textFont,
    XFont bulletFont,
    XBrush brush,
    double bulletX,
    double textX,
    double textW,
    ref double y,
    double lineH,
    double gapAfter)
        {
            if (lines == null || lines.Length == 0)
                return;

            gfx.DrawString(
                "•",
                bulletFont,
                brush,
                new XRect(bulletX, y, Px(8), lineH),
                FmtTL
            );

            for (int i = 0; i < lines.Length; i++)
            {
                gfx.DrawString(
                    lines[i],
                    textFont,
                    brush,
                    new XRect(textX, y + (i * lineH), textW, lineH),
                    FmtTL
                );
            }

            y += (lines.Length * lineH) + gapAfter;
        }
        // ─────────────────────────────────────────────────────────────
        // RANGE BAR DRAWING
        // ─────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────
        // RANGE BAR / PILLS
        // ─────────────────────────────────────────────────────────────
        private void DrawRangeBarByJsLogic(
            XGraphics gfx,
            double low,
            double high,
            double trackX,
            double trackY,
            double trackW,
            double trackH,
            double globalMin,
            double globalMax)
        {
            double range = globalMax - globalMin;

            double pLow = range == 0 ? 0 : (low - globalMin) / range;
            double pHigh = range == 0 ? 1 : (high - globalMin) / range;

            pLow = Clamp01(pLow);
            pHigh = Clamp01(pHigh);

            double fillX = trackX + pLow * trackW;
            double fillW = Math.Max(0, (pHigh - pLow) * trackW);
            double pillRightInset = Cm(0.18);

            double maxFillRight = trackX + trackW - pillRightInset;

            if (fillX + fillW > maxFillRight)
            {
                fillW = maxFillRight - fillX;
            }

            if (fillW < 0)
            {
                fillW = 0;
            }
            double pillRadius = trackH / 2.0;

            gfx.DrawRoundedRectangle(
                XBrushes.Black,
                trackX,
                trackY,
                trackW,
                trackH,
                pillRadius,
                pillRadius
            );

            if (fillW <= 0)
                return;

            double minimumPillW = Cm(1.35);

            if (fillW < minimumPillW)
            {
                fillW = minimumPillW;
            }

            if (fillX + fillW > maxFillRight)
            {
                fillX = maxFillRight - fillW;
            }

            if (fillX < trackX)
            {
                fillX = trackX;
            }

            gfx.DrawRoundedRectangle(
                XBrushes.White,
                fillX,
                trackY,
                fillW,
                trackH,
                pillRadius,
                pillRadius
            );

            var f = FontArialNarrow(14.5, XFontStyleEx.Bold);

            double textPadLeft = Px(8);
            double textPadRight = Px(8);

            gfx.DrawString(
                "$" + low.ToString("0"),
                f,
                XBrushes.Black,
                new XRect(
                    fillX + textPadLeft,
                    trackY,
                    fillW / 2.0 - textPadLeft,
                    trackH
                ),
                FmtML
            );

            gfx.DrawString(
                "$" + high.ToString("0"),
                f,
                XBrushes.Black,
                new XRect(
                    fillX + fillW / 2.0,
                    trackY,
                    fillW / 2.0 - textPadRight,
                    trackH
                ),
                FmtMR
            );
        }

        // ─────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────
        private double Clamp01(double value)
        {
            return Math.Max(0, Math.Min(1, value));
        }

        private double ToDouble(object value, double fallback)
        {
            if (value == null)
                return fallback;

            double result;

            if (double.TryParse(value.ToString(), out result))
                return result;

            return fallback;
        }

        private double ConvertUnitToPoints(double value, string unit)
        {
            switch ((unit ?? "cm").ToLower())
            {
                case "cm":
                    return Cm(value);

                case "mm":
                    return value * 2.83465;

                case "in":
                case "inch":
                case "inches":
                    return In(value);

                case "pt":
                case "pts":
                case "point":
                case "points":
                    return value;

                case "px":
                    return Px(value);

                default:
                    return Cm(value);
            }
        }

        private string Safe(object value)
        {
            return value == null ? string.Empty : value.ToString();
        }
    }
}