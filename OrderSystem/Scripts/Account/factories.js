app.factory('SendSms', [
	'$rootScope',
	'$http',
	'$interval',
	function ($rootScope, $http, $interval) {
		var SendSms = function ($scope, url, phoneNumber) {
			$scope.sendSMSBtnText = "正在发送";
			$scope.canSendSMS = false;

			$http.post(url, {
				PhoneNumber: phoneNumber
			}).then(function (response) {
				var data = response.data;
				if (data.Succeeded) {
					$scope.canTypeSMS = true;

					var interval = 60;
					var timer = $interval(function () {
						$scope.sendSMSBtnText = interval + '秒后重发';
						interval--;
						if (interval == 0) {
							$scope.canSendSMS = true;
							$scope.sendSMSBtnText = '重发';
							interval = 60;
							$interval.cancel(timer);
						}
					}, 1000);
				} else {
					$scope.sendSMSBtnText = '重发';
					$scope.canSendSMS = true;
					toastr.error(data.ErrorMessage);
				}
			})
		}
		return SendSms;
	}
]);