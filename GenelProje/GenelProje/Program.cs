using GenelProje.Models;
using GenelProje.Service;
using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Hangfire.Logging;
using Serilog;
using Serilog.Events;
using k8s.KubeConfigModels;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();


//Log.Logger = new LoggerConfiguration()
//          .WriteTo.Seq("http://localhost:5341")  // Seq sunucu adresini buraya ekleyin
//          .MinimumLevel.Debug()
//          .CreateLogger();

//builder.Host.UseSerilog((context,loggerconfig)=>
//loggerconfig.ReadFrom.Configuration(context.Configuration));




builder.Services.AddControllers();

var HangFireDbConnectionString = builder.Configuration.GetConnectionString("HangFireDb");
var NorthwindDbConnectionString = builder.Configuration.GetConnectionString("NorthwindContext");
//var HealthCheckDbConnectionString = builder.Configuration.GetConnectionString("HealthCheckDb");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services
//    .AddHealthChecks()
//    .AddSqlServer(HealthCheckDbConnectionString);


//HEALTHCHECKS

builder.Services
    .AddHealthChecks()
    .AddCheck<HealthCheckDeneme>("HealtCheckDeneme")

    .AddSeqPublisher(options =>
    {
        options.ApiKey = "AkOKVfQ5kCTw1S1lLEN7";
        options.Endpoint = "http://localhost:5341";
        options.DefaultInputLevel = HealthChecks.Publisher.Seq.SeqInputLevel.Information;
    }, "MS SQL Server Check")

    .AddSqlServer(
    connectionString: builder.Configuration.GetConnectionString("HealthCheckDb")!,
    //healthQuery: "SELECT 1",
    name: "MS SQL Server Check",//sql server ba�lant�s� sa�l�k kontrol� ad�
    failureStatus: HealthStatus.Unhealthy | HealthStatus.Degraded,
    tags: new string[] { "db", "sql", "sqlserver" });//sa�l�k kontrol�ne �zel ek bilgi



builder.Services
    .AddHealthChecksUI()
    .AddSqlServerStorage(connectionString: builder.Configuration.GetConnectionString("HealthCheckDb"));//servislerin durumlar�na dair verileri tutaca��m storage entegrasyonunu


builder.Services.AddDbContext<NorthwndContext>(options => options.UseSqlServer(NorthwindDbConnectionString));

//HANGFIRE

builder.Services.AddHangfire(configuration =>
{
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)//Hangfire 1.7.0 ve sonraki s�r�mleri i�in uyumluluk seviyesi
        .UseSimpleAssemblyNameTypeSerializer()//g�revleri s�ralama ve depolama i�lemleri i�in t�r bilgilerini kolayla�t�rmak i�in
        .UseRecommendedSerializerSettings()//Hangfire'�n varsay�lan ayarlar�n� kullanarak seri hale getirme i�lemlerini d�zenler
        .UseSqlServerStorage(HangFireDbConnectionString);
});

builder.Services.AddHangfireServer();


builder.Services.AddTransient<IServiceManagement, ServiceManagement>();
builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSerilogRequestLogging();//Apimize gelen http isteklerini g�nl��e kaydetmemizi sa�lar.
app.UseHttpsRedirection();

app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions()
{
    DashboardTitle = "My Dashboard",
    Authorization = new[]
    {
        new HangfireCustomBasicAuthenticationFilter()
        {
            Pass = "deneme",
            User = "deneme"
        }
    }
});

app.MapHealthChecks("/healthcheck", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse//json format�nda verileri getirmek i�in

});

//app.MapHealthChecks("/healthcheck");

//app.MapHealthChecksUI();
app.UseHealthChecksUI(options => options.UIPath = "/health-ui");

app.MapControllers();

app.Run();
