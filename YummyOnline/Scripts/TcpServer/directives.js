app.directive('highchartsCpu', [
	function () {
		return {
			link: function (scope, elem, attr) {
				scope.$watch('tcpServerInfo', function (tcpServerInfo) {

				}, true);

				elem.highcharts({
					chart: {
						type: 'gauge',
					},

					title: {
						text: null
					},

					pane: {
						startAngle: -150,
						endAngle: 150,
						background: {
							backgroundColor: '#EEE',
						}
					},

					yAxis: {
						min: 0,
						max: 100,

						minorTickInterval: 5,

						tickPixelInterval: 50,
						
						title: {
							text: '%'
						},
						plotBands: [{
							from: 0,
							to: 20,
							color: '#55BF3B' // green
						}, {
							from: 20,
							to: 60,
							color: '#DDDF0D' // yellow
						}, {
							from: 60,
							to: 100,
							color: '#DF5353' // red
						}]
					},

					series: [{
						name: 'CPU',
						data: [2.33435],
						tooltip: {
							valueSuffix: ' %'
						}
					}]

				})
			}
		}
	}]
)