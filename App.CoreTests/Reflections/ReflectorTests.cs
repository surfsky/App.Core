using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.ComponentModel;

namespace App.Utils.Tests
{
    [TestClass()]
    public class ReflectorTests
    {
        [TestMethod()]
        public void GetPropertyNameTest()
        {
            //var name = ReflectionHelper.GetPropertyName<Person>(t => t.Name);
            var p = new Person() { Name = "kevin", Sex = SexType.Male };
            var n = p.GetValue(t => t.Name);
            Assert.AreEqual(n, "kevin");
        }

        [TestMethod()]
        public void GetCurrentMethodTest()
        {
            var method = Reflector.GetCurrentMethod();
            Assert.AreEqual(method.Name, "GetCurrentMethodTest");
        }


        [TestMethod()]
        public void GetCurrentTypeTest()
        {
            var type = Reflector.GetCurrentType();
            Assert.AreEqual(type, this.GetType());
        }

        [TestMethod()]
        public void GetEventDelegatesTest()
        {
            var person = new Person("Kevin");
            person.Cry += (t) => { IO.Trace(t); };
            var subscribers = Reflector.GetEventSubscribers(person, nameof(Person.Cry));
            foreach (var t in subscribers)
                IO.Trace("sender={0}, method={1}", t.Target.GetType().FullName, t.Method.Name);
        }

        [TestMethod()]
        public void GetMethodNameTest()
        {
            var person = new Person("Kevin");
            //ReflectionHelper.GetMemberInfo<Person>(t => t.Cry);
        }

        [TestMethod()]
        public void GetTypeStringTest()
        {
            var t1 = typeof(List<string>).GetTypeString();
            var t2 = typeof(Dictionary<string, int>).GetTypeString();
            var t3 = typeof(string).GetTypeString();
            var t4 = typeof(int).GetTypeString();
            var t5 = typeof(bool?).GetTypeString();
            var t6 = typeof(A<string, int>).GetTypeString();

            Assert.AreEqual(t1, "List<String>");
            Assert.AreEqual(t2, "Dictionary<String, Int32>");
            Assert.AreEqual(t3, "String");
            Assert.AreEqual(t4, "Int32");
            Assert.AreEqual(t5, "Boolean?");
            Assert.AreEqual(t6, "A<String, Int32>");
        }

        class A<T1, T2> { }

        [TestMethod()]
        public void GetMethodStringTest()
        {
            var m = typeof(Person).GetMethod(nameof(Person.Speek));
            var t = m.GetMethodString();
            Assert.AreEqual(t, "Void Speek(String msg, Int32 times)");
        }

        [TestMethod()]
        public void GetMethodsTest()
        {
            var methods = typeof(Giant).GetMethods("ToString");
            Assert.AreEqual(methods[0].ReflectedType, typeof(Giant));
            Assert.AreEqual(methods[1].ReflectedType, typeof(Person));
            Assert.AreEqual(methods[2].ReflectedType, typeof(Object));

            var ms = typeof(Giant).GetMethods("ToString", false);
            Assert.AreEqual(ms.Count, 1);
        }

        [UI("title1")]
        [T("title2")]
        [Param("param1", "param1")]
        [Param("param2", "param2")]
        [TestMethod()]
        public void GetAttributesTest()
        {
            var m = Reflector.GetCurrentMethod();
            Assert.AreEqual(m.GetAttributes<ParamAttribute>().Count, 2);
            Assert.AreEqual(m.GetAttributes<UIAttribute>().Count, 1);
            Assert.AreEqual(m.GetAttributes<TAttribute>().Count, 1);
        }

        [Param("param1", "param1")]
        [Param("param2", "param2")]
        [TestMethod()]
        public void GetAttributeTest()
        {
            var m = Reflector.GetCurrentMethod();
            Assert.AreEqual(m.GetAttribute<ParamAttribute>().Name, "param1");
        }

        [TestMethod()]
        public void SetPropertyValueTest()
        {
            var p = new Person("Father");
            //p.Languages = new string[] { "En", "Cn", "Fr" };
            var lang1 = new string[] { "En", "Cn", "Fr", "Gr" };
            Reflector.SetValue(p, "Languages", lang1);
            var lang2 = Reflector.GetValue(p, "Languages") as string[];
            Assert.AreEqual(lang1.Length, lang2.Length);
        }

        [TestMethod()]
        public void SetDisplayNameTest()
        {
            var p = new Person("Father");
            p.SetDisplayName("Name", "姓名");
            var displayName = p.GetDisplayName("Name");
            //var attr = p.GetProperty("Name").GetAttribute<DisplayNameAttribute>();
            Assert.AreEqual(displayName, "姓名");
        }

        [TestMethod()]
        public void GetShortNameTest()
        {
            Type t1 = typeof(int);                     // System.Int32
            Type t2 = typeof(int?);                    // System.Nullable`1[[System.Int32]]
            Type t3 = typeof(List<int>);               // System.Collections.Generic.List`1[[System.Int32]]
            Type t4 = typeof(List<SexType>);           // System.Collections.Generic.List`1[[App.Core.Tests.SexType, App.CoreTest]]
            Type t5 = typeof(Dictionary<string, int>); // System.Collections.Generic.Dictionary`2[[System.String],[System.Int32]]
            Assert.AreEqual(Type.GetType(t1.GetShortName()), t1);
            Assert.AreEqual(Type.GetType(t2.GetShortName()), t2);
            Assert.AreEqual(Type.GetType(t3.GetShortName()), t3);
            Assert.AreEqual(Type.GetType(t4.GetShortName()), t4);
            Assert.AreEqual(Type.GetType(t5.GetShortName()), t5);
        }

        [TestMethod()]
        public void IsCollectionTest()
        {
            Assert.AreEqual(typeof(List<User>).IsCollection(), true);
            Assert.AreEqual(typeof(Dictionary<long, User>).IsCollection(), true);
            Assert.AreEqual(typeof(ICollection<User>).IsCollection(), true);
        }

        [TestMethod()]
        public void IsInterfaceTest()
        {
            Assert.AreEqual(typeof(Person).IsInterface(typeof(ISpeek)), true);
            Assert.AreEqual(typeof(Giant).IsType(typeof(Person)), true);
        }

        [TestMethod()]
        public void GetNameTest()
        {
            //var exp = (Person p) => p.Name;
            Expression<Func<Person, object>> exp1 = t => t.Name;
            Assert.AreEqual(exp1.GetName(), "Name");
            Assert.AreEqual(Reflector.GetName<Person>(t=> t.Name), "Name");
            Assert.AreEqual(Reflector.GetName<Person>(t => t.Father.Name), "Father.Name");
        }

        [TestMethod()]
        public void GetTitleTest()
        {
            Assert.AreEqual(UIExtension.GetTitle<Person>(t => t.Name), "姓名");
            Assert.AreEqual(UIExtension.GetTitle<Person>(t => t.Father.Name), "父亲姓名");
        }

        [TestMethod()]
        public void GetPropertyTest()
        {
            Assert.AreEqual(Reflector.GetProperty<Person>(t => t.Name).Name, "Name");
            Assert.AreEqual(Reflector.GetProperty<Person>(t => t.Father.Name).Name, "Name");
        }
    }
}