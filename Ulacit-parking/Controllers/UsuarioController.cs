using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;
using Ulacit_parking.Models.ViewModels;
using System.Data.Entity;

namespace Ulacit_parking.Controllers
{
    public class UsuarioController : Controller
    {
        private ParkingDatabaseContext db = new ParkingDatabaseContext();

        public ActionResult Index()
        {
            var user = Session["UserLogged"] as User;

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var vehiculos = db.Vehicles
                .Where(v => v.OwnerId == user.Id)
                .Select(v => new VehicleViewModel
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Color = v.Color,
                    LicensePlate = v.LicensePlate,
                    VehicleType = v.VehicleType,
                    UsesSpecialSpace = v.UsesSpecialSpace ?? false
                })
                .ToList();

            return View(vehiculos);
        }
        public ActionResult Historial(int id)
        {
            var historial = db.MovementLogs
               .Include(m => m.ParkingLot)
               .Where(m => m.VehicleId == id)
               .OrderByDescending(m => m.Timestamp)
               .ToList();

            ViewBag.VehicleId = id;
            return View(historial);
        }
    }
}
