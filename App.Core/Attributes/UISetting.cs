using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;

namespace App.Utils
{
    //========================================================
    // UISetting
    //========================================================
    /// <summary>
    /// UI 设置。可根据该类动态设置用户界面（如网格、表单等）
    /// </summary>
    [XmlInclude(typeof(UIAttribute))]
    public class UISetting
    {
        /// <summary>标题</summary>
        public string Title { get; set; }

        /// <summary>数据模型类型</summary>
        public Type EntityType { get; set; }

        /// <summary>关联的方法</summary>
        public MethodInfo Method { get; set; }

        /// <summary>成员</summary>
        public List<UIAttribute> Items { get; set; } = new List<UIAttribute>();

        /// <summary>分组</summary>
        [JsonIgnore]
        public Dictionary<string, List<UIAttribute>> Groups { get; set; }

        //---------------------------------------
        // 构造函数
        //---------------------------------------
        public UISetting(){}
        public UISetting(Type type, string title = "")
        {
            this.Title = title.IsEmpty() ? type.GetTitle() : title;
            this.EntityType = type;
            this.Items = type.GetUIAttributes();
            BuildGroups();
        }

        /// <summary>构建组结构</summary>
        public UISetting BuildGroups()
        {
            this.Groups = new Dictionary<string, List<UIAttribute>>();
            foreach (var item in Items)
            {
                if (item.Group == null)
                    item.Group = "";
                if (!Groups.Keys.Contains(item.Group))
                    Groups.Add(item.Group, new List<UIAttribute>());
                Groups[item.Group].Add(item);
            }
            return this;
        }

        /// <summary>
        /// 从方法构建 UISetting 对象。该方法返回类型必须为IQueryable&lt;Org&gt;
        /// （如果没有ParamAttribute则显示所有参数；如果有，则只保留标注过的参数）
        /// </summary>
        public UISetting(MethodInfo m)
        {
            if (m == null)
                return;
            if (m.ReturnType.Name != "IQueryable`1")
                throw new Exception("方法返回类型必须是 IQueryable<T>");

            this.Method = m;
            this.EntityType = m.ReturnType.GenericTypeArguments[0];
            this.Title = $"{m.DeclaringType.Name}.{m.Name}";
            var attrs = m.GetAttributes<ParamAttribute>();
            var ps = m.GetParameters();
            foreach (var p in ps)
            {
                var ui = new UIAttribute(p.Name);
                ui.Name = p.Name;
                ui.Type = p.ParameterType;
                ui.Required = false;  // 搜索栏上的所有控件都不是必填的

                // 如果没有任何 [Param] 标签，则全部启用
                if (attrs.Count == 0)
                {
                    this.Items.Add(ui);
                    continue;
                }

                // 如果找到匹配的 [Param] 标签，则使用其配置，否则设置该参数为隐藏
                var attr = attrs.FirstOrDefault(t => t.Name == p.Name);
                if (attr == null)
                {
                    if (p.IsOptional)
                        continue;
                    ui.Mode = PageMode.None;
                }
                else
                {
                    if (attr.Type != null)
                        ui.Type = attr.Type;
                    ui.ValueType = attr.ValueType;
                    ui.ValueField = attr.ValueField;
                    ui.Text = attr.Text;
                    ui.TextField = attr.TextField;
                    ui.Tag = attr.Tag;
                    ui.Title = attr.Title;
                    ui.Remark = attr.Remark;
                    ui.Regex = attr.Regex;
                    ui.Precision = attr.Precision;
                    ui.Width = attr.Width;
                    ui.Height = attr.Height;
                    ui.QueryString = attr.QueryString;
                }
                this.Items.Add(ui);
            }
        }
    }

    /// <summary>UI配置信息（泛型版本）</summary>
    public class UISetting<T> : UISetting
    {
        public UISetting(bool build)
        { 
            this.EntityType = typeof(T);
            this.Title = this.EntityType.GetTitle();
            if (build)
                BuildItems();
        }

        /// <summary>根据类型构建Items</summary>
        void BuildItems()
        {
            this.Items = typeof(T).GetUIAttributes();
            // 如果该类有ID字段，放到第一位
            if (typeof(T).IsInterface(typeof(IID)))
            {
                var idItem = Items.FirstOrDefault(t => t.Name == "ID");
                if (idItem != null)
                {
                    Items.Remove(idItem);
                    Items.Insert(0, idItem);
                }
            }
        }

