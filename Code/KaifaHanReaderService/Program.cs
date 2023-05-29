// See https://aka.ms/new-console-template for more information
Console.WriteLine("Starting....");
var builder = new HostBuilder();
builder.ConfigureServices(services => { });

var app = builder.Build();

app.Run();