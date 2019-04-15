using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace App.Core
{

    /// <summary>序列化类型</summary>
    public enum SerializationType
    {
        /// <summary>简单值类型和字符串</summary>
        Value,
        /// <summary>复杂类对象</summary>
        Class,
        /// <summary>列表</summary>
        List,
        /// <summary>数组</summary>
        Array,
        /// <summary>字典</summary>
        Dict
    }

    /// <summary>枚举输出方式</summary>
    public enum EnumFomatting
    {
        Text = 0,
        Int = 1
    }

    /// <summary>
    /// Xml序列化及反序列化操作（无需定义Xml相关的特性标签）
    /// History:
    ///     2019-10-22 实现XML输出，并改为非泛型版本，更为通用，很多情况我们并不知道要序列化的对象的类型
    ///     2019-04-16 实现解析XML为对象
    ///     
    /// Todo:
    ///     优化解析：可处理attribute类型的属性值
    ///     优化输出：构建一个 XmlDocument 对象，最后再根据格式参数再生成 xml 文本
    ///               检测和避免无限循环引用
    /// Author:
    ///     surfsky.cnblogs.com
    /// </summary>
    public class Xmlizer
    {
        public bool FormatLowCamel { get; set; } = false;
        public EnumFomatting FormatEnum { get; set; } = EnumFomatting.Text;
        public string FormatDateTime { get; set; } = "yyyy-MM-dd HH:mm:ss";
        public bool FormatIndent { get; set; } = false;



        //-------------------------------------------------
        // 构造析构
        //-------------------------------------------------
        /// <summary>Xml序列化</summary>
        /// <param name="xmlHead">XML文件头<?xml ... ?></param>
        /// <param name="useCData">是否需要CDATA包裹数据</param>
        public Xmlizer(bool formatLowCamel=false, EnumFomatting formatEnum=EnumFomatting.Text, string formatDateTime="yyyy-MM-dd HH:mm:ss", bool formatIndent=false)
        {
            this.FormatLowCamel = formatLowCamel;
            this.FormatEnum = formatEnum;
            this.FormatDateTime = formatDateTime;
            this.FormatIndent = formatIndent;
        }

        //-------------------------------------------------
        // 对象转 XML
        //-------------------------------------------------
        /// <summary>序列化报文为xml</summary>
        /// <param name="obj"></param>
        /// <param name="rootName">根节点名称</param>
        /// <param name="ignoreNull">是否跳过空元素</param>
        /// <param name="addXmlHead">是否添加xml头部</param>
        public string ToXml(object obj, string rootName, bool ignoreNull=true, bool addXmlHead=false)
        {
            var sb = new StringBuilder();
            if (rootName.IsEmpty())
                rootName = GetTagName(obj.GetType());
            if (addXmlHead)
                sb.AppendFormat("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
            WriteXml(sb, obj, rootName, ignoreNull);
            return sb.ToString();
        }


        /// <summary>访问对象</summary>
        private void WriteXml(StringBuilder sb, object obj, string name, bool ignoreNull=true)
        {
            name = GetTagName(name);

            // 对象为空处理
            if (obj == null)
            {
                if (!ignoreNull && name.IsNotEmpty())
                    sb.AppendFormat("<{0}/>", name);
                return;
            }

            // 正常处理
            var type = obj.GetType();
            if (type.IsNullable())
                type = type.GetNullableDataType();
            if (name.IsEmpty())
                name = GetTagName(type);

            // 根据类型进行输出（还要判断可空类型）
            sb.AppendFormat("<{0}>", name);
            if (obj is string)
            {
                sb.Append(GetXmlSafeText(obj));
            }
            else if (obj is DateTime)
            {
                var dt = Convert.ToDateTime(obj);
                if (dt != new DateTime())
                    sb.AppendFormat(dt.ToString(this.FormatDateTime));
            }
            else if (type.IsEnum)
            {
                if (this.FormatEnum == EnumFomatting.Int) sb.AppendFormat("{0:d}", obj);
                else sb.AppendFormat("{0}", obj);
            }
            else if (obj is DataTable)
            {
                var table = obj as DataTable;
                var cols = table.Columns;
                foreach (DataRow row in table.Rows)
                {
                    sb.AppendFormat("<Row>");
                    foreach (DataColumn col in cols)
                    {
                        var columnName = col.ColumnName;
                        WriteXml(sb, row[columnName], columnName);
                    }
                    sb.AppendFormat("</Row>");
                }
            }
            else if (obj is IDictionary)
            {
                var dict = (obj as IDictionary);
                foreach (var key in dict.Keys)
                {
                    sb.AppendFormat("<Item Key=\"{0}\">", key);
                    WriteXml(sb, dict[key], "");
                    sb.AppendFormat("</Item>");
                }
            }
            else if (obj is IEnumerable)
            {
                foreach (var item in (obj as IEnumerable))
                    WriteXml(sb, item, "");
            }
            else if (type.IsValueType)
            {
                sb.AppendFormat("{0}", obj);
            }
            else
            {
                var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    if (ReflectionHelper.GetAttribute<NonSerializedAttribute>(property) != null
                        || ReflectionHelper.GetAttribute<JsonIgnoreAttribute>(property) != null
                        || ReflectionHelper.GetAttribute<System.Xml.Serialization.XmlIgnoreAttribute>(property) != null
                        )
                        continue;

                    var subObj = property.GetValue(obj);
                    WriteXml(sb, subObj, property.Name);
                }
            }
            sb.AppendFormat("</{0}>", name);
        }

        /// <summary>获取标签名</summary>
        private string GetTagName(string name)
        {
            if (this.FormatLowCamel)
                name = name.ToLowCamel();
            return name;
        }


        /// <summary>获取节点名</summary>
        string GetTagName(Type type)
        {
            if (type.Name.Contains("AnonymousType"))
                return "Item";
            if (type.GetInterface("IDictionary") != null)
                return "Dictionary";
            if (type.IsGenericType)
                return GetTagName(type.GetGenericDataType()) + "s";
            if (type.IsArray)
                return GetTagName(type.GetElementType()) + "s";
            return type.Name;
        }

        /// <summary>获取Xml安全文本</summary>
        static string GetXmlSafeText(object obj)
        {
            // "<" 字符和"&"字符对于XML来说是严格禁止使用的，可用转义符或CDATA解决
            var txt = obj.ToString();
            if (txt.IndexOfAny(new char[] { '<', '&' }) != -1)
                return string.Format("<![CDATA[ {0} ]]>", txt);
            return txt;
        }


        //-------------------------------------------------
        // XML转对象
        //-------------------------------------------------
        /// <summary>类型对应的标签信息</summary>
        public class TypeTag
        {
            public SerializationType Type { get; set; }
            public Type ItemType { get; set; }
            public string Name { get; set; }

            public TypeTag(SerializationType type, Type itemType, string name)
            {
                this.Type = type;
                this.ItemType = itemType;
                this.Name = name;
            }

            /// <summary>获取类型相关信息</summary>
            public static TypeTag FromType(Type type)
            {
                var realType = type.GetRealType();
                if (realType.IsValueType || type == typeof(string))
                    return new TypeTag(SerializationType.Value, type, type.Name);
                if (type.Name.Contains("AnonymousType"))
                    return new TypeTag(SerializationType.Class, type.GenericTypeArguments[0], "Anonymous");
                if (type.IsGenericType)
                    return new TypeTag(SerializationType.List, type.GenericTypeArguments[0], type.GenericTypeArguments[0].Name + "s");
                if (type.IsType(typeof(IDictionary)))
                    return new TypeTag(SerializationType.Array, type.GetTypeInfo().GetElementType(), "Dictionary");
                if (type.IsArray)
                    return new TypeTag(SerializationType.Array, type.GetTypeInfo().GetElementType(), type.GetTypeInfo().GetElementType().ToString() + "s");
                return new TypeTag(SerializationType.Class, type.GetType(), type.Name);
            }
        }


        /// <summary>Xml字符串序列化为对象</summary>
        public T Parse<T>(string xml, string rootTag="") where T : class
        {
            // 去掉<?xml?>行
            xml = xml.TrimStart();
            int index;
            if (xml.StartsWith("<?xml") && (index = xml.IndexOf("?>")) != -1)
                xml = xml.Substring(index + 2).Trim('\r', '\n', ' ');

            try
            {
                return Parse(xml, typeof(T), rootTag) as T;
            }
            catch (Exception ex)
            {
                throw new Exception($"反序列化对象信息异常:{ex.Message}", ex);
            }
        }

        /// <summary>解析Xml为object</summary>
        private object Parse(string xml, Type type, string tagName="")
        {
            var tag = TypeTag.FromType(type);
            if (tagName.IsNotEmpty())
                tag.Name = tagName;

            switch (tag.Type)
            {
                case SerializationType.Value:      return xml.Parse(type);
                case SerializationType.List:       return ParseToList(xml, type);
                case SerializationType.Array:      return ParseToArray(xml, type);
                default:                     return ParseToObject(xml, type);
            }
        }

        /// <summary>将xml解析为对象</summary>
        private object ParseToObject(string xml, Type type)
        {
            var o = Activator.CreateInstance(type);
            var fields = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields)
            {
                var fieldType = field.PropertyType;
                var content = GetTagContent(xml, field.Name);
                if (content == null)
                    continue;

                var fieldValue = Parse(content, fieldType);
                field.SetValue(o, fieldValue);
            }
            return o;
        }

        /// <summary>将xml解析为集合</summary>
        private IList ParseToList(string xml, Type type)
        {
            var tag = TypeTag.FromType(type);
            var list = Activator.CreateInstance(type) as IList;
            List<string> contents = GetTagContents(xml, tag.ItemType.Name);
            foreach (var content in contents)
            {
                var item = Parse(content, tag.ItemType);
                list.Add(item);
                //AddElement(collection, content, obj => { Add(collection, obj); });
            }
            return list;
        }

        /// <summary>将xml解析为数组</summary>
        private object ParseToArray(string xml, Type type)
        {
            var tag = TypeTag.FromType(type);
            List<string> contents = GetTagContents(xml, tag.ItemType.Name);
            Array array = Array.CreateInstance(type, contents.Count);
            var collection = Convert.ChangeType(array, type);
            int index = 0;
            foreach (var content in contents)
            {
                var item = Parse(content, tag.ItemType);
                SetValue(collection, item, index++);
                //AddElement(collection, content, obj => { SetValue(collection, obj, index++); });
            }
            return collection;
        }


        /// <summary>添加元素到集合中</summary>
        private void Add<T>(T collection, object obj)
        {
            var methodInfo = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name.Equals("Add"));
            if (methodInfo == null)
                throw new Exception($"反序列化集合xml内容失败，目标{typeof(T).FullName}非集合类型");

            var instance = Expression.Constant(collection);
            var param = Expression.Constant(obj);
            var addExpression = Expression.Call(instance, methodInfo, param);
            var add = Expression.Lambda<Action>(addExpression).Compile();
            add.Invoke();
        }

        /// <summary>添加元素到集合中</summary>
        private void SetValue<T>(T collection, object obj, int index)
        {
            var methodInfo = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name.Equals("SetValue"));
            if (methodInfo == null)
                throw new Exception($"反序列化集合xml内容失败，目标{typeof(T).FullName}非集合类型");

            var instance = Expression.Constant(collection);
            var param1 = Expression.Constant(obj);
            var param2 = Expression.Constant(index);
            var addExpression = Expression.Call(instance, methodInfo, param1, param2);
            var setValue = Expression.Lambda<Action>(addExpression).Compile();
            setValue.Invoke();
        }



        //-------------------------------------------
        // 辅助方法
        //-------------------------------------------
        /*
        /// <summary>获取字符中指定标签的值（可解析CDATA标签）</summary>  
        public static string GetTagContent(string content, string tagName, string attrib, bool parseCData=true)
        {
            var valueRegex = parseCData ? "<!\\[CDATA\\[(.*)\\]\\]>" : "([\\s\\S]*?)";
            var tagRegex =  $"<{tagName}\\s*{attrib}\\s*=\\s*.*?>{valueRegex}</{tagName}>";
            var match = Regex.Match(content, tagRegex, RegexOptions.IgnoreCase);
            return  (match.Success) ? match.Groups[1].Value : null;
        }
        */
        /// <summary>获取字符中指定标签的值（可解析CDATA标签）</summary>
        public static string GetTagContent(string content, string tagName)
        {
            var tagRegex = string.Format(@"<{0}>([\s\S]*?)</{0}>", tagName);
            var match = Regex.Match(content, tagRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                return ParseCDATA(match.Groups[1].Value);
            return null;
        }

        /// <summary>获取字符中指定标签的值列表</summary>  
        public static List<string> GetTagContents(string content, string tagName)
        {
            var values = new List<string>();
            var tagRegex = string.Format(@"<{0}>([\s\S]*?)</{0}>", tagName);
            var matches = Regex.Matches(content, tagRegex, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                var value = ParseCDATA(match.Groups[1].Value);
                values.Add(value);
            }
            return values;
        }

        /// <summary>解析CDATA内容（以<![CDATA[开头)</summary>
        private static string ParseCDATA(string txt)
        {
            var tagRegex = @"^<!\[CDATA\[(.*)\]\]>";
            var match = Regex.Match(txt.TrimStart(), tagRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;
            else
                return txt;
        }

    }
}
