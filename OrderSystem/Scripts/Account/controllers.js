app.controller('UserCtrl', [
	'$rootScope',
	'$scope',
	'$http',
	'$location',
	function ($rootScope, $scope, $http, $location) {
		$rootScope.isLoading = true;
		$http.post('/Account/IsAuthenticated').then(function (response) {
			$rootScope.isLoading = false;
			var data = response.data;
			if (data.Succeeded) {
				$scope.user = data.Data;
			} else {
				$location.path('/Signin');
			}
		});
		$scope.signout = function () {
			$http.post('/Account/Signout').then(function (response) {
				var data = response.data;
				if (data.Succeeded) {
					$location.path('/Signin');
				}
			});
		}
	}
]);

app.controller('SignupCtrl', [
	'$scope',
	'$http',
	'$interval',
	'$window',

	'SendSms',
	function ($scope, $http, $interval, $window, $SendSms) {

		$scope.signupFormData = {
			PhoneNumber: '',
			Code: '',
			Password: '',
			PasswordAga: '',
		};
		$scope.canTypeSMS = false;
		$scope.canSendSMS = true;
		$scope.sendSMSBtnText = '发送验证码';


		$scope.sendSMS = function () {
			$SendSms($scope, '/Account/SendSMS', $scope.signupFormData.PhoneNumber);
		}

		$scope.signup = function () {
			if ($scope.signupFormData.PasswordAga != $scope.signupFormData.Password) {
				$scope.signupFormData.PasswordAga = '';
				toastr.error('两次密码不匹配');
				return;
			}
			$http.post('/Account/Signup', $scope.signupFormData).then(function (response) {
				if (response.data.Succeeded) {
					$window.location.href = '/Order';
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}
	}
]).controller('SigninCtrl', [
	'$scope',
	'$http',
	'$window',
	'$localStorage',
	function ($scope, $http, $window, $localStorage) {

		$scope.signinFormData = {
			PhoneNumber: '',
			CodeImg: '',
			Password: '',
		};
		if ($localStorage.phoneNumber != null) {
			$scope.signinFormData.PhoneNumber = $localStorage.phoneNumber;
		}
		$scope.codeImgSrc = '/Account/CodeImage'
		$scope.changeCodeImg = function () {
			$scope.codeImgSrc = '/Account/CodeImage?' + Math.random();
		}

		$scope.signin = function () {
			$http.post('/Account/Signin', $scope.signinFormData).then(function (response) {
				if (response.data.Succeeded) {

					$localStorage.phoneNumber = $scope.signinFormData.PhoneNumber;
					$window.location.href = '/Order';
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}
	}
]).controller('ForgetCtrl', [
	'$scope',
	'$http',
	'$location',

	'SendSms',
	function ($scope, $http, $location, $SendSms) {

		$scope.forgetFormData = {
			PhoneNumber: '',
			Code: '',
			Password: '',
			PasswordAga: ''
		};

		$scope.signupFormData = {};
		$scope.isSendSMS = false;
		$scope.canSendSMS = true;
		$scope.sendSMSBtnText = '发送验证码';

		$scope.sendSMS = function () {
			$SendSms($scope, '/Account/SendForgetSMS', $scope.forgetFormData.PhoneNumber);
		}

		$scope.forget = function () {
			if ($scope.forgetFormData.PasswordAga != $scope.forgetFormData.Password) {
				$scope.forgetFormData.PasswordAga = '';
				toastr.error('两次密码不匹配');
				return;
			}
			$http.post('/Account/Forget', $scope.forgetFormData).then(function (response) {
				if (response.data.Succeeded) {
					$location.path('/Singin');
				} else {
					toastr.error(response.data.ErrorMessage);
				}
			});
		}
	}
]);

