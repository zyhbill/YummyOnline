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
	}).when('/Current', {
		templateUrl: '/Order/_ViewCurrent',
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