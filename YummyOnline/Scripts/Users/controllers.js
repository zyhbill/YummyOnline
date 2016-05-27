app.controller('CustomerCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('普通用户管理', '');

		$scope.countPerPage = 30;
		$scope.count = 0;
		$scope.currPage = 1;

		$scope.refresh = function () {
			$http.post('/Users/GetCustomers', {
				CountPerPage: $scope.countPerPage,
				CurrPage: $scope.currPage
			}).then(function (response) {
				for (var i in response.data) {
					response.data.IsShowUserDines = false;
					response.data.IsLoading = false;
					response.data.DineHotels = [];
				}
				$scope.customers = response.data.Users;
				$scope.count = response.data.Count;
			});
		}
		$scope.refresh();

		$scope.showUserDines = function (customer) {
			if (!customer.IsShowUserDines) {
				customer.IsLoading = true;
				$http.post('/Users/GetUserDines', {
					UserId: customer.Id
				}).then(function (response) {
					customer.DineHotels = response.data;
					customer.IsShowUserDines = true;
					customer.IsLoading = false;
				});
			} else {
				customer.DineHotels = [];
				customer.IsShowUserDines = false;
			}
		}
	}
]);

app.controller('NemoCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('匿名用户管理', '');

		$scope.countPerPage = 30;
		$scope.count = 0;
		$scope.currPage = 1;

		$scope.refresh = function () {
			$http.post('/Users/GetNemoes', {
				CountPerPage: $scope.countPerPage,
				CurrPage: $scope.currPage
			}).then(function (response) {
				for (var i in response.data) {
					response.data.IsShowUserDines = false;
					response.data.IsLoading = false;
					response.data.DineHotels = [];
				}
				$scope.nemoes = response.data.Users;
				$scope.count = response.data.Count;
			});
		}
		$scope.refresh();

		$scope.deleteNemoesHavenotDine = function () {
			$http.post('/Users/DeleteNemoesHavenotDine').then(function (response) {
				if (response.data.Succeeded) {
					$scope.refresh();
				}
			});
		}
		$scope.showUserDines = function (nemo) {
			if (!nemo.IsShowUserDines) {
				nemo.IsLoading = true;
				$http.post('/Users/GetUserDines', {
					UserId: nemo.Id
				}).then(function (response) {
					nemo.DineHotels = response.data;
					nemo.IsShowUserDines = true;
					nemo.IsLoading = false;
				});
			} else {
				nemo.DineHotels = [];
				nemo.IsShowUserDines = false;
			}
		}
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

