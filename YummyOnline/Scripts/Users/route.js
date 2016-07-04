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
	}).when('/Nemo', {
		templateUrl: '/Users/_ViewNemo',
		controller: 'NemoCtrl'
	}).when('/HotelAdmin', {
		templateUrl: '/Users/_ViewHotelAdmin',
		controller: 'HotelAdminCtrl'
	});
});