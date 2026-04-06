using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Register services with dependency injection
builder.Services.AddSingleton<IValidationService, ValidationService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IEventService, EventService>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapOpenApi();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

