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
					var h = $scope.hotels[i];
					h.NewData = {
						CssThemePath: h.CssThemePath,
						ConnectionString: h.ConnectionString,
						AdminConnectionString: h.AdminConnectionString,
						Usable: h.Usable
					}
					h.IsReadyForConfirm = false;
					if (h.ConnectionString == null && !h.Usable) {
						h.IsReadyForConfirm = true;
						h.DatabaseName = 'YummyOnlineHotel' + h.Id;
					}
				}
			})
		}

		refresh();

		$scope.update = function (hotel) {
			$http.post('/Hotel/UpdateHotel', {
				Id: hotel.Id,
				CssThemePath: hotel.NewData.CssThemePath,
				ConnectionString: hotel.NewData.ConnectionString,
				AdminConnectionString: hotel.NewData.AdminConnectionString,
				Usable: hotel.NewData.Usable
			}).then(function (response) {
				if (response.data.Succeeded) {
					toastr.success('修改成功');
					refresh();
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
]).controller('DineCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('订单查询', '');

		$scope.dines = [];
		$http.post('/Hotel/GetHotelNames').then(function (response) {
			$scope.hotels = response.data;
			$scope.hotelId = $scope.hotels[0].Id;
		});

		$scope.search = function () {
			$scope.isLoading = true;
			var dineIds = $scope.dineIds.split(/\s+/);
			$http.post('/Hotel/GetDines', {
				HotelId: $scope.hotelId,
				DineIds: dineIds
			}).then(function (response) {
				$scope.dines = response.data;
				$scope.isLoading = false;
			}, function (response) {
				$scope.isLoading = false;
			});
		}
	}
])