namespace ScrumPoker.Models
{
    using System.Collections.Generic;

    public class Room
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Story CurrentStory { get; set; }

        public GameConfiguration Config { get; set; }

        public ICollection<Story> Stories { get; set; }

        public ICollection<Player> Players { get; set; }
    }
}