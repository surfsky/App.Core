# Description

AppPlat Core utility library (ui-independ, platform-independ)

This module supports many useful extensions and methods, such as:
    - ToXXX
    - CastXXX
    - ParseXXX
    - Reflection
    - Encode/Decode
    - Interop
    - EF extensions
    - ...

# Nuget Install

```

Nuget: install-package App.Corer
```
> The desired name *App.Core* has been used in nuget，so I have to use *App.Corer*, ATTENTION*

# Examples

## Utils
```csharp
        [TestMethod()]
        public void IsEmptyTest()
        {
            // string
            string text = null;
            Assert.IsTrue(text.IsEmpty());
            text = "";
            Assert.IsTrue(text.IsEmpty());
            text = "aa";
            Assert.IsTrue(text.IsNotEmpty());

            // list
            List<string> arr = null;
            Assert.IsTrue(arr.IsEmpty());
            arr = new List<string> { };
            Assert.IsTrue(arr.IsEmpty());
            arr = new List<string> { "aa" };
            Assert.IsTrue(arr.IsNotEmpty());

            // object
            Person p = null;
            Assert.IsTrue(p.IsEmpty());
            p = new Person();
            Assert.IsTrue(p.IsNotEmpty());

        }
        [TestMethod()]
        public void IIFTest()
        {
            var score = 2000;
            var result = score.IIF(t => t > 1000, "High", "Low");
        }

        [TestMethod()]
        public void IndexOfTest()
        {
            var items = new string[] { "ID", "Name", "Url" };
            var n = items.IndexOf(t => t == "Name");
            Assert.AreEqual(n, 1);
        }
```
## Cache
```csharp
        [TestMethod()]
        public void GetCacheTest()
        {
            var key = "Name";
            var name1 = IO.GetCache(key, () => "Kevin");
            var name2 = IO.GetCache<string>(key);
            Assert.AreEqual(name1, name2);
            IO.SetCache(key, "John");
            var name3 = IO.GetCache<string>(key);
            Assert.AreEqual(name3, "John");
        }
```

## File name and url
```csharp
        [TestMethod()]
        public void GetFileNameTest()
        {
            string url = "http://oa.wzcc.com/oamain.aspx?a=x&b=xx";
            var name = url.GetFileName();
            var ext = url.GetFileExtension();
            var folder = url.GetFileFolder();
            var q = url.GetQuery().ToString();
            var u = url.TrimQuery();
        }

        [TestMethod()]
        public void GetNextNameTest()
        {
            var name1 = "c:\\folder\\filename.doc?x=1";

            //
            var name2 = name1.GetNextName(@"_{0}").GetNextName(@"_{0}");
            Assert.AreEqual(name2, "c:\\folder\\filename_3.doc?x=1");

            //
            var name3 = name1.GetNextName(@"-{0}").GetNextName(@"-{0}");
            Assert.AreEqual(name3, "c:\\folder\\filename-3.doc?x=1");

            //
            var name4 = name1.GetNextName(@"({0})").GetNextName(@"({0})");
            Assert.AreEqual(name4, "c:\\folder\\filename(3).doc?x=1");
        }
```

