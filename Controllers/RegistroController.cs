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
        public IActionResult Registro2(string modo = "")
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["Error"] = "Primero registrá un usuario 🐕‍🦺";
                return RedirectToAction("Registro1");
            }

            ViewBag.Modo = modo;
            if (!string.IsNullOrEmpty(modo))
                HttpContext.Session.SetString("ModoRegistro", modo);


            return View(new Mascota());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Registro2(Mascota model, string modo = "")
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
                    decimal.TryParse(pesoInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoNormalizado);
                    pesoNormalizado = Math.Round(pesoNormalizado, 2);
                }
                catch { }

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

                int idMascota = Convert.ToInt32(BD.ExecuteScalar(query, parametros));
                HttpContext.Session.SetInt32("MascotaId", idMascota);
                HttpContext.Session.SetString("MascotaNombre", model.Nombre ?? "MiMascota");
                HttpContext.Session.SetString("MascotaEspecie", model.Especie ?? "Desconocida");

                Console.WriteLine($"🚀 Redirigiendo a Registro3 con modo = {modo}");
                return RedirectToAction("Registro3", new { modo });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Registro2 POST: " + ex.Message);
                ViewBag.Error = "Ocurrió un error al registrar la mascota 🐾";
                return View(model);
            }
        }

[HttpGet]
public IActionResult Registro3(string modo = "")
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null) 
        return RedirectToAction("Registro1");

    // 🟢 Recuperamos los valores de sesión (que vienen de Registro2)
    var nombre = HttpContext.Session.GetString("MascotaNombre") ?? "MiMascota";
    var especie = HttpContext.Session.GetString("MascotaEspecie") ?? "Desconocida";
    var raza = HttpContext.Session.GetString("MascotaRaza") ?? "";
    var peso = decimal.TryParse(HttpContext.Session.GetString("MascotaPeso"), out var p) ? p : 0;

    // 🟡 Llenamos el modelo
    var mascota = new Mascota
    {
        Nombre = nombre,
        Especie = especie,
        Raza = raza,
        Peso = peso
    };

    // 🟠 Pasamos los datos a ViewBag para el .cshtml
    ViewBag.MascotaNombre = nombre;
    ViewBag.MascotaEspecie = especie;
    ViewBag.MascotaRaza = raza;
    ViewBag.Modo = modo;

    if (!string.IsNullOrEmpty(modo))
        HttpContext.Session.SetString("ModoRegistro", modo);

    Console.WriteLine($"🐾 LLEGÓ A REGISTRO3 con especie: {especie}");

    return View(mascota);
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Registro3Post(Mascota model, string modo = "")
{
    try
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        var mascotaId = HttpContext.Session.GetInt32("MascotaId");
        if (userId == null || mascotaId == null)
            return RedirectToAction("Registro1");

        string modoFinal = !string.IsNullOrEmpty(modo) ? modo : HttpContext.Session.GetString("ModoRegistro") ?? "";

        decimal pesoNormalizado = 0;
        string pesoInput = Request.Form["Peso"].ToString().Replace(',', '.');
        decimal.TryParse(pesoInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoNormalizado);
        pesoNormalizado = Math.Round(pesoNormalizado, 2);

        string query = @"
    UPDATE Mascota
    SET Sexo=@Sexo, Raza=@Raza, Peso=@Peso, Edad=@Edad, Foto=@Foto
    WHERE Id_Mascota=@Id_Mascota";


        BD.ExecuteNonQuery(query, new Dictionary<string, object>
        {
            { "@Sexo", model.Sexo ?? "" },
            { "@Raza", model.Raza ?? "" },
            { "@Peso", pesoNormalizado },
            { "@Edad", model.Edad },
            { "@Foto", model.Foto ?? "" },
            { "@Id_Mascota", mascotaId.Value }
        });

        // 🟢 GUARDA EN SESIÓN LOS DATOS PARA REGISTRO4
        HttpContext.Session.SetString("MascotaRaza", model.Raza ?? "");
        HttpContext.Session.SetString("MascotaPeso", pesoNormalizado.ToString(System.Globalization.CultureInfo.InvariantCulture));
        HttpContext.Session.SetInt32("MascotaEdad", model.Edad);

        Console.WriteLine($"🐾 Registro3Post -> Especie: {HttpContext.Session.GetString("MascotaEspecie")}, Raza: {model.Raza}");

        if (modoFinal.ToLower() == "nuevamascota")
        {
            TempData["Exito"] = "Mascota agregada correctamente 🐾";
            return RedirectToAction("Configuracion", "Home");
        }

        return RedirectToAction("Registro4", "Registro");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error en Registro3Post: " + ex.Message);
        TempData["Error"] = "Error al guardar los datos.";
        return RedirectToAction("Registro3");
    }
}

