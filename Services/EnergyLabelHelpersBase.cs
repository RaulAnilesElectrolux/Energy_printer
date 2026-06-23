using PdfSharp.Drawing;
using PdfSharp.Fonts;
using System;
using System.IO;

namespace Energy_printer.Services
{
    // ══════════════════════════════════════════════════════════════════════
    //  CLASE BASE: Helpers compartidos entre el dibujo de la etiqueta
    //  USA (amarilla) y Canadá (blanca). No contiene lógica de PDF propia,
    //  únicamente utilidades comunes para evitar duplicar código.
    // ══════════════════════════════════════════════════════════════════════
    public abstract class EnergyLabelHelpersBase
    {
        protected readonly string _contentPath;

        protected EnergyLabelHelpersBase(string contentPath)
        {
            _contentPath = contentPath;
        }

        // ══════════════════════════════════════════════════════════════════════
        //  HELPERS GLOBALES
        // ══════════════════════════════════════════════════════════════════════

        protected static double Cm(double cm) { return cm * 28.3465; }
        protected static double In(double inch) { return inch * 72.0; }
        protected static double Px(double px) { return px * 0.75; }   // 96px = 72pt

        protected static XStringFormat Fmt(XStringAlignment h, XLineAlignment v)
        {
            return new XStringFormat { Alignment = h, LineAlignment = v };
        }

        protected static readonly XStringFormat FmtTL = XStringFormats.TopLeft;
        protected static readonly XStringFormat FmtTR = XStringFormats.TopRight;
        protected static readonly XStringFormat FmtC = XStringFormats.Center;
        protected static readonly XStringFormat FmtML = Fmt(XStringAlignment.Near, XLineAlignment.Center);
        protected static readonly XStringFormat FmtMR = Fmt(XStringAlignment.Far, XLineAlignment.Center);
        protected static readonly XStringFormat FmtMC = Fmt(XStringAlignment.Center, XLineAlignment.Center);
        protected static readonly XStringFormat FmtBL = Fmt(XStringAlignment.Near, XLineAlignment.Far);
        protected static readonly XStringFormat FmtBR = Fmt(XStringAlignment.Far, XLineAlignment.Far);
        protected static readonly XStringFormat FmtBC = Fmt(XStringAlignment.Center, XLineAlignment.Far);
        protected static readonly XStringFormat FmtTC = Fmt(XStringAlignment.Center, XLineAlignment.Near);

        protected XImage LoadImage(string filename)
        {
            string path = Path.Combine(_contentPath, filename);
            return File.Exists(path) ? XImage.FromFile(path) : null;
        }
    }

public class CustomFontResolver : IFontResolver
    {
        public byte[] GetFont(string faceName)
        {
            var fontPath = Path.Combine(
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
                if (isBold && isItalic)
                    return new FontResolverInfo("arialbi");
                if (isBold)
                    return new FontResolverInfo("arialbd");
                if (isItalic)
                    return new FontResolverInfo("ariali");

                return new FontResolverInfo("arial");
            }

            if (familyName.Equals("Arial Narrow", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold && isItalic)
                    return new FontResolverInfo("ARIALNBI");
                if (isBold)
                    return new FontResolverInfo("ARIALNB");
                if (isItalic)
                    return new FontResolverInfo("ARIALN");

                return new FontResolverInfo("ARIALN");
            }

            if (familyName.Equals("ElmsSans", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold && isItalic)
                    return new FontResolverInfo("ElmsSans-Regular");
                if (isBold)
                    return new FontResolverInfo("ElmsSans-SemiBold");
                if (isItalic)
                    return new FontResolverInfo("ElmsSans-SemiBold");

                return new FontResolverInfo("ElmsSans-Regular");
            }

            // fallback
            return new FontResolverInfo("arial");
        }
    }
}
