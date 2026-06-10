using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static EnergyGuideUSALabel;
using static EnerGuideCanadaLabel;

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
            return View();
        }
    }
}