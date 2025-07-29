using System;
using System.Collections.Generic;
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
        private ParkingDatabaseContext db = new ParkingDatabaseContext();

        public ActionResult Index()
        {
            var logs = db.MovementLogs
                .Include(m => m.Vehicle)
                .Include(m => m.ParkingLot)
                .Include(m => m.TemporaryVehicle)
                .OrderByDescending(m => m.Timestamp)
                .ToList();

            var logsViewModel = logs.Select(m => new MovementLogsViewModel // sirve para hacerlos en view models y pasarlos a la vista
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
    }
}
