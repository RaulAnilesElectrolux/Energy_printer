using System.Web.Mvc;
using Energy_printer.Models;
using Energy_printer.Services;

namespace Energy_printer.Controllers
{
    public class LabelController : Controller
    {
        // ── GET /Label/Index ──────────────────────────────────────────────────
        // Muestra el PDF nativamente en el navegador (sin visor propio)
        public ActionResult Index(string model = null)
        {
            var svc = new EnergyLabelService(Server.MapPath("~/Content/"));
            var usa = svc.GetLabelDataUSA(model);
            var canada = svc.GetLabelDataCanada(model);

            // Genera el PDF con PDFSharp
            byte[] bytes = svc.GeneratePdf(usa, canada);

            // Muestra el PDF directamente usando el visor nativo del navegador
            Response.AddHeader("Content-Disposition", "inline; filename=EnergyLabels.pdf");
            return File(bytes, "application/pdf");
        }

        // ── GET /Label/Download ───────────────────────────────────────────────
        // Fuerza la descarga directa del archivo a la computadora
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
    }
}