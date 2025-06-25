using System;

namespace Ulacit_parking.Models.ViewModels
{
    public class MovementLogViewModel
    {
        //revisar con el modelo de mysql y quitar lo que se deba
        public int id { get; set; }
        public Nullable<int> vehicle_id { get; set; }
        public string action { get; set; }
        public Nullable<System.DateTime> timestamp { get; set; }
        public string reason { get; set; }

        public virtual VehicleViewModel Vehicle { get; set; }
    }
}
