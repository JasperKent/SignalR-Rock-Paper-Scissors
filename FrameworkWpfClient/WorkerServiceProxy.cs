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
        private NamedPipeClientStream _pipeStream;

        private StreamReader _streamIn;
        private StreamWriter _streamOut;

        private readonly Dictionary<string, Action<object[]>> _subscriptions = new Dictionary<string, Action<object[]>>();

        public void Dispose()
        {
            _streamIn.Dispose();
            _streamOut.Dispose();
            _pipeStream.Dispose();
        }

        public void Start()
        {
            _pipeStream = new NamedPipeClientStream(".", "RPSPipe", PipeDirection.InOut, PipeOptions.Asynchronous);

            _pipeStream.Connect();

            _streamIn = new StreamReader(_pipeStream);
            _streamOut = new StreamWriter(_pipeStream) { AutoFlush = true };

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
