using ConfigEditor.Server.Services;
using ConfigEditor.Shared.Data;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);

// register services
builder.Services.AddGrpc();
builder.Services.AddDbContext<ConfigDbContext>();

var app = builder.Build();

// initialize and seed db
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ConfigDbContext>();
    await db.Database.EnsureCreatedAsync();
    await db.SeedAsync();
    Console.WriteLine($"Database: {db.DbPath}");
}

// map gRPC services
app.MapGrpcService<GameConfigGrpcService>();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine(@"
  |--------------------------------------|
  |  Game Config Editor — gRPC Server    |
  |   Listening on http://localhost:5080 |
  |--------------------------------------|
");
Console.ResetColor();

app.Run("http://localhost:5080");
