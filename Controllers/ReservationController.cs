using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using internshipPartTwo.Models;

namespace internshipPartTwo.Controllers
{
    public class ReservationController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpGet]
        public JsonResult GetReservations()
        {
            var reservations = new List<object>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
            SELECT r.Id, r.ClientId, r.PhoneNumberId, r.BED, r.EED, 
                   c.Name AS ClientName, p.Number AS PhoneNumber 
            FROM PhoneNumberReservations r
            INNER JOIN Clients c ON r.ClientId = c.Id
            INNER JOIN PhoneNumbers p ON r.PhoneNumberId = p.Id";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        reservations.Add(new
                        {
                            Id = (int)reader["Id"],
                            ClientId = (int)reader["ClientId"],
                            PhoneNumberId = (int)reader["PhoneNumberId"],
                            BED = reader["BED"] != DBNull.Value
                                ? ((DateTime)reader["BED"]).ToString("yyyy-MM-ddTHH:mm:ss")
                                : null,
                            EED = reader["EED"] != DBNull.Value
                                ? ((DateTime)reader["EED"]).ToString("yyyy-MM-ddTHH:mm:ss")
                                : null,
                            ClientName = reader["ClientName"].ToString(),
                            PhoneNumber = reader["PhoneNumber"].ToString()
                        });
                    }
                }
            }

            return Json(reservations, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddReservation(PhoneNumberReservation reservation)
        {
            try
            {
                if (reservation.ClientId <= 0)
                    return Json(new { success = false, message = "Invalid Client" });
                if (reservation.PhoneNumberId <= 0)
                    return Json(new { success = false, message = "Invalid Phone Number" });

                if (reservation.BED == default(DateTime))
                    return Json(new { success = false, message = "Begin Date is required" });
                reservation.BED = reservation.BED.Date;
                // allow end date to be null
                reservation.EED = reservation.EED ?? null;
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string checkQuery = @"
                     SELECT COUNT(*) 
                     FROM PhoneNumberReservations 
                     WHERE ClientId = @ClientId 
                     AND PhoneNumberId = @PhoneNumberId 
                     AND (EED IS NULL OR EED >= GETDATE()) 
                     AND (
                     
                     (@BED >= BED AND (@BED <= EED OR EED IS NULL))
                     OR
                 
                     (@EED IS NOT NULL AND @EED >= BED AND (@EED <= EED OR EED IS NULL))
                     OR
                     (@BED <= BED AND (@EED IS NULL OR @EED >= ISNULL(EED, @BED)))
                     )";
                    using (var checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@ClientId", reservation.ClientId);
                        checkCmd.Parameters.AddWithValue("@PhoneNumberId", reservation.PhoneNumberId);
                        checkCmd.Parameters.AddWithValue("@BED", reservation.BED);
                        checkCmd.Parameters.AddWithValue("@EED", reservation.EED.HasValue ? (object)reservation.EED.Value : DBNull.Value);

                        int overlappingReservations = (int)checkCmd.ExecuteScalar();
                        if (overlappingReservations > 0)
                        {
                            return Json(new { success = false, message = "This phone number is already reserved for the selected date range" });
                        }
                    }

                    // Insert the reservation with EED nullable
                    string insertQuery = @"
                INSERT INTO PhoneNumberReservations (ClientId, PhoneNumberId, BED, EED) 
                VALUES (@ClientId, @PhoneNumberId, @BED, @EED);
                SELECT SCOPE_IDENTITY();";

                    using (var cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@ClientId", reservation.ClientId);
                        cmd.Parameters.AddWithValue("@PhoneNumberId", reservation.PhoneNumberId);
                        cmd.Parameters.AddWithValue("@BED", reservation.BED);
                        cmd.Parameters.AddWithValue("@EED", reservation.EED.HasValue ? (object)reservation.EED.Value : DBNull.Value);

                        int newId = Convert.ToInt32(cmd.ExecuteScalar());
                        return Json(new { success = true, id = newId });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateReservation(PhoneNumberReservation reservation)
        {
            try
            {
                if (reservation.Id <= 0)
                    return Json(new { success = false, message = "Invalid Reservation ID" });

                if (reservation.ClientId <= 0)
                    return Json(new { success = false, message = "Invalid Client" });

                if (reservation.PhoneNumberId <= 0)
                    return Json(new { success = false, message = "Invalid Phone Number" });

                if (reservation.BED == default(DateTime))
                    return Json(new { success = false, message = "Begin Date is required" });

                if (reservation.EED.HasValue && reservation.EED < reservation.BED)
                    return Json(new { success = false, message = "End Date cannot be before Begin Date" });

                // Normalize dates to remove time component
                reservation.BED = reservation.BED.Date;
                if (reservation.EED.HasValue)
                {
                    reservation.EED = reservation.EED.Value.Date;
                }

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Improved overlap check for update
                    string checkQuery = @"
                        SELECT COUNT(*) 
                        FROM PhoneNumberReservations 
                        WHERE PhoneNumberId = @PhoneNumberId 
                        AND Id != @Id
                        AND (
                            -- New reservation start date is within existing reservation
                            (@BED >= BED AND (@BED <= EED OR EED IS NULL))
                            OR
                            -- New reservation end date is within existing reservation
                            (@EED IS NOT NULL AND @EED >= BED AND (@EED <= EED OR EED IS NULL))
                            OR
                            -- New reservation completely contains existing reservation
                            (@BED <= BED AND (@EED IS NULL OR @EED >= ISNULL(EED, @BED)))
                        )";

                    using (var checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Id", reservation.Id);
                        checkCmd.Parameters.AddWithValue("@PhoneNumberId", reservation.PhoneNumberId);
                        checkCmd.Parameters.AddWithValue("@BED", reservation.BED);
                        checkCmd.Parameters.AddWithValue("@EED", reservation.EED.HasValue ? (object)reservation.EED.Value : DBNull.Value);

                        int overlappingReservations = (int)checkCmd.ExecuteScalar();
                        if (overlappingReservations > 0)
                        {
                            return Json(new { success = false, message = "This phone number is already reserved for the selected date range" });
                        }
                    }

                    // Update the reservation
                    string updateQuery = @"
                        UPDATE PhoneNumberReservations 
                        SET ClientId = @ClientId, 
                            PhoneNumberId = @PhoneNumberId, 
                            BED = @BED, 
                            EED = @EED
                        WHERE Id = @Id";

                    using (var cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", reservation.Id);
                        cmd.Parameters.AddWithValue("@ClientId", reservation.ClientId);
                        cmd.Parameters.AddWithValue("@PhoneNumberId", reservation.PhoneNumberId);
                        cmd.Parameters.AddWithValue("@BED", reservation.BED);
                        cmd.Parameters.AddWithValue("@EED", reservation.EED.HasValue ? (object)reservation.EED.Value : DBNull.Value);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return Json(new { success = rowsAffected > 0, message = rowsAffected > 0 ? "" : "Reservation not found" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteReservation(int id)
        {
            try
            {
                if (id <= 0)
                    return Json(new { success = false, message = "Invalid Reservation ID" });

                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string deleteQuery = "DELETE FROM PhoneNumberReservations WHERE Id = @Id";

                    using (var cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", id);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return Json(new { success = rowsAffected > 0, message = rowsAffected > 0 ? "" : "Reservation not found" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetClientActiveReservations(int clientId)
        {
            var activeReservations = new List<object>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    SELECT r.Id, r.ClientId, r.PhoneNumberId, r.BED, r.EED, 
                           p.Number AS PhoneNumber
                    FROM PhoneNumberReservations r
                    INNER JOIN PhoneNumbers p ON r.PhoneNumberId = p.Id
                    WHERE r.ClientId = @ClientId
                    AND r.BED <= GETDATE()
                    AND (r.EED IS NULL OR r.EED >= GETDATE())";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            activeReservations.Add(new
                            {
                                Id = (int)reader["Id"],
                                ClientId = (int)reader["ClientId"],
                                PhoneNumberId = (int)reader["PhoneNumberId"],
                                BED = reader["BED"] != DBNull.Value
                                    ? ((DateTime)reader["BED"]).ToString("yyyy-MM-ddTHH:mm:ss")
                                    : null,
                                EED = reader["EED"] != DBNull.Value
                                    ? ((DateTime)reader["EED"]).ToString("yyyy-MM-ddTHH:mm:ss")
                                    : null,
                                PhoneNumber = reader["PhoneNumber"].ToString()
                            });
                        }
                    }
                }
            }

            return Json(activeReservations, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateReservationEndDate(int phoneNumberId, int clientId)
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Update the reservation's end date to now
                    string updateQuery = @"
                        UPDATE PhoneNumberReservations 
                        SET EED = @EED
                        WHERE PhoneNumberId = @PhoneNumberId 
                        AND ClientId = @ClientId
                        AND BED <= GETDATE()
                        AND (EED IS NULL OR EED >= GETDATE())";

                    using (var cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PhoneNumberId", phoneNumberId);
                        cmd.Parameters.AddWithValue("@ClientId", clientId);
                        cmd.Parameters.AddWithValue("@EED", DateTime.Now);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        return Json(new { success = rowsAffected > 0, message = rowsAffected > 0 ? "" : "Reservation not found or already ended" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }
}