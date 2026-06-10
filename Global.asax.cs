using PdfSharp.Fonts;
using System.Web.Mvc;
using System.Web.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace Energy_printer
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // ── Registros estándar de MVC ──────────────────────────────────────
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // ── Fuentes para PdfSharp ──────────────────────────────────────────
            // Se registra una sola vez en toda la vida de la aplicación.
            // Server.MapPath resuelve la ruta física desde la raíz del sitio.
            // Coloca los .ttf en ~/App_Data/Fonts/ o en la carpeta que prefieras.
            string fontsPath = Server.MapPath("~/App_Data/Fonts");
            GlobalFontSettings.FontResolver = new ArialFontResolver(fontsPath);
        }
    }

}