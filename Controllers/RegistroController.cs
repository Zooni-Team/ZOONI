using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System;
using System.Collections.Generic;

namespace Zooni.Controllers
{
    public class RegistroController : Controller
    {

        [HttpGet]
        [Route("Registro/Registro1")]
        public IActionResult Registro1()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                return RedirectToAction("Registro2");
            }

            return View();
        }
[HttpPost]
[Route("Registro/CrearUsuarioDesdeLogin")]
public IActionResult CrearUsuarioDesdeLogin(string correo, string contrasena)
{
    try
    {
        string checkQuery = "SELECT TOP 1 U.Id_User FROM [User] U INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail WHERE M.Correo = @Correo";
        var checkParams = new Dictionary<string, object> { { "@Correo", correo } };
        object existingId = BD.ExecuteScalar(checkQuery, checkParams);

        int idUser;

        if (existingId != null && existingId != DBNull.Value)
        {
            idUser = Convert.ToInt32(existingId);
        }
        else
        {
            string queryMail = @"
                INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion)
                VALUES (@Correo, @Contrasena, SYSDATETIME());
                SELECT SCOPE_IDENTITY();";

            var mailParams = new Dictionary<string, object>
            {
                { "@Correo", correo },
                { "@Contrasena", contrasena ?? "zooni@123" }
            };

            int idMail = Convert.ToInt32(BD.ExecuteScalar(queryMail, mailParams));

            string queryUser = @"
                INSERT INTO [User] (Id_Mail, Nombre, Apellido, Fecha_Registro, Id_Ubicacion, Id_TipoUsuario)
                VALUES (@Id_Mail, 'Nuevo', 'Usuario', SYSDATETIME(), 1, 1);
                SELECT SCOPE_IDENTITY();";

            var userParams = new Dictionary<string, object> { { "@Id_Mail", idMail } };
            idUser = Convert.ToInt32(BD.ExecuteScalar(queryUser, userParams));
        }

        HttpContext.Session.SetInt32("UserId", idUser);

        return RedirectToAction("Registro2", "Registro");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error CrearUsuarioDesdeLogin: " + ex.Message);
        TempData["Error"] = "No se pudo crear el usuario.";
        return RedirectToAction("Login", "Auth");
    }
}

        [HttpPost]
public IActionResult CrearUsuarioRapido(string correo, string contrasena)
{
    try
    {
        var existingUserId = HttpContext.Session.GetInt32("UserId");
        if (existingUserId != null)
            return RedirectToAction("Registro2", "Registro");

        string checkQuery = "SELECT TOP 1 U.Id_User FROM [User] U INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail WHERE M.Correo = @Correo";
        var checkParams = new Dictionary<string, object> { { "@Correo", correo } };
        object existingId = BD.ExecuteScalar(checkQuery, checkParams);

        if (existingId != null && existingId != DBNull.Value)
        {
            HttpContext.Session.SetInt32("UserId", Convert.ToInt32(existingId));
            return RedirectToAction("Registro2", "Registro");
        }

        string queryMail = @"
            INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion)
            VALUES (@Correo, @Contrasena, SYSDATETIME());
            SELECT SCOPE_IDENTITY();";

        var mailParams = new Dictionary<string, object>
        {
            { "@Correo", correo ?? $"temp_{Guid.NewGuid()}@zooni.app" },
            { "@Contrasena", contrasena ?? "temp123" }
        };

        int idMail = Convert.ToInt32(BD.ExecuteScalar(queryMail, mailParams));

        string queryUser = @"
            INSERT INTO [User] (Id_Mail, Nombre, Apellido, Fecha_Registro, Id_Ubicacion, Id_TipoUsuario, Estado_Usuario)
            VALUES (@Id_Mail, 'Nuevo', 'Usuario', SYSDATETIME(), 1, 1, 1);
            SELECT SCOPE_IDENTITY();";

        var userParams = new Dictionary<string, object> { { "@Id_Mail", idMail } };
        int idUser = Convert.ToInt32(BD.ExecuteScalar(queryUser, userParams));

        HttpContext.Session.SetInt32("UserId", idUser);

        return RedirectToAction("Registro2", "Registro");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error al crear usuario rápido: " + ex.Message);
        TempData["Error"] = "Error al conectar con la base de datos.";
        return RedirectToAction("Registro1", "Registro");
    }
}
        [HttpGet]
