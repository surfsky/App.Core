using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Tests
{
    public enum ProductType : int 
    {
        Goods = 0,
        Service = 1,
        Repair = 2
    }

    [TestClass()]
    public class ExtensionsTests
    {
        [TestMethod()]
        public void ToEnumTest()
        {
            object o;
            ProductType type = ProductType.Repair;
            o = type.GetDescription();
            o = "Goods".ParseEnum<ProductType>();
            o = "1".ParseEnum<ProductType>();
        }
    }
}