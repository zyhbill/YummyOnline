angular.module('Services', [])
.service('WebSocketService', ['$q', '$rootScope', function ($q, $rootScope) {
    var Service = {};
    Service.Start = function (Url) {
        var callbacks = {};
        var currentCallbackId = 0;
        var ws = new ReconnectingWebSocket(Url);

        ws.onopen = function () {
            console.log("连接到了服务器!");
        };

        ws.onclose = function () {
            alert('连接被关闭.');
            $rootScope.Logout();
        };

        ws.onmessage = function (message) {
            listener(JSON.parse(message.data));
        };

        function sendRequest(request) {
            var defer = $q.defer();
            var callbackId = getCallbackId();
            callbacks[callbackId] = {
                time: new Date(),
                cb: defer
            };
            request.callbackId = callbackId;
            ws.send(JSON.stringify(request));
            return defer.promise;
        }

        function listener(data) {
            console.log(data);
            var messageObj = data;
            if (callbacks.hasOwnProperty(messageObj.callbackId)) {//主动请求
                $rootScope.$apply(callbacks[messageObj.callbackId].cb.resolve(messageObj));
                delete callbacks[messageObj.callbackId];
            } else {//服务端推送（广播给子scope）
                $rootScope.$broadcast(messageObj.change, messageObj);
            }
        }

        function getCallbackId() {
            currentCallbackId += 1;
            if (currentCallbackId > 10000) {
                currentCallbackId = 0;
            }
            return currentCallbackId;
        }
    }
    

    Service.sendMessage = function (message) {
        var request = {
            message: message
        }
        var promise = sendRequest(request);
        return promise;
    };
    return Service;
}]);