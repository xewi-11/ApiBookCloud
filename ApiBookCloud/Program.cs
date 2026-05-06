using ApiBookCloud.Data;
using ApiBookCloud.Repositories;
using ApiOAuthEmpleados.Helpers;
using Azure.Storage.Blobs;
using BookCloud.Repositories;
using Microsoft.EntityFrameworkCore;
using MvcCoreAzureStorage.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<BookCloudContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AzureDb")));

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

string azureStorageConnectionString = builder.Configuration.GetValue<string>("AzureStorage:ConnectionString")
    ?? throw new ArgumentNullException("AzureStorage:ConnectionString", "Debes configurar AzureStorage:ConnectionString en appsettings.");

builder.Services.AddSingleton(new BlobServiceClient(azureStorageConnectionString));
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





//using ApiBookCloud.Data;
//using ApiBookCloud.Repositories;
//using ApiOAuthEmpleados.Helpers;
//using Azure.Identity; 
//using Azure.Storage.Blobs;
//using BookCloud.Repositories;
//using Microsoft.EntityFrameworkCore;
//using MvcCoreAzureStorage.Services;
//using Scalar.AspNetCore;

//var builder = WebApplication.CreateBuilder(args);

//// --- INICIO CONFIGURACIÓN DE KEY VAULT ---
//var vaultUri = builder.Configuration["Secrets:VaultUri"];

//if (!string.IsNullOrEmpty(vaultUri))
//{
//    builder.Configuration.AddAzureKeyVault(
//        new Uri(vaultUri),
//        new DefaultAzureCredential());
//}
//// --- FIN CONFIGURACIÓN DE KEY VAULT ---

//// --- EXTRACCIÓN DE VARIABLES DEL KEY VAULT (Y APPSETTINGS) ---
//// La base de datos es especial y se usa GetConnectionString
//string azureDb = builder.Configuration.GetConnectionString("AzureDb")
//    ?? throw new ArgumentNullException("ConnectionStrings:AzureDb");

//// El resto usan la jerarquía de dos puntos (:)
//string apiOAuthSecretKey = builder.Configuration["ApiOAuthToken:SecretKey"];

//string azureStorageConnectionString = builder.Configuration["AzureStorage:ConnectionString"]
//    ?? throw new ArgumentNullException("AzureStorage:ConnectionString");
//string azureStorageContainerName = builder.Configuration["AzureStorage:ContainerName"];

//string cypherKey = builder.Configuration["Cypher:Key"];

//string stripePublishableKey = builder.Configuration["Stripe:PublishableKey"];
//string stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
//string stripeWebhookSecret = builder.Configuration["Stripe:WebhookSecret"];
//// -------------------------------------------------------------

//// Add services to the container.
//builder.Services.AddControllers();
//builder.Services.AddOpenApi();

//// Usamos la variable que extrajimos arriba
//builder.Services.AddDbContext<BookCloudContext>(options =>
//    options.UseSqlServer(azureDb));

//HelperCryptography.Initialize(builder.Configuration);

//HelperActionOAuthService oauthHelper = new HelperActionOAuthService(builder.Configuration);
//builder.Services.AddSingleton(oauthHelper);
//builder.Services.AddScoped<RepositoryUsuarios>();
//builder.Services.AddScoped<RepositoryPedidos>();
//builder.Services.AddScoped<RepositoryFavoritos>();
//builder.Services.AddScoped<RepositoryWallet>();
//builder.Services.AddScoped<RepositoryPagos>();
//builder.Services.AddScoped<RepositoryChats>();
//builder.Services.AddScoped<RepositoryLibros>();
//builder.Services.AddHttpContextAccessor();
//builder.Services.AddScoped<HelperUsuarioToken>();

//// Usamos la variable de la cadena de conexión de Storage que extrajimos arriba
//builder.Services.AddSingleton(new BlobServiceClient(azureStorageConnectionString));
//builder.Services.AddScoped<RepositoryStorageBlobs>();

//builder.Services.AddAuthentication(oauthHelper.GetAuthenticationSchema())
//    .AddJwtBearer(oauthHelper.GetJWtBearerOptions());

//builder.Services.AddAuthorization();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{

//}
//app.MapOpenApi();
//app.MapScalarApiReference();
//app.MapGet("/", context =>
//{
//    context.Response.Redirect("/scalar");
//    return Task.CompletedTask;
//});

//app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();
