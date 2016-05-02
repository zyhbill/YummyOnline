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
						if (i == 0) {
							var date = new Date(data[i].DailyCount[j].DateTime);
							dineCount.categories.push($filter('date')(date, 'MM-dd'));
						}
						dineCount.series[i].data.push(data[i].DailyCount[j].Count);
					}
				}
			});
		}

		$scope.refreshDinePerHourLine = function () {
			$scope.dinePerHourCount = {
				categories: [],
				series: []
			}
			$http.post('/DashBoard/GetDinePerHourCount', {
				'DateTime': $scope.currDateTime
			}).then(function (response) {
				
				var dinePerHourCount = $scope.dinePerHourCount;
				var data = response.data;
				for (var i in data) {
					dinePerHourCount.series.push({
						name: data[i].HotelName,
						data: []
					});
					for (var j in data[i].Counts) {
						if (i == 0) {
							dinePerHourCount.categories.push(j);
						}
						dinePerHourCount.series[i].data.push(data[i].Counts[j]);
					}
				}
			});
		}

		$scope.$watch('currDateTime', function () {
			$scope.refreshDinePerHourLine();
		});
		$scope.refreshUserLine();
		$scope.refreshDineLine();
	}
])