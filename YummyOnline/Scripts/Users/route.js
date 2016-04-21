app.config(function ($routeProvider) {
	$routeProvider.when('/', {
		templateUrl: '/Users/_ViewCustomer',
		controller: 'CustomerCtrl'
	}).when('/Customer', {
		templateUrl: '/Users/_ViewCustomer',
		controller: 'CustomerCtrl'
	})
	
	.when('/Admin', {
		templateUrl: '/Users/_ViewAdmin',
		controller: 'AdminCtrl'
	});
});