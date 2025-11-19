using System;
using System.Security.Cryptography;
using System.Text;

namespace Zooni.Utils
{
    /// <summary>
    /// Clase helper para hashing seguro de contraseñas usando SHA-256 con salt
    /// </summary>
    public static class PasswordHelper
    {
        /// <summary>
        /// Genera un hash seguro de una contraseña usando SHA-256 con salt
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            // Generar un salt aleatorio
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Combinar password + salt
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
            Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
            Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

            // Hashear con SHA-256
            byte[] hash;
            using (var sha256 = SHA256.Create())
            {
                hash = sha256.ComputeHash(saltedPassword);
            }

            // Combinar salt + hash en un solo string (salt al inicio)
            byte[] hashWithSalt = new byte[hash.Length + salt.Length];
            Buffer.BlockCopy(salt, 0, hashWithSalt, 0, salt.Length);
            Buffer.BlockCopy(hash, 0, hashWithSalt, salt.Length, hash.Length);

            // Convertir a Base64 para almacenamiento
            return Convert.ToBase64String(hashWithSalt);
        }

        /// <summary>
        /// Verifica si una contraseña coincide con un hash almacenado
        /// </summary>
        public static bool VerifyPassword(string password, string hashWithSalt)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashWithSalt))
                return false;

            try
            {
                // Decodificar el hash con salt
                byte[] hashWithSaltBytes = Convert.FromBase64String(hashWithSalt);

                // Extraer el salt (primeros 16 bytes)
                byte[] salt = new byte[16];
                Buffer.BlockCopy(hashWithSaltBytes, 0, salt, 0, 16);

                // Extraer el hash (últimos 32 bytes)
                byte[] storedHash = new byte[32];
                Buffer.BlockCopy(hashWithSaltBytes, 16, storedHash, 0, 32);

                // Calcular el hash de la contraseña proporcionada con el mismo salt
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
                Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                byte[] computedHash;
                using (var sha256 = SHA256.Create())
                {
                    computedHash = sha256.ComputeHash(saltedPassword);
                }

                // Comparar los hashes byte por byte
                if (computedHash.Length != storedHash.Length)
                    return false;

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i])
                        return false;
                }

                return true;
            }
            catch
            {
                // Si hay error al decodificar, puede ser una contraseña antigua en texto plano
                // Intentar comparación directa (para migración gradual)
                return password == hashWithSalt;
            }
        }

        /// <summary>
        /// Verifica si un hash es de formato antiguo (texto plano) o nuevo (hasheado)
        /// </summary>
        public static bool IsHashed(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            try
            {
                // Intentar decodificar como Base64
                byte[] bytes = Convert.FromBase64String(hash);
                // Si tiene al menos 48 bytes (16 salt + 32 hash), es un hash válido
                return bytes.Length >= 48;
            }
            catch
            {
                // Si no es Base64 válido, es texto plano
                return false;
            }
        }
    }
}

