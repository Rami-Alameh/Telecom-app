var app = angular.module("DeviceApp", ["ngRoute"]);


app.config(function ($routeProvider) {
    $routeProvider
        .when("/devices", {
            templateUrl: "/Views/Home/Index.cshtml",
            controller: "DeviceController"
        })
        .when("/clients", {
            templateUrl: "/Views/Home/Clients.cshtml",
            controller: "ClientController"
        })
        .when("/phoneNumbers", {
            templateUrl: "/Views/Home/PhoneNumbers.cshtml",
            controller: "PhoneNumberController"
        })
        .when("/reservations", {
            templateUrl: "/Views/Home/Reservations.cshtml",
            controller: "ReservationController"
        })
        .otherwise({
            redirectTo: "/devices"
        });
});
