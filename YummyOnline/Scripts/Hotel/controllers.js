app.controller('HotelCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('饭店管理', '');

		function refresh() {
			$http.post('/Hotel/GetHotels').then(function (response) {
				$scope.hotels = response.data;
				for (var i in $scope.hotels) {
					$scope.hotels[i].IsReadyForConfirm = false;
					if ($scope.hotels[i].ConnectionString == null && !$scope.hotels[i].Usable) {
						$scope.hotels[i].IsReadyForConfirm = true;
						$scope.hotels[i].DatabaseName = 'YummyOnlineHotel' + $scope.hotels[i].Id;
					}
				}
			})
		}

		refresh();

		$scope.update = function (hotel) {
			$http.post('/Hotel/UpdateHotel', {
				Id: hotel.Id,
				CssThemePath: hotel.CssThemePath,
				ConnectionString: hotel.ConnectionString,
				AdminConnectionString: hotel.AdminConnectionString,
				Usable: hotel.Usable
			}).then(function (response) {
				if (response.data.Succeeded) {
					toastr.success('修改成功');
					hotel.IsShowUpdate = false;
				} else {
					toastr.error('修改失败');
					refresh();
				}
			})
		}
		$scope.create = function (hotel) {
			$http.post('/Hotel/CreateHotel', {
				HotelId: hotel.Id,
				DatabaseName: hotel.DatabaseName,
			}).then(function (response) {
				if (response.data.Succeeded) {
					toastr.success('创建成功');
					refresh();
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			})
		}
	}
]);