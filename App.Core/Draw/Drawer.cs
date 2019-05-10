using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing.Drawing2D;
using ThoughtWorks.QRCode.Codec;
using System.Text;

namespace App.Core
{
    /// <summary>
    /// 绘图相关辅助方法
    /// </summary>
    public static class Drawer
    {
        /// <summary>创建二维码图片</summary>
        /// <param name="text">文本</param>
        /// <param name="iconUrl">图标地址</param>
        public static Image CreateQrCodeImage(string text, string iconUrl="")
        {
            // 二维码图片
            QRCodeEncoder encoder = new QRCodeEncoder();
            encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE; //编码方法
            encoder.QRCodeScale = 24;  // 大小
            encoder.QRCodeVersion = 0; // 版本
            encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            Image bmp = encoder.Encode(text, Encoding.UTF8);
            return DrawIcon(bmp, iconUrl);
        }

        /// <summary>叠加绘制图标</summary>
        public static Image DrawIcon(Image img, string iconUrl)
        {
            if (iconUrl.IsNotEmpty())
            {
                var icon = HttpHelper.GetServerOrNetworkImage(iconUrl);
                int s = img.Width / 5;
                icon = Drawer.CreateThumbnail(icon, s, s);
                var point = new Point((img.Width - s) / 2, (img.Height - s) / 2);
                img = Drawer.MergeImage(img, (Bitmap)icon, 0.95f, point);
                icon.Dispose();
            }
            return img;
        }

        /// <summary>加载图片。如果用Image.FromFile()方法的话会锁定图片，无法编辑、移动、删除。</summary>
        public static Image LoadImage(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                return Image.FromStream(stream);
        }

        /// <summary>绘制缩略图</summary>
        public static void CreateThumbnail(string sourceImagePath, string targetImagePath, int width, int? height=null)
        {
            string savePath = targetImagePath.IsEmpty() ? sourceImagePath : targetImagePath;
            Image img = Image.FromFile(sourceImagePath);   // 用LoadImage()反而会导致后面的 bmp.Save() 报错，先这样
            Image bmp = CreateThumbnail(img, width, height);
            img.Dispose();
            bmp.Save(savePath);
            bmp.Dispose();
        }


