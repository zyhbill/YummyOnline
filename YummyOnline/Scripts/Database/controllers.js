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

		function refresh() {
			$http.post('/Database/GetDbPartitionDetailByHotelId', {
				HotelId: $routeParams.hotelId
			}).then(function (response) {
				$scope.partitionDetail = response.data;
			});
		}

		$scope.createDbPartition = function () {
			$http.post('/Database/CreateDbPartition', {
				HotelId: $routeParams.hotelId
			}).then(function (response) {
				if (response.data.Succeeded) {
					toastr.success('成功创建分区表');
					refresh();
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}
		$scope.split = function () {
			$http.post('/Database/SplitPartition', {
				HotelId: $routeParams.hotelId,
				DateTime: $scope.dateTime
			}).then(function (response) {
				if (response.data.Succeeded) {
					toastr.success('成功分割');
					refresh();
				} else {
					toastr.error(response.data.ErrorMessage);
					refresh();
				}
			});
		}

		$scope.merge = function () {
			$http.post('/Database/MergePartition', {
				HotelId: $routeParams.hotelId,
				DateTime: $scope.dateTime
			}).then(function (response) {
				if (response.data.Succeeded) {
					toastr.success('成功合并');
					refresh();
				} else {
					toastr.error(response.data.ErrorMessage);
					refresh();
				}
			});
		}

		refresh();
	}
])