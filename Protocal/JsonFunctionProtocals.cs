using System;
using System.Collections.Generic;

namespace Protocal {
	public class JsonError {
		public JsonError() { }
		public JsonError(string errorMessage) {
			ErrorMessage = errorMessage;
		}
		public JsonError(string errorMessage, string errorPosition) {
			ErrorMessage = errorMessage;
			ErrorPosition = errorPosition;
		}
		public bool Succeeded { get; set; } = false;
		public string ErrorMessage { get; set; }
		public string ErrorPosition { get; set; }
	}
	public class JsonSuccess {
		public JsonSuccess() : this("") { }
		public JsonSuccess(string redirectUrl) : this(redirectUrl, null) { }
		public JsonSuccess(object data) : this("", data) { }
		public JsonSuccess(string redirectUrl, object data) {
			RedirectUrl = redirectUrl;
			Data = data;
		}
		public bool Succeeded { get; set; } = true;
		public string RedirectUrl { get; set; }
		public object Data { get; set; }
	}

	public class FunctionResult {
		public FunctionResult(bool succeeded = true) {
			Succeeded = succeeded;
		}
		public FunctionResult(bool succeeded, string msg) {
			Succeeded = succeeded;
			Message = msg;
		}
		public FunctionResult(bool succeeded, object data) {
			Succeeded = succeeded;
			Data = data;
		}
		public FunctionResult(Exception e) {
			Succeeded = false;
			Message = e.Message;
		}
		public bool Succeeded { get; set; }
		public string Message { get; set; }
		public object Data { get; set; }
	}
}