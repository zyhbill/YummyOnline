# DesCryptography
Des加密解密包

## Install
- 进入 `工具` - `NuGet 包管理器` - `程序包管理器设置` - `程序包源`
- 新增程序包源 `http://nuget.yummyonline.net/nuget` 名称 `YummyOnline`
- 进入程序包管理界面, 程序包源选择 `YummyOnline`, 即可搜索到 `DesCryptography`
- 选择要安装的项目安装即可

## Specification

### `class Cryptography.DesCryptography`
Des加密解密类
#### `static string DesEncrypt(string strText)`
加密
参数(明文)

#### `static string DesDecrypt(string strText)`
解密
参数(密文)

# WeixinPay
微信支付流程

#### OrderySystem

1.1 读取数据库中微信支付的支付地址、支付完成异步通知地址、支付完成跳转地址

1.2 构造需要跳转的支付地址

##### URL
`RedirectUrl`
##### Params
```
"HotelId" 饭店号

"DineId" 点单号

"Price" 支付价格（经过DES加密）

"NotifyUrl" 支付完成异步通知地址

"CompleteUrl" 支付完成跳转地址
```

##### Eg
`http://www.test.com/?HotelId=1&DineId=16040300000001&Price=AUuAdrGRMGI=&NotifyUrl=/Payment/OnlineNotify&CompleteUrl=/Payment/Complete`

1.3 跳转至支付页面

#### 微信支付

2.1 处理

2.2 支付完成，构造异步通知地址

##### URL
`NotifyUrl`
##### TO DO
首先以json形式构造参数
```json
{
	"HotelId": <int>,
	"DineId": <string>,
	"RecordId": <string>,
}
```
将构造好的json字符串利用DES加密作为异步通知的参数传递

##### Params
```
"encryptedInfo" 加密之后的信息
```

##### Eg
POST
`http://ordersystem.yummyonline.net/Payment/OnlineNotify

Data:
```json
{
	"EncryptedInfo": <string>
}
```

2.3 支付完成，跳转支付完成页面
##### URL
`CompleteUrl`
##### Params
```
"Succeeded" 是否成功支付，可以省略此参数，默认为true
"DineId" 订单号
```