        /// <summary>获取成员</summary>
        public UIAttribute Get(Expression<Func<T, object>> field)
        {
            return this.Items.FirstOrDefault(t => t.Name == field.GetName());
        }
        public UIAttribute GetOrCreate(Expression<Func<T, object>> field)
        {
            var attr = Get(field);
            if (attr == null)
            {
                var p = field.GetProperty();
                attr = p.GetPropertyUI();
                var name = field.GetName();
                if (attr.Name != name)
                    attr.Name = name;               // t.Dept.Name => "Dept.Name"
                this.Items.Add(attr);
            }
            attr.Title = field.GetTitle();  // 部门名称

            return attr;
        }


        /// <summary>删除成员</summary>
        public void Remove(Expression<Func<T, object>> field)
        {
            var item = Get(field);
            if (item != null)
                this.Items.Remove(item);
        }

        /// <summary>设置显示模式（何种页面模式下才显示）</summary>
        public UIAttribute SetMode(Expression<Func<T, object>> field, PageMode mode)
        {
            return GetOrCreate(field).SetMode(mode);
        }


        //---------------------------------------------------
        // 网格列设置
        //---------------------------------------------------
        /// <summary>设置列成员</summary>
        public UIAttribute SetColumn(Expression<Func<T, object>> field, int? width = null, ColumnType column = ColumnType.Auto, string title = "", bool? sort = null, object tag = null)
        {
            return GetOrCreate(field).SetColumn(column, width, title, sort, tag);
        }


        /// <summary>添加弹出网格成员</summary>
        public UIAttribute SetColumnWin(Expression<Func<T, object>> field, string text, Expression<Func<T, object>> textField, string urlTemplate, int? width, string title, bool? sort = null, Size? winSize = null)
        {
            var attr = SetColumn(field, width, ColumnType.Win, title, sort, null);
            attr.Text = text;
            attr.TextField = textField.GetName();
            attr.UrlTemplate = urlTemplate;
            attr.WinSize = winSize ?? new Size(1000, 800);
            return attr;
        }

        /// <summary>添加弹出表单成员（AuthForm)</summary>
        public UIAttribute SetColumnWinForm(Expression<Func<T, object>> field, string text, Expression<Func<T, object>> textField, Type valueType, int? width, string title, bool? sort = null, Size? winSize = null)
        {
            var attr = SetColumn(field, width, ColumnType.WinForm, title, sort, null);
            attr.Text = text;
            attr.TextField = textField.GetName();
            attr.ValueType = valueType;
            attr.WinSize = winSize ?? new Size(1000, 800);
            return attr;
        }

        /// <summary>添加弹出网格成员（AutoGrid）</summary>
        public UIAttribute SetColumnWinGrid(Expression<Func<T, object>> field, string text, Expression<Func<T, object>> textField, Type valueType, int? width, string title, bool? sort = null, Size? winSize = null)
        {
            var attr = SetColumn(field, width, ColumnType.WinGrid, title, sort, null);
            attr.Text = text;
            attr.TextField = textField.GetName();
            attr.ValueType = valueType;
            attr.Title = title;
            attr.WinSize = winSize ?? new Size(1000, 800);
            return attr;
        }


        /// <summary>设置树列成员</summary>
        public UIAttribute SetColumnTree(Expression<Func<T, object>> field, Expression<Func<T, object>> idField, int? width = null, string title="")
        {
            var ui = GetOrCreate(field).SetColumn(ColumnType.Auto, width, title: title);
            ui.Tree = true;
            ui.ValueField = idField.GetName();
            return ui;
        }


        /// <summary>添加图标成员（只读图像）</summary>
        public UIAttribute SetColumnIcon(Expression<Func<T, object>> field, string title="")
        {
            return GetOrCreate(field).SetColumn(ColumnType.Icon, title: title);
        }


        /// <summary>添加图像成员</summary>
        public UIAttribute SetColumnImage(Expression<Func<T, object>> field, string title="")
        {
            return GetOrCreate(field).SetColumn(ColumnType.Image, title: title);
        }


        //---------------------------------------------------
        // 设置编辑器
        //---------------------------------------------------
        /// <summary>设置表单成员</summary>
        public UIAttribute SetEditor(Expression<Func<T, object>> field, EditorType editor = EditorType.Auto, object tag = null)
        {
            return GetOrCreate(field).SetEditor(editor, tag);
        }


        /// <summary>添加图像成员</summary>
        public UIAttribute SetEditorImage(Expression<Func<T, object>> field, Size? size=null)
        {
            return GetOrCreate(field).SetEditor(EditorType.Image, size);
        }

