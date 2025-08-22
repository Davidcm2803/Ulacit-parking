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
        public string VehicleType { get; set; }
        public bool? UsesSpecialSpace { get; set; }


    }
}

