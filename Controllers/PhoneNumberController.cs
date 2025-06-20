// Updated PhoneNumberController.cs with simple hyphen-separated validation
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using internshipPartTwo.Models;
using System.Text.RegularExpressions;

namespace internshipPartTwo.Controllers
{
    public class PhoneNumberController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpGet]
        public JsonResult GetPhoneNumbers()
        {
            var phoneNumbers = new List<PhoneNumber>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Id, Number, DeviceId FROM PhoneNumbers", conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    phoneNumbers.Add(new PhoneNumber
                    {
                        Id = (int)reader["Id"],
                        Number = reader["Number"].ToString(),
                        DeviceId = (int)reader["DeviceId"]
                    });
                }
            }

            return Json(phoneNumbers, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetAvailablePhoneNumbers()
        {
            var availablePhoneNumbers = new List<object>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT p.Id, p.Number, d.Name AS DeviceName
                    FROM PhoneNumbers p
                    INNER JOIN Devices d ON p.DeviceId = d.Id
                    WHERE p.Id NOT IN (
                        SELECT PhoneNumberId 
                        FROM PhoneNumberReservations 
                        WHERE (EED IS NULL OR EED >= GETDATE()) 
                        AND BED <= GETDATE()
                    )";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        availablePhoneNumbers.Add(new
                        {
                            Id = (int)reader["Id"],
                            Number = reader["Number"].ToString(),
                            DeviceName = reader["DeviceName"].ToString()
                        });
                    }
                }
            }

            return Json(availablePhoneNumbers, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddPhoneNumber(PhoneNumber phoneNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phoneNumber.Number) || phoneNumber.DeviceId <= 0)
                {
                    return Json(new { success = false, message = "Invalid phone number data." });
                }

                var regex = new Regex("-+"); // Must include at least one dash
                if (!regex.IsMatch(phoneNumber.Number))
                {
                    return Json(new { success = false, message = "Phone number must separated parts." });
                }

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO PhoneNumbers (Number, DeviceId) OUTPUT INSERTED.Id VALUES (@Number, @DeviceId)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Number", phoneNumber.Number);
                        cmd.Parameters.AddWithValue("@DeviceId", phoneNumber.DeviceId);
                        phoneNumber.Id = (int)cmd.ExecuteScalar();
                    }
                }

                return Json(new { success = true, message = "Phone number added successfully.", data = phoneNumber });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding phone number", details = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdatePhoneNumber(PhoneNumber phoneNumber)
        {
            try
            {
                if (phoneNumber.Id <= 0 || string.IsNullOrWhiteSpace(phoneNumber.Number) || phoneNumber.DeviceId <= 0)
                {
                    return Json(new { success = false, message = "Invalid phone number data." });
                }

                var regex = new Regex("-+"); // Must include at least one dash
                if (!regex.IsMatch(phoneNumber.Number))
                {
                    return Json(new { success = false, message = "Phonenumber must include at least one dash (-) to separate parts." });
                }

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE PhoneNumbers SET Number = @Number, DeviceId = @DeviceId WHERE Id = @Id";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Number", phoneNumber.Number);
                        cmd.Parameters.AddWithValue("@DeviceId", phoneNumber.DeviceId);
                        cmd.Parameters.AddWithValue("@Id", phoneNumber.Id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return Json(new { success = false, message = "No phone number found with that ID." });
                        }
                    }
                }

                return Json(new { success = true, message = "Phone number updated successfully.", data = phoneNumber });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error updating phone number", details = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult DeletePhoneNumber(int id)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = "Invalid Phone Number ID" });

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    //check if this phone number is currently reserved
                    string checkQuery = @"
                SELECT COUNT(*) 
                FROM PhoneNumberReservations 
                WHERE PhoneNumberId = @PhoneNumberId
                AND (EED IS NULL OR EED >= GETDATE())
                AND BED <= GETDATE()";

                    using (var checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@PhoneNumberId", id);
                        int reservationCount = (int)checkCmd.ExecuteScalar();

                        if (reservationCount > 0)
                        {
                            return Json(new
                            {
                                success = false,
                                message = "This phone number is currently reserved and cannot be deleted."
                            });
                        }
                    }

                    // If not reserved delete
                    string deleteQuery = "DELETE FROM PhoneNumbers WHERE Id = @Id";

                    using (var cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return Json(new
                            {
                                success = true,
                                message = "Phone number deleted successfully."
                            });
                        }
                        else
                        {
                            return Json(new
                            {
                                success = false,
                                message = "Phone number not found or already deleted."
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //this checks if any forign key is connected to phonenumber to not break the db
                if (ex.Message.Contains("REFERENCE constraint") || ex.Message.Contains("foreign key"))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot delete this phone number because it is referenced in other records."
                    });
                }

                return Json(new
                {
                    success = false,
                    message = "Error deleting phone number: " + ex.Message
                });
            }
        }
    }
}
