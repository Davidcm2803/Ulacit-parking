using System;

namespace Ulacit_parking.Models.ViewModels
{
    public class MovementLogViewModel
    {
        public int id { get; set; }
        public Nullable<int> vehicle_id { get; set; }
        public string action { get; set; }
        public Nullable<System.DateTime> timestamp { get; set; }
        public string reason { get; set; }

        // En lugar de usar directamente el modelo 'Vehicles', usamos un ViewModel para 'Vehicle'
        public virtual VehicleViewModel Vehicle { get; set; }
    }
}
