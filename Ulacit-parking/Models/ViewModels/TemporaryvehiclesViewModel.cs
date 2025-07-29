using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ulacit_parking.Models;

namespace Ulacit_parking.Models.ViewModels
{
    public class TemporaryVehicleViewModel
    {
        [Key]
        public int Id { get; set; }

        public string LicensePlate { get; set; }
        public DateTime? EntryTime { get; set; }
        public DateTime? ExitTime { get; set; }

        public int ParkingLotId { get; set; }
        public virtual ParkingLot ParkingLot { get; set; }

        public int MovementLogId { get; set; }

        [ForeignKey("MovementLogId")]
        public virtual MovementLogs MovementLog { get; set; }
    }
}

