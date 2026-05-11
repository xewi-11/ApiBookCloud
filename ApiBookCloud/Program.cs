using ApiBookCloud.Data;
using ApiBookCloud.Repositories;
using ApiOAuthEmpleados.Helpers;
using Azure.Storage.Blobs;
using BookCloud.Repositories;
using Microsoft.EntityFrameworkCore;
using MvcCoreAzureStorage.Services;
using Scalar.AspNetCore;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Azure;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var vaultUri = builder.Configuration["Secrets:VaultUri"];

// 1. Configuramos AddAzureKeyVault para inyectar automáticamente al IConfiguration.
// Esto es MUCHISIMO más estable para producción porque gestiona reintentos y cachés.
if (!string.IsNullOrEmpty(vaultUri))
{
    try 
    {
        builder.Configuration.AddAzureKeyVault(
            new Uri(vaultUri),
            new DefaultAzureCredential());
    }
    catch(Exception ex)
    {
        Debug.WriteLine($"Error conectando a Key Vault: {ex.Message}");
        // No detenemos el arranque inmediatamente, permitimos que falle controladamente luego si falta info clave.
    }
}

// 2. Registramos el cliente por si otras partes (como AzureClients) lo requieren.
builder.Services.AddAzureClients(factory =>
{
    factory.AddSecretClient(new Uri(vaultUri!));
});

// 3. Obtenemos las dependencias a través del IConfiguration que ya posee los datos bajados de Azure.
builder.Services.AddDbContext<BookCloudContext>((serviceProvider, options) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    // Usará el valor del KV automáticamente sustituyendo los ':' por '--' transparentemente
    var connectionString = configuration.GetConnectionString("AzureDb"); 
    options.UseSqlServer(connectionString);
});

HelperCryptography.Initialize(builder.Configuration);

HelperActionOAuthService oauthHelper = new HelperActionOAuthService(builder.Configuration);
builder.Services.AddSingleton(oauthHelper);
builder.Services.AddScoped<RepositoryUsuarios>();
builder.Services.AddScoped<RepositoryPedidos>();
builder.Services.AddScoped<RepositoryFavoritos>();
builder.Services.AddScoped<RepositoryWallet>();
builder.Services.AddScoped<RepositoryPagos>();
builder.Services.AddScoped<RepositoryChats>();
builder.Services.AddScoped<RepositoryLibros>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<HelperUsuarioToken>();

builder.Services.AddSingleton(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var storageSecret = configuration["AzureStorage:ConnectionString"];
    return new BlobServiceClient(storageSecret);
});
builder.Services.AddScoped<RepositoryStorageBlobs>();

builder.Services.AddAuthentication(oauthHelper.GetAuthenticationSchema())
    .AddJwtBearer(oauthHelper.GetJWtBearerOptions());

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{

}
app.MapOpenApi();
app.MapScalarApiReference();
app.MapGet("/", context =>
{
    context.Response.Redirect("/scalar");
    return Task.CompletedTask;
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
