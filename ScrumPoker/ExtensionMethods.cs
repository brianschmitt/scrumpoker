using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ScrumPoker
{
    using ScrumPoker.Models;

    public static class ExtensionMethods
    {
        public static bool AllVoted(this Room room)
        {
            return room.Players.Where(p => p.Role == "Player").All(p => !string.IsNullOrWhiteSpace(p.Vote));
        }

        public static string Truncate(this string value, int length)
        {
            if (value == null)
                throw new ArgumentNullException("value");
            value = value.Trim();
            return value.Length <= length ? value : value.Substring(0, length);
        }
    }
}