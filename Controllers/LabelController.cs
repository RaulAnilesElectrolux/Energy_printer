using System.IO;
using System.Linq;
using System.Web.Mvc;
using Energy_printer.Models;
using Energy_printer.Services;
using PdfSharp.Pdf;

namespace Energy_printer.Controllers
{
    public class LabelController : Controller
    {
        public ActionResult Index(string model = null)
        {
            var usaService = new EnergyLabelServiceUSA(Server.MapPath("~/Content/"));
            var canadaService = new EnergyLabelServiceCanada(Server.MapPath("~/Content/"));

            var doc = new PdfDocument();

            using (var db = new JZAPPROVALEntities())
            {
                var datosLabel = db.DATA_LABEL.FirstOrDefault(d => d.ID_LABEL == 1);

                if (datosLabel == null)
                {
                    return Content("No se encontró DATA_LABEL con ID_LABEL = 1");
                }

                var configUSA = db.CONFIG_DATA_LABEL
                    .FirstOrDefault(c => c.LABEL_TYPE != null && c.LABEL_TYPE.ToUpper() == "USA");

                var canadaData = canadaService.FromDataLabel(datosLabel);
                var usaData = usaService.FromDataLabel(datosLabel);

                usaService.AddUSASheet(doc, usaData, configUSA);

                /*canadaService.AddCanadaPage(doc, canadaData);
                canadaService.AddCanadaPage(doc, canadaData);*/

                using (var stream = new MemoryStream())
                {
                    doc.Save(stream, false);
                    return File(stream.ToArray(), "application/pdf");
                }
            }
        }
    }
}