using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;

namespace Ulacit_parking.Controllers
{
    public class MovementLogController : Controller
    {
        private ParkingDatabaseContext db = new ParkingDatabaseContext();

        public ActionResult Index()
        {
            var logs = db.MovementLogs
                .Include(m => m.Vehicle)
                .Include(m => m.ParkingLot)
                .OrderByDescending(m => m.Timestamp)
                .ToList();

            return View(logs);
        }

        [HttpGet]
        public ActionResult EntryExit()
        {
            ViewBag.VehicleId = new SelectList(db.Vehicles, "Id", "LicensePlate");
            ViewBag.ParkingLotId = new SelectList(db.ParkingLots, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EntryExit(Ulacit_parking.Models.MovementLogs log)
        {
            if (string.IsNullOrEmpty(log.EntryExit) || !new[] { "E", "S" }.Contains(log.EntryExit.ToUpper()))
                ModelState.AddModelError("EntryExit", "Tipo de movimiento inválido. Use 'E' o 'S'.");

            if (!db.Vehicles.Any(v => v.Id == log.VehicleId))
                ModelState.AddModelError("VehicleId", "Vehículo inválido.");

            if (!db.ParkingLots.Any(pl => pl.Id == log.ParkingLotId))
                ModelState.AddModelError("ParkingLotId", "Parqueo inválido.");

            if (ModelState.IsValid)
            {
                log.EntryExit = log.EntryExit.ToUpper();
                log.Timestamp = DateTime.Now;

                db.MovementLogs.Add(log);
                db.SaveChanges();

                TempData["SuccessMessage"] = $"Movimiento registrado correctamente: {(log.EntryExit == "E" ? "Entrada" : "Salida")}.";
                return RedirectToAction("EntryExit");
            }

            ViewBag.VehicleId = new SelectList(db.Vehicles, "Id", "LicensePlate", log.VehicleId);
            ViewBag.ParkingLotId = new SelectList(db.ParkingLots, "Id", "Name", log.ParkingLotId);
            return View(log);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}
