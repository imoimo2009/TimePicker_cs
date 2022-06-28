using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace TimePicker
{
    class TimePicker : PictureBox
    {
        enum eMode
        {
            Hour,
            Minute
        }

        private const int DefaultWidth = 200;
        private const int DefaultHeight = 240;
        private const int BitmapWidth = 800;
        private const int BitmapHeight = 960;
        private const int CenterLeft = 400;
        private const int CenterTop = 560;
        private const int DigitalLeft = 200;
        private const int DigitalTop = 40;
        private const int DigitalWidth = 400;
        private const int DigitalHeight = 144;
        private const int BaseSize = 720;
        private const int ValueSize = 100;
        private const int ValueRadius = 292;
        private const int ValueSize2 = 88;
        private const int ValueRadius2 = 188;
        private int X;
        private int Y;
        private bool Clicked;
        private eMode Mode;
        PointF DrawScale;
        Point Center;
        Rectangle DigitalRect;
        Dictionary<string, SolidBrush> Brushes;
        Dictionary<string, Pen> Pens;
        System.Drawing.Bitmap Bmp;
        System.Drawing.Graphics Gp;
        System.Drawing.StringFormat Format;
        System.Drawing.Font ClkFont;
        System.Drawing.Font ClkFont2;
        public int Hour { get; private set; }
        public int Minute { get; private set; }
        public new string Text { get; set; }
        public TimePicker() : base()
        {
            this.Init();
        }

        public TimePicker(int left, int top) : base() {
            this.Location = new Point(left, top);
            this.Init();
        }

        public TimePicker(int left, int top, int width, int height) : base()
        {
            this.Location = new Point(left, top);
            this.Init(width, height);
        }

        private void Init()
        {
            this.Init(DefaultWidth, DefaultHeight);
        }

        private void Init(int width, int height)
        {
            // プロパティ初期化
            this.Hour = 0;
            this.Minute = 0;
            this.X = 0;
            this.Y = 0;
            this.Center = new Point(CenterLeft, CenterTop);
            this.DigitalRect = new Rectangle(DigitalLeft, DigitalTop, DigitalWidth, DigitalHeight);
            this.Text = "";
            this.Brushes = new Dictionary<string, SolidBrush>()
            {
                ["BG"] = new SolidBrush(ColorTranslator.FromHtml("#AAAAFF")),
                ["BASE"] = new SolidBrush(ColorTranslator.FromHtml("#4444AA")),
                ["CELL"] = new SolidBrush(ColorTranslator.FromHtml("#6666CC")),
                ["SCELL"] = new SolidBrush(ColorTranslator.FromHtml("#FFFFFF")),
                ["RCELL"] = new SolidBrush(ColorTranslator.FromHtml("#AAAADD"))
            };
            this.Pens = new Dictionary<string, Pen>()
            {
                ["SLINE"] = new Pen(ColorTranslator.FromHtml("#FFFFFF")),
                ["RLINE"] = new Pen(ColorTranslator.FromHtml("#AAAADD"))
            };
            this.Pens["SLINE"].Width = 8;
            this.Pens["RLINE"].Width = 8;
            this.Bmp = new Bitmap(BitmapWidth, BitmapHeight);
            this.Gp = Graphics.FromImage(this.Bmp);
            this.Format = new StringFormat();
            this.Format.Alignment = StringAlignment.Center;
            this.Format.LineAlignment = StringAlignment.Center;
            this.ClkFont = new Font("ＭＳ　ゴシック", 48, FontStyle.Bold);
            this.ClkFont2 = new Font("ＭＳ　ゴシック", 36, FontStyle.Regular);
            this.DrawScale = this.GetScale(width, height, BitmapWidth, BitmapHeight);
            this.Mode = eMode.Hour;
            // PictureBox初期化
            this.Size = new Size(width, height);
            this.BorderStyle = BorderStyle.None;
            this.BackColor = Color.Transparent;
            this.Font = new Font("ＭＳ　ゴシック", 80, FontStyle.Bold);
            this.SizeMode = PictureBoxSizeMode.StretchImage;
            // イベントハンドラ登録
            this.Paint += TimePicker_Paint;
            this.MouseDown += TimePicker_MouseDown;
            this.MouseUp += TimePicker_MouseUp;
            this.MouseMove += TimePicker_MouseMove;
            this.MouseLeave += TimePicker_MouseLeave;
        }

        // オーナードロー(独自描画)処理
        private void TimePicker_Paint(object sender, PaintEventArgs e)
        {
            double rad, d;
            Point c = this.Center,a;
            Graphics g = this.Gp;
            Size rect = this.Bmp.Size;
            List<object> cell = new List<object>();
            SolidBrush bh, bm,b = this.Brushes["BG"];
            // 背景
            g.FillRectangle(b, 0, 32, rect.Width, rect.Height - 64);
            g.FillRectangle(b, 32, 0, rect.Width - 64, rect.Height);
            g.FillPie(b, 0, 0, 64, 64, 720, 360);
            g.FillPie(b, rect.Width - 64, 0, 64, 64, 270, 90);
            g.FillPie(b, 0, rect.Height - 64, 64, 64, 90, 90);
            g.FillPie(b, rect.Width - 64, rect.Height - 64, 64, 64, 0, 90);
            // アナログ部
            int f = BaseSize;
            g.FillPie(this.Brushes["BASE"], c.X - f / 2, c.Y - f / 2, f, f, 0, 360);
            if (this.Mode == 0) {
                // 時間入力モード
                for (int i = 23; i >= 0; i--) {
                    int r, vs;
                    Font fnt;
                    if (i < 12) {
                        // 0-11時
                        r = ValueRadius;
                        vs = ValueSize;
                        fnt = this.ClkFont;
                    } else
                    {
                        // 12-23時
                        r = ValueRadius2;
                        vs = ValueSize2;
                        fnt = this.ClkFont2;
                    }
                    rad = this.Rad((i % 12) * 30 - 90);
                    a = this.GetArcPos(rad, r, c.X, c.Y);
                    d = this.GetDistance(this.X, this.Y, a.X, a.Y);
                    if (d <= vs / 2)
                    {
                        cell.Add(new Dictionary<string, object>()
                        {
                            ["radian"] = rad,
                            ["radius"] = r,
                            ["size"] = vs,
                            ["font"] = fnt,
                            ["brush"] = Brushes["SCELL"],
                            ["pen"] = Pens["SLINE"],
                            ["value"] = i.ToString("D2")
                        });
                        if (this.Clicked) {
                            this.Hour = i;
                        }
                    }
                    else if(i == this.Hour){
                        cell.Add(new Dictionary<string, object>()
                        {
                            ["radian"] = rad,
                            ["radius"] = r,
                            ["size"] = vs,
                            ["font"] = fnt,
                            ["brush"] = Brushes["RCELL"],
                            ["pen"] = Pens["RLINE"],
                            ["value"] = i.ToString("D2")
                        });
                }
                    g.FillPie(Brushes["CELL"], a.X - vs / 2, a.Y - vs / 2, vs, vs, 0, 360);
                    g.DrawString(i.ToString("D2"), fnt, this.Brushes["BASE"], a.X, a.Y, this.Format);
                }
                // デジタル切り替え用
                bh = this.Brushes["SCELL"];
                bm = this.Brushes["RCELL"];
            }
            else
            {
                // 分入力モード
                for (int i = 0; i < 60 ; i++){
                    rad = this.Rad(i * 6 - 90);
                    if(i % 5 == 0){
                        a = this.GetArcPos(rad, ValueRadius, c.X, c.Y);
                        g.DrawString(i.ToString("D2"), this.ClkFont, this.Brushes["BG"], a.X, a.Y, this.Format);
                    }
                }
                cell.Add(new Dictionary<string, object>()
                {
                    ["radian"] = this.Rad(this.Minute * 6 - 90),
                    ["radius"] = ValueRadius,
                    ["size"] = ValueSize,
                    ["font"] = this.ClkFont,
                    ["brush"] = this.Brushes["RCELL"],
                    ["pen"] = this.Pens["RLINE"],
                    ["value"] = this.Minute.ToString("D2")
                });
                d = this.GetDistance(this.X, this.Y, c.X, c.Y);
                if (d <= f / 2)
                {
                    rad = Math.Atan2(this.Y - c.Y, this.X - c.X);
                    int min = this.Rad2Minute(rad);
                    cell.Add(new Dictionary<string, object>()
                    {
                        ["radian"] = rad,
                        ["radius"] = ValueRadius,
                        ["size"] = ValueSize,
                        ["font"] = this.ClkFont,
                        ["brush"] = this.Brushes["SCELL"],
                        ["pen"] = this.Pens["SLINE"],
                        ["value"] = min.ToString("D2")
                    });
                    if (this.Clicked)
                    {
                        this.Minute = min;
                    }
                }
                // デジタル切り替え用
                bh = this.Brushes["RCELL"];
                bm = this.Brushes["SCELL"];
            }
            // 選択カーソル表示
            foreach(Dictionary<string,object> o in cell){
                a = this.GetArcPos((double)o["radian"], (int)o["radius"], c.X, c.Y);
                g.DrawLine((Pen)o["pen"], c.X, c.Y, a.X, a.Y);
                g.FillPie((SolidBrush)o["brush"], c.X - 16, c.Y - 16, 32, 32, 0, 360);
                int vs = (int)o["size"];
                g.FillPie((SolidBrush)o["brush"], a.X - vs / 2, a.Y - vs / 2, vs, vs, 0, 360);
                g.DrawString((string)o["value"], (Font)o["font"], this.Brushes["BASE"], a.X, a.Y, this.Format);
            }
            // デジタル部
            Rectangle dr = DigitalRect;
            g.FillRectangle(this.Brushes["BASE"], dr.Left, dr.Top, dr.Width, dr.Height);
            g.DrawString(this.Hour.ToString("D2"), this.Font, bh, c.X - 88, 112, this.Format);
            g.DrawString(":", this.Font, this.Brushes["RCELL"], c.X, 104, this.Format);
            g.DrawString(this.Minute.ToString("D2"), this.Font, bm, c.X + 88, 112, this.Format);
            this.Image = this.Bmp;
        }

        // 角度をラジアンに変換
        private double Rad(int deg)
        {
            return Math.PI / 180 * deg;
        }

        // 指定角度の円弧座標を返す
        private Point GetArcPos(double rad, int r, int x, int y)
        {
            Point p = new Point();
            p.X = Convert.ToInt32(Math.Cos(rad) * r + x);
            p.Y = Convert.ToInt32(Math.Sin(rad) * r + y);
            return p;
        }

        // 2点間の距離を算出
        private double GetDistance(int x1, int y1, int x2, int y2)
        {
            double xp = Math.Pow(Math.Abs(x2 - x1), 2);
            double yp = Math.Pow(Math.Abs(y2 - y1), 2);
            return Math.Sqrt(xp + yp);
        }

        // ラジアンから分数を返す
        private int Rad2Minute(double rad)
        {
            int m = Convert.ToInt32(rad / (Math.PI * 2) * 60 + 15);
            if (m < 0)
            {
                m += 60;
            }
            return m;
        }

        private PointF GetScale(int w1, int h1, int w2, int h2)
        {
            PointF p = new PointF();
            p.X = (float)w2 / w1;
            p.Y = (float)h2 / h1;
            return p;
        }

        // テキストプロパティを更新
        private void SetText()
        {
            this.Text = this.Hour.ToString("D2") + ":" + this.Minute.ToString("D2");
        }

        // マウスが移動したとき
        private void TimePicker_MouseMove(object sender, MouseEventArgs e)
        {
            this.X = (int)((float)e.X * this.DrawScale.X);
            this.Y = (int)((float)e.Y * this.DrawScale.Y);
            this.Invalidate();
        }

        // ボタンが離されたとき
        private void TimePicker_MouseUp(object sender, MouseEventArgs e)
        {
            this.Clicked = false;
        }

        // ボタンが押されたとき
        private void TimePicker_MouseDown(object sender, MouseEventArgs e)
        {
            this.Clicked = true;
            Point c = this.Center;
            Rectangle d = this.DigitalRect;
            if (this.Y < d.Top + d.Height){
                if (this.X < c.X){
                    this.Mode = eMode.Hour;
                    }else
                {
                    this.Mode = eMode.Minute;
                }
            }
            this.Invalidate();
            this.SetText();
        }
        private void TimePicker_MouseLeave(object sender, EventArgs e)
        {
            this.Invalidate();
        }

    }
}

