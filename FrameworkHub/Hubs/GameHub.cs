using FrameworkHub.GameLogic;
using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;


namespace FrameworkHub.Hubs
{
    public class GameHub : Hub
    {
        private static readonly GameManager _manager = new GameManager();

        public async Task Register(string name)
        {
            var group = _manager.Register(name);

            await Groups.Add(Context.ConnectionId, group.Name);

            if (group.Full)
                await Clients.Group(group.Name).GameStarted(group.Game.Player1.Name, group.Game.Player2.Name, group.Name);
            else
                await Clients.Caller.WaitingForPlayer();
        }

        public async Task Throw(string groupName, string player, string selection)
        {
            var game = _manager.Throw(groupName, player, (Sign)Enum.Parse(typeof(Sign), selection, true));
            
            if (game.Pending)
                await Clients.Group(groupName).Pending(game.WaitingFor);
            else
            {
                var winner = game.Winner;
                var explanation = game.Explanation;

                game.Reset();

                if (winner == null)
                    await Clients.Group(groupName).Drawn(explanation, game.Scores);
                else
                    await Clients.Group(groupName).Won(winner, explanation, game.Scores);
            }
        }
    }
}
