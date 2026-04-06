using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Register services with dependency injection
// Note: We register them without circular dependencies and wire them up after
builder.Services.AddSingleton<IValidationService, ValidationService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IEventService, EventService>();

// Build the app
var app = builder.Build();

// Wire up the services to resolve circular dependency
var userService = app.Services.GetRequiredService<IUserService>() as UserService;
var eventService = app.Services.GetRequiredService<IEventService>() as EventService;

if (userService != null && eventService != null)
{
    userService.SetEventService(eventService);
    eventService.SetUserService(userService);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

