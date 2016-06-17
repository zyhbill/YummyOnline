app.directive('sideBarMenu', [
	'layout',
	function ($layout) {
		return {
			replace: true,
			template: function (elem, attr) {
				attr.title = attr.title == null ? '' : attr.title;
				attr.subtitle = attr.subtitle == null ? '' : attr.subtitle;
				return '<li ng-class="{active:layout.Title == \'' + attr.title + '\' && layout.Subtitle == \'' + attr.subtitle + '\'}">' +
					'<a href="' + attr.href + '">' +
						'<i class="fa ' + attr.fa + '"></i><span>' + attr.title + '</span>' +
					'</a>' +
				'</li>';
			}
		};
	}]
);

app.directive('confirmClick', [
	function () {
		return {
			scope: {
				confirmClick: '&',
				confirmText: '@',
			},
			transclude: true,
			template: '<span ng-show="confirming">{{confirmText}}</span> <span ng-transclude ng-show="!confirming"></span>',
			priority: -1,
			link: function (scope, elem, attrs) {
				if (attrs.confirmText == null) {
					attrs.confirmText = '确认';
				}

				elem.click(function (e) {
					elem.width(elem.width());
					e.stopImmediatePropagation();
					if (scope.confirming) {
						scope.confirmClick();
					}
					scope.confirming = true;
					elem.addClass(attrs.confirmClass)
					scope.$apply();
				});
				elem.on('mouseleave', function () {
					scope.confirming = false;
					elem.removeClass(attrs.confirmClass)
					scope.$apply();
				});
			}
		}
	}
]);

app.directive('switch', [
	function () {
		return {
			scope: {
				ngModel: '=',
				ngClick: '&'
			},
			link: function (scope, elem, attrs) {
				$(elem).bootstrapSwitch({
					state: scope.ngModel
				}).bootstrapSwitch('onSwitchChange', function (e, s) {
					scope.ngModel = s;
					scope.$apply();
					scope.ngClick();
				});
			}
		}
	}
]).directive('convertToNumber', function () {
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
})