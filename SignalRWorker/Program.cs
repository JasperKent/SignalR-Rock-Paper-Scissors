using SignalRWorker;
using System.Diagnostics;

Debugger.Launch();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "SignalRService";
});
builder.Services.AddHostedService<Worker>(); 

var host = builder.Build();
host.Run();
