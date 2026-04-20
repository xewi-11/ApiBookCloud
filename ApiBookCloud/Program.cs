using ApiBookCloud.Data;
using ApiBookCloud.Repositories;
using ApiOAuthEmpleados.Helpers;
using Azure.Storage.Blobs;
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
