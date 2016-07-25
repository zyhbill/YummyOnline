var app = angular.module('app', []);

app.controller('SigninCtrl', [
	'$scope',
	'$http',
	'$window',
	'$q',
	function ($scope, $http, $window, $q) {
		$scope.signinFormData = {
			SigninName: '',
			Password: '',
		}

		$scope.signin = function () {
			var deferred = $q.defer();

			setTimeout(function () {
				var result = window.external.app.signin($scope.signinFormData.SigninName, $scope.signinFormData.Password);
				deferred.resolve(result);
			}, 500);

			$scope.isSigning = true;
			var promise = deferred.promise;
			promise.then(function (result) {
				result = JSON.parse(result);
				if (result.Succeeded) {
					$window.location.href = 'main.html';
				} else {
					toastr.error(result.ErrorMessage);
				}
				$scope.isSigning = false;
			});

		}
	}
]);