[HttpGet]
[Route("Registro/Registro4")]
public IActionResult Registro4(string modo = "")
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
    {//SEBASTIAN CALVIÑO ESTUVO AQUI JIJI
        TempData["Error"] = "Sesión expirada. Volvé a iniciar sesión 🐾";
        return RedirectToAction("Registro1");
    }

    var nombreMascota = HttpContext.Session.GetString("MascotaNombre") ?? "MiMascota";
    var especie = HttpContext.Session.GetString("MascotaEspecie") ?? "Desconocida";
    var raza = HttpContext.Session.GetString("MascotaRaza") ?? "(vacía)";
    var peso = decimal.TryParse(HttpContext.Session.GetString("MascotaPeso"), out var p) ? p : 0;

    Console.WriteLine($"🐾 [REGISTRO4 GET] Datos recibidos → Nombre: {nombreMascota}, Especie: {especie}, Raza: {raza}, Peso: {peso}");

    ViewBag.MascotaNombre = nombreMascota;
    ViewBag.MascotaEspecie = especie;
    ViewBag.MascotaRaza = raza;
    ViewBag.MascotaPeso = peso;
    ViewBag.Modo = modo;

    return View();
}


[HttpPost]
[ValidateAntiForgeryToken]
[Route("Registro/Registro4")]
public IActionResult Registro4(string nombre, string apellido, string mail, string contrasena, string confirmarContrasena, string modo = "")
{
    try
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["Error"] = "Sesión expirada. Iniciá nuevamente.";
            return RedirectToAction("Registro1");
        }

        // 🟡 Detectar si estamos agregando nueva mascota
        var modoFinal = !string.IsNullOrEmpty(modo)
            ? modo
            : (HttpContext.Session.GetString("ModoRegistro") ?? "").ToLower();

        if (modoFinal == "nuevamascota")
        {
            TempData["Exito"] = "Mascota agregada correctamente 🐶";
            return RedirectToAction("Configuracion", "Home");
        }

        // 🧩 Validaciones básicas
        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(mail))
        {
            TempData["Error"] = "Por favor completá todos los campos.";
            return RedirectToAction("Registro4");
        }

        if (contrasena != confirmarContrasena)
        {
            TempData["Error"] = "Las contraseñas no coinciden.";
            return RedirectToAction("Registro4");
        }

        // 💌 Verificar si ya existe un usuario con el mismo mail
        string checkQuery = @"
            SELECT COUNT(*) 
            FROM Mail M 
            INNER JOIN [User] U ON U.Id_Mail = M.Id_Mail
            WHERE LOWER(M.Correo) = LOWER(@Correo)
              AND U.Id_User <> @Id_User;";

        int existe = Convert.ToInt32(BD.ExecuteScalar(checkQuery, new Dictionary<string, object>
        {
            { "@Correo", mail.Trim().ToLower() },
            { "@Id_User", userId.Value }
        }));

        if (existe > 0)
        {
            // ⚠️ Mostrar error en la misma vista sin romper flujo
            ViewBag.Error = "Este correo ya está registrado 🐾. Iniciá sesión o usá otro.";
            ViewBag.Nombre = nombre;
            ViewBag.Apellido = apellido;
            ViewBag.Mail = mail;
            ViewBag.Modo = modo;
            return View("Registro4");
        }

        // ✅ Actualizar datos personales
        string updateUserQuery = @"
            UPDATE [User]
            SET Nombre = @Nombre,
                Apellido = @Apellido
            WHERE Id_User = @Id_User;";

        BD.ExecuteNonQuery(updateUserQuery, new Dictionary<string, object>
        {
            { "@Nombre", nombre },
            { "@Apellido", apellido },
            { "@Id_User", userId.Value }
        });

        // ✅ Actualizar correo y contraseña
        string updateMailQuery = @"
            UPDATE M
            SET M.Correo = @Correo,
                M.Contrasena = @Contrasena
            FROM Mail M
            INNER JOIN [User] U ON M.Id_Mail = U.Id_Mail
            WHERE U.Id_User = @Id_User;";

        BD.ExecuteNonQuery(updateMailQuery, new Dictionary<string, object>
        {
            { "@Correo", mail },
            { "@Contrasena", contrasena },
            { "@Id_User", userId.Value }
        });

        // 🟢 Guardar en sesión
        HttpContext.Session.SetString("UserNombre", nombre);
        HttpContext.Session.SetString("UserApellido", apellido);
        HttpContext.Session.SetString("UserMail", mail);
        HttpContext.Session.SetString("UserContrasena", contrasena);

        Console.WriteLine($"✅ Registro4 (POST): usuario {nombre} {apellido}, mail {mail}");

        TempData["Exito"] = "Datos guardados correctamente 🦮";
        return RedirectToAction("Registro5", "Registro");
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

        // 🔹 Recuperar datos de la mascota desde la sesión
        string mascotaNombre = HttpContext.Session.GetString("MascotaNombre") ?? "";
        string mascotaEspecie = HttpContext.Session.GetString("MascotaEspecie") ?? "";
        string mascotaRaza = HttpContext.Session.GetString("MascotaRaza") ?? "";
        decimal mascotaPeso = decimal.TryParse(HttpContext.Session.GetString("MascotaPeso"), out var p) ? p : 0;
        string mascotaSexo = HttpContext.Session.GetString("MascotaSexo") ?? "No definido";
        int mascotaEdad = HttpContext.Session.GetInt32("MascotaEdad") ?? 0;

        // 🔧 Función interna para truncar strings largos
        string Truncar(string valor, int max) =>
            string.IsNullOrEmpty(valor) ? "" :
            valor.Length > max ? valor.Substring(0, max) : valor;

        // 🛡️ Sanitizar los campos ANTES del insert
        mascotaNombre = Truncar(mascotaNombre, 150);
        mascotaEspecie = Truncar(mascotaEspecie, 100);
        mascotaRaza = Truncar(mascotaRaza, 150);
        mascotaSexo = Truncar(mascotaSexo, 30);

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

        TempData["CuentaCreada"] = "✅ ¡Cuenta creada exitosamente!";
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
[Route("Registro/NuevaMascota")]
public IActionResult NuevaMascota()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
    {
        TempData["Error"] = "Debés iniciar sesión primero 🐾";
        return RedirectToAction("Login", "Auth");
    }

    return View("Registro2", new Mascota());
}

