using System.Globalization;
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
            string qPerfilCheck = "SELECT COUNT(*) FROM Perfil WHERE Id_Usuario = @Id";
int existePerfil = Convert.ToInt32(BD.ExecuteScalar(qPerfilCheck, new Dictionary<string, object> { { "@Id", idUser } }));

if (existePerfil == 0)
{
    string qPerfilInsert = @"
        INSERT INTO Perfil (Id_Usuario, FotoPerfil, Descripcion, AniosVigencia)
        VALUES (@U, '/img/perfil/default.png', 'Amante de los animales ❤️', 1)";
    BD.ExecuteNonQuery(qPerfilInsert, new Dictionary<string, object> { { "@U", idUser } });
}

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
        string qPerfilCheck = "SELECT COUNT(*) FROM Perfil WHERE Id_Usuario = @Id";
int existePerfil = Convert.ToInt32(BD.ExecuteScalar(qPerfilCheck, new Dictionary<string, object> { { "@Id", idUser } }));

if (existePerfil == 0)
{
    string qPerfilInsert = @"
        INSERT INTO Perfil (Id_Usuario, FotoPerfil, Descripcion, AniosVigencia)
        VALUES (@U, '/img/perfil/default.png', 'Amante de los animales ❤️', 1)";
    BD.ExecuteNonQuery(qPerfilInsert, new Dictionary<string, object> { { "@U", idUser } });
}



        HttpContext.Session.SetInt32("UserId", idUser);

        return RedirectToAction("Registro2", "Registro");
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error al crear usuario rápido: " + ex.Message);
        TempData["Error"] = "Error al conectar con la base de datos.";
        return RedirectToAction("Registro1", "Registro");
    }
}[HttpGet]
public IActionResult Registro2(string modo = "", string origen = "")
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
    {
        TempData["Error"] = "Primero registrá un usuario 🐕‍🦺";
        return RedirectToAction("Registro1");
    }

    // 🧹 Si vuelve hacia atrás desde Registro3 o más, limpiar los datos previos
    if (Request.Query["volver"] == "true")
    {
        HttpContext.Session.Remove("MascotaNombre");
        HttpContext.Session.Remove("MascotaEspecie");
        HttpContext.Session.Remove("MascotaRaza");
        HttpContext.Session.Remove("MascotaSexo");
        HttpContext.Session.Remove("MascotaColor");
        HttpContext.Session.Remove("MascotaChip");
        HttpContext.Session.Remove("MascotaFoto");
        HttpContext.Session.Remove("MascotaEsterilizado");
        HttpContext.Session.Remove("MascotaPeso");
        HttpContext.Session.Remove("MascotaEdad");
        HttpContext.Session.Remove("MascotaPesoDisplay");

        Console.WriteLine("🔄 Datos de mascota limpiados al volver a Registro2");
    }

    // 👇❌ BORRÁ ESTA SEGUNDA LÍNEA DUPLICADA 👇
    // var userId = HttpContext.Session.GetInt32("UserId");

    if (!string.IsNullOrEmpty(origen))
        HttpContext.Session.SetString("OrigenRegistro", origen);

    ViewBag.Modo = modo;
    if (!string.IsNullOrEmpty(modo))
        HttpContext.Session.SetString("ModoRegistro", modo);

    ViewBag.Origen = origen;
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
        string displayWeight = "";
        try
        {
            string pesoInput = Request.Form["Peso"].ToString();
            var (peso, display) = PesoHelper.NormalizarPeso(pesoInput);
            pesoNormalizado = peso;
            displayWeight = display;

            if (!PesoHelper.ValidarPesoParaEspecie(pesoNormalizado, model.Especie))
            {
                ViewBag.Error = $"El peso ingresado es demasiado alto para un {model.Especie}";
                return View(model);
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"❌ Error al procesar peso: {ex.Message}");
            ViewBag.Error = "Error al procesar el peso ingresado";
            return View(model);
        }

        // Guardamos solo en sesión
        HttpContext.Session.SetString("MascotaNombre", model.Nombre ?? "MiMascota");
        HttpContext.Session.SetString("MascotaEspecie", model.Especie ?? "Desconocida");
        HttpContext.Session.SetString("MascotaRaza", model.Raza ?? "");
        HttpContext.Session.SetString("MascotaSexo", model.Sexo ?? "");
        HttpContext.Session.SetString("MascotaColor", model.Color ?? "");
        HttpContext.Session.SetString("MascotaChip", model.Chip ?? "");
        HttpContext.Session.SetString("MascotaFoto", model.Foto ?? "");
        HttpContext.Session.SetString("MascotaEsterilizado", model.Esterilizado.ToString());
        HttpContext.Session.SetString("MascotaPeso", pesoNormalizado.ToString("F2", CultureInfo.InvariantCulture));
        HttpContext.Session.SetString("MascotaPesoDisplay", displayWeight);
        HttpContext.Session.SetInt32("MascotaEdad", model.Edad);

        Console.WriteLine($"🚀 Registro2 completado parcialmente: {model.Nombre}, {model.Especie}, {model.Raza}, {pesoNormalizado}kg");

