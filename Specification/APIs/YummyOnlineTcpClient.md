

[TOC]

# YummyOnlineTcpClient

YummyOnline项目中与Tcp服务器连接交换数据的客户端类库



## DLLs Intro
- `AsynchronousTcp.dll`: 异步Tcp的底层实现
- `Protocol.dll`: 所有协议
- `YummyOnlineTcpClient.dll`: 底层Tcp的客户端封装
- `HotelDAOModels.dll`: 饭店数据库模型
- `YummyOnlineDAOModels.dll`: 总数据库模型

## To Do
- 引用 `YummyOnlineTcpClient` 与 `Protocol` 两个命名空间
- 创建客户端对象

```csharp
TcpClient client = new TcpClient(
	IPAddress.Parse("127.0.0.1"),
    18000,
    new NewDineInformClientConnectProtocol("THIS IS A TEST GUID")
);

client.CallBackWhenMessageReceived = (t, p) => {
	if(t != TcpProtocolType.NewDineInform) {
		return;
	}
	NewDineInformProtocol protocal = (NewDineInformProtocol)p;
	Console.WriteLine($"{protocal.HotelId}, {protocal.DineId}, {protocal.IsPaid}");
};

client.CallBackWhenConnected = () => {
	Console.WriteLine("Connected");
};

client.CallBackWhenExceptionOccured = e => {
	Console.WriteLine(e);
};

client.Start();
```

## Specification

### *class TcpClient*


Tcp客户端的核心类
#### `Constructor`

```csharp
public TcpClient(
    IPAddress ip, // 服务器Ip地址
    int port, // 服务器端口号
    BaseTcpProtocol connectSender, // Tcp连接完成后发送的身份信息
)
```

`ip`与`port`需要从数据库中读取

`connectSender` 请设置为 `new NewDineInformClientConnectProtocol(string guid)`, 表明该客户端为需要立即接受新订单信息的客户端

#### `void Start()`

开始连接

#### `void Send(BaseTcpProtocol p)`

发送Tcp请求:

eg. :
```csharp
client.Send(new RequestPrintDineProtocol(hotelId, dineId, new List<PrintType>() {
	PrintType.Recipt, 
	PrintType.KitchenOrder, 
	PrintType.ServeOrder
}));
```
#### `int ReconnectInterval`
重新连接的等待时间(秒), 默认5秒

#### `Action CallBackWhenConnected`
Tcp连接成功回调函数，默认NULL

#### `Action<Exception> CallBackWhenExceptionOccured`
异常发生回调函数，默认NULL

#### `Action<string, object> CallBackWhenMessageReceived`
接收到新消息回调函数，默认NULL

---

### *class NewDineInformProtocol*
新订单通知协议

#### `int HotelId { get; set; }`
饭店Id

#### `string DineId { get; set; }`
新增订单号

#### `bool IsPaid { get; set; }`
订单是否支付

---

### *class RequestPrintDineProtocol*
打印请求协议

#### `int HotelId { get; set; }`
饭店Id

#### `string DineId { get; set; }`
订单号

#### `List<int> DineMenuIds { get; set; }`
菜品编号

#### `List<PrintType> PrintTypes { get; set; }`
请求的打印类型集合

---

### *enum PrintType*
请求打印类型

#### `Recipt = 0`
收银条

#### `KitchenOrder = 1`
厨房单

#### `ServeOrder = 2`
传菜单

------

### *class RequestPrintShiftsProtocol*

打印交接班请求协议

#### `List<int> Ids {get; set; }`

交接班号

#### `int HotelId { get; set; }`

饭店Id

#### `DateTime DateTime {get; set; }`

交接班时间

