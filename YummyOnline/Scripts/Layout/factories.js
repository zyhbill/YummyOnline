app.factory('layout', [
	'$rootScope',
	'$http',
	'$window',
	function ($rootScope, $http, $window) {
		var layout = {
			Title: '',
			Subtitle: '',

			Set: function (title, subtitle) {
				this.Title = title;
				this.Subtitle = subtitle;
				document.title = title;
			},

			Signout: function () {
				$http.post('/Account/Signout').then(function (response) {
					if (response.data.Succeeded) {
						$window.location.href = '/Account';
					} else {
						toastr.error(response.data.ErrorMessage);
					}
				});
			}
		}
		$rootScope.layout = layout;
		return layout;
	}
]);