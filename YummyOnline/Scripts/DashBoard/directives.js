app.directive('highchartsUserLine', [
	function () {
		return {
			link: function (scope, elem, attr) {
				scope.$watch('userCount', function (userCount) {
					elem.highcharts({
						title: null,
						chart: {
							type: 'spline',
						},
						xAxis: {
							categories: userCount.categories,
							tickInterval: 7,
						},
						yAxis: {
							title: null
						},
						
						plotOptions: {
							series: {
								marker: {
									lineWidth: 1
								}
							},
							spline: {
								marker: {
									enabled: true
								}
							}
						},
						series: [{
							name: '普通用户',
							data: userCount.customerDailyCount
						}, {
							name: '匿名用户',
							data: userCount.nemoDailyCount
						}]
					});
				}, true);
			}
		}
	}]
).directive('highchartsDineLine', [
	function () {
		return {
			link: function (scope, elem, attr) {
				scope.$watch('dineCount', function (dineCount) {
					elem.highcharts({
						title: null,
						chart: {
							type: 'spline',
						},
						xAxis: {
							categories: dineCount.categories,
							tickInterval: 7,
						},
						yAxis: {
							title: null
						},

						plotOptions: {
							series: {
								marker: {
									lineWidth: 1
								}
							},
							spline: {
								marker: {
									enabled: true
								}
							}
						},
						series: dineCount.series
					});
				}, true);
			}
		}
	}]
)