        /// <summary>添加网格成员</summary>
        /// <param name="field">关联属性。如 UniID</param>
        /// <param name="valueType">值类型。如 typeof(Res)</param>
        /// <param name="query">查询参数。如 key={0}</param>
        /// <remarks>创建控件方法见：FormRender.CreateGrid</remarks>
        public UIAttribute SetEditorGrid(Expression<Func<T, object>> field, Type valueType, string title, string query)
        {
            var attr = SetEditor(field, EditorType.Grid);
            attr.ValueField = field.GetName();
            attr.ValueType = valueType;
            attr.QueryString = query;
            attr.Title = title;
            return attr;
        }

        /// <summary>添加面板成员</summary>
        /// <param name="field">关联属性。如 UniID</param>
        /// <param name="urlTemplate">URL模板</param>
        /// <remarks>创建控件方法见：FormRender.CreatePanel</remarks>
        public UIAttribute SetEditorPanel(Expression<Func<T, object>> field, string urlTemplate, string title)
        {
            var attr = SetEditor(field, EditorType.Panel);
            attr.UrlTemplate = urlTemplate;
            attr.Title = title;
            return attr;
        }

        /// <summary>添加图片列表</summary>
        /// <param name="field">关联属性。如UniID</param>
        /// <param name="cate">图片保存路径。如Articles</param>
        /// <param name="imageWidth">图片保存最大宽度</param>
        /// <remarks>创建控件方法见：FormRender.CreateImages</remarks>
        public UIAttribute SetEditorImages(Expression<Func<T, object>> field, string cate, string title="", int? imageWidth=null)
        {
            var attr = SetEditor(field, EditorType.Images);
            attr.Tag = new { cate, imageWidth }.ToJson();
            if (title.IsNotEmpty())
                attr.Title = title;
            return attr;
        }


        /// <summary>添加文件列表</summary>
        /// <param name="field">关联属性。如UniID</param>
        /// <param name="cate">保存路径。如Articles</param>
        /// <remarks>创建控件方法见：FormRender.CreateFiles</remarks>
        public UIAttribute SetEditorFiles(Expression<Func<T, object>> field, string cate, string title="")
        {
            var attr = SetEditor(field, EditorType.Files);
            attr.Tag = new { cate}.ToJson();
            if (title.IsNotEmpty())
                attr.Title = title;
            return attr;
        }


        /// <summary>添加弹窗选择器成员</summary>
        public UIAttribute SetEditorWin(Expression<Func<T, object>> field, Type valueType, string textField, string urlTemplate, string title="")
        {
            var attr = SetEditor(field, EditorType.Win);
            attr.ValueType = valueType;
            attr.TextField = textField;
            attr.UrlTemplate = urlTemplate;
            if (title.IsNotEmpty())
                attr.Title = title;
            return attr;
        }

        /// <summary>添加弹出网格成员</summary>
        /// <param name="valueType">值类型。如 DAL.Res</param>
        /// <param name="textField">值类型中的文本域名称。如 Name</param>
        /// <param name="query">查询参数.如resKey={0}</param>
        public UIAttribute SetEditorWinGrid(Expression<Func<T, object>> field, Type valueType, string textField, string query="")
        {
            var attr = SetEditor(field, EditorType.WinGrid);
            attr.ValueType = valueType;
            attr.TextField = textField;
            attr.QueryString = query;
            return attr;
        }

        /// <summary>添加弹出GPS成员</summary>
        /// <param name="valueType">值类型。如 DAL.Res</param>
        /// <param name="textField">值类型中的文本域名称。如 Name</param>
        /// <param name="query">查询参数.如resKey={0}</param>
        public UIAttribute SetEditorGPS(Expression<Func<T, object>> field, Type valueType, string textField, string query="")
        {
            var attr = SetEditor(field, EditorType.GPS);
            attr.ValueType = valueType;
            attr.TextField = textField;
            attr.QueryString = query;
            return attr;
        }


        /// <summary>添加列表成员</summary>
        public UIAttribute SetEditorWinList(Expression<Func<T, object>> field, Type valueType, string textField)
        {
            var attr = SetEditor(field, EditorType.WinList);
            attr.ValueType = valueType;
            attr.TextField = textField;
            return attr;
        }
        /// <summary>添加列表成员</summary>
        public UIAttribute SetEditorWinList(Expression<Func<T, object>> field, Dictionary<string, object> dict)
        {
            var attr = SetEditor(field, EditorType.WinList);
            attr.Values = dict;
            return attr;
        }

        /// <summary>添加弹出树成员</summary>
        public UIAttribute SetEditorWinTree(Expression<Func<T, object>> field, Type valueType, string valueField, string textField)
        {
            var attr = SetEditor(field, EditorType.WinTree);
            attr.ValueType = valueType;
            attr.ValueField = valueField;
            attr.TextField = textField;
            return attr;
        }
    }
}