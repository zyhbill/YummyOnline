app.controller('StatusCtrl', [
	'$scope',
	'$http',
	'layout',
	function ($scope, $http, $layout) {
		$layout.Set('Tcp服务器使用情况', '');
		$scope.refreshTcpServerInfo = function () {
			$http.post('/TcpServer/GetTcpServerInfo').then(function (response) {
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
			$http.post('/TcpServer/StartTcpServer').then(function (response) {
				if (response.data.Succeeded) {
					$scope.refreshTcpServerInfo();
				}
			});
		}
		$scope.stopTcpServer = function () {
			$http.post('/TcpServer/StopTcpServer').then(function (response) {
				if (response.data.Succeeded) {
					$scope.refreshTcpServerInfo();
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
			$http.post('/TcpServer/AddGuid', $scope.newGuid).then(function (response) {
				if (response.data.Succeeded) {
					refresh();
				} else {
					toastr.error('添加失败');
				}
			});
		}
		$scope.deleteGuid = function (guid) {
			$http.post('/TcpServer/DeleteGuid', {
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
			$http.post('/TcpServer/GetGuids').then(function (response) {
				$scope.guids = response.data;
			});
		}
		refresh();
	}
])