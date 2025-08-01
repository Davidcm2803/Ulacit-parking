﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using Ulacit_parking.Models;
using Ulacit_parking.Models.ViewModels;

namespace Ulacit_parking.Controllers
{
    [AuthorizeRole(1)]
    public class NuevoUsuarioController : Controller
    {
        private readonly ParkingDatabaseContext db = new ParkingDatabaseContext();

        [HttpGet]
        public ActionResult Index()
        {
            var usuarios = db.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Identification = u.Identification,
                    DateOfBirth = u.DateOfBirth,
                    RoleId = u.RoleId,
                    RoleName = db.Roles
                                .Where(r => r.Id == u.RoleId)
                                .Select(r => r.RoleName)
                                .FirstOrDefault() ?? "Sin rol"
                }).ToList();

            return View(usuarios);
        }



        [HttpGet]
        public ActionResult Add()
        {
            var rolesList = db.Roles.ToList();
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

        [HttpPost]
        public JsonResult AgregarUsuario(string nombre, string cedula, string email, DateTime fechaNacimiento, string password, int rol)
        {
            try
            {
                if (!email.EndsWith("@ulacit.ed.cr", StringComparison.OrdinalIgnoreCase))
                    return Json(new { success = false, message = "El correo debe ser del dominio @ulacit.ed.cr." });

                var rolEntity = db.Roles.FirstOrDefault(r => r.Id == rol);
                if (rolEntity == null)
                    return Json(new { success = false, message = "Rol inválido." });

                if (db.Users.Any(u => u.Email == email))
                    return Json(new { success = false, message = "Correo ya registrado." });

                if (db.Users.Any(u => u.Identification == cedula))
                    return Json(new { success = false, message = "Cédula ya registrada." });

                var nuevoUsuario = new User
                {
                    Name = nombre,
                    Email = email,
                    DateOfBirth = fechaNacimiento,
                    Identification = cedula,
                    RoleId = rol,
                    Password = password,
                    FirstLogin = "0",
                };

                using (var scope = new TransactionScope())
                {
                    db.Users.Add(nuevoUsuario);
                    db.SaveChanges();
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
            var usuario = db.Users.FirstOrDefault(u => u.Id == id);
            if (usuario == null)
                return HttpNotFound();

            var roles = db.Roles
                .AsEnumerable()
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.RoleName
                }).ToList();

            ViewBag.Roles = roles;

            var model = new UserViewModel
            {
                Id = usuario.Id,
                Name = usuario.Name,
                Email = usuario.Email,
                Identification = usuario.Identification,
                DateOfBirth = usuario.DateOfBirth,
                RoleId = usuario.RoleId,
            };
            return View(model);
        }



        [HttpPost]
        public JsonResult EditarUsuario(int id, string nombre, string cedula, string email, DateTime fechaNacimiento, int rol)
        {
            try
            {
                var usuario = db.Users.FirstOrDefault(u => u.Id == id);
                if (usuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado." });


                if (db.Users.Any(u => u.Email == email && u.Id != id))
                    return Json(new { success = false, message = "Correo ya registrado." });

                if (db.Users.Any(u => u.Identification == cedula && u.Id != id))
                    return Json(new { success = false, message = "Cédula ya registrada." });

                usuario.Name = nombre;
                usuario.Email = email;
                usuario.Identification = cedula;
                usuario.DateOfBirth = fechaNacimiento;
                usuario.RoleId = rol;

                db.SaveChanges();

                return Json(new { success = true, message = "Usuario actualizado exitosamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult EliminarUsuario(int id)
        {
            try
            {
                var usuario = db.Users.FirstOrDefault(u => u.Id == id);

                if (usuario == null)
                    return Json(new { success = false, message = "Usuario no encontrado." });

                db.Entry(usuario).Collection(u => u.Vehicles).Load();

                foreach (var vehiculo in usuario.Vehicles.ToList())
                {
                    var movimientos = db.MovementLogs.Where(m => m.VehicleId == vehiculo.Id).ToList();
                    if (movimientos.Any())
                    {
                        db.MovementLogs.RemoveRange(movimientos);
                    }

                    db.Vehicles.Remove(vehiculo);
                }

                db.Users.Remove(usuario);
                db.SaveChanges();

                return Json(new { success = true, message = "Usuario eliminado exitosamente." });
            }
            catch (Exception ex)
            {
                var error = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = "Error: " + error });
            }
        }
    }
}
