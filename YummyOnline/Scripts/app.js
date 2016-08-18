var app = angular.module('adminApp', ['ngRoute', 'ui.bootstrap']);

app.config(function ($httpProvider) {
	$httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
});
app.filter(
    'to_trusted', ['$sce', function ($sce) {
    	return function (text) {
    		return $sce.trustAsHtml(text);
    	}
    }]
)

toastr.options = {
	"progressBar": true,
}