public IActionResult Registro2()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
    {
        TempData["Error"] = "Primero registrá un usuario 🐕‍🦺";
        return RedirectToAction("Registro1");
    }

    return View(new Mascota());
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Registro2(Mascota model)
{
    try
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["Error"] = "No hay usuario en sesión 🐾";
            return RedirectToAction("Registro1");
        }

        if (string.IsNullOrWhiteSpace(model.Especie))
        {
            ViewBag.Error = "Seleccioná una especie antes de continuar 🐕🐈";
            return View(model);
        }

        decimal pesoNormalizado = 0;
try
{
    string pesoInput = Request.Form["Peso"].ToString().Replace(',', '.');
    if (!decimal.TryParse(pesoInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoNormalizado))
        pesoNormalizado = 0;
    pesoNormalizado = Math.Round(pesoNormalizado, 2);
}
catch
{
    pesoNormalizado = 0;
}

        string query = @"
            INSERT INTO Mascota 
            (Id_User, Nombre, Especie, Edad, Raza, Sexo, Peso, Color, Chip, Foto, Esterilizado, Fecha_Nacimiento)
            VALUES 
            (@Id_User, @Nombre, @Especie, @Edad, @Raza, @Sexo, @Peso, @Color, @Chip, @Foto, @Esterilizado, SYSDATETIME());
            SELECT SCOPE_IDENTITY();";

        var parametros = new Dictionary<string, object>
        {
            { "@Id_User", userId.Value },
            { "@Nombre", model.Nombre ?? "MiMascota" },
            { "@Especie", model.Especie },
            { "@Edad", model.Edad },
            { "@Raza", model.Raza ?? "" },
            { "@Sexo", model.Sexo ?? "" },
            { "@Peso", pesoNormalizado },
            { "@Color", model.Color ?? "" },
            { "@Chip", model.Chip ?? "" },
            { "@Foto", model.Foto ?? "" },
            { "@Esterilizado", model.Esterilizado }
        };

        var idMascotaObj = BD.ExecuteScalar(query, parametros);

        if (idMascotaObj == null)
        {
            ViewBag.Error = "Error al registrar la mascota 😿";
            return View(model);
        }

        int idMascota = Convert.ToInt32(idMascotaObj);

        HttpContext.Session.SetInt32("MascotaId", idMascota);
        HttpContext.Session.SetString("MascotaNombre", model.Nombre ?? "MiMascota");
        HttpContext.Session.SetString("MascotaEspecie", model.Especie ?? "Desconocida");

        return RedirectToAction("Registro3");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error en Registro2 POST: " + ex.Message);
        ViewBag.Error = "Ocurrió un error al registrar la mascota 🐾";
        return View(model);
    }
}

