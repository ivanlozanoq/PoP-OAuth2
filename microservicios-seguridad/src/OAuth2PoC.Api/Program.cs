using OAuth2PoC.Application.Extensions;
using OAuth2PoC.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configuración de servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "OAuth2 PoC API", 
        Version = "v1",
        Description = "Proof of Concept for OAuth2 authentication implementation"
    });
});

// Registrar servicios de las capas
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices();

// Configuración de CORS para desarrollo
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configuración del pipeline de HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OAuth2 PoC API v1");
    c.RoutePrefix = "swagger"; // Swagger UI en /swagger
});

if (app.Environment.IsDevelopment())
{
    app.UseCors("Development");
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
