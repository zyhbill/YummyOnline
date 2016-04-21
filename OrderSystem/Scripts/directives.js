app.directive('ngStaticHeight', function () {
	return function ($scope, $elem, attr) {

		set();
		$(window).on('resize', function () {
			set();
		});

		function set() {
			$elem.css({
				'max-height': $(window).height() - attr.ngStaticHeight + 'px',
				'overflow-y': 'auto',
			});
		}
	};
}).directive('routeHref', ['$location', function ($location) {
	return function ($scope, $elem, attr) {
		$elem.click(function () {
			$location.path(attr.routeHref);
			$scope.$apply();
		});
	};
}]).directive('convertToNumber', function () {
	return {
		require: 'ngModel',
		link: function (scope, element, attrs, ngModel) {
			ngModel.$parsers.push(function (val) {
				return parseInt(val, 10);
			});
			ngModel.$formatters.push(function (val) {
				return '' + val;
			});
		}
	};
}).directive('backButton', function () {
	return {
		restrict: 'A',

		link: function (scope, element, attrs) {
			element.bind('click', goBack);

			function goBack() {
				history.back();
				scope.$apply();
			}
		}
	}
}).directive('stopEvent', function () {
	return {
		restrict: 'A',
		link: function (scope, element, attr) {
			element.bind('click', function (e) {
				e.stopPropagation();
			});
		}
	};
});