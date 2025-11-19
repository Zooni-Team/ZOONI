using Microsoft.AspNetCore.Mvc;
using Zooni.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Zooni.Controllers
{
    public abstract class BaseController : Controller
    {
        // Método helper para verificar si el usuario es proveedor
        protected bool EsProveedor()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return false;

            string? esProveedor = HttpContext.Session.GetString("EsProveedor");
            if (esProveedor == "true") return true;

            // Verificar en la base de datos por si acaso
            try
            {
                string query = @"
                    IF OBJECT_ID('ProveedorServicio', 'U') IS NOT NULL
                        SELECT COUNT(*) FROM ProveedorServicio WHERE Id_User = @UserId
                    ELSE
                        SELECT 0";
                object? result = BD.ExecuteScalar(query, new Dictionary<string, object> { { "@UserId", userId.Value } });
                int count = result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
                
                if (count > 0)
                {
                    HttpContext.Session.SetString("EsProveedor", "true");
                    return true;
                }
            }
            catch
            {
                // Si hay error, asumir que no es proveedor
            }

            return false;
        }

        // Método helper para verificar si el usuario NO es proveedor (es dueño)
        protected bool EsDueño()
        {
            return !EsProveedor();
        }

        // Método helper para redirigir proveedores a su dashboard
        protected IActionResult RedirigirProveedorSiEsNecesario()
        {
            if (EsProveedor())
            {
                string tipoPrincipal = HttpContext.Session.GetString("ProveedorTipoPrincipal") ?? "";
                if (tipoPrincipal == "Paseador")
                    return RedirectToAction("DashboardPaseador", "Proveedor");
                else if (tipoPrincipal == "Cuidador")
                    return RedirectToAction("DashboardCuidador", "Proveedor");
                else
                    return RedirectToAction("Dashboard", "Proveedor");
            }
            return null;
        }

        // Método helper para redirigir dueños al Index
        protected IActionResult RedirigirDueñoSiEsNecesario()
        {
            if (EsDueño())
            {
                return RedirectToAction("Index", "Home");
            }
            return null;
        }
    }
}