return RedirectToAction("Registro3", new { modo = "normal" });
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error en Registro2 POST: " + ex.Message);
        ViewBag.Error = "Ocurrió un error al registrar la mascota 🐾";
        return View(model);
    }
}[HttpGet]
public IActionResult Registro3(string modo = "")
{
    // 👇 esta es la única línea válida
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
        return RedirectToAction("Registro1");

    // 🧹 Limpieza si vuelve desde Registro4
    if (Request.Query["volver"] == "true")
    {
        HttpContext.Session.Remove("MascotaSexo");
        HttpContext.Session.Remove("MascotaRaza");
        HttpContext.Session.Remove("MascotaPeso");
        HttpContext.Session.Remove("MascotaPesoDisplay");
        HttpContext.Session.Remove("MascotaEdad");
        HttpContext.Session.Remove("MascotaFoto");

        Console.WriteLine("🔄 Datos de Registro3 limpiados al volver desde Registro4");
    }

    var nombre = HttpContext.Session.GetString("MascotaNombre");
    var especie = HttpContext.Session.GetString("MascotaEspecie");
    var raza = HttpContext.Session.GetString("MascotaRaza");
    var pesoStr = HttpContext.Session.GetString("MascotaPeso");
    var edadInt = HttpContext.Session.GetInt32("MascotaEdad") ?? 0;

    if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(especie))
    {
        TempData["Error"] = "Faltan datos de la mascota. Volvé a completar el paso anterior 🐾";
        return RedirectToAction("Registro2");
    }

    decimal.TryParse(pesoStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal peso);

    var mascota = new Mascota
    {
        Nombre = nombre,
        Especie = especie,
        Raza = raza,
        Peso = peso,
        Edad = edadInt
    };

    if (!string.IsNullOrEmpty(modo))
        HttpContext.Session.SetString("ModoRegistro", modo);

    string? origen = HttpContext.Session.GetString("OrigenRegistro");
    if (string.IsNullOrEmpty(origen))
        HttpContext.Session.SetString("OrigenRegistro", Request.Query["origen"].ToString() ?? "");

    ViewBag.MascotaNombre = nombre;
    ViewBag.MascotaEspecie = especie;
    ViewBag.MascotaRaza = raza;
    ViewBag.MascotaPeso = peso;
    ViewBag.MascotaPesoDisplay = HttpContext.Session.GetString("MascotaPesoDisplay");
    ViewBag.Modo = modo;

    return View(mascota);
}


