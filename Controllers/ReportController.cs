using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using internshipPartTwo.Models;

namespace internshipPartTwo.Controllers
{
    public class ReportController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //gets the number of client using count per type 
        [HttpGet]
        public JsonResult GetClientsByType(int? typeFilter = null)
        {
            var clientCounts = new List<ClientTypeCount>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
            SELECT 
                CASE Type
                    WHEN 0 THEN 'Individual'
                    WHEN 1 THEN 'Organization'
                END AS TypeName,
                COUNT(*) as ClientCount
            FROM Clients
            WHERE @TypeFilter IS NULL OR Type = @TypeFilter
            GROUP BY Type"; // Group by original enum value

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TypeFilter", (object)typeFilter ?? DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clientCounts.Add(new ClientTypeCount
                            {
                                TypeName = reader["TypeName"].ToString(),
                                Count = (int)reader["ClientCount"]
                            });
                        }
                    }
                }
            }
            return Json(clientCounts, JsonRequestBehavior.AllowGet);
        }
       

// Report 2: Number of Reserved/Unreserved Phone Numbers per device
[HttpGet]
        public JsonResult GetPhoneNumbersByDeviceAndStatus(int? deviceId = null, bool? isReserved = null)
        {
            var phoneNumberStats = new List<PhoneNumberStatistic>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                //  Get reserved phone ids and check if end date is null if its not null then the number is unreserved 
                var reservedPhoneIds = new HashSet<int>();
                using (var reservedCmd = new SqlCommand(@"
            SELECT DISTINCT PhoneNumberId 
            FROM PhoneNumberReservations 
            WHERE BED <= GETDATE() 
            AND (EED IS NULL OR EED >= GETDATE())", conn))
                {
                    using (var reader = reservedCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reservedPhoneIds.Add((int)reader["PhoneNumberId"]);
                        }
                    }
                }

                // Step 2: Get all phone numbers with device info
                string query = @"
            SELECT 
                p.Id AS PhoneNumberId,
                d.Id AS DeviceId,
                d.Name AS DeviceName
            FROM PhoneNumbers p
            INNER JOIN Devices d ON p.DeviceId = d.Id
            WHERE (@DeviceId IS NULL OR d.Id = @DeviceId)";

                var allPhones = new List<(int PhoneId, int DeviceId, string DeviceName)>();

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DeviceId", (object)deviceId ?? DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allPhones.Add((
                                (int)reader["PhoneNumberId"],
                                (int)reader["DeviceId"],
                                reader["DeviceName"].ToString()
                            ));
                        }
                    }
                }

                // Step 3: Group and count based on reservation status, filter out 0-counts
                var grouped = allPhones
                    .GroupBy(p => new { p.DeviceId, p.DeviceName })//group them by device
                    .SelectMany(g =>
                    {
                        int reservedCount = g.Count(p => reservedPhoneIds.Contains(p.PhoneId));
                        int unreservedCount = g.Count() - reservedCount;

                        var stats = new List<PhoneNumberStatistic>();

                        if ((!isReserved.HasValue || isReserved.Value) && reservedCount > 0)
                        {
                            stats.Add(new PhoneNumberStatistic
                            {
                                DeviceId = g.Key.DeviceId,
                                DeviceName = g.Key.DeviceName,
                                Status = "Reserved",
                                Count = reservedCount
                            });
                        }

                        if ((!isReserved.HasValue || !isReserved.Value) && unreservedCount > 0)
                        {
                            stats.Add(new PhoneNumberStatistic
                            {
                                DeviceId = g.Key.DeviceId,
                                DeviceName = g.Key.DeviceName,
                                Status = "Unreserved",
                                Count = unreservedCount
                            });
                        }

                        return stats;
                    })
                    .ToList();

                phoneNumberStats.AddRange(grouped);
            }

            return Json(phoneNumberStats, JsonRequestBehavior.AllowGet);
        }
        // Add this method to your ReportController class
        [HttpGet]
        public JsonResult GetClientDetails(int? typeFilter = null)
        {
            var clients = new List<Client>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
        SELECT 
            Id,
            Name,
            BirthDate,
            Type
        FROM Clients
        WHERE @TypeFilter IS NULL OR Type = @TypeFilter";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TypeFilter", (object)typeFilter ?? DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(new Client
                            {
                                Id = (int)reader["Id"],
                                Name = reader["Name"].ToString(),
                                Type = (ClientType)reader["Type"], // get the type of the client
                                BirthDate = reader["BirthDate"] != DBNull.Value
                                    ? DateTime.SpecifyKind((DateTime)reader["BirthDate"], DateTimeKind.Local)
                                    : (DateTime?)null
                            });
                        }
                    }
                }
            }

            // Custom JSON serialization to handle date formatting
            return Json(clients.Select(c => new
            {
                c.Id,
                c.Name,
                TypeName = c.Type.ToString(), // Convert enum to string
                BirthDate = c.BirthDate.HasValue
                    ? c.BirthDate.Value.ToString("yyyy-MM-dd") //  date format 
                    : null
            }), JsonRequestBehavior.AllowGet);
        }
    }
}

    