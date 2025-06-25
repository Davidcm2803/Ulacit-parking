namespace Ulacit_parking.Models.ViewModels
{
    public class ParkingViewModel
    {
        //revisar con el modelo de mysql y quitar lo que se deba
        public int ParkingLotId { get; set; }
        public string LicensePlate { get; set; }
        public string Action { get; set; }
        public int VehicleId { get; set; }
    }
}
