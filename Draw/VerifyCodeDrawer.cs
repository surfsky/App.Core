using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace App.Core
{
    /// <summary>
    /// 校验码绘制器。更复杂的验证码可参考：
    /// 三维验证码：https://www.cnblogs.com/Aimeast/archive/2011/05/02/2034525.html
    /// 空心字验证码：http://blog.51cto.com/xclub/1597200
    /// </summary>
    public class VerifyCodeDrawer
    {
        /// <summary>
        /// 验证码字体类型
        /// </summary>
        public class VerifyCodeFont
        {
            public string Font { get; set; }
            public float FontSize { get; set; }
            public float FontWidth { get; set; }
            public float HMargin { get; set; }
            public float VMargin { get; set; }

            public VerifyCodeFont(string font, float fontSize, float fontWidth, float hmargin, float vmargin)
            {
                this.Font = font;
                this.FontSize = fontSize;
                this.FontWidth = fontWidth;
                this.HMargin = hmargin;
                this.VMargin = vmargin;
            }
        }


        /// <summary>生成验证码图片</summary>
        /// <returns>验证码和图片元组对象</returns>
        public static Tuple<string, Bitmap> Draw(int w = 80, int h = 40)
        {
            // 颜色、字体、字符（去掉了一些容易混淆的字符)
            Color[] colors = { Color.Black, Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Brown, Color.Brown, Color.DarkBlue };
            char[] chars = { '2', '3', '4', '5', '6', '8', '9', 'a', 'b', 'd', 'e', 'f', 'h', 'k', 'm', 'n', 'r', 'x', 'y', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'R', 'S', 'T', 'W', 'X', 'Y' };
            var fonts = new List<VerifyCodeFont>()
            {
                new VerifyCodeFont("Agent Red", 18, 18, -2, 0),      // 空心字体
                new VerifyCodeFont("Disko", 24, 17, -2, 2),           // 空心字体
                //new VerifyCodeFont("Times New Roman", 18, 18, 2, 2),
                //new VerifyCodeFont("Gungsuh", 18, 18, 2, 2),
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
            for (int i = 0; i < 100; i++)
            {
                int x = rnd.Next(w);
                int y = rnd.Next(h);
                int width = rnd.Next(5);
                int height = width;
                Color clr = colors[rnd.Next(colors.Length)];
                //g.FillEllipse(new SolidBrush(clr), x, y, width, height);
                bmp.SetPixel(x, y, clr);
            }

            // 画噪线 
            for (int i = 0; i < 5; i++)
            {
                int x1 = rnd.Next(w);
                int y1 = rnd.Next(h);
                int x2 = rnd.Next(w);
                int y2 = rnd.Next(h);
                Color clr = colors[rnd.Next(colors.Length)];
                g.DrawLine(new Pen(clr), x1, y1, x2, y2);
            }

            // 画验证码字符串 
            var type = fonts[rnd.Next(fonts.Count)];
            for (int i = 0; i < code.Length; i++)
            {
                Font font = new Font(type.Font, type.FontSize);
                Color color = colors[rnd.Next(colors.Length)];
                g.DrawString(
                    code[i].ToString(), 
                    font, 
                    new SolidBrush(color), 
                    i * type.FontWidth + type.HMargin, 
                    type.VMargin
                    );
            }

            // 扭曲
            bmp = DrawHelper.TwistImage(bmp);

            // 反相部分区域（未完成）
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

            //
            return new Tuple<string, Bitmap>(code, bmp);
        }
    }
}