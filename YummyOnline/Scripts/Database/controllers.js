app.controller('PartitionDetailCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('分区表概览', '');

		$http.post('/Database/GetDbPartitionDetails').then(function (response) {
			$scope.partitionDetails = response.data;
		})
	}
])
.controller('PartitionHandleCtrl', [
	'$scope',
	'$http',
	'$routeParams',
	'layout',
	function ($scope, $http, $routeParams, $layout) {
		$layout.Set('分区', '');

		$http.post('/Hotel/GetHotelNames').then(function (response) {
			$scope.hotels = response.data;
		});

		$http.post('/Database/GetDbPartitionDetailByHotelId', {
			HotelId: $routeParams.hotelId
		}).then(function (response) {
			$scope.partitionDetail = response.data;
		})
	}
])