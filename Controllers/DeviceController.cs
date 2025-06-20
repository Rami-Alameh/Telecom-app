using System;
using System.Web.Mvc;
using internshipPartTwo.DAL;
using internshipPartTwo.Models;

namespace internshipPartTwo.Controllers
{
    /// <summary>
    /// Controller for device-related API endpoints
    /// </summary>
    [RoutePrefix("api/device")]  //route: "/api/device"
    public class DeviceController : Controller
    {
        // DAL instance to handle all DB operations
        private readonly DeviceDAL _deviceDAL;

        public DeviceController()
        {
            _deviceDAL = new DeviceDAL();
            // TODO: Consider using dependency injection instead of direct instantiation
        }

        public ActionResult Index()
        {
            return View();
        }

        /// Gets all devices
        [HttpGet]
        [Route("get")]
        public JsonResult GetDevices()
        {
            try
            {
                // Lets the DAL handle the data access
                var devices = _deviceDAL.GetAllDevices();
                return Json(devices, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Return error info to client
                return Json(new { error = "Error fetching devices", details = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        /// Adds a new device
        [HttpPost]
        [Route("add")]
        public JsonResult AddDevice(Device device)
        {
            try
            {
                // DAL handles the insert and returns updated device with ID
                var addedDevice = _deviceDAL.AddDevice(device);
                return Json(new { success = true, message = "Device added successfully!", device = addedDevice });
            }
            catch (Exception ex)
            {
                // TODO: Add logging here
                return Json(new { success = false, error = "Error adding device", details = ex.Message });
            }
        }

        /// Updates a device
        [HttpPost]
        [Route("update")]
        public JsonResult UpdateDevice(Device updated)
        {
            try
            {
                // DAL handles the update and returns success/failure
                bool success = _deviceDAL.UpdateDevice(updated);

                if (success)
                {
                    return Json(new { success = true, message = "Device updated successfully!", device = updated });
                }
                else
                {
                    // No device with that ID found
                    return Json(new { success = false, error = "No device found with that ID" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Error updating device", details = ex.Message });
            }
        }

        /// Deletes a device if it has no phone numbers
        [HttpPost]
        public JsonResult DeleteDevice(int id)
        {
            try
            {
                // Check if safe to delete 
                bool hasPhoneNumbers = _deviceDAL.HasPhoneNumbers(id);

                if (hasPhoneNumbers)
                {
                    return Json(new
                    {
                        success = false,
                        error = "Cannot delete this device because it has phone numbers attached."
                    });
                }

                // DAL handles the actual delete
                bool success = _deviceDAL.DeleteDevice(id);

                if (success)
                {
                    return Json(new { success = true, message = "Device deleted successfully" });
                }
                else
                {
                    return Json(new { success = false, error = "No device found with that ID" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Error deleting device", details = ex.Message });
            }
        }
    }
}