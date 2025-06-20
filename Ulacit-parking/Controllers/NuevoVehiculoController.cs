using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;
using Ulacit_parking.Models.ViewModels;

namespace Ulacit_parking.Controllers
{
    public class NuevoVehiculoController : Controller
    {
        private ParkingDatabaseContext db = new ParkingDatabaseContext();

        // Acción GET para mostrar el formulario de creación
        public ActionResult Index()
        {
            // Obtener los usuarios para el dropdown
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

        // Acción POST para crear un nuevo vehículo
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(VehicleViewModel newVehicle)
        {
            // Validar si el usuario logueado tiene restricción por dominio
            var admin = (User)Session["AdminLogged"];
            if (admin != null && admin.Email.EndsWith("@guarda.com"))
            {
                TempData["ErrorMessage"] = "No tienes permisos para agregar vehículos.";
                return RedirectToAction("Index", "AdminInicio");
            }

            if (ModelState.IsValid)
            {
                // Verificar si el usuario ya tiene 2 vehículos
                int vehiclesCount = db.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM vehicles WHERE owner_id = @p0",
                    newVehicle.OwnerId).FirstOrDefault();

                if (vehiclesCount >= 2)
                {
                    TempData["ErrorMessage"] = "Este usuario ya tiene 2 vehículos registrados.";
                    return RedirectToAction("Index");
                }

                // Verificar si ya existe un vehículo con misma placa y tipo
                int existe = db.Database.SqlQuery<int>(
                    @"SELECT COUNT(*) FROM vehicles 
                      WHERE LicensePlate = @p0 AND VehicleType = @p1",
                    newVehicle.LicensePlate, newVehicle.VehicleType).FirstOrDefault();

                if (existe > 0)
                {
                    TempData["ErrorMessage"] = "Ya existe un vehículo con esta placa y tipo de vehículo.";
                    return RedirectToAction("Index");
                }

                // Insertar nuevo vehículo directamente en la base de datos
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
                    "1", // is_active
                    0    // is_parked
                );

                TempData["SuccessMessage"] = "Vehículo registrado exitosamente.";
                return RedirectToAction("Index");
            }

            // Si el modelo no es válido, recargar los usuarios
            newVehicle.Usuarios = db.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Name = u.Name
            }).ToList();

            return View("Index", newVehicle);
        }
    }
}
