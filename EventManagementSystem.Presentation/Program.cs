using EventManagementSystem.Application.Commands.CreateEvent;
using EventManagementSystem.Domain.Repositories;
using EventManagementSystem.Infrastructure.Persistence;
using EventManagementSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
// Pastikan kamu menambahkan using untuk letak class Repository kamu nanti
// using EventManagementSystem.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// --- 1. REGISTRASI DATABASE ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 2. REGISTRASI CONTROLLER ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// --- 3. REGISTRASI MEDIATR (WAJIB UNTUK CQRS) ---
builder.Services.AddMediatR(cfg => {
    // Ini memberitahu MediatR untuk mencari semua Handler yang ada di project Application
    cfg.RegisterServicesFromAssembly(typeof(CreateEventCommand).Assembly);
});

// --- 4. REGISTRASI DEPENDENCY INJECTION (DI) REPOSITORY ---

//builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<IRefundRepository, RefundRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// --- 5. MAPPING ROUTE CONTROLLER (SOLUSI ERROR 404) ---
app.MapControllers();

// (Bagian WeatherForecast bawaan saya biarkan agar tidak merusak template aslimu)
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}