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
using System.Xml;

namespace App.Core
{
    /// <summary>枚举输出方式</summary>
    public enum EnumFomatting
    {
        Text = 0,
        Int = 1
    }

    /// <summary>序列化类型</summary>
    public enum SerializationType
    {
        /// <summary>简单值类型和字符串（可以直接格式化为文本）</summary>
        Simple,
        /// <summary>类或结构体对象</summary>
        Class,
        /// <summary>列表</summary>
        List,
        /// <summary>数组</summary>
        Array,
        /// <summary>字典</summary>
        Dict
    }

    /// <summary>序列化节点信息</summary>
    public class SerializationNode
    {
        public SerializationType Type { get; set; }
        public Type ItemType { get; set; }
        public string Name { get; set; }

        public SerializationNode(SerializationType type, Type itemType, string name)
        {
            this.Type = type;
            this.ItemType = itemType;
            this.Name = name;
        }

        /// <summary>获取类型相关信息</summary>
        public static SerializationNode FromType(Type type)
        {
            var realType = type.GetRealType();
            if (realType.IsSimpleType())    return new SerializationNode(SerializationType.Simple, type, type.Name);
            if (type.IsAnonymous())         return new SerializationNode(SerializationType.Class, type.GenericTypeArguments[0], "Anonymous");
            if (type.IsGenericDict())       return new SerializationNode(SerializationType.Dict, type.GenericTypeArguments[1], type.GenericTypeArguments[1].Name + "s");
            if (type.IsGenericList())       return new SerializationNode(SerializationType.List, type.GenericTypeArguments[0], type.GenericTypeArguments[0].Name + "s");
            if (type.IsArray)               return new SerializationNode(SerializationType.Array, type.GetTypeInfo().GetElementType(), type.GetTypeInfo().GetElementType().ToString() + "s");
            return new SerializationNode(SerializationType.Class, type, type.Name);
        }
    }


    /// <summary>
    /// Xml序列化及反序列化操作类（无需定义Xml相关的特性标签）
    /// </summary>
    /// <remarks>
    /// Author: surfsky.cnblogs.com
    /// LastUpdate: 2019-04-18
    /// 
    /// [已实现]
    ///     2019-10-22 实现XML输出（并改为非泛型版本，更为通用，很多情况我们并不知道要序列化的对象的类型）
    ///         支持简单类型：字符串、值类型。
    ///         支持List：<Persons><Person/>...<Person/></Persons>
    ///         支持Dict：<Items><Item Key="a">value</Item>...<Item Key="b">value</Item></Items>
    ///         支持Array：<Persons></Persons>
    ///         支持Table：<Table><Row>...</Row></Table>
    ///         支持类类型：<Object><PropertyName>,..</PropertyName></Object>
    ///         可控输出格式: 时间、枚举
    ///     2019-04-18 实现解析XML为对象
    ///         支持简单类型：字符串、值类型
    ///         支持List\Dict: 
    ///         支持类类型：
    ///         注意：
    ///             现阶段仅支持属性标签方式（如<Person><Name>X</Name></Person>），不支持Attribute方式（如<Person Name="X"></Person>)
    ///     
    /// [任务]
    /// 输出
    ///     支持Attribute，定义该属性的序列化和解析方式
    ///         [XmlAttribute] : <Person Name="X"></Person>
    ///         [XmlArray]  或 [XmlSerilizer(typeof(XmlArraySerilizer))]
    ///         [XmlString(useCDATA)]
    ///         [XmlEnum(useInt)]
    ///         [XmlDateTime("yyyy-MM-dd")]
    ///         [XmlTable("Row")]
    ///     优化输出格式控制
    ///         构建一个 XmlDocument 对象，最后再根据格式参数再生成 xml 文本
    ///     测试
    ///         检测和避免无限循环引用
    /// 
    /// 解析
    ///     支持Attribute
    ///     
    /// </remarks>
    public class Xmlizer
    {
        //-------------------------------------------------
        // 属性
        //-------------------------------------------------
        /// <summary>是否采用LowCamel方式输出标签名称</summary>
        public bool FormatLowCamel { get; set; } = false;

