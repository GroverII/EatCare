using Microsoft.EntityFrameworkCore;
using EatCareService.Data;
using CloudinaryDotNet;
using dotenv.net;
using EatCareService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), new MySqlServerVersion(new Version(8, 0, 28))));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Загрузите конфигурацию Cloudinary
DotEnv.Load(options: new DotEnvOptions(probeForEnv: true));
builder.Services.AddSingleton<Cloudinary>(_ => new Cloudinary("cloudinary://964566126474851:bZ38Wq_zi_LlVdrE5uPoVRuA2_c@dafuaprrm"));
builder.Services.AddScoped<IEatCareServices, EatCareServices>();
builder.Services.AddScoped<IDataService, DataService>();

var app = builder.Build();

// Если приложение находится в режиме разработки, добавим Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// MapControllers and pass cloudinary instance to DataController
app.MapControllers();

app.Run();
