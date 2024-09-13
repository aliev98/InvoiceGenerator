using InvoiceGenerator.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Design;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var allowedOrigins = new[] { "https://localhost:5173", "https://localhost:7223" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});


//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAllOrigins",
//        builder => builder.AllowAnyOrigin()
//                          .AllowAnyHeader()
//                          .AllowAnyMethod());
//});


builder.Services.AddControllers();
builder.Services.AddDbContext<InvoiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();


//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//});

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("CorsPolicy");

//app.UseCors(options =>
//{
//    options.AllowAnyOrigin()
//           .AllowAnyMethod()
//           .AllowAnyHeader();
//});

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();