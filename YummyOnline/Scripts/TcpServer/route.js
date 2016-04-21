app.config(function ($routeProvider) {
	$routeProvider.when('/Status', {
		templateUrl: '/TcpServer/_ViewStatus',
		controller: 'StatusCtrl'
	}).when('/Guids', {
		templateUrl: '/TcpServer/_ViewGuids',
		controller: 'GuidsCtrl'
	});
});