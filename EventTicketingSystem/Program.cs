using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency injection
builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IVenueService, InMemoryVenueService>();
builder.Services.AddSingleton<IEventService, InMemoryEventService>();
builder.Services.AddSingleton<ITicketService, InMemoryTicketService>();
builder.Services.AddSingleton(typeof(ICache<,>), typeof(InMemoryCache<,>));


builder.Logging.ClearProviders(); 
builder.Logging.AddConsole();     
builder.Logging.AddDebug();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EventTicketingSystem API", Version = "v1" });
});

var app = builder.Build();
app.MapControllers(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventTicketingSystem API V1");
    });
}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
     app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();