## Parse
```csharp
[TestMethod()]
        public void ToEnumTest()
        {
            object o;
            ProductType type = ProductType.Repair;
            o = type.GetDescription();
            o = "Goods".ParseEnum<ProductType>();
            o = "1".ParseEnum<ProductType>();
        }
[TestMethod()]
        public void ParseSimplyXmlTest()
        {
            var xml = "<Person><Name>Kevin</Name><Age>21</Age><Birthday>2001-01-01</Birthday><Sex></Sex></Person>";
            var o = xml.ParseXml<Person>();
            var name = o.Name;
            var age = o.Age;
            var birthday = o.Birthday;
        }



        [TestMethod()]
        public void ParseDynamicTest()
        {
            var json = "{name:'Kevin', age:21}";
            var o = json.ParseDynamic();
            string name = o.name;
            int age = o.age;
        }

        [TestMethod()]
        public void ParseJObjectTest()
        {
            var json = "{name:'Kevin', age:21}";
            var o = json.ParseJObject();

            var nameObject = o["name"];
            var ageObject = o["age"];

            var nameString = (string)o["name"];
            var ageInt = (int)o["age"];

            string name = o["name"].ToText();
            int? age = o["age"].ToString().ParseInt();  //.ToInt();
        }


        [TestMethod()]
        public void ParseDictTest()
        {
            var txt = "id=1&name=Kevin";
            var dict = txt.ParseDict();
            var id = dict["id"];
            var age = dict["age"];
            dict["age"] = "5";
            age = dict["age"];
            dict.Remove("id");
            dict.Remove("birthday");
            txt = dict.ToString();
        }

        [TestMethod()]
        public void ToBinaryStringTest()
        {
            int n = 99;
            var text = n.ToBitString();
            var bytes = text.ToBitBytes();
            var m = bytes.ToInt32();
        }

        [TestMethod()]
        public void UnicodeTest()
        {
            var txt1 = @"亲爱的，你慢慢飞，小心前面带刺的玫瑰...";
            var txt2 = @"\u4eb2\u7231\u7684\uff0c\u4f60\u6162\u6162\u98de\uff0c\u5c0f\u5fc3\u524d\u9762\u5e26\u523a\u7684\u73ab\u7470...";
            var encode = txt1.UnicodeEncode();
            var decode = txt2.UnicodeDecode();
            Assert.IsTrue(encode == txt2);
            Assert.IsTrue(decode == txt1);
        }

        [TestMethod()]
        public void ParseDateTest()
        {
            string txt3 = null;
            var dt1 = "2019-01-01".ParseDate();
            var dt2 = "".ParseDate();
            var dt3 = txt3.ParseDate();
        }



        [TestMethod()]
        public void ParseBoolTest()
        {
            string t = null;
            Assert.AreEqual("true".ParseBool(), true);
            Assert.AreEqual("false".ParseBool(), false);
            Assert.AreEqual("True".ParseBool(), true);
            Assert.AreEqual("False".ParseBool(), false);
            Assert.AreEqual("Yes".ParseBool(), null);
            Assert.AreEqual("".ParseBool(), null);
            Assert.AreEqual(t.ParseBool(), null);
        }

        [TestMethod()]
        public void ParseEnumTest()
        {
            Assert.AreEqual("Male".ParseEnum<SexType>(), SexType.Male);
            Assert.AreEqual("0".ParseEnum<SexType>(), SexType.Male);
            Assert.AreEqual("Male,Female".ParseEnums<SexType>(), new List<SexType>() { SexType.Male, SexType.Female });
            Assert.AreEqual("0,1".ParseEnums<SexType>(), new List<SexType>() { SexType.Male, SexType.Female });
        }

        [TestMethod()]
        public void ParseTest()
        {
            string s = "1";
            var o = s.Parse<string>();
            var n = s.Parse<int>();
            var b = s.Parse<bool?>();
            Assert.AreEqual(o, "1");
            Assert.AreEqual(n, 1);
            Assert.AreEqual(b, null);
        }

        [TestMethod()]
        public void ToASCStringTest()
        {
            var txt = "abcdefg";
            var bytes = txt.ToASCBytes();
            var asc = bytes.ToASCString();
            Assert.AreEqual(txt, asc);
        }

        [TestMethod()]
        public void ToHexStringTest()
        {
            var enc = Encoding.UTF8;
            var txt = "abcdefg";
            var bytes = txt.ToBytes(enc);

            var hexText = txt.ToHexString(enc);
            var bytes2 = hexText.ToHexBytes();

            var txt2 = bytes2.ToString(enc);
            Assert.AreEqual(txt, txt2);
        }

```

## Setting
```csharp
        [TestMethod()]
        public void GetAppSettingTest()
        {
            var o1 = IO.GetAppSetting<int?>("MachineID");
            var o2 = IO.GetAppSetting<int?>("machineID");
            var o3 = IO.GetAppSetting<int?>("NotExist");
            Assert.AreEqual(o1, o2);
            Assert.AreEqual(o3, null);
        }
```

