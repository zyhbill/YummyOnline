### 服务员API域名
waiter.yummyonline.net


### 服务员登录
#### POST
#### URL
	/Account/Signin
#### Parameters
```json
{
	"SigninName": <string 用户名>,
	"Password": <string 密码>
}
```
#### Results
```json
{
	"Succeeded": <bool 是否成功(下同)>,
	"ErrorMessage": <string 错误信息(下同)>
}
```
成功登录后，服务器会发送Cookie信息，以下所有带 ***AUTHORIZED*** 的API都需要验证该Cookie信息

* * *

### 服务员登出
#### POST ***AUTHORIZED***
#### URL
	/Account/Signout
#### Parameters
#### Results
```json
{
	"Succeeded": true
}
```

* * *

### 获取所有数据
#### POST ***AUTHORIZED***
#### URL
	/Order/GetMenuInfos
#### Parameters
#### Results
*所有参数说明详见[数据库结构文档](http://system.yummyonline.net/Log/GetFile?dir=Database&name=YummyOnlineDataBase.html)*
```json
{
	"MenuClasses": [{
		"Id": <string>,
		"Name": <string>
	}, ...],
	"Menus": [{
		"Id": <string>,
		"Code": <string>,
		"Name": <string>,
		"NameAbbr": <string>,
		"PicturePath": <string>,
		"IsFixed": <bool>,
		"SupplyDate": <int>,
		"Unit": <string>,
		"MinOrderCount": <int>,
		"Ordered": <int>,
		"Remarks": [{
			"Id": <int>,
			"Name": <string>,
			"Price": <float>
		}, ...],
		"MenuClasses": [<string>, ...],
		"MenuPrice": {
			"ExcludePayDiscount": <bool>,
			"Price": <float>,
			"Discount": <float>,
			"Points": <int>
		}
	}, ...],
	"MenuOnSales": [{
		"Id": <string>,
		"Price": <float>
	}, ...],
	"MenuSetMeals": [{
		"MenuSetId": <string>,
		"Count": <int>,
		"Menu": {
			"Id": <string>,
			"Name": <string>,
			"Price": <float>,
			"Discount": <float>,
			"Ordered": <int>
		}
	}, ...],
	"PayKind": {
		"Id": <int>,
		"Name": <string>,
		"Type": <int>,
		"Description": <string>,
		"Discount": <float>,
	},
	"DiscountMethods": {
		"TimeDiscounts": [{
			"From": <string>,
			"To": <string>,
			"Week": <int>,
			"Discount": <float>,
			"Name": <string>
		}, ...],
		"VipDiscounts": [{
			"Id": <int>,
			"Discount": <float>,
			"Name": <string>
		}, ...]
	},
	"Hotel": {
		"Id": <int>,
		"CssThemePath": <string>,
		"NeedCodeImg": <bool>,
		"PointsRatio": <int>,
		"Name": <string>,
		"Address": <string>,
		"Tel": <string>,
		"OpenTime": <string>,
		"CloseTime": <string>
	},
	"Desks": [{
		"Area": {
			"Id": <string>,
			"Name": <string>
		},
		"Id": <string>,
		"QrCode": <string>,
		"Name": <string>,
		"Description": <string>,
		"Status": <int>,
		"Order": <int>,
		"MinPrice": <float>,
	}, ...]
}
```

* * *

### 获取饭店基本信息
#### POST ***AUTHORIZED***
#### URL
	/Order/GetHotelInfos
#### Parameters
#### Results
*所有参数说明详见[数据库结构文档](http://system.yummyonline.net/Log/GetFile?dir=Database&name=YummyOnlineDataBase.html)*
```json
{
	"Areas": [
		{
			"Id": <string>,
			"Name": <string>,
			"Description": <string>,
			"DepartmentReciptId": <int>,
			"DepartmentServeId": <int>
		}, ...
	],
	"PayKinds": [
		{
			"Id": <int>,
			"Name": <string>,
			"Type": <int>,
			"Description": <string>,
			"Discount": <float>,
		}, ...
	],
	"Remarks": [
		{
			"Id": <int>,
			"Name": <string>,
			"Price": <float>
		}, ...
	],
	"Staffs": [
		{
			"Id": <string>,
			"Name": <string>,
			"Schemas": [<int>, ...]
		}, ...
	]
}

```

* * *

### 获取会员信息
#### POST ***AUTHORIZED***
#### URL
	/Account/VerifyCustomer
#### Parameters
```json
{
	"PhoneNumber": <string 会员手机号>,
	"Password": <string 密码>
}
```
#### Results
```json
{
	"Succeeded": <bool>,
	"Data": {
		"Id": <string 会员编号>,
	    "Points": <int 积分>,
	    "VipLevelId": <int 等级>,
	}
}
```

* * *

### 验证是否为会员
#### POST ***AUTHORIZED***
#### URL
	/Account/IsCustomer
#### Parameters
```json
{
	"UserId": <string>
}
```
#### Results
```json
{
	"Succeeded": <bool>,
	"Data": {
		"Id": <string>,
	    "Points": <int>,
	    "VipLevelId": <int>,
	}
}
```

* * *

### 提交点单
#### POST ***AUTHORIZED***
#### URL
	/Payment/WaiterPay
#### Parameters
```json
{
	"Cart": {
		"HeadCount": <int 人数>,
		"Price": <float 需要支付价格>,
		"PriceInPoints": <float 积分支付价格>,
		"Invoice": <string 发票抬头>,
		"DeskId": <string 桌号>,
		"OrderedMenus":[{
			"Id": <string 菜品编号>,
			"Ordered": <int 数量>,
			"Remarks": [<int 备注编号>, ...]
		}, ...]
	},
	"CartAddition": {
		"UserId": <string 会员编号>,
		"Discount": <float 自定义折扣率>,
		"DiscountName": <string 自定义折扣名称>
	}
}
```
#### Results
```json
{
	"Succeeded": <bool>,
	"ErrorMessage": <string>,
	"Data": <string 新产生的订单编号>
}
```

* * *

### 服务员支付完成，记录支付详情
#### POST ***AUTHORIZED***
#### URL
	/Payment/WaiterPayCompleted
#### Parameters
```json
{
	"PaidDetails": {
		"DineId": <string 订单编号>,
		"PaidDetails": [
			{
				"PayKindId": <int 支付种类编号>,
				"Price": <double 支付价格>,
				"RecordId": <string 额外信息如银行卡号>
			}, ...
		]
	}
}
```
#### Results
```json
{
	"Succeeded": <bool>,
	"ErrorMessage": <string>,
	"Data": <string 新产生的订单号>
}
```

* * *

### 服务员支付并且带所有支付详情
#### POST ***AUTHORIZED***
#### URL
	/Payment/WaiterPayWithPaidDetails
#### Parameters
```json
{
	"Cart": {
		<详见'提交点单'>
	},
	"CartAddition": {
		<详见'提交点单'>
	},
	"PaidDetails": {
		"PaidDetails": [
			{
				"PayKindId": <int 支付种类编号>,
				"Price": <double 支付价格>,
				"RecordId": <string 额外信息如银行卡号>
			}, ...
		]
	}
}
```
#### Results
```json
{
	"Succeeded": <bool>,
	"ErrorMessage": <string>,
	"Data": <string 新产生的订单号>
}
```

* * *

### 打印完成通知
#### POST ***AUTHORIZED***
#### URL
	/Payment/PrintCompleted
#### Parameters
```json
{
	"HotelId": <int 饭店号>,
	"DineId": <string 订单号>
}
```
#### Results

* * *

### 获取当前的点单
#### POST ***AUTHORIZED***
#### URL
	/Order/GetCurrentDines
#### Parameters
```json
{
	"DeskId": <string 桌号>
}
```
#### Results
```json
[{
	<详见'根据订单号获取订单'>
}, ...]
```

* * *

### 根据订单号获取点单
#### POST ***AUTHORIZED***
#### URL
	/Order/GetDineById
#### Parameters
```json
{
	"DineId": <string 订单号>
}
```
#### Results
*所有参数说明详见[数据库结构文档](http://system.yummyonline.net/Log/GetFile?dir=Database&name=YummyOnlineDataBase.html)*
```json
{
	"Id": <string>,
	"Status": <int>,
	"Type": <int>,
	"HeadCount": <int>,
	"Price": <float>,
	"OriPrice": <float>,
	"Discount": <float>,
	"DiscountName": <string>,
	"BeginTime": <string>,
	"IsOnline": <bool>,
	"IsPaid": <bool>,
	"ClerkId": <string>,
	"WaiterId": <string>,
	"UserId": <string>,
	"Desk": {
		"Id": <string>,
		"QrCode": <string>,
		"Name": <string>,
		"Description": <string>,
	},
	"DineMenus": [{
		"Status": <int>,
		"Count": <int>,
		"OriPrice": <float>,
		"Price": <float>,
		"RemarkPrice": <float>,
		"Remarks":[{
            "Id": <int>,
            "Name": <string>
		}, ...],
		"Menu": {
			"Id": <string>,
			"Code": <string>,
			"Name": <string>,
			"NameAbbr": <string>,
			"PicturePath": <string>,
			"Unit": <string>
		},
	}, ...],
	"DinePaidDetails": [{
		"Price": <float>,
        "RecordId": <string>,
        "PayKind": {
          "Id": <int>,
          "Name": <string>,
          "Type": <int>
        }
	}, ...]
}
```

* * *
