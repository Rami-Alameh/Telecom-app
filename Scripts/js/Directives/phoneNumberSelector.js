app.directive('phoneNumberSelector', function ($http) {
    return {
        restrict: 'E',
        scope: {
            selectedId: '=',
            label: '@'
        },
        template: `
            <div class="form-group">
                <label>{{label || 'Phone Number'}}</label>
                <select class="form-control"
                        ng-model="selectedId"
                        ng-options="phone.Id as phone.Number for phone in phoneNumbers">
                    <option value="">-- Select Phone Number --</option>
                </select>
            </div>
        `,
        controller: function ($scope) {
            $scope.phoneNumbers = [];

            $scope.loadPhoneNumbers = function () {
                $http.get('/PhoneNumber/GetPhoneNumbers')
                    .then(function (response) {
                        $scope.phoneNumbers = response.data;
                    });
            };

            $scope.getData = function () {
                return $scope.selectedId;
            };

            // Initial load
            $scope.loadPhoneNumbers();
        }
    };
});

// This will go in a new file: ~/Scripts/js/Directives/clientSelector.js
app.directive('clientSelector', function ($http) {
    return {
        restrict: 'E',
        scope: {
            selectedId: '=',
            label: '@'
        },
        template: `
            <div class="form-group">
                <label>{{label || 'Client'}}</label>
                <select class="form-control"
                        ng-model="selectedId"
                        ng-options="client.Id as client.Name for client in clients">
                    <option value="">-- Select Client --</option>
                </select>
            </div>
        `,
        controller: function ($scope) {
            $scope.clients = [];

            $scope.loadClients = function () {
                $http.get('/Client/GetClients')
                    .then(function (response) {
                        $scope.clients = response.data;
                    });
            };

            $scope.getData = function () {
                return $scope.selectedId;
            };

            // Initial load
            $scope.loadClients();
        }
    };
});
app.directive('phoneNumberSearch', function ($http, $timeout) {
    return {
        restrict: 'E',
        scope: {
            selectedId: '=',
            label: '@'
        },
        template: `
            <div class="form-group">
                <label>{{label || 'Phone Number'}}</label>
                <input type="text" class="form-control" 
                       ng-model="searchQuery" 
                       ng-keyup="searchPhoneNumbers()"
                       placeholder="Type to search phone numbers...">
                
                <div class="list-group mt-2" ng-if="searchResults.length > 0">
                    <a href="#" class="list-group-item list-group-item-action" 
                       ng-repeat="phone in searchResults" 
                       ng-click="selectPhone(phone)">
                        {{phone.Number}}
                    </a>
                </div>
                
                <div class="mt-2" ng-if="selectedPhone">
                    <strong>Selected:</strong> {{selectedPhone.Number}}
                    <button class="btn btn-sm btn-link text-danger" ng-click="clearSelection()">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            </div>
        `,
        controller: function ($scope) {
            $scope.searchQuery = '';
            $scope.searchResults = [];
            $scope.selectedPhone = null;
            $scope.debounceTimer = null;

            $scope.searchPhoneNumbers = function () {
                if ($scope.debounceTimer) {
                    $timeout.cancel($scope.debounceTimer);
                }

                if (!$scope.searchQuery) {
                    $scope.searchResults = [];
                    return;
                }

                $scope.debounceTimer = $timeout(function () {
                    $http.get('/PhoneNumber/Search', {
                        params: { query: $scope.searchQuery }
                    }).then(function (response) {
                        $scope.searchResults = response.data || [];
                    });
                }, 300);
            };

            $scope.selectPhone = function (phone) {
                $scope.selectedPhone = phone;
                $scope.selectedId = phone.Id;
                $scope.searchResults = [];
            };

            $scope.clearSelection = function () {
                $scope.selectedPhone = null;
                $scope.selectedId = null;
                $scope.searchQuery = '';
            };
        }
    };
});