using AssignmentManagement.Api.Extensions;
using AssignmentManagement.Api.Middleware;
using AssignmentManagement.Api.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

const string ClientOrigin = "ClientOrigin";
builder.Services.AddCors(options =>
{
    options.AddPolicy(ClientOrigin, policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiServices();
builder.Services.AddJwtAuthentication(builder.Configuration);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DbInitializationService>();
}

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(ClientOrigin);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
