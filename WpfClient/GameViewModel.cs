using Microsoft.AspNetCore.SignalR.Client;
using System.ComponentModel;
using System.Windows.Input;

namespace WpfClient;

internal enum Phase { Welcome, Waiting, Playing }

internal class GameViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly HubConnection _hubConnection;

    private string _gameId = "";
    private string _opponentName = "";

    public Phase Phase { get; private set; }

    public string PlayerName { get; set; } = "";
    public string Message1 { get; set; } = "";
    public string Message2 { get; set; } = "";
    public string Selection { get; set; } = "";
    public string Scores { get; set; } = "";
    public string OpponentMessage => $"You are playing against {_opponentName}.";

    public ICommand RegisterCommand => new DelegateCommand(_ =>  Register(), _ => !string.IsNullOrWhiteSpace(PlayerName));
    public ICommand ThrowCommand => new DelegateCommand(selection => ThrowHand(selection?.ToString() ?? ""));

    public GameViewModel()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7234/gameHub")
            .Build();

        _hubConnection.On("WaitingForPlayer", () =>
        {
            Phase = Phase.Waiting;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Phase)));
        });

        _hubConnection.On("GameStarted", (string player1, string player2, string gameId) =>
        {
            Phase = Phase.Playing;
            _gameId = gameId;

            _opponentName = player1 == PlayerName ? player2 : player1;
   
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Phase)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(OpponentMessage)));
        });

        _hubConnection.On("Pending", (string waitingFor) =>
        {
            Message1 = PlayerName == waitingFor
                      ? "Your opponent has chosen ..."
                      : $"Waiting for {waitingFor}.";

            Message2 = "";

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message1)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message2)));
        });

        _hubConnection.On("Drawn", (string explanation, string scores) =>
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


        _hubConnection.On("Won", (string winner, string explanation, string scores) =>
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
        await _hubConnection.StartAsync();
    }

    private async void Register()
    {
        await _hubConnection.SendAsync("Register", PlayerName);
    }

    private async void ThrowHand(string selection)
    {
        Selection = selection;

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Selection)));

        await _hubConnection.SendAsync("Throw", _gameId, PlayerName, selection);
    }
}
