using System;
using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;

namespace Ulacit_parking.Controllers
{
    [AuthorizeRole(1, 2)]
    public class AccessController : Controller
    {
        private readonly ParkingDatabaseContext db = new ParkingDatabaseContext();

        // Lock para evitar race conditions al insertar temporales
        private static readonly object _tempVehicleLock = new object();

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
            var tempVehicle = db.TemporaryVehicles.FirstOrDefault(t => t.LicensePlate == placa && t.ExitTime == null);
            var parking = db.ParkingLots.Find(parkingLotId);

            if (parking == null)
                return Json(new { success = false, color = "red", message = "Parqueo no encontrado." });

            if (vehicle == null)
            {
                if (tempVehicle != null)
                    return Json(new { success = false, color = "red", message = "Vehículo temporal ya está dentro." });

                // Checar capacidad sumando temporales y vehículos registrados
                int ocupadosTemporales = db.TemporaryVehicles.Count(t =>
                    t.ParkingLotId == parking.Id && t.EntryTime != null && t.ExitTime == null);

                int ocupadosRegulares = db.Vehicles.Count(v =>
                    v.VehicleType == "Carro" &&
                    v.UsesSpecialSpace == false &&
                    db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                    db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

                int ocupadosTotales = ocupadosRegulares + ocupadosTemporales;

                if (ocupadosTotales >= parking.RegularCapacity)
                    return Json(new { success = false, color = "red", message = "Parqueo lleno para vehículos temporales." });

                return Json(new { success = true, color = "green", message = "Ingreso autorizado con pase único." });
            }

            // Vehículo registrado ya dentro
            var entradasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "E");
            var salidasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "S");
            if (entradasTotales > salidasTotales)
                return Json(new { success = false, color = "red", message = "Vehículo ya está dentro de un parqueo." });

