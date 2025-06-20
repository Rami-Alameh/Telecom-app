using System;
using System.ComponentModel.DataAnnotations;

namespace internshipPartTwo.Models
{
    public class PhoneNumberReservation
    {
        [Key]  // Ensures Entity Framework recognizes it as primary key
        public int Id { get; set; }

        public int ClientId { get; set; }
        public int PhoneNumberId { get; set; }
        public DateTime BED { get; set; } // Begin Effective Date
        public DateTime? EED { get; set; } // End Effective Date (nullable)

        // Navigation properties for display purposes
        public string ClientName { get; set; }  // Not stored in DB
        public string PhoneNumber { get; set; } // Not stored in DB
    }
}