        /// <summary>枚举格式化方式</summary>
        public EnumFomatting FormatEnum { get; set; } = EnumFomatting.Text;

        /// <summary>时间格式化方式</summary>
        public string FormatDateTime { get; set; } = "yyyy-MM-dd HH:mm:ss";

        /// <summary>是否插入渐进符</summary>
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

        /// <summary>获取Camel格式名称</summary>
        string GetCamelName(string name)
        {
            return (this.FormatLowCamel) ? name.ToLowCamel() : name;
        }

        /// <summary>获取标签名（根据类型自动命名）</summary>
        string GetTagName(Type type)
        {
            if (type.IsAnonymous())        return GetCamelName("Item");
            if (type.IsDict())             return GetCamelName("Dictionary");
            if (type.IsList())             return GetTagName(type.GetGenericDataType()) + "s";
            if (type.IsArray)              return GetTagName(type.GetElementType()) + "s";
            return GetCamelName(type.Name);
        }

        /// <summary>获取Xml安全文本（将特殊字符用CDATA解决）</summary>
        static string GetXmlSafeText(object obj)
        {
            // "<" 字符和"&"字符对于XML来说是严格禁止使用的，可用转义符或CDATA解决
            var txt = obj.ToString();
            if (txt.IndexOfAny(new char[] { '<', '&' }) != -1)
                return string.Format("<![CDATA[ {0} ]]>", txt);
            return txt;
        }


        //-------------------------------------------------
        // 对象转 XML
        //-------------------------------------------------
        #region 将对象序列化为XML
        /// <summary>将对象序列化为 XML</summary>
        /// <param name="rootName">根节点名称</param>
        /// <param name="ignoreNull">是否跳过空元素</param>
        /// <param name="addXmlHead">是否添加xml头部</param>
        public string ToXml(object o, string rootName="", bool ignoreNull=true, bool addXmlHead=false)
        {
            var sb = new StringBuilder();
            if (rootName.IsEmpty())
                rootName = GetTagName(o.GetType());
            if (addXmlHead)
                sb.AppendFormat("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n");
            WriteObject(sb, o, rootName, ignoreNull);
            return sb.ToString();
        }


        /// <summary>将对象序列化为 XML</summary>
        private void WriteObject(StringBuilder sb, object o, string tagName="", bool ignoreNull=true)
        {
            // 对象为空处理
            tagName = GetCamelName(tagName);
            if (o == null)
            {
                if (!ignoreNull && tagName.IsNotEmpty())
                    sb.AppendFormat("<{0}/>", tagName);
                return;
            }

            // 获取对象的类型
            var type = o.GetType().GetRealType();
            if (tagName.IsEmpty())
                tagName = GetTagName(type);

            // 根据类型进行输出（注意顺序不可颠倒）
            sb.AppendFormat("<{0}>", tagName);
            if (o is string)              WriteString(sb, o);
            else if (o is DateTime)       WriteDateTime(sb, o);
            else if (type.IsEnum)         WriteEnum(sb, o);
            else if (o is DataTable)      WriteDataTable(sb, o);
            else if (o is IDictionary)    WriteDict(sb, o);
            else if (o is IEnumerable)    WriteList(sb, o);
            else if (type.IsValueType)    WriteValue(sb, o);
            else                          WriteClass(sb, o);
            sb.AppendFormat("</{0}>", tagName);
        }

        /// <summary>输出字符串类型数据</summary>
        private static void WriteString(StringBuilder sb, object obj)
        {
            sb.Append(GetXmlSafeText(obj));
        }

        /// <summary>输出枚举类型数据</summary>
        private void WriteEnum(StringBuilder sb, object obj)
        {
            if (this.FormatEnum == EnumFomatting.Int) sb.AppendFormat("{0:d}", obj);
            else sb.AppendFormat("{0}", obj);
        }