## Encrypt
```csharp
[TestMethod()]
        public void DesEncryptTest()
        {
            var key = "12345678";
            var msg = "Hello world";
            var encrypt = msg.DesEncrypt(key);
            var decrypt = encrypt.DesDecrypt(key);
        }

        [TestMethod()]
        public void RSACreateKeyPairTest()
        {
            var msg = "hello world";
            var pair = EncryptHelper.RSACreateKeyPair();
            string encrytedMsg = EncryptHelper.RSAEncrypt(msg, pair.Key);
            string decrytedMsg = EncryptHelper.RSADecrypt(encrytedMsg, pair.Value);
        }

        [TestMethod()]
        public void MD5Test()
        {
            var txt = "Hello world!";
            var m3 = txt.MD5();        // "86FB269D190D2C85F6E0468CECA42A20"
            var m4 = txt.MD5();        // "86FB269D190D2C85F6E0468CECA42A20"
            var s = txt.SHA1();        // "D3486AE9136E7856BC42212385EA797094475802"
            var s2 = txt.HmacSHA256(); // "852D2FEC4BDA6ADD8F12C5C1DFF8420510AC5B85EF432140C7097AAEE3C270CA"

            var t2 = "expireDt=1571896546&name=190929.%E7%94%B5%E4%BF%A1%E7%9F%A5%E8%AF%86%E5%BA%93.sketch&nonceStr=6727658767&url=%2fFiles%2fArticles%2f191011-fb27021dfe9145adac4bde7f6f4b17b5.sketch&key=SignKey";
            var m1 = t2.MD5();         // "C6CEBD9247AAB3A6EDAA7629F404CC50"
            var m2 = t2.MD5();         // "C6CEBD9247AAB3A6EDAA7629F404CC50"
            Assert.AreEqual(m1, m2);
        }
```

## Convertor
```csharp
[TestMethod()]
        public void ToEnumTest()
        {
            object o;
            ProductType type = ProductType.Repair;
            o = type.GetDescription();
            o = "Goods".ParseEnum<ProductType>();
            o = "1".ParseEnum<ProductType>();
        }

        [TestMethod()]
        public void ToTextTest()
        {
            string txt = null;
            var info = txt.ToText("");

            DateTime dt = DateTime.Now;
            info = dt.ToText("{0:yyyy-MM-dd}");
            info = dt.ToText("yyyy-MM-dd");

            DateTime? dt2 = null;
            info = dt2.ToText("yyyy-MM-dd");
        }
```

## Reflection
```csharp
        [TestMethod()]
        public void GetPropertyNameTest()
        {
            //var name = ReflectionHelper.GetPropertyName<Person>(t => t.Name);
            var p = new Person() {Name="kevin", Sex=SexType.Male };
            var n = p.GetPropertyValue(t => t.Name);
            Assert.AreEqual(n, "kevin");
        }

        [TestMethod()]
        public void GetCurrentMethodNameTest()
        {
            var method = ReflectionHelper.GetCurrentMethodInfo();
            Assert.AreEqual(method.Name, "GetCurrentMethodNameTest");
        }

        [TestMethod()]
        public void GetEventDelegatesTest()
        {
            var person = new Person("Kevin");
            person.Speak += (t) => { IO.Trace(t); };
            var subscribers = ReflectionHelper.GetEventSubscribers(person, nameof(Person.Speak));
            foreach (var t in subscribers)
                IO.Trace("sender={0}, method={1}", t.Target.GetType().FullName, t.Method.Name);
        }
```

