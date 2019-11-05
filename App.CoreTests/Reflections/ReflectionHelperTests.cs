﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace App.Core.Tests
{
    [TestClass()]
    public class ReflectionHelperTests
    {
        [TestMethod()]
        public void GetPropertyNameTest()
        {
            //var name = ReflectionHelper.GetPropertyName<Person>(t => t.Name);
            var p = new Person() { Name = "kevin", Sex = SexType.Male };
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
            var m = typeof(Person).GetMethod(nameof(Person.Cry));
            var t = m.GetMethodString();
            Assert.AreEqual(t, "Void Cry(String msg, Int32 times)");
        }
    }
}