[HttpGet]
public IActionResult Registro3()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    var mascotaNombre = HttpContext.Session.GetString("MascotaNombre");
    var mascotaEspecie = HttpContext.Session.GetString("MascotaEspecie");

    if (userId == null || string.IsNullOrEmpty(mascotaNombre) || string.IsNullOrEmpty(mascotaEspecie))
    {
        TempData["Error"] = "Faltan datos del registro anterior 🐾";
        return RedirectToAction("Registro1");
    }

    ViewBag.MascotaNombre = mascotaNombre;
    ViewBag.MascotaEspecie = mascotaEspecie;

    return View(new Mascota());
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Registro3(Mascota model)
{
    try
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var mascotaId = HttpContext.Session.GetInt32("MascotaId");

        if (userId == null || mascotaId == null)
        {
            TempData["Error"] = "No hay usuario o mascota activa 🐶";
            return RedirectToAction("Registro1");
        }

        if (string.IsNullOrEmpty(model.Raza) || string.IsNullOrEmpty(model.Sexo))
        {
            TempData["Error"] = "Completá todos los datos antes de continuar 🐾";
            return RedirectToAction("Registro3");
        }

decimal pesoNormalizado = 0;
try
{
    string pesoInput = Request.Form["Peso"].ToString().Replace(',', '.');
    if (!decimal.TryParse(pesoInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoNormalizado))
        pesoNormalizado = 0;
    pesoNormalizado = Math.Round(pesoNormalizado, 2);
}
catch
{
    pesoNormalizado = 0;
}


        string query = @"
            UPDATE Mascota
            SET Sexo = @Sexo,
                Raza = @Raza,
                Peso = @Peso,
                Edad = @Edad
            WHERE Id_Mascota = @Id_Mascota";

        var parametros = new Dictionary<string, object>
        {
            { "@Sexo", model.Sexo ?? "" },
            { "@Raza", model.Raza ?? "" },
            { "@Peso", pesoNormalizado },
            { "@Edad", model.Edad },
            { "@Id_Mascota", mascotaId.Value }
        };

        BD.ExecuteNonQuery(query, parametros);

        HttpContext.Session.SetString("MascotaSexo", model.Sexo ?? "");
        HttpContext.Session.SetString("MascotaRaza", model.Raza ?? "");
        HttpContext.Session.SetString("MascotaPeso", pesoNormalizado.ToString("F2"));
        HttpContext.Session.SetInt32("MascotaEdad", model.Edad);

        return RedirectToAction("Registro4", "Registro");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error en Registro3 POST: " + ex.Message);
        TempData["Error"] = "Error al guardar los datos de la mascota 🐾";
        return RedirectToAction("Registro3");
    }
}

        [HttpGet]
        public IActionResult Registro4()
        {
            var mascotaNombre = HttpContext.Session.GetString("MascotaNombre");
            var mascotaEspecie = HttpContext.Session.GetString("MascotaEspecie");
            var mascotaRaza = HttpContext.Session.GetString("MascotaRaza");
            var mascotaPeso = HttpContext.Session.GetInt32("MascotaPeso");
            var mascotaSexo = HttpContext.Session.GetString("MascotaSexo");
            var mascotaEdad = HttpContext.Session.GetInt32("MascotaEdad");

            if (string.IsNullOrEmpty(mascotaNombre) || string.IsNullOrEmpty(mascotaEspecie))
            {
                TempData["Error"] = "Faltan datos de la mascota.";
                return RedirectToAction("Registro3");
            }

            ViewBag.MascotaNombre = mascotaNombre;
            ViewBag.MascotaEspecie = mascotaEspecie;
            ViewBag.MascotaRaza = mascotaRaza;
            ViewBag.MascotaPeso = mascotaPeso;
            ViewBag.MascotaSexo = mascotaSexo;
            ViewBag.MascotaEdad = mascotaEdad;

            return View();
        }

        [HttpPost]
[HttpPost]
public IActionResult Registro4(string nombre, string apellido, string mail, string contrasena, string confirmarContrasena)
{
    try
    {
        var userId = HttpContext.Session.GetInt32("UserId");

        if (userId == null)
        {
            TempData["Error"] = "Sesión expirada. Iniciá nuevamente.";
            return RedirectToAction("Registro1");
        }

        if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(apellido) || string.IsNullOrEmpty(mail))
        {
            TempData["Error"] = "Por favor completá todos los campos.";
            return RedirectToAction("Registro4");
        }

        if (contrasena != confirmarContrasena)
        {
            TempData["Error"] = "Las contraseñas no coinciden.";
            return RedirectToAction("Registro4");
        }

        string checkMailQuery = @"
            SELECT COUNT(*)
            FROM Mail
            WHERE Correo = @Correo
              AND Id_Mail NOT IN (
                  SELECT Id_Mail FROM [User] WHERE Id_User = @Id_User
              )";

        var checkParams = new Dictionary<string, object>
        {
            { "@Correo", mail },
            { "@Id_User", userId.Value }
        };

        int existe = Convert.ToInt32(BD.ExecuteScalar(checkMailQuery, checkParams));
        if (existe > 0)
        {
            TempData["Error"] = "Ese correo ya está registrado. Probá con otro.";
            return RedirectToAction("Registro4");
        }

        string updateUserQuery = @"
            UPDATE [User]
            SET Nombre = @Nombre,
                Apellido = @Apellido
            WHERE Id_User = @Id_User";

        BD.ExecuteNonQuery(updateUserQuery, new Dictionary<string, object>
        {
            { "@Nombre", nombre },
            { "@Apellido", apellido },
            { "@Id_User", userId.Value }
        });

        string updateMailQuery = @"
            UPDATE M
            SET M.Correo = @Correo,
                M.Contrasena = @Contrasena
            FROM Mail M
            INNER JOIN [User] U ON M.Id_Mail = U.Id_Mail
            WHERE U.Id_User = @Id_User";

        BD.ExecuteNonQuery(updateMailQuery, new Dictionary<string, object>
        {
            { "@Correo", mail },
            { "@Contrasena", contrasena },
            { "@Id_User", userId.Value }
        });

        HttpContext.Session.SetString("UserNombre", nombre);
        HttpContext.Session.SetString("UserApellido", apellido);
        HttpContext.Session.SetString("UserMail", mail);
        HttpContext.Session.SetString("UserContrasena", contrasena);

        return RedirectToAction("Registro5");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error en Registro4 POST: " + ex.Message);
        TempData["Error"] = "Error al guardar los datos del usuario.";
        return RedirectToAction("Registro4");
    }
}




