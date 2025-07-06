using System;
using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;

namespace Ulacit_parking.Controllers
{
    public class AccessController : Controller
    {
        private readonly ParkingDatabaseContext db = new ParkingDatabaseContext();

        [HttpGet]
        public ActionResult Index()
        {
            var parqueos = db.ParkingLots.ToList();
            return View(parqueos);
        }

        [HttpPost]
        public JsonResult VerificarIngresoSemaforo(string placa, int parkingLotId)
        {
            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == placa);
            var parking = db.ParkingLots.Find(parkingLotId);

            if (parking == null)
                return Json(new { success = false, color = "red", message = "Parqueo no encontrado." });

            if (vehicle == null)
                return Json(new { success = false, color = "red", message = "Vehículo no registrado en el sistema." });

            // Validar si el vehículo ya está dentro de cualquier parqueo
            var entradasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "E");
            var salidasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "S");
            if (entradasTotales > salidasTotales)
                return Json(new { success = false, color = "red", message = "Vehículo ya está dentro de un parqueo." });

            // Contar cantidad actual según el tipo de vehículo
            int ocupadosRegulares = db.Vehicles.Count(v =>
                v.VehicleType == "Carro" &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            int ocupadosMotos = db.Vehicles.Count(v =>
                v.VehicleType == "Moto" &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            int ocupadosEspeciales = db.Vehicles.Count(v =>
                v.UsesSpecialSpace == true &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            bool tieneEspacio =
                (vehicle.VehicleType == "Carro" && ocupadosRegulares < parking.RegularCapacity) ||
                (vehicle.VehicleType == "Moto" && ocupadosMotos < parking.MotorcycleCapacity) ||
                (vehicle.UsesSpecialSpace == true && ocupadosEspeciales < parking.SpecialCapacity);

            if (!tieneEspacio)
                return Json(new { success = false, color = "red", message = "Parqueo lleno para este tipo de vehículo." });

            return Json(new { success = true, color = "green", message = "Ingreso autorizado." });
        }

        [HttpPost]
        public JsonResult RegistrarIngreso(string placa, int parkingLotId)
        {
            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == placa);
            var parking = db.ParkingLots.Find(parkingLotId);

            if (parking == null)
                return Json(new { success = false, color = "red", message = "Parqueo no encontrado." });

            if (vehicle == null)
                return Json(new { success = false, color = "red", message = "Vehículo no registrado en el sistema." });

            var entradasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "E");
            var salidasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "S");

            if (entradasTotales > salidasTotales)
                return Json(new { success = false, color = "red", message = "Vehículo ya está dentro de un parqueo." });

            // Validar capacidad
            int ocupadosRegulares = db.Vehicles.Count(v =>
                v.VehicleType == "Carro" &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            int ocupadosMotos = db.Vehicles.Count(v =>
                v.VehicleType == "Moto" &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            int ocupadosEspeciales = db.Vehicles.Count(v =>
                v.UsesSpecialSpace == true &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            bool tieneEspacio =
                (vehicle.VehicleType == "Carro" && ocupadosRegulares < parking.RegularCapacity) ||
                (vehicle.VehicleType == "Moto" && ocupadosMotos < parking.MotorcycleCapacity) ||
                (vehicle.UsesSpecialSpace == true && ocupadosEspeciales < parking.SpecialCapacity);

            if (!tieneEspacio)
                return Json(new { success = false, color = "red", message = "Parqueo lleno para este tipo de vehículo." });

            db.MovementLogs.Add(new Ulacit_parking.Models.MovementLogs
            {
                VehicleId = vehicle.Id,
                EntryExit = "E",
                Timestamp = DateTime.Now,
                ParkingLotId = parking.Id
            });
            db.SaveChanges();

            return Json(new { success = true, color = "green", message = "Ingreso registrado correctamente." });
        }

        [HttpPost]
        public JsonResult RegistrarSalida(string placa, int parkingLotId)
        {
            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == placa);
            if (vehicle == null)
                return Json(new { success = false, message = "Vehículo no registrado." });

            var entradas = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.ParkingLotId == parkingLotId && m.EntryExit == "E");
            var salidas = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.ParkingLotId == parkingLotId && m.EntryExit == "S");

            if (entradas <= salidas)
                return Json(new { success = false, message = "Este vehículo no está dentro de este parqueo." });

            db.MovementLogs.Add(new Ulacit_parking.Models.MovementLogs
            {
                VehicleId = vehicle.Id,
                EntryExit = "S",
                Timestamp = DateTime.Now,
                ParkingLotId = parkingLotId
            });
            db.SaveChanges();

            return Json(new { success = true, message = "Salida registrada correctamente." });
        }

        [HttpPost]
        public JsonResult ObtenerEstadoParqueo(int parkingLotId)
        {
            var parking = db.ParkingLots.Find(parkingLotId);
            if (parking == null)
                return Json(new { success = false });

            int ocupadosRegulares = db.Vehicles.Count(v =>
                v.VehicleType == "Carro" &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            int ocupadosMotos = db.Vehicles.Count(v =>
                v.VehicleType == "Moto" &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            int ocupadosEspeciales = db.Vehicles.Count(v =>
                v.UsesSpecialSpace == true &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            return Json(new
            {
                success = true,
                regular = $"{ocupadosRegulares}/{parking.RegularCapacity}",
                moto = $"{ocupadosMotos}/{parking.MotorcycleCapacity}",
                especial = $"{ocupadosEspeciales}/{parking.SpecialCapacity}"
            });
        }
    }
}