[HttpPost]
[ValidateAntiForgeryToken]
[Route("Registro/NuevaMascotaPost")]
public IActionResult NuevaMascotaPost(Mascota model)
{
    try
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["Error"] = "Sesión expirada. Volvé a iniciar sesión.";
            return RedirectToAction("Login", "Auth");
        }

        decimal pesoNormalizado = 0;
        string pesoInput = Request.Form["Peso"].ToString().Replace(',', '.');
        decimal.TryParse(pesoInput, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoNormalizado);
        pesoNormalizado = Math.Round(pesoNormalizado, 2);

        string query = @"
            INSERT INTO Mascota 
            (Id_User, Nombre, Especie, Edad, Raza, Sexo, Peso, Color, Chip, Foto, Esterilizado, Fecha_Nacimiento)
            VALUES 
            (@Id_User, @Nombre, @Especie, @Edad, @Raza, @Sexo, @Peso, @Color, @Chip, @Foto, @Esterilizado, SYSDATETIME());";

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

        BD.ExecuteNonQuery(query, parametros);

        TempData["Exito"] = "Mascota agregada correctamente 🐶";
        return RedirectToAction("Configuracion", "Home");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error al agregar nueva mascota: " + ex.Message);
        TempData["Error"] = "No se pudo agregar la mascota.";
        return RedirectToAction("Configuracion", "Home");
    }
}

    }
    
}