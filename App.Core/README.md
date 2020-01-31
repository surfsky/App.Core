## History

1.2.0

* 重构Convertor类
    * 将各个分散的转换方法聚合在此类中
    * 增加ToBase64(), ParseBase64() 方法
* 重构HttpHelper 类
    * 给各个方法都增加了 headers 参数
    * 删除方法 PostJson() 方法，请直接用 PostText() 方法
    * 删除 PostDictionary 方法。不通用，容易混淆
* 将 HttpHelper 中服务器端处理方法移到 Asp 类中
+ 增加 EncrypHelper.ToHmacSHA256()
* EncryptHelper 的以下方法迁移到 Convertor 类
        ToByteString
        ToByteSeperateString
        ToBase64String
        ToBase64Bytes
* 修正 HttpHelper cookie 和请求头处理，不报异常
* 修正 PostMultipartForm，增加 Cookie 和 Header 参数
+ 增加分布式ID生成类：SnowflakeID
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

1.7.6
* Modify StringHelper.ToSeparatedString(string text, string seperator)
* Fix ReflectionHelper.GetTypeString()
* Add ReflectionHelper.GetMethodString()

1.7.7
* move HistoryAttribute to App.Core
* move ParamAttribute to App.Core, and simply construnctor
* move RegexValidationAttribute to App.Core


1.7.8
* Param.Optional -> Require, add Regex

1.7.9
* Param.Require -> true

1.7.10
* ParamAttribute + ToString(); Require -> Required
* UIAttribute + Remark + ToString()

1.8.0
* Fix StringHelper.TrimEnd() bug


1.8.1
* Asp.Write(string format, params object[] args)

1.8.2
+ RegexHelper.ReplaceRegex(text, matchRegex, callback)

1.8.3
+ ReflectionHelper.SearchMethods

1.8.4
* ReflectionHelper.GetMethods(type, methodName, searchAncestors)
 
1.8.5
+ Add MethodInfo.GetAttributes<T>
* Adjust ParamAttribute construction functions
* Adjust ReflectionHelper.GetCurrentMethod()
* export xml file to nuget, for programming time


1.8.6
* UISetting.Items auto init data, not null
* ParamAttribute.Type init data is null

1.8.7
+ UIAttribute.Name

1.8.8
* EditorType
* ShowType

1.8.9
+ JsonHelper.AsJObject
+ JsonHelper.AddProperty
+ StringHelper.AddQueryString
* Fix StringHelper.TrimEnd
+ TAttribute - ParamAttribute - UIAttribute
+ UIAttribute support globalizatin, Add AppCoreConfig.Instance
+ Fix GetAttribute&lt;T&gt;()
+ Parse<T>()
+ To<T>()
+ ToChinaNumber
+ Net.Ping

1.9.0
+ Asp.ToVirtualPath()
+ IO.ToRelativePath()
+ ReflectionHelper.GetItemType()
+ GetDescription() support DisplayNameAttribute
+ GetDescription() -> GetTitle()
+ Each、Each2
* GetPropertyValue support subproperty, eg user.Parent.Name
+ Asp.GetHandler() add cache
+ IO.GetDict()
+ Convertor.Merge(List, List)
+ Asp.GetParam()
+ DateTimeHelper.ToFriendlyText()
+ IO.CombinePath(), CombineWebPath()

2.0.0
* ParseJson(txt, ignoreException) can ignore exception
* ToJson() only export name and assembly when export Type to json.
* StringHelper.TrimStartTo()
* SimplyReflectionApi GetPropertyValue->GetValue, GetPropertyInfo->GetProperty
* ReflectionHelper.GetTypeValues -> GetEnumString
+ ReflectionHelper.IsCollection()
* ReflectionHelper.GetName(), GetProperty(), GetTitle(), GetValue(), SetValue()

2.0.1
+ Asp.QueryString
* App.PageMode
* Fix MethodInvoker.InvokeMethod