## String
```csharp
[TestMethod()]
        public void TrimEndTest()
        {
            var t1 = "ProductNameID";
            var t2 = t1.ReplaceRegex("Name", "Key");
            var t3 = t2.TrimEnd("ID");
        }

        [TestMethod()]
        public void RemoveBlankTest()
        {
            var t1 = @"
                <script>ProductNameID</script>
                <!- sfdsfdsfdsfdremark ->
                Text \r\n Enter \v\f ProductNameIDProductNameIDProductNameIDProductNameID
                etc 
                ";
            var t2 = t1.RemoveHtml();
            var t3 = t1.RemoveBlank();
            var t4 = t1.RemoveBlankTranslator();
            var t5 = t1.Slim();
            var t6 = t1.RemoveTag();

            var t7 = t1.RemoveHtml().Slim().RemoveBlankTranslator().Summary(20);
            var t10 = t1.RemoveTag().RemoveBlankTranslator().Slim().Summary(20);
        }

        [TestMethod()]
        public void QuoteTest()
        {
            var t1 = "\"Text is 'text' \"  is \r\n 'Enter' \v\f \t Prtempl";
            var t2 = t1.Quote();
            var t3 = t2.Unquote();
            Assert.AreEqual(t3, t1);
            var t4 = t1.Escape('t');
            var t5 = t4.Unescape();
            Assert.AreEqual(t5, t1);
        }

        [TestMethod()]
        public void RemoveHtmlTest()
        {
            var t1 = @"
hello world
<script>
function do() {
    console.write('hello world');
}
</script>
<style>
@font-face {
	font-family: 宋体;
}
</style>
";
            var t2 = t1.RemoveHtml();
            var t3 = t1.RemoveStyleBlock();
            var t4 = t1.RemoveStyleBlock();
            Assert.AreEqual(t2, "hello world");
        }


        [TestMethod()]
        public void SplitTest()
        {
            var t1 = "1,2,3,4,5";
            var t2 = "1 2 3 4 5";
            var a1 = t1.Split<int>();
            var a2 = t2.Split<string>();
        }

        [TestMethod()]
        public void SubTextTest()
        {
            var text = "0123456789";
            Assert.AreEqual(text.SubText(0, 8), "01234567");
            Assert.AreEqual(text.SubText(0, 10), "0123456789");
            Assert.AreEqual(text.SubText(0, 12), "0123456789");
            Assert.AreEqual(text.SubText(0, 5), "01234");
            Assert.AreEqual(text.SubText(5, 12), "56789");
            Assert.AreEqual(text.SubText(0, 12), "0123456789");
        }

        [TestMethod()]
        public void ContainsTest()
        {
            var str = "Hello world";
            Assert.IsTrue(str.Contains("Hello", true));
            Assert.IsTrue(str.Contains("hello", true));
            Assert.IsFalse(str.Contains("hello", false));
            Assert.IsFalse(str.Contains("", true));
        }

        [TestMethod()]
        public void ToSizeTextTest()
        {
            long size1 = 786;
            long size2 = (long)(15.78 * 1024);
            long size3 = (long)(15.70 * 1024 * 1024);
            long size4 = (long)(15.782 * 1024 * 1024 * 1024);
            long size5 = (long)(15.786 * 1024 * 1024 * 1024 * 1024);
            Assert.AreEqual(size1.ToSizeText(), "786 bytes");
            Assert.AreEqual(size2.ToSizeText(), "15.78 KB");
            Assert.AreEqual(size3.ToSizeText(), "15.7 MB");
            Assert.AreEqual(size4.ToSizeText(), "15.78 GB");
            Assert.AreEqual(size5.ToSizeText(), "15.79 TB");

            Assert.AreEqual(size3.ToSizeText("{0:0.00}"), "15.70 MB");
        }
```

## SnowflakeID
```csharp
[TestMethod()]
        public void SnowflakeIDTest()
        {
            // 生成
            var snow = new SnowflakeID(1);
            var ids = new List<long>();
            for (int i = 0; i < 1000; i++)
            {
                long id = snow.NewID();
                ids.Add(id);
            }

            // 解析
            foreach (var id in ids)
            {
                IO.Write("{0} : {1}", id.ToString(), id.ToBitString());
                var snowId = SnowflakeID.Parse(1259605479504482304);
                var timestamp = snowId.TimeStamp;
                var machine = snowId.Machine;
                var sequence = snowId.Sequence;
            }
        }

        [TestMethod()]
        public  static void TestShift()
        {
            ulong n1 = 12;
            ulong n2 = 12 << 4;
            IO.Write("{0} {1}", n1.ToBitString(), n2.ToBitString());
        }
```

