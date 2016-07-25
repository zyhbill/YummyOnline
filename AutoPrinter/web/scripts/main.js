var app = angular.module('app', []);

app.controller('mainCtrl', [
	'$scope',
	function ($scope) {
		$scope.logs = [];
		$scope.ipPrinterLogs = [];
		$scope.ipPrinterStatuses = [];

		$scope.localIP = '192.168.0.253';
		$scope.recipt = true;
		$scope.serveOrder = true,
		$scope.kitchenOrder = false,

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
			window.external.app.connectPrinters();
		}
	}
])

var internal = {
	addLog: function (dateTime, message) {
		var appElement = document.querySelector('body');
		var $scope = angular.element(appElement).scope();
		$scope.logs.splice(0, 0, {
			DateTime: dateTime,
			Message: message
		});
		$scope.$apply();
	},
	addIPPrinterLog: function (dateTime, ip, message, hashCode) {
		var appElement = document.querySelector('body');
		var $scope = angular.element(appElement).scope();
		$scope.ipPrinterLogs.splice(0, 0, {
			DateTime: dateTime,
			IP: ip,
			Message: message,
			HashCode: hashCode
		});
		$scope.$apply();
	},
	refreshIPPrinterStatuses: function (statuses) {
		var appElement = document.querySelector('body');
		var $scope = angular.element(appElement).scope();
		$scope.ipPrinterStatuses = [];
		for (var i in statuses) {
			$scope.ipPrinterStatuses.push(statuses[i]);
		}

		$scope.$apply();
	}
}