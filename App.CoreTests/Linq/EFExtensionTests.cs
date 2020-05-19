using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Utils.Tests
{
    [TestClass()]
    public class EFExtensionTests
    {
        [TestMethod()]
        public void SortTest()
        {
            var persons = Person.GetPersons();
            var page0 = persons.AsQueryable().SortPage(t => t.Age, true, 0, 3).ToList();
            var page1 = persons.AsQueryable().SortPage(t => t.Age, true, 1, 3).ToList();
            var page2 = persons.AsQueryable().SortPage(t => t.Age, true, 2, 3).ToList();
            var page3 = persons.AsQueryable().SortPage(t => t.Age, true, 3, 3).ToList();
            var page4 = persons.AsQueryable().SortPage(t => t.Age, true, 4, 3).ToList();
            Assert.AreEqual(page0.Count, 3);
            Assert.AreEqual(page1.Count, 3);
            Assert.AreEqual(page2.Count, 3);
            Assert.AreEqual(page3.Count, 1);
            Assert.AreEqual(page4.Count, 0);
        }
    }
}