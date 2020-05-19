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
    public class DynamicXmlTests
    {
        [TestMethod()]
        public void DynamicXmlTest()
        {
            var xml = @"<root>
                <books>
                    <book is_read=""false""><author>Test</author></book>
                    <book is_read=""true""><author>Test2</author></book>
                </books>
            </root>";

            dynamic data = new DynamicXml(xml);
            Console.WriteLine(data.books.book[0].author);
        }


    }
}