using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;

namespace Ulacit_parking.Controllers
{
    public class AccessController : Controller
    {
        private readonly ParkingDatabaseContext db = new ParkingDatabaseContext();

        // Diccionario estático en memoria para registrar un pase gratuito por placa
        // Key: placa del vehículo, Value: fecha de salida (null si está adentro)
        private static Dictionary<string, DateTime?> pasesGratuitos = new Dictionary<string, DateTime?>();

        [HttpGet]
        public ActionResult Index()
        {
            // Cargar todos los parqueos para mostrar en el dropdown
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

            // Si el vehículo no está registrado
            if (vehicle == null)
            {
                // Primera vez que intenta entrar → permitir
                if (!pasesGratuitos.ContainsKey(placa))
                    return Json(new { success = true, color = "green", message = "Ingreso autorizado con pase único." });

                // Ya entró y no ha salido
                if (pasesGratuitos[placa] == null)
                    return Json(new { success = false, color = "red", message = "Vehículo ya está dentro." });

                // Ya usó su pase
                return Json(new { success = false, color = "red", message = "Vehículo ya utilizó su pase único." });
            }

            // Si el vehículo ya está dentro de algún parqueo, no se permite nuevo ingreso
            var entradasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "E");
            var salidasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "S");
            if (entradasTotales > salidasTotales)
                return Json(new { success = false, color = "red", message = "Vehículo ya está dentro de un parqueo." });

            // Verifica espacio disponible para el tipo de vehículo
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

            // Si no está registrado → lógica del pase único
            if (vehicle == null)
            {
                if (!pasesGratuitos.ContainsKey(placa))
                {
                    pasesGratuitos[placa] = null; // Registrar ingreso con pase
                    return Json(new { success = true, color = "green", message = "Ingreso registrado con pase único." });
                }

                if (pasesGratuitos[placa] == null)
                    return Json(new { success = false, color = "red", message = "Este vehículo ya está dentro." });

                return Json(new { success = false, color = "red", message = "Este vehículo ya usó su pase único." });
            }

            // Validar que no esté dentro aún
            var entradasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "E");
            var salidasTotales = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "S");
            if (entradasTotales > salidasTotales)
                return Json(new { success = false, color = "red", message = "Vehículo ya está dentro de un parqueo." });

            // Capacidad por tipo
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

            // Registrar entrada
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

            // Manejo para vehículos no registrados
            if (vehicle == null)
            {
                if (!pasesGratuitos.ContainsKey(placa))
                    return Json(new { success = false, message = "Este vehículo no tiene registro de entrada." });

                if (pasesGratuitos[placa] != null)
                    return Json(new { success = false, message = "Este vehículo ya salió." });

                pasesGratuitos[placa] = DateTime.Now;
                return Json(new { success = true, message = "Salida registrada del vehículo no registrado." });
            }

            // Verifica que esté dentro del parqueo
            var entradas = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.ParkingLotId == parkingLotId && m.EntryExit == "E");
            var salidas = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.ParkingLotId == parkingLotId && m.EntryExit == "S");

            if (entradas <= salidas)
                return Json(new { success = false, message = "Este vehículo no está dentro de este parqueo." });

            // Registrar salida
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

            // Calcular ocupación actual
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

            // Devolver resumen al cliente
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
