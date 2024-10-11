using System.Net.Http.Headers;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("estatemanner", httpClient =>
{
    httpClient.BaseAddress = new Uri("https://app.estatemanner.com/");
    httpClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjbGllbnR8aVdjWWlrdHB2c3MxS0p4T0xyY2RBUEJEM2poVUtnOWYiLCJjbGllbnRfbWV0YWRhdGEiOnsidHlwZSI6InF1YW50aXR5IiwibGltaXQiOjEzMDAwfSwiaWF0IjoxNzI3MTQ2NzY1LCJleHAiOjE3MjcxNjgzNjUsImF1ZCI6ImFwcC5lc3RhdGVtYW5uZXIuY29tIiwiaXNzIjoiYXBwLmVzdGF0ZW1hbm5lci5jb20ifQ.NXF9NCup6FVnvbE8wV3OmLeeNUNiyKvLPbqX0lY5wKM");
});

builder.Services.AddSingleton<ReeSoftDataProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
