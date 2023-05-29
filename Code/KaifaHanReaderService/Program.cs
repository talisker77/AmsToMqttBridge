// See https://aka.ms/new-console-template for more information
using KaifaHanReaderService.Services;

Console.WriteLine("Starting....");
var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureLogging((logging) =>
{
    logging.AddConsole();
    logging.AddDebug();
    logging.AddSystemdConsole();
    //logging.Configure(f=>{ f.d})
    logging.AddSimpleConsole();
    logging.SetMinimumLevel(LogLevel.Trace);
});
builder.ConfigureServices(services =>
{
    services.AddHostedService<ReaderService>();

});
builder.UseSystemd();
//builder.use


var app = builder.Build();
app.Run();