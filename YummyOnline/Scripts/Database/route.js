app.config(function ($routeProvider) {
	$routeProvider
	.when('/PartitionDetail', {
		templateUrl: '/Database/_ViewPartitionDetail',
		controller: 'PartitionDetailCtrl'
	})

	.when('/PartitionHandle', {
		templateUrl: '/Database/_ViewPartitionHandle',
		controller: 'PartitionHandleCtrl'
	})
	.when('/PartitionHandle/:hotelId', {
		templateUrl: '/Database/_ViewPartitionHandle',
		controller: 'PartitionHandleCtrl'
	})

	.when('/Execution', {
		templateUrl: '/Database/_ViewExecution',
		controller: 'ExecutionCtrl'
	})
});