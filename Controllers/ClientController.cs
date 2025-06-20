// Updated ClientController.cs
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using internshipPartTwo.Models;
using System.Globalization;
using System.Linq;

namespace internshipPartTwo.Controllers
{
    public class ClientController : Controller
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpGet]
        public JsonResult GetClients()
        {
            var clients = new List<Client>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT Id, Name, Type, BirthDate FROM Clients", conn);
                var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    clients.Add(new Client
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        Type = (ClientType)reader["Type"],
                        BirthDate = reader["BirthDate"] != DBNull.Value
                            ? DateTime.SpecifyKind((DateTime)reader["BirthDate"], DateTimeKind.Local)
                            : (DateTime?)null
                    });
                }
            }

            return Json(clients.Select(c => new
            {
                c.Id,
                c.Name,
                c.Type,
                BirthDate = c.BirthDate?.ToString("yyyy-MM-ddTHH:mm:ss")
            }), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AddClient(Client client)
        {
            var validation = ValidateClient(client);
            if (!validation.success)
            {
                return Json(new { success = false, message = validation.message });
            }

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO Clients (Name, Type, BirthDate) VALUES (@Name, @Type, @BirthDate)", conn);
                cmd.Parameters.AddWithValue("@Name", client.Name);
                cmd.Parameters.AddWithValue("@Type", (int)client.Type);
                cmd.Parameters.AddWithValue("@BirthDate",
                    client.BirthDate.HasValue ? (object)DateTime.SpecifyKind(client.BirthDate.Value, DateTimeKind.Local) : DBNull.Value);
                cmd.ExecuteNonQuery();
            }

            return Json(new { success = true, message = "Client added successfully." });
        }

        [HttpPost]
        public JsonResult UpdateClient(Client client)
        {
            var validation = ValidateClient(client);
            if (!validation.success)
            {
                return Json(new { success = false, message = validation.message });
            }

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE Clients SET Name = @Name, Type = @Type, BirthDate = @BirthDate WHERE Id = @Id", conn);
                cmd.Parameters.AddWithValue("@Name", client.Name);
                cmd.Parameters.AddWithValue("@Type", (int)client.Type);
                cmd.Parameters.AddWithValue("@BirthDate",
                    client.BirthDate.HasValue ? (object)DateTime.SpecifyKind(client.BirthDate.Value, DateTimeKind.Local) : DBNull.Value);
                cmd.Parameters.AddWithValue("@Id", client.Id);
                cmd.ExecuteNonQuery();
            }

            return Json(new { success = true, message = "Client updated successfully." });
        }

        private (bool success, string message) ValidateClient(Client client)
        {
            if (string.IsNullOrWhiteSpace(client.Name) || client.Name.Length < 3)//cant be empty or < 3 char
            {
                return (false, "Client name must be at least 3 characters long.");
            }
            

            if ((int)client.Type == 0) // Individual
            {
                if (!client.BirthDate.HasValue)
                {
                    return (false, "Birth date is required for Individual clients.");//must have a birthday if individual
                }

                var age = DateTime.Today.Year - client.BirthDate.Value.Year;
                if (client.BirthDate.Value > DateTime.Today.AddYears(-age)) age--;//method to check the age based on current date
                if (age < 18)
                {
                    return (false, "Client must be at least 18 years old.");//must be > 18
                }
            }
            return (true, "Valid");
        }
    }
}
