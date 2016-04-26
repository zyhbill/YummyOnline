app.controller('SigninCtrl', [
	'$scope',
	'$http',
	'$window',
	function ($scope, $http, $window) {
		$scope.signinFormData = {
			SigninName: '',
			Password: '',
		}
		//if (!angular.isUndefined($localStorage.signinFormData)) {
		//	$scope.signinFormData = angular.copy($localStorage.signinFormData);
		//}

		$scope.signin = function () {
			$http.post('/Account/Signin', $scope.signinFormData).then(function (response) {
				if (response.data.Succeeded) {
					//$localStorage.signinFormData = $scope.signinFormData;
					$window.location.href = '/Order';
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}
	}
]);