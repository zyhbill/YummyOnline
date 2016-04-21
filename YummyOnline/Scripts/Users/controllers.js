app.controller('CustomerCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('普通用户管理', '');
	}
]);

app.controller('AdminCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('管理员管理', '');

		function refresh() {
			$http.post('/Users/GetAdmins').then(function (response) {
				$scope.admins = response.data;
			});
		}
		refresh();

		$scope.newAdminPhoneNumber = null;
		$scope.addAdmin = function () {
			$http.post('/Users/AddAdmin', {
				PhoneNumber: $scope.newAdminPhoneNumber
			}).then(function (response) {
				if (response.data.Succeeded) {
					refresh();
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}
		$scope.deleteAdmin = function (admin) {
			$http.post('/Users/DeleteAdmin', {
				Id: admin.Id
			}).then(function (response) {
				if (response.data.Succeeded) {
					refresh();
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}
	}
]);