using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.Utils
{
    /// <summary>
    /// 正则表达式验证标注
    /// </summary>
    public class RegexValidationAttribute : ValidationAttribute
    {
        private string _pattern;
        public RegexValidationAttribute(string pattern = @"\d+", string errorMessage = "数据格式不正确")
        {
            _pattern = pattern;
            this.ErrorMessage = errorMessage;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //validationContext.DisplayName:指定某个实体类的属性
            //获取指定属性的相关属性信息
            //获取对应属性的值
            var t = validationContext.ObjectType.GetProperty(validationContext.DisplayName);
            var p = t.GetValue(validationContext.ObjectInstance, null);

            //做正则做匹配
            if (Regex.IsMatch(p.ToString().ToUpper(), _pattern))
                return ValidationResult.Success;
            return new ValidationResult(this.ErrorMessage);
        }

    }
}
