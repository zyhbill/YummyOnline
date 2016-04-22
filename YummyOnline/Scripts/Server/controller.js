app.controller('IISStatusCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('IIS使用情况', '');

		function refresh() {
			$http.post('/Server/GetIISInfo').then(function (response) {
				$scope.iisInfos = response.data;
			});
		}

		refresh();

		$scope.startSite = function (id) {
			$http.post('/Server/StartSite', {
				SiteId: id
			}).then(function (response) {
				if (response.data.Succeeded) {
					refresh();
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}

		$scope.stopSite = function (id) {
			$http.post('/Server/StopSite', {
				SiteId: id
			}).then(function (response) {
				if (response.data.Succeeded) {
					refresh();
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}
	}
])

app.controller('TcpServerStatusCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('Tcp服务器使用情况', '');

		$scope.refreshTcpServerInfo = function () {
			$http.post('/Server/GetTcpServerInfo').then(function (response) {
				var data = response.data;
				if (data == '') {
					$scope.tcpServerInfo = null;
				} else {
					$scope.tcpServerInfo = data;
				}
			});
		}

		$scope.refreshTcpServerInfo();

		$scope.startTcpServer = function () {
			$http.post('/Server/StartTcpServer').then(function (response) {
				if (response.data.Succeeded) {
					$scope.refreshTcpServerInfo();
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}
		$scope.stopTcpServer = function () {
			$http.post('/Server/StopTcpServer').then(function (response) {
				if (response.data.Succeeded) {
					$scope.refreshTcpServerInfo();
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}
	}
]);

app.controller('GuidsCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('Tcp Guid授权', '');

		$scope.newGuid = {
			Guid: null,
			Description: null,
		}
		$scope.addGuid = function () {
			$http.post('/Server/AddGuid', $scope.newGuid).then(function (response) {
				if (response.data.Succeeded) {
					refresh();
				} else {
					toastr.error('添加失败');
				}
			});
		}
		$scope.deleteGuid = function (guid) {
			$http.post('/Server/DeleteGuid', {
				Guid: guid.Guid
			}).then(function (response) {
				if (response.data.Succeeded) {
					refresh();
				} else {
					toastr.error('删除失败');
				}
			});
		}

		function refresh() {
			$http.post('/Server/GetGuids').then(function (response) {
				$scope.guids = response.data;
			});
		}
		refresh();
	}
])