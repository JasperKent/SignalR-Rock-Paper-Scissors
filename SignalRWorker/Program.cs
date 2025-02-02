using SignalRWorker;
using System.Diagnostics;

//Debugger.Launch();

var inPipe = args[0];
var outPipe = args[1];

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService(_ => new Worker(inPipe, outPipe));

var host = builder.Build();
host.Run();
