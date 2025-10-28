using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Data;
using System.Text.Json;
using Zooni.Models; // Importa tus modelos y acceso a BD

namespace Zooni.Controllers
{
    public class ChatController : Controller
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;

        public ChatController(Kernel kernel)
        {
            _kernel = kernel;
            _chatService = _kernel.GetRequiredService<IChatCompletionService>();
        }

        // ============================
        // ✅ GET: /Chat/ChatZooni
        // ============================
        [HttpGet]
        public IActionResult ChatZooni()
        {
            try
            {
                // 🧠 Verificamos usuario logueado
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["Error"] = "Iniciá sesión para acceder al chat de ZooniVet.";
                    return RedirectToAction("Login", "Auth");
                }

                // 🐾 Traemos la mascota más reciente del usuario desde la BD
                string query = @"
                    SELECT TOP 1 
                        Nombre, Especie, Raza, Peso, Edad, Sexo
                    FROM Mascota
                    WHERE Id_User = @UserId
                    ORDER BY Id_Mascota DESC";

                var parametros = new Dictionary<string, object>
                {
                    { "@UserId", userId.Value }
                };

                DataTable dt = BD.ExecuteQuery(query, parametros);

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "No se encontró ninguna mascota asociada. Registrá una primero.";
                    return RedirectToAction("Registro2", "Registro");
                }

                var mascota = dt.Rows[0];

                // 📦 Pasamos los datos reales a la vista
                ViewBag.Nombre = mascota["Nombre"].ToString();
                ViewBag.Especie = mascota["Especie"].ToString();
                ViewBag.Raza = mascota["Raza"].ToString();
                ViewBag.Peso = mascota["Peso"]?.ToString() ?? "0";
                ViewBag.Edad = mascota["Edad"]?.ToString() ?? "0";
                ViewBag.Sexo = mascota["Sexo"]?.ToString() ?? "No definido";
                ViewBag.InfoMedica = "Historial médico cargado automáticamente.";

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error ChatZooni GET: " + ex.Message);
                TempData["Error"] = "Error al cargar datos de la mascota.";
                return RedirectToAction("Index", "Home");
            }
        }

        // ============================
        // ✅ POST: /Chat/EnviarMensaje
        // ============================
        [HttpPost]
        public async Task<IActionResult> EnviarMensaje(
            string especie, string raza, string edad, string peso,
            string infoMedica, string mensaje)
        {
            var historyJson = HttpContext.Session.GetString("ZooniHistory");
            ChatHistory chatHistory;

            if (string.IsNullOrEmpty(historyJson))
            {
                chatHistory = new ChatHistory();
                string sys = $"""
                Eres ZooniVet, el asistente veterinario empático y profesional de la app Zooni 🩵.
                Trata de formatear bien los textos y no uses ** ni nada, pensa que es como un mensaje de texto, se tiene que entender bien ni enumeres.
                Tu misión es ayudar a dueños de mascotas con consejos útiles, preventivos y empáticos.
                Si algo parece grave, recomendá siempre una consulta veterinaria presencial.
                Si vas a decir la edad de la mascota decila en años y meses, no en meses.
                Datos actuales de la mascota:
                - Especie: {especie}
                - Raza: {raza}
                - Edad: {edad} meses
                - Peso: {peso} kg
                - Info médica: {infoMedica}
                """;
                chatHistory.AddSystemMessage(sys);
            }
            else
            {
                chatHistory = JsonSerializer.Deserialize<ChatHistory>(historyJson) ?? new ChatHistory();
            }

            chatHistory.AddUserMessage(mensaje);

            var settings = new PromptExecutionSettings
            {
                ExtensionData = new Dictionary<string, object?>
                {
                    ["temperature"] = 0.6,
                    ["top_p"] = 0.9
                }
            };

            try
            {
                var response = await _chatService.GetChatMessageContentAsync(chatHistory, settings);
                string respuesta = response.Content ?? "⚠️ No recibí respuesta del modelo.";
                chatHistory.AddAssistantMessage(respuesta);

                HttpContext.Session.SetString("ZooniHistory", JsonSerializer.Serialize(chatHistory));
                return Json(new { respuesta });
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error EnviarMensaje: " + ex.Message);
                return Json(new { respuesta = $"❌ Error de conexión con Gemini: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ReiniciarChat()
        {
            HttpContext.Session.Remove("ZooniHistory");
            return Json(new { ok = true });
        }
    }
}
