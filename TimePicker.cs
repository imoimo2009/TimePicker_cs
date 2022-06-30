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

        // ブラシインデックスの列挙体
        enum eBrush
        {
            BG,
            BASE,
            CELL,
            SCELL,
            RCELL,
            CLOSE,
            SCLOSE
        }

        // ペンインデックスの列挙体
        enum ePen
        {
            SLINE,
            RLINE,
            CLOSE,
            SCLOSE
        }

        // 選択カーソルパラメータインデックスの列挙体
        enum eCell
        {
            Radian,
            Radius,
            Size,
            Font,
            Brush,
            Pen,
            Value
        }

        // 定数定義
        private const int DefaultWidth = 200;
        private const int DefaultHeight = 240;
        private const int BitmapWidth = 800;
        private const int BitmapHeight = 960;
        private const int CenterLeft = 400;
        private const int CenterTop = 560;
        private const int BGRadius = 32;
        private const int BGRadiusx2 = BGRadius * 2;
        private const int BGLeftTopDeg = 180;
        private const int BGRightTopDeg = 270;
        private const int BGLeftBottomDeg = 90;
        private const int BGRrightBottomDeg = 0;
        private const int DigitalLeft = 200;
        private const int DigitalTop = 40;
        private const int DigitalWidth = 400;
        private const int DigitalHeight = 144;
        private const int CloseBtnLeft = BitmapWidth - 37;
        private const int CloseBtnTop = 37;
        private const int CloseBtnRadius = 27;
        private const int CloseBtnLine = 15;
        private const int CloseBtnLineWidth = 6;
        private const int BaseSize = 720;
        private const int ValueSize = 100;
        private const int ValueRadius = 292;
        private const int ValueSize2 = 88;
        private const int ValueRadius2 = 188;
        private const string ClkFontName = "ＭＳ　ゴシック";
        private const int ClkFontSise = 48;
        private const int ClkFontSise2 = 36;
        private const string DigitalFontName = "ＭＳ　ゴシック";
        private const int DigitalFontSise = 80;
        private const int LineWidth = 8;
        private const string ClkFormat = "D2";
        private const string BrushesColor_BG = "#AAAAFF";
        private const string BrushesColor_BASE = "#4444AA";
        private const string BrushesColor_CELL = "#6666CC";
        private const string BrushesColor_SCELL = "#FFFFFF";
        private const string BrushesColor_RCELL = "#AAAADD";
        private const string BrushesColor_CLOSE = "#FFAAAA";
        private const string BrushesColor_SCLOSE = "#FF0000";
        private const string PensColor_SLINE = "#FFFFFF";
        private const string PensColor_RLINE = "#AAAADD";
        private const string PensColor_CLOSE = "#FF0000";
        private const string PensColor_SCLOSE = "#FFDDDD";

        // 変数定義
        private int X,Y;
        private bool Clicked;
        private eMode Mode;
        private PointF DrawScale;
        private Point Center,CloseBtn;
        private Rectangle DigitalRect;
        private SolidBrush[] Brushes;
        private Pen[] Pens;
        private Bitmap Bmp;
        private Graphics Gp;
        private StringFormat Format;
        private Font ClkFont,ClkFont2;

        // プロパティ定義
        public int Hour { get; private set; }
        public int Minute { get; private set; }
        public new string Text { get; set; }

        // コンストラクタ
        public TimePicker() : base()
        {
            Init();
        }

        // コンストラクタ（位置指定）
        public TimePicker(int left, int top) : base() {
            Location = new Point(left, top);
            Init();
        }

        // コンストラクタ（位置・サイズ指定）
        public TimePicker(int left, int top, int width, int height) : base()
        {
            Location = new Point(left, top);
            Init(width, height);
        }

        // 初期化処理
        private void Init()
        {
            Init(DefaultWidth, DefaultHeight);
        }

        // 初期化処理（サイズ指定）
        private void Init(int width, int height)
        {
            // プロパティ初期化
            Hour = 0;
            Minute = 0;
            Text = "";
            // 変数初期化
            X = 0;
            Y = 0;
            Center = new Point(CenterLeft, CenterTop); // 原点
            DigitalRect = new Rectangle(DigitalLeft, DigitalTop, DigitalWidth, DigitalHeight); //デジタル部矩形
            CloseBtn = new Point(CloseBtnLeft, CloseBtnTop);
            Brushes = new SolidBrush[] // ブラシをあらかじめ作成しておく
            {
                new SolidBrush(ColorTranslator.FromHtml(BrushesColor_BG)),
                new SolidBrush(ColorTranslator.FromHtml(BrushesColor_BASE)),
                new SolidBrush(ColorTranslator.FromHtml(BrushesColor_CELL)),
                new SolidBrush(ColorTranslator.FromHtml(BrushesColor_SCELL)),
                new SolidBrush(ColorTranslator.FromHtml(BrushesColor_RCELL)),
                new SolidBrush(ColorTranslator.FromHtml(BrushesColor_CLOSE)),
                new SolidBrush(ColorTranslator.FromHtml(BrushesColor_SCLOSE))
            };
            Pens = new Pen[] // ペンをあらかじめ作成しておく
            {
                new Pen(ColorTranslator.FromHtml(PensColor_SLINE)),
                new Pen(ColorTranslator.FromHtml(PensColor_RLINE)),
                new Pen(ColorTranslator.FromHtml(PensColor_CLOSE)),
                new Pen(ColorTranslator.FromHtml(PensColor_SCLOSE))
            };
            Pens[(int)ePen.SLINE].Width = LineWidth;
            Pens[(int)ePen.RLINE].Width = LineWidth;
            Pens[(int)ePen.CLOSE].Width = CloseBtnLineWidth;
            Pens[(int)ePen.SCLOSE].Width = CloseBtnLineWidth;
            Bmp = new Bitmap(BitmapWidth, BitmapHeight); // レンダリング用ビットマップオブジェクト
            Gp = Graphics.FromImage(Bmp); // ビットマップからGraphicオブジェクトを作成しておく
            Format = new StringFormat(); // DrawStringに指定する文字配置
            Format.Alignment = StringAlignment.Center;
            Format.LineAlignment = StringAlignment.Center;
            ClkFont = new Font(ClkFontName, ClkFontSise, FontStyle.Bold); // 時計盤用フォント（外側）
            ClkFont2 = new Font(ClkFontName, ClkFontSise2, FontStyle.Regular); // 時計盤用フォント（内側）
            DrawScale = GetScale(width, height, BitmapWidth, BitmapHeight); // UIとビットマップの拡大率
            Mode = eMode.Hour; // 時間入力モードに設定
            // PictureBox初期化
            Size = new Size(width, height);
            BorderStyle = BorderStyle.None;
            BackColor = Color.Transparent;
            Font = new Font(DigitalFontName,DigitalFontSise, FontStyle.Bold);
            SizeMode = PictureBoxSizeMode.StretchImage;
            // イベントハンドラ登録
            Paint += TimePicker_Paint;
            MouseDown += TimePicker_MouseDown;
            MouseUp += TimePicker_MouseUp;
            MouseMove += TimePicker_MouseMove;
            MouseLeave += TimePicker_MouseLeave;
        }

        // オーナードロー(独自描画)処理
        private void TimePicker_Paint(object sender, PaintEventArgs e)
        {
            // 変数定義
            int r,s,f = BaseSize;
            double rad,d;
            Font fnt;
            Point a,c = Center;
            Graphics g = Gp;
            Size sz = Bmp.Size;
            List<object[]> cell = new List<object[]>();
            SolidBrush bh, bm,b = Brushes[(int)eBrush.BG];
            Pen p;
            Rectangle dr = DigitalRect;

            // 背景
            g.FillRectangle(b, 0,BGRadius, sz.Width, sz.Height - BGRadiusx2);
            g.FillRectangle(b, BGRadius, 0, sz.Width - BGRadiusx2, sz.Height);
            g.FillPie(b, 0, 0, BGRadiusx2, BGRadiusx2, BGLeftTopDeg, 90);
            g.FillPie(b, sz.Width - BGRadiusx2, 0, BGRadiusx2, BGRadiusx2, BGRightTopDeg, 90);
            g.FillPie(b, 0, sz.Height - BGRadiusx2, BGRadiusx2, BGRadiusx2, BGLeftBottomDeg, 90);
            g.FillPie(b, sz.Width - BGRadiusx2, sz.Height - BGRadiusx2, BGRadiusx2, BGRadiusx2, BGRrightBottomDeg, 90);
            // 閉じるボタン
            d = GetDistance(X, Y,CloseBtn.X,CloseBtn.Y);
            if (d <= CloseBtnRadius)
            {
                b = Brushes[(int)eBrush.SCLOSE];
                p = Pens[(int)ePen.SCLOSE];
            }
            else
            {
                b = Brushes[(int)eBrush.CLOSE];
                p = Pens[(int)ePen.CLOSE];
            }
            a = CloseBtn;
            r = CloseBtnRadius;
            g.FillPie(b, a.X - CloseBtnRadius,a.Y - r, r * 2, r * 2, 0, 360);
            r = CloseBtnLine;
            g.DrawLine(p, a.X - r, a.Y - r, a.X + r, a.Y + r);
            g.DrawLine(p, a.X - r, a.Y + r, a.X + r, a.Y - r);
            // アナログ部
            g.FillPie(Brushes[(int)eBrush.BASE], c.X - f / 2, c.Y - f / 2, f, f, 0, 360);
            if (Mode == 0) {
                // 時間入力モード
                for (int i = 23; i >= 0; i--)
                {
                    if (i < 12)
                    {
                        // 0-11時
                        r = ValueRadius;
                        s = ValueSize;
                        fnt = ClkFont;
                    }
                    else
                    {
                        // 12-23時
                        r = ValueRadius2;
                        s = ValueSize2;
                        fnt = ClkFont2;
                    }
                    rad = Rad((i % 12) * 30 - 90); // 時計盤に表示する時間の角度を取得
                    a = GetArcPos(rad, r, c.X, c.Y); // 時計盤に表示する時間の座標を取得
                    d = GetDistance(X, Y, a.X, a.Y); //マウスカーソルと時間の座標の距離を求める
                    if (d <= s / 2) // 時間表示の円の内側にマウスカーソルがあるとき
                    {
                        // 選択カーソルを追加（マウス追随）
                        cell.Add(new object[]
                        {
                            rad,r,s,fnt,Brushes[(int)eBrush.SCELL],Pens[(int)ePen.SLINE],i.ToString(ClkFormat)
                        });
                        // クリックされていたら現在の時間を選択する
                        if (Clicked)
                        {
                            Hour = i;
                            SetText();
                        }
                    }
                    else if (i == Hour) // 表示時間が選択時間と同じ場合
                    {
                        // 選択カーソルを追加（選択済時間表示）
                        cell.Add(new object[]
                        {
                            rad,r,s,fnt,Brushes[(int)eBrush.RCELL],Pens[(int)ePen.RLINE],i.ToString(ClkFormat)
                        });
                    }
                    // 時間の描画
                    g.FillPie(Brushes[(int)eBrush.CELL], a.X - s / 2, a.Y - s / 2, s, s, 0, 360);
                    g.DrawString(i.ToString(ClkFormat), fnt, Brushes[(int)eBrush.BASE], a.X, a.Y, Format);
                }
                // デジタル切り替え用ブラシを設定
                bh = Brushes[(int)eBrush.SCELL];
                bm = Brushes[(int)eBrush.RCELL];
            }
            else
            {
                // 分入力モード
                for (int i = 0; i < 60 ; i++)
                {
                    rad = Rad(i * 6 - 90);
                    if(i % 5 == 0){
                        a = GetArcPos(rad, ValueRadius, c.X, c.Y);
                        g.DrawString(i.ToString(ClkFormat), ClkFont, Brushes[(int)eBrush.BG], a.X, a.Y, Format);
                    }
                }
                // 選択カーソルを追加（選択済分表示）
                cell.Add(new object[]
                {
                    Rad(Minute * 6 - 90),ValueRadius,ValueSize,ClkFont,
                    Brushes[(int)eBrush.RCELL],Pens[(int)ePen.RLINE],Minute.ToString(ClkFormat)
                });
                d = GetDistance(X, Y, c.X, c.Y); // マウスカーソルと中心座標の距離を求める
                if (d <= f / 2) // マウスカーソルが時計盤の中にあるとき
                {
                    rad = Math.Atan2(Y - c.Y, X - c.X); // 中心座標とマウスカーソルの相対角度を取得
                    int min = Rad2Minute(rad); // 角度(ラジアン）から分を取得
                    // 選択カーソルを追加（マウス追随）
                    cell.Add(new object[]
                    {
                        rad,ValueRadius,ValueSize,ClkFont,
                        Brushes[(int)eBrush.SCELL],Pens[(int)ePen.SLINE],min.ToString(ClkFormat)
                    });
                    // クリックされていたら現在の分を選択する
                    if (Clicked)
                    {
                        Minute = min;
                        SetText();
                    }
                }
                // デジタル切り替え用ブラシを設定
                bh = Brushes[(int)eBrush.RCELL];
                bm = Brushes[(int)eBrush.SCELL];
            }
            // 選択カーソル表示
            foreach(object[] o in cell)
            {
                a = GetArcPos((double)o[(int)eCell.Radian], (int)o[(int)eCell.Radius], c.X, c.Y);
                g.DrawLine((Pen)o[(int)eCell.Pen], c.X, c.Y, a.X, a.Y);
                g.FillPie((SolidBrush)o[(int)eCell.Brush], c.X - 16, c.Y - 16, 32, 32, 0, 360);
                s = (int)o[(int)eCell.Size];
                g.FillPie((SolidBrush)o[(int)eCell.Brush], a.X - s / 2, a.Y - s / 2, s, s, 0, 360);
                g.DrawString((string)o[(int)eCell.Value], (Font)o[(int)eCell.Font], Brushes[(int)eBrush.BASE], a.X, a.Y, Format);
            }
            // デジタル部
            g.FillRectangle(Brushes[(int)eBrush.BASE], dr.Left, dr.Top, dr.Width, dr.Height);
            g.DrawString(Hour.ToString(ClkFormat), Font, bh, c.X - 88, 112, Format);
            g.DrawString(":", Font, Brushes[(int)eBrush.CELL], c.X, 104, Format);
            g.DrawString(Minute.ToString(ClkFormat), Font, bm, c.X + 88, 112, Format);
            Image = Bmp;
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
            Text = Hour.ToString(ClkFormat) + ":" + Minute.ToString(ClkFormat);
        }

        // 矩形の内部にマウスカーソルがあるか判定する
        private bool ChkInRect(Rectangle r)
        {
            bool rx = X > r.Left && X < r.Right;
            bool ry = Y > r.Top && Y < r.Bottom;
            return (rx && ry);
        }

        // コントロール上でマウスカーソルが移動したとき
        private void TimePicker_MouseMove(object sender, MouseEventArgs e)
        {
            X = (int)((float)e.X * DrawScale.X);
            Y = (int)((float)e.Y * DrawScale.Y);
            Invalidate();
        }

        // マウスボタンが離されたとき
        private void TimePicker_MouseUp(object sender, MouseEventArgs e)
        {
            Clicked = false;
        }

        // マウスボタンが押されたとき
        private void TimePicker_MouseDown(object sender, MouseEventArgs e)
        {
            Clicked = true;
            if (ChkInRect(DigitalRect)){
                if (X < Center.X)
                {
                    Mode = eMode.Hour;
                }
                else
                {
                    Mode = eMode.Minute;
                }
                Invalidate();
            }
            else if (GetDistance(X,Y,CloseBtn.X,CloseBtn.Y) < CloseBtnRadius)
            {
                Visible = false;
            }
        }

        // マウスカーソルがコントロールから離れたとき
        private void TimePicker_MouseLeave(object sender, EventArgs e)
        {
            Invalidate();
        }

    }
}

