namespace ScrumPoker.Models
{
    public class Player
    {
        public string NickName { get; set; }

        public string Vote { get; set; }

        public string Role { get; set; }

        internal string ConnectionId { get; set; }
    }
}