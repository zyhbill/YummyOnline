app.config(function ($routeProvider) {
	$routeProvider
	.when('/', {
		templateUrl: '/DashBoard/_ViewDashBoard',
		controller: 'DashBoardCtrl'
	})

	.when('/Monitor', {
		templateUrl: '/DashBoard/_ViewMonitor',
		controller: 'MonitorCtrl'
	})
});