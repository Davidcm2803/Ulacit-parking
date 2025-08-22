using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;
using Ulacit_parking.Models.ViewModels;

namespace Ulacit_parking.Controllers
{
    [AuthorizeRole(1)]
    public class MovementLogController : Controller
    {
        private readonly ParkingDatabaseContext db = new ParkingDatabaseContext();

        public ActionResult Index()
        {
            var logs = db.MovementLogs
                .Include(m => m.Vehicle)
                .Include(m => m.ParkingLot)
                .Include(m => m.TemporaryVehicle)
                .OrderByDescending(m => m.Timestamp)
                .ToList();

            var logsViewModel = logs.Select(m => new MovementLogsViewModel
            {
                Id = m.Id,
                VehicleId = m.VehicleId,
                TemporaryVehicleId = m.TemporaryVehicleId,
                EntryExit = m.EntryExit,
                Timestamp = m.Timestamp,
                ParkingLotId = m.ParkingLotId,
                Vehicle = m.Vehicle,
                ParkingLot = m.ParkingLot,
                TemporaryVehicle = m.TemporaryVehicle
            }).ToList();

            return View(logsViewModel);
        }

        [HttpGet]
        public JsonResult Filtrar(string search = "", string tipo = "todos")
        {
            var query = db.MovementLogs
                .Include(m => m.Vehicle)
                .Include(m => m.ParkingLot)
                .Include(m => m.TemporaryVehicle)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(m =>
                    (m.Vehicle != null && (
                        m.Vehicle.LicensePlate.ToLower().Contains(search) ||
                        m.Vehicle.Brand.ToLower().Contains(search)
                    )) ||
                    (m.ParkingLot != null && m.ParkingLot.Name.ToLower().Contains(search)) ||
                    (m.TemporaryVehicle != null && m.TemporaryVehicle.LicensePlate.ToLower().Contains(search))
                );
            }

            if (tipo == "registrados")
            {
                query = query.Where(m => m.VehicleId != null);
            }
            else if (tipo == "temporales")
            {
                query = query.Where(m => m.TemporaryVehicleId != null);
            }

            var logs = query
                .OrderByDescending(m => m.Timestamp)
                .Take(100)
                .ToList();

            var logsViewModel = logs.Select(m => new
            {
                m.Id,
                Marca = m.Vehicle?.Brand ?? "N/D",
                Placa = m.Vehicle?.LicensePlate ?? m.TemporaryVehicle?.LicensePlate ?? "N/D",
                Parqueo = m.ParkingLot?.Name ?? "N/D",
                Tipo = m.EntryExit == "E" ? "Entrada" : "Salida",
                Fecha = m.Timestamp.ToString("dd/MM/yyyy HH:mm"),
                EsTemporal = m.TemporaryVehicleId != null
            });

            return Json(logsViewModel, JsonRequestBehavior.AllowGet);
        }
    }
}