[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Registro3Post(string Sexo, string Raza, decimal Peso, int Edad, string Foto, string modo = "")
{
    try
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
            return RedirectToAction("Registro1");

        // 🐶 Guardar la foto si existe
        if (!string.IsNullOrEmpty(Foto) && Foto.StartsWith("data:image"))
        {
            try
            {
                var base64 = Foto.Split(',')[1];
                var bytes = Convert.FromBase64String(base64);
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsPath))
                    Directory.CreateDirectory(uploadsPath);
                var fileName = $"mascota_{Guid.NewGuid()}.png";
                var filePath = Path.Combine(uploadsPath, fileName);
                System.IO.File.WriteAllBytes(filePath, bytes);
                HttpContext.Session.SetString("MascotaFoto", "/uploads/" + fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al guardar la foto: " + ex.Message);
                HttpContext.Session.SetString("MascotaFoto", "");
            }
        }

        // 🟢 Normalizar peso
        var (pesoNormalizado, pesoDisplay) = PesoHelper.NormalizarPeso(Peso.ToString());
        string especie = HttpContext.Session.GetString("MascotaEspecie") ?? "";
        if (!string.IsNullOrEmpty(especie) && !PesoHelper.ValidarPesoParaEspecie(pesoNormalizado, especie))
        {
            TempData["Error"] = $"El peso ingresado es demasiado alto para un {especie}";
            return RedirectToAction("Registro3");
        }

        // 🧠 Guardar en sesión
        HttpContext.Session.SetString("MascotaSexo", Sexo);
        HttpContext.Session.SetString("MascotaRaza", Raza);
        HttpContext.Session.SetString("MascotaPeso", pesoNormalizado.ToString("F2", CultureInfo.InvariantCulture));
        HttpContext.Session.SetString("MascotaPesoDisplay", pesoDisplay);
        HttpContext.Session.SetInt32("MascotaEdad", Edad);

        // 🧩 Definir modo final con fallback
        // 🔹 Definición robusta del modo final
string modoFinal = (modo ?? "").Trim().ToLower();
if (string.IsNullOrEmpty(modoFinal))
    modoFinal = HttpContext.Session.GetString("ModoRegistro")?.ToLower() ?? "normal";

Console.WriteLine($"🐾 Registro3Post → Modo final: {modoFinal}");

// 🔄 Lógica final
if (modoFinal == "nuevamascota")
{
    string nombre = HttpContext.Session.GetString("MascotaNombre") ?? "MiMascota";
    string especie = HttpContext.Session.GetString("MascotaEspecie") ?? "";
    string raza = HttpContext.Session.GetString("MascotaRaza") ?? "";
    string sexo = HttpContext.Session.GetString("MascotaSexo") ?? "";
    decimal.TryParse(HttpContext.Session.GetString("MascotaPeso"), out decimal peso);
    int edad = HttpContext.Session.GetInt32("MascotaEdad") ?? 0;
    string foto = HttpContext.Session.GetString("MascotaFoto") ?? "";
    string pesoDisplay = HttpContext.Session.GetString("MascotaPesoDisplay") ?? $"{peso} kg";

    string queryInsert = @"
        INSERT INTO Mascota (Id_User, Nombre, Especie, Raza, Sexo, Peso, Edad, Foto, Fecha_Nacimiento, PesoDisplay)
        VALUES (@U, @Nombre, @Especie, @Raza, @Sexo, @Peso, @Edad, @Foto, SYSDATETIME(), @PesoDisplay);
        SELECT SCOPE_IDENTITY();";

    var parametros = new Dictionary<string, object>
    {
        { "@U", userId.Value },
        { "@Nombre", nombre },
        { "@Especie", especie },
        { "@Raza", raza },
        { "@Sexo", sexo },
        { "@Peso", peso },
        { "@Edad", edad },
        { "@Foto", foto },
        { "@PesoDisplay", pesoDisplay }
    };

    object nuevaId = BD.ExecuteScalar(queryInsert, parametros);

    if (nuevaId != null && nuevaId != DBNull.Value)
        HttpContext.Session.SetInt32("MascotaId", Convert.ToInt32(nuevaId));

    TempData["Exito"] = $"Mascota {nombre} agregada correctamente 🐶";
    Console.WriteLine("➡️ Mascota insertada y redirigiendo a Configuración");
    return RedirectToAction("Configuracion", "Home");
}


// 🔹 Si no es modo nueva mascota, continuar el registro normal
Console.WriteLine("➡️ Redirigiendo a Registro4 (modo normal)");
return RedirectToAction("Registro4", "Registro");

    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error en Registro3Post: " + ex.Message);
        TempData["Error"] = "Error al guardar los datos.";
return RedirectToAction("Registro3", new { modo = "normal" });
    }
}


