app.controller('CartCtrl', [
	'$scope',
	'$rootScope',
	'$http',
	'$window',
	'$uibModal',
	'cart',
	'menuFilter',
	function ($scope, $rootScope, $http, $window, $modal, $cart, $menuFilter) {
		$scope.openRemarkModal = function (menu) {
			var modalInstance = $modal.open({
				templateUrl: 'remarkModal.html',
				controller: 'RemarkModalCtrl',
				resolve: {
					menu: function () {
						return menu;
					}
				}
			});
		}

		$rootScope.openCustomerModal = function () {
			var modalInstance = $modal.open({
				templateUrl: 'customerModal.html',
				controller: 'CustomerModalCtrl'
			});
		}
		$rootScope.signout = function () {
			$http.post('/Account/Signout').then(function (response) {
				if (response.data.Succeeded) {
					window.location.href = '/';
				}
			})
		}

		$cart.Initialize(function () {
			$menuFilter.IntoRankMode();
		});
	}
]);

app.controller('RemarkModalCtrl', function ($scope, $uibModalInstance, menu) {
	$scope.menu = menu;

	$scope.ok = function () {
		$uibModalInstance.close();
	};
});
app.controller('CustomerModalCtrl', [
	'$scope',
	'$http',
	'$uibModalInstance',
	'cart',
	function ($scope, $http, $uibModalInstance, $cart) {
		$scope.signinFormData = {
			PhoneNumber: '',
			Password: '',
		};

		$scope.signin = function () {
			$http.post('/Account/VerifyCustomer', $scope.signinFormData).then(function (response) {
				if (response.data.Succeeded) {
					$cart.Customer = response.data.Data;
					$uibModalInstance.close();
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}

		$scope.ok = function () {
			$uibModalInstance.close();
		};
	}
]);

app.controller('PaymentCtrl', [
	'$scope',
	'dataSet',
	'cart',
	'$http',
	'$location',
	function ($scope, $dataSet, $cart, $http, $location) {
		if (!$cart.IsInitialized) {
			$location.path('/');
		}

		var headCountsAll = [];
		for (var i = 1; i < 50; i++) {
			headCountsAll.push(i);
		}
		$scope.headCountsAll = headCountsAll;


		$scope.canSubmit = function () {
			var result = $cart.CanSubmit() && !$scope.isSubmitting && $cart.PayKind != null;
			if ($cart.Customer != null && $dataSet.Hotel.PointsRatio > 0) {
				result = result && $cart.Customer.Points / $dataSet.Hotel.PointsRatio >= $cart.PriceInPoints;
			}
			return result;
		}

		$scope.isSubmitting = false;

		$scope.pay = function () {
			if (!$scope.canSubmit()) return;

			if ($scope.isSubmitting) return;
			$scope.isSubmitting = true;

			$cart.Submit().then(function (data) {
				if (data.Succeeded) {
					toastr.success('下单成功');
					$cart.SubmitSucceeded();
					$location.path('/');
				} else {
					toastr.error(data.ErrorMessage);
				}
				$scope.isSubmitting = false;
			}, function () {
				toastr.error('网络连接失败，请稍候重试');
				$scope.isSubmitting = false;
			});
		}
	}
]);

app.controller('CurrentCtrl', [
	'$scope',
	'$http',
	'$location',
	'cart',
	function ($scope, $http, $location, $cart) {
		if (!$cart.IsInitialized) {
			$location.path('/');
		}

		$scope.dines = null;
		var getCurrentDines = function (desk) {
			$http.post('/Order/GetCurrentDines', {
				DeskId: desk.Id
			}).then(function (response) {
				$scope.dines = response.data;
			});
		}

		$scope.$watch(function () {
			return $cart.Desk
		}, function () {
			if ($cart.Desk != null) {
				getCurrentDines($cart.Desk);
			}
		}, true);
	}
]).controller('HistoryCtrl', [
	'$scope',
	'$http',
	'$location',
	'cart',
	function ($scope, $http, $location, $cart) {
		if (!$cart.IsInitialized) {
			$location.path('/');
		}

		$http.post('/Order/GetHistoryDines').then(function (response) {
			$scope.dines = response.data;
		});
	}
]);