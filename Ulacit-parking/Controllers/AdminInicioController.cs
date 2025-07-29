using System.Web.Mvc;

namespace Ulacit_parking.Controllers
{
    [AuthorizeRole(1)]
    public class AdminInicioController : Controller
    {
        // GET: AdminInicio
        public ActionResult Index()
        {
            return View();
        }

    }
}
