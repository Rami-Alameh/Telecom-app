app.controller("ReservationController", function ($scope, $http, $timeout) {
    // Data stores
    $scope.reservations = [];
    $scope.filterClientId = null;
    $scope.filterPhoneNumberId = null;
    $scope.searchQuery = '';
    $scope.isEditing = false;
    $scope.currentReservation = {};
    $scope.reservationToDelete = null;
    $scope.alert = null;

    // Load all reservations
    $scope.loadReservations = function () {
        $http.get('/Reservation/GetReservations')
            .then(function (res) {
                $scope.reservations = res.data.map(function (reservation) {
                    // Parse dates
                    if (reservation.BED) reservation.BED = new Date(reservation.BED);
                    if (reservation.EED) reservation.EED = new Date(reservation.EED);
                    return reservation;
                });
            }).catch(function (error) {
                console.error("Error loading reservations:", error);
                $scope.showAlert("Failed to load reservations", 'danger');
            });
    };
    
    // Filter function (now properly defined as a function)
    $scope.getFilteredReservations = function () {
        var results = $scope.reservations || [];

        // Apply search filter
        if ($scope.searchQuery) {
            results = results.filter(function (r) {
                return r.PhoneNumber &&
                    r.PhoneNumber.toLowerCase().includes($scope.searchQuery.toLowerCase());
            });
        }

        // Apply client filter
        if ($scope.filterClientId) {
            results = results.filter(function (r) {
                return r.ClientId == $scope.filterClientId;
            });
        }

        // Apply phone number filter
        if ($scope.filterPhoneNumberId) {
            results = results.filter(function (r) {
                return r.PhoneNumberId == $scope.filterPhoneNumberId;
            });
        }

        return results;
    };

    // Edit existing reservation
    $scope.editReservation = function (reservation) {
        $scope.isEditing = true;
        $scope.currentReservation = angular.copy(reservation);
        $('#reservationModal').modal('show');
    };

    // Confirm delete
    $scope.confirmDelete = function (reservation) {
        $scope.reservationToDelete = reservation;
        $('#deleteModal').modal('show');
    };

    // Save reservation
    $scope.saveReservation = function () {
        var reservation = angular.copy($scope.currentReservation);

        // Normalize dates
        if (reservation.BED) {
            var bed = new Date(reservation.BED);
            bed.setHours(0, 0, 0, 0);
            reservation.BED = bed.toISOString().split('T')[0];
        }

        if (reservation.EED) {
            var eed = new Date(reservation.EED);
            eed.setHours(0, 0, 0, 0);
            reservation.EED = eed.toISOString().split('T')[0];
        }

        $http.post('/Reservation/UpdateReservation', reservation)
            .then(function (response) {
                $scope.loadReservations();
                $('#reservationModal').modal('hide');
                $scope.showAlert("Reservation updated successfully!", 'success');
            })
            .catch(function (error) {
                console.error("Error saving reservation:", error);
                $scope.showAlert(error.data?.message || "Error saving reservation", 'danger');
            });
    };

    // Delete reservation
    $scope.deleteReservation = function () {
        $http.post('/Reservation/DeleteReservation', { id: $scope.reservationToDelete.Id })
            .then(function (response) {
                $scope.loadReservations();
                $('#deleteModal').modal('hide');
                $scope.showAlert("Reservation deleted successfully!", 'success');
            })
            .catch(function (error) {
                console.error("Error deleting reservation:", error);
                $scope.showAlert(error.data?.message || "Error deleting reservation", 'danger');
            });
    };

    // Alert helper
    $scope.showAlert = function (message, type) {
        $scope.alert = { message: message, type: type };
        $timeout(function () { $scope.alert = null; }, 5000);
    };

    // Initial load
    $scope.loadReservations();
});