var app = angular.module('waiterApp', ['ngRoute', 'ngStorage', 'ui.bootstrap']);

app.config(function ($httpProvider) {
	$httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
});