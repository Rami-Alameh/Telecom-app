app.directive('clientTypeFilter', function () {
    return {
        restrict: 'E',
        scope: {
            selectedType: '=',
            onTypeChange: '&', // changes data being got
            label: '@'
        },
        template: `
            <div class="form-group">
                <label ng-if="label">{{label}}</label>
                <select class="form-control" ng-model="selectedType" ng-change="typeChanged()">
                    <option value="">All Types</option>
                    <option ng-repeat="type in clientTypes" value="{{type.value}}">
                        {{type.label}}
                    </option>
                </select>
            </div>
        `,
        controller: function ($scope) {
            $scope.clientTypes = [
                { value: 0, label: 'Individual' },
                { value: 1, label: 'Organization' }
            ];

            $scope.typeChanged = function () {
                if ($scope.onTypeChange) {
                    // Calls the passed function with the selected type
                    $scope.onTypeChange({ type: $scope.selectedType });
                }
            };
        }
    };
});
