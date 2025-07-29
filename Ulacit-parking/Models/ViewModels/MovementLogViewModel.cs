using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ulacit_parking.Models;

namespace Ulacit_parking.Models.ViewModels
{
    public class MovementLogsViewModel
    {
        [Key]
        public int Id { get; set; }

        [NotMapped]
        public string OwnerName { get; set; }

        public int? VehicleId { get; set; }
        public int? TemporaryVehicleId { get; set; }

        public string EntryExit { get; set; }
        public DateTime Timestamp { get; set; }
        public int? ParkingLotId { get; set; }

        public virtual Vehicle Vehicle { get; set; }
        public virtual ParkingLot ParkingLot { get; set; }

        public virtual TemporaryVehicle TemporaryVehicle { get; set; }


    }
}


