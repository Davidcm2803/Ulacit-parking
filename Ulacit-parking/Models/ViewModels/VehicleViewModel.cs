using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ulacit_parking.Models.ViewModels
{
    public class VehicleViewModel
    {
        public int Id { get; set; }
    public string Brand { get; set; }
    public string Color { get; set; }

    [Column("license_plate")]
    public string LicensePlate { get; set; }

    [Column("vehicle_type")]
    public string VehicleType { get; set; }
    public int OwnerId { get; set; }

    [Column("uses_special_space")]
    public bool UsesSpecialSpace { get; set; }

    [Column("is_active")]
    public string IsActive { get; set; }

    [Column("is_parked")]
    public bool IsParked { get; set; }

    [Column("ParkingLotId")]
    public int? ParkingLotId { get; set; }

    public List<UserViewModel> Usuarios { get; set; }
    }
}
