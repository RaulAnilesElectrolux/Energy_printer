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

                usaService.AddUSAPage(doc, usaData, true);
                usaService.AddUSAPage(doc, usaData,false);

                using (var stream = new MemoryStream())
                {
                    doc.Save(stream, false);
                    return File(stream.ToArray(), "application/pdf");
                }
            }

            /*
            var canada = claseCan.AddCanadaPage() ;
            var usa = svc.GetLabelDataUSA(model);

            // Genera el PDF con PDFSharp
            byte[] bytes = svc.GeneratePdf(usa, canada);

            // Muestra el PDF directamente usando el visor nativo del navegador
            Response.AddHeader("Content-Disposition", "inline; filename=EnergyLabels.pdf");
            return File(bytes, "application/pdf");
            */
        }

        // ── GET /Label/Download ───────────────────────────────────────────────
        // Fuerza la descarga directa del archivo a la computadora
        /*
        public ActionResult Download(string model = null)
        {
            var svc = new EnergyLabelService(Server.MapPath("~/Content/"));
            var usa = svc.GetLabelDataUSA(model);
            var canada = svc.GetLabelDataCanada(model);

            byte[] bytes = svc.GeneratePdf(usa, canada);

            string fileName = string.Format("EnergyLabels_{0}.pdf",
                (usa.MODEL ?? "label").Replace("*", "X").Replace("/", "-"));

            return File(bytes, "application/pdf", fileName);
        }
        */
    }
}