        /// <summary>输出时间类型数据</summary>
        private void WriteDateTime(StringBuilder sb, object obj)
        {
            var dt = Convert.ToDateTime(obj);
            if (dt != new DateTime())
                sb.AppendFormat(dt.ToString(this.FormatDateTime));
        }

        /// <summary>输出值类型数据</summary>
        private static void WriteValue(StringBuilder sb, object obj)
        {
            sb.AppendFormat("{0}", obj);
        }

        /// <summary>输出列表类型数据</summary>
        private void WriteList(StringBuilder sb, object obj)
        {
            foreach (var item in (obj as IEnumerable))
                WriteObject(sb, item, "");
        }

        /// <summary>输出字典类型数据</summary>
        private void WriteDict(StringBuilder sb, object obj)
        {
            var dict = (obj as IDictionary);
            foreach (var key in dict.Keys)
            {
                sb.AppendFormat("<Item Key=\"{0}\">", key);
                WriteObject(sb, dict[key], "");
                sb.AppendFormat("</Item>");
            }
        }

        /// <summary>输出数据表类型数据</summary>
        private void WriteDataTable(StringBuilder sb, object obj)
        {
            var table = obj as DataTable;
            var cols = table.Columns;
            foreach (DataRow row in table.Rows)
            {
                sb.AppendFormat("<Row>");
                foreach (DataColumn col in cols)
                {
                    var columnName = col.ColumnName;
                    WriteObject(sb, row[columnName], columnName);
                }
                sb.AppendFormat("</Row>");
            }
        }

