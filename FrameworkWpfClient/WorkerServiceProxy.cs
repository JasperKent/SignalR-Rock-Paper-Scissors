using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        private readonly Dictionary<string, Action<object[]>> _subscriptions = new Dictionary<string, Action<object[]>>();

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
            _workerProcess.StartInfo.CreateNoWindow = true;

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

                        var response = JsonSerializer.Deserialize<Response>(message);

                        _subscriptions[response.Method](response.Args);
                    }
                }
                catch
                {
                }
            });
        }

        internal void On(string eventName, Action onData)
        {
            _subscriptions[eventName] = _ => onData();
        }

        internal void On<T1>(string eventName, Action<T1> onData)
        {
            _subscriptions[eventName] = args => onData((T1)args[0]);
        }

        internal void On<T1,T2>(string eventName, Action<T1,T2> onData)
        {
            _subscriptions[eventName] = args => onData((T1)args[0], (T2)args[1]);
        }

        internal void On<T1,T2,T3>(string eventName, Action<T1,T2,T3> onData)
        {
            _subscriptions[eventName] = args => onData((T1)args[0], (T2)args[1], (T3)args[2]);
        }

        internal void Invoke(string method, params string[] args)
        {
            _streamOut.WriteLine(JsonSerializer.Serialize(new {method, args}));
        }
    }
}
