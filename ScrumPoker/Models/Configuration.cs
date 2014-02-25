namespace ScrumPoker.Models
{
    using System.Collections.Generic;

    public class GameConfiguration
    {
        public int Id { get; set; }

        public virtual ICollection<CardValue> CardValues { get; set; }

        public bool AutoReveal { get; set; }

        public bool PreventVoteAfterReveal { get; set; }

        public bool HighlightHigh { get; set; }

        public bool HighlightLow { get; set; }
    }
}