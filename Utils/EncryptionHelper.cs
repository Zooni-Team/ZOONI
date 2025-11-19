using System;
using System.Security.Cryptography;
using System.Text;

namespace Zooni.Utils
{
    /// <summary>
    /// Clase helper para encriptación y desencriptación de datos sensibles usando AES-256
    /// </summary>
    public static class EncryptionHelper
    {
        // Clave de encriptación - EN PRODUCCIÓN DEBE ESTAR EN VARIABLES DE ENTORNO O KEY VAULT
        private static readonly string EncryptionKey = GetEncryptionKey();

        private static string GetEncryptionKey()
        {
            // Intentar obtener de configuración, si no existe usar una clave por defecto
            // IMPORTANTE: En producción, usar variables de entorno o Azure Key Vault
            var key = Environment.GetEnvironmentVariable("ZOONI_ENCRYPTION_KEY");
            if (string.IsNullOrEmpty(key))
            {
                // Clave por defecto - CAMBIAR EN PRODUCCIÓN
                key = "Zooni2024SecureKey256Bits!!"; // 32 caracteres para AES-256
            }
            
            // Asegurar que la clave tenga exactamente 32 caracteres
            if (key.Length < 32)
            {
                key = key.PadRight(32, '0');
            }
            else if (key.Length > 32)
            {
                key = key.Substring(0, 32);
            }
            
            return key;
        }

        /// <summary>
        /// Encripta un texto usando AES-256
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                byte[] iv = new byte[16];
                byte[] array;

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                            {
                                streamWriter.Write(plainText);
                            }
                            array = memoryStream.ToArray();
                        }
                    }
                }

                return Convert.ToBase64String(array);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al encriptar: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Desencripta un texto encriptado con AES-256
        /// </summary>
        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                // Verificar si el texto parece estar encriptado (Base64 válido y longitud mínima)
                if (cipherText.Length < 16 || !IsBase64String(cipherText))
                {
                    // Probablemente no está encriptado, retornar tal cual
                    return cipherText;
                }

                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Encoding.UTF8.GetBytes(EncryptionKey);
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader(cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (FormatException)
            {
                // No es Base64 válido, probablemente no está encriptado
                return cipherText;
            }
            catch (CryptographicException ex)
            {
                Console.WriteLine($"⚠️ Error criptográfico al desencriptar (posible clave incorrecta): {ex.Message}");
                // Si falla la desencriptación, puede ser que el dato no esté encriptado o la clave sea diferente
                // Retornar el texto original
                return cipherText;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error al desencriptar: {ex.Message}");
                // Si falla la desencriptación, puede ser que el dato no esté encriptado (datos antiguos)
                // Retornar el texto original
                return cipherText;
            }
        }

        /// <summary>
        /// Verifica si una cadena es Base64 válido
        /// </summary>
        private static bool IsBase64String(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length % 4 != 0)
                return false;

            try
            {
                Convert.FromBase64String(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Encripta un número (DNI, etc.) convirtiéndolo a string primero
        /// </summary>
        public static string EncryptNumber(string number)
        {
            if (string.IsNullOrEmpty(number))
                return number;
            return Encrypt(number);
        }

        /// <summary>
        /// Desencripta un número
        /// </summary>
        public static string DecryptNumber(string encryptedNumber)
        {
            if (string.IsNullOrEmpty(encryptedNumber))
                return encryptedNumber;
            return Decrypt(encryptedNumber);
        }
    }
}

