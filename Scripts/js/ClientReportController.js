app.controller("ClientsReportController", function ($scope, $http) {
    $scope.selectedType = null;
    $scope.clientCounts = [];
    $scope.filteredClients = [];

    // Load report data based on type filter
    $scope.loadReport = function (type) {
        $scope.selectedType = type;

        if (!type) {
            // Load summary counts
            $http.get('/Report/GetClientsByType')
                .then(function (response) {
                    
                    $scope.clientCounts = response.data;
                });
        } else {
            // Load filtered clients 
            $http.get('/Report/GetClientDetails', { params: { typeFilter: type } })
                .then(function (response) {
                    $scope.filteredClients = response.data.map(function (client) {
                        if (client.BirthDate) {
                            client.BirthDate = new Date(client.BirthDate);
                        }
                        return client;
                    });
                });
        }
    };

    // Initial load
    $scope.loadReport(null);
});