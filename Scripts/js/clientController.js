app.controller("ClientController", function ($scope, $http) {
    // Client data and state
    $scope.clients = [];
    $scope.showModal = false;
    $scope.isEdit = false;
    $scope.modalClient = {};
    $scope.statusMessage = "";
    $scope.statusType = "";
    // Reservation data and state
    $scope.showReservationModal = false;
    $scope.reservationType = ''; // 'reserve' or 'unreserve'
    $scope.selectedReservation = { PhoneNumberId: null };
    $scope.availablePhoneNumbers = [];
    $scope.clientActiveReservations = [];
    $scope.currentClientId = null;

    // Client types 
    $scope.clientTypes = [
        { value: 0, label: 'Individual' },
        { value: 1, label: 'Organization' }
    ];

    // Watch for client type changes
    $scope.$watch("modalClient.Type", function (newType) {
        if (newType == 1) {
            $scope.modalClient.BirthDate = null; // Clear birth date for organizations
        }
    });

    // ======================
    // CLIENT FUNCTIONS
    // ======================
    $scope.showStatus = function (type, message) {
        $scope.statusType = type;
        $scope.statusMessage = message;

        // Auto-hide after 4 seconds
        setTimeout(() => {
            $scope.statusMessage = "";
            $scope.$apply();
        }, 4000);
    };

    // Load all clients
    $scope.loadClients = function () {
        $http.get('/Client/GetClients').then(function (res) {
            $scope.clients = res.data.map(client => {
                if (client.BirthDate) {
                    client.BirthDate = new Date(client.BirthDate);
                }
                return client;
            });

            // Check reservations for each client
            $scope.clients.forEach(client => {
                $scope.checkClientReservations(client);
            });
        });
    };

    // Open add client modal
    $scope.openModal = function () {
        $scope.isEdit = false;
        $scope.modalClient = { Type: 0 }; // Default to Individual
        $scope.showModal = true;
    };

    // Open edit client modal
    $scope.editClient = function (client) {
        $scope.isEdit = true;
        $scope.modalClient = angular.copy(client);

        if ($scope.modalClient.BirthDate) {
            $scope.modalClient.BirthDate = new Date($scope.modalClient.BirthDate);
        }

        $scope.showModal = true;
    };

    // Save client (add or update)
    $scope.saveClient = function () {
        const name = $scope.modalClient.Name?.trim();
        if (!name || name.length < 3) {//should be more than 3 characters 
            $scope.showStatus("error", "Client name must be at least 3 characters.");
            return;
        }

        if ($scope.modalClient.Type === 0 && !$scope.modalClient.BirthDate) {
            $scope.showStatus("error", "Birth date is required for Individual clients.");

            return;//make sure birthday is entered 
        }

        const clientToSave = angular.copy($scope.modalClient);
        if (clientToSave.Type == 1) {
            clientToSave.BirthDate = null;
        } else if (clientToSave.BirthDate) {
            clientToSave.BirthDate = clientToSave.BirthDate.toISOString();
        }

        const url = $scope.isEdit ? '/Client/UpdateClient' : '/Client/AddClient';

        $http.post(url, clientToSave).then(function (res) {
            if (res.data.success) {
                $scope.showStatus("success",res.data.message);

                $scope.loadClients();
                $scope.closeModal();
            } else {
                $scope.showStatus("error", res.data.message);
            }
        }).catch(function (error) {
            alert("Server error: " + (error.data?.message || "Request failed"));
        });
    };


    // Close client modal
    $scope.closeModal = function () {
        $scope.showModal = false;
        $scope.modalClient = {};
    };

    // ---------------------------------
    // RESERVATION FUNCTIONS
    //------------------------------------

    // Check if client has active reservations
    $scope.checkClientReservations = function (client) {
        $http.get('/Reservation/GetClientActiveReservations', { params: { clientId: client.Id } })
            .then(function (res) {
                client.hasActiveReservations = res.data.length > 0;
                client.activeReservationsCount = res.data.length;
            });
    };

    // Open reserve phone modal
    $scope.openReserveModal = function (client) {
        $scope.reservationType = 'reserve';
        $scope.currentClientId = client.Id;
        $scope.selectedReservation = { PhoneNumberId: null };
        $scope.availablePhoneNumbers = [];

        $http.get('/PhoneNumber/GetAvailablePhoneNumbers')//important to get phonenumbers with no clients to avoid errors 
            .then(function (res) {
                $scope.availablePhoneNumbers = res.data;//gets data
                $scope.showReservationModal = true;//opens model
            })
            .catch(function (error) {
                console.error("Error loading phone numbers:", error);
                alert("Failed to load available phone numbers");
            });
    };

    // Open unreserve phone modal
    $scope.openUnreserveModal = function (client) {
        $scope.reservationType = 'unreserve';
        $scope.currentClientId = client.Id;
        $scope.selectedReservation = { PhoneNumberId: null };
        $scope.clientActiveReservations = [];

        $http.get('/Reservation/GetClientActiveReservations', { params: { clientId: client.Id } })//gets all active reservations
            .then(function (res) {
                if (res.data.length === 0) {//if he doesnt have reservations
                    alert("This client has no active reservations");
                } else {
                    $scope.clientActiveReservations = res.data;
                    $scope.showReservationModal = true;
                }
            })
            .catch(function (error) {
                console.error("Error loading reservations:", error);
                alert("Failed to load client reservations");
            });
    };

    // Process reservation/unreservation
    $scope.processReservation = function () {
        if (!$scope.selectedReservation.PhoneNumberId) {
            alert("Please select a phone number");
            return;
        }

        const payload = {
            ClientId: $scope.currentClientId,
            PhoneNumberId: $scope.selectedReservation.PhoneNumberId
        };

        // Adds begin date for new reservations
        if ($scope.reservationType === 'reserve') {
            payload.BED = new Date().toISOString();
        }

        const endpoint = $scope.reservationType === 'reserve'
            ? '/Reservation/AddReservation'
            : '/Reservation/UpdateReservationEndDate';

        $http.post(endpoint, payload)//uses payload to move data to the server post 
            .then(function (response) {//recieves response with the data
                if (response.data.success) {
                    alert("Phone Number operation successfull!");//alert if reserve/unreserve was successful
                    $scope.closeReservationModal();
                    $scope.loadClients(); // Refresh client list
                } else {
                    alert("Error: " + (response.data.message || "Operation failed"));
                }
            })
            .catch(function (error) {
                alert("Server error: " + (error.data?.message || "Request failed"));
            });
    };

    // Close reservation modal
    $scope.closeReservationModal = function () {
        $scope.showReservationModal = false;
        $scope.selectedReservation = { PhoneNumberId: null };
    };

    // Initial load
    $scope.loadClients();
});