using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using Ulacit_parking.Models;
using Ulacit_parking.Models.ViewModels;

namespace Ulacit_parking.Controllers
{
    public class NuevoUsuarioController : Controller
    {
        private readonly ParkingDatabaseContext _db = new ParkingDatabaseContext();

        // LISTAR USUARIOS
        [HttpGet]
        public ActionResult Index()
        {
            var usuarios = _db.Users.Select(u => new UserViewModel
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Identification = u.Identification,
                DateOfBirth = u.DateOfBirth,
                RoleId = u.RoleId
            }).ToList();

            return View(usuarios);
        }

        // CREAR USUARIO - GET
        [HttpGet]
        public ActionResult Add()
        {
            var rolesList = _db.Roles.ToList();
            var rolesViewModelList = rolesList.Select(r => new RoleViewModel
            {
                Id = r.Id,
                RoleName = r.RoleName
            }).ToList();

            var model = new UserViewModel
            {
                Roles = rolesViewModelList
            };

            return View(model);
        }

        // CREAR USUARIO - POST
        [HttpPost]
        public JsonResult AgregarUsuario(string nombre, string cedula, string email, DateTime fechaNacimiento, string password, int rol)
        {
            try
            {
                var rolEntity = _db.Roles.FirstOrDefault(r => r.Id == rol);
                if (rolEntity == null)
                    return Json(new { success = false, message = "Rol inválido." });

                if (_db.Users.Any(u => u.Email == email))
                    return Json(new { success = false, message = "Correo ya registrado." });

                if (_db.Users.Any(u => u.Identification == cedula))
                    return Json(new { success = false, message = "Cédula ya registrada." });

                var nuevoUsuario = new User
                {
                    Name = nombre,
                    Email = email,
                    DateOfBirth = fechaNacimiento,
                    Identification = cedula,
                    RoleId = rol,
                    Password = password,
                    FirstLogin = "1",
                    PasswordChanged = "0"
                };

                using (var scope = new TransactionScope())
                {
                    _db.Users.Add(nuevoUsuario);
                    _db.SaveChanges();
                    scope.Complete();
                }

                return Json(new { success = true, message = "Usuario creado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            var usuario = _db.Users.FirstOrDefault(u => u.Id == id);
            if (usuario == null)
                return HttpNotFound();

            var rolesList = _db.Roles.ToList();

            var model = new UserViewModel
            {
                Id = usuario.Id,
                Name = usuario.Name,
                Email = usuario.Email,
                Identification = usuario.Identification,
                DateOfBirth = usuario.DateOfBirth,
                RoleId = usuario.RoleId,
                Roles = rolesList.Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    RoleName = r.RoleName
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public JsonResult EditarUsuario(int id, string nombre, string cedula, string email, DateTime fechaNacimiento, int rol)
        {
            try
            {
                var usuario = _db.Users.FirstOrDefault(u => u.Id == id);
                if (usuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado." });

                // Validar duplicados excepto el mismo usuario
                if (_db.Users.Any(u => u.Email == email && u.Id != id))
                    return Json(new { success = false, message = "Correo ya registrado." });

                if (_db.Users.Any(u => u.Identification == cedula && u.Id != id))
                    return Json(new { success = false, message = "Cédula ya registrada." });

                usuario.Name = nombre;
                usuario.Email = email;
                usuario.Identification = cedula;
                usuario.DateOfBirth = fechaNacimiento;
                usuario.RoleId = rol;

                _db.SaveChanges();

                return Json(new { success = true, message = "Usuario actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EliminarUsuario(int id)
        {
            try
            {
                var usuario = _db.Users.Include("Vehicles").FirstOrDefault(u => u.Id == id);
                if (usuario == null)
                {
                    return Json(new { success = false, message = "Usuario no encontrado." });
                }

                foreach (var vehiculo in usuario.Vehicles.ToList())
                {
                    _db.Vehicles.Remove(vehiculo);
                }

                // Eliminar usuario
                _db.Users.Remove(usuario);
                _db.SaveChanges();

                return Json(new { success = true, message = "Usuario eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

    }
}
