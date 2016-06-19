app.config(function ($routeProvider) {
	$routeProvider
	.when('/Backup', {
		templateUrl: '/Database/_ViewBackup',
		controller: 'BackupCtrl'
	})

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