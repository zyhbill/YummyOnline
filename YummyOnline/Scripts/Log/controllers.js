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
	'$scope',
	'$http',
	'$route',
	'layout',
	function ($scope, $http, $route, $layout) {
		$layout.Set('系统日志', '');

		$http.post('/Log/GetYummyOnlinePrograms').then(function (response) {
			$scope.programs = response.data;

			$scope.currProgram = $scope.programs[0];
			$scope.currCount = 100;
			$scope.currDateTime = new Date();
		});

		$scope.refresh = function () {
			if ($scope.currProgram == null)
				return;

			$http.post('/Log/GetYummyOnlineLogs', {
				Program: $scope.currProgram.Id,
				DateTime: $scope.currDateTime,
				Count: $scope.currCount
			}).then(function (response) {
				var data = response.data;
				for (var i in data) {
					data[i].Class = _getCssClass(data[i].Level);
				}
				$scope.logs = data;
			});
		}

		$scope.$watch('currDateTime', function () {
			$scope.refresh();
		});
		$scope.changeLogProgram = function (program) {
			$scope.currProgram = program;
			$scope.refresh();
		}
		$scope.changeLogCount = function (count) {
			$scope.currCount = count == undefined ? null : count;
			$scope.refresh();
		}
	}
]).controller('HotelCtrl', [
	'$scope',
	'$http',
	'$route',
	'layout',
	function ($scope, $http, $route, $layout) {
		$layout.Set('饭店日志', '');

		$http.post('/Hotel/GetHotelNames').then(function (response) {
			$scope.hotels = response.data;

			$scope.currHotel = $scope.hotels[0];
			$scope.currCount = 100;
			$scope.currDateTime = new Date();
		});

		$scope.refresh = function () {
			if ($scope.currHotel == null)
				return;

			$http.post('/Log/GetHotelLogs', {
				HotelId: $scope.currHotel.Id,
				DateTime: $scope.currDateTime,
				Count: $scope.currCount
			}).then(function (response) {
				var data = response.data;
				for (var i in data) {
					data[i].Class = _getCssClass(data[i].Level);
				}
				$scope.logs = data;
			});
		}

		$scope.$watch('currDateTime', function () {
			$scope.refresh();
		});
		$scope.changeLogHotel = function (hotel) {
			$scope.currHotel = hotel;
			$scope.refresh();
		}
		$scope.changeLogCount = function (count) {
			$scope.currCount = count == undefined ? null : count;
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