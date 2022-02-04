using Microsoft.AspNetCore.SignalR;
using SignalR.GameLogic;

namespace SignalR.Hubs
{
    public class GameHub : Hub
    {
        public static GameManager _manager = new();

        public async Task Register(string name)
        {
            var group = _manager.Register(name);

            await Groups.AddToGroupAsync(Context.ConnectionId, group.Name);

            if (group.Full)
                await Clients.Group(group.Name).SendAsync("GameStarted", group.Game.Player1.Name, group.Game.Player2.Name, group.Name);
            else
                await Clients.Caller.SendAsync("WaitingForPlayer");
        }

        public async Task Throw(string groupName, string player, string selection)
        {
            var game = _manager.Throw(groupName, player, Enum.Parse<Sign>(selection, true));
            
            if (game.Pending)
                await Clients.Group(groupName).SendAsync("Pending", game.WaitingFor);
            else
            {
                var winner = game.Winner;
                var explanation = game.Explanation;

                game.Reset();

                if (winner == null)
                    await Clients.Group(groupName).SendAsync("Drawn", explanation, game.Scores);
                else
                    await Clients.Group(groupName).SendAsync("Won", winner, explanation, game.Scores);
            }
        }
    }
}
