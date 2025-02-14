using Microsoft.AspNetCore.SignalR.Client;
using System.ComponentModel;
using System.IO.Pipes;
using System.Numerics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;

namespace SignalRWorker
{
    public class Worker : BackgroundService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private readonly NamedPipeServerStream _pipeStream;

        private StreamReader? _streamIn;
        private StreamWriter? _streamOut;

        private readonly HubConnection _hubConnection;

        public Worker() 
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);

            var pipeSecurity = new PipeSecurity();

            pipeSecurity.SetAccessRule(new PipeAccessRule(sid, PipeAccessRights.FullControl, AccessControlType.Allow));

            _pipeStream = NamedPipeServerStreamAcl.Create("RPSPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0, 0, pipeSecurity);

            _hubConnection = new HubConnectionBuilder()
               .WithUrl("https://localhost:7234/gameHub", options =>
               {
                   options.HttpMessageHandlerFactory = message =>
                   {
                       if (message is HttpClientHandler clientHandler)
                           clientHandler.ServerCertificateCustomValidationCallback += (_, _, _, _) => true;

                       return message;
                   };
               })
               .Build();
        }

        private async Task Configure()
        {
            await _pipeStream.WaitForConnectionAsync();

            _streamOut = new StreamWriter(_pipeStream) { AutoFlush = true };
            _streamIn = new StreamReader(_pipeStream);

            _hubConnection.On("WaitingForPlayer", () =>
            {
                _streamOut.WriteLine(JsonSerializer.Serialize(new { Method = "WaitingForPlayer", Args = Array.Empty<string>() }));
            });

            _hubConnection.On("GameStarted", (string player1, string player2, string gameId) =>
            {
                _streamOut.WriteLine(JsonSerializer.Serialize(new
                {
                    Method = "GameStarted",
                    Args = new string[] { player1, player2, gameId }
                }));
            });

            _hubConnection.On("Pending", (string waitingFor) =>
            {
                _streamOut.WriteLine(JsonSerializer.Serialize(new
                {
                    Method = "Pending",
                    Args = new string[] { waitingFor }
                }));
            });

            _hubConnection.On("Drawn", (string explanation, string scores) =>
            {
                _streamOut.WriteLine(JsonSerializer.Serialize(new
                {
                    Method = "Drawn",
                    Args = new string[] { explanation, scores }
                }));
            });

            _hubConnection.On("Won", (string winner, string explanation, string scores) =>
            {
                _streamOut.WriteLine(JsonSerializer.Serialize(new
                {
                    Method = "Won",
                    Args = new string[] { winner, explanation, scores }
                }));
            });

            await _hubConnection.StartAsync();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Configure();

            while (!stoppingToken.IsCancellationRequested)
            {
                var message = _streamIn?.ReadLine();

                if (message == null)
                    continue;

                var request = JsonSerializer.Deserialize<Request>(message, _jsonOptions);

                if (request == null)
                    continue;

                await _hubConnection.SendCoreAsync(request.Method, request.Args, stoppingToken);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _streamOut?.Dispose();
            _streamIn?.Dispose();
            _pipeStream.Dispose();
        }
    }
}
