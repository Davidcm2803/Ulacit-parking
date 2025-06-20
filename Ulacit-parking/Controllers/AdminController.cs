using System;
using System.Linq;
using System.Web.Mvc;
using Ulacit_parking.Models;
using MySql.Data.MySqlClient;

namespace Ulacit_parking.Controllers
{
    public class AdminController : Controller
    {
        private ParkingDatabaseContext db = new ParkingDatabaseContext();


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
                var admin = db.Users.FirstOrDefault(u => u.Email == email);
                if (admin == null)
                {
                    ViewBag.ErrorMessage = "El correo electrónico no está registrado.";
                    return View();
                }

                if (admin.Password != password)
                {
                    ViewBag.ErrorMessage = "La contraseña es incorrecta.";
                    return View();
                }
                switch (admin.RoleId)
                {
                    case 1: // Admin
                        Session["AdminLogged"] = admin;
                        TempData["SuccessMessage"] = "Inicio de sesión exitoso.";
                        return RedirectToAction("Index", "AdminInicio");

                    case 2: // Guarda
                        Session["AdminLogged"] = admin;
                        TempData["SuccessMessage"] = "Inicio de sesión exitoso.";
                        return RedirectToAction("Index", "GuardaInicio");

                    case 3: // Estudiante
                        Session["UserLogged"] = admin;
                        TempData["SuccessMessage"] = "Inicio de sesión exitoso.";
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


        // Método para probar la conexión directa a MySQL
        public ActionResult TestMySqlConnection()
        {
            string connStr = "server=localhost;port=3306;database=parkingdatabase;uid=root;pwd=;";
            try
            {
                using (var conn = new MySql.Data.MySqlClient.MySqlConnection(connStr))
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
