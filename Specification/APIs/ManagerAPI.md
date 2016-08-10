### 收银台提交点单
#### POST
#### URL
	ordersystem.yummyonline.net/Payment/ManagerPay
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
			"SetMealClasses": [{
				"Id": <int 套餐分类编号>,
				"OrderedMenus": [{
					"Id": <string 菜品编号>,
					"Ordered": <int 数量>
				}, ...]
			}, ...]
		}, ...]
	},
	"CartAddition": {
		"Token": <string 身份验证信息>,
		"HotelId": <int 饭店号>,
		"WaiterId": <string 服务员号>,
		"UserId": <string 会员号>,
		"Discount": <float 自定义折扣率>,
		"DiscountName": <string 自定折扣名称>
	}
}
```
