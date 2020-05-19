using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Web;

namespace App.Utils
{
    /// <summary>
    /// 验证码图片
    /// </summary>
    public class VerifyImage
    {
        public string Code { get; set; }
        public Image Image { get; set; }

        public VerifyImage(string code, Image image)
        {
            this.Code = code;
            this.Image = image;
        }
    }

    /// <summary>
    /// 校验码绘制器。更复杂的验证码可参考：
    /// 三维验证码：https://www.cnblogs.com/Aimeast/archive/2011/05/02/2034525.html
    /// 空心字验证码：http://blog.51cto.com/xclub/1597200
    /// </summary>
    public class VerifyPainter
    {
        /// <summary>
        /// 验证码配置
        /// </summary>
        public class VerifyCodeConfig
        {
            public Font Font { get; set; }
            public float Width { get; set; }
            public float HMargin { get; set; }
            public float VMargin { get; set; }

            public VerifyCodeConfig(Font font, float width, float hmargin, float vmargin)
            {
                this.Font = font;
                this.Width = width;
                this.HMargin = hmargin;
                this.VMargin = vmargin;
            }
        }



        /// <summary>生成验证码图片</summary>
        /// <returns>验证码和图片元组对象</returns>
        public static VerifyImage Draw(int w = 80, int h = 40)
        {
            var fontStencil = FontHelper.GetFont(App.Utils.Properties.Resources.Stencil_ICG, 36, FontStyle.Bold);
            //var fontPledg = FontHelper.GetFont(App.Properties.Resources.PLEDG_KI, 36, FontStyle.Italic);  // 注意样式要匹配，否则会报异常
            //var fontAgent = FontHelper.GetFont(App.Properties.Resources.Agent_Red, 26, FontStyle.Regular);

            // 颜色、字体、字符（去掉了一些容易混淆的字符)
            Color[] colors = { Color.Green};
            char[] chars = { '2', '3', '4', '5', '6', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'M', 'N', 'P', 'R', 'S', 'T', 'W', 'X', 'Y' };
            var cfgs = new List<VerifyCodeConfig>()
            {
                //new VerifyCodeConfig(fontAgent, 18, -2, 0),      // 空心字体
                //new VerifyCodeConfig(new Font("Disko", 24), 17, -2, 2),           // 空心字体
                //new VerifyCodeConfig(new Font("Times New Roman", 36), 18, -8, 2),
                new VerifyCodeConfig(fontStencil, 22, -5, 0),
                //new VerifyCodeConfig(fontPledg, 20, -2, 0),
                //new VerifyCodeFont(new Font("Gungsuh", 18), 18, 2, 2),
            };

            // 生成验证码字符串 
            Random rnd = new Random();
            string code = string.Empty;
            for (int i = 0; i < 4; i++)
                code += chars[rnd.Next(chars.Length)];

            // 创建画布
            var bmp = new Bitmap(w, h);
            var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);

            // 画噪点 
            for (int i = 0; i < 40; i++)
            {
                int x = rnd.Next(w);
                int y = rnd.Next(h);
                int width = rnd.Next(5);
                int height = width;
                bmp.SetPixel(x, y, Color.Gray);
            }

            // 画噪线 
            /*
            for (int i = 0; i < 5; i++)
            {
                int x1 = rnd.Next(w);
                int y1 = rnd.Next(h);
                int x2 = rnd.Next(w);
                int y2 = rnd.Next(h);
                Color clr = colors[rnd.Next(colors.Length)];
                g.DrawLine(new Pen(clr), x1, y1, x2, y2);
            }
            */

            // 画验证码字符串
            //Font font = new Font("Arial", 36);
            //Color color = Color.Blue;
            //DrawChar(g, 'B', 0, 0, font, color);
            var cfg = cfgs[rnd.Next(cfgs.Count)];
            for (int i = 0; i < code.Length; i++)
            {
                var c = code[i];
                var x = i * cfg.Width + cfg.HMargin;
                var y = cfg.VMargin;
                var angle = new Random().Next(-30, 30);
                var font = cfg.Font;
                var color = colors[rnd.Next(colors.Length)];
                if (angle == 0) angle = 15;
                DrawChar(g, c, x, y, font, color, angle);
                System.Threading.Thread.Sleep(1);
            }

            // 扭曲
            bmp = Utils.Painter.Twist(bmp);

            // 反相部分区域（未完成）
            /*
            var bmp2 = new Bitmap(w, h);
            var g2 = Graphics.FromImage(bmp2);
            //g2.FillRectangle(new SolidBrush(Color.Black), 0, 0, w, h);
            var polygon1 = new PointF[] {
                new PointF(0, 0),
                new PointF((float)(w*2/3.0), 0),
                new PointF(0, (float)(h*2/3.0))
            };
            g2.FillPolygon(new SolidBrush(Color.White), polygon1);
            var polygon2 = new PointF[] {
                new PointF((float)(w/3.0), h),
                new PointF(w, h),
                new PointF(w, (float)(h/3.0))
            };
            g2.FillPolygon(new SolidBrush(Color.White), polygon2);
            //bmp = DrawHelper.ReverseImage(bmp, bmp2, new Point(0, 0));
            */

            //
            return new VerifyImage(code, bmp);
        }

        /// <summary>在指定位置绘制字符</summary>
        /// <param name="angle">旋转角度（以度为单位）</param>
        private static void DrawChar(Graphics g, char c, float x, float y, Font font, Color color, int angle)
        {
            var pen = new Pen(color, 1);
            var brush = new SolidBrush(color);

            //g.DrawString(c.ToString(), font, brush, x, y);
            var path = GetTextPath(g, c.ToString(), font);
            var region = new Region(path);
            var rect = region.GetBounds(g);
            var rectFull = new RectangleF(0, 0, rect.Width + rect.X, rect.Height + rect.Y);
            var rectHalf = new RectangleF(0, rect.Y + rect.Height/2, rectFull.Width, rect.Height / 2);  // 下半部区域
            region.Intersect(rectHalf);

            // 绘制字符图片(描边、填充一半底部)
            var bitmap = new Bitmap((int)rectFull.Width, (int)rectFull.Height);
            var g2 = Graphics.FromImage(bitmap);
            g2.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g2.SmoothingMode = SmoothingMode.AntiAlias;
            g2.CompositingQuality = CompositingQuality.HighQuality;
            g2.DrawPath(pen, path);           // 描边
            g2.FillRegion(brush, region);     // 填充一半底部

            // 旋转
            bitmap = Utils.Painter.Rotate(bitmap, angle);

            // 绘制到目标位置
            g.DrawImageUnscaled(bitmap, (int)x, (int)y);
        }

        /// <summary>获取文字轮廓路径</summary>
        public static GraphicsPath GetTextPath(Graphics g, string text, Font font)
        {
            var path = new GraphicsPath();
            path.AddString(text, font.FontFamily, (int)font.Style, font.Size, new PointF(0, 0), new StringFormat());
            return path;
        }

        /// <summary>获取文字区域</summary>
        public static Region GetTextRegion(Graphics g, string text, Font font)
        {
            var path = GetTextPath(g, text, font);
            return new Region(path);
        }
    }
}