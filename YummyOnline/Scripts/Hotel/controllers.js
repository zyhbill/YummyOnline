app.controller('HotelCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('饭店管理', '');

		function refresh() {
			$http.post('/Hotel/GetHotels').then(function (response) {
				$scope.hotels = response.data;
			})
		}

		refresh();

		$scope.update = function (hotel) {
			$http.post('/Hotel/UpdateHotelUsable', {
				Hotel: hotel
			}).then(function (response) {
				if (!response.data.Succeeded) {
					toastr.error('修改失败');
					refresh();
				}
			})
		}
	}
]);