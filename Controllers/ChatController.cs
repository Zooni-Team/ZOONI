using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Data;
using System.Text.Json;
using Zooni.Models;

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

        [HttpGet]
        public IActionResult ChatZooni()
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["Error"] = "Iniciá sesión para acceder al chat de ZooniVet.";
                    return RedirectToAction("Login", "Auth");
                }

                string query = @"
                    SELECT TOP 1 
                        M.Id_Mascota, M.Nombre, M.Especie, M.Raza, M.Peso, M.PesoDisplay, M.Edad, M.Sexo, 
                        M.Esterilizado, M.Color, M.Chip
                    FROM Mascota M
                    WHERE M.Id_User = @UserId
                    ORDER BY M.Id_Mascota DESC";

                var parametros = new Dictionary<string, object> { { "@UserId", userId.Value } };
                DataTable dt = BD.ExecuteQuery(query, parametros);

                if (dt.Rows.Count == 0)
                {
                    TempData["Error"] = "No se encontró ninguna mascota asociada. Registrá una primero.";
                    return RedirectToAction("Registro2", "Registro");
                }

                var mascota = dt.Rows[0];
                int idMascota = Convert.ToInt32(mascota["Id_Mascota"]);
                string nombre = mascota["Nombre"].ToString();
                string especie = mascota["Especie"].ToString();
                string raza = mascota["Raza"].ToString();
                
                // Manejo mejorado del peso usando PesoHelper
                decimal pesoDecimal = 0;
                string? pesoDisplay = null;
                
                if (mascota.Table.Columns.Contains("PesoDisplay") && mascota["PesoDisplay"] != DBNull.Value)
                {
                    pesoDisplay = mascota["PesoDisplay"].ToString();
                }
                
                if (mascota["Peso"] != DBNull.Value && decimal.TryParse(mascota["Peso"].ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out pesoDecimal))
                {
                    // Usar el peso tal cual está en la BD, sin correcciones
                    if (string.IsNullOrEmpty(pesoDisplay))
                    {
                        pesoDisplay = PesoHelper.FormatearPeso(pesoDecimal);
                    }
                }
                int edad = Convert.ToInt32(mascota["Edad"] ?? 0);
                string sexo = mascota["Sexo"]?.ToString() ?? "No definido";
                bool esterilizado = Convert.ToBoolean(mascota["Esterilizado"]);
                string color = mascota["Color"]?.ToString() ?? "";
                string chip = mascota["Chip"]?.ToString() ?? "";

                // 🔹 Consultamos vacunas y tratamientos de esa mascota
                string queryVacunas = @"
                    SELECT Nombre, Fecha_Aplicacion, Proxima_Dosis 
                    FROM Vacuna 
                    WHERE Id_Mascota = @IdMascota";
                string queryTratamientos = @"
                    SELECT Nombre, Fecha_Inicio, Proximo_Control 
                    FROM Tratamiento 
                    WHERE Id_Mascota = @IdMascota";

                var paramMascota = new Dictionary<string, object> { { "@IdMascota", idMascota } };
                DataTable dtVacunas = BD.ExecuteQuery(queryVacunas, paramMascota);
                DataTable dtTratamientos = BD.ExecuteQuery(queryTratamientos, paramMascota);

                string resumenVacunas = dtVacunas.Rows.Count > 0
                    ? string.Join("; ", dtVacunas.AsEnumerable().Select(v =>
                        $"{v["Nombre"]} (última {Convert.ToDateTime(v["Fecha_Aplicacion"]):dd/MM/yyyy}, próxima {(v["Proxima_Dosis"] == DBNull.Value ? "sin definir" : Convert.ToDateTime(v["Proxima_Dosis"]).ToString("dd/MM/yyyy"))})"))
                    : "Sin vacunas registradas.";

                string resumenTratamientos = dtTratamientos.Rows.Count > 0
                    ? string.Join("; ", dtTratamientos.AsEnumerable().Select(t =>
                        $"{t["Nombre"]} (desde {Convert.ToDateTime(t["Fecha_Inicio"]):dd/MM/yyyy}, control {(t["Proximo_Control"] == DBNull.Value ? "no programado" : Convert.ToDateTime(t["Proximo_Control"]).ToString("dd/MM/yyyy"))})"))
                    : "Sin tratamientos activos.";

                // 🟩 Guardamos todo en ViewBag y en sesión
                ViewBag.Nombre = nombre;
                ViewBag.Especie = especie;
                ViewBag.Raza = raza;
                ViewBag.Peso = pesoDisplay ?? PesoHelper.FormatearPeso(pesoDecimal);
                ViewBag.PesoCalculos = pesoDecimal;
                ViewBag.Edad = edad;
                ViewBag.Sexo = sexo;
                ViewBag.Color = color;
                ViewBag.Chip = chip;
                ViewBag.Esterilizado = esterilizado ? "Sí" : "No";
                ViewBag.Vacunas = resumenVacunas;
                ViewBag.Tratamientos = resumenTratamientos;

                HttpContext.Session.SetString("MascotaDatosMedicos", JsonSerializer.Serialize(new
                {
                    nombre,
                    especie,
                    raza,
                    edad,
                    peso = pesoDisplay ?? PesoHelper.FormatearPeso(pesoDecimal),
                    sexo,
                    esterilizado,
                    color,
                    chip,
                    vacunas = resumenVacunas,
                    tratamientos = resumenTratamientos
                }));

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error ChatZooni GET: " + ex.Message);
                TempData["Error"] = "Error al cargar datos de la mascota.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnviarMensaje(string mensaje)
        {
            var historyJson = HttpContext.Session.GetString("ZooniHistory");
            ChatHistory chatHistory;

            if (string.IsNullOrEmpty(historyJson))
            {
                chatHistory = new ChatHistory();

                var datosJson = HttpContext.Session.GetString("MascotaDatosMedicos");
                var datos = string.IsNullOrEmpty(datosJson)
                    ? null
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(datosJson);

                string pesoTexto = datos?["peso"]?.ToString() ?? "0,10 kg";
                
                string sys = $"""
Eres ZooniVet, el asistente veterinario empático y profesional de la app Zooni.
Responde de forma breve, clara y directa, como si chatearas con el dueño de la mascota.
Dá siempre consejos útiles y preventivos, sin rodeos ni tecnicismos innecesarios.
Si algo puede ser grave o requiere examen físico, recomendá ir al veterinario presencial.

Información actual de la mascota:
- Nombre: {datos?["nombre"]}
- Especie: {datos?["especie"]}
- Raza: {datos?["raza"]}
- Edad: {datos?["edad"]} meses
- Peso: {pesoTexto}
- Sexo: {datos?["sexo"]}
- Vacunas: {datos?["vacunas"]}
- Tratamientos: {datos?["tratamientos"]}

CONOCIMIENTO SOBRE LA APLICACIÓN ZOONI:
Eres también un asistente de ayuda para usar la aplicación. Si el usuario pregunta cómo hacer algo en la web, podés guiarlo.

SECCIONES PRINCIPALES:
1. 🏠 Inicio (/Home/Index): Vista principal con la mascota activa, puedes cambiar entre mascotas con las flechas
2. 👥 Comunidad (/Home/Comunidad): Ver publicaciones de otros usuarios, seguir amigos, compartir contenido
3. 🛒 Marketplace (/Marketplace): Comprar productos para mascotas
4. 💉 Ficha Médica (/Home/FichaMedica): Ver y editar información médica, vacunas, tratamientos, peso
5. 📅 Calendario (/Home/Calendario): Ver eventos, recordatorios de vacunas y controles
6. 👕 Closet (/Home/Closet): Ropa y accesorios de la mascota
7. 👤 Perfil (/Home/Perfil): Ver tu perfil, publicaciones, mascotas, amigos
8. ⚙️ Configuración (/Home/Configuracion): Ajustes generales de la cuenta

GESTIÓN DE MASCOTAS:
- Agregar nueva mascota: /Registro/NuevaMascota
- Configurar mascotas: /Home/ConfigMascotas (editar, archivar, recuperar mascotas archivadas)
- Cambiar mascota activa: En el Inicio, usar las flechas laterales o ir a ConfigMascotas
- Editar datos de mascota: /Home/EditarMascota?id=[Id_Mascota]
- Archivar mascota: Desde ConfigMascotas, botón "Archivar" (no se borra, solo se oculta)
- Recuperar mascota archivada: Desde ConfigMascotas, sección "Archivadas", botón "Recuperar"

CONFIGURACIÓN Y PREFERENCIAS:
- Cambiar tema (claro/oscuro): /Home/ConfigTema
- Cambiar datos personales: /Home/Configuracion
- Actualizar contacto (email/teléfono): Desde Configuracion
- Cambiar contraseña: Desde Configuracion

FICHA MÉDICA:
- Ver ficha médica: /Home/FichaMedica
- Actualizar peso: Desde FichaMedica, sección de peso
- Agregar vacuna: Desde FichaMedica
- Agregar tratamiento: Desde FichaMedica
- Ver historial médico: Desde FichaMedica

PROVEEDORES DE SERVICIOS:
- Buscar paseadores/cuidadores: /Home/BuscarProveedores
- Registrarse como proveedor: Botón "Registrarse como Proveedor" en el login
- Los proveedores pueden marcar su zona de atención en un mapa

CHAT Y MENSAJERÍA:
- Chat con amigos: /Home/Mensajes
- Para chatear, primero agregá amigos desde Comunidad

PUBLICACIONES Y COMUNIDAD:
- Crear publicación: Desde Comunidad
- Ver publicaciones de amigos: Desde Comunidad
- Seguir usuarios: Desde Comunidad o Perfil
- Compartir historias: Desde Comunidad

CONSEJOS ÚTILES:
- Si el usuario quiere cambiar algo específico, indicá la ruta exacta (ej: "Andá a ConfigMascotas para editar tu mascota")
- Si pregunta sobre una funcionalidad, explicá brevemente cómo acceder
- Siempre combiná consejos veterinarios con ayuda sobre cómo usar la app si es relevante
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
                    ["temperature"] = 0.7,
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
                return Json(new { respuesta = $"Error al conectar con el modelo: {ex.Message}" });
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
