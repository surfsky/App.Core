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
    public enum SexType : int
    {
        [UI("正常",    "男")] Male = 0,
        [UI("正常",    "女")] Female = 1,
        [UI("不正常",  "人妖")] RenYao = 2
    }

    /// <summary>
    /// 人员测试类
    /// </summary>
    public class Person
    {
        [UI("基础", "Name")]  public string Name { get; set; }
        [UI("基础", "年龄")]  public int? Age { get; set; }
        [UI("基础", "三日")]  public DateTime? Birthday { get; set; }
        [UI("基础", "性别")]  public SexType? Sex { get; set; }
        [UI("基础", "关于")]  public string About { get; set; }
        [UI("亲友", "兄弟")]  public Person Brother { get; set; }
        [UI("亲友", "父母")]  public List<Person> Parents { get; set; }
        [UI("亲友", "朋友")]  public Dictionary<string, Person> Friends { get; set; }
        [UI("其它", "关注")]  public List<string> Favorites { get; set; }
        [UI("其它", "分数")]  public Dictionary<string, float> Scores { get; set; }

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
