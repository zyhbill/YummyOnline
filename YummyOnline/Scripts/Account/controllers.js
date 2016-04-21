app.controller('SigninCtrl', [
	'$scope',
	'$http',
	'$window',
	function ($scope, $http, $window) {
		$scope.signinFormData = {
			UserName: '',
			Password: '',
			RememberMe: false
		};
		$scope.isSigning = false;
		$scope.signin = function () {
			if ($scope.isSigning) return;
			$scope.isSigning = true;

			$http.post('/Account/Signin', $scope.signinFormData).then(function (response) {
				if (response.data.Succeeded) {
					$window.location.href = '/';
				} else {
					toastr.error(response.data.ErrorMessage);
				}
				$scope.isSigning = false;
			}, function (response) {
				toastr.error(response.status + ' ' + response.statusText);
				$scope.isSigning = false;
			});

			return false;
		}
	}
]);