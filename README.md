# 概述

AppPlat 核心辅助类库（UI无关、平台无关）

    - Convertors : 各种数据类型的相互转换
    - Data       : 数据、数据类型转换、序列化相关
    - Draw	     : 绘图相关
    - Linq	     : Linq和EF相关
    - Math       : 算法相关
    - Net        : 网络相关
    - Web        : Asp.net 网站相关（由于非常常用，就不分离出来了）
    - Utils      : 遍历扩展方法

# Nuget Install

```

Nuget: install-package App.Corer
```
> 由于 App.Core nuget名称已经被用了，只能用App.Corer 了，注意区分


# 备注

- 该类库着重可移植性和基础性，不涉及UI、平台相关代码。
- 该类库属于基础类库，变更频率不宜过快。
- 版本发布遵照“主版本号.次版本号.修订版本号”结构。如
    * 1.1.2 与 1.1.0 兼容，只是修改了bug
    * 1.2.0 与 1.1.0 相比，api可能不兼容
    * 2.0.0 与 1.0.0 相比，结构可能会大改，api 基本不兼容
- 处于兼容性及减轻历史负担的折衷考虑
    * 0.x.x 版本会直接抛弃老 api，不考虑兼容性
    * 即将废弃的api会标注上 [Obsolete]，该api只保留2个次版本
    * 在后继第3个次版本时，会废弃该api（事不过三原则）
    

# 常用功能

## Utils
    IsEmpty()         : 字符串为null或为空判断, 对象为空判断
    IsNotEmpty()      : if (name.IsNotEmpt()) 比 if (!name.IsEmpty()) 更为自然
    ToText()          : 对象转化为字符串，对象可以为空。可用于替代 ToString() 方法。

## Convertor
    ToXXX()           : 转换对象类型，如 ToInt, ToEnum, ToXml, ToJson, ToMd5, ToBase64, ToUrlEncode, ToHtmlEncode
    ParseXXX()        : 解析文本为对象，如 ParseInt, ParseEnum, ParseXml, ParseJson
    CastXXX()         : 遍历并转换数据类型，如 var items = array.CastInt();
    Take()            : 遍历并找到匹配的数据，如 var items = students.Take(t => t.Sex==Sex.Male);

## Linq
    SortBy            : 
    SortAndPage       : 
    Between           : 

## Math
    ToMd5             : MD5 采样编码
    ToSHA1            : SHA1 采样编码
    XOR               : XOR 异或编码
    DesXXX            : DES 编解码
    RsaXXX            : RAS 编解码

## Interop
    JsEvaluator       : 解析并运行 Javascript 脚本
    CsEvaluator       : 解析并运行 CSharp 脚本

## Draw
    Drawer            : 绘图辅助类
    FontHelper        : 从文件或资源中获取字体
    VerifyImageDrawer : 图片验证码绘制

## Net
    HttpHelper        : Http 辅助方法
    SocketClient      : Socket 客户端快速实现

## Reflections
    ReflectionHelper  : 各种反射相关的辅助方法

## Serilization
    XmlLizer          : 轻量级Xml序列化反序列化类。支持Dictionary；无需任何attribute标注。
    JsonLizer         : 轻量级json序列化类，可控制层次深度，输出属性个数等。

## Web
    Asp               : Asp.net 相关辅助方法
    AuthHelper        : Asp.net 授权方法
    CookieHelper      : cookie 辅助类
    ScriptHelper      : js 脚本辅助输出类
    ResourceHelper    : 内嵌资源管理类
    Url               : 可方便的处理querystring参数

## UIAttribute
    - 可给类、枚举添加附属信息，非常便利。
    - 本框架广泛使用该标签来简化枚举、类成员的标题输出。

    ```
    public enum EditorType
    {
        [UI("自动选择")]     Auto,
        [UI("标签")]         Label,
    }
    var txt = EditorType.Auto.GetDescription(); // "自动选择"
    ```



## History

2019-06

    - 修正 HttpHelper cookie 和请求头处理，不报异常
    - 修正 PostMultipartForm，增加 Cookie 和 Header 参数

2019-05

    - 重构Convertor类
        * 将各个分散的转换方法聚合在此类中
        * 增加ToBase64(), ParseBase64() 方法
    - 重构HttpHelper 类
        * 给各个方法都增加了 headers 参数
        * 删除方法 PostJson() 方法，请直接用 PostText() 方法
            /// <summary>Post Json 字符串</summary>
            public static string PostJson(string url, string json, Encoding encoding = null, CookieContainer cookieContainer = null, Dictionary<string, string> headers = null)
            {
                return Post(url, json, encoding, "application/json", cookieContainer, headers);
            }
        * 删除 PostDictionary 方法。不通用，容易混淆
            /// <summary>Post 文本字典（会拼装成QueryString的形式）</summary>
            public static string Post(string url, Dictionary<string, string> data, Encoding encoding = null, string contentType = null, CookieContainer cookieContainer = null, Dictionary<string, string> headers = null)
            {
                return Post(url, data.ToQueryString(), encoding, contentType, cookieContainer, headers);
            }
    - 将 HttpHelper 中服务器端处理方法移到 Asp 类中
    - 增加 EncrypHelper.ToHmacSHA256()
    - EncryptHelper 的以下方法迁移到 Convertor 类
            ToByteString
            ToByteSeperateString
            ToBase64String
            ToBase64Bytes
