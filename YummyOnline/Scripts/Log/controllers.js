function _getCssClass(level) {
	switch (level) {
		case 'Error':
			return 'danger';
		case 'Info':
			return 'info';
		case 'Success':
			return 'success';
		case 'Warning':
			return 'warning';
		default:
			return null;
	}
}

app.controller('YummyOnlineCtrl', [
	'$rootScope',
	'$scope',
	'$http',
	'$routeParams',
	'layout',
	function ($rootScope, $scope, $http, $routeParams, $layout) {
		$layout.Set('系统日志', '');

		if ($rootScope.currCount == undefined) {
			$rootScope.currCount = 100;
			$rootScope.currDateTime = new Date();
		}

		$http.post('/Log/GetYummyOnlinePrograms').then(function (response) {
			$scope.programs = response.data;
		});

		$scope.refresh = function () {
			$scope.isLoading = true;
			$http.post('/Log/GetYummyOnlineLogs', {
				Program: $routeParams.program,
				DateTime: $rootScope.currDateTime,
				Count: $rootScope.currCount
			}).then(function (response) {
				var data = response.data;
				for (var i in data.Logs) {
					data.Logs[i].Class = _getCssClass(data.Logs[i].Level);
				}
				$scope.logDetail = data;
				$scope.isLoading = false;
			});
		}

		$scope.$watch('currDateTime', function () {
			$rootScope.currDateTime = $scope.currDateTime;
			$scope.refresh();
		});

		$scope.changeLogCount = function (count) {
			$rootScope.currCount = count == undefined ? null : count;
			$scope.refresh();
		}
	}
]).controller('HotelCtrl', [
	'$rootScope',
	'$scope',
	'$http',
	'$routeParams',
	'layout',
	function ($rootScope, $scope, $http, $routeParams, $layout) {
		$layout.Set('饭店日志', '');

		if ($rootScope.currCount == undefined) {
			$rootScope.currCount = 100;
			$rootScope.currDateTime = new Date();
		}

		$http.post('/Hotel/GetHotelNames').then(function (response) {
			$scope.hotels = response.data;
		});

		$scope.refresh = function () {
			$scope.isLoading = true;
			$http.post('/Log/GetHotelLogs', {
				HotelId: $routeParams.hotelId,
				DateTime: $rootScope.currDateTime,
				Count: $rootScope.currCount
			}).then(function (response) {
				var data = response.data;
				for (var i in data.Logs) {
					data.Logs[i].Class = _getCssClass(data.Logs[i].Level);
				}
				$scope.logDetail = data;
				$scope.isLoading = false;
			});
		}

		$scope.$watch('currDateTime', function () {
			$rootScope.currDateTime = $scope.currDateTime;
			$scope.refresh();
		});
		$scope.changeLogCount = function (count) {
			$rootScope.currCount = count == undefined ? null : count;
			$scope.refresh();
		}
	}
]);

app.controller('SpecificationCtrl', [
	'$scope',
	'layout',
	function ($scope, $layout) {
		$layout.Set('文档', '');
	}
])