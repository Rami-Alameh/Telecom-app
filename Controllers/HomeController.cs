using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace internshipPartTwo.Controllers
{
    public class HomeController : BaseController
    {

        public ActionResult Index()
        {
            return View("~/Views/Home/index.cshtml");
        }
        public ActionResult ClientsReport()
        {
            return View("~/Views/Home/ClientsReport.cshtml");
        }

        public ActionResult PhoneNumbersReport()
        {
            return View("~/Views/Home/PhoneNumbersReport.cshtml");
        }
        public ActionResult Clients()
        {
            return View("~/Views/Home/Clients.cshtml");
        }

        public ActionResult PhoneNumbers()
        {
            return View("~/Views/Home/PhoneNumbers.cshtml");
        }
        public ActionResult Reservations()
        {
            return View("~/Views/Home/Reservations.cshtml");
        }
    }
}