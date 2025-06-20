using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using internshipPartTwo.Models;

namespace internshipPartTwo.DAL
{
    /// Data Access Layer for Device operations
    /// Handles all database interactions for the Device entity
    public class DeviceDAL
    {
        // Get connection string from web.config
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        // Constructor - makes sure DB is ready when we create an instance
        public DeviceDAL()
        {
            EnsureDatabaseSetup();
        }

        /// Gets all devices from the database-
        public List<Device> GetAllDevices()
        {
            var devices = new List<Device>();

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Simple query to get all devices - might need pagination later if table grows
                string query = "SELECT Id, Name FROM Devices";
                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Map DB fields to Device object
                        devices.Add(new Device
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }

            return devices;
        }

        /// Gets a specific device by ID
        /// Returns null if device not found
        public Device GetDeviceById(int id)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = "SELECT Id, Name FROM Devices WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    //paramater to avoid sql injections
                    cmd.Parameters.AddWithValue("@Id", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Device
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            };
                        }
                    }
                }
            }

            // No device found with that ID
            return null;
        }

        /// Adds a new device to the database
        /// Updates the ID of the passed device object with the new ID
        public Device AddDevice(Device device)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // OUTPUT INSERTED.Id gives us back the new ID
                string query = "INSERT INTO Devices (Name) OUTPUT INSERTED.Id VALUES (@Name)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", device.Name);

                    // ExecuteScalar returns the first column of the first row
                    // In this case, that's the new ID
                    device.Id = (int)cmd.ExecuteScalar();
                }
            }

            // Return the device with its new ID
            return device;
        }

        /// Updates an existing device
        /// Returns true if a device was updated, false if no device with that ID exists
        public bool UpdateDevice(Device device)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Devices SET Name = @Name WHERE Id = @Id";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Name", device.Name);
                    cmd.Parameters.AddWithValue("@Id", device.Id);

                    // ExecuteNonQuery returns the number of rows affected
                    int rowsAffected = cmd.ExecuteNonQuery();

                    // If rowsAffected is 0, no device with that ID was found
                    return rowsAffected > 0;
                }
            }
        }

        // Deletes a device if it has no associated phone numbers

        public bool DeleteDevice(int id)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string deleteQuery = "DELETE FROM Devices WHERE Id = @Id";
                using (var cmd = new SqlCommand(deleteQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0; // True if device existed and was deleted
                }
            }}

        // Checks if a device has any associated phone numbers
        public bool HasPhoneNumbers(int deviceId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM PhoneNumbers WHERE DeviceId = @DeviceId";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DeviceId", deviceId);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0; // True if there are phone numbers
                }
            }
        }
        /// Makes sure the database is properly set up
        /// Called by the constructor
        private void EnsureDatabaseSetup()
        {
            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    CreateTableIfNotExists(conn);
                }
            }
            catch (Exception ex)
            {
                // TODO: Add proper logging here
                throw new Exception("Error ensuring database setup: " + ex.Message);
            }
        }

        
        /// Creates the Devices table if it doesn't exist
        private void CreateTableIfNotExists(SqlConnection conn)
        {
            // SQL to check if table exists and create it if not
            string query = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Devices' AND xtype='U')
                CREATE TABLE Devices (
                    Id INT PRIMARY KEY IDENTITY(1,1), 
                    Name NVARCHAR(100) NOT NULL
                )";
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}