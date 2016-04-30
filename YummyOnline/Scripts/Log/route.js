app.config(function ($routeProvider) {
	$routeProvider
	.when('/Specification', {
		templateUrl: '/Log/_ViewSpecification',
		controller: 'SpecificationCtrl'
	})

	.when('/Hotel', {
		templateUrl: '/Log/_ViewHotel',
		controller: 'HotelCtrl'
	})
	.when('/Hotel/:hotelId', {
		templateUrl: '/Log/_ViewHotel',
		controller: 'HotelCtrl'
	})

	.when('/YummyOnline/', {
		templateUrl: '/Log/_ViewYummyOnline',
		controller: 'YummyOnlineCtrl'
	})
	.when('/YummyOnline/:program', {
		templateUrl: '/Log/_ViewYummyOnline',
		controller: 'YummyOnlineCtrl'
	})
});