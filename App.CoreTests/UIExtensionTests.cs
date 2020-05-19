using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Utils;


namespace App.Utils.Tests
{
    [TestClass()]
    public class UIExtensionTests
    {
        [TestMethod()]
        public void GetTitleTest()
        {
            var p = new Person() { Sex = SexType.Male };
            var sex = SexType.Female;
            var type = p.GetType();

            object o;
            o = type.GetTitle();
            o = type.GetTitle(t => t.Name);
            o = p.GetTitle(t => t.Name);
            o = type.GetProperty(nameof(Person.Name)).GetTitle();
            o = sex.GetTitle();
            o = sex.GetUIGroup();
            o = sex.GetUIAttribute();
        }
    }
}