        /// <summary>创建缩略图</summary>
        public static Image CreateThumbnail(string filePath, int w, int? h)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var img = Image.FromStream(stream);
                return CreateThumbnail(img, w, h);
            }
        }

        /// <summary>创建缩略图</summary>
        public static Image CreateThumbnail(Image img, int width, int? height=null)
        {
            if (img == null) return null;
            // 计算图片的尺寸
            if (height == null)
                height = img.Height * width / img.Width;

            // 绘制Bitmap新实例
            Bitmap bmp = new Bitmap(width, height.Value, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.Clear(Color.Transparent);
            g.DrawImage(img, new Rectangle(0, 0, width, height.Value));
            g.Dispose();

            return bmp;
        }

        /// <summary>
        /// 合并两张图片。第二张图片可指定不透明度以及粘贴位置。
        /// 注意 img 和 img2 在本函数中都没有释放，请自行Dispose。
        /// </summary>
        public static Image MergeImage(Image img, Image img2, float opacity, params Point[] points)
        {
            if (img == null || img2 == null)
                return null;

            // 创建一个图像用于最后输出
            Bitmap bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));

            // 设置图像绘制属性: 设置透明度
            ImageAttributes imageAttributes = new ImageAttributes();
            float[][] colorMatrixElements = {
                new float[] {1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                new float[] {0.0f, 1.0f, 0.0f, 0.0f, 0.0f},
                new float[] {0.0f, 0.0f, 1.0f, 0.0f, 0.0f},
                new float[] {0.0f, 0.0f, 0.0f, opacity, 0.0f},
                new float[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}};
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // 合并
            int wmWidth = img2.Width;
            int wmHeight = img2.Height;
            foreach (Point pt in points)
                g.DrawImage(
                    img2,
                    new Rectangle(pt.X, pt.Y, wmWidth, wmHeight),
                    0, 0, wmWidth, wmHeight,
                    GraphicsUnit.Pixel,
                    imageAttributes
                    );

            // 释放资源
            img.Dispose();
            g.Dispose();
            return bmp;
        }


        /// <summary>图片颜色反相叠加（未完成）</summary>
        public static Bitmap ReverseImage(Bitmap img, Bitmap img2, params Point[] points)
        {
            if (img == null || img2 == null)
                return null;

            // 创建一个图像用于最后输出
            Bitmap bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.DrawImage(img, new Rectangle(0, 0, img.Width, img.Height));

            // 设置图像绘制属性: 设置反相叠加矩阵
            ImageAttributes imageAttributes = new ImageAttributes();
            float[][] colorMatrixElements = {
                new float[] {-1.0f, 0.0f, 0.0f, 0.0f, 0.0f},
                new float[] {0.0f, -1.0f, 0.0f, 0.0f, 0.0f},
                new float[] {0.0f, 0.0f, -1.0f, 0.0f, 0.0f},
                new float[] {0.0f, 0.0f, 0.0f, 1.0f, 0.0f},
                new float[] {0.0f, 0.0f, 0.0f, 0.0f, 1.0f}};
            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // 合并
            int wmWidth = img2.Width;
            int wmHeight = img2.Height;
            foreach (Point pt in points)
                g.DrawImage(
                    img2,
                    new Rectangle(pt.X, pt.Y, wmWidth, wmHeight),
                    0, 0, wmWidth, wmHeight,
                    GraphicsUnit.Pixel,
                    imageAttributes
                    );

            // 释放资源
            img.Dispose();
            g.Dispose();
            return bmp;
        }

        /// <summary>旋转图片</summary>
        /// <param name="angle">角度（-360 到 360）</param>
        public static Bitmap RotateImage(Bitmap bmp, float angle)
        {
            Bitmap returnBitmap = new Bitmap((int)(bmp.Width*1.5), (int)(bmp.Height*1.5));
            Graphics g = Graphics.FromImage(returnBitmap);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);     // move rotation point to center of image
            g.RotateTransform(angle);                                          // rotate
            g.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);   // move image back
            g.DrawImage(bmp, new Point(0, 0));
            return returnBitmap;
        }

        /// <summary>TODO:三维贴图扭曲图片（未完成）</summary>
        public static Bitmap TwistImage3D(Bitmap img, string model3DRes)
        {
            throw new NotImplementedException();
        }

        /// <summary>正弦扭曲图片</summary>  
        /// <param name="img">图片路径</param>  
        /// <param name="range">波形的幅度倍数，越大扭曲的程度越高，一般为3</param>  
        /// <param name="phase">波形的起始相位，取值区间[0-2*PI)</param>  
        /// <param name="direction">扭曲方向</param>  
        /// <remarks>现在只能实现0度和90度扭曲，难的验证码是三维曲面扭曲，字体完全变形粘连才难破解（容后）</remarks>
        public static Bitmap TwistImage(Bitmap img, double range = 3, double phase = 0, bool direction = false)
        {
            double PI2 = 6.283185307179586476925286766559;
            Bitmap destBmp = new Bitmap(img.Width, img.Height);
            Graphics g = Graphics.FromImage(destBmp);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, destBmp.Width, destBmp.Height);
            g.Dispose();

            // 遍历填充像素
            double baseAxisLen = direction ? (double)destBmp.Height : (double)destBmp.Width;
            for (int i = 0; i < destBmp.Width; i++)
            {
                for (int j = 0; j < destBmp.Height; j++)
                {
                    double dx = 0;
                    dx = direction ? (PI2 * (double)j) / baseAxisLen : (PI2 * (double)i) / baseAxisLen;
                    dx += phase;
                    double dy = Math.Sin(dx);

                    // 取得当前点的颜色
                    int nOldX = 0, nOldY = 0;
                    nOldX = direction ? i + (int)(dy * range) : i;
                    nOldY = direction ? j : j + (int)(dy * range);
                    Color color = img.GetPixel(i, j);
                    if (nOldX >= 0 && nOldX < destBmp.Width
                     && nOldY >= 0 && nOldY < destBmp.Height)
                    {
                        destBmp.SetPixel(nOldX, nOldY, color);
                    }
                }
            }
            return destBmp;
        }



        //-----------------------------------------------
        /// <summary>将图片转化为 Base64 字符串</summary>
        public static string ToBase64(this Image image)
        {
            if (image == null)
                return "";

            var ms = new MemoryStream();
            image.Save(ms, ImageFormat.Png);
            byte[] bytes = ms.GetBuffer();
            return "data:image/png;base64," + Convert.ToBase64String(bytes);
        }

        /// <summary>从 Base64 字符串中创建图像</summary>
        public static Image ToImage(this string base64Image)
        {
            try
            {
                var txt = base64Image.Split(',')[1];
                var ms = new MemoryStream(Convert.FromBase64String(txt));
                return Image.FromStream(ms);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>将图像转换为字节数组</summary>
        public static byte[] ToBytes(this Image img)
        {
            if (img == null)
                return null;
            else
            {
                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Png);
                byte[] bytes = ms.ToArray();
                ms.Close();
                return bytes;
            }
        }
    }
}