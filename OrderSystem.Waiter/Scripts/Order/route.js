app.config(function ($routeProvider) {
	$routeProvider.when('/', {
		templateUrl: '/Order/_ViewCart',
		controller: 'CartCtrl'
	}).when('/Cart', {
		templateUrl: '/Order/_ViewCart',
		controller: 'CartCtrl'
	}).when('/Payment', {
		templateUrl: '/Order/_ViewPayment',
		controller: 'PaymentCtrl'
	})

	// 当前桌台点单
	.when('/Current', {
		templateUrl: '/Order/_ViewCurrent',
		controller: 'CurrentCtrl'
	})
	// 历史点单 共用一个视图
	.when('/History', {
		templateUrl: '/Order/_ViewCurrent',
		controller: 'HistoryCtrl'
	})
});

app.controller('LoadingCtrl', function ($rootScope) {
	$rootScope.$on('$routeChangeStart', function () {
		$rootScope.isLoading = true;
	});
	$rootScope.$on('$routeChangeSuccess', function () {
		$rootScope.isLoading = false;
	});
});