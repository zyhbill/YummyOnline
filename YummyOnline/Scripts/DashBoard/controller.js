app.controller('DashBoardCtrl', [
	'$scope',
	'$http',
	'$filter',
	'layout',
	function ($scope, $http, $filter, $layout) {
		$layout.Set('仪表盘', '');

		$scope.refreshUserLine = function () {
			$scope.userCount = {
				categories: [],
				customerDailyCount: [],
				nemoDailyCount: []
			};
			$http.post('/DashBoard/GetUserDailyCount').then(function (response) {
				var userCount = $scope.userCount;
				var data = response.data;
				for (var i in data.CustomerDailyCount) {
					var date = new Date(data.CustomerDailyCount[i].DateTime);
					userCount.categories.push($filter('date')(date, 'MM-dd'));
					userCount.customerDailyCount.push(data.CustomerDailyCount[i].Count);
				}
				for (var i in data.NemoDailyCount) {
					userCount.nemoDailyCount.push(data.NemoDailyCount[i].Count)
				}
				//var temp = 0;
				//for (var i = userCount.customerDailyCount.length - 1; i >= 0; i--) {
				//	var t = userCount.customerDailyCount[i];
				//	userCount.customerDailyCount[i] = data.CustomerCount - temp;
				//	temp += t;
				//}
				//temp = 0;
				//for (var i = userCount.nemoDailyCount.length - 1; i >= 0; i--) {
				//	var t = userCount.nemoDailyCount[i];
				//	userCount.nemoDailyCount[i] = data.NemoCount - temp;
				//	temp += t;
				//}
			});
		}

		$scope.refreshDineLine = function () {
			$scope.dineCount = {
				categories: [],
				series: []
			}
			$http.post('/DashBoard/GetDineDailyCount').then(function (response) {
				var dineCount = $scope.dineCount;
				var data = response.data;
				for (var i in data) {
					dineCount.series.push({
						name: data[i].HotelName,
						data: []
					});
					for (var j in data[i].DailyCount) {
						var date = new Date(data[i].DailyCount[j].DateTime);
						dineCount.categories.push($filter('date')(date, 'MM-dd'));
						dineCount.series[i].data.push(data[i].DailyCount[j].Count);
					}
				}
			});
		}
		$scope.refreshUserLine();
		$scope.refreshDineLine();

	}
])