app.service('websocket', [
	'$q',
	'$rootScope',
	function ($q, $rootScope) {
		var ws,
			local;
		function init(location) {
			local = location;
			ws = new WebSocket(location);

			ws.onopen = function () {
				toastr.success("成功连接到了服务器");
			};

			ws.onclose = function () {
				toastr.error("连接被关闭，5秒后重试");
				setTimeout(function () {
					init(local);
				}, 5000);
			};

			ws.onmessage = function (message) {
				var data = JSON.parse(message.data)
				$rootScope.$broadcast('WebSocketMessageReceived', data);
			};
		}

		return {
			Initialize: function (location) {
				init(location)
			}
		};
	}]
);