[HttpGet]
[Route("Registro/Registro5")]
public IActionResult Registro5()
{
    var mascotaNombre = HttpContext.Session.GetString("MascotaNombre");
    var mascotaEspecie = HttpContext.Session.GetString("MascotaEspecie");
    var mascotaRaza = HttpContext.Session.GetString("MascotaRaza");

    if (string.IsNullOrEmpty(mascotaNombre) || string.IsNullOrEmpty(mascotaEspecie))
    {
        TempData["Error"] = "Faltan datos de la mascota.";
        return RedirectToAction("Registro3");
    }

    ViewBag.MascotaNombre = mascotaNombre;
    ViewBag.MascotaEspecie = mascotaEspecie;
    ViewBag.MascotaRaza = mascotaRaza;

    return View();
}


[HttpPost]
[Route("Registro/Registro5")]
[ValidateAntiForgeryToken]
public IActionResult Registro5(string pais, string provincia, string ciudad, string codigoPais, string telefono)
{
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    TempData["Error"] = "Sesión expirada. Iniciá nuevamente.";
                    return RedirectToAction("Registro1");
                }

                HttpContext.Session.SetString("UserPais", pais);
                HttpContext.Session.SetString("UserProvincia", provincia);
                HttpContext.Session.SetString("UserCiudad", ciudad);
                HttpContext.Session.SetString("UserTelefono", $"{codigoPais} {telefono}");

                string query = @"
            UPDATE [User]
            SET Pais = @Pais,
                Provincia = @Provincia,
                Ciudad = @Ciudad,
                Telefono = @Telefono
            WHERE Id_User = @Id_User";

                var parametros = new Dictionary<string, object>
        {
            { "@Pais", pais },
            { "@Provincia", provincia },
            { "@Ciudad", ciudad },
            { "@Telefono", $"{codigoPais} {telefono}" },
            { "@Id_User", userId.Value }
        };

                BD.ExecuteNonQuery(query, parametros);

                string mascotaNombre = HttpContext.Session.GetString("MascotaNombre") ?? "";
                string mascotaEspecie = HttpContext.Session.GetString("MascotaEspecie") ?? "";
                string mascotaRaza = HttpContext.Session.GetString("MascotaRaza") ?? "";
decimal mascotaPeso = decimal.TryParse(HttpContext.Session.GetString("MascotaPeso"), out var p) ? p : 0;
                string mascotaSexo = HttpContext.Session.GetString("MascotaSexo") ?? "No definido";
                int mascotaEdad = HttpContext.Session.GetInt32("MascotaEdad") ?? 0;

                if (!string.IsNullOrEmpty(mascotaNombre))
                {
                    
                    string insertMascota = @"
                INSERT INTO Mascota (Nombre, Especie, Raza, Peso, Sexo, Edad, Id_User)
                VALUES (@Nombre, @Especie, @Raza, @Peso, @Sexo, @Edad, @Id_User)";

                    var paramMascota = new Dictionary<string, object>
            {
                { "@Nombre", mascotaNombre },
                { "@Especie", mascotaEspecie },
                { "@Raza", mascotaRaza },
                { "@Peso", mascotaPeso },
                { "@Sexo", mascotaSexo },
                { "@Edad", mascotaEdad },
                { "@Id_User", userId.Value }
            };

                    BD.ExecuteNonQuery(insertMascota, paramMascota);
                }

                TempData["Success"] = "¡Registro completado con éxito!";
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Registro5 POST: " + ex.Message);
                TempData["Error"] = "Error al finalizar el registro.";
                return RedirectToAction("Registro5");
            }
    
}
[HttpGet]
public IActionResult VerificarMail(string mail)
{
    string query = "SELECT COUNT(*) FROM Mail WHERE Correo = @Correo";
    var parametros = new Dictionary<string, object> { { "@Correo", mail } };
    int existe = Convert.ToInt32(BD.ExecuteScalar(query, parametros));

    return Json(new { existe = existe > 0 });
}
    }
    
}