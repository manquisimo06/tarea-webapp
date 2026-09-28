using WebEmpresa.Components;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient para comunicarse con la API
builder.Services.AddHttpClient("ApiEmpresa", client =>
{
    client.BaseAddress = new Uri("https://localhost:5125/");
});

// También dejamos HttpClient disponible mediante inyección directa
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>()
      .CreateClient("ApiEmpresa"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();