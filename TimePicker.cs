using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace TimePicker
{
    class TimePicker : PictureBox
    {
        // 時間・分モード切替用列挙体
        enum eMode
        {
            Hour,
            Minute
        }

        // 定数定義
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

        // 変数定義
        private int X;
        private int Y;
        private bool Clicked;
        private eMode Mode;
        private PointF DrawScale;
        private Point Center;
        private Rectangle DigitalRect;
        private Dictionary<string, SolidBrush> Brushes;
        private Dictionary<string, Pen> Pens;
        private Bitmap Bmp;
        private Graphics Gp;
        private StringFormat Format;
        private Font ClkFont;
        private Font ClkFont2;

        // プロパティ定義
        public int Hour { get; private set; }
        public int Minute { get; private set; }
        public new string Text { get; set; }

        // コンストラクタ
        public TimePicker() : base()
        {
            this.Init();
        }

        // コンストラクタ（位置指定）
        public TimePicker(int left, int top) : base() {
            this.Location = new Point(left, top);
            this.Init();
        }

        // コンストラクタ（位置・サイズ指定）
        public TimePicker(int left, int top, int width, int height) : base()
        {
            this.Location = new Point(left, top);
            this.Init(width, height);
        }

        // 初期化処理
        private void Init()
        {
            this.Init(DefaultWidth, DefaultHeight);
        }

        // 初期化処理（サイズ指定）
        private void Init(int width, int height)
        {
            // プロパティ初期化
            this.Hour = 0;
            this.Minute = 0;
            this.Text = "";
            // 変数初期化
            this.X = 0;
            this.Y = 0;
            this.Center = new Point(CenterLeft, CenterTop); // 原点
            this.DigitalRect = new Rectangle(DigitalLeft, DigitalTop, DigitalWidth, DigitalHeight); //デジタル部矩形
            this.Brushes = new Dictionary<string, SolidBrush>() // ブラシをあらかじめ作成しておく
            {
                ["BG"] = new SolidBrush(ColorTranslator.FromHtml("#AAAAFF")),
                ["BASE"] = new SolidBrush(ColorTranslator.FromHtml("#4444AA")),
                ["CELL"] = new SolidBrush(ColorTranslator.FromHtml("#6666CC")),
                ["SCELL"] = new SolidBrush(ColorTranslator.FromHtml("#FFFFFF")),
                ["RCELL"] = new SolidBrush(ColorTranslator.FromHtml("#AAAADD"))
            };
            this.Pens = new Dictionary<string, Pen>() // ペンをあらかじめ作成しておく
            {
                ["SLINE"] = new Pen(ColorTranslator.FromHtml("#FFFFFF")),
                ["RLINE"] = new Pen(ColorTranslator.FromHtml("#AAAADD"))
            };
            this.Pens["SLINE"].Width = 8;
            this.Pens["RLINE"].Width = 8;
            this.Bmp = new Bitmap(BitmapWidth, BitmapHeight); // レンダリング用ビットマップオブジェクト
            this.Gp = Graphics.FromImage(this.Bmp); // ビットマップからGraphicオブジェクトを作成しておく
            this.Format = new StringFormat(); // DrawStringに指定する文字配置
            this.Format.Alignment = StringAlignment.Center;
            this.Format.LineAlignment = StringAlignment.Center;
            this.ClkFont = new Font("ＭＳ　ゴシック", 48, FontStyle.Bold); // 時計盤用フォント（外側）
            this.ClkFont2 = new Font("ＭＳ　ゴシック", 36, FontStyle.Regular); // 時計盤用フォント（内側）
            this.DrawScale = this.GetScale(width, height, BitmapWidth, BitmapHeight); // UIとビットマップの拡大率
            this.Mode = eMode.Hour; // 時間入力モードに設定
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
            // 変数定義
            int r,s,f = BaseSize;
            double rad, d;
            Font fnt;
            Point c = this.Center,a;
            Graphics g = this.Gp;
            Size rect = this.Bmp.Size;
            List<object> cell = new List<object>();
            SolidBrush bh, bm,b = this.Brushes["BG"];
            Rectangle dr = DigitalRect;

            // 背景
            g.FillRectangle(b, 0, 32, rect.Width, rect.Height - 64);
            g.FillRectangle(b, 32, 0, rect.Width - 64, rect.Height);
            g.FillPie(b, 0, 0, 64, 64, 720, 360);
            g.FillPie(b, rect.Width - 64, 0, 64, 64, 270, 90);
            g.FillPie(b, 0, rect.Height - 64, 64, 64, 90, 90);
            g.FillPie(b, rect.Width - 64, rect.Height - 64, 64, 64, 0, 90);
            // アナログ部
            g.FillPie(this.Brushes["BASE"], c.X - f / 2, c.Y - f / 2, f, f, 0, 360);
            if (this.Mode == 0) {
                // 時間入力モード
                for (int i = 23; i >= 0; i--)
                {
                    if (i < 12)
                    {
                        // 0-11時
                        r = ValueRadius;
                        s = ValueSize;
                        fnt = this.ClkFont;
                    }
                    else
                    {
                        // 12-23時
                        r = ValueRadius2;
                        s = ValueSize2;
                        fnt = this.ClkFont2;
                    }
                    rad = this.Rad((i % 12) * 30 - 90); // 時計盤に表示する時間の角度を取得
                    a = this.GetArcPos(rad, r, c.X, c.Y); // 時計盤に表示する時間の座標を取得
                    d = this.GetDistance(this.X, this.Y, a.X, a.Y); //マウスカーソルと時間の座標の距離を求める
                    if (d <= s / 2) // 時間表示の円の内側にマウスカーソルがあるとき
                    {
                        // 選択カーソルを追加（マウス追随）
                        cell.Add(new Dictionary<string, object>()
                        {
                            ["radian"] = rad,
                            ["radius"] = r,
                            ["size"] = s,
                            ["font"] = fnt,
                            ["brush"] = Brushes["SCELL"],
                            ["pen"] = Pens["SLINE"],
                            ["value"] = i.ToString("D2")
                        });
                        // クリックされていたら現在の時間を選択する
                        if (this.Clicked)
                        {
                            this.Hour = i;
                        }
                    }
                    else if (i == this.Hour) // 表示時間が選択時間と同じ場合
                    {
                        // 選択カーソルを追加（選択済時間表示）
                        cell.Add(new Dictionary<string, object>()
                        {
                            ["radian"] = rad,
                            ["radius"] = r,
                            ["size"] = s,
                            ["font"] = fnt,
                            ["brush"] = Brushes["RCELL"],
                            ["pen"] = Pens["RLINE"],
                            ["value"] = i.ToString("D2")
                        });
                    }
                    // 時間の描画
                    g.FillPie(Brushes["CELL"], a.X - s / 2, a.Y - s / 2, s, s, 0, 360);
                    g.DrawString(i.ToString("D2"), fnt, this.Brushes["BASE"], a.X, a.Y, this.Format);
                }
                // デジタル切り替え用ブラシを設定
                bh = this.Brushes["SCELL"];
                bm = this.Brushes["RCELL"];
            }
            else
            {
                // 分入力モード
                for (int i = 0; i < 60 ; i++)
                {
                    rad = this.Rad(i * 6 - 90);
                    if(i % 5 == 0){
                        a = this.GetArcPos(rad, ValueRadius, c.X, c.Y);
                        g.DrawString(i.ToString("D2"), this.ClkFont, this.Brushes["BG"], a.X, a.Y, this.Format);
                    }
                }
                // 選択カーソルを追加（選択済分表示）
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
                d = this.GetDistance(this.X, this.Y, c.X, c.Y); // マウスカーソルと中心座標の距離を求める
                if (d <= f / 2) // マウスカーソルが時計盤の中にあるとき
                {
                    rad = Math.Atan2(this.Y - c.Y, this.X - c.X); // 中心座標とマウスカーソルの相対角度を取得
                    int min = this.Rad2Minute(rad); // 角度(ラジアン）から分を取得
                    // 選択カーソルを追加（マウス追随）
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
                    // クリックされていたら現在の分を選択する
                    if (this.Clicked)
                    {
                        this.Minute = min;
                    }
                }
                // デジタル切り替え用ブラシを設定
                bh = this.Brushes["RCELL"];
                bm = this.Brushes["SCELL"];
            }
            // 選択カーソル表示
            foreach(Dictionary<string,object> o in cell)
            {
                a = this.GetArcPos((double)o["radian"], (int)o["radius"], c.X, c.Y);
                g.DrawLine((Pen)o["pen"], c.X, c.Y, a.X, a.Y);
                g.FillPie((SolidBrush)o["brush"], c.X - 16, c.Y - 16, 32, 32, 0, 360);
                s = (int)o["size"];
                g.FillPie((SolidBrush)o["brush"], a.X - s / 2, a.Y - s / 2, s, s, 0, 360);
                g.DrawString((string)o["value"], (Font)o["font"], this.Brushes["BASE"], a.X, a.Y, this.Format);
            }
            // デジタル部
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

        // ラジアンから分を返す
        private int Rad2Minute(double rad)
        {
            int m = Convert.ToInt32(rad / (Math.PI * 2) * 60 + 15);
            if (m < 0)
            {
                m += 60;
            }
            return m;
        }

        // レンダリングサイズに対するUIサイズの比率を取得
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

        // コントロール上でマウスカーソルが移動したとき
        private void TimePicker_MouseMove(object sender, MouseEventArgs e)
        {
            this.X = (int)((float)e.X * this.DrawScale.X);
            this.Y = (int)((float)e.Y * this.DrawScale.Y);
            this.Invalidate();
        }

        // マウスボタンが離されたとき
        private void TimePicker_MouseUp(object sender, MouseEventArgs e)
        {
            this.Clicked = false;
        }

        // マウスボタンが押されたとき
        private void TimePicker_MouseDown(object sender, MouseEventArgs e)
        {
            this.Clicked = true;
            if (this.Y < this.DigitalRect.Top + this.DigitalRect.Height){
                if (this.X < this.Center.X)
                {
                    this.Mode = eMode.Hour;
                }
                else
                {
                    this.Mode = eMode.Minute;
                }
            }
            this.Invalidate();
            this.SetText();
        }

        // マウスカーソルがコントロールから離れたとき
        private void TimePicker_MouseLeave(object sender, EventArgs e)
        {
            this.Invalidate();
        }

    }
}

