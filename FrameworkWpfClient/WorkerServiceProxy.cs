using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FrameworkWpfClient
{
    internal class WorkerServiceProxy : IDisposable
    {
        private Process _workerProcess;
        private AnonymousPipeServerStream _pipeOut;
        private AnonymousPipeServerStream _pipeIn;

        private StreamReader _streamIn;
        private StreamWriter _streamOut;

        public void Dispose()
        {
            _workerProcess.Kill();
            _workerProcess.Dispose();
            _streamIn.Dispose();
            _streamOut.Dispose();
            _pipeOut.Dispose();
            _pipeIn.Dispose();
        }

        public void Start()
        {
            _pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            _pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);

            _workerProcess = new Process();

            _workerProcess.StartInfo.FileName = @"E:\MyData\Repos\SignalR-Rock-Paper-Scissors\SignalRWorker\bin\Debug\net9.0\SignalRWorker.exe";

            _workerProcess.StartInfo.UseShellExecute = false;
            _workerProcess.StartInfo.CreateNoWindow = false;

            _workerProcess.StartInfo.Arguments = $"{_pipeOut.GetClientHandleAsString()} {_pipeIn.GetClientHandleAsString()}";

            _workerProcess.Start();

            _pipeOut.DisposeLocalCopyOfClientHandle();
            _pipeIn.DisposeLocalCopyOfClientHandle();

            _streamIn = new StreamReader(_pipeIn);
            _streamOut = new StreamWriter(_pipeOut) { AutoFlush = true };

            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        var message = _streamIn.ReadLine();

                        MessageBox.Show(message);
                    }
                }
                catch
                {
                }
            });
        }

        internal void SendMessage(string message)
        {
            _streamOut.WriteLine(message);
        }
    }
}
