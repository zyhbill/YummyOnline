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

app.directive('confirmBtn', [
	function () {
		return {
			link: function (scope, element, attrs) {

			}
		}
	}
])