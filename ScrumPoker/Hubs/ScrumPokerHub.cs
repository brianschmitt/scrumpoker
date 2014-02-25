namespace ScrumPoker.Hubs
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNet.SignalR;
    using ScrumPoker.Models;

    public class ScrumPokerHub : Hub
    {
        private static readonly List<Room> Rooms = new List<Room>();

        private readonly List<CardValue> defaultCardValues = new List<CardValue>
                                                                 {
                                                                     new CardValue { Value = "1" },
                                                                     new CardValue { Value = "2" },
                                                                     new CardValue { Value = "3" },
                                                                     new CardValue { Value = "5" },
                                                                     new CardValue { Value = "8" },
                                                                     new CardValue { Value = "11" },
                                                                     new CardValue { Value = "20" },
                                                                     new CardValue { Value = "50" },
                                                                     new CardValue { Value = "99" },
                                                                     new CardValue { Value = "?" }
                                                                 };

        public void JoinGame(string roomName, string nickName, string playerType)
        {
            var room = GetRoom(roomName);
            nickName = nickName.Truncate(10);
            if (string.IsNullOrWhiteSpace(nickName) || room.Players.Any(p => p.NickName == nickName))
            {
                Clients.Caller.badUsername();
                return;
            }

            playerType = playerType ?? "Player";

            var player = new Player { NickName = nickName, ConnectionId = Context.ConnectionId, Role = playerType };
            room.Players.Add(player);

            var currentGame = new
            {
                Players = room.Players.Where(p => p.Role == "Player"),
                Observers = room.Players.Where(p => p.Role == "Observer"),
                CardValues = room.Config.CardValues.Select(c => c.Value),
                Role = playerType,
                Name = room.Name
            };

            Groups.Add(Context.ConnectionId, room.Name);
            Clients.Caller.joinGame(currentGame);
            Clients.OthersInGroup(room.Name).playerAdded(player);
        }

        public void Vote(string roomName, string name, string vote)
        {
            var room = GetRoom(roomName);
            if (room.Config.PreventVoteAfterReveal && room.AllVoted())
            {
                return;
            }

            var player = room.Players.First(n => n.NickName == name);
            player.Vote = vote;
            
            if (room.Config.AutoReveal && room.AllVoted())
            {
                this.Reveal(room.Name);
            }
        }

        public void Reveal(string roomName)
        {
            var room = GetRoom(roomName);
            var results = room.Players.Select(p => new { p.NickName, p.Vote });
            Clients.Group(room.Name).gameReveal(results);
        }

        public void Reset(string roomName)
        {
            var room = GetRoom(roomName);
            room.Players.ToList().ForEach(p => p.Vote = string.Empty);
            Clients.Group(room.Name).gameReset();
        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            foreach (var r in Rooms)
            {
                var player = r.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (player != null)
                {
                    r.Players.Remove(player);
                    Clients.Group(r.Name).removePlayer(player.NickName);
                }
            }

            return base.OnDisconnected();
        }

        private Room GetRoom(string roomName)
        {
            if (string.IsNullOrWhiteSpace(roomName))
            {
                roomName = "NewRoom";
            }

            var room = Rooms.FirstOrDefault(r => r.Name == roomName);
            if (room != null) return room;

            room = new Room
            {
                Name = roomName,
                Players = new List<Player>(),
                Stories = new List<Story>(),
                Config = new GameConfiguration
                {
                    CardValues = this.defaultCardValues,
                    AutoReveal = true,
                    HighlightHigh = true,
                    HighlightLow = true
                },
                CurrentStory = new Story()
            };

            Rooms.Add(room);
            return room;
        }
    }
}