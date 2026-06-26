using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System;
using System.Globalization;
using System.IO;

namespace Energy_printer.Services
{
    public abstract class EnergyLabelHelpersBase
    {
        protected readonly string _contentPath;

        protected EnergyLabelHelpersBase(string contentPath)
        {
            _contentPath = contentPath;
        }

        protected static double Cm(double cm)
        {
            return cm * 28.3464566929;
        }

        protected static double Px(double px)
        {
            return px * 0.75;
        }

        protected static double ToDoubleSafe(object value, double fallback = 0)
        {
            if (value == null) return fallback;

            string s = Convert.ToString(value, CultureInfo.InvariantCulture);

            if (string.IsNullOrWhiteSpace(s)) return fallback;

            s = s.Replace("$", "")
                 .Replace(",", "")
                 .Replace("cm", "")
                 .Replace("px", "")
                 .Trim();

            double result;

            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;

            if (double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out result))
                return result;

            return fallback;
        }

        protected static XStringFormat Fmt(XStringAlignment h, XLineAlignment v)
        {
            return new XStringFormat
            {
                Alignment = h,
                LineAlignment = v
            };
        }

        protected static readonly XStringFormat FmtTL = XStringFormats.TopLeft;
        protected static readonly XStringFormat FmtTR = XStringFormats.TopRight;
        protected static readonly XStringFormat FmtTC = Fmt(XStringAlignment.Center, XLineAlignment.Near);
        protected static readonly XStringFormat FmtML = Fmt(XStringAlignment.Near, XLineAlignment.Center);
        protected static readonly XStringFormat FmtMR = Fmt(XStringAlignment.Far, XLineAlignment.Center);
        protected static readonly XStringFormat FmtMC = Fmt(XStringAlignment.Center, XLineAlignment.Center);
        protected static readonly XStringFormat FmtBL = Fmt(XStringAlignment.Near, XLineAlignment.Far);
        protected static readonly XStringFormat FmtBC = Fmt(XStringAlignment.Center, XLineAlignment.Far);

        protected XImage LoadImage(string filename)
        {
            string path = Path.Combine(_contentPath, filename);

            if (!File.Exists(path))
                return null;

            return XImage.FromFile(path);
        }
    }

    public class CustomFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            string fontPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "fonts",
                faceName + ".ttf"
            );

            return File.ReadAllBytes(fontPath);
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Arial", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold && isItalic) return new FontResolverInfo("arialbi");
                if (isBold) return new FontResolverInfo("arialbd");
                if (isItalic) return new FontResolverInfo("ariali");

                return new FontResolverInfo("arial");
            }

            if (familyName.Equals("Arial Black", StringComparison.OrdinalIgnoreCase))
            {
                return new FontResolverInfo("ariblk");
            }

            if (familyName.Equals("Arial Narrow", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold && isItalic) return new FontResolverInfo("ARIALNBI");
                if (isBold) return new FontResolverInfo("ARIALNB");
                if (isItalic) return new FontResolverInfo("ARIALNI");

                return new FontResolverInfo("ARIALN");
            }

            return new FontResolverInfo("arial");
        }
    }
}