using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using Zooni.Utils;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zooni.Controllers
{
    public class AuthController : Controller
    {
        private void AsegurarColumnasEstadoOnline()
        {
            try
            {
                // Verificar y crear columna EstadoOnline
                string qEstadoOnline = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'EstadoOnline' AND Object_ID = Object_ID(N'[User]'))
                    BEGIN
                        ALTER TABLE [User] ADD EstadoOnline BIT NOT NULL DEFAULT 0;
                    END";

                BD.ExecuteNonQuery(qEstadoOnline, new Dictionary<string, object>());

                // Verificar y crear columna UltimaActividad
                string qUltimaActividad = @"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = N'UltimaActividad' AND Object_ID = Object_ID(N'[User]'))
                    BEGIN
                        ALTER TABLE [User] ADD UltimaActividad DATETIME2 NULL;
                    END";

                BD.ExecuteNonQuery(qUltimaActividad, new Dictionary<string, object>());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al crear columnas EstadoOnline/UltimaActividad: " + ex.Message);
            }
        }
        // ============================
        // ✅ GET: /Auth/Login
        // ============================
        [HttpGet]
        [Route("Auth/Login")]
        public IActionResult Login()
        {
            return View();
        }

        // ============================
        // ✅ POST: /Auth/Login
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Auth/Login")]
        public IActionResult Login(string correo, string contrasena)
        {
            try
            {
                if (string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(contrasena))
                {
                    ViewBag.Error = "Completá todos los campos.";
                    return View();
                }

                // 🔍 Buscar usuario y validar credenciales
                // Intentar buscar primero con correo encriptado (si los datos están encriptados)
                string correoNormalizado = correo.ToLower().Trim();
                string correoEncrypted = EncryptionHelper.Encrypt(correoNormalizado);
                
                DataTable dt = null;
                
                // Primero intentar buscar con correo encriptado
                string query = @"
                    SELECT TOP 1 
                        U.Id_User, U.Nombre, U.Apellido, 
                        M.Correo, M.Contrasena,
                        U.Pais, U.Provincia, U.Ciudad, U.Telefono
                    FROM [User] U
                    INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                    WHERE M.Correo = @Correo AND U.Estado = 1";

                var parametros = new Dictionary<string, object>
                {
                    { "@Correo", correoEncrypted }
                };

                dt = BD.ExecuteQuery(query, parametros);

                // Si no encontró con correo encriptado, intentar buscar todos y comparar desencriptando
                if (dt.Rows.Count == 0)
                {
                    Console.WriteLine("⚠️ No se encontró con correo encriptado, intentando buscar desencriptando...");
                    string queryAll = @"
                        SELECT 
                            U.Id_User, U.Nombre, U.Apellido, 
                            M.Correo, M.Contrasena,
                            U.Pais, U.Provincia, U.Ciudad, U.Telefono
                        FROM [User] U
                        INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                        WHERE U.Estado = 1";
                    
                    DataTable dtAll = BD.ExecuteQuery(queryAll, new Dictionary<string, object>());
                    
                    foreach (DataRow row in dtAll.Rows)
                    {
                        try
                        {
                            string correoStored = row["Correo"].ToString() ?? "";
                            string correoDesencriptado = EncryptionHelper.Decrypt(correoStored);
                            
                            // Si la desencriptación devuelve el mismo texto, significa que no estaba encriptado
                            // Comparar directamente
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
                            Console.WriteLine($"⚠️ Error al desencriptar correo: {ex.Message}");
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

                if (dt == null || dt.Rows.Count == 0)
                {
                    ViewBag.Error = "Usuario o contraseña incorrectos.";
                    return View();
                }

                // Verificar contraseña usando PasswordHelper
                string storedPasswordHash = dt.Rows[0]["Contrasena"].ToString() ?? "";
                if (!PasswordHelper.VerifyPassword(contrasena, storedPasswordHash))
                {
                    ViewBag.Error = "Usuario o contraseña incorrectos.";
                    return View();
                }

                // ✅ Usuario válido → guardar sesión
                var user = dt.Rows[0];
                int userId = Convert.ToInt32(user["Id_User"]);
                
                // Desencriptar datos para la sesión
                string correoDecrypted = EncryptionHelper.Decrypt(user["Correo"].ToString() ?? "");
                string nombreDecrypted = EncryptionHelper.Decrypt(user["Nombre"].ToString() ?? "");
                string apellidoDecrypted = EncryptionHelper.Decrypt(user["Apellido"].ToString() ?? "");
                
                HttpContext.Session.SetInt32("UserId", userId);
                HttpContext.Session.SetString("UserNombre", nombreDecrypted);
                HttpContext.Session.SetString("UserApellido", apellidoDecrypted);
                HttpContext.Session.SetString("UserMail", correoDecrypted);

                // 🔍 Verificar si es proveedor (solo si la tabla existe)
                // Primero verificar si existe la tabla ProveedorServicio
                int esProveedor = 0;
                try
                {
                    string checkTableQuery = "SELECT COUNT(*) FROM sys.tables WHERE name = 'ProveedorServicio'";
                    object? tableExists = BD.ExecuteScalar(checkTableQuery);
                    int tableExistsInt = tableExists != null && tableExists != DBNull.Value ? Convert.ToInt32(tableExists) : 0;
                    
                    if (tableExistsInt > 0)
                    {
                        string proveedorQuery = @"
                            SELECT COUNT(*) FROM ProveedorServicio 
                            WHERE Id_User = @UserId AND Estado = 1";
                        object? proveedorResult = BD.ExecuteScalar(proveedorQuery, new Dictionary<string, object> { { "@UserId", userId } });
                        esProveedor = proveedorResult != null && proveedorResult != DBNull.Value ? Convert.ToInt32(proveedorResult) : 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ Error al verificar proveedor (tabla puede no existir): " + ex.Message);
                    esProveedor = 0;
                }
                
                // Establecer sesión según tipo de usuario
                if (esProveedor > 0)
                {
                    HttpContext.Session.SetString("EsProveedor", "true");
                    HttpContext.Session.SetString("TipoUsuario", "Proveedor");
                    
                    // Obtener tipo principal del proveedor (Paseador o Cuidador)
                    try
                    {
                        string tipoQuery = @"
                            SELECT TOP 1 TS.Descripcion
                            FROM ProveedorServicio P
                            INNER JOIN ProveedorServicio_TipoServicio PSTS ON P.Id_Proveedor = PSTS.Id_Proveedor
                            INNER JOIN TipoServicio TS ON PSTS.Id_TipoServicio = TS.Id_TipoServicio
                            WHERE P.Id_User = @UserId
                              AND TS.Descripcion IN ('Paseador', 'Cuidador')
                            ORDER BY 
                                CASE 
                                    WHEN TS.Descripcion = 'Paseador' THEN 1
                                    WHEN TS.Descripcion = 'Cuidador' THEN 2
                                END";
                        
                        object? tipoResult = BD.ExecuteScalar(tipoQuery, new Dictionary<string, object> { { "@UserId", userId } });
                        string tipoPrincipal = tipoResult?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(tipoPrincipal))
                        {
                            HttpContext.Session.SetString("ProveedorTipoPrincipal", tipoPrincipal);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("⚠️ Error al obtener tipo principal del proveedor: " + ex.Message);
                    }
                }
                else
                {
                    HttpContext.Session.SetString("EsProveedor", "false");
                    HttpContext.Session.SetString("TipoUsuario", "Dueño");
                }

                // 🟢 Marcar usuario como online
                try
                {
                    AsegurarColumnasEstadoOnline();
                    string qUpdateOnline = @"
                        UPDATE [User] 
                        SET EstadoOnline = 1, UltimaActividad = GETDATE()
                        WHERE Id_User = @UserId";
                    BD.ExecuteNonQuery(qUpdateOnline, new Dictionary<string, object> { { "@UserId", userId } });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ Error al actualizar estado online: " + ex.Message);
                }

                // Redirigir según tipo de usuario
                if (esProveedor > 0)
                {
                    // Redirigir al dashboard personalizado según el tipo principal
                    string tipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";
                    if (tipoPrincipal == "Paseador")
                    {
                        return RedirectToAction("DashboardPaseador", "Proveedor");
                    }
                    else if (tipoPrincipal == "Cuidador")
                    {
                        return RedirectToAction("DashboardCuidador", "Proveedor");
                    }
                    else
                    {
                        return RedirectToAction("Dashboard", "Proveedor");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en Auth/Login POST: " + ex.Message);
                ViewBag.Error = "Error al iniciar sesión. Intentalo de nuevo.";
                return View();
            }
        }

        // ============================
        // ✅ GET: /Auth/Logout
        // ============================
        [HttpGet]
        [Route("Auth/Logout")]
        public IActionResult Logout()
        {
            // 🔴 Marcar usuario como offline antes de limpiar la sesión
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                try
                {
                    AsegurarColumnasEstadoOnline();
                    string qUpdateOffline = @"
                        UPDATE [User] 
                        SET EstadoOnline = 0, UltimaActividad = GETDATE()
                        WHERE Id_User = @UserId";
                    BD.ExecuteNonQuery(qUpdateOffline, new Dictionary<string, object> { { "@UserId", userId.Value } });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("⚠️ Error al actualizar estado offline: " + ex.Message);
                }
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
