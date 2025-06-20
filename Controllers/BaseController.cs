using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace internshipPartTwo.Controllers
{
    public class BaseController : Controller
    {
      //used basecontroller what it does is simply become a controller handler which only enables access 
      //if the user has completed loggin in and then it creates a session 

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //here if session is not set then we redirect to login 
            if (Session["loggedIn"] == null)
            {
                filterContext.Result = RedirectToAction("Login", "Account");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
