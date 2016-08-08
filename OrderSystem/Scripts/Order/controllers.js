app.controller('MenuCtrl', [
	'$scope',
	'$rootScope',
	'$uibModal',
	'cart',
	'menuFilter',
	'setMealFilter',
	function ($scope, $rootScope, $modal, $cart, $menuFilter, $setMealFilter) {
		$scope.showMenuModal = function (menu) {
			$modal.open({
				templateUrl: 'menuModal.html',
				controller: 'MenuModalCtrl',
				resolve: {
					menu: function () {
						return menu;
					}
				}
			});
		}

		$scope.showSetMealsModal = function (menu) {
			if (menu.IsSetMeal) {
				$modal.open({
					templateUrl: 'setMealsModal.html',
					controller: 'SetMealsCtrl',
					backdrop: 'static',
					resolve: {
						menu: function () {
							return menu;
						},
						index: null
					}
				});
			}
		}
		$cart.Initialize(function () {
			$menuFilter.IntoRankMode();
		});
	}
]);

app.controller('MenuModalCtrl', [
	'$scope',
	'$uibModalInstance',
	'menu',
	function ($scope, $modalInstance, $menu) {
		$scope.menu = $menu;
		$scope.close = function () {
			$modalInstance.dismiss();
		};
	}
]);

app.controller('SetMealsCtrl', [
	'$scope',
	'$uibModalInstance',
	'menu',
	'index',
	'cart',
	'setMealFilter',
	function ($scope, $modalInstance, $menu, $index, $cart, $setMealFilter) {
		$scope.menu = $menu;

		$scope.cancel = function () {
			$cart.RemoveMenu($menu)
			$modalInstance.dismiss();
		}

		$scope.ok = function () {
			$modalInstance.dismiss();
		}

		if ($index == null) {
			$index = $menu.Addition.OrderedSetMealClasses.length - 1;
		}
		$setMealFilter.ToggleSetMealSelected($menu.Addition.OrderedSetMealClasses[$index]);
	}
]);

app.controller('CartCtrl', [
	'$scope',
	'$rootScope',
	'$location',
	'$uibModal',
	'cart',
	'setMealFilter',
	function ($scope, $rootScope, $location, $modal, $cart, $setMealFilter) {
		$cart.Initialize();

		$scope.gotoPayment = function () {
			if (!$setMealFilter.IsSetMealAllOrdered()) {
				var result = confirm('套餐中尚有未点的菜品, 是否仍然下单?');
				if (!result)
					return;
			}

			$location.path('/Payment');
		}

		$scope.showSetMealsModal = function (menu, index) {
			console.log(index);
			if (menu.IsSetMeal) {
				$modal.open({
					templateUrl: 'setMealsModal.html',
					controller: 'SetMealsCtrl',
					backdrop: 'static',
					resolve: {
						menu: function () {
							return menu;
						},
						index: function () {
							return index;
						}
					}
				});
			}
		}
	}
]);


app.controller('PaymentCtrl', [
	'$scope',
	'dataSet',
	'cart',
	'$http',
	'$location',
	'$window',
	function ($scope, $dataSet, $cart, $http, $location, $window) {
		$cart.Initialize();

		var headCountsAll = [];
		for (var i = 1; i < 50; i++) {
			headCountsAll.push(i);
		}
		$scope.headCountsAll = headCountsAll;

		$http.post('/Account/IsAuthenticated').then(function (response) {
			if (response.data.Succeeded) {
				$cart.Customer = response.data.Data;
			}
		});

		$scope.canSubmit = function () {
			var result = $cart.CanSubmit() && !$scope.isSubmitting && $cart.PayKind != null;
			if ($cart.Customer != null && $dataSet.Hotel.PointsRatio > 0) {
				result = result && $cart.Customer.Points / $dataSet.Hotel.PointsRatio >= $cart.PriceInPoints;
			}
			return result;
		}

		$scope.isSubmitting = false;

		$scope.pay = function () {
			if ($scope.isSubmitting) return;
			$scope.isSubmitting = true;

			$cart.Submit().then(function (data) {
				if (data.Succeeded) {
					$cart.Reset();
					$window.location.href = data.RedirectUrl;
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


app.controller('HistoryCtrl', [
	'$rootScope',
	'$scope',
	'$http',
	'$window',
	'$location',

	'cart',
	'dineToCart',
	function ($rootScope, $scope, $http, $window, $location, $cart, $dineToCart) {
		$cart.Initialize();
		$rootScope.isLoading = true;
		$http.post('/Order/GetHistoryDines').then(function (response) {
			$scope.dines = response.data;
			$rootScope.isLoading = false;
		});

		$scope.tryAgain = function (dine) {
			$dineToCart(dine);
			$location.path('/Cart');
		}
		$scope.payAgain = function (dine) {
			$http.post('/Payment/PayAgain', { DineId: dine.Id }).then(function (response) {
				$window.location.href = response.data.RedirectUrl;
			});
		}
	}
]);