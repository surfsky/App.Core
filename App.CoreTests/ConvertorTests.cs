using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace App.CoreTests
{
    [TestClass()]
    public class ConvertorTests
    {
        // 调整属性顺序再测试
        string xml = @"
            <Person>
                <Name>Kevin</Name>
                <About><![CDATA[Kevin'Brother]]></About>
                <Age>21</Age>
                <Birthday>2001-01-01</Birthday>
                <Sex></Sex>
                <Brother>
                    <Name>Bob</Name>
                </Brother>
                <Parents>
                    <Person>
                        <Name>Monther</Name>
                    </Person>
                    <Person>
                        <Name>Father</Name>
                    </Person>
                </Parents>
            </Person>
            ";

        [TestMethod()]
        public void ParseXmlTest()
        {
            var p = Person.Demo();
            var x = p.ToXml("Person");
            Trace.Write(x);

            var o1 = x.ParseXml<Person>();
            Trace.Write(o1.ToJson());

            var o2 = xml.ParseXml<Person>();
            Trace.Write(xml);
            Trace.Write(o2.ToJson());
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
        public void ToDynamicTest()
        {
            var json = "{name:'Kevin', age:21}";
            var o = json.ParseDynamic();
            string name = o.name;
            int age = o.age;
        }

        [TestMethod()]
        public void ToJObjectTest()
        {
            var json = "{name:'Kevin', age:21}";
            var o = json.ParseJObject();
            string name = o["name"].ToText();
            int? age = o["age"].ToInt();
        }

        [TestMethod()]
        public void ToXmlTest()
        {
            var o = new
            {
                ToUserName = "ToUserName",
                FromUserName = "FromUserName",
                CreateTime = DateTime.Now.ToTimeStamp(),
                MsgType = "image",
                Image = new
                {
                    MediaId = "123"
                }
            };
            var xml = o.ToXml("xml");

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
            txt = dict.ToQueryString();
        }
    }
}