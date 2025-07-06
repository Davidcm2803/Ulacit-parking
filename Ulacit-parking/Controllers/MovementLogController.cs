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
    }
}