##Interop
```csharp
[TestMethod()]
        public void EvalTest()
        {
            var eval = new CsEvaluator();
            var b = eval.EvalBool("5 > 4");
            var d = eval.EvalDecimal("2.5");
            var o = eval.Eval("new DateTime(2018,1,1)");
            var t = eval.EvalDateTime("new DateTime(2018,1,1)");
        }
```

## Linq
```csharp
[TestMethod()]
        public void SortTest()
        {
            var persons = Person.GetPersons();
            var page0 = persons.AsQueryable().SortAndPage(t => t.Age, true, 0, 3).ToList();
            var page1 = persons.AsQueryable().SortAndPage(t => t.Age, true, 1, 3).ToList();
            var page2 = persons.AsQueryable().SortAndPage(t => t.Age, true, 2, 3).ToList();
            var page3 = persons.AsQueryable().SortAndPage(t => t.Age, true, 3, 3).ToList();
            var page4 = persons.AsQueryable().SortAndPage(t => t.Age, true, 4, 3).ToList();
            Assert.AreEqual(page0.Count, 3);
            Assert.AreEqual(page1.Count, 3);
            Assert.AreEqual(page2.Count, 3);
            Assert.AreEqual(page3.Count, 1);
            Assert.AreEqual(page4.Count, 0);
        }
```

## Url
```csharp
[TestMethod()]
        public void UrlTest()
        {
            // 完整url测试
            var url = new Url("http://www.company.com:8080/a/b/c.aspx?mode=new&parentid=1");
            url["mode"] = "edit";
            url["id"] = "5";
            url["more"] = "8";
            url.Remove("parentid");
            var txt = url.QueryString;

            // 仅参数部分
            url = new Url("mode=new&id=1");
            var mode = url["mode"];
            var id = url["id"];
            url["mode"] = "edit";
            txt = url.QueryString;

            // 仅前面部分
            url = new Url("http://www.company.com:8080/a/b/c.aspx");
            mode = url["mode"];
            id = url["id"];
            url["mode"] = "edit";
            txt = url.QueryString;
        }
```

# More

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
    Md5               : MD5 采样编码
    SHA1              : SHA1 采样编码
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
    

## History


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

2019-06

- 修正 HttpHelper cookie 和请求头处理，不报异常
- 修正 PostMultipartForm，增加 Cookie 和 Header 参数

2019-07

- 增加分布式ID生成类：SnowflakeID


2019-08
* Convertor.ToCommaString 增加 seperator 可选参数，默认值为','
+ Convertor public static string ToString(this byte[] bytes, Encoding encoding = null)
+ StringHelper.Quato(); Unquate(); Escape(), Unescape()
* StringHelper.RemoveHtml(), RemoveScriptBlock(), RemoveStyleBlock() 修正
+ IO.GetNextName() ，构建后继文件名（附加递增数字），如：rawname_2.eml, rawname_3.eml
+ StringHelper.ToSizeText(this long bytes) , 转化为文件大小文本（如 1.3M）


1.2.6
- 删除XmlSerializeHelper，直接用 XmlHelper
- JsonHelper 集中所有 Json 相关操作
- MathHelper.Approx 小数约等于判断

1.2.7
- EncryptHelper.ToMD5        -> MD5
- EncryptHelper.ToSHA1       -> SHA1
- EncryptHelper.ToHmacSHA256 -> HmacSHA256
- Convertor.ToUrlEncode      -> UrlEncode 
- Convertor.ToUrlDecode      -> UrlDecode 
- Convertor.ToHtmlEncode     -> HtmlEncode
- Convertor.ToHtmlDecode     -> HtmlDecode
- Add Convertor.UnicodeEncode/UnicodeDecode
 

1.2.8
- ParseXXX(...) 将不抛出异常，若解析失败，则返回null
- Fix RegexHelper bugs: AABB, ABAB, Int....etc

