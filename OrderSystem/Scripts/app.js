var app = angular.module('orderApp', ['ngRoute', 'ngStorage', 'ui.bootstrap']);

window.onerror = function (errorMessage, scriptURI, lineNumber, columnNumber, errorObj) {
	if (errorMessage.indexOf('WeixinJSBridge') >= 0) {
		return;
	}
	toastr.error("错误信息：" + errorMessage
				+ "\n出错文件：" + scriptURI
				+ "\n出错行号：" + lineNumber
				+ "\n出错列号：" + columnNumber
				+ "\n错误详情：" + errorObj);
}
