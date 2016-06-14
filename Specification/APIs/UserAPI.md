### 用户通用API域名
ordersystem.yummyonline.net


### 发送用户注册短信
#### POST
#### URL
	/Account/SendSMS
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
	/Account/Signup
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

### 获取用户登录图片验证码
#### POST
#### URL
	/Account/CodeImage
#### Parameters
#### Results
`image/jpeg`图片文件
有些饭店可能不需要图片验证码
* * *

### 用户登录
#### POST
#### URL
	/Account/Signin
#### Parameters
```json
{
	"PhoneNumber": <string 手机号>,
	"Password": <string 密码>,
	"CodeImg": <string 图片验证码>,
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
不需要图片验证码的饭店不需要传递`CodeImg`参数
* * *

### 用户登出
#### POST
#### URL
	/Account/Signout
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
	/Account/SendForgetSMS
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
	/Account/Forget
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
	/Account/IsAuthenticated
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
		"Points": <string 该饭店的积分>,
		"VipLevelId": <string 该饭店的会员等级>,
		"DinesCountId": <int 该饭店点过的点单数量>,
	}
}
```
如`Succeeded`为false, `Data`为`null`
* * *