### 服务员登录
#### POST
#### URL
	/Waiter/Account/Signin
#### Parameters
```json
{
	"SigninName": <string>,
	"Password": <string>
}
```
#### Results
```json
{
	"Succeeded": <bool>,
	"ErrorMessage": <string>
}
```

---

### 服务员登出
#### POST ***AUTHORIZED***
#### URL
	/Waiter/Account/Signout
#### Parameters
#### Results
```json
{
	"Succeeded": true
}
```

---

### 获取所有数据
#### POST ***AUTHORIZED***
#### URL
	/Waiter/Order/GetMenuInfos
#### Parameters
#### Results
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

---

### 获取饭店基本信息
#### POST ***AUTHORIZED***
#### URL
	/Waiter/Order/GetHotelInfos
#### Parameters
#### Results
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
	]
}

```

---

### 获取会员信息
#### POST ***AUTHORIZED***
#### URL
	/Waiter/Account/VerifyCustomer
#### Parameters
```json
{
	"PhoneNumber": <string>,
	"Password": <string>
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

---

### 验证是否为会员
#### POST ***AUTHORIZED***
#### URL
	/Waiter/Account/IsCustomer
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

---

### 提交点单
#### POST ***AUTHORIZED***
#### URL
	/Payment/WaiterPay
#### Parameters
```json
{
	"Cart": {
		"HeadCount": <int>,
		"Price": <float>,
		"PriceInPoints": <float>,
		"Invoice": <string>,
		"DeskId": <string>,
		"OrderedMenus":[{
			"Id": <string>,
			"Ordered": <int>,
			"Remarks": [<int>, ...]
		}, ...]
	},
	"CartAddition": {
		"UserId": <string>,
		"Discount": <float>,
		"DiscountName": <string>
	}
}
```
#### Results
```json
{
	"Succeeded": <bool>,
	"ErrorMessage": <string>,
	"Data": <string: DineId>
}
```

---

### 服务员支付完成，记录支付详情
#### POST ***AUTHORIZED***
#### URL
	/Payment/WaiterPayCompleted
#### Parameters
```json
{
	"PaidDetails": {
		"DineId": <string>,
		"PaidDetails": [
			{
				"PayKindId": <int>,
				"Price": <double>,
				"RecordId": <string>
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
	"Data": <string: DineId>
}
```

---

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
				"PayKindId": <int>,
				"Price": <double>,
				"RecordId": <string>
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
	"Data": <string: DineId>
}
```

---

### 打印完成通知
#### POST ***AUTHORIZED***
#### URL
	/Payment/PrintCompleted
#### Parameters
```json
{
	"HotelId": <int>,
	"DineId": <string>
}
```
#### Results

---

### 获取当前的点单
#### POST ***AUTHORIZED***
#### URL
	/Waiter/Order/GetCurrentDines
#### Parameters
```json
{
	"DeskId": <string>
}
```
#### Results
```json
[{
	<详见'根据订单号获取订单'>
}, ...]
```

---

### 根据订单号获取点单
#### POST ***AUTHORIZED***
#### URL
	/Waiter/Order/GetDineById
#### Parameters
```json
{
	"DineId": <string>
}
```
#### Results
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

---
