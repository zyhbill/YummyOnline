var app = angular.module('app', []);

app.controller('SigninCtrl', [
	'$scope',
	'$http',
	'$window',
	function ($scope, $http, $window) {
		$scope.signinFormData = {
			SigninName: '',
			Password: '',
		}

		$scope.signin = function () {
			var result = window.external.app.signin($scope.signinFormData.SigninName, $scope.signinFormData.Password);
			result = JSON.parse(result);
			if (result.Succeeded) {
				$window.location.href = 'main.html';
			} else {
				toastr.error(result.ErrorMessage);
			}
		}
	}
]);