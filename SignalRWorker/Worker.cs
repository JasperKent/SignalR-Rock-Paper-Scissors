using Microsoft.AspNetCore.SignalR.Client;
using System.ComponentModel;
using System.IO.Pipes;
using System.Numerics;
using System.Text.Json;

namespace SignalRWorker
{
    public class Worker : BackgroundService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private readonly AnonymousPipeClientStream _pipeOut;
        private readonly AnonymousPipeClientStream _pipeIn;

        private StreamReader _streamIn;
        private StreamWriter _streamOut;

        private readonly HubConnection _hubConnection;

        public Worker(string inPipe, string outPipe)
        {
            _pipeOut = new AnonymousPipeClientStream(PipeDirection.Out, outPipe);
            _pipeIn = new AnonymousPipeClientStream(PipeDirection.In, inPipe);

            _streamOut = new StreamWriter(_pipeOut) { AutoFlush = true };
            _streamIn = new StreamReader(_pipeIn);

            _hubConnection = new HubConnectionBuilder()
               .WithUrl("https://localhost:7234/gameHub")
               .Build();

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
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _hubConnection.StartAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                var message = _streamIn.ReadLine();

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

            _pipeOut.Dispose();
            _pipeIn.Dispose();
            _streamOut.Dispose();
            _streamIn.Dispose();
        }
    }
}
