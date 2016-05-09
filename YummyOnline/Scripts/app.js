var app = angular.module('adminApp', ['ngRoute', 'ui.bootstrap']);

app.filter('trustHtml', function ($sce) {
	return function (input) {
		return $sce.trustAsHtml(input);
	}
});

toastr.options = {
	"progressBar": true,
}