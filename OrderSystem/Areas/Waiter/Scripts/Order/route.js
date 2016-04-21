app.config(function ($routeProvider) {
	$routeProvider.when('/', {
		templateUrl: '/Waiter/Order/_ViewCart',
		controller: 'CartCtrl'
	}).when('/Cart', {
		templateUrl: '/Waiter/Order/_ViewCart',
		controller: 'CartCtrl'
	}).when('/Payment', {
		templateUrl: '/Waiter/Order/_ViewPayment',
		controller: 'PaymentCtrl'
	}).when('/Current', {
		templateUrl: '/Waiter/Order/_ViewCurrent',
		controller: 'CurrentCtrl'
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