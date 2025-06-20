// Define your AngularJS module and include 'ui.bootstrap' for modal support
angular.module('deviceApp', ['ui.bootstrap'])

    .controller('DeviceController', function ($scope, $uibModal) {
        // ========================
        // Initialization
        // ========================
        $scope.devices = [];             // Main array of device objects
        $scope.filteredDevices = [];     // Filtered list based on search
        $scope.searchText = "";          // Text bound to search input
        $scope.isModalOpen = false;      // Controls background blur
        let idCounter = 1;               //  ID generator for new devices

        // ========================
        // Search Functionality
        // ========================
        $scope.searchDevice = function () {
            var query = $scope.searchText.toLowerCase();

            // If input is blank, show all devices
            if (!query) {
                $scope.filteredDevices = $scope.devices;
                return;
            }

            // Filter devices by name containing the search text
            $scope.filteredDevices = $scope.devices.filter(function (device) {
                return device.name.toLowerCase().includes(query);
            });
        };

        // ========================
        // Open Modal (Add or Edit)
        // ========================
        $scope.openModal = function (existingDevice) {
            $scope.isModalOpen = true; // Enable background blur

            var modalInstance = $uibModal.open({
                templateUrl: 'deviceModal.html',   // HTML template inside index.html
                controller: 'ModalController',     // Handles modal logic
                resolve: {
                    // Pass a copy of the existing device or a new one
                    device: function () {
                        return existingDevice ? angular.copy(existingDevice) : { name: '' };
                    },
                    // Flag to determine whether it's add or edit
                    isEdit: function () {
                        return !!existingDevice;
                    }
                }
            });

     
            // adding and editing and functionality to close model 
           
            modalInstance.result.then(function (resultDevice) {
                if (existingDevice) {
                    // Update existing device with new name
                    existingDevice.name = resultDevice.name;
                } else {
                    // Add a new device with a new ID
                    resultDevice.id = idCounter++;
                    $scope.devices.push(resultDevice);
                }

                // Update filtered list after change
                $scope.searchDevice();
                $scope.isModalOpen = false; // Remove background blur
            }, function () {
                // Modal dismissed 
                $scope.isModalOpen = false; // Remove background blur
            });
        };
    })


   
    // Modal Controller (Handles modal form logic)
    .controller('ModalController', function ($scope, $uibModalInstance, device, isEdit) {
        $scope.device = device;                    // Device passed from parent
        $scope.isEdit = isEdit;                    // True if editing, false if adding
        $scope.modalTitle = isEdit ? 'Edit Device' : 'Add Device';  // Modal header title

        // Called when user clicks 'Add' or 'Update'
        $scope.save = function () {
            // Only save if name is not empty
            if ($scope.device.name.trim()) {
                $uibModalInstance.close($scope.device); // Send data back to parent
            }
        };

        // Called when user clicks 'Close' or clicks outside modal
        $scope.cancel = function () {
            $uibModalInstance.dismiss('cancel'); // Cancel modal without saving
        };
    });
