using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Energy_printer.Models;

namespace Energy_printer.Controllers
{
    public class FormatosController : Controller
    {
        // GET: Formatos
        public ActionResult Amarillo()
        {
            return View();
        }

        public ActionResult Blanco()
        {
            return View();
        }

        public ActionResult Formato4()
        {
            using (var db = new JZAPPROVALEntities())
            {
                var datosLabel = db.DATA_LABEL.FirstOrDefault(d => d.ID_LABEL == 1);
                return View(datosLabel);
            }
        }
    }
}