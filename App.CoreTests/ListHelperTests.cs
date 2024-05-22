using Microsoft.VisualStudio.TestTools.UnitTesting;
using App.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Tests
{
    [TestClass()]
    public class ListHelperTests
    {
        //----------------------------------------------
        // list -- dict
        //----------------------------------------------
        [TestMethod()]
        public void GetItemTest()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>
            {
                { "a", "va"  },
                { "b", "vb"  },
                { "c", "vc"  },
            };
            Assert.AreEqual(dict.GetItem("A", true), "va");
            Assert.AreEqual(dict.GetItem("A", false), null);
        }



        //----------------------------------------------
        // list
        //----------------------------------------------
        List<int> _list1 = new List<int>() { 0, 1, 2, 3, 4, 5 };
        List<int> _list2 = new List<int>() { 6, 7, 8, 9 };

        [TestMethod()]
        public void IndexOfTest()
        {
            Assert.AreEqual(_list1.IndexOf(t => t == 2), 2);
        }

        [TestMethod()]
        public void UnionTest()
        {
            Assert.AreEqual(_list1.Union(_list2).Count, _list1.Count + _list2.Count);
        }



        [TestMethod()]
        public void MoveItemTest()
        {
            var list = _list1.Clone();
            int n = list.MoveItem(2, 4);
            Assert.AreEqual(list[n], 2);

        }

        [TestMethod()]
        public void MoveItemHeadTest()
        {
            var list = _list1.Clone();
            var n = list.MoveItemHead(2);
            Assert.AreEqual(n, 0);
        }

        [TestMethod()]
        public void MoveItemTailTest()
        {
            var list = _list1.Clone();
            var n = list.MoveItemTail(2);
            Assert.AreEqual(n, list.Count-1);
        }

        [TestMethod()]
        public void MoveItemUpTest()
        {
            var list = _list1.Clone();
            int n = list.MoveItemUp(2);
            Assert.AreEqual(list[n], 2);
        }

        [TestMethod()]
        public void MoveItemDownTest()
        {
            var list = _list1.Clone();
            int n = list.MoveItemDown(2);
            Assert.AreEqual(list[n], 2);
        }

        
    }
}