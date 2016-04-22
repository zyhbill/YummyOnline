app.config(function ($routeProvider) {
	$routeProvider.when('/IISStatus', {
		templateUrl: '/Server/_ViewIISStatus',
		controller: 'IISStatusCtrl'
	}).when('/TcpServerStatus', {
		templateUrl: '/Server/_ViewTcpServerStatus',
		controller: 'TcpServerStatusCtrl'
	}).when('/Guids', {
		templateUrl: '/Server/_ViewGuids',
		controller: 'GuidsCtrl'
	});
});