var app = angular.module('DeviceApp', []);

app.controller('DeviceController', function ($scope, $http) {
    $scope.devices = [];
    $scope.showModal = false;
    $scope.isEdit = false;
    $scope.modalDevice = {};
    $scope.statusMessage = "";
    $scope.statusType = ""; // if success green if fail red
    //delete confirmation
    $scope.showDeleteConfirmModal = false;
    $scope.deviceToDelete = null;



    // Load all devices
    $scope.loadDevices = function () {
        $http.get('/Device/GetDevices').then(function (res) {
            $scope.devices = res.data;
            console.log($scope.devices)

        });
    };
    //adds a success/fail message and auto hides it 
    $scope.showStatus = function (type, message) {
        $scope.statusType = type;
        $scope.statusMessage = message;

        // Auto-hide message after 3 seconds
        setTimeout(() => {
            $scope.statusMessage = "";
            $scope.$apply();
        }, 3000);
    };
    // Open modal to add
    $scope.openModal = function () {
        $scope.isEdit = false;
        $scope.modalDevice = {};
        $scope.showModal = true;
    };

    // Open modal to edit
    $scope.editDevice = function (device) {
        $scope.isEdit = true;
        console.log(device
        )
        $scope.modalDevice = {
            Id: device.Id,  // Ensure the property names match the backend model
            Name: device.Name

        };
        $scope.showModal = true;
    };
    // Save or update device
    $scope.saveDevice = function () {
        if (!$scope.modalDevice.Name) {
            $scope.showStatus("error", "Device name is required.");
            return;
        }

        const url = $scope.isEdit ? '/Device/UpdateDevice' : '/Device/AddDevice';
        $http.post(url, $scope.modalDevice).then(function (response) {
            if (response.data.success) {
                $scope.loadDevices();
                $scope.closeModal();
                $scope.showStatus("success", response.data.message);
            } else {
                $scope.showStatus("error", response.data.error || "Something went wrong.");
            }
        }, function (error) {
            console.error('Error saving device:', error);
            $scope.showStatus("error", "An error occurred while saving.");
        });
    };

    // Confirm delete model gets device and show model
    $scope.confirmDeleteDevice = function (device) {
        $scope.deviceToDelete = device;
        $scope.showDeleteConfirmModal = true;
    };
    // Perform delete
    $scope.deleteDevice = function () {
        if (!$scope.deviceToDelete) return;

        $http.post('/Device/DeleteDevice/' + $scope.deviceToDelete.Id)
            .then(function (response) {
                if (response.data.success) {
                    $scope.loadDevices();
                    $scope.showStatus('success', response.data.message);
                } else {
                    $scope.showStatus('error', response.data.error || response.data.message || "Failed to delete device.");
                }
                $scope.closeDeleteConfirmModal();
            }, function (error) {
                console.error('Error deleting device:', error);
                $scope.showStatus('error', error.data?.error || error.data?.message || "An unexpected error occurred.");
                $scope.closeDeleteConfirmModal();
            });
    };

    // Close delete modal
    $scope.closeDeleteConfirmModal = function () {
        $scope.showDeleteConfirmModal = false;
        $scope.deviceToDelete = null;
    };
    // Close modal
    $scope.closeModal = function () {
        $scope.showModal = false;
    };

    $scope.loadDevices();
});
