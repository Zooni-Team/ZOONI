using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using Zooni.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zooni.Controllers
{
    public class ProveedorController : BaseController
    {
        // ============================
        // Método para crear tablas automáticamente
        // ============================
        private void AsegurarTablasProveedores()
        {
            try
            {
                // Verificar y crear TipoUsuario "Proveedor"
                string checkTipoUsuario = "SELECT COUNT(*) FROM TipoUsuario WHERE Descripcion = 'Proveedor'";
                object? countTipoUsuario = BD.ExecuteScalar(checkTipoUsuario);
                if (countTipoUsuario == null || Convert.ToInt32(countTipoUsuario) == 0)
                {
                    BD.ExecuteNonQuery("INSERT INTO TipoUsuario (Descripcion) VALUES ('Proveedor')");
                }

                // Verificar y crear tabla ProveedorServicio
                string checkTable = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ProveedorServicio'";
                object? tableExists = BD.ExecuteScalar(checkTable);
                if (tableExists == null || Convert.ToInt32(tableExists) == 0)
                {
                    string createTable = @"
                        CREATE TABLE [dbo].[ProveedorServicio](
                            [Id_Proveedor] [int] IDENTITY(1,1) NOT NULL,
                            [Id_User] [int] NOT NULL,
                            [DNI] [nvarchar](20) NOT NULL,
                            [NombreCompleto] [nvarchar](200) NOT NULL,
                            [Experiencia_Anios] [int] NOT NULL DEFAULT 0,
                            [Descripcion] [nvarchar](1000) NULL,
                            [FotoPerfil] [nvarchar](500) NULL,
                            [Telefono] [nvarchar](30) NULL,
                            [Direccion] [nvarchar](200) NULL,
                            [Ciudad] [nvarchar](100) NULL,
                            [Provincia] [nvarchar](100) NULL,
                            [Pais] [nvarchar](100) NULL,
                            [Precio_Hora] [decimal](12, 2) NULL,
                            [Calificacion_Promedio] [decimal](4, 2) NULL DEFAULT 0,
                            [Cantidad_Resenas] [int] NOT NULL DEFAULT 0,
                            [Estado] [bit] NOT NULL DEFAULT 1,
                            [Fecha_Registro] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                            [Verificado] [bit] NOT NULL DEFAULT 0,
                            [Latitud] [decimal](10, 8) NULL,
                            [Longitud] [decimal](11, 8) NULL,
                            [Radio_Atencion_Km] [decimal](10, 2) NULL DEFAULT 5.00,
                            [Tipo_Ubicacion] [nvarchar](20) NULL DEFAULT 'Cobertura',
                            CONSTRAINT [PK_ProveedorServicio] PRIMARY KEY CLUSTERED ([Id_Proveedor] ASC),
                            CONSTRAINT [UQ_ProveedorServicio_User] UNIQUE NONCLUSTERED ([Id_User] ASC),
                            CONSTRAINT [UQ_ProveedorServicio_DNI] UNIQUE NONCLUSTERED ([DNI] ASC)
                        )";
                    BD.ExecuteNonQuery(createTable);

                    // Agregar Foreign Key si existe la tabla User
                    try
                    {
                        string addFK = @"
                            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ProveedorServicio_User')
                            BEGIN
                                ALTER TABLE [dbo].[ProveedorServicio] WITH CHECK ADD CONSTRAINT [FK_ProveedorServicio_User] 
                                FOREIGN KEY([Id_User]) REFERENCES [dbo].[User] ([Id_User]) ON DELETE CASCADE
                                ALTER TABLE [dbo].[ProveedorServicio] CHECK CONSTRAINT [FK_ProveedorServicio_User]
                            END";
                        BD.ExecuteNonQuery(addFK);
                    }
                    catch { }
                }

                // Verificar y agregar columnas de ubicación si no existen
                try
                {
                    string checkLat = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ProveedorServicio]') AND name = 'Latitud'";
                    object? latExists = BD.ExecuteScalar(checkLat);
                    if (latExists == null || Convert.ToInt32(latExists) == 0)
                    {
                        BD.ExecuteNonQuery("ALTER TABLE [dbo].[ProveedorServicio] ADD [Latitud] [decimal](10, 8) NULL");
                        BD.ExecuteNonQuery("ALTER TABLE [dbo].[ProveedorServicio] ADD [Longitud] [decimal](11, 8) NULL");
                        BD.ExecuteNonQuery("ALTER TABLE [dbo].[ProveedorServicio] ADD [Radio_Atencion_Km] [decimal](10, 2) NULL DEFAULT 5.00");
                        BD.ExecuteNonQuery("ALTER TABLE [dbo].[ProveedorServicio] ADD [Tipo_Ubicacion] [nvarchar](20) NULL DEFAULT 'Cobertura'");
                    }
                }
                catch { }

                // Verificar y crear tabla ProveedorServicio_TipoServicio
                string checkTableTipos = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ProveedorServicio_TipoServicio'";
                object? tableTiposExists = BD.ExecuteScalar(checkTableTipos);
                if (tableTiposExists == null || Convert.ToInt32(tableTiposExists) == 0)
                {
                    string createTableTipos = @"
                        CREATE TABLE [dbo].[ProveedorServicio_TipoServicio](
                            [Id] [int] IDENTITY(1,1) NOT NULL,
                            [Id_Proveedor] [int] NOT NULL,
                            [Id_TipoServicio] [int] NOT NULL,
                            CONSTRAINT [PK_ProveedorServicio_TipoServicio] PRIMARY KEY CLUSTERED ([Id] ASC),
                            CONSTRAINT [UQ_Proveedor_TipoServicio] UNIQUE NONCLUSTERED ([Id_Proveedor] ASC, [Id_TipoServicio] ASC)
                        )";
                    BD.ExecuteNonQuery(createTableTipos);

                    try
                    {
                        BD.ExecuteNonQuery(@"
                            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Proveedor_TipoServicio_Proveedor')
                            BEGIN
                                ALTER TABLE [dbo].[ProveedorServicio_TipoServicio] WITH CHECK ADD CONSTRAINT [FK_Proveedor_TipoServicio_Proveedor] 
                                FOREIGN KEY([Id_Proveedor]) REFERENCES [dbo].[ProveedorServicio] ([Id_Proveedor]) ON DELETE CASCADE
                                ALTER TABLE [dbo].[ProveedorServicio_TipoServicio] CHECK CONSTRAINT [FK_Proveedor_TipoServicio_Proveedor]
                            END");
                        BD.ExecuteNonQuery(@"
                            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Proveedor_TipoServicio_TipoServicio')
                            BEGIN
                                ALTER TABLE [dbo].[ProveedorServicio_TipoServicio] WITH CHECK ADD CONSTRAINT [FK_Proveedor_TipoServicio_TipoServicio] 
                                FOREIGN KEY([Id_TipoServicio]) REFERENCES [dbo].[TipoServicio] ([Id_TipoServicio])
                                ALTER TABLE [dbo].[ProveedorServicio_TipoServicio] CHECK CONSTRAINT [FK_Proveedor_TipoServicio_TipoServicio]
                            END");
                    }
                    catch { }
                }

                // Verificar y crear tabla ProveedorServicio_Especie
                string checkTableEspecies = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ProveedorServicio_Especie'";
                object? tableEspeciesExists = BD.ExecuteScalar(checkTableEspecies);
                if (tableEspeciesExists == null || Convert.ToInt32(tableEspeciesExists) == 0)
                {
                    string createTableEspecies = @"
                        CREATE TABLE [dbo].[ProveedorServicio_Especie](
                            [Id] [int] IDENTITY(1,1) NOT NULL,
                            [Id_Proveedor] [int] NOT NULL,
                            [Especie] [nvarchar](50) NOT NULL,
                            CONSTRAINT [PK_ProveedorServicio_Especie] PRIMARY KEY CLUSTERED ([Id] ASC),
                            CONSTRAINT [UQ_Proveedor_Especie] UNIQUE NONCLUSTERED ([Id_Proveedor] ASC, [Especie] ASC)
                        )";
                    BD.ExecuteNonQuery(createTableEspecies);

                    try
                    {
                        BD.ExecuteNonQuery(@"
                            IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Proveedor_Especie_Proveedor')
                            BEGIN
                                ALTER TABLE [dbo].[ProveedorServicio_Especie] WITH CHECK ADD CONSTRAINT [FK_Proveedor_Especie_Proveedor] 
                                FOREIGN KEY([Id_Proveedor]) REFERENCES [dbo].[ProveedorServicio] ([Id_Proveedor]) ON DELETE CASCADE
                                ALTER TABLE [dbo].[ProveedorServicio_Especie] CHECK CONSTRAINT [FK_Proveedor_Especie_Proveedor]
                            END");
                    }
                    catch { }
                }

                // Verificar y crear tabla ReservaProveedor
                string checkTableReserva = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ReservaProveedor'";
                object? tableReservaExists = BD.ExecuteScalar(checkTableReserva);
                if (tableReservaExists == null || Convert.ToInt32(tableReservaExists) == 0)
                {
                    string createTableReserva = @"
                        CREATE TABLE [dbo].[ReservaProveedor](
                            [Id_Reserva] [int] IDENTITY(1,1) NOT NULL,
                            [Id_User] [int] NOT NULL,
                            [Id_Proveedor] [int] NOT NULL,
                            [Id_Mascota] [int] NOT NULL,
                            [Id_TipoServicio] [int] NOT NULL,
                            [Fecha_Inicio] [datetime2](7) NOT NULL,
                            [Fecha_Fin] [datetime2](7) NULL,
                            [Hora_Inicio] [time](0) NOT NULL,
                            [Hora_Fin] [time](0) NULL,
                            [Duracion_Horas] [decimal](5,2) NULL,
                            [Precio_Total] [decimal](12, 2) NOT NULL,
                            [Id_EstadoReserva] [int] NOT NULL DEFAULT 1,
                            [Notas] [nvarchar](1000) NULL,
                            [Direccion_Servicio] [nvarchar](500) NULL,
                            [Latitud_Servicio] [decimal](10, 8) NULL,
                            [Longitud_Servicio] [decimal](11, 8) NULL,
                            [Compartir_Ubicacion] [bit] NOT NULL DEFAULT 0,
                            [Fecha_Creacion] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT [PK_ReservaProveedor] PRIMARY KEY CLUSTERED ([Id_Reserva] ASC)
                        )";
                    BD.ExecuteNonQuery(createTableReserva);
                }

                // Agregar columnas adicionales a ReservaProveedor si no existen
                try
                {
                    string checkDistancia = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ReservaProveedor]') AND name = 'Distancia_Total_Metros'";
                    object? distExists = BD.ExecuteScalar(checkDistancia);
                    if (distExists == null || Convert.ToInt32(distExists) == 0)
                    {
                        BD.ExecuteNonQuery("ALTER TABLE [dbo].[ReservaProveedor] ADD [Distancia_Total_Metros] [decimal](10,2) NULL");
                        BD.ExecuteNonQuery("ALTER TABLE [dbo].[ReservaProveedor] ADD [Tiempo_Total_Segundos] [int] NULL");
                        BD.ExecuteNonQuery("ALTER TABLE [dbo].[ReservaProveedor] ADD [Ruta_GPS_JSON] [nvarchar](max) NULL");
                        BD.ExecuteNonQuery("ALTER TABLE [dbo].[ReservaProveedor] ADD [Fecha_Hora_Inicio_Real] [datetime2](7) NULL");
                        BD.ExecuteNonQuery("ALTER TABLE [dbo].[ReservaProveedor] ADD [Fecha_Hora_Fin_Real] [datetime2](7) NULL");
                    }
                }
                catch { }

                // Verificar y crear tabla UbicacionServicio
                string checkTableUbicacion = "SELECT COUNT(*) FROM sys.tables WHERE name = 'UbicacionServicio'";
                object? tableUbicacionExists = BD.ExecuteScalar(checkTableUbicacion);
                if (tableUbicacionExists == null || Convert.ToInt32(tableUbicacionExists) == 0)
                {
                    string createTableUbicacion = @"
                        CREATE TABLE [dbo].[UbicacionServicio](
                            [Id] [int] IDENTITY(1,1) NOT NULL,
                            [Id_Reserva] [int] NOT NULL,
                            [Id_Proveedor] [int] NOT NULL,
                            [Latitud] [decimal](10, 8) NOT NULL,
                            [Longitud] [decimal](11, 8) NOT NULL,
                            [Fecha_Hora] [datetime2](7) NOT NULL DEFAULT GETDATE(),
                            [Tipo] [nvarchar](20) NOT NULL DEFAULT 'Proveedor',
                            [Distancia_Acumulada_Metros] [decimal](10,2) NULL DEFAULT 0,
                            [Tiempo_Transcurrido_Segundos] [int] NULL DEFAULT 0,
                            CONSTRAINT [PK_UbicacionServicio] PRIMARY KEY CLUSTERED ([Id] ASC)
                        )";
                    BD.ExecuteNonQuery(createTableUbicacion);
                }

                // Agregar columna Id_Proveedor a Resena si no existe
                try
                {
                    string checkResenaProv = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Resena]') AND name = 'Id_Proveedor'";
                    object? resenaProvExists = BD.ExecuteScalar(checkResenaProv);
                    if (resenaProvExists == null || Convert.ToInt32(resenaProvExists) == 0)
                    {
                        BD.ExecuteNonQuery("ALTER TABLE [dbo].[Resena] ADD [Id_Proveedor] [int] NULL");
                    }
                }
                catch { }

                Console.WriteLine("✅ Tablas de proveedores verificadas/creadas correctamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al crear tablas de proveedores: " + ex.Message);
            }
        }
        // ============================
        // GET: /Proveedor/Registro1
        // ============================
        [HttpGet]
        [Route("Proveedor/Registro1")]
        public IActionResult Registro1()
        {
            AsegurarTablasProveedores();
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
                // Intentar buscar primero con correo encriptado (si los datos están encriptados)
                string correoNormalizado = correo.ToLower().Trim();
                string correoEncrypted = EncryptionHelper.Encrypt(correoNormalizado);
                
                string checkQuery = @"
                    SELECT TOP 1 U.Id_User, U.Id_TipoUsuario, M.Correo, M.Contrasena
                    FROM [User] U 
                    INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail 
                    WHERE M.Correo = @Correo";
                
                var checkParams = new Dictionary<string, object> { { "@Correo", correoEncrypted } };
                DataTable dt = BD.ExecuteQuery(checkQuery, checkParams);
                
                // Si no encontró con correo encriptado, intentar buscar todos y comparar desencriptando
                if (dt.Rows.Count == 0)
                {
                    Console.WriteLine("⚠️ No se encontró con correo encriptado en Registro1, intentando buscar desencriptando...");
                    string queryAll = @"
                        SELECT U.Id_User, U.Id_TipoUsuario, M.Correo, M.Contrasena
                        FROM [User] U 
                        INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail";
                    
                    DataTable dtAll = BD.ExecuteQuery(queryAll, new Dictionary<string, object>());
                    
                    foreach (DataRow row in dtAll.Rows)
                    {
                        try
                        {
                            string correoStored = row["Correo"].ToString() ?? "";
                            string correoDesencriptado = EncryptionHelper.Decrypt(correoStored);
                            
                            // Comparar correos
                            if (correoDesencriptado.ToLower().Trim() == correoNormalizado || 
                                (correoDesencriptado == correoStored && correoStored.ToLower().Trim() == correoNormalizado))
                            {
                                // Crear un DataTable con solo esta fila
                                dt = dtAll.Clone();
                                dt.ImportRow(row);
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"⚠️ Error al desencriptar correo en Registro1: {ex.Message}");
                            // Si falla la desencriptación, comparar directamente (puede que no esté encriptado)
                            if (row["Correo"].ToString()?.ToLower().Trim() == correoNormalizado)
                            {
                                dt = dtAll.Clone();
                                dt.ImportRow(row);
                                break;
                            }
                        }
                    }
                }

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
                    
                    // Verificar contraseña usando PasswordHelper (puede estar hasheada)
                    string? passActual = dt.Rows[0]["Contrasena"]?.ToString();
                    
                    if (string.IsNullOrEmpty(passActual))
                    {
                        ViewBag.Error = "Error al verificar la contraseña.";
                        return View();
                    }
                    
                    // Intentar verificar con PasswordHelper primero (si está hasheada)
                    bool passwordCorrecta = PasswordHelper.VerifyPassword(contrasena, passActual);
                    
                    // Si no funciona con hash, comparar directamente (para contraseñas antiguas sin hash)
                    if (!passwordCorrecta && passActual == contrasena)
                    {
                        passwordCorrecta = true;
                    }
                    
                    if (!passwordCorrecta)
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
            AsegurarTablasProveedores();
            // Verificar que haya correo y contraseña en sesión
            string? correo = HttpContext.Session.GetString("ProveedorCorreo");
            if (string.IsNullOrEmpty(correo))
            {
                TempData["Error"] = "Sesión expirada. Volvé a iniciar.";
                return RedirectToAction("Registro1", "Proveedor");
            }

            // Asegurar que existan Paseador y Cuidador
            try
            {
                // Insertar Paseador si no existe
                string checkPaseador = "SELECT COUNT(*) FROM TipoServicio WHERE Descripcion = 'Paseador'";
                object? countPaseador = BD.ExecuteScalar(checkPaseador);
                if (countPaseador == null || Convert.ToInt32(countPaseador) == 0)
                {
                    string insertPaseador = "INSERT INTO TipoServicio (Descripcion) VALUES ('Paseador')";
                    BD.ExecuteNonQuery(insertPaseador);
                    Console.WriteLine("✅ Tipo de servicio 'Paseador' creado");
                }

                // Insertar Cuidador si no existe
                string checkCuidador = "SELECT COUNT(*) FROM TipoServicio WHERE Descripcion = 'Cuidador'";
                object? countCuidador = BD.ExecuteScalar(checkCuidador);
                if (countCuidador == null || Convert.ToInt32(countCuidador) == 0)
                {
                    string insertCuidador = "INSERT INTO TipoServicio (Descripcion) VALUES ('Cuidador')";
                    BD.ExecuteNonQuery(insertCuidador);
                    Console.WriteLine("✅ Tipo de servicio 'Cuidador' creado");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al verificar/crear tipos de servicio: " + ex.Message);
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
            AsegurarTablasProveedores();
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

                // Asegurar que las tablas existan
                AsegurarTablasProveedores();

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
                    // Crear nuevo Mail - Encriptar correo y hashear contraseña
                    string correoEncrypted = EncryptionHelper.Encrypt(correo.ToLower().Trim());
                    string contrasenaHashed = PasswordHelper.HashPassword(contrasena);
                    
                    string queryMail = @"
                        INSERT INTO Mail (Correo, Contrasena, Fecha_Creacion)
                        VALUES (@Correo, @Contrasena, SYSDATETIME());
                        SELECT SCOPE_IDENTITY();";

                    var mailParams = new Dictionary<string, object>
                    {
                        { "@Correo", correoEncrypted },
                        { "@Contrasena", contrasenaHashed }
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

                    // Dividir nombre completo y encriptar
                    string[] nombres = nombreCompleto.Split(' ', 2);
                    string nombre = nombres[0];
                    string apellido = nombres.Length > 1 ? nombres[1] : "";
                    string nombreEncrypted = EncryptionHelper.Encrypt(nombre);
                    string apellidoEncrypted = EncryptionHelper.Encrypt(apellido);

                    // Crear nuevo User
                    string queryUser = @"
                        INSERT INTO [User] (Id_Mail, Nombre, Apellido, Fecha_Registro, Id_Ubicacion, Id_TipoUsuario, Estado)
                        VALUES (@Id_Mail, @Nombre, @Apellido, SYSDATETIME(), 1, @Id_TipoUsuario, 1);
                        SELECT SCOPE_IDENTITY();";

                    var userParams = new Dictionary<string, object> 
                    { 
                        { "@Id_Mail", idMail },
                        { "@Nombre", nombreEncrypted },
                        { "@Apellido", apellidoEncrypted },
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

                // Crear proveedor - Encriptar datos sensibles
                string dniEncrypted = EncryptionHelper.Encrypt(dni);
                string nombreCompletoEncrypted = EncryptionHelper.Encrypt(nombreCompleto);
                string telefonoEncrypted = telefono != null ? EncryptionHelper.Encrypt(telefono) : null;
                
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
                    { "@DNI", dniEncrypted },
                    { "@NombreCompleto", nombreCompletoEncrypted },
                    { "@Experiencia", experiencia },
                    { "@Descripcion", descripcion },
                    { "@Telefono", telefonoEncrypted ?? (object)DBNull.Value },
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
            AsegurarTablasProveedores();
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
                    SELECT P.*, U.Nombre, U.Apellido, M.Correo
                    FROM ProveedorServicio P
                    INNER JOIN [User] U ON P.Id_User = U.Id_User
                    INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                    WHERE P.Id_User = @UserId";
                
                dt = BD.ExecuteQuery(proveedorQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al verificar proveedor: " + ex.Message);
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
            
            // Redirigir a dashboard personalizado según el tipo
            if (tipoPrincipal == "Paseador")
            {
                return RedirectToAction("DashboardPaseador", "Proveedor");
            }
            else if (tipoPrincipal == "Cuidador")
            {
                return RedirectToAction("DashboardCuidador", "Proveedor");
            }
            
            return View();
        }

        // ============================
        // GET: /Proveedor/DashboardPaseador
        // ============================
        [HttpGet]
        [Route("Proveedor/DashboardPaseador")]
        public IActionResult DashboardPaseador()
        {
            AsegurarTablasProveedores();
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
                Console.WriteLine("⚠️ Error al verificar proveedor: " + ex.Message);
            }
            
            if (dt.Rows.Count == 0)
            {
                return RedirectToAction("Registro1", "Proveedor");
            }

            int idProveedor = Convert.ToInt32(dt.Rows[0]["Id_Proveedor"]);
            ViewBag.Proveedor = dt.Rows[0];
            ViewBag.TipoPrincipal = "Paseador";

            // Obtener estadísticas de reservas
            try
            {
                string statsQuery = @"
                    SELECT 
                        COUNT(CASE WHEN Id_EstadoReserva = 1 THEN 1 END) AS Pendientes,
                        COUNT(CASE WHEN Id_EstadoReserva = 2 THEN 1 END) AS Confirmadas,
                        COUNT(CASE WHEN Id_EstadoReserva = 3 THEN 1 END) AS EnCurso,
                        COUNT(CASE WHEN Id_EstadoReserva = 4 THEN 1 END) AS Completadas,
                        COUNT(*) AS Total
                    FROM ReservaProveedor
                    WHERE Id_Proveedor = @IdProveedor";
                
                DataTable statsDt = BD.ExecuteQuery(statsQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                if (statsDt.Rows.Count > 0)
                {
                    ViewBag.ReservasPendientes = statsDt.Rows[0]["Pendientes"] ?? 0;
                    ViewBag.ReservasConfirmadas = statsDt.Rows[0]["Confirmadas"] ?? 0;
                    ViewBag.ReservasEnCurso = statsDt.Rows[0]["EnCurso"] ?? 0;
                    ViewBag.ReservasCompletadas = statsDt.Rows[0]["Completadas"] ?? 0;
                    ViewBag.TotalReservas = statsDt.Rows[0]["Total"] ?? 0;
                }

                // Obtener reservas próximas (próximas 5)
                string proximasQuery = @"
                    SELECT TOP 5
                        RP.Id_Reserva,
                        RP.Fecha_Inicio,
                        RP.Hora_Inicio,
                        M.Nombre AS MascotaNombre,
                        M.Foto AS MascotaFoto,
                        U.Nombre + ' ' + U.Apellido AS DuenioNombre,
                        ER.Descripcion AS Estado,
                        TS.Descripcion AS TipoServicio
                    FROM ReservaProveedor RP
                    INNER JOIN Mascota M ON RP.Id_Mascota = M.Id_Mascota
                    INNER JOIN [User] U ON RP.Id_User = U.Id_User
                    INNER JOIN EstadoReserva ER ON RP.Id_EstadoReserva = ER.Id_EstadoReserva
                    INNER JOIN TipoServicio TS ON RP.Id_TipoServicio = TS.Id_TipoServicio
                    WHERE RP.Id_Proveedor = @IdProveedor
                      AND RP.Id_EstadoReserva IN (1, 2, 3)
                      AND RP.Fecha_Inicio >= CAST(GETDATE() AS DATE)
                    ORDER BY RP.Fecha_Inicio ASC, RP.Hora_Inicio ASC";
                
                DataTable proximasDt = BD.ExecuteQuery(proximasQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                ViewBag.ProximasReservas = proximasDt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al obtener estadísticas: " + ex.Message);
                ViewBag.ReservasPendientes = 0;
                ViewBag.ReservasConfirmadas = 0;
                ViewBag.ReservasEnCurso = 0;
                ViewBag.ReservasCompletadas = 0;
                ViewBag.TotalReservas = 0;
                ViewBag.ProximasReservas = new DataTable();
            }

            return View();
        }

        // ============================
        // GET: /Proveedor/DashboardCuidador
        // ============================
        [HttpGet]
        [Route("Proveedor/DashboardCuidador")]
        public IActionResult DashboardCuidador()
        {
            AsegurarTablasProveedores();
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar que sea proveedor, si no, redirigir
            if (!EsProveedor())
            {
                return RedirectToAction("Index", "Home");
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
                Console.WriteLine("⚠️ Error al verificar proveedor: " + ex.Message);
            }
            
            if (dt.Rows.Count == 0)
            {
                return RedirectToAction("Registro1", "Proveedor");
            }

            ViewBag.Proveedor = dt.Rows[0];
            ViewBag.TipoPrincipal = "Cuidador";
            return View();
        }

        // ============================
        // GET: /Proveedor/Configuracion
        // ============================
        [HttpGet]
        [Route("Proveedor/Configuracion")]
        public IActionResult Configuracion()
        {
            AsegurarTablasProveedores();
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar que sea proveedor, si no, redirigir
            if (!EsProveedor())
            {
                return RedirectToAction("Index", "Home");
            }

            // Obtener datos del proveedor
            DataTable dt = new DataTable();
            try
            {
                string proveedorQuery = @"
                    SELECT P.*, U.Nombre, U.Apellido, M.Correo
                    FROM ProveedorServicio P
                    INNER JOIN [User] U ON P.Id_User = U.Id_User
                    INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                    WHERE P.Id_User = @UserId";
                
                dt = BD.ExecuteQuery(proveedorQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al obtener proveedor: " + ex.Message);
            }
            
            if (dt.Rows.Count == 0)
            {
                return RedirectToAction("Registro1", "Proveedor");
            }

            // Obtener tipos de servicio del proveedor
            int idProveedor = Convert.ToInt32(dt.Rows[0]["Id_Proveedor"]);
            string tiposQuery = @"
                SELECT TS.Id_TipoServicio, TS.Descripcion
                FROM ProveedorServicio_TipoServicio PST
                INNER JOIN TipoServicio TS ON PST.Id_TipoServicio = TS.Id_TipoServicio
                WHERE PST.Id_Proveedor = @IdProveedor";
            DataTable tiposDt = BD.ExecuteQuery(tiposQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });

            // Asegurar que existan Paseador y Cuidador
            try
            {
                // Insertar Paseador si no existe
                string checkPaseador = "SELECT COUNT(*) FROM TipoServicio WHERE Descripcion = 'Paseador'";
                object? countPaseador = BD.ExecuteScalar(checkPaseador);
                if (countPaseador == null || Convert.ToInt32(countPaseador) == 0)
                {
                    string insertPaseador = "INSERT INTO TipoServicio (Descripcion) VALUES ('Paseador')";
                    BD.ExecuteNonQuery(insertPaseador);
                    Console.WriteLine("✅ Tipo de servicio 'Paseador' creado");
                }

                // Insertar Cuidador si no existe
                string checkCuidador = "SELECT COUNT(*) FROM TipoServicio WHERE Descripcion = 'Cuidador'";
                object? countCuidador = BD.ExecuteScalar(checkCuidador);
                if (countCuidador == null || Convert.ToInt32(countCuidador) == 0)
                {
                    string insertCuidador = "INSERT INTO TipoServicio (Descripcion) VALUES ('Cuidador')";
                    BD.ExecuteNonQuery(insertCuidador);
                    Console.WriteLine("✅ Tipo de servicio 'Cuidador' creado");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al verificar/crear tipos de servicio: " + ex.Message);
            }

            // Obtener todos los tipos de servicio disponibles
            string todosTiposQuery = @"
                SELECT Id_TipoServicio, Descripcion 
                FROM TipoServicio 
                ORDER BY 
                    CASE 
                        WHEN Descripcion = 'Paseador' THEN 1
                        WHEN Descripcion = 'Cuidador' THEN 2
                        ELSE 3
                    END,
                    Descripcion";
            DataTable todosTiposDt = BD.ExecuteQuery(todosTiposQuery);

            // Obtener especies del proveedor
            string especiesQuery = @"
                SELECT Especie
                FROM ProveedorServicio_Especie
                WHERE Id_Proveedor = @IdProveedor";
            DataTable especiesDt = BD.ExecuteQuery(especiesQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });

            // Obtener tipo principal
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
                tipoPrincipal = tipoResult?.ToString() ?? HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";
                HttpContext.Session.SetString("ProveedorTipoPrincipal", tipoPrincipal);
            }
            catch (Exception ex)
            {
                Console.WriteLine("⚠️ Error al obtener tipo principal: " + ex.Message);
                tipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";
            }

            ViewBag.Proveedor = dt.Rows[0];
            ViewBag.TiposServicio = tiposDt;
            ViewBag.TodosTiposServicio = todosTiposDt;
            ViewBag.Especies = especiesDt;
            ViewBag.TipoPrincipal = tipoPrincipal;
            return View();
        }

        // ============================
        // POST: /Proveedor/Configuracion
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Proveedor/Configuracion")]
        public IActionResult Configuracion(string dni, string nombreCompleto, int experiencia, string? descripcion,
            string? telefono, string? direccion, string? ciudad, string? provincia, string? pais, decimal? precioHora,
            decimal? latitud, decimal? longitud, decimal? radioAtencion, IFormFile? fotoPerfil)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    return RedirectToAction("Login", "Auth");
                }

                // Verificar que sea proveedor, si no, redirigir
                if (!EsProveedor())
                {
                    return RedirectToAction("Index", "Home");
                }

                // Obtener ID del proveedor
                string proveedorQuery = "SELECT Id_Proveedor FROM ProveedorServicio WHERE Id_User = @UserId";
                object? proveedorIdResult = BD.ExecuteScalar(proveedorQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
                if (proveedorIdResult == null || proveedorIdResult == DBNull.Value)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction("Configuracion", "Proveedor");
                }
                int idProveedor = Convert.ToInt32(proveedorIdResult);

                // Determinar tipo de ubicación según el tipo principal
                string tipoUbicacion = "Cobertura"; // Por defecto para paseadores
                string tipoPrincipal = "";
                
                // Obtener tipo principal actual
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
                    
                    if (tipoPrincipal == "Cuidador")
                    {
                        tipoUbicacion = "Precisa";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ Error al obtener tipo principal: " + ex.Message);
                }

                // Manejar foto de perfil si se subió
                string fotoPerfilPath = null;
                if (fotoPerfil != null && fotoPerfil.Length > 0)
                {
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "proveedores");
                    if (!Directory.Exists(uploadsPath))
                        Directory.CreateDirectory(uploadsPath);

                    var fileName = $"proveedor_{idProveedor}_{Guid.NewGuid()}{Path.GetExtension(fotoPerfil.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        fotoPerfil.CopyTo(stream);
                    }
                    
                    fotoPerfilPath = $"/uploads/proveedores/{fileName}";
                }

                // Encriptar datos sensibles antes de actualizar
                string dniEncrypted = EncryptionHelper.Encrypt(dni);
                string nombreCompletoEncrypted = EncryptionHelper.Encrypt(nombreCompleto);
                string telefonoEncrypted = telefono != null ? EncryptionHelper.Encrypt(telefono) : null;
                
                // Actualizar datos del proveedor
                string updateQuery = @"
                    UPDATE ProveedorServicio 
                    SET DNI = @DNI,
                        NombreCompleto = @NombreCompleto,
                        Experiencia_Anios = @Experiencia,
                        Descripcion = @Descripcion,
                        Telefono = @Telefono,
                        Direccion = @Direccion,
                        Ciudad = @Ciudad,
                        Provincia = @Provincia,
                        Pais = @Pais,
                        Precio_Hora = @PrecioHora,
                        Latitud = @Latitud,
                        Longitud = @Longitud,
                        Radio_Atencion_Km = @RadioAtencion,
                        Tipo_Ubicacion = @TipoUbicacion" + 
                        (fotoPerfilPath != null ? ", FotoPerfil = @FotoPerfil" : "") + @"
                    WHERE Id_Proveedor = @IdProveedor";

                var updateParams = new Dictionary<string, object>
                {
                    { "@DNI", dniEncrypted },
                    { "@NombreCompleto", nombreCompletoEncrypted },
                    { "@Experiencia", experiencia },
                    { "@Descripcion", descripcion ?? (object)DBNull.Value },
                    { "@Telefono", telefonoEncrypted ?? (object)DBNull.Value },
                    { "@Direccion", direccion ?? (object)DBNull.Value },
                    { "@Ciudad", ciudad ?? (object)DBNull.Value },
                    { "@Provincia", provincia ?? (object)DBNull.Value },
                    { "@Pais", pais ?? (object)DBNull.Value },
                    { "@PrecioHora", precioHora ?? (object)DBNull.Value },
                    { "@Latitud", latitud ?? (object)DBNull.Value },
                    { "@Longitud", longitud ?? (object)DBNull.Value },
                    { "@RadioAtencion", radioAtencion ?? (object)DBNull.Value },
                    { "@TipoUbicacion", tipoUbicacion },
                    { "@IdProveedor", idProveedor }
                };

                if (fotoPerfilPath != null)
                {
                    updateParams.Add("@FotoPerfil", fotoPerfilPath);
                }

                BD.ExecuteNonQuery(updateQuery, updateParams);

                // Actualizar tipos de servicio
                var tiposServicioForm = Request.Form["tiposServicio"];
                tipoPrincipal = ""; // Reutilizar variable existente
                if (tiposServicioForm.Count > 0)
                {
                    // Eliminar tipos actuales
                    string deleteTiposQuery = "DELETE FROM ProveedorServicio_TipoServicio WHERE Id_Proveedor = @IdProveedor";
                    BD.ExecuteNonQuery(deleteTiposQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });

                    // Insertar nuevos tipos y determinar tipo principal
                    foreach (var tipoIdStr in tiposServicioForm)
                    {
                        if (int.TryParse(tipoIdStr, out int tipoId))
                        {
                            string insertTipo = @"
                                INSERT INTO ProveedorServicio_TipoServicio (Id_Proveedor, Id_TipoServicio)
                                VALUES (@IdProveedor, @IdTipoServicio)";
                            BD.ExecuteNonQuery(insertTipo, new Dictionary<string, object>
                            {
                                { "@IdProveedor", idProveedor },
                                { "@IdTipoServicio", tipoId }
                            });

                            // Determinar tipo principal (Paseador o Cuidador)
                            if (string.IsNullOrEmpty(tipoPrincipal))
                            {
                                string tipoQuery = "SELECT Descripcion FROM TipoServicio WHERE Id_TipoServicio = @Id";
                                object? tipoResult = BD.ExecuteScalar(tipoQuery, new Dictionary<string, object> { { "@Id", tipoId } });
                                string? tipoDesc = tipoResult?.ToString();
                                
                                if (tipoDesc == "Paseador" || tipoDesc == "Cuidador")
                                {
                                    tipoPrincipal = tipoDesc;
                                }
                            }
                        }
                    }
                    
                    // Actualizar tipo principal en sesión
                    if (!string.IsNullOrEmpty(tipoPrincipal))
                    {
                        HttpContext.Session.SetString("ProveedorTipoPrincipal", tipoPrincipal);
                    }
                }

                // Actualizar especies
                var especiesForm = Request.Form["especies"];
                if (especiesForm.Count > 0)
                {
                    // Eliminar especies actuales
                    string deleteEspeciesQuery = "DELETE FROM ProveedorServicio_Especie WHERE Id_Proveedor = @IdProveedor";
                    BD.ExecuteNonQuery(deleteEspeciesQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });

                    // Insertar nuevas especies
                    foreach (var especie in especiesForm)
                    {
                        string insertEspecie = @"
                            INSERT INTO ProveedorServicio_Especie (Id_Proveedor, Especie)
                            VALUES (@IdProveedor, @Especie)";
                        BD.ExecuteNonQuery(insertEspecie, new Dictionary<string, object>
                        {
                            { "@IdProveedor", idProveedor },
                            { "@Especie", especie.ToString() }
                        });
                    }
                }

                // Actualizar nombre del usuario
                string[] nombres = nombreCompleto.Split(' ', 2);
                string nombre = nombres[0];
                string apellido = nombres.Length > 1 ? nombres[1] : "";

                string updateUser = @"
                    UPDATE [User] 
                    SET Nombre = @Nombre, Apellido = @Apellido
                    WHERE Id_User = @UserId";
                BD.ExecuteNonQuery(updateUser, new Dictionary<string, object>
                {
                    { "@Nombre", nombre },
                    { "@Apellido", apellido },
                    { "@UserId", userId.Value }
                });

                TempData["Exito"] = "Configuración actualizada exitosamente.";
                
                // Redirigir al dashboard correcto según el tipo principal
                if (tipoPrincipal == "Paseador")
                {
                    return RedirectToAction("DashboardPaseador", "Proveedor");
                }
                else if (tipoPrincipal == "Cuidador")
                {
                    return RedirectToAction("DashboardCuidador", "Proveedor");
                }
                
                return RedirectToAction("Configuracion", "Proveedor");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Proveedor/Configuracion POST: " + ex.Message);
                TempData["Error"] = "Error al actualizar la configuración.";
                return RedirectToAction("Configuracion", "Proveedor");
            }
        }

        // ============================
        // GET: /Proveedor/Perfil
        // ============================
        [HttpGet]
        [Route("Proveedor/Perfil")]
        public IActionResult Perfil()
        {
            AsegurarTablasProveedores();
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar que sea proveedor, si no, redirigir
            if (!EsProveedor())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                string proveedorQuery = @"
                    SELECT P.*, U.Nombre, U.Apellido, M.Correo
                    FROM ProveedorServicio P
                    INNER JOIN [User] U ON P.Id_User = U.Id_User
                    INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                    WHERE P.Id_User = @UserId";
                
                DataTable proveedorDt = BD.ExecuteQuery(proveedorQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
                if (proveedorDt.Rows.Count == 0)
                {
                    return RedirectToAction("Registro1", "Proveedor");
                }

                ViewBag.Proveedor = proveedorDt.Rows[0];
                int idProveedor = Convert.ToInt32(proveedorDt.Rows[0]["Id_Proveedor"]);

                // Obtener tipos de servicio
                string tiposQuery = @"
                    SELECT TS.Descripcion
                    FROM ProveedorServicio_TipoServicio PSTS
                    INNER JOIN TipoServicio TS ON PSTS.Id_TipoServicio = TS.Id_TipoServicio
                    WHERE PSTS.Id_Proveedor = @IdProveedor";
                DataTable tiposDt = BD.ExecuteQuery(tiposQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                ViewBag.TiposServicio = tiposDt;

                // Obtener especies
                string especiesQuery = @"
                    SELECT Especie
                    FROM ProveedorServicio_Especie
                    WHERE Id_Proveedor = @IdProveedor";
                DataTable especiesDt = BD.ExecuteQuery(especiesQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                ViewBag.Especies = especiesDt;

                // Obtener reseñas
                string resenasQuery = @"
                    SELECT R.*, U.Nombre + ' ' + U.Apellido AS NombreUsuario, M.Nombre AS NombreMascota
                    FROM Resena R
                    LEFT JOIN [User] U ON R.Id_Usuario = U.Id_User
                    LEFT JOIN Mascota M ON R.Id_Mascota = M.Id_Mascota
                    WHERE R.Id_Proveedor = @IdProveedor
                    ORDER BY R.Fecha DESC";
                DataTable resenasDt = BD.ExecuteQuery(resenasQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                ViewBag.Resenas = resenasDt;

                ViewBag.Tema = HttpContext.Session.GetString("Tema") ?? "claro";
                ViewBag.TipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Proveedor/Perfil: " + ex.Message);
                TempData["Error"] = "Error al cargar el perfil.";
                string tipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";
                if (tipoPrincipal == "Paseador")
                    return RedirectToAction("DashboardPaseador", "Proveedor");
                else if (tipoPrincipal == "Cuidador")
                    return RedirectToAction("DashboardCuidador", "Proveedor");
                return RedirectToAction("Dashboard", "Proveedor");
            }
        }

        // ============================
        // GET: /Proveedor/Reservas
        // ============================
        [HttpGet]
        [Route("Proveedor/Reservas")]
        public IActionResult Reservas()
        {
            AsegurarTablasProveedores();
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar que sea proveedor, si no, redirigir
            if (!EsProveedor())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                string proveedorQuery = @"
                    SELECT Id_Proveedor
                    FROM ProveedorServicio
                    WHERE Id_User = @UserId";
                
                DataTable proveedorDt = BD.ExecuteQuery(proveedorQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
                if (proveedorDt.Rows.Count == 0)
                {
                    return RedirectToAction("Registro1", "Proveedor");
                }

                int idProveedor = Convert.ToInt32(proveedorDt.Rows[0]["Id_Proveedor"]);

                string reservasQuery = @"
                    SELECT 
                        RP.Id_Reserva,
                        RP.Fecha_Inicio,
                        RP.Hora_Inicio,
                        RP.Precio_Total,
                        M.Nombre AS MascotaNombre,
                        M.Foto AS MascotaFoto,
                        M.Especie AS MascotaEspecie,
                        U.Nombre + ' ' + U.Apellido AS DuenioNombre,
                        ER.Descripcion AS Estado,
                        ER.Id_EstadoReserva,
                        TS.Descripcion AS TipoServicio,
                        RP.Notas
                    FROM ReservaProveedor RP
                    INNER JOIN Mascota M ON RP.Id_Mascota = M.Id_Mascota
                    INNER JOIN [User] U ON RP.Id_User = U.Id_User
                    INNER JOIN EstadoReserva ER ON RP.Id_EstadoReserva = ER.Id_EstadoReserva
                    INNER JOIN TipoServicio TS ON RP.Id_TipoServicio = TS.Id_TipoServicio
                    WHERE RP.Id_Proveedor = @IdProveedor
                    ORDER BY RP.Fecha_Inicio DESC, RP.Hora_Inicio DESC";
                
                DataTable reservasDt = BD.ExecuteQuery(reservasQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                ViewBag.Reservas = reservasDt;
                ViewBag.TipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Proveedor/Reservas: " + ex.Message);
                TempData["Error"] = "Error al cargar las reservas.";
                string tipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";
                if (tipoPrincipal == "Paseador")
                    return RedirectToAction("DashboardPaseador", "Proveedor");
                else if (tipoPrincipal == "Cuidador")
                    return RedirectToAction("DashboardCuidador", "Proveedor");
                return RedirectToAction("Dashboard", "Proveedor");
            }
        }

        // ============================
        // GET: /Proveedor/Resenas
        // ============================
        [HttpGet]
        [Route("Proveedor/Resenas")]
        public IActionResult Resenas()
        {
            AsegurarTablasProveedores();
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar que sea proveedor, si no, redirigir
            if (!EsProveedor())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                string proveedorQuery = @"
                    SELECT Id_Proveedor
                    FROM ProveedorServicio
                    WHERE Id_User = @UserId";
                
                DataTable proveedorDt = BD.ExecuteQuery(proveedorQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
                if (proveedorDt.Rows.Count == 0)
                {
                    return RedirectToAction("Registro1", "Proveedor");
                }

                int idProveedor = Convert.ToInt32(proveedorDt.Rows[0]["Id_Proveedor"]);

                string resenasQuery = @"
                    SELECT 
                        R.Id_Resena,
                        R.Calificacion,
                        R.Comentario,
                        R.Fecha,
                        U.Nombre + ' ' + U.Apellido AS UsuarioNombre,
                        M.Nombre AS MascotaNombre
                    FROM Resena R
                    INNER JOIN [User] U ON R.Id_User = U.Id_User
                    LEFT JOIN ReservaProveedor RP ON R.Id_Reserva = RP.Id_Reserva
                    LEFT JOIN Mascota M ON RP.Id_Mascota = M.Id_Mascota
                    WHERE R.Id_Proveedor = @IdProveedor
                    ORDER BY R.Fecha DESC";
                
                DataTable resenasDt = BD.ExecuteQuery(resenasQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                ViewBag.Resenas = resenasDt;
                ViewBag.TipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Proveedor/Resenas: " + ex.Message);
                TempData["Error"] = "Error al cargar las reseñas.";
                string tipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";
                if (tipoPrincipal == "Paseador")
                    return RedirectToAction("DashboardPaseador", "Proveedor");
                else if (tipoPrincipal == "Cuidador")
                    return RedirectToAction("DashboardCuidador", "Proveedor");
                return RedirectToAction("Dashboard", "Proveedor");
            }
        }

        // ============================
        // GET: /Proveedor/Estadisticas
        // ============================
        [HttpGet]
        [Route("Proveedor/Estadisticas")]
        public IActionResult Estadisticas()
        {
            AsegurarTablasProveedores();
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Verificar que sea proveedor, si no, redirigir
            if (!EsProveedor())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                string proveedorQuery = @"
                    SELECT P.*
                    FROM ProveedorServicio P
                    WHERE P.Id_User = @UserId";
                
                DataTable proveedorDt = BD.ExecuteQuery(proveedorQuery, new Dictionary<string, object> { { "@UserId", userId.Value } });
                if (proveedorDt.Rows.Count == 0)
                {
                    return RedirectToAction("Registro1", "Proveedor");
                }

                int idProveedor = Convert.ToInt32(proveedorDt.Rows[0]["Id_Proveedor"]);
                ViewBag.Proveedor = proveedorDt.Rows[0];

                // Estadísticas detalladas
                string statsQuery = @"
                    SELECT 
                        COUNT(*) AS TotalReservas,
                        COUNT(CASE WHEN Id_EstadoReserva = 1 THEN 1 END) AS Pendientes,
                        COUNT(CASE WHEN Id_EstadoReserva = 2 THEN 1 END) AS Confirmadas,
                        COUNT(CASE WHEN Id_EstadoReserva = 3 THEN 1 END) AS EnCurso,
                        COUNT(CASE WHEN Id_EstadoReserva = 4 THEN 1 END) AS Completadas,
                        COUNT(CASE WHEN Id_EstadoReserva = 5 THEN 1 END) AS Canceladas,
                        SUM(CASE WHEN Id_EstadoReserva = 4 THEN Precio_Total ELSE 0 END) AS IngresosTotales,
                        AVG(CASE WHEN Id_EstadoReserva = 4 THEN Precio_Total ELSE NULL END) AS PromedioPorReserva
                    FROM ReservaProveedor
                    WHERE Id_Proveedor = @IdProveedor";
                
                DataTable statsDt = BD.ExecuteQuery(statsQuery, new Dictionary<string, object> { { "@IdProveedor", idProveedor } });
                if (statsDt.Rows.Count > 0)
                {
                    ViewBag.Estadisticas = statsDt.Rows[0];
                }

                ViewBag.TipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Proveedor/Estadisticas: " + ex.Message);
                TempData["Error"] = "Error al cargar las estadísticas.";
                string tipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";
                if (tipoPrincipal == "Paseador")
                    return RedirectToAction("DashboardPaseador", "Proveedor");
                else if (tipoPrincipal == "Cuidador")
                    return RedirectToAction("DashboardCuidador", "Proveedor");
                return RedirectToAction("Dashboard", "Proveedor");
            }
        }

        // ============================
        // POST: /Proveedor/IniciarReserva
        // ============================
        [HttpPost]
        [Route("Proveedor/IniciarReserva")]
        public IActionResult IniciarReserva([FromBody] dynamic request)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null || !EsProveedor())
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                int idReserva = Convert.ToInt32(request.idReserva);

                // Verificar que la reserva pertenece al proveedor
                string verificarQuery = @"
                    SELECT RP.Id_Reserva, P.Id_User
                    FROM ReservaProveedor RP
                    INNER JOIN ProveedorServicio P ON RP.Id_Proveedor = P.Id_Proveedor
                    WHERE RP.Id_Reserva = @IdReserva AND P.Id_User = @UserId";
                
                DataTable verificarDt = BD.ExecuteQuery(verificarQuery, new Dictionary<string, object>
                {
                    { "@IdReserva", idReserva },
                    { "@UserId", userId.Value }
                });

                if (verificarDt.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "Reserva no encontrada o no autorizada" });
                }

                // Actualizar estado a "EnCurso" (3) y marcar fecha/hora de inicio real
                string updateQuery = @"
                    UPDATE ReservaProveedor
                    SET Id_EstadoReserva = 3,
                        Fecha_Hora_Inicio_Real = GETDATE()
                    WHERE Id_Reserva = @IdReserva";

                BD.ExecuteNonQuery(updateQuery, new Dictionary<string, object> { { "@IdReserva", idReserva } });

                return Json(new { success = true, message = "Servicio iniciado" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en IniciarReserva: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // POST: /Proveedor/CompletarReserva
        // ============================
        [HttpPost]
        [Route("Proveedor/CompletarReserva")]
        public IActionResult CompletarReserva([FromBody] dynamic request)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null || !EsProveedor())
                {
                    return Json(new { success = false, message = "No autorizado" });
                }

                int idReserva = Convert.ToInt32(request.idReserva);

                // Verificar que la reserva pertenece al proveedor y está en curso
                string verificarQuery = @"
                    SELECT RP.Id_Reserva, P.Id_User, RP.Fecha_Hora_Inicio_Real
                    FROM ReservaProveedor RP
                    INNER JOIN ProveedorServicio P ON RP.Id_Proveedor = P.Id_Proveedor
                    WHERE RP.Id_Reserva = @IdReserva 
                      AND P.Id_User = @UserId
                      AND RP.Id_EstadoReserva = 3";
                
                DataTable verificarDt = BD.ExecuteQuery(verificarQuery, new Dictionary<string, object>
                {
                    { "@IdReserva", idReserva },
                    { "@UserId", userId.Value }
                });

                if (verificarDt.Rows.Count == 0)
                {
                    return Json(new { success = false, message = "Reserva no encontrada o no está en curso" });
                }

                // Calcular tiempo total si hay fecha de inicio real
                DateTime? fechaInicio = null;
                if (verificarDt.Rows[0]["Fecha_Hora_Inicio_Real"] != DBNull.Value)
                {
                    fechaInicio = Convert.ToDateTime(verificarDt.Rows[0]["Fecha_Hora_Inicio_Real"]);
                }

                // Obtener distancia total y tiempo total de las ubicaciones
                string statsQuery = @"
                    SELECT 
                        MAX(Distancia_Acumulada_Metros) AS DistanciaTotal,
                        MAX(Tiempo_Transcurrido_Segundos) AS TiempoTotal
                    FROM UbicacionServicio
                    WHERE Id_Reserva = @IdReserva";

                DataTable statsDt = BD.ExecuteQuery(statsQuery, new Dictionary<string, object> { { "@IdReserva", idReserva } });
                
                decimal? distanciaTotal = null;
                int? tiempoTotal = null;

                if (statsDt.Rows.Count > 0)
                {
                    if (statsDt.Rows[0]["DistanciaTotal"] != DBNull.Value)
                    {
                        distanciaTotal = Convert.ToDecimal(statsDt.Rows[0]["DistanciaTotal"]);
                    }
                    if (statsDt.Rows[0]["TiempoTotal"] != DBNull.Value)
                    {
                        tiempoTotal = Convert.ToInt32(statsDt.Rows[0]["TiempoTotal"]);
                    }
                }

                // Si no hay tiempo calculado, calcular desde fecha de inicio
                if (!tiempoTotal.HasValue && fechaInicio.HasValue)
                {
                    tiempoTotal = (int)(DateTime.Now - fechaInicio.Value).TotalSeconds;
                }

                // Actualizar estado a "Completada" (4) y marcar fecha/hora de fin real
                string updateQuery = @"
                    UPDATE ReservaProveedor
                    SET Id_EstadoReserva = 4,
                        Fecha_Hora_Fin_Real = GETDATE(),
                        Distancia_Total_Metros = @DistanciaTotal,
                        Tiempo_Total_Segundos = @TiempoTotal
                    WHERE Id_Reserva = @IdReserva";

                BD.ExecuteNonQuery(updateQuery, new Dictionary<string, object>
                {
                    { "@IdReserva", idReserva },
                    { "@DistanciaTotal", distanciaTotal ?? (object)DBNull.Value },
                    { "@TiempoTotal", tiempoTotal ?? (object)DBNull.Value }
                });

                return Json(new { success = true, message = "Servicio completado" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en CompletarReserva: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ============================
        // Método para insertar curiosidades iniciales
        // ============================
        private void InsertarCuriosidadesIniciales()
        {
            try
            {
                var curiosidades = new List<Dictionary<string, string>>
                {
                    // Perros - Golden Retriever
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Golden Retriever" }, { "Curiosidad", "Los Golden Retrievers tienen una membrana especial en sus patas que les permite nadar mejor. Son excelentes nadadores y les encanta el agua." }, { "Categoria", "Física" } },
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Golden Retriever" }, { "Curiosidad", "Fueron criados originalmente en Escocia en el siglo XIX para recuperar aves acuáticas durante la caza. Su nombre 'Retriever' significa 'recuperador'." }, { "Categoria", "Histórica" } },
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Golden Retriever" }, { "Curiosidad", "Tienen un 'pelaje doble' que los protege del agua y del frío. La capa externa repele el agua y la interna los mantiene calientes." }, { "Categoria", "Física" } },
                    
                    // Perros - Labrador Retriever
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Labrador Retriever" }, { "Curiosidad", "Los Labradores tienen una cola única llamada 'cola de nutria' que les ayuda a nadar y mantener el equilibrio." }, { "Categoria", "Física" } },
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Labrador Retriever" }, { "Curiosidad", "Son la raza más popular del mundo desde hace más de 30 años. Su inteligencia y carácter amigable los hacen ideales como perros de familia." }, { "Categoria", "Popularidad" } },
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Labrador Retriever" }, { "Curiosidad", "Tienen una membrana entre los dedos que les permite nadar de manera más eficiente. Son excelentes perros de rescate acuático." }, { "Categoria", "Habilidad" } },
                    
                    // Perros - Pastor Alemán
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Pastor Alemán" }, { "Curiosidad", "Los Pastores Alemanes tienen un sentido del olfato 100,000 veces más desarrollado que los humanos. Por eso son excelentes perros policía." }, { "Categoria", "Sentidos" } },
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Pastor Alemán" }, { "Curiosidad", "Fueron criados originalmente para pastorear ovejas, pero su inteligencia y lealtad los convirtieron en perros de trabajo versátiles." }, { "Categoria", "Histórica" } },
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Pastor Alemán" }, { "Curiosidad", "Tienen orejas que pueden moverse independientemente para captar sonidos desde diferentes direcciones, mejorando su capacidad de alerta." }, { "Categoria", "Física" } },
                    
                    // Perros - Caniche
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Caniche" }, { "Curiosidad", "Los Caniches son considerados la segunda raza más inteligente del mundo, después del Border Collie. Aprenden comandos muy rápido." }, { "Categoria", "Inteligencia" } },
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Caniche" }, { "Curiosidad", "Originalmente fueron criados como perros de caza acuática. Su pelaje rizado los protegía del agua fría mientras recuperaban aves." }, { "Categoria", "Histórica" } },
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Caniche" }, { "Curiosidad", "Vienen en tres tamaños: estándar, miniatura y toy. Todos comparten la misma personalidad activa e inteligente." }, { "Categoria", "Variedad" } },
                    
                    // Perros - Rottweiler
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Rottweiler" }, { "Curiosidad", "Los Rottweilers tienen una mordida de 328 libras de presión, una de las más fuertes entre las razas de perros." }, { "Categoria", "Física" } },
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Rottweiler" }, { "Curiosidad", "Fueron criados originalmente por los romanos para pastorear ganado y proteger a los soldados durante las campañas militares." }, { "Categoria", "Histórica" } },
                    new Dictionary<string, string> { { "Especie", "Perro" }, { "Raza", "Rottweiler" }, { "Curiosidad", "A pesar de su apariencia intimidante, son perros muy leales y protectores con sus familias. Son excelentes perros guardianes." }, { "Categoria", "Personalidad" } },
                    
                    // Gatos - Persa
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Persa" }, { "Curiosidad", "Los gatos Persas tienen una cara plana debido a una mutación genética llamada braquicefalia. Necesitan limpieza facial regular." }, { "Categoria", "Física" } },
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Persa" }, { "Curiosidad", "Tienen el pelaje más largo de todas las razas de gatos. Requieren cepillado diario para evitar nudos y mantenerlo saludable." }, { "Categoria", "Cuidado" } },
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Persa" }, { "Curiosidad", "Son conocidos por su personalidad tranquila y relajada. Prefieren ambientes calmados y no son muy activos." }, { "Categoria", "Personalidad" } },
                    
                    // Gatos - Siames
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Siames" }, { "Curiosidad", "Los gatos Siameses tienen un gen de temperatura que hace que su pelaje sea más oscuro en las partes más frías del cuerpo (orejas, patas, cola)." }, { "Categoria", "Genética" } },
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Siames" }, { "Curiosidad", "Son extremadamente vocales y 'hablan' mucho con sus dueños. Tienen un maullido distintivo y fuerte." }, { "Categoria", "Comportamiento" } },
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Siames" }, { "Curiosidad", "Fueron considerados gatos sagrados en el antiguo reino de Siam (Tailandia). Solo la realeza podía tenerlos." }, { "Categoria", "Histórica" } },
                    
                    // Gatos - Maine Coon
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Maine Coon" }, { "Curiosidad", "Los Maine Coon son la raza de gato doméstico más grande. Los machos pueden pesar hasta 11 kg y medir más de 1 metro de largo." }, { "Categoria", "Tamaño" } },
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Maine Coon" }, { "Curiosidad", "Tienen mechones de pelo en las orejas que los protegen del frío, similar a los linces. Son excelentes cazadores." }, { "Categoria", "Física" } },
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Maine Coon" }, { "Curiosidad", "A diferencia de la mayoría de los gatos, a muchos Maine Coon les encanta el agua. Algunos incluso disfrutan nadar." }, { "Categoria", "Comportamiento" } },
                    
                    // Gatos - Bombay
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Bombay" }, { "Curiosidad", "Los gatos Bombay fueron criados para parecerse a una pantera negra en miniatura. Tienen un pelaje completamente negro y brillante." }, { "Categoria", "Apariencia" } },
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Bombay" }, { "Curiosidad", "A pesar de su apariencia salvaje, son gatos muy cariñosos y sociables. Les encanta estar cerca de sus dueños." }, { "Categoria", "Personalidad" } },
                    new Dictionary<string, string> { { "Especie", "Gato" }, { "Raza", "Bombay" }, { "Curiosidad", "Tienen ojos dorados o cobrizos que contrastan hermosamente con su pelaje negro. Son conocidos como 'panteras de salón'." }, { "Categoria", "Apariencia" } }
                };

                foreach (var cur in curiosidades)
                {
                    string insertQuery = @"
                        INSERT INTO CuriosidadRaza (Especie, Raza, Curiosidad, Categoria)
                        VALUES (@Especie, @Raza, @Curiosidad, @Categoria)";
                    
                    BD.ExecuteNonQuery(insertQuery, new Dictionary<string, object>
                    {
                        { "@Especie", cur["Especie"] },
                        { "@Raza", cur["Raza"] },
                        { "@Curiosidad", cur["Curiosidad"] },
                        { "@Categoria", cur["Categoria"] }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al insertar curiosidades iniciales: " + ex.Message);
            }
        }
    }
}

