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
            var p = new Person() { Sex = SexType.Male };
            var sex = SexType.Female;

            object o;
            o = p.GetDescription();
            o = p.GetDescription(t => t.Name);
            o = sex.GetDescription();
            o = sex.GetUIGroup();
            o = sex.GetUIAttribute();
            o = UIExtension.GetDescription<Person>(t => t.Name);
            o = typeof(Person).GetProperty(nameof(Person.Name)).GetDescription();
        }
    }
}