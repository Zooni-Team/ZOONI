using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Kernel + Gemini
builder.Services.AddSingleton<Kernel>(provider =>
{
    var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    var apiKey = config["GoogleAI:ApiKey"] ?? throw new InvalidOperationException("Falta API Key");
    var modelId = config["GoogleAI:ModelId"] ?? "gemini-2.0-flash";

    var kernelBuilder = Kernel.CreateBuilder();

#pragma warning disable SKEXP0070
    kernelBuilder.AddGoogleAIGeminiChatCompletion(modelId: modelId, apiKey: apiKey);
#pragma warning restore SKEXP0070

    return kernelBuilder.Build();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Home/Error404", "?code={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
