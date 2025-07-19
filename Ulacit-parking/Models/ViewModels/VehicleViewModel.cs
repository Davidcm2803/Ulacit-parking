using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ulacit_parking.Models.ViewModels
{
    public class VehicleViewModel
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string LicensePlate { get; set; }
        public string VehicleType { get; set; }
        public int OwnerId { get; set; }
        [NotMapped]
        public string OwnerName { get; set; }
        public bool UsesSpecialSpace { get; set; }
        public string FirstLogin { get; set; }

        public List<UserViewModel> Usuarios { get; set; }
    }
}

