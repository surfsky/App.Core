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

    }
}