using com.google.zxing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using com.google.zxing.common;
using System.Drawing.Imaging;

namespace ZxingScan
{
    public partial class Form1 : Form
    {
        private int qr_width=200;
        private int qr_height=200;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            System.Collections.Hashtable hints =new Hashtable();
            hints.Add(EncodeHintType.CHARACTER_SET,"utf-8");
            com.google.zxing.qrcode.QRCodeWriter qrCodeWriter = new com.google.zxing.qrcode.QRCodeWriter();
  
            //图像数据转换，使用了矩阵转换
            ByteMatrix byteMatrix = qrCodeWriter.encode(textBox1.Text, BarcodeFormat.QR_CODE,qr_width, qr_height, hints);
            //下面这里按照二维码的算法，逐个生成二维码的图片，
            //两个for循环是图片横列扫描的结果
            //uint[] pixels = new uint[qr_width * qr_height];
            Bitmap bitmap = new Bitmap(byteMatrix.Width, byteMatrix.Height, PixelFormat.Format64bppArgb);
            for (int y = 0; y < byteMatrix.Height; y++)
            {
                for (int x = 0; x < byteMatrix.Width; x++)
                {
                    if (byteMatrix.Array[x][y]==-1)
                    {
                        bitmap.SetPixel(x, y, Color.White);
                    }
                    else
                    {
                        bitmap.SetPixel(x, y, Color.Black);
                    }
                }
            }

            pictureBox1.Image = bitmap;
        }
    }
}
