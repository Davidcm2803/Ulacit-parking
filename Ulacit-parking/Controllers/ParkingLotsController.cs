using System;
using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;
using Ulacit_parking.Models.ViewModels;

namespace Ulacit_parking.Controllers
{
    public class ParkingLotController : Controller
    {
        private readonly ParkingDatabaseContext db = new ParkingDatabaseContext();

        public ActionResult Index()
        {
            var parkingLots = db.ParkingLots
                .Select(pl => new ParkingLotViewModel
                {
                    Id = pl.Id,
                    Name = pl.Name,
                    RegularCapacity = pl.RegularCapacity,
                    MotorcycleCapacity = pl.MotorcycleCapacity,
                    SpecialCapacity = pl.SpecialCapacity
                }).ToList();

            return View(parkingLots);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ParkingLotViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError("Name", "El nombre no puede estar vacío o contener solo espacios.");
            }
            else
            {
                bool existeNombre = db.ParkingLots.Any(pl => pl.Name.ToLower().Trim() == model.Name.ToLower().Trim());
                if (existeNombre)
                {
                    ModelState.AddModelError("Name", "Ya existe un estacionamiento con ese nombre.");
                }
            }

            if (model.RegularCapacity < 0)
            {
                ModelState.AddModelError("RegularCapacity", "La capacidad regular no puede ser negativa.");
            }

            if (model.MotorcycleCapacity < 0)
            {
                ModelState.AddModelError("MotorcycleCapacity", "La capacidad para motos no puede ser negativa.");
            }

            if (model.SpecialCapacity < 0)
            {
                ModelState.AddModelError("SpecialCapacity", "La capacidad especial no puede ser negativa.");
            }

            if (ModelState.IsValid)
            {
                var parkingLot = new ParkingLot
                {
                    Name = model.Name.Trim(),
                    RegularCapacity = model.RegularCapacity,
                    MotorcycleCapacity = model.MotorcycleCapacity,
                    SpecialCapacity = model.SpecialCapacity
                };

                db.ParkingLots.Add(parkingLot);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(model);
        }


        [HttpGet]
        public ActionResult Edit(int id)
        {
            var parkingLot = db.ParkingLots.Find(id);
            if (parkingLot == null)
                return HttpNotFound();

            var model = new ParkingLotViewModel
            {
                Id = parkingLot.Id,
                Name = parkingLot.Name,
                RegularCapacity = parkingLot.RegularCapacity,
                MotorcycleCapacity = parkingLot.MotorcycleCapacity,
                SpecialCapacity = parkingLot.SpecialCapacity
            };

            return View(model);
        }

        [HttpPost]
        public JsonResult EditarParqueo(int id, string name, int regularCapacity, int motorcycleCapacity, int specialCapacity)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return Json(new { success = false, message = "El nombre no puede estar vacío." });

                if (regularCapacity < 0 || motorcycleCapacity < 0 || specialCapacity < 0)
                    return Json(new { success = false, message = "Las capacidades no pueden ser negativas." });

                bool nombreDuplicado = db.ParkingLots
                    .Any(p => p.Id != id && p.Name.ToLower().Trim() == name.ToLower().Trim());

                if (nombreDuplicado)
                    return Json(new { success = false, message = "Ya existe un estacionamiento con ese nombre." });

                var parkingLot = db.ParkingLots.Find(id);
                if (parkingLot == null)
                    return Json(new { success = false, message = "Estacionamiento no encontrado." });

                parkingLot.Name = name.Trim();
                parkingLot.RegularCapacity = regularCapacity;
                parkingLot.MotorcycleCapacity = motorcycleCapacity;
                parkingLot.SpecialCapacity = specialCapacity;

                db.SaveChanges();

                return Json(new { success = true, message = "Estacionamiento actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }



        [HttpGet]
        public ActionResult Delete(int id)
        {
            var parkingLot = db.ParkingLots.Find(id);
            if (parkingLot == null)
                return HttpNotFound();

            return View(parkingLot);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var parkingLot = db.ParkingLots.Find(id);
            if (parkingLot == null)
                return HttpNotFound();

            db.ParkingLots.Remove(parkingLot);
            db.SaveChanges();

            TempData["Message"] = "Estacionamiento eliminado correctamente.";
            return RedirectToAction("Index");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();

            base.Dispose(disposing);
        }
    }
}
