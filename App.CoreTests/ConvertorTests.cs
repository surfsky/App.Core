using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace App.Core.Tests
{
    public enum ProductType : int
    {
        Goods = 0,
        Service = 1,
        Repair = 2
    }


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
        public void ParseEnumTest()
        {
            Assert.AreEqual("Male".ParseEnum<SexType>(), SexType.Male);
            Assert.AreEqual("0".ParseEnum<SexType>(), SexType.Male);
            Assert.AreEqual("Male,Female".ParseEnums<SexType>().Count, 2);
            Assert.AreEqual("0,1".ParseEnums<SexType>().Count, 2);
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

        [TestMethod()]
        public void ToXmlTest()
        {
            // dict
            var dict = new Dictionary<string, string>();
            dict.Add("appId", "-----appid----");
            dict.Add("appSecret", "----<appsecret----");
            var xml = dict.ToXml();
            var json = dict.ToJson();
            xml = dict.ToXml();

            // object
            var o = new
            {
                ToUserName = "&ToUserName",
                FromUserName = "<FromUserName",
                CreateTime = DateTime.Now.ToTimeStamp(),
                MsgType = "image",
                Image = new
                {
                    MediaId = "123"
                }
            };
            xml = o.ToXml("xml");
        }

        [TestMethod()]
        public void ParseXmlTest()
        {
            var p = Person.GetPerson();
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
        public void ParseTest()
        {
            Assert.AreEqual("1".Parse<string>(), "1");
            Assert.AreEqual("1".Parse<int>(), 1);
            Assert.AreEqual("1".Parse<bool?>(), null);

            var s = new Person("Jack").ToJson();
            var p = s.Parse<Person>();
            Assert.AreEqual(p.Name, "Jack");
        }


        [TestMethod()]
        public void ToTest()
        {
            Assert.AreEqual(1.To<string>(), "1");
            Assert.AreEqual(1.To<int>(), 1);
            Assert.AreEqual("true".To<bool>(), true);
            Assert.AreEqual("true".To<bool?>(), true);
            Assert.AreEqual("2010-01-01".To<DateTime>().Year, 2010);
            Assert.AreEqual(SexType.Male.To<int>(), (int)SexType.Male);
            Assert.AreEqual("Male".To<SexType>(), SexType.Male);
            Assert.AreEqual(0.To<SexType>(), SexType.Male);

            Assert.AreEqual(new Person("Jack").To<Person>().Name, "Jack");
            Assert.AreEqual(new Giant("Jack").To<Person>().Name, "Giant Jack");
            Assert.AreEqual(new Person("Jack").To<Giant>().Name, "Giant Jack");
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
            Assert.AreEqual("true".ParseBool(), true);
            Assert.AreEqual("false".ParseBool(), false);
            Assert.AreEqual("True".ParseBool(), true);
            Assert.AreEqual("False".ParseBool(), false);
            //
            string t = null;
            Assert.AreEqual("1".ParseBool(), null);
            Assert.AreEqual("0".ParseBool(), null);
            Assert.AreEqual("Yes".ParseBool(), null);
            Assert.AreEqual("No".ParseBool(), null);
            Assert.AreEqual("".ParseBool(), null);
            Assert.AreEqual(t.ParseBool(), null);
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

        [TestMethod()]
        public void EachTest()
        {
            var items = new List<Person>();
            items.Add(new Person() { Age = 1 });
            items.Add(new Person() { Age = 2 });
            items.Add(new Person() { Age = 3 });
            items.Each(t => t.Age = t.Age + 10);
            Assert.AreEqual(items[0].Age, 11);
        }

        [TestMethod()]
        public void Each2Test()
        {
            var items = new List<Person>();
            items.Add(new Person() { Age = 1 });
            items.Add(new Person() { Age = 2 });
            items.Add(new Person() { Age = 3 });
            items.Each2((item, preItem) =>
            {
                int n = preItem?.Age ?? 0;
                item.Age = item.Age + n;
            });
            Assert.AreEqual(items[1].Age, 3);
        }

        [TestMethod()]
        public void MergeTest()
        {
            var p1 = new Person("1");
            var p2 = new Person("2");
            var p3 = new Person("3");
            var p4 = new Person("4");
            var p5 = new Person("5");
            var p6 = new Person("6");

            var list1 = new List<Person>() { p1, p2, p3 };
            var list2 = new List<Person>() { p1, p4, p5 };
            var list3 = list1.Union(list2);
            Assert.AreEqual(list3.Count, 5);
        }
    }
}