        /// <summary>输出类类型数据</summary>
        private void WriteClass(StringBuilder sb, object obj)
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var property in properties)
            {
                // 跳过加忽略标签的节点
                if (ReflectionHelper.GetAttribute<NonSerializedAttribute>(property) != null
                    || ReflectionHelper.GetAttribute<JsonIgnoreAttribute>(property) != null
                    || ReflectionHelper.GetAttribute<System.Xml.Serialization.XmlIgnoreAttribute>(property) != null
                    )
                    continue;

                var subObj = property.GetValue(obj);
                WriteObject(sb, subObj, property.Name);
            }
        }
        #endregion


        //-------------------------------------------------
        // XML转对象
        //-------------------------------------------------
        #region 将Xml解析为对象
        /// <summary>解析 XML 字符串为对象（请自行捕捉解析异常）</summary>
        public  T Parse<T>(string xml) where T : class
        {
            return Parse(xml, typeof(T)) as T;
        }

        /// <summary>解析 XML 字符串为对象（请自行捕捉解析异常）</summary>
        public object Parse(string xml, Type type)
        {
            // 简单值类型直接解析
            var tag = SerializationNode.FromType(type);
            if (tag.Type == SerializationType.Simple)
                return xml.Parse(type);

            // 复杂类型再解析ML
            var doc = new XmlDocument();
            doc.LoadXml(xml);
            return ParseNode(doc.DocumentElement, type);
        }

        /// <summary>将XML节点解析为指定类型的对象</summary>
        object ParseNode(XmlNode node, Type type)
        {
            var tag = SerializationNode.FromType(type);
            if (tag.Type == SerializationType.Simple) return ParseNodeToValue(node, type);
            if (tag.Type == SerializationType.List)   return ParseNodeToList(node, type);
            if (tag.Type == SerializationType.Array)  return ParseNodeToArray(node, type);
            if (tag.Type == SerializationType.Dict)   return ParseNodeToDict(node, type);
            return ParseNodeToObject(node, type);
        }

        /// <summary>将XML节点解析为简单值对象</summary>
        object ParseNodeToValue(XmlNode node, Type type)
        {
            var text = node.InnerText;
            return text.Parse(type);
        }

        /// <summary>将xml解析为对象</summary>
        object ParseNodeToObject(XmlNode node, Type type)
        {
            if (node.IsEmpty()) return null;
            var o = Activator.CreateInstance(type);
            foreach (var p in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!p.CanWrite) continue;
                XmlNode cnode = node.SelectSingleNode(p.Name);
                if (cnode == null)
                    continue;

                var value = ParseNode(cnode, p.PropertyType);
                p.SetValue(o, value, null);
            }
            return o;
        }

        /// <summary>将xml解析为集合</summary>
        private IList ParseNodeToList(XmlNode node, Type type)
        {
            var tag = SerializationNode.FromType(type);
            var list = Activator.CreateInstance(type) as IList;
            var nodes = node.SelectNodes(tag.ItemType.Name);
            foreach (XmlNode subNode in nodes)
            {
                var item = ParseNode(subNode, tag.ItemType);
                list.Add(item);
            }
            return list;
        }

        /// <summary>将xml解析为数组</summary>
        private object ParseNodeToArray(XmlNode node, Type type)
        {
            var tag = SerializationNode.FromType(type);
            var nodes = node.SelectNodes(tag.ItemType.Name);
            Array array = Array.CreateInstance(type, nodes.Count);
            var collection = Convert.ChangeType(array, type);
            int index = 0;
            foreach (XmlNode subNode in nodes)
            {
                var item = ParseNode(subNode, tag.ItemType);
                SetItemValue(collection, item, index++);
            }
            return collection;
        }

        /// <summary>将xml解析为字典</summary>
        /// <remarks>
        ///     <Persons>
        ///         <Persion Key="Kevin">content</Person>
        ///         <Persion Key="Willion">content</Person>
        ///     </Persons>
        /// </remarks>
        private object ParseNodeToDict(XmlNode node, Type type)
        {
            var tag = SerializationNode.FromType(type);
            var dict = Activator.CreateInstance(type) as IDictionary;
            var nodes = node.SelectNodes("Item");
            foreach (XmlNode subNode in nodes)
            {
                var key = subNode.Attributes["Key"]?.Value;
                var valueNode = subNode.FirstChild;
                var item = ParseNode(valueNode, tag.ItemType);
                dict.Add(key, item);
            }
            return dict;
        }



        /// <summary>设置集合某个元素的值</summary>
        private void SetItemValue<T>(T collection, object obj, int index)
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
        #endregion


        //-------------------------------------------
        // 用正则表达式解析Xml标签（仅供参考本类并没有用到）
        //-------------------------------------------
        /// <summary>获取标签列表</summary>
        /// <param name="tagOrContent">获取整个标签还是内容部分</param>
        /// <remarks>有问题：应该获取直接下属标签，而不必返回子级标签；否则若顺序错乱一下，就会取错标签了；</remarks>
        public static List<string> GetTags(string content, string tagName, bool tagOrContent)
        {
            var values = new List<string>();
            if (content.IsNotEmpty())
            {
                var tagRegex = string.Format(@"<{0}[^>]*>([\s\S]*?)</\s*{0}>", tagName);
                var matches = Regex.Matches(content, tagRegex, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    var value = tagOrContent ? match.Value : GetCDATAValue(match.Groups[1].Value);
                    values.Add(value);
                }
            }
            return values;
        }

        /// <summary>获取标签</summary>
        public static string GetTag(string content, string tagName, bool tagOrContent)
        {
            var tags = GetTags(content, tagName, tagOrContent);
            return tags.Count > 0 ? tags[0] : null;
        }

        /// <summary>获取指定特性的值（如 Name="Kevin"）</summary>
        public static string GetTagAttribute(string content, string tagName, string attributeName)
        {
            var tagRegex = string.Format(@"<{0}.*{1}\s*=\s*['""]([^'""]*)['""][^>]*>", tagName, attributeName);
            var match = Regex.Match(content, tagRegex, RegexOptions.IgnoreCase);
            if (match.Success)
                return match.Groups[1].Value;
            return null;
        }

        /// <summary>解析CDATA内容（以<![CDATA[开头)</summary>
        private static string GetCDATAValue(string txt)
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
