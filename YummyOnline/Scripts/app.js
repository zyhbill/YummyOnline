var app = angular.module('adminApp', ['ngRoute', 'ui.bootstrap']);

app.config(function ($httpProvider) {
	$httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
});

toastr.options = {
	"progressBar": true,
}