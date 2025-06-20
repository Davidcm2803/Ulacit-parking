using System;
using System.Collections.Generic;

namespace Ulacit_parking.Models.ViewModels
{
    using System.ComponentModel.DataAnnotations.Schema;

    public class UserViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        [Column("date_of_bith")] // Esto es lo importante
        public DateTime DateOfBirth { get; set; }

        public string Identification { get; set; }
        public int RoleId { get; set; }
        public string Password { get; set; }
        public string FirstLogin { get; set; }
        public string PasswordChanged { get; set; }

        public List<RoleViewModel> Roles { get; set; }

        public virtual ICollection<Vehicle> Vehicles { get; set; }
    }

}