            // Verifica capacidad disponible para vehículo registrado
            int ocupadosRegularesVeh = db.Vehicles.Count(v =>
                v.VehicleType == "Carro" &&
                v.UsesSpecialSpace == false &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            int ocupadosMotos = db.Vehicles.Count(v =>
                v.VehicleType == "Moto" &&
                v.UsesSpecialSpace == false &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            int ocupadosEspeciales = db.Vehicles.Count(v =>
                v.UsesSpecialSpace == true &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            bool tieneEspacio =
                (vehicle.VehicleType == "Carro" && ocupadosRegularesVeh < parking.RegularCapacity) ||
                (vehicle.VehicleType == "Moto" && ocupadosMotos < parking.MotorcycleCapacity) ||
                (vehicle.UsesSpecialSpace == true && ocupadosEspeciales < parking.SpecialCapacity);

            if (!tieneEspacio)
                return Json(new { success = false, color = "red", message = "Parqueo lleno para este tipo de vehículo." });

            return Json(new { success = true, color = "green", message = "Ingreso autorizado." });
        }

        [HttpPost]
        public JsonResult RegistrarIngreso(string placa, int parkingLotId)
        {
            lock (_tempVehicleLock)
            {
                var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == placa);
                var parking = db.ParkingLots.Find(parkingLotId);

                if (parking == null)
                    return Json(new { success = false, color = "red", message = "Parqueo no encontrado." });

                if (vehicle == null)
                {
                    // Buscar temporal activo (sin salida)
                    var tempActivo = db.TemporaryVehicles.FirstOrDefault(t => t.LicensePlate == placa && t.ExitTime == null);

                    if (tempActivo != null)
                        return Json(new { success = false, color = "red", message = "Este vehículo temporal ya está dentro." });

                    // Buscar si ya uso pase único (temporal con salida != null en ese parqueo)
                    var tempUsado = db.TemporaryVehicles.Any(t => t.LicensePlate == placa && t.ExitTime != null && t.ParkingLotId == parkingLotId);
                    if (tempUsado)
                        return Json(new { success = false, color = "orange", message = "Ya usó su pase único para este parqueo." });

                    // Verificar capacidad sumando temporales + vehículos
                    int ocupadosTemporales = db.TemporaryVehicles.Count(t =>
                        t.ParkingLotId == parking.Id && t.EntryTime != null && t.ExitTime == null);

                    int ocupadosRegulares = db.Vehicles.Count(v =>
                        v.VehicleType == "Carro" &&
                        v.UsesSpecialSpace == false &&
                        db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                        db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

                    int ocupadosTotales = ocupadosRegulares + ocupadosTemporales;

                    if (ocupadosTotales >= parking.RegularCapacity)
                        return Json(new { success = false, color = "red", message = "Parqueo lleno para vehículos temporales." });

                    try
                    {
                        var nuevoTemp = new TemporaryVehicle
                        {
                            LicensePlate = placa,
                            EntryTime = DateTime.Now,
                            ExitTime = null,
                            ParkingLotId = parkingLotId
                        };
                        db.TemporaryVehicles.Add(nuevoTemp);
                        db.SaveChanges();

                        db.MovementLogs.Add(new MovementLogs
                        {
                            TemporaryVehicleId = nuevoTemp.Id,
                            EntryExit = "E",
                            Timestamp = DateTime.Now,
                            ParkingLotId = parkingLotId
                        });
                        db.SaveChanges();

                        return Json(new { success = true, color = "green", message = "Ingreso registrado con pase único." });
                    }
                    catch (System.Data.Entity.Infrastructure.DbUpdateException)
                    {
                        return Json(new { success = false, color = "red", message = "Error: Vehículo temporal ya está dentro." });
                    }
                }

                // Vehículo registrado - validación y registro
                var entradasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "E");
                var salidasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "S");
                if (entradasTotales > salidasTotales)
                    return Json(new { success = false, color = "red", message = "Vehículo ya está dentro de un parqueo." });

                // Verificar capacidad para vehículos registrados
                int ocupadosRegularesVeh = db.Vehicles.Count(v =>
                    v.VehicleType == "Carro" &&
                    v.UsesSpecialSpace == false &&
                    db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                    db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

                int ocupadosMotos = db.Vehicles.Count(v =>
                    v.VehicleType == "Moto" &&
                    v.UsesSpecialSpace == false &&
                    db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                    db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

                int ocupadosEspeciales = db.Vehicles.Count(v =>
                    v.UsesSpecialSpace == true &&
                    db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                    db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

                bool tieneEspacio =
                    (vehicle.VehicleType == "Carro" && ocupadosRegularesVeh < parking.RegularCapacity) ||
                    (vehicle.VehicleType == "Moto" && ocupadosMotos < parking.MotorcycleCapacity) ||
                    (vehicle.UsesSpecialSpace == true && ocupadosEspeciales < parking.SpecialCapacity);

                if (!tieneEspacio)
                    return Json(new { success = false, color = "red", message = "Parqueo lleno para este tipo de vehículo." });

                db.MovementLogs.Add(new MovementLogs
                {
                    VehicleId = vehicle.Id,
                    EntryExit = "E",
                    Timestamp = DateTime.Now,
                    ParkingLotId = parking.Id
                });
                db.SaveChanges();

                return Json(new { success = true, color = "green", message = "Ingreso registrado correctamente." });
            }
        }

        [HttpPost]
        public JsonResult RegistrarSalida(string placa, int parkingLotId)
        {
            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == placa);
            var tempVehicle = db.TemporaryVehicles.FirstOrDefault(t => t.LicensePlate == placa && t.ExitTime == null);

            if (vehicle == null)
            {
                if (tempVehicle == null)
                    return Json(new { success = false, message = "Este vehículo no tiene registro de entrada." });

                tempVehicle.ExitTime = DateTime.Now;
                db.SaveChanges();

                db.MovementLogs.Add(new MovementLogs
                {
                    TemporaryVehicleId = tempVehicle.Id,
                    EntryExit = "S",
                    Timestamp = DateTime.Now,
                    ParkingLotId = tempVehicle.ParkingLotId
                });
                db.SaveChanges();

                return Json(new { success = true, message = "Salida registrada del vehículo temporal." });
            }

            var entradas = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.ParkingLotId == parkingLotId && m.EntryExit == "E");
            var salidas = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.ParkingLotId == parkingLotId && m.EntryExit == "S");

            if (entradas <= salidas)
                return Json(new { success = false, message = "Este vehículo no está dentro de este parqueo." });

            db.MovementLogs.Add(new MovementLogs
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
                v.UsesSpecialSpace == false &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            int ocupadosMotos = db.Vehicles.Count(v =>
                v.VehicleType == "Moto" &&
                v.UsesSpecialSpace == false &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            int ocupadosEspeciales = db.Vehicles.Count(v =>
                v.UsesSpecialSpace == true &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parking.Id && m.EntryExit == "S"));

            int ocupadosTemporales = db.TemporaryVehicles.Count(t =>
                t.ParkingLotId == parking.Id && t.EntryTime != null && t.ExitTime == null);

            return Json(new
            {
                success = true,
                regular = $"{ocupadosRegulares + ocupadosTemporales}/{parking.RegularCapacity}",
                moto = $"{ocupadosMotos}/{parking.MotorcycleCapacity}",
                especial = $"{ocupadosEspeciales}/{parking.SpecialCapacity}",
                temporales = ocupadosTemporales
            });
        }
    }
}
