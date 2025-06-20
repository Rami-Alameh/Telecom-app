app.controller("PhoneNumberController", function ($scope, $http) {
    $scope.phoneNumbers = [];
    $scope.devices = [];
    $scope.showModal = false;
    $scope.isEdit = false;
    $scope.modalPhoneNumber = {};
    $scope.showDeleteConfirmModal = false;
    $scope.phoneNumberToDelete = null;
    // Message display logic
    $scope.statusMessage = "";
    $scope.statusType = "";

    //  Load all devices FIRST
    $scope.loadDevices = function () {
        $http.get('/Device/GetDevices')
            .then(function (res) {
                $scope.devices = res.data;
                console.log("Devices Loaded:", $scope.devices);
            }).catch(function (error) {
                console.error("Error fetching devices:", error);
            });
    };
    $scope.showStatus = function (type, message) {
        $scope.statusType = type;
        $scope.statusMessage = message;

        // Auto hide after 4 seconds
        setTimeout(() => {
            $scope.statusMessage = "";
            $scope.$apply();
        }, 4000);
    };
    //get device nam
    $scope.getDeviceName = function (deviceId) {
        const device = $scope.devices.find(d => d.Id === deviceId);
        return device ? device.Name : "Unknown Device"; //  Show "Unknown Device" if not found
    };
    //  Load all phone numbers
    $scope.loadPhoneNumbers = function () {
        $http.get('/PhoneNumber/GetPhoneNumbers')
            .then(function (res) {
                $scope.phoneNumbers = res.data;
                $scope.loadDevices();
                console.log("Phone Numbers Loaded:", $scope.phoneNumbers);
            }).catch(function (error) {
                console.error("Error fetching phone numbers:", error);
            });

    };

    // ✅ Open Modal for Add OR Edit
    $scope.openModal = function (phoneNumber) {
        $scope.isEdit = !!phoneNumber;  // If phoneNumber exists, we are editing
        $scope.modalPhoneNumber = phoneNumber ? angular.copy(phoneNumber) : {};
        $scope.showModal = true;
        $scope.loadDevices();
        console.log($scope.isEdit ? "Editing Phone Number:" : "Adding Phone Number:", $scope.modalPhoneNumber);
    };

    // ✅ Save (Handles BOTH Add & Edit)
    $scope.savePhoneNumber = function () {
        if (!$scope.modalPhoneNumber.Number || !$scope.modalPhoneNumber.DeviceId) {
            $scope.showStatus("error", "Phone number and device are required.");
            return;
        }

        const url = $scope.isEdit ? '/PhoneNumber/UpdatePhoneNumber' : '/PhoneNumber/AddPhoneNumber';
        $http.post(url, $scope.modalPhoneNumber)//takes from model and sends back
            .then(function (response) {
                if (response.data.success) {
                    $scope.showStatus("success", response.data.message);//success
                    $scope.loadPhoneNumbers();
                    $scope.closeModal();
                } else {
                    $scope.showStatus("error", response.data.message);//error gotten from the backend 
                }
            })
            .catch(function (error) {
                $scope.showStatus("error", "Error: " + (error.data?.message || "Server error"));//server/other errors
            });
    };

    $scope.editPhoneNumber = function (phoneNumber) {
        if (!phoneNumber) return;

        console.log("Editing Phone Number:", phoneNumber);

        $scope.isEdit = true;
        $scope.modalPhoneNumber = angular.copy(phoneNumber);
        $scope.showModal = true;
        $scope.loadDevices(); //Load devices when opening the edit modal
    };

    // delete confirmation modal
    $scope.confirmDeletePhoneNumber = function (phoneNumber) {
        $scope.phoneNumberToDelete = phoneNumber;
        $scope.showDeleteConfirmModal = true;
    };

    //delete 
    $scope.deletePhoneNumber = function () {
        if (!$scope.phoneNumberToDelete) return;

        $http.post('/PhoneNumber/DeletePhoneNumber', { id: $scope.phoneNumberToDelete.Id })
            .then(function (response) {
                if (response.data.success) {
                    $scope.showStatus("success", response.data.message);
                    $scope.loadPhoneNumbers();
                } else {
                    $scope.showStatus("error", response.data.message);
                }
                $scope.closeDeleteConfirmModal();
            })
            .catch(function (error) {
                $scope.showStatus("error", "Error: " + (error.data?.message || "Server error"));
                $scope.closeDeleteConfirmModal();
            });
    };

    //Close Delete Confirmation Modal
    $scope.closeDeleteConfirmModal = function () {
        $scope.showDeleteConfirmModal = false;
        $scope.phoneNumberToDelete = null;
    };

    //Close Modal
    $scope.closeModal = function () {
        $scope.showModal = false;
    };

    //Initial Load
    $scope.loadPhoneNumbers();
    $scope.loadDevices();
});