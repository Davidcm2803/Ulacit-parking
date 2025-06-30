using MySql.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;

namespace Ulacit_parking.Models
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class ParkingDatabaseContext : DbContext
    {
        public ParkingDatabaseContext() : base("name=ParkingDatabaseContext") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ParkingLot> ParkingLots { get; set; }
        public DbSet<ParkingAssignment> ParkingAssignments { get; set; }
        public DbSet<MovementLogs> MovementLogs { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Identification { get; set; }
        public int RoleId { get; set; }
        public string Password { get; set; }
        public string FirstLogin { get; set; }

        public virtual Role Role { get; set; }
        public virtual ICollection<Vehicle> Vehicles { get; set; }
        public virtual ICollection<ParkingAssignment> ParkingAssignments { get; set; }
    }

    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; }

        public virtual ICollection<User> Users { get; set; }
    }

    public class Vehicle
    {
        public int Id { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string LicensePlate { get; set; }
        public string VehicleType { get; set; }
        public int OwnerId { get; set; }
        public bool? UsesSpecialSpace { get; set; }

        public virtual User Owner { get; set; }
        public virtual ICollection<MovementLogs> MovementLogs { get; set; }
    }

    public class ParkingLot
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int RegularCapacity { get; set; }
        public int MotorcycleCapacity { get; set; }
        public int SpecialCapacity { get; set; }

        public virtual ICollection<ParkingAssignment> ParkingAssignments { get; set; }
        public virtual ICollection<MovementLogs> MovementLogs { get; set; }
    }

    public class ParkingAssignment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ParkingLotId { get; set; }
        public DateTime AssignmentDate { get; set; }

        public virtual User User { get; set; }
        public virtual ParkingLot ParkingLot { get; set; }
    }

    public class MovementLogs
    {
        public int Id { get; set; }
        public int? VehicleId { get; set; }
        public string EntryExit { get; set; } 
        public DateTime Timestamp { get; set; }
        public int? ParkingLotId { get; set; }

        public virtual Vehicle Vehicle { get; set; }
        public virtual ParkingLot ParkingLot { get; set; }
    }
}
