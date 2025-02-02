using Microsoft.AspNet.SignalR.Client;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace FrameworkWpfClient
{

    internal enum Phase { Welcome, Waiting, Playing }

    internal class GameViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly WorkerServiceProxy _workerServiceProxy;

        private string _gameId = "";
        private string _opponentName = "";

        public Phase Phase { get; private set; }

        public string PlayerName { get; set; } = "";
        public string Message1 { get; set; } = "";
        public string Message2 { get; set; } = "";
        public string Selection { get; set; } = "";
        public string Scores { get; set; } = "";
        public string OpponentMessage => $"You are playing against {_opponentName}.";

        public ICommand RegisterCommand => new DelegateCommand(_ => Register(), _ => !string.IsNullOrWhiteSpace(PlayerName));
        public ICommand ThrowCommand => new DelegateCommand(selection => ThrowHand(selection?.ToString() ?? ""));

        public GameViewModel()
        {
            _workerServiceProxy = new WorkerServiceProxy();

            _workerServiceProxy.On("WaitingForPlayer", () =>
            {
                Phase = Phase.Waiting;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Phase)));
            });

            _workerServiceProxy.On("GameStarted", (string player1, string player2, string gameId) =>
            {
                Phase = Phase.Playing;
                _gameId = gameId;

                _opponentName = player1 == PlayerName ? player2 : player1;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Phase)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OpponentMessage)));
            });

            _workerServiceProxy.On("Pending", (string waitingFor) =>
            {
                Message1 = PlayerName == waitingFor
                          ? "Your opponent has chosen ..."
                          : $"Waiting for {waitingFor}.";

                Message2 = "";

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message1)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message2)));
            });

            _workerServiceProxy.On("Drawn", (string explanation, string scores) =>
            {
                Message1 = "Drawn.";
                Message2 = $"({explanation})";
                Scores = scores;

                Selection = "";

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message1)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message2)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scores)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selection)));
            });


            _workerServiceProxy.On("Won", (string winner, string explanation, string scores) =>
            {
                Message1 = winner == PlayerName ? "You won!" : $"{winner} won.";
                Message2 = $"({explanation})";
                Scores = scores;

                Selection = "";

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message1)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message2)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scores)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selection)));
            });
        }

        public async void Connect()
        {
            _workerServiceProxy.Start();

           // await _hubConnection.Start();
        }

        private void Register()
        {
            _workerServiceProxy.Invoke("Register", PlayerName);
        }

        private void ThrowHand(string selection)
        {
            Selection = selection;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selection)));

             _workerServiceProxy.Invoke("Throw", _gameId, PlayerName, selection);
        }

        public void Dispose()
        {
            ((IDisposable)_workerServiceProxy).Dispose();
        }
    }
}