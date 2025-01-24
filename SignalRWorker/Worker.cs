using System.IO.Pipes;

namespace SignalRWorker
{
    public class Worker : BackgroundService
    {
        private readonly AnonymousPipeClientStream _pipeOut;
        private readonly AnonymousPipeClientStream _pipeIn;

        private StreamReader _streamIn;
        private StreamWriter _streamOut;

        public Worker(string inPipe, string outPipe)
        {
            _pipeOut = new AnonymousPipeClientStream(PipeDirection.Out, outPipe);
            _pipeIn = new AnonymousPipeClientStream(PipeDirection.In, inPipe);

            _streamOut = new StreamWriter(_pipeOut) { AutoFlush = true };
            _streamIn = new StreamReader(_pipeIn);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = _streamIn.ReadLine();

                await Task.Delay(1000, stoppingToken);

                _streamOut.WriteLine($"You just sent me the message {message}");
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
