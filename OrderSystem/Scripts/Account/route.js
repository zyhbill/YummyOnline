app.config(function ($routeProvider) {
	$routeProvider.when('/', {
		templateUrl: '/Account/_ViewUser',
		controller: 'UserCtrl'
	}).when('/User', {
		templateUrl: '/Account/_ViewUser',
		controller: 'UserCtrl'
	}).when('/Signup', {
		templateUrl: '/Account/_ViewSignup',
		controller: 'SignupCtrl'
	}).when('/Signin', {
		templateUrl: '/Account/_ViewSignin',
		controller: 'SigninCtrl'
	}).when('/Forget', {
		templateUrl: '/Account/_ViewForget',
		controller: 'ForgetCtrl'
	})
});

app.controller('LoadingCtrl', function ($rootScope) {
	$rootScope.$on('$routeChangeStart', function () {
		$rootScope.isLoading = true;
	});
	$rootScope.$on('$routeChangeSuccess', function () {
		$rootScope.isLoading = false;
	});
});