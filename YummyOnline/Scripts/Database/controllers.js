app.controller('BackupCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('备份', '');

		function refresh() {
			$http.post('/Database/GetBackups').then(function (response) {
				$scope.backups = response.data;
			})
		}

		$http.post('/Hotel/GetHotelNames').then(function (response) {
			for (var i in response.data) {
				response.data[i].Checked = true;
			}
			$scope.yummyonline = true;
			$scope.hotels = response.data;
		});
		refresh();


		$scope.execute = function () {
			$scope.isLoading = true;
			var hotelIds = [];
			for (var i in $scope.hotels) {
				if ($scope.hotels[i].Checked) {
					hotelIds.push($scope.hotels[i].Id);
				}
			}
			$http.post('/Database/Backup', {
				HotelIds: hotelIds,
				IsYummyOnline: $scope.yummyonline
			}).then(function (response) {
				if (response.data.Succeeded) {
					toastr.success('备份成功');
				} else {
					toastr.error(response.data.ErrorMessage);
				}
				$scope.isLoading = false;
				refresh();
			});
		}
	}
]);

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
]);

app.controller('ExecutionCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('SQL批量执行', '');

		$http.post('/Hotel/GetHotelNames').then(function (response) {
			for (var i in response.data) {
				response.data[i].Checked = true;
			}
			$scope.hotels = response.data;
		});

		$scope.execute = function () {
			var hotelIds = [];
			for (var i in $scope.hotels) {
				if ($scope.hotels[i].Checked) {
					hotelIds.push($scope.hotels[i].Id);
				}
			}
			$http.post('/Database/ExecuteSql', {
				HotelIds: hotelIds,
				Sql: $scope.sql
			}).then(function (response) {
				if (response.data.Succeeded) {
					toastr.success('执行成功');
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}
	}
])