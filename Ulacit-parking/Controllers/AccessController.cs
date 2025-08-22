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

        [HttpGet]
        public ActionResult Index()
        {
            var parqueos = db.ParkingLots.ToList();
            return View(parqueos);
        }

        [HttpPost]
        public JsonResult VerificarIngresoSemaforo(string placa, int parkingLotId)
        {
            var parking = db.ParkingLots.Find(parkingLotId);
            if (parking == null)
                return Json(new { success = false, color = "red", message = "Parqueo no encontrado." });

            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == placa);
            var tempVehicle = db.TemporaryVehicles.FirstOrDefault(t => t.LicensePlate == placa);

            if (vehicle != null)
            {
                bool estaDentro = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "E") >
                                  db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "S");

                if (estaDentro)
                    return Json(new { success = false, color = "red", message = "Vehículo ya está dentro." });

                bool tieneEspacio =
                    (vehicle.VehicleType == "Carro" && EspaciosOcupados("Carro", parking.Id) < parking.RegularCapacity) ||
                    (vehicle.VehicleType == "Moto" && EspaciosOcupados("Moto", parking.Id) < parking.MotorcycleCapacity) ||
                    (vehicle.UsesSpecialSpace == true && EspaciosOcupados("Especial", parking.Id) < parking.SpecialCapacity);

                if (!tieneEspacio)
                    return Json(new { success = false, color = "red", message = "Parqueo lleno para este tipo de vehículo." });

                return Json(new { success = true, color = "green", message = "Ingreso autorizado." });
            }

            if (tempVehicle != null)
            {
                bool yaFueUsado = db.MovementLogs.Any(m => m.TemporaryVehicleId == tempVehicle.Id);

                if (yaFueUsado)
                    return Json(new { success = false, color = "red", message = "Este vehículo temporal ya fue usado. Debe registrarse como vehículo permanente." });
            }

            return Json(new { success = false, color = "yellow", message = "Vehículo no registrado", needsModal = true });
        }

        [HttpPost]
        public JsonResult RegistrarIngreso(string placa, int parkingLotId, string vehicleType = null, bool usesSpecialSpace = false)
        {
            var parking = db.ParkingLots.Find(parkingLotId);
            if (parking == null)
                return Json(new { success = false, message = "Parqueo no encontrado." });

            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == placa);

            if (vehicle != null)
            {
                int entradas = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "E");
                int salidas = db.MovementLogs.Count(m => m.VehicleId == vehicle.Id && m.EntryExit == "S");
                if (entradas > salidas)
                    return Json(new { success = false, message = "Vehículo ya está dentro de un parqueo." });

                db.MovementLogs.Add(new MovementLogs
                {
                    VehicleId = vehicle.Id,
                    EntryExit = "E",
                    Timestamp = DateTime.Now,
                    ParkingLotId = parking.Id
                });
                db.SaveChanges();

                return Json(new { success = true, message = "Ingreso registrado correctamente." });
            }

            var existingTempVehicle = db.TemporaryVehicles.FirstOrDefault(t => t.LicensePlate == placa);

            if (existingTempVehicle != null)
            {
                bool yaFueUsado = db.MovementLogs.Any(m => m.TemporaryVehicleId == existingTempVehicle.Id);
                if (yaFueUsado)
                    return Json(new { success = false, message = "Este vehículo temporal ya fue usado. Debe registrarse como vehículo permanente." });
            }

            if (string.IsNullOrEmpty(vehicleType))
                return Json(new { success = false, message = "Debe especificar el tipo de vehículo." });

            bool tieneEspacio = false;
            if (usesSpecialSpace)
            {
                tieneEspacio = EspaciosTemporales("Especial", parking.Id) < parking.SpecialCapacity;
                if (!tieneEspacio)
                    return Json(new { success = false, message = "Parqueo lleno para espacios especiales." });
            }
            else if (vehicleType == "Moto")
            {
                tieneEspacio = EspaciosTemporales("Moto", parking.Id) < parking.MotorcycleCapacity;
                if (!tieneEspacio)
                    return Json(new { success = false, message = "Parqueo lleno para motocicletas." });
            }
            else
            {
                tieneEspacio = EspaciosTemporales("Carro", parking.Id) < parking.RegularCapacity;
                if (!tieneEspacio)
                    return Json(new { success = false, message = "Parqueo lleno para vehículos regulares." });
            }

            TemporaryVehicle tempVehicle;
            if (existingTempVehicle == null)
            {
                tempVehicle = new TemporaryVehicle
                {
                    LicensePlate = placa,
                    VehicleType = vehicleType,
                    UsesSpecialSpace = usesSpecialSpace
                };
                db.TemporaryVehicles.Add(tempVehicle);
                db.SaveChanges();
            }
            else
            {
                tempVehicle = existingTempVehicle;
                tempVehicle.VehicleType = vehicleType;
                tempVehicle.UsesSpecialSpace = usesSpecialSpace;
                db.SaveChanges();
            }

            db.MovementLogs.Add(new MovementLogs
            {
                TemporaryVehicleId = tempVehicle.Id,
                EntryExit = "E",
                Timestamp = DateTime.Now,
                ParkingLotId = parking.Id
            });
            db.SaveChanges();

            return Json(new { success = true, message = "Ingreso registrado como temporal (uso único)." });
        }

        [HttpPost]
        public JsonResult RegistrarSalida(string placa, int parkingLotId)
        {
            var vehicle = db.Vehicles.FirstOrDefault(v => v.LicensePlate == placa);
            var tempVehicle = db.TemporaryVehicles.FirstOrDefault(t => t.LicensePlate == placa);

            if (vehicle != null)
            {
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
            else if (tempVehicle != null)
            {
                db.MovementLogs.Add(new MovementLogs
                {
                    TemporaryVehicleId = tempVehicle.Id,
                    EntryExit = "S",
                    Timestamp = DateTime.Now,
                    ParkingLotId = parkingLotId
                });
                db.SaveChanges();
                return Json(new { success = true, message = "Salida registrada de temporal." });
            }

            return Json(new { success = false, message = "No se encontró vehículo con esa placa." });
        }

        [HttpPost]
        public JsonResult ObtenerEstadoParqueo(int parkingLotId)
        {
            var parking = db.ParkingLots.Find(parkingLotId);
            if (parking == null)
                return Json(new { success = false });

            int carrosRegularesOcupados = EspaciosOcupados("Carro", parking.Id) + EspaciosTemporales("Carro", parking.Id);
            int motosOcupadas = EspaciosOcupados("Moto", parking.Id) + EspaciosTemporales("Moto", parking.Id);
            int especialesOcupados = EspaciosOcupados("Especial", parking.Id) + EspaciosTemporales("Especial", parking.Id);

            return Json(new
            {
                success = true,
                regular = $"{carrosRegularesOcupados}/{parking.RegularCapacity}",
                moto = $"{motosOcupadas}/{parking.MotorcycleCapacity}",
                especial = $"{especialesOcupados}/{parking.SpecialCapacity}",
                temporales = EspaciosTemporales("Carro", parking.Id) + EspaciosTemporales("Moto", parking.Id) + EspaciosTemporales("Especial", parking.Id)
            });
        }

        private int EspaciosOcupados(string tipo, int parkingLotId)
        {
            return db.Vehicles.Count(v =>
                ((tipo == "Especial" && v.UsesSpecialSpace == true) ||
                 (tipo == "Carro" && v.VehicleType == "Carro" && v.UsesSpecialSpace != true) ||
                 (tipo == "Moto" && v.VehicleType == "Moto" && v.UsesSpecialSpace != true)) &&
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parkingLotId && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.VehicleId == v.Id && m.ParkingLotId == parkingLotId && m.EntryExit == "S"));
        }

        private int EspaciosTemporales(string tipo, int parkingLotId)
        {
            return db.TemporaryVehicles.Count(t =>
                ((tipo == "Especial" && t.UsesSpecialSpace == true) ||
                 (tipo == "Carro" && t.VehicleType == "Carro" && t.UsesSpecialSpace != true) ||
                 (tipo == "Moto" && t.VehicleType == "Moto" && t.UsesSpecialSpace != true)) &&
                db.MovementLogs.Count(m => m.TemporaryVehicleId == t.Id && m.ParkingLotId == parkingLotId && m.EntryExit == "E") >
                db.MovementLogs.Count(m => m.TemporaryVehicleId == t.Id && m.ParkingLotId == parkingLotId && m.EntryExit == "S"));
        }
    }
}
