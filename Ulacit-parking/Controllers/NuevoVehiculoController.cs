using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;
using Ulacit_parking.Models.ViewModels;

namespace Ulacit_parking.Controllers
{
    public class NuevoVehiculoController : Controller
    {
        private ParkingDatabaseContext db = new ParkingDatabaseContext();

        public ActionResult Index()
        {
            var usuarios = db.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Name = u.Name
            }).ToList();

            var viewModel = new VehicleViewModel
            {
                Usuarios = usuarios
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VehicleViewModel newVehicle)
        {
            var admin = (User)Session["AdminLogged"];
            if (admin != null && admin.Email.EndsWith("@guarda.com"))
            {
                TempData["ErrorMessage"] = "No tienes permisos para agregar vehículos.";
                return RedirectToAction("Index", "AdminInicio");
            }

            if (ModelState.IsValid)
            {

                int vehiclesCount = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM vehicles WHERE owner_id = @p0",
                    newVehicle.OwnerId).FirstOrDefault();

                if (vehiclesCount >= 2)
                {
                    TempData["ErrorMessage"] = "Este usuario ya tiene 2 vehículos registrados.";
                    return RedirectToAction("Index");
                }

                int existe = db.Database.SqlQuery<int>(
                    @"SELECT COUNT(*) FROM vehicles 
                      WHERE LicensePlate = @p0 AND VehicleType = @p1",
                    newVehicle.LicensePlate, newVehicle.VehicleType).FirstOrDefault();

                if (existe > 0)
                {
                    TempData["ErrorMessage"] = "Ya existe un vehículo con esta placa y tipo de vehículo.";
                    return RedirectToAction("Index");
                }


                db.Database.ExecuteSqlCommand(
                    @"INSERT INTO vehicles 
                    (Brand, Color, LicensePlate, VehicleType, owner_id	, UsesSpecialSpace	, is_active, IsParked) 
                    VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7)",
                    newVehicle.Brand,
                    newVehicle.Color,
                    newVehicle.LicensePlate,
                    newVehicle.VehicleType,
                    newVehicle.OwnerId,
                    newVehicle.UsesSpecialSpace ? 1 : 0,
                    "1", 
                    0 
                );

                TempData["SuccessMessage"] = "Vehículo registrado exitosamente.";
                return RedirectToAction("Index");
            }

            newVehicle.Usuarios = db.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Name = u.Name
            }).ToList();

            return View("Index", newVehicle);
        }
    }
}
