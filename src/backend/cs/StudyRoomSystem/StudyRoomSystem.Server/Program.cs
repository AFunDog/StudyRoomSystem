var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// warn: Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionMiddleware[3]
// Failed to determine the https port for redirect.
// app.UseHttpsRedirection();


app.Run();