namespace Ulacit_parking.Models.ViewModels
{
    public class ParkingLotViewModel
    {
        //revisar con el modelo de mysql y quitar lo que se deba
        public int Id { get; set; }
        public string Name { get; set; }
        public int RegularCapacity { get; set; }
        public int MotorcycleCapacity { get; set; }
        public int SpecialCapacity { get; set; }

        // tener en cuenta mas no obligatorio
        public int RegularOccupied { get; set; }
        public int MotorcycleOccupied { get; set; }
        public int SpecialOccupied { get; set; }
    }
}
