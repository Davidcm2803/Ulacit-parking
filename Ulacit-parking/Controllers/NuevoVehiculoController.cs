using System;
using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;
using Ulacit_parking.Models.ViewModels;

namespace Ulacit_parking.Controllers
{
    public class NuevoVehiculoController : Controller
    {
        private readonly ParkingDatabaseContext db = new ParkingDatabaseContext();

        [HttpGet]
        public ActionResult Index()
        {
            var vehiculos = db.Vehicles
                .Select(v => new VehicleViewModel
                {
                    Id = v.Id,
                    Brand = v.Brand,
                    Color = v.Color,
                    LicensePlate = v.LicensePlate,
                    VehicleType = v.VehicleType,
                    OwnerId = v.OwnerId,
                    OwnerName = v.Owner.Name,
                    UsesSpecialSpace = v.UsesSpecialSpace ?? false
                }).ToList();

            return View(vehiculos);
        }
        [HttpGet]
        public ActionResult Create()
        {
            var model = new VehicleViewModel
            {
                Usuarios = db.Users.Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.Name
                }).ToList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VehicleViewModel newVehicle)
        {
            if (!ModelState.IsValid)
            {
                newVehicle.Usuarios = db.Users.Select(u => new UserViewModel { Id = u.Id, Name = u.Name }).ToList();
                return View(newVehicle);
            }

            var countVehiculos = db.Vehicles.Count(v => v.OwnerId == newVehicle.OwnerId);
            if (countVehiculos >= 2)
            {
                TempData["ErrorMessage"] = "Este usuario ya tiene 2 vehículos registrados.";
                return RedirectToAction("Index");
            }

            bool existe = db.Vehicles.Any(v =>
                v.LicensePlate == newVehicle.LicensePlate &&
                v.VehicleType == newVehicle.VehicleType);

            if (existe)
            {
                TempData["ErrorMessage"] = "Ya existe un vehículo con esta placa y tipo de vehículo.";
                return RedirectToAction("Index");
            }

            var vehiculo = new Vehicle
            {
                Brand = newVehicle.Brand,
                Color = newVehicle.Color,
                LicensePlate = newVehicle.LicensePlate,
                VehicleType = newVehicle.VehicleType,
                OwnerId = newVehicle.OwnerId,
                UsesSpecialSpace = newVehicle.UsesSpecialSpace,
            };

            db.Vehicles.Add(vehiculo);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Vehículo registrado exitosamente.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public JsonResult EliminarVehiculo(int id)
        {
            try
            {
                var vehiculo = db.Vehicles.Find(id);
                if (vehiculo == null)
                    return Json(new { success = false, message = "Vehículo no encontrado." });

                db.Vehicles.Remove(vehiculo);
                db.SaveChanges();

                return Json(new { success = true, message = "Vehículo eliminado correctamente." });
            }
            catch (Exception ex)
            {
                var error = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = "Error al eliminar: " + error });
            }
        }
    }
}
