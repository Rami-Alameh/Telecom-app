Phone Number Reservation System

A simple and functional web app for managing devices and phone number reservations. Built using ASP.NET MVC and AngularJS, this project lets users add, edit, delete, and reserve phone numbers tied to specific devices. It also includes filtering, confirmation modals, success/error messages, and basic validation.


## 🔧 What It's Built With

- ASP.NET MVC for the backend
- AngularJS for the frontend logic
- Bootstrap for styling
- SQL Server as the database
- Visual Studio for development

---

What You Can Do

- Add, edit, and delete devices  
- Add, edit, and delete phone numbers  
- Reserve a phone number for a client, with a selected date range  
- See which phone numbers are tied to which devices  
- Filter and search through the device and phone number lists  
- Get confirmation before deleting anything  
- See clear success or error messages for every action  
- Prevent deleting devices that still have phone numbers attached

---
file structure
Controllers → All C# logic for MVC and WebAPI
Models → C# classes for Device, PhoneNumber, Reservation, etc.
Views → Razor Views using AngularJS inside .cshtml files
Scripts/js → AngularJS controllers (DeviceController.js, PhoneNumberController.js)
Content/css → Custom CSS styles
App_Start → Route and bundle configuration

Made with by Rami Alameh




