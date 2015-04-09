using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        Settings appSettings;   //アプリの設定パラメータなど
        string gSettingFn;  //設定ファイルの名前

        //アプリの設定保存用クラス
        public class Settings
        {
            public int r;   //投影半径
            public int d;   //投影距離
            public bool save_check; //保存するかチェック
            public Settings()
            {
                r = 1700;
                d = 1200;
                save_check = false;
            }

        }

        public Form1()
        {
            InitializeComponent();

            this.AllowDrop = true;  //フォームがドラッグアンドドロップを受けられるようにする
            timer1.Enabled = true;

            appSettings = new Settings();
            gSettingFn = "setting.xml";

            //＜XMLファイルから設定を読み込む＞

            //XmlSerializerオブジェクトの作成
            System.Xml.Serialization.XmlSerializer serializer2 =
                new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            //ファイルを開く
            System.IO.StreamReader sr = new System.IO.StreamReader(
                gSettingFn, new System.Text.UTF8Encoding(false));
            //XMLファイルから読み込み、逆シリアル化する
            appSettings =
                (Settings)serializer2.Deserialize(sr);
            //閉じる
            sr.Close();

            //設定を回復する
            trackBar1.Value = appSettings.r;
            trackBar2.Value=appSettings.d;
            checkBox1.Checked = appSettings.save_check;
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            //コントロール内にドラッグされたとき実行される
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                //ドラッグされたデータ形式を調べ、ファイルのときはコピーとする
                e.Effect = DragDropEffects.Copy;
            else
                //ファイル以外は受け付けない
                e.Effect = DragDropEffects.None;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            //コントロール内にドロップされたとき実行される
            //ドロップされたすべてのファイル名を取得する
            string[] fileName =
                (string[])e.Data.GetData(DataFormats.FileDrop, false);



            for (int i = 0; i < fileName.Length; i++)
            {
                string fn = fileName[i];
                xLog(fn + "を処理中..");

                if (System.IO.Directory.Exists(fn) == false)
                {
                    xLog(fn + "はファイルです。");
                    xHosei(fn);
                }
                else
                {
                    xLog(fn + "はフォルダです。");
                    string[] files = System.IO.Directory.GetFiles(fn, "*", System.IO.SearchOption.AllDirectories);  //fn内のファイルを取得
                    int n = files.Length;
                    for(int j=0;j<n;j++)
                    {
                        xLog(j + "/" + n);
                        xHosei(files[j]);
                    }

                }

            }

        }
        //ファイルを補正して保存する
        private void xHosei(string fn)
        {
            xLog(fn + "を処理中..");
            if (System.IO.Path.GetExtension(fn).Equals(".jpg") || System.IO.Path.GetExtension(fn).Equals(".JPG"))
            {
                //pictureBox1.ImageLocation = fn;
                pictureBox1.Image = new Bitmap(fn); //picturebox1にファイルから読み込む
                pictureBox1.Refresh();
                Bitmap bitmap = new Bitmap(pictureBox1.Image);  //picturebox1から画像を読み込む
                Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height);
                //                    xImageProcess(bitmap, bitmap2);
                xImageProcessGyogan(bitmap, bitmap2);   //画像処理をして、その結果がpicturebox2に入る

                pictureBox2.Refresh();
                //pictureBox2.Image = bitmap2;
                if (checkBox1.Checked == true)
                {    //保存する場合
                    string dir = System.IO.Path.GetDirectoryName(fn);
                    string filename = System.IO.Path.GetFileNameWithoutExtension(fn);
                    string ext = System.IO.Path.GetExtension(fn);
                    string fn2 = System.IO.Path.Combine(dir, filename + "_hosei" + ext);


                    //exif情報をコピーする
                    foreach (System.Drawing.Imaging.PropertyItem item in pictureBox1.Image.PropertyItems)
                    {
                        bitmap2.SetPropertyItem(item);
                    }
                    //保存する
                    bitmap2.Save(fn2, System.Drawing.Imaging.ImageFormat.Jpeg);


                }
                bitmap.Dispose();
                bitmap2.Dispose();
                Application.DoEvents();
            }

        }



        //MimeTypeで指定されたImageCodecInfoを探して返す
        private static System.Drawing.Imaging.ImageCodecInfo
            GetEncoderInfo(string mineType)
        {
            //GDI+ に組み込まれたイメージ エンコーダに関する情報をすべて取得
            System.Drawing.Imaging.ImageCodecInfo[] encs =
                System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            //指定されたMimeTypeを探して見つかれば返す
            foreach (System.Drawing.Imaging.ImageCodecInfo enc in encs)
            {
                if (enc.MimeType == mineType)
                {
                    return enc;
                }
            }
            return null;
        }

        //ImageFormatで指定されたImageCodecInfoを探して返す
        private static System.Drawing.Imaging.ImageCodecInfo
            GetEncoderInfo(System.Drawing.Imaging.ImageFormat f)
        {
            System.Drawing.Imaging.ImageCodecInfo[] encs =
                System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            foreach (System.Drawing.Imaging.ImageCodecInfo enc in encs)
            {
                if (enc.FormatID == f.Guid)
                {
                    return enc;
                }
            }
            return null;
        }
        private void xImageProcess(Bitmap srcimg,Bitmap dstimg)
        {
            //単純に拡大するだけバイリニア法で。
            int w=srcimg.Width;
            int h=srcimg.Height;

            //srcimgに対して処理をして、picturebox2のbmpに格納する。

//            BitmapData data = srcimg.LockBits(new Rectangle(0, 0, srcimg.Width, srcimg.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData data = srcimg.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData data2 = dstimg.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[] buf = new byte[w * h * 3];    //RGBバッファを確保
            byte[] buf2 = new byte[w * h * 3];    //RGBバッファを確保
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);   //bufに画像をコピー
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
//                    Color color = xGetPixel(x, y, w, h, buf);

                    Color color = BiLinearInterpolation(((double)x)/20, ((double)y)/20, w, h, buf);
                    Color color2 = new Color();
                    color2 = color;
                    xSetPixel(x, y, color2, w, h, buf2);
                }
            }
            
  
            Marshal.Copy(buf2, 0, data2.Scan0, buf2.Length);
            srcimg.UnlockBits(data); 
            dstimg.UnlockBits(data2);

            pictureBox2.Image = dstimg;
            
            
        }


        private void xImageProcessGyogan(Bitmap srcimg, Bitmap dstimg)
        {
            //魚眼レンズ補正を行う
            int w = srcimg.Width;
            int h = srcimg.Height;

            //srcimgに対して処理をして、picturebox2のbmpに格納する。

            //            BitmapData data = srcimg.LockBits(new Rectangle(0, 0, srcimg.Width, srcimg.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            BitmapData data = srcimg.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            BitmapData data2 = dstimg.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            byte[] buf = new byte[w * h * 3];    //RGBバッファを確保
            byte[] buf2 = new byte[w * h * 3];    //RGBバッファを確保
            Marshal.Copy(data.Scan0, buf, 0, buf.Length);   //bufに画像をコピー

            double r = trackBar1.Value;
            double D = trackBar2.Value;


            //dstの(x,y)に対して、元画像srcのどの点が対応するか計算し、(x2,y2)とする。
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    double x1, y1, w1, h1 ;
                    double x2, y2;

                    x1 = (double)x;
                    y1 = (double)y;
                    w1 = (double)w;
                    h1 = (double)h;

                    x2 = r * (x1 - w1 / 2) / Math.Sqrt(D * D + (x1 - w1 / 2) * (x1 - w1 / 2) + (y1 - h1 / 2) * (y1 - h1 / 2)) + w / 2;
                    y2 = r * (y1 - h1 / 2) / Math.Sqrt(D * D + (x1 - w1 / 2) * (x1 - w1 / 2) + (y1 - h1 / 2) * (y1 - h1 / 2)) + h / 2;
                    Color c=BiLinearInterpolation(x2,y2,w,h,buf);   //元画像のx2,y2のいちのピクセル値を得る
                    xSetPixel(x, y, c, w, h, buf2);
                }
            }


            Marshal.Copy(buf2, 0, data2.Scan0, buf2.Length);
            srcimg.UnlockBits(data);
            dstimg.UnlockBits(data2);

            pictureBox2.Image = dstimg;


        }


        public Color xGetPixel(int x, int y, int w, int h, byte[] buf)
        {
            byte r, g, b;
            r = 0x80;
            g = 0x80;
            b = 0x80;
           
            if ((0 <= x) && (x < w) && (0 <= y) && (y < h))
            {
                r = buf[(x + y * w)*3 + 0];
                g = buf[(x + y * w)*3 + 1];
                b = buf[(x + y * w)*3 + 2];
                
            }
            
            return Color.FromArgb(r,g,b);
        }

        private Color BiLinearInterpolation(double x, double y, int w,int h,byte[] buf)
        {
            //バイリニア法で補完したピクセルを得る

            int x1 = (int)Math.Floor(x);
            int y1 = (int)Math.Floor(y);

            double R = 0;
            double G = 0;
            double B = 0;

            double[] Wx = new double[2];
            double[] Wy = new double[2];
            
            Wx[0] = 1 - (x - x1);
            Wx[1] = x - x1;
            Wy[0] = 1 - (y - y1);
            Wy[1] = y - y1;

            for (int i = 0; i < Wx.Length; i++)
            {
                for (int j = 0; j < Wy.Length; j++)
                {
                    Color c = xGetPixel(x1 + i, y1 + j, w, h, buf);
                    R += ((double)c.R * Wx[i] * Wy[j]);
                    G += ((double)c.G * Wx[i] * Wy[j]);
                    B += ((double)c.B * Wx[i] * Wy[j]);

                }
            }

            return Color.FromArgb((int)R, (int)G, (int)B);
        }

        private void xSetPixel(int x, int y, Color color, int w, int h, byte[] buf)
        {
            byte r, g, b;
            r = color.R;
            g = color.G;
            b = color.B;

            if ((0 <= x) && (x < w) && (0 <= y) && (y < h))
            {
                buf[(x + y * w) * 3 + 0] = r;
                buf[(x + y * w) * 3 + 1] = g;
                buf[(x + y * w) * 3 + 2] = b;

            }
        }
        private void xLog(String s)
        {
            //ログ出力
            DateTime dt = DateTime.Now;
            textBox1.AppendText(dt.ToString() + " " + s + "\n");
            System.IO.StreamWriter sw = new System.IO.StreamWriter(@"log.txt", true, System.Text.Encoding.GetEncoding("shift_jis"));
            sw.Write(dt.ToString() + " " + s + "\r\n");
            sw.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = trackBar1.Value.ToString();
            label2.Text = trackBar2.Value.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(pictureBox1.Image);  //picturebox1から画像を読み込む
            Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height);
            //                    xImageProcess(bitmap, bitmap2);
            xImageProcessGyogan(bitmap, bitmap2);
            //pictureBox2.Image = bitmap2;

        }


        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //フォームが閉じるときに現在の設置値を保存する
            appSettings.r = trackBar1.Value;
            appSettings.d = trackBar2.Value;
            appSettings.save_check = checkBox1.Checked;

            //＜XMLファイルに書き込む＞
            //XmlSerializerオブジェクトを作成
            //書き込むオブジェクトの型を指定する
            System.Xml.Serialization.XmlSerializer serializer1 =
                new System.Xml.Serialization.XmlSerializer(typeof(Settings));
            //ファイルを開く（UTF-8 BOM無し）
            System.IO.StreamWriter sw = new System.IO.StreamWriter(
                gSettingFn, false, new System.Text.UTF8Encoding(false));
            //シリアル化し、XMLファイルに保存する
            serializer1.Serialize(sw, appSettings);
            //閉じる
            sw.Close();
        }
    }
}
