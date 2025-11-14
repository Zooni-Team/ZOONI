using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Zooni.Models
{
    public static class EmailHelper
    {
        // Validar formato de email
        public static bool ValidarFormatoEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Regex para validar formato de email
                string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                return Regex.IsMatch(email.Trim(), pattern, RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        // Verificar que el dominio del email existe (DNS lookup)
        public static bool VerificarDominioEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Extraer dominio del email
                string dominio = email.Split('@')[1].ToLower();

                // Lista de dominios comunes de email válidos
                string[] dominiosValidos = {
                    "gmail.com", "yahoo.com", "hotmail.com", "outlook.com",
                    "live.com", "msn.com", "icloud.com", "me.com", "mac.com",
                    "aol.com", "protonmail.com", "proton.me", "zoho.com",
                    "mail.com", "gmx.com", "yandex.com", "mail.ru",
                    "qq.com", "163.com", "sina.com", "rediffmail.com",
                    "terra.com.br", "uol.com.br", "bol.com.br", "ig.com.br",
                    "globo.com", "yahoo.com.br", "hotmail.com.br", "gmail.com.br"
                };

                // Verificar si está en la lista de dominios conocidos
                foreach (var dom in dominiosValidos)
                {
                    if (dominio == dom || dominio.EndsWith("." + dom))
                        return true;
                }

                // Intentar verificar DNS del dominio
                try
                {
                    var hostEntry = Dns.GetHostEntry(dominio);
                    return hostEntry.AddressList.Length > 0;
                }
                catch
                {
                    // Si falla el DNS lookup, pero el dominio está en la lista, es válido
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        // Validar email completo (formato + dominio)
        public static bool ValidarEmailCompleto(string email)
        {
            if (!ValidarFormatoEmail(email))
                return false;

            return VerificarDominioEmail(email);
        }

        // Verificar si es un email de Gmail (o similar)
        public static bool EsEmailGmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                string dominio = email.Split('@')[1].ToLower();
                return dominio == "gmail.com" || dominio == "googlemail.com";
            }
            catch
            {
                return false;
            }
        }
    }
}