## Updates
- 2016-3-6: 优化`TcpClient`类，改进构造函数，增加`Start`启动函数
- 2016-2-22: 更改底层tcp协议
- 2016-2-19: 删除`PrintDineProtocol`, 分离到`OrderSystem`中, 由打印客户端接收到打印信息后http到`OrderSystem`获取打印的订单信息
- 2016-2-18: 增加接收到的数据不完整的异常处理, 不会导致程序奔溃
- 2016-2-16: 新增连接成功时的回调函数
- 2016-4-3: 修复可能导致远程连接断开后, 客户端的CPU使用率上升的bug, 修改底层TCP传输时的头字节
- 2016-4-19: `NewDineInformConnect`协议增加`Guid`
- 2016-5-22: 修复在服务器断开的极端情况下, 客户端主线程阻塞的bug
- 2016-6-20: 修改打印订单协议, 可以打印单个菜品
- 2016-6-22: 加入打印交接班协议
- 2016-6-23: 加入心跳包机制

# YummyOnlineTcpClient (Cross Platform)

## To Do
1. 连接Tcp服务器, Ip: `122.114.96.157`, Port: `18000`, 建立连接
2. 发送身份信息(详见`ConnectionProtocol`)
3. 每隔10秒左右发送接收心跳包信息(详见`ReceivingProtocol`, `SendingProtocol`)
4. 等待接收信息(详见`ReceivingProtocol`), 发送信息(详见`SendingProtocol`)

## Protocols
### ConnectionProtocol
```json
{
	"Type": "{053A168C-D4B8-409A-A058-7E2208B57CDA}",
	"Guid": <string>
}
```
>  *特别注意！ Guid请向管理员申请，同一时刻，只能有一个Guid对应的Socket连入*

### ReceivingProtocol
心跳包
```json
{
    "Type": "{F3E101EA-F55D-40DD-9747-BF0DB29C98AF}",
}
```
新订单通知
```json
{
	"Type": "{6309155D-B9D9-4417-B1BF-C985F2EA6630}",
	"HotelId": <int>,
	"DineId": <string>,
	"IsPaid": <bool>
}
```
> *特别注意！ 心跳包每隔10秒左右服务器会发送心跳包, 客户端接收到心跳包后立即返回相同的心跳包, 如果服务器在60秒之内没有接收到客户端发来的心跳包则视为客户端已断开连接并强制与客户端断开连接*
> *客户端应自行判断服务器在多少时间之内如果没有接收到服务器发来的心跳包, 则视为服务器已断开连接需要重连, 推荐60秒*

### SendingProtocol
心跳包
```json
{
    "Type": "{F3E101EA-F55D-40DD-9747-BF0DB29C98AF}",
}
```
请求打印
```json
{
	"Type": "{FCAC99D2-1807-4FD0-8B5C-71D00B91A927}",
	"HotelId": <int>,
	"DineId": <string>,
	"DineMenuIds": [<int>, ...],
	"PrintTypes": [<int>, ...]
}
```
请求打印交接班

```json
{
	"Type": "{4E6D44F1-9FD6-4DAD-BAE6-545577701149}",
	"HotelId": <int>,
	"Ids": [<int>, ...],
	"DateTime": <string>
}
```

新订单通知

```json
{
	"Type": "{6309155D-B9D9-4417-B1BF-C985F2EA6630}",
	"HotelId": <int>,
	"DineId": <string>,
	"IsPaid": <bool>
}
```

## Tcp Bytes Array Stream
### To Do
* 原始字符串转换为UTF8编码的字节数组
* 构造传输字节数组，传输字节数组分为三部分:

| 字节       | 说明                                       |
| -------- | ---------------------------------------- |
| 1 - 4    | 0x00, 0xFF, 0x11, 0xEE                   |
| 5 - 10   | 需要传输的字符串转换为字节数组之后的长度，即从第11个字节开始直到最后的数量，以`little-endian`形式存储6个字节 |
| 11 - ... | 需要传输的字节数组                                |

* 发送传输字节数组

### What is `little-endian`

即低位字节排放在低地址端, 高位字节排放在高地址端

如:

`int num = 3435;`

`num` 的十六进制为`00 00 0D 6B`, 则存储在大小为6e的字节数组中的顺序应为:

`6B 0D 00 00 00 00`

C#中`BitConverter.GetBytes(int value)` 可以直接把 `int` 类型的变量转换成 `little-endian` 形式的字节数组, 大小为 `int` 类型的字节长度***4***

