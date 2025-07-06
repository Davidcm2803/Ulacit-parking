using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ulacit_parking.Models;

[Table("movementlogs")]
public class MovementLogs
{
    [Key]
    public int Id { get; set; }
    public int? VehicleId { get; set; }
    public string EntryExit { get; set; }
    public DateTime Timestamp { get; set; }
    public int? ParkingLotId { get; set; }

    public virtual Vehicle Vehicle { get; set; }
    public virtual ParkingLot ParkingLot { get; set; }
}
