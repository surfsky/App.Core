using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Core;


namespace App.Core.Tests
{
    [TestClass()]
    public class UIExtensionTests
    {
        [TestMethod()]
        public void GetDescriptionTest()
        {
            object o;
            var sex = SexType.Female;
            var p = new Person();

            o = sex.GetDescription();
            o = sex.GetUIGroup();
            o = sex.GetUIAttribute();
            o = p.GetDescription(t => t.Name);
            o = UIExtension.GetDescription<Person>(t => t.Name);
        }
    }
}