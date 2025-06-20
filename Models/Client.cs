using System;

namespace internshipPartTwo.Models
{
    public enum ClientType
    {
        Individual = 0,
        Organization = 1
    }

    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ClientType Type { get; set; }
        public DateTime? BirthDate { get; set; } // Nullable for Organizations
    }
}
