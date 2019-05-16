using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Tests
{
    /// <summary>
    /// 性别枚举
    /// </summary>
    public enum SexType
    {
        Male,
        Female
    }

    /// <summary>
    /// 人员测试类
    /// </summary>
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime? Birthday { get; set; }
        public SexType? Sex { get; set; }
        public string About { get; set; }
        public Person Brother { get; set; }
        public List<Person> Parents { get; set; }
        public List<string> Favorites { get; set; }
        public Dictionary<string, Person> Friends { get; set; }
        public Dictionary<string, float> Scores { get; set; }

        public Person() { }
        public Person(string name) { this.Name = name; }

        public static Person Demo()
        {
            var p = new Person();
            p.Name = "Kevin";
            p.Age = 21;
            p.Birthday = DateTime.Now.AddYears(-21);
            p.Sex = SexType.Male;
            p.About = "<This is me>";
            p.Brother = new Person() { Name = "Kevin's brother" };
            p.Favorites = new List<string>() { "Art", "Computer" };
            p.Parents = new List<Person>() { new Person("Monther"), new Person("Father") };
            p.Scores = new Dictionary<string, float>()
            {
                {"Math", 99},
                {"English", 100 }
            };
            p.Friends = new Dictionary<string, Person>()
            {
                {"GirlFriend", new Person("Cherry")},
                {"BoyFriend", new Person("Bob") }
            };
            return p;
        }
    }

}
