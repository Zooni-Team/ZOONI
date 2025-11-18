using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zooni.Controllers
{
    public class ProveedorController : Controller
    {
        // ============================
        // GET: /Proveedor/Registro1
        // ============================
        [HttpGet]
        [Route("Proveedor/Registro1")]
        public IActionResult Registro1()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                // Verificar si ya es proveedor (solo si la tabla existe)
                try
                {
                    string checkQuery = @"
                        IF OBJECT_ID('ProveedorServicio', 'U') IS NOT NULL
                            SELECT COUNT(*) FROM ProveedorServicio WHERE Id_User = @UserId
                        ELSE
                            SELECT 0";
                    object? existeResult = BD.ExecuteScalar(checkQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
                    int existe = existeResult != null && existeResult != DBNull.Value ? Convert.ToInt32(existeResult) : 0;
                    
                    if (existe > 0)
                    {
                        return RedirectToAction("Dashboard", "Proveedor");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ Error al verificar proveedor (tabla puede no existir): " + ex.Message);
                    // Continuar con el registro si la tabla no existe
                }
            }
            return View();
        }

        // ============================
        // POST: /Proveedor/Registro1
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Proveedor/Registro1")]
        public IActionResult Registro1(string correo, string contrasena, string confirmarContrasena)
        {
            try
            {
                if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena) || string.IsNullOrEmpty(confirmarContrasena))
                {
                    ViewBag.Error = "Completá todos los campos.";
                    return View();
                }

                if (contrasena != confirmarContrasena)
                {
                    ViewBag.Error = "Las contraseñas no coinciden.";
                    return View();
                }

                if (contrasena.Length < 6)
                {
                    ViewBag.Error = "La contraseña debe tener al menos 6 caracteres.";
                    return View();
                }

                // Solo verificar si el correo ya existe (NO crear usuario todavía)
                string checkQuery = @"
                    SELECT TOP 1 U.Id_User, U.Id_TipoUsuario 
                    FROM [User] U 
                    INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail 
                    WHERE M.Correo = @Correo";
                
                var checkParams = new Dictionary<string, object> { { "@Correo", correo } };
                DataTable dt = BD.ExecuteQuery(checkQuery, checkParams);

                if (dt.Rows.Count > 0)
                {
                    // Verificar si es dueño de mascota (no proveedor)
                    int esDueño = 0;
                    try
                    {
                        string esDueñoQuery = @"
                            IF OBJECT_ID('ProveedorServicio', 'U') IS NOT NULL
                                SELECT COUNT(*) FROM [User] U 
                                WHERE U.Id_User = @UserId 
                                AND NOT EXISTS (SELECT 1 FROM ProveedorServicio P WHERE P.Id_User = U.Id_User)
                            ELSE
                                SELECT COUNT(*) FROM [User] U WHERE U.Id_User = @UserId";
                        object? esDueñoResult = BD.ExecuteScalar(esDueñoQuery, new Dictionary<string, object> { { "@UserId", Convert.ToInt32(dt.Rows[0]["Id_User"]) } });
                        esDueño = esDueñoResult != null && esDueñoResult != DBNull.Value ? Convert.ToInt32(esDueñoResult) : 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("⚠️ Error al verificar si es dueño (tabla puede no existir): " + ex.Message);
                        esDueño = 1; // Asumir que es dueño si hay error
                    }
                    
                    if (esDueño > 0)
                    {
                        ViewBag.Error = "Este correo ya está registrado como dueño de mascota. Usá otro correo o iniciá sesión.";
                        return View();
                    }
                    
                    // Usuario existe, verificar si ya es proveedor
                    int idUser = Convert.ToInt32(dt.Rows[0]["Id_User"]);
                    
                    // Verificar contraseña
                    string passQuery = @"
                        SELECT M.Contrasena FROM Mail M 
                        INNER JOIN [User] U ON U.Id_Mail = M.Id_Mail 
                        WHERE U.Id_User = @UserId";
                    object? passResult = BD.ExecuteScalar(passQuery, new Dictionary<string, object> { { "@UserId", idUser } });
                    string? passActual = passResult != null && passResult != DBNull.Value ? passResult.ToString() : null;
                    
                    if (passActual != contrasena)
                    {
                        ViewBag.Error = "Contraseña incorrecta.";
                        return View();
                    }

                    // Verificar si ya es proveedor
                    int esProveedor = 0;
                    try
                    {
                        string proveedorQuery = @"
                            IF OBJECT_ID('ProveedorServicio', 'U') IS NOT NULL
                                SELECT COUNT(*) FROM ProveedorServicio WHERE Id_User = @UserId
                            ELSE
                                SELECT 0";
                        object? proveedorResult = BD.ExecuteScalar(proveedorQuery, new Dictionary<string, object> { { "@UserId", idUser } });
                        esProveedor = proveedorResult != null && proveedorResult != DBNull.Value ? Convert.ToInt32(proveedorResult) : 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("⚠️ Error al verificar proveedor (tabla puede no existir): " + ex.Message);
                        esProveedor = 0;
                    }
                    
                    if (esProveedor > 0)
                    {
                        // Ya es proveedor, iniciar sesión
                        HttpContext.Session.SetInt32("UserId", idUser);
                        HttpContext.Session.SetString("EsProveedor", "true");
                        return RedirectToAction("Dashboard", "Proveedor");
                    }
                    
                    // Usuario existe pero no es proveedor, guardar ID para completar registro
                    HttpContext.Session.SetInt32("UserIdExistente", idUser);
                }

                // Guardar correo y contraseña en sesión (NO crear usuario todavía)
                HttpContext.Session.SetString("ProveedorCorreo", correo);
                HttpContext.Session.SetString("ProveedorContrasena", contrasena);

                return RedirectToAction("Registro2", "Proveedor");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Proveedor/Registro1 POST: " + ex.Message);
                ViewBag.Error = "Error al procesar el registro. Intentalo de nuevo.";
                return View();
            }
        }

        // ============================
        // GET: /Proveedor/Registro2
        // ============================
        [HttpGet]
        [Route("Proveedor/Registro2")]
        public IActionResult Registro2()
        {
            // Verificar que haya correo y contraseña en sesión
            string? correo = HttpContext.Session.GetString("ProveedorCorreo");
            if (string.IsNullOrEmpty(correo))
            {
                TempData["Error"] = "Sesión expirada. Volvé a iniciar.";
                return RedirectToAction("Registro1", "Proveedor");
            }

            // Cargar tipos de servicio disponibles (priorizar Paseador y Cuidador)
            try
            {
                string tiposQuery = @"
                    SELECT Id_TipoServicio, Descripcion 
                    FROM TipoServicio 
                    ORDER BY 
                        CASE 
                            WHEN Descripcion = 'Paseador' THEN 1
                            WHEN Descripcion = 'Cuidador' THEN 2
                            ELSE 3
                        END,
                        Descripcion";
                DataTable tiposDt = BD.ExecuteQuery(tiposQuery);
                ViewBag.TiposServicio = tiposDt;
                
                if (tiposDt.Rows.Count == 0)
                {
                    ViewBag.Error = "No hay tipos de servicio disponibles. Por favor, contactá al administrador.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error al cargar tipos de servicio: " + ex.Message);
                ViewBag.Error = "Error al cargar los tipos de servicio. Intentalo de nuevo.";
                ViewBag.TiposServicio = new DataTable();
            }

            return View();
        }

        // ============================
        // POST: /Proveedor/Registro2
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Proveedor/Registro2")]
        public IActionResult Registro2(string dni, string nombreCompleto, int experiencia, string? descripcion)
        {
            try
            {
                // Verificar que haya correo y contraseña en sesión
                string? correo = HttpContext.Session.GetString("ProveedorCorreo");
                if (string.IsNullOrEmpty(correo))
                {
                    TempData["Error"] = "Sesión expirada.";
                    return RedirectToAction("Registro1", "Proveedor");
                }

                if (string.IsNullOrWhiteSpace(dni) || string.IsNullOrWhiteSpace(nombreCompleto))
                {
                    ViewBag.Error = "Completá todos los campos obligatorios.";
                    string tiposQuery = "SELECT Id_TipoServicio, Descripcion FROM TipoServicio ORDER BY Descripcion";
                    DataTable tiposDt = BD.ExecuteQuery(tiposQuery);
                    ViewBag.TiposServicio = tiposDt;
                    return View();
                }

                // Obtener tipos de servicio del formulario
                var tiposServicioForm = Request.Form["tiposServicio"];
                if (tiposServicioForm.Count == 0)
                {
                    ViewBag.Error = "Seleccioná al menos un tipo de servicio.";
                    string tiposQuery = @"
                        SELECT Id_TipoServicio, Descripcion 
                        FROM TipoServicio 
                        ORDER BY 
                            CASE 
                                WHEN Descripcion = 'Paseador' THEN 1
                                WHEN Descripcion = 'Cuidador' THEN 2
                                ELSE 3
                            END,
                            Descripcion";
                    DataTable tiposDt = BD.ExecuteQuery(tiposQuery);
                    ViewBag.TiposServicio = tiposDt;
                    return View();
                }

                // Obtener especies del formulario
                var especiesForm = Request.Form["especies"];
                if (especiesForm.Count == 0)
                {
                    ViewBag.Error = "Seleccioná al menos una especie.";
                    string tiposQuery = "SELECT Id_TipoServicio, Descripcion FROM TipoServicio ORDER BY Descripcion";
                    DataTable tiposDt = BD.ExecuteQuery(tiposQuery);
                    ViewBag.TiposServicio = tiposDt;
                    return View();
                }

                // Verificar si el DNI ya existe
                int dniExiste = 0;
                try
                {
                    string dniCheckQuery = @"
                        IF OBJECT_ID('ProveedorServicio', 'U') IS NOT NULL
                            SELECT COUNT(*) FROM ProveedorServicio WHERE DNI = @DNI
                        ELSE
                            SELECT 0";
                    object? dniResult = BD.ExecuteScalar(dniCheckQuery, new Dictionary<string, object> { { "@DNI", dni } });
                    dniExiste = dniResult != null && dniResult != DBNull.Value ? Convert.ToInt32(dniResult) : 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ Error al verificar DNI (tabla puede no existir): " + ex.Message);
                    dniExiste = 0;
                }
                
                if (dniExiste > 0)
                {
                    ViewBag.Error = "Este DNI ya está registrado.";
                    string tiposQuery = "SELECT Id_TipoServicio, Descripcion FROM TipoServicio ORDER BY Descripcion";
                    DataTable tiposDt = BD.ExecuteQuery(tiposQuery);
                    ViewBag.TiposServicio = tiposDt;
                    return View();
                }

                // Guardar en sesión
                HttpContext.Session.SetString("ProveedorDNI", dni);
                HttpContext.Session.SetString("ProveedorNombreCompleto", nombreCompleto);
                HttpContext.Session.SetInt32("ProveedorExperiencia", experiencia);
                HttpContext.Session.SetString("ProveedorDescripcion", descripcion ?? "");
                
                // Guardar listas como strings separados por comas
                List<string> tiposServicioList = new List<string>();
                foreach (var tipo in tiposServicioForm)
                {
                    tiposServicioList.Add(tipo.ToString());
                }
                
                List<string> especiesList = new List<string>();
                foreach (var especie in especiesForm)
                {
                    especiesList.Add(especie.ToString());
                }
                
                HttpContext.Session.SetString("ProveedorTiposServicio", string.Join(",", tiposServicioList));
                HttpContext.Session.SetString("ProveedorEspecies", string.Join(",", especiesList));
                
                // Guardar el tipo de servicio principal (Paseador o Cuidador) para determinar la interfaz
                string tipoPrincipal = "";
                foreach (var tipoIdStr in tiposServicioList)
                {
                    if (int.TryParse(tipoIdStr, out int tipoId))
                    {
                        string tipoQuery = "SELECT Descripcion FROM TipoServicio WHERE Id_TipoServicio = @Id";
                        object? tipoResult = BD.ExecuteScalar(tipoQuery, new Dictionary<string, object> { { "@Id", tipoId } });
                        string? tipoDesc = tipoResult?.ToString();
                        
                        if (tipoDesc == "Paseador" || tipoDesc == "Cuidador")
                        {
                            tipoPrincipal = tipoDesc;
                            break; // Priorizar Paseador o Cuidador
                        }
                    }
                }
                HttpContext.Session.SetString("ProveedorTipoPrincipal", tipoPrincipal);

                return RedirectToAction("Registro3", "Proveedor");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Proveedor/Registro2 POST: " + ex.Message);
                ViewBag.Error = "Error al guardar los datos.";
                string tiposQuery = "SELECT Id_TipoServicio, Descripcion FROM TipoServicio ORDER BY Descripcion";
                DataTable tiposDt = BD.ExecuteQuery(tiposQuery);
                ViewBag.TiposServicio = tiposDt;
                return View();
            }
        }

        // ============================
        // GET: /Proveedor/Registro3
        // ============================
        [HttpGet]
        [Route("Proveedor/Registro3")]
        public IActionResult Registro3()
        {
            // Verificar que haya datos en sesión
            string? correo = HttpContext.Session.GetString("ProveedorCorreo");
            string? dni = HttpContext.Session.GetString("ProveedorDNI");
            if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(dni))
            {
                TempData["Error"] = "Sesión expirada.";
                return RedirectToAction("Registro1", "Proveedor");
            }

            return View();
        }

        // ============================
        // POST: /Proveedor/Registro3
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Proveedor/Registro3")]
        public IActionResult Registro3(string? telefono, string? direccion, string? ciudad, 
            string? provincia, string? pais, decimal? precioHora)
        {
            try
            {
                // Obtener datos de sesión
                string? correo = HttpContext.Session.GetString("ProveedorCorreo");
                string? contrasena = HttpContext.Session.GetString("ProveedorContrasena");
                string dni = HttpContext.Session.GetString("ProveedorDNI") ?? "";
                string nombreCompleto = HttpContext.Session.GetString("ProveedorNombreCompleto") ?? "";
                int experiencia = HttpContext.Session.GetInt32("ProveedorExperiencia") ?? 0;
                string descripcion = HttpContext.Session.GetString("ProveedorDescripcion") ?? "";
                string tiposServicioStr = HttpContext.Session.GetString("ProveedorTiposServicio") ?? "";
                string especiesStr = HttpContext.Session.GetString("ProveedorEspecies") ?? "";

                if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
                {
                    TempData["Error"] = "Sesión expirada. Volvé a iniciar.";
                    return RedirectToAction("Registro1", "Proveedor");
                }

                if (string.IsNullOrEmpty(dni) || string.IsNullOrEmpty(nombreCompleto))
                {
                    TempData["Error"] = "Faltan datos del paso anterior.";
                    return RedirectToAction("Registro2", "Proveedor");
                }

                // Verificar que la tabla existe antes de insertar
                string checkTableQuery = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ProveedorServicio'";
                object? tableExists = BD.ExecuteScalar(checkTableQuery);
                int tableExistsInt = tableExists != null && tableExists != DBNull.Value ? Convert.ToInt32(tableExists) : 0;
                
                if (tableExistsInt == 0)
                {
                    TempData["Error"] = "La tabla de proveedores no existe. Por favor, ejecutá el script SQL 'CreateTablesProveedorServicio.sql' en la base de datos primero.";
                    return View();
                }

                // ✅ CREAR USUARIO AL FINAL (solo si no existe)
                int userId;
                int? userIdExistente = HttpContext.Session.GetInt32("UserIdExistente");
                
                if (userIdExistente.HasValue)
                {
                    // Usuario ya existe, usar ese ID
                    userId = userIdExistente.Value;
                }
                else
                {
                    // Crear nuevo Mail
                    string queryMail = @"
                        INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion)
                        VALUES (@Correo, @Contrasena, SYSDATETIME());
                        SELECT SCOPE_IDENTITY();";

                    var mailParams = new Dictionary<string, object>
                    {
                        { "@Correo", correo },
                        { "@Contrasena", contrasena }
                    };

                    object? mailResult = BD.ExecuteScalar(queryMail, mailParams);
                    if (mailResult == null || mailResult == DBNull.Value)
                    {
                        TempData["Error"] = "Error al crear la cuenta. Intentalo de nuevo.";
                        return View();
                    }
                    int idMail = Convert.ToInt32(mailResult);

                    // Obtener ID de tipo Proveedor
                    string tipoProveedorQuery = "SELECT Id_TipoUsuario FROM TipoUsuario WHERE Descripcion = 'Proveedor'";
                    object? tipoProveedorId = BD.ExecuteScalar(tipoProveedorQuery);
                    int idTipoProveedor = tipoProveedorId != null && tipoProveedorId != DBNull.Value ? Convert.ToInt32(tipoProveedorId) : 1;

                    // Dividir nombre completo
                    string[] nombres = nombreCompleto.Split(' ', 2);
                    string nombre = nombres[0];
                    string apellido = nombres.Length > 1 ? nombres[1] : "";

                    // Crear nuevo User
                    string queryUser = @"
                        INSERT INTO [User] (Id_Mail, Nombre, Apellido, Fecha_Registro, Id_Ubicacion, Id_TipoUsuario, Estado)
                        VALUES (@Id_Mail, @Nombre, @Apellido, SYSDATETIME(), 1, @Id_TipoUsuario, 1);
                        SELECT SCOPE_IDENTITY();";

                    var userParams = new Dictionary<string, object> 
                    { 
                        { "@Id_Mail", idMail },
                        { "@Nombre", nombre },
                        { "@Apellido", apellido },
                        { "@Id_TipoUsuario", idTipoProveedor }
                    };
                    object? userResult = BD.ExecuteScalar(queryUser, userParams);
                    if (userResult == null || userResult == DBNull.Value)
                    {
                        TempData["Error"] = "Error al crear el usuario. Intentalo de nuevo.";
                        return View();
                    }
                    userId = Convert.ToInt32(userResult);
                }

                // Crear proveedor
                string insertProveedor = @"
                    INSERT INTO ProveedorServicio 
                    (Id_User, DNI, NombreCompleto, Experiencia_Anios, Descripcion, 
                     Telefono, Direccion, Ciudad, Provincia, Pais, Precio_Hora, 
                     Estado, Fecha_Registro)
                    VALUES 
                    (@Id_User, @DNI, @NombreCompleto, @Experiencia, @Descripcion,
                     @Telefono, @Direccion, @Ciudad, @Provincia, @Pais, @PrecioHora,
                     1, SYSDATETIME());
                    SELECT SCOPE_IDENTITY();";

                var proveedorParams = new Dictionary<string, object>
                {
                    { "@Id_User", userId },
                    { "@DNI", dni },
                    { "@NombreCompleto", nombreCompleto },
                    { "@Experiencia", experiencia },
                    { "@Descripcion", descripcion },
                    { "@Telefono", telefono ?? (object)DBNull.Value },
                    { "@Direccion", direccion ?? (object)DBNull.Value },
                    { "@Ciudad", ciudad ?? (object)DBNull.Value },
                    { "@Provincia", provincia ?? (object)DBNull.Value },
                    { "@Pais", pais ?? (object)DBNull.Value },
                    { "@PrecioHora", precioHora ?? (object)DBNull.Value }
                };

                object? proveedorIdResult = BD.ExecuteScalar(insertProveedor, proveedorParams);
                if (proveedorIdResult == null || proveedorIdResult == DBNull.Value)
                {
                    TempData["Error"] = "Error al crear el proveedor.";
                    return View();
                }
                int idProveedor = Convert.ToInt32(proveedorIdResult);

                // Insertar tipos de servicio
                string[] tiposServicioArray = tiposServicioStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                List<int> tiposServicio = new List<int>();
                foreach (string tipoStr in tiposServicioArray)
                {
                    if (int.TryParse(tipoStr, out int tipoId))
                    {
                        tiposServicio.Add(tipoId);
                    }
                }
                
                foreach (int tipoId in tiposServicio)
                {
                    string insertTipo = @"
                        INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
                        VALUES (@Id_Proveedor, @Id_TipoServicio)";
                    BD.ExecuteNonQuery(insertTipo, new Dictionary<string, object>
                    {
                        { "@Id_Proveedor", idProveedor },
                        { "@Id_TipoServicio", tipoId }
                    });
                }

                // Insertar especies
                string[] especiesArray = especiesStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                List<string> especies = new List<string>(especiesArray);
                
                foreach (string especie in especies)
                {
                    string insertEspecie = @"
                        INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie)
                        VALUES (@Id_Proveedor, @Especie)";
                    BD.ExecuteNonQuery(insertEspecie, new Dictionary<string, object>
                    {
                        { "@Id_Proveedor", idProveedor },
                        { "@Especie", especie }
                    });
                }

                // Actualizar nombre del usuario si ya existía
                if (userIdExistente.HasValue)
                {
                    string[] nombres = nombreCompleto.Split(' ', 2);
                    string nombre = nombres[0];
                    string apellido = nombres.Length > 1 ? nombres[1] : "";

                    string updateUser = @"
                        UPDATE [User] 
                        SET Nombre = @Nombre, Apellido = @Apellido
                        WHERE Id_User = @Id_User";
                    BD.ExecuteNonQuery(updateUser, new Dictionary<string, object>
                    {
                        { "@Nombre", nombre },
                        { "@Apellido", apellido },
                        { "@Id_User", userId }
                    });
                }

                // Establecer sesión de usuario
                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("EsProveedor", "true");

                // Limpiar sesión de registro
                HttpContext.Session.Remove("ProveedorCorreo");
                HttpContext.Session.Remove("ProveedorContrasena");
                HttpContext.Session.Remove("UserIdExistente");
                HttpContext.Session.Remove("ProveedorDNI");
                HttpContext.Session.Remove("ProveedorNombreCompleto");
                HttpContext.Session.Remove("ProveedorExperiencia");
                HttpContext.Session.Remove("ProveedorDescripcion");
                HttpContext.Session.Remove("ProveedorTiposServicio");
                HttpContext.Session.Remove("ProveedorEspecies");

                TempData["Exito"] = "¡Registro completado exitosamente!";
                return RedirectToAction("Dashboard", "Proveedor");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Proveedor/Registro3 POST: " + ex.Message);
                TempData["Error"] = "Error al completar el registro.";
                return View();
            }
        }

        // ============================
        // GET: /Proveedor/Dashboard
        // ============================
        [HttpGet]
        [Route("Proveedor/Dashboard")]
        public IActionResult Dashboard()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar que sea proveedor
            DataTable dt = new DataTable();
            try
            {
                string proveedorQuery = @"
                    IF OBJECT_ID('ProveedorServicio', 'U') IS NOT NULL
                        SELECT P.*, U.Nombre, U.Apellido, M.Correo
                        FROM ProveedorServicio P
                        INNER JOIN [User] U ON P.Id_User = U.Id_User
                        INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                        WHERE P.Id_User = @UserId";
                
                dt = BD.ExecuteQuery(proveedorQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al verificar proveedor (tabla puede no existir): " + ex.Message);
            }
            
            if (dt.Rows.Count == 0)
            {
                return RedirectToAction("Registro1", "Proveedor");
            }

            // Obtener el tipo de servicio principal (Paseador o Cuidador)
            int idProveedor = Convert.ToInt32(dt.Rows[0]["Id_Proveedor"]);
            string tipoPrincipal = "";
            
            try
            {
                string tipoQuery = @"
                    SELECT TOP 1 TS.Descripcion
                    FROM ProveedorServicio_TipoServicio PSTS
                    INNER JOIN TipoServicio TS ON PSTS.Id_TipoServicio = TS.Id_TipoServicio
                    WHERE PSTS.Id_Proveedor = @IdProveedor
                      AND TS.Descripcion IN ('Paseador', 'Cuidador')
                    ORDER BY 
                        CASE 
                            WHEN TS.Descripcion = 'Paseador' THEN 1
                            WHEN TS.Descripcion = 'Cuidador' THEN 2
                        END";
                
                object? tipoResult = BD.ExecuteScalar(tipoQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                tipoPrincipal = tipoResult?.ToString() ?? "";
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al obtener tipo de servicio principal: " + ex.Message);
            }
            
            // Guardar en sesión para uso futuro
            if (!string.IsNullOrEmpty(tipoPrincipal))
            {
                HttpContext.Session.SetString("ProveedorTipoPrincipal", tipoPrincipal);
            }
            else
            {
                // Si no se encontró, usar el de sesión o establecer por defecto
                tipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";
            }

            ViewBag.Proveedor = dt.Rows[0];
            ViewBag.TipoPrincipal = tipoPrincipal; // "Paseador" o "Cuidador"
            return View();
        }
    }
}

