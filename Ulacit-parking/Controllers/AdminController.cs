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

                // Redirigir según el rol
                TempData["SuccessMessage"] = "Inicio de sesión exitoso.";
                switch (user.RoleId)
                {
                    case 1:
                        Session["AdminLogged"] = user;
                        return RedirectToAction("Index", "AdminInicio");
                    case 2:
                        Session["AdminLogged"] = user;
                        return RedirectToAction("Index", "AdminInicio");
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

            user.Password = model.Password;
            user.FirstLogin = "1";

            db.SaveChanges();

            TempData["SuccessMessage"] = "Contraseña actualizada correctamente.";

            switch (user.RoleId)
            {
                case 1:
                    Session["AdminLogged"] = user;
                    return RedirectToAction("Index", "AdminInicio");
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

        public ActionResult TestMySqlConnection()
        {
            string connStr = "server=localhost;port=3306;database=parkingdatabase;uid=root;pwd=;";
            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                }
                return Content("Conexión MySQL exitosa");
            }
            catch (Exception ex)
            {
                return Content("Error de conexión: " + ex.Message);
            }
        }
    }
}
