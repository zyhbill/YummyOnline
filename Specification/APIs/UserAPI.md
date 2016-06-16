### 用户通用API域名
ordersystem.yummyonline.net

### Updates
- v0.1.0 用户通用API脱离原有OrderSystem API, 全新API, Controller为GlobalAccount


### 发送用户注册短信
#### POST
#### URL
	/GlobalAccount/SendSMS
#### Parameters
```json
{
	"PhoneNumber": <string 手机号>,
}
```
#### Results
```json
{
	"Succeeded": <bool 是否成功(下同)>,
	"ErrorMessage": <string 错误信息(下同)>
}
```
该接口只能每隔50秒调用一次, 小于50秒会发送错误信息, 建议前端显示时每隔60秒才能重新发送短信

* * *

### 用户注册
#### POST
#### URL
	/GlobalAccount/Signup
#### Parameters
```json
{
	"PhoneNumber": <string 手机号>,
	"Code": <string 短信验证码>,
	"Password": <string 密码>,
	"PasswordAga": <string 重复密码>,
}
```
#### Results
```json
{
	"Succeeded": <bool>,
	"ErrorMessage": <string>
}
```

* * *

### 用户登录
#### POST
#### URL
	/GlobalAccount/Signin
#### Parameters
```json
{
	"PhoneNumber": <string 手机号>,
	"Password": <string 密码>,
	"RememberMe": <bool 是否记住>
}
```
#### Results
```json
{
	"Succeeded": <bool>,
	"ErrorMessage": <string>
}
```
* * *

### 用户登出
#### POST
#### URL
	/GlobalAccount/Signout
#### Parameters
#### Results
```json
{
	"Succeeded": true,
}
```

* * *

### 发送忘记密码短信
#### POST
#### URL
	/GlobalAccount/SendForgetSMS
#### Parameters
```json
{
	"PhoneNumber": <string 手机号>,
}
```
#### Results
```json
{
	"Succeeded": <bool>,
	"ErrorMessage": <string>
}
```
与发送用户注册短信相同, 该接口只能每隔50秒调用一次
* * *

### 发送忘记密码短信
#### POST
#### URL
	/GlobalAccount/Forget
#### Parameters
```json
{
	"PhoneNumber": <string 手机号>,
	"Code": <string 短信验证码>,
	"Password": <string 密码>,
	"PasswordAga": <string 重复密码>,
}
```
#### Results
```json
{
	"Succeeded": <bool>,
	"ErrorMessage": <string>
}
```

* * *

### 验证用户登录并获取用户所有信息
#### POST
#### URL
	/GlobalAccount/IsAuthenticated
#### Parameters
#### Results
```json
{
	"Succeeded": <bool>,
	"Data": {
		"Id": <string 用户编号>,
		"Email": <string>,
		"PhoneNumber": <string 手机号>,
		"UserName": <string 用户名>,
		"CustomerInfos": [{
			"Hotel": {
				"Id": <int 饭店编号>,
				"Name": <string 饭店名>
			},
			"Points": <string 该饭店的积分>,
			"VipLevel": {
				"Id": <int 会员等级编号>,
				"Name": <string 会员等级名>
			},
			"DinesCountId": <int 该饭店点过的点单数量>,
		}, ...]
	}
}
```
如`Succeeded`为false, `Data`为`null`
如会员没有去过某饭店, 则`CustomerInfos`中没有该饭店信息
如会员不是该饭店会员, 则`CustomerInfos`中该饭店的`VipLevel`为`null`
* * *