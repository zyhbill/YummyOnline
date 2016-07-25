var app = angular.module('app', []);

app.controller('MainCtrl', [
	'$scope',
	function ($scope) {
		try {
			var result = window.external.app.initialize();
			result = JSON.parse(result);

			$scope.hotelId = result.Id;
			$scope.hotelName = result.Name;
		} catch (e) { }


		$scope.logs = [];
		$scope.ipPrinterLogs = [];
		$scope.ipPrinterStatuses = [];

		$scope.localIP = '';
		$scope.recipt = true;
		$scope.serveOrder = true,
		$scope.kitchenOrder = true,

		$scope.testLocalDines = function () {
			window.external.app.testLocalDines(
				$scope.localIP, $scope.recipt, $scope.serveOrder, $scope.kitchenOrder);
		}
		$scope.testRemoteDines = function () {
			window.external.app.testRemoteDines(
				$scope.localIP, $scope.recipt, $scope.serveOrder, $scope.kitchenOrder);
		}
		$scope.connectPrinter = function () {
			window.external.app.connectPrinter($scope.localIP);
		}
		$scope.connectPrinters = function () {
			window.external.app.connectPrinters($scope.localIP);
		}
	}
])


var addLog = function (log) {
	var appElement = document.querySelector('body');
	var $scope = angular.element(appElement).scope();
	$scope.logs.splice(0, 0, log);
	$scope.$apply();
}
var addIPPrinterLog = function (log) {
	var appElement = document.querySelector('body');
	var $scope = angular.element(appElement).scope();
	$scope.ipPrinterLogs.splice(0, 0, log);
	$scope.$apply();
}
var refreshIPPrinterStatuses = function (statuses) {
	var appElement = document.querySelector('body');
	var $scope = angular.element(appElement).scope();
	$scope.ipPrinterStatuses = [];
	for (var i in statuses) {
		$scope.ipPrinterStatuses.push(statuses[i]);
	}

	$scope.$apply();
}

//window.onerror = function (msg, url, line) {

//	alert("真不幸，又出错了\n"

//	+ "\n错误信息：" + msg

//	+ "\n所在文件：" + url

//	+ "\n错误行号：" + line);

//}