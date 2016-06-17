app.config(function ($routeProvider) {
	$routeProvider
	.when('/', {
		templateUrl: '/Hotel/_ViewHotel',
		controller: 'HotelCtrl'
	})
	.when('/Dine/', {
		templateUrl: '/Hotel/_ViewDine',
		controller: 'DineCtrl'
	})
});