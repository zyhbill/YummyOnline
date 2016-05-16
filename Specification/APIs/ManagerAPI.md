### 收银台提交点单
#### POST
#### URL
	/Payment/ManagerPay
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
		"Token": <string>,
		"HotelId": <int>,
		"WaiterId": <string>,
		"UserId": <string>,
		"Discount": <float>,
		"DiscountName": <string>
	}
}
```
