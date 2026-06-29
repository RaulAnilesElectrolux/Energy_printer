using Energy_printer.Models;
using Energy_printer.Services;
using PdfSharp.Pdf;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Energy_printer.Controllers
{
    public class LabelController : Controller
    {
        public ActionResult Index(int id = 1)
        {
            using (var db = new JZAPPROVALEntities())
            {
                DATA_LABEL datosLabel = db.DATA_LABEL.FirstOrDefault(d => d.ID_LABEL == id);

                if (datosLabel == null)
                    return HttpNotFound();

                EnergyLabelServiceUSA usaService = new EnergyLabelServiceUSA(Server.MapPath("~/Content/"));
                EnergyLabelServiceCanada canadaService = new EnergyLabelServiceCanada(Server.MapPath("~/Content/"));

                EnergyLabelDataUSA usaData = usaService.FromDataLabel(datosLabel);
                EnergyLabelDataCanada canadaData = canadaService.FromDataLabel(datosLabel);

                ViewBag.USA = usaData;
                ViewBag.Canada = canadaData;

                return View();
            }
        }

        public ActionResult PreviewPdf(int id = 1)
        {
            byte[] pdf = BuildPdf(id);
            return File(pdf, "application/pdf");
        }

        public ActionResult DownloadPdf(int id = 1)
        {
            byte[] pdf = BuildPdf(id);
            return File(pdf, "application/pdf", "EnergyLabels.pdf");
        }

        private byte[] BuildPdf(int id)
        {
            using (var db = new JZAPPROVALEntities())
            {
                DATA_LABEL datosLabel = db.DATA_LABEL.FirstOrDefault(d => d.ID_LABEL == id);

                if (datosLabel == null)
                    return new byte[0];

                var config = db.CONFIG_DATA_LABEL.ToList();

                EnergyLabelServiceUSA usaService = new EnergyLabelServiceUSA(Server.MapPath("~/Content/"));
                EnergyLabelServiceCanada canadaService = new EnergyLabelServiceCanada(Server.MapPath("~/Content/"));

                EnergyLabelDataUSA usaData = usaService.FromDataLabel(datosLabel);
                EnergyLabelDataCanada canadaData = canadaService.FromDataLabel(datosLabel);

                PdfDocument doc = new PdfDocument();
                doc.Info.Title = "Energy Labels";

                usaService.AddUSASheet(doc, usaData, config);

                canadaService.AddCanadaPage(doc, canadaData);
                canadaService.AddCanadaPage(doc, canadaData);

                using (MemoryStream stream = new MemoryStream())
                {
                    doc.Save(stream, false);
                    return stream.ToArray();
                }
            }
        }
    }
}