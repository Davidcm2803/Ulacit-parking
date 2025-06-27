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
        public DateTime DateOfBirth { get; set; }
        public string Identification { get; set; }
        public int RoleId { get; set; }
        [NotMapped]
        public string RoleName { get; set; }
        public string Password { get; set; }
        public string FirstLogin { get; set; }



        public List<RoleViewModel> Roles { get; set; }

        public virtual ICollection<Vehicle> Vehicles { get; set; }
    }

}
