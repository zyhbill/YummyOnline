app.config(function ($routeProvider) {
	$routeProvider.when('/', {
		templateUrl: '/Order/_ViewMenu',
		controller: 'MenuCtrl'
	}).when('/Menu', {
		templateUrl: '/Order/_ViewMenu',
		controller: 'MenuCtrl'
	})

	.when('/Cart', {
		templateUrl: '/Order/_ViewCart',
		controller: 'CartCtrl'
	}).when('/Payment', {
		templateUrl: '/Order/_ViewPayment',
		controller: 'PaymentCtrl'
	})

	.when('/History', {
		templateUrl: '/Order/_ViewHistory',
		controller: 'HistoryCtrl'
	});
});

app.controller('LoadingCtrl', function ($rootScope) {
	$rootScope.$on('$routeChangeStart', function () {
		$rootScope.isLoading = true;
	});
	$rootScope.$on('$routeChangeSuccess', function () {
		$rootScope.isLoading = false;
	});
});