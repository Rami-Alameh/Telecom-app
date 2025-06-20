using System;
using System.Collections.Generic;

namespace internshipPartTwo.Models
{
    public class ClientTypeCount
    {
        public ClientType Type { get; set; }
        public string TypeName { get; set; }
        public int Count { get; set; }
    
    }

    public class PhoneNumberStatistic
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string Status { get; set; } // Reserved or Unreserved
        public int Count { get; set; }
    }
}