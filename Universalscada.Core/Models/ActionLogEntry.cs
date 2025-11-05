// Universalscada.core/Models/ActionLogEntry.cs
using System;

namespace Universalscada.core.Models
{
    public class ActionLogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string ActionType { get; set; }
        public string Details { get; set; }
    }
}