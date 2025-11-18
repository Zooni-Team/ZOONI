using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zooni.Controllers
{
    public class BuscarProveedorController : Controller
    {
        // ============================
        // GET: /BuscarProveedor
        // ============================
        [HttpGet]
        [Route("BuscarProveedor")]
        public IActionResult Index(string? ciudad, string? provincia, string? tipoServicio, string? especie, 
            decimal? latitud, decimal? longitud, decimal? radioKm)
        {
            try
            {
                // Cargar tipos de servicio
                string tiposQuery = "SELECT Id_TipoServicio, Descripcion FROM TipoServicio ORDER BY Descripcion";
                DataTable tiposDt = BD.ExecuteQuery(tiposQuery);
                ViewBag.TiposServicio = tiposDt;

                // Construir query de búsqueda
                string query = @"
                    SELECT DISTINCT
                        P.Id_Proveedor,
                        P.NombreCompleto,
                        P.Descripcion,
                        P.FotoPerfil,
                        P.Ciudad,
                        P.Provincia,
                        P.Precio_Hora,
                        P.Calificacion_Promedio,
                        P.Cantidad_Resenas,
                        P.Experiencia_Anios,
                        P.Latitud,
                        P.Longitud,
                        P.Radio_Atencion_Km,
                        P.Tipo_Ubicacion";

                // Si hay coordenadas, calcular distancia
                if (latitud.HasValue && longitud.HasValue)
                {
                    query += @",
                        (6371 * acos(
                            cos(radians(@Latitud)) * 
                            cos(radians(P.Latitud)) * 
                            cos(radians(P.Longitud) - radians(@Longitud)) + 
                            sin(radians(@Latitud)) * 
                            sin(radians(P.Latitud))
                        )) AS Distancia_Km";
                }

                query += @"
                    FROM ProveedorServicio P
                    WHERE P.Estado = 1 
                      AND P.Latitud IS NOT NULL 
                      AND P.Longitud IS NOT NULL";

                var parametros = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(ciudad))
                {
                    query += " AND P.Ciudad LIKE @Ciudad";
                    parametros.Add("@Ciudad", $"%{ciudad}%");
                }

                if (!string.IsNullOrEmpty(provincia))
                {
                    query += " AND P.Provincia LIKE @Provincia";
                    parametros.Add("@Provincia", $"%{provincia}%");
                }

                // Filtrado por ubicación (radio de búsqueda)
                if (latitud.HasValue && longitud.HasValue)
                {
                    parametros.Add("@Latitud", latitud.Value);
                    parametros.Add("@Longitud", longitud.Value);
                    
                    decimal radioBusqueda = radioKm ?? 10.0M; // Radio por defecto: 10 km
                    query += @"
                        AND (
                            (6371 * acos(
                                cos(radians(@Latitud)) * 
                                cos(radians(P.Latitud)) * 
                                cos(radians(P.Longitud) - radians(@Longitud)) + 
                                sin(radians(@Latitud)) * 
                                sin(radians(P.Latitud))
                            )) <= 
                            CASE 
                                WHEN P.Tipo_Ubicacion = 'Precisa' THEN @RadioBusqueda
                                WHEN P.Tipo_Ubicacion = 'Cobertura' THEN (P.Radio_Atencion_Km + @RadioBusqueda)
                                ELSE @RadioBusqueda
                            END
                        )";
                    parametros.Add("@RadioBusqueda", radioBusqueda);
                }

                if (!string.IsNullOrEmpty(tipoServicio))
                {
                    query += @"
                        AND EXISTS (
                            SELECT 1 FROM ProveedorServicio_TipoServicio PST
                            INNER JOIN TipoServicio TS ON PST.Id_TipoServicio = TS.Id_TipoServicio
                            WHERE PST.Id_Proveedor = P.Id_Proveedor
                            AND TS.Descripcion LIKE @TipoServicio
                        )";
                    parametros.Add("@TipoServicio", $"%{tipoServicio}%");
                }

                if (!string.IsNullOrEmpty(especie))
                {
                    query += @"
                        AND EXISTS (
                            SELECT 1 FROM ProveedorServicio_Especie PE
                            WHERE PE.Id_Proveedor = P.Id_Proveedor
                            AND PE.Especie = @Especie
                        )";
                    parametros.Add("@Especie", especie);
                }

                // Ordenar por distancia si hay coordenadas, sino por calificación
                if (latitud.HasValue && longitud.HasValue)
                {
                    query += " ORDER BY Distancia_Km ASC, P.Calificacion_Promedio DESC, P.Cantidad_Resenas DESC";
                }
                else
                {
                    query += " ORDER BY P.Calificacion_Promedio DESC, P.Cantidad_Resenas DESC";
                }

                DataTable proveedoresDt = BD.ExecuteQuery(query, parametros);
                ViewBag.Proveedores = proveedoresDt;
                ViewBag.Ciudad = ciudad;
                ViewBag.Provincia = provincia;
                ViewBag.TipoServicio = tipoServicio;
                ViewBag.Especie = especie;
                ViewBag.Latitud = latitud;
                ViewBag.Longitud = longitud;
                ViewBag.RadioKm = radioKm ?? 10.0M;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en BuscarProveedor/Index: " + ex.Message);
                ViewBag.Error = "Error al buscar proveedores.";
                return View();
            }
        }

        // ============================
        // GET: /BuscarProveedor/Perfil/{id}
        // ============================
        [HttpGet]
        [Route("BuscarProveedor/Perfil/{id}")]
        public IActionResult Perfil(int id)
        {
            try
            {
                // Obtener datos del proveedor
                string proveedorQuery = @"
                    SELECT 
                        P.*,
                        U.Nombre,
                        U.Apellido,
                        M.Correo
                    FROM ProveedorServicio P
                    INNER JOIN [User] U ON P.Id_User = U.Id_User
                    INNER JOIN Mail M ON U.Id_Mail = M.Id_Mail
                    WHERE P.Id_Proveedor = @Id AND P.Estado = 1";

                DataTable proveedorDt = BD.ExecuteQuery(proveedorQuery, new Dictionary<string, object> { { "@Id", id } });

                if (proveedorDt.Rows.Count == 0)
                {
                    TempData["Error"] = "Proveedor no encontrado.";
                    return RedirectToAction("Index", "BuscarProveedor");
                }

                ViewBag.Proveedor = proveedorDt.Rows[0];

                // Obtener tipos de servicio del proveedor
                string tiposQuery = @"
                    SELECT TS.Descripcion
                    FROM ProveedorServicio_TipoServicio PST
                    INNER JOIN TipoServicio TS ON PST.Id_TipoServicio = TS.Id_TipoServicio
                    WHERE PST.Id_Proveedor = @Id";
                DataTable tiposDt = BD.ExecuteQuery(tiposQuery, new Dictionary<string, object> { { "@Id", id } });
                ViewBag.TiposServicio = tiposDt;

                // Obtener especies del proveedor
                string especiesQuery = @"
                    SELECT Especie
                    FROM ProveedorServicio_Especie
                    WHERE Id_Proveedor = @Id";
                DataTable especiesDt = BD.ExecuteQuery(especiesQuery, new Dictionary<string, object> { { "@Id", id } });
                ViewBag.Especies = especiesDt;

                // Obtener reseñas
                string resenasQuery = @"
                    SELECT 
                        R.Id_Resena,
                        R.Calificacion,
                        R.Comentario,
                        R.Fecha,
                        U.Nombre,
                        U.Apellido
                    FROM Resena R
                    INNER JOIN [User] U ON R.Id_User = U.Id_User
                    WHERE R.Id_Proveedor = @Id
                    ORDER BY R.Fecha DESC";
                DataTable resenasDt = BD.ExecuteQuery(resenasQuery, new Dictionary<string, object> { { "@Id", id } });
                ViewBag.Resenas = resenasDt;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error en BuscarProveedor/Perfil: " + ex.Message);
                TempData["Error"] = "Error al cargar el perfil.";
                return RedirectToAction("Index", "BuscarProveedor");
            }
        }
    }
}