1.3.0
- Asp.GetCacheData() 迁移到IO.GetCacheData()，采用 HttpRuntime.Cache，替代 HttpContext.Current.Cache, 可用于非Web环境。
- 修正 AssemblyVersion，采用 Assembly.GetEntryAssembly() 替代 CallingAssembly

1.4.0
- object ParseBasicType<T>(..) 返回值改为  T ParseBaseType<T>(...)
- 废除ToInt，ToFloat，ToDouble(this object o)等方法，请用ParseXXX()方法替代，侵入性过强。
- 废除 public static byte[] ToBytes(this object o)，侵入性过强。以后扩展方法尽量不直接用this object参数
- Convertor.ToCommonString -> ToSeparatedString

1.4.4
- UIAttribute 增加3个构造方法
- UISetting 增加Groups属性

1.4.5
- IO.GetCache(), SetCache(), RemoveCache()


1.4.6 /7/9
- App.Core.Data -> App.Core.Base
- BytesExtension -> ByteHelper
- EnumInfo.Info -> ToString()
- + Type.GetDescription()


1.5.0
+ Asp.IsRequestValid

1.5.1
+ EditorType.Switch
+ EditorType.EnumGroup
+ UIAttribute.GetUIGroup(this Type type)

1.5.3
+ ShowType [Flags], ExportType [Flags]
+ RegexHelper.IsMatch\ReplaceRegex 增加大小写忽略选项

1.5.4
* FreeDictionary -> App.Core
* fix CastInt64

1.5.5/6
* fix Asp.BuildRequestHtml
+ HistoryAttribute
+ ParamAttribute

1.5.7
+ Asp.GetHandler

1.5.8
+ Convertor.Parse<T>()

1.5.9
+ StringHelper.Split<T>()

1.5.10
* Asp.GetCompiledType no exception
* Utils.IndexOf<T>(Func<T, bool> condition)

1.5.11
* MathHelper.Inc(ref int i, int n, int? max) change param 1 to ref
* Asp.GetHandler don't throw exception

1.5.12
* MathHelper.Inc(int i, int n, int? max) recover

1.5.13
* Utils.IsEmpty() support null, string, IEnumerable


1.5.14
* Fix MathHelper.Inc, Dec bug

1.5.15
+ IO.GetQueryString(this url)
+ GZipper
* ToHexBytes() ToHexString()
* StringHelper.SubText(int startIndex, int length)

1.6.0
* EFExtension.SortBy() -> Sort()

1.6.1
* EFExtension.Page() fix bug

1.6.2
+ IO.Mimes
* IO.GetMimeType()
* Asp.WriteFile() auto fix mimetype

1.6.3
+ StringHelper.Contains(this string text, string value, bool ignoreCase=true)

1.6.4
* Asp.WriteFile(string filePath, string mimeType = "", string attachName = "") rename last parameter to avoid chaos

1.6.5
+ ParamAttribute.Optional
+ IO.GetAppSetting<T>(string key)
+ Asp.GetQuery<T>(string key)

1.6.6
+ Net.IPs
* Asp.WriteFile() buffer mode
+ Asp.WriteBigFile()
+ Asp.WriteStream()


1.6.8
* Asp.IsLocalFile -> IsSiteFile
* Asp.MapPath... -> extension function

1.6.9
* AuthHelper.LoadPrincipalFromCookie no exception

1.7.0
* Asp.WriteFile: CacheControl = "no-cache";
* SetCachePolicy(httpcontext -> httpResponse)

1.7.1
* StringHelper.ToSizeText  supports TB

1.7.2
* StringHelper.ToSizeText  supports format
* IO.MimeType update office file mimetype

1.7.3
+ ReflectionHelper.GetEventSubscribers()
+ TypeBuilder

1.7.4
* Asp.BuildRequestCoreInfo -> BuildRequestInfo
+ ReflectionHelper.GetAttributes()


1.7.5
+ Utils.GetText(format, args)
+ Asp.Write(format, args)
+ App.AspTest project