[HttpGet]
[Route("Registro/Registro4")]
public IActionResult Registro4(string modo = "")
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (userId == null)
    {
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
    if (HttpContext.Session.GetString("MascotaNombre") == null)
{
    TempData["Error"] = "Faltan datos de la mascota. Volvé a completar los pasos anteriores 🐾";
    return RedirectToAction("Registro2");
}
    try
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            TempData["Error"] = "Sesión expirada. Iniciá nuevamente.";
            return RedirectToAction("Registro1");
        }

        var modoFinal = !string.IsNullOrEmpty(modo)
            ? modo
            : (HttpContext.Session.GetString("ModoRegistro") ?? "").ToLower();

        if (modoFinal == "nuevamascota")
        {
            TempData["Exito"] = "Mascota agregada correctamente 🐶";
            return RedirectToAction("Configuracion", "Home");
        }

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
            ViewBag.Error = "Este correo ya está registrado 🐾. Iniciá sesión o usá otro.";
            ViewBag.Nombre = nombre;
            ViewBag.Apellido = apellido;
            ViewBag.Mail = mail;
            ViewBag.Modo = modo;
            return View("Registro4");
        }

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

        // Guardamos datos del usuario
        HttpContext.Session.SetString("UserPais", pais);
        HttpContext.Session.SetString("UserProvincia", provincia);
        HttpContext.Session.SetString("UserCiudad", ciudad);
        HttpContext.Session.SetString("UserTelefono", $"{codigoPais} {telefono}");

        string queryUser = @"
            UPDATE [User]
            SET Pais = @Pais,
                Provincia = @Provincia,
                Ciudad = @Ciudad,
                Telefono = @Telefono
            WHERE Id_User = @Id_User";

        BD.ExecuteNonQuery(queryUser, new Dictionary<string, object>
        {
            { "@Pais", pais },
            { "@Provincia", provincia },
            { "@Ciudad", ciudad },
            { "@Telefono", $"{codigoPais} {telefono}" },
            { "@Id_User", userId.Value }
        });

        // Datos finales de mascota desde sesión
        string nombre = HttpContext.Session.GetString("MascotaNombre") ?? "";
        string especie = HttpContext.Session.GetString("MascotaEspecie") ?? "";
        string raza = HttpContext.Session.GetString("MascotaRaza") ?? "";
        string sexo = HttpContext.Session.GetString("MascotaSexo") ?? "";
        decimal.TryParse(HttpContext.Session.GetString("MascotaPeso"), out decimal peso);
        int edad = HttpContext.Session.GetInt32("MascotaEdad") ?? 0;

        // Insert definitivo
        if (!string.IsNullOrEmpty(nombre) && !string.IsNullOrEmpty(especie))
        {
            string insert = @"
                INSERT INTO Mascota (Id_User, Nombre, Especie, Raza, Sexo, Peso, Edad, Fecha_Nacimiento, PesoDisplay)
                VALUES (@Id_User, @Nombre, @Especie, @Raza, @Sexo, @Peso, @Edad, SYSDATETIME(), @PesoDisplay)";

            BD.ExecuteNonQuery(insert, new Dictionary<string, object>
            {
                { "@Id_User", userId.Value },
                { "@Nombre", nombre },
                { "@Especie", especie },
                { "@Raza", raza },
                { "@Sexo", sexo },
                { "@Peso", peso },
                { "@PesoDisplay", HttpContext.Session.GetString("MascotaPesoDisplay") ?? (peso.ToString("F2") + " kg") },
                { "@Edad", edad }
            });
        }

        // Limpieza final de sesión
        HttpContext.Session.Remove("MascotaNombre");
        HttpContext.Session.Remove("MascotaEspecie");
        HttpContext.Session.Remove("MascotaRaza");
        HttpContext.Session.Remove("MascotaSexo");
        HttpContext.Session.Remove("MascotaPeso");
        HttpContext.Session.Remove("MascotaEdad");

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

[HttpGet]
[Route("Registro/VerificarMail")]
public JsonResult VerificarMail(string mail)
{
    try
    {
        string query = "SELECT COUNT(*) FROM Mail WHERE LOWER(Correo) = LOWER(@Correo)";
        var param = new Dictionary<string, object> { { "@Correo", mail.Trim().ToLower() } };
        int existe = Convert.ToInt32(BD.ExecuteScalar(query, param));

        return Json(new { existe = existe > 0 });
    }
    catch (Exception ex)
    {
        Console.WriteLine("❌ Error en VerificarMail: " + ex.Message);
        return Json(new { existe = false });
    }
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

        // Normalizar el peso
        string pesoInput = Request.Form["Peso"].ToString();
        var (pesoNormalizado, pesoDisplay) = PesoHelper.NormalizarPeso(pesoInput);

        // Validar peso para la especie
        if (!PesoHelper.ValidarPesoParaEspecie(pesoNormalizado, model.Especie))
        {
            TempData["Error"] = $"El peso ingresado es demasiado alto para un {model.Especie}";
            return RedirectToAction("Configuracion", "Home");
        }

        // Asignar valores por defecto si no se proporcionan
        string tagColor = model.TagColor ?? "#39b77c";  // Color predeterminado
        bool estado = model.Estado;  // Por defecto ya está en true en el modelo Mascota (activo)

        // Insertar la mascota
        string query = @"
            INSERT INTO Mascota 
            (Id_User, Nombre, Especie, Edad, Raza, Sexo, Peso, Color, Chip, Foto, Esterilizado, Estado, TagColor, Fecha_Nacimiento)
            VALUES 
            (@Id_User, @Nombre, @Especie, @Edad, @Raza, @Sexo, @Peso, @Color, @Chip, @Foto, @Esterilizado, @Estado, @TagColor, SYSDATETIME());";

        var parametros = new Dictionary<string, object>
        {
            { "@Id_User", userId.Value },
            { "@Nombre", model.Nombre ?? "MiMascota" },
            { "@Especie", model.Especie },
            { "@Edad", model.Edad },
            { "@Raza", model.Raza ?? "" },
            { "@Sexo", model.Sexo ?? "" },
            { "@Peso", pesoNormalizado },
            { "@PesoDisplay", pesoDisplay },
            { "@Color", model.Color ?? "" },
            { "@Chip", model.Chip ?? "" },
            { "@Foto", model.Foto ?? "" },
            { "@Esterilizado", model.Esterilizado },
            { "@Estado", estado },  // Estado se establece en 'true' si no se pasa un valor
            { "@TagColor", tagColor }  // Asignación de color del tag
        };

        // Ejecutar la consulta de inserción
        BD.ExecuteNonQuery(query, parametros);

        // Mensaje de éxito
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