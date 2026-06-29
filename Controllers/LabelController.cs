using System.Web.Mvc;
using Energy_printer.Models;
using Energy_printer.Services;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace Energy_printer.Controllers
{
    public class LabelController : Controller
    {
        // ── GET /Label/Index ──────────────────────────────────────────────────
        // Muestra el PDF nativamente en el navegador (sin visor propio)
        public ActionResult Index(string model = null)
        {
            var canadaService = new EnergyLabelServiceCanada(Server.MapPath("~/Content/"));
            var usaService = new EnergyLabelServiceUSA(Server.MapPath("~/Content/"));

            var doc = new PdfDocument();

            using (var db = new JZAPPROVALEntities())
            {
                var datosLabel = db.DATA_LABEL.FirstOrDefault(d => d.ID_LABEL == 1);

                var canadaData = canadaService.FromDataLabel(datosLabel);
                var usaData = usaService.FromDataLabel(datosLabel);

                canadaService.AddCanadaPage(doc, canadaData);
                canadaService.AddCanadaPage(doc, canadaData);

                usaService.AddUSAPage(doc, usaData);
                usaService.AddUSAPage(doc, usaData);

                using (var stream = new MemoryStream())
                {
                    doc.Save(stream, false);
                    return File(stream.ToArray(), "application/pdf");
                }
            }
        }
    }
}