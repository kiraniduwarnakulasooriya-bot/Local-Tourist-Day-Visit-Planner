using LocalTouristPlanner.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<DatabaseContext>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.Run();