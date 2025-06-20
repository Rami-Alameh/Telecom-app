app.controller("PhoneNumbersReportController", function ($scope, $http) {
    $scope.devices = [{ Id: null, Name: 'All Devices' }];
    $scope.statuses = [
        { value: null, label: 'All Statuses' },
        { value: true, label: 'Reserved' },
        { value: false, label: 'Unreserved' }
    ];

    $scope.selectedDevice = null;
    $scope.selectedStatus = null;
    $scope.phoneNumberStats = [];
    $scope.loading = false;

    // Load all devices for filter
    $scope.loadDevices = function () {
        $http.get('/Device/GetDevices').then(function (response) {
            $scope.devices = [{ Id: null, Name: 'All Devices' }].concat(response.data);
        }, function (error) {
            console.error('Error loading devices:', error);
        });
    };

    // Load report data based on filters
    $scope.loadReport = function () {
        $scope.loading = true;

        $http.get('/Report/GetPhoneNumbersByDeviceAndStatus', {
            params: {
                deviceId: $scope.selectedDevice,
                isReserved: $scope.selectedStatus
            }
        }).then(function (response) {
            $scope.phoneNumberStats = response.data;
            $scope.loading = false;
        }, function (error) {
            console.error('Error loading phone numbers report:', error);
            alert('Failed to load phone numbers report');
            $scope.loading = false;
        });
    };

    // Initialize
    $scope.loadDevices();
    $scope.loadReport();

    // Update report when filters change
    $scope.$watchGroup(['selectedDevice', 'selectedStatus'], function () {
        $scope.loadReport();
    });
});