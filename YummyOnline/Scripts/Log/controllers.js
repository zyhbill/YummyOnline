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
			for (var i in $scope.programs) {
				if ($scope.programs[i].Id == $route.current.params.ProgramId) {
					$scope.loadProgramLog($scope.programs[i]);
					return;
				}
			}
			$scope.loadProgramLog($scope.programs[0]);
		});

		$scope.loadProgramLog = function (program) {
			$scope.currProgram = program;
			$http.post('/Log/GetYummyOnlineLogs', { Program: program.Id }).then(function (response) {
				var data = response.data;
				for (var i in data) {
					data[i].Class = _getCssClass(data[i].Level);
				}
				$scope.logs = data;
			});
		};

		$scope.deleteLog = function () {
			$http.post('/Log/DeleteLogs').then(function (response) {
				$scope.loadProgramLog($scope.currProgram);
			});
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
			for (var i in $scope.hotels) {
				if ($scope.hotels[i].Id == $route.current.params.HotelId) {
					$scope.loadHotelLog($scope.hotels[i]);
					return;
				}
			}
			$scope.loadHotelLog($scope.hotels[0]);
		});

		$scope.loadHotelLog = function (hotel) {
			$scope.currHotel = hotel;
			$http.post('/Log/GetHotelLogs', { HotelId: hotel.Id }).then(function (response) {
				var data = response.data;
				for (var i in data) {
					data[i].Class = _getCssClass(data[i].Level);
				}
				$scope.logs = data;
			});
		};
	}
]);

app.controller('SpecificationCtrl', [
	'$scope',
	'layout',
	function ($scope, $layout) {
		$layout.Set('文档', '');
	}
])