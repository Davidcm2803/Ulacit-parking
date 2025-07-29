using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;
using Ulacit_parking.Models.ViewModels;

namespace Ulacit_parking.Controllers
{
    public class AdminController : Controller
    {
        private readonly ParkingDatabaseContext db = new ParkingDatabaseContext();

        private const string ContraseñaPorDefecto = "Ulacit123";

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string email, string password)
        {
            try
            {
                var user = db.Users.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    ViewBag.ErrorMessage = "El correo electrónico no está registrado.";
                    return View();
                }

                if (user.Password != password)
                {
                    ViewBag.ErrorMessage = "La contraseña es incorrecta.";
                    return View();
                }

                Session["UserId"] = user.Id;

                if (user.FirstLogin == "0")
                {
                    TempData["FirstLogin"] = true;
                    return RedirectToAction("CambiarPassword", "Admin");
                }

                TempData["SuccessMessage"] = "Inicio de sesión exitoso.";

                switch (user.RoleId)
                {
                    case 1:
                        Session["AdminLogged"] = user;
                        return RedirectToAction("Index", "AdminInicio");
                    case 2:
                        Session["AdminLogged"] = user;
                        return RedirectToAction("Index", "Access");
                    case 3:
                        Session["UserLogged"] = user;
                        return RedirectToAction("Index", "Usuario");
                    default:
                        ViewBag.ErrorMessage = "Rol de usuario no reconocido.";
                        return View();
                }
            }
            catch (Exception ex)
            {
                string fullError = ex.Message;
                if (ex.InnerException != null)
                    fullError += " -- INNER: " + ex.InnerException.Message;

                ViewBag.ErrorMessage = "Ocurrió un error: " + fullError;
                return View();
            }
        }

        [HttpGet]
        public ActionResult CambiarPassword()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login");

            int userId = (int)Session["UserId"];
            var user = db.Users.FirstOrDefault(u => u.Id == userId);

            if (user == null)
                return HttpNotFound();

            var model = new UserViewModel
            {
                Id = user.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarPassword(UserViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("Password", "Debe ingresar una nueva contraseña.");
                return View(model);
            }

            var user = db.Users.FirstOrDefault(u => u.Id == model.Id);
            if (user == null)
                return HttpNotFound();

            if (user.Password == model.Password)
            {
                ModelState.AddModelError("Password", "La nueva contraseña no puede ser igual a la anterior.");
                return View(model);
            }

            if (model.Password == ContraseñaPorDefecto)
            {
                ModelState.AddModelError("Password", "No puede usar la contraseña por defecto 'Ulacit123'.");
                return View(model);
            }

            user.Password = model.Password;
            user.FirstLogin = "1";
            db.SaveChanges();

            TempData["SuccessMessage"] = "Contraseña actualizada correctamente.";

            switch (user.RoleId)
            {
                case 1:
                case 2:
                    Session["AdminLogged"] = user;
                    return RedirectToAction("Index", "AdminInicio");
                case 3:
                    Session["UserLogged"] = user;
                    return RedirectToAction("Index", "Usuario");
                default:
                    return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            TempData["SuccessMessage"] = "Sesión cerrada correctamente.";
            return RedirectToAction("Login", "Admin");
        }

    }
}

