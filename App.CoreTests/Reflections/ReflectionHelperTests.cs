using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            //var name = ReflectionHelper.GetPropertyName<DAL.UserAsset>(t => t.CreateOrderItem.Order);
            //var order = new DAL.Order() { ID = 1, Summary = "OrderSummary" };
            //var orderItem = new DAL.OrderItem() { Order = order, Title="OrderItemTitle" };
            //var asset = new DAL.UserAsset() { CreateOrderItem = orderItem };
            //var o = asset.GetPropertyValue(t => t.CreateOrderItem.Order.Summary);
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
            //return ReflectionHelper.GetMethodName<Person>(t => t.Cry);
        }
    }
}