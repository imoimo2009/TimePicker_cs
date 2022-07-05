using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;

namespace TimePicker
{
    class TimePicker : PictureBox
    {
        // 時間・分モード切替用列挙体
        enum eMode
        {
            Hour,       // 時間入力モード
            Minute      // 分入力モード
        }

        // ブラシインデックスの列挙体
        enum eBrush
        {
            BG,         // バックグラウンド
            BASE,       // 時計盤
            CELL,       // 時間表示円
            SCELL,      // マウスオーバー時
            RCELL,      // 選択項目
            CLOSE,      // 閉じるボタン
            SCLOSE      // 閉じるボタンマウスオーバー時
        }

        // ペンインデックスの列挙体
        enum ePen
        {
            SLINE,      // マウスオーバー時
            RLINE,      // 選択項目
            CLOSE,      // 閉じるボタン
            SCLOSE      // 閉じるボタンマウスオーバー時
        }

        // 選択カーソルパラメータインデックスの列挙体
        enum eCursol
        {
            Point,      // 座標
            Size,       // サイズ
            Font,       // フォント
            Brush,      // ブラシ
            Pen,        // ペン
            Value       // 表示内容
        }

        // 定数定義
        private const int DefaultWidth = 200;                       // デフォルトの幅
        private const int DefaultHeight = 240;                      // デフォルトの高さ
        private const string DefaultText = "00:00";                 // デフォルトのTextプロパティの値
        private const int BitmapWidth = 800;                        // ビットマップの幅
        private const int BitmapHeight = 960;                       // ビットマップの高さ
        private const int CaptionHeight = 79;                       // キャプションの高さ
        private const int CaptionStrVOffset = 4;                    // キャプション文字列の垂直オフセット
        private const int CloseBtnSize = 79;                        // 閉じるボタンのサイズ
        private const int CloseBtnLeft = BitmapWidth - CloseBtnSize;// 閉じるボタンの位置X
        private const int CloseBtnTop = 0;                          // 閉じるボタンの位置Y
        private const float CloseBtnLine = 75F;                     // 閉じるボタンＸの描画開始位置(%)
        private const int CloseBtnLineWidth = 6;                    // 閉じるボタンＸの太さ
        private const int CenterLeft = 400;                         // 時計盤の原点X
        private const int CenterTop = 580;                          // 時計盤の原点Y
        private const int CenterRadius = 16;                        // 時計盤中心点の半径
        private const int BaseRadius = 360;                         // 時計盤の半径
        private const int ValueSize = 50;                           // 時間表示のサイズ(外側)
        private const int ValueRadius = 292;                        // 時間表示の原点からの距離(外側)
        private const int ValueSize2 = 44;                          // 時間表示のサイズ(内側)
        private const int ValueRadius2 = 188;                       // 時間表示の原点からの距離(内側)
        private const string CaptionFontName = "ＭＳ　ゴシック";     // キャプションのフォント名称 
        private const int CaptionFontSise = 48;                     // キャプションのフォントサイズ
        private const string ClkFontName = "ＭＳ　ゴシック";         // 時間表示のフォント名称 
        private const int ClkFontSise = 48;                         // 時間表示のフォントサイズ(外側)
        private const int ClkFontSise2 = 36;                        // 時間表示のフォントサイズ(内側)
        private const int ClkLineWidth = 8;                         // 時計盤の針の太さ
        private const string ClkFormat = "D2";                      // 時計盤のフォーマット文字列
        private const int DigitalLeft = 220;                        // デジタル矩形部の位置X
        private const int DigitalTop = 90;                          // デジタル矩形部の位置Y
        private const int DigitalWidth = 360;                       // デジタル矩形部の幅
        private const int DigitalHeight = 120;                      // デジタル矩形部の高さ
        private const int DigitalDlmVOffset = 8;                    // デジタル区切り文字のオフセット
        private const int DigitalStrVOffset = 8;                    // デジタル文字列の垂直オフセット
        private const int DigitalStrHOffset = 84;                   // デジタル文字列の水平オフセット
        private const string DigitalDelimiter = ":";                // デジタル文字列の区切り文字
        private const string DigitalFontName = "ＭＳ　ゴシック";     // デジタル表示部のフォント名称
        private const int DigitalFontSise = 80;                     // デジタル表示部のフォントサイズ
        private const string BrushesColor_BG = "#AAAAFF";           // 背景の色(ブラシ)
        private const string BrushesColor_BASE = "#4444AA";         // 時計盤の色(ブラシ)
        private const string BrushesColor_CELL = "#6666CC";         // 時間表示の色(ブラシ)
        private const string BrushesColor_SCELL = "#FFFFFF";        // マウスオーバー時の色(ブラシ)
        private const string BrushesColor_RCELL = "#AAAADD";        // 選択項目の色(ブラシ)
        private const string BrushesColor_CLOSE = "#FFAAAA";        // 閉じるボタンの色(ブラシ)
        private const string BrushesColor_SCLOSE = "#FF0000";       // 閉じるボタンマウスオーバーの色(ブラシ)
        private const string PensColor_SLINE = "#FFFFFF";           // マウスオーバー時の色(ペン)
        private const string PensColor_RLINE = "#AAAADD";           // 選択項目の色(ペン)
        private const string PensColor_CLOSE = "#FF0000";           // 閉じるボタンの色(ペン)
        private const string PensColor_SCLOSE = "#FFDDDD";          // 閉じるボタンマウスオーバーの色(ペン)

        // 変数定義
        private int X, Y;                                           // マウス座標
        private bool Clicked;                                       // クリックの状態
        private eMode Mode;                                         // 入力モード
        private PointF DrawScale;                                   // 描画スケール
        private Point Center;                                       // 原点、閉じるボタンの位置
        private Rectangle DigitalRect, CaptionRect, CloseBtn;       // 矩形エリア（デジタル部、キャプション、閉じるボタン）
        private SolidBrush[] Brushes;                               // ブラシ格納用
        private Pen[] Pens;                                         // ペン格納用
        private Bitmap Bmp;                                         // ビットマップ
        private Graphics Gp;                                        // ビットマップ描画用グラフィックオブジェクト
        private StringFormat Format;                                // 文字列配置指定
        private Font CaptionFont, ClkFont, ClkFont2;                // 時間表示フォント(外側、内側)

        // プロパティ定義
        public int Hour { get; private set; }                       // 時間
        public int Minute { get; private set; }                     // 分
        public bool AutoNext { get; set; }                          // 自動切換モード
        public bool Afternoon { get; set; }                         // 午後モード
        public string Caption { get; set; }                         // キャプション

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TimePicker() : base()
        {
            Init();
        }

        /// <summary>
        /// コンストラクタ（位置指定）
        /// </summary>
        /// <param name="left">左端座標(X座標)</param>
        /// <param name="top">上端座標(Y座標)</param>
        public TimePicker(int left, int top) : base()
        {
            Location = new Point(left, top);
            Init();
        }
        /// <summary>
        /// コンストラクタ（位置・サイズ指定）
        /// </summary>
        /// <param name="left">左端座標(X座標)</param>
        /// <param name="top">上端座標(Y座標)</param>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        public TimePicker(int left, int top, int width, int height) : base()
        {
            Location = new Point(left, top);
            Init(width, height);
        }

        /// <summary>
        /// デストラクタ
        /// </summary>
        ~TimePicker()
        {
            foreach (SolidBrush b in Brushes)
            {
                b.Dispose();
            }
            foreach (Pen p in Pens)
            {
                p.Dispose();
            }
            Format.Dispose();
            Gp.Dispose();
            Bmp.Dispose();
            ClkFont.Dispose();
            ClkFont2.Dispose();
        }

        /// <summary>
        /// TimePickerを開く
        /// </summary>
        public void Open()
        {
            Visible = true;
        }

        /// <summary>
        /// TimePickerを閉じる
        /// </summary>
        public void Close()
        {
            Visible = false;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Init()
        {
            Init(DefaultWidth, DefaultHeight);
        }

        /// <summary>
        /// 初期化処理（サイズ指定）
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        private void Init(int width, int height)
        {
            // プロパティ初期化
            Hour = 0;
            Minute = 0;
            Text = DefaultText;
            Caption = "";
            // 変数初期化
            X = 0;
            Y = 0;
            Center = new Point(CenterLeft, CenterTop); // 原点
            CaptionRect = new Rectangle(0, 0, BitmapWidth, CaptionHeight); // キャプション表示部矩形
            DigitalRect = new Rectangle(DigitalLeft, DigitalTop, DigitalWidth, DigitalHeight); // デジタル部矩形
            CloseBtn = new Rectangle(CloseBtnLeft, CloseBtnTop,CloseBtnSize,CloseBtnSize); // 閉じるボタンの矩形
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
            GetPen(ePen.SLINE).Width = ClkLineWidth;
            GetPen(ePen.RLINE).Width = ClkLineWidth;
            GetPen(ePen.CLOSE).Width = CloseBtnLineWidth;
            GetPen(ePen.SCLOSE).Width = CloseBtnLineWidth;
            Bmp = new Bitmap(BitmapWidth, BitmapHeight); // レンダリング用ビットマップオブジェクト
            Gp = Graphics.FromImage(Bmp); // ビットマップからGraphicオブジェクトを作成しておく
            Format = new StringFormat(); // DrawStringに指定する文字配置
            Format.Alignment = StringAlignment.Center;
            Format.LineAlignment = StringAlignment.Center;
            CaptionFont = new Font(CaptionFontName, CaptionFontSise, FontStyle.Bold); // キャプション用フォント
            ClkFont = new Font(ClkFontName, ClkFontSise, FontStyle.Bold); // 時計盤用フォント（外側）
            ClkFont2 = new Font(ClkFontName, ClkFontSise2, FontStyle.Regular); // 時計盤用フォント（内側）
            DrawScale = GetScale(width, height, BitmapWidth, BitmapHeight); // UIとビットマップの拡大率
            Mode = eMode.Hour; // 時間入力モードに設定
            // PictureBox初期化
            Size = new Size(width, height);
            BorderStyle = BorderStyle.None;
            BackColor = Color.Transparent;
            Font = new Font(DigitalFontName, DigitalFontSise, FontStyle.Bold);
            SizeMode = PictureBoxSizeMode.StretchImage;
            // イベントハンドラ登録
            Paint += TimePicker_Paint;
            MouseDown += TimePicker_MouseDown;
            MouseUp += TimePicker_MouseUp;
            MouseMove += TimePicker_MouseMove;
            MouseLeave += TimePicker_MouseLeave;
            VisibleChanged += TimePicker_VisibleChanged;
            TextChanged += TimePicker_TextChanged;
        }

        /// <summary>
        /// [イベントハンドラ]
        /// Textプロパティが変更されたとき
        /// </summary>
        /// <param name="sender">イベント元オブジェクト</param>
        /// <param name="e">イベントパラメータ</param>
        private void TimePicker_TextChanged(object sender, EventArgs e)
        {
            RestoreValues();
        }

        /// <summary>
        /// [イベントハンドラ]
        /// コントロール上でマウスカーソルが移動したとき
        /// </summary>
        /// <param name="sender">イベント元オブジェクト</param>
        /// <param name="e">イベントパラメータ</param>
        private void TimePicker_MouseMove(object sender, MouseEventArgs e)
        {
            X = (int)((float)e.X * DrawScale.X);
            Y = (int)((float)e.Y * DrawScale.Y);
            Invalidate();
        }

        /// <summary>
        /// [イベントハンドラ]
        /// マウスボタンが離されたとき
        /// </summary>
        /// <param name="sender">イベント元オブジェクト</param>
        /// <param name="e">イベントパラメータ</param>
        private void TimePicker_MouseUp(object sender, MouseEventArgs e)
        {
            if (Clicked && AutoNext && ChkInCircle(Center, BaseRadius))
            {
                switch (Mode)
                {
                    case eMode.Hour:
                        SetMode(eMode.Minute);
                        Invalidate();
                        break;
                    case eMode.Minute:
                        Close();
                        break;
                }
            }
            Clicked = false;
        }

        /// <summary>
        /// [イベントハンドラ]
        /// マウスボタンが押されたとき
        /// </summary>
        /// <param name="sender">イベント元オブジェクト</param>
        /// <param name="e">イベントパラメータ</param>
        private void TimePicker_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Clicked = true;
                    if (ChkInRect(DigitalRect))
                    {
                        if (X < Center.X)
                        {
                            SetMode(eMode.Hour);
                        }
                        else
                        {
                            SetMode(eMode.Minute);
                        }
                    }
                    Invalidate();
                    if (ChkInRect(CloseBtn))
                    {
                        Close();
                    }
                    break;
                case MouseButtons.Right:
                    if (ChkInCircle(Center,BaseRadius))
                    {
                        SetMode((eMode)1 - (int)Mode);
                        Invalidate();
                    }
                    break;
            }
        }

        /// <summary>
        /// [イベントハンドラ]
        /// マウスカーソルがコントロールから離れたとき
        /// </summary>
        /// <param name="sender">イベント元オブジェクト</param>
        /// <param name="e">イベントパラメータ</param>
        private void TimePicker_MouseLeave(object sender, EventArgs e)
        {
            // 保持しているマウス情報をクリア
            X = 0;
            Y = 0;
            Clicked = false;
            Invalidate();
        }

        /// <summary>
        /// [イベントハンドラ]
        /// Visibleプロパティが変更されたとき
        /// </summary>
        /// <param name="sender">イベント元オブジェクト</param>
        /// <param name="e">イベントパラメータ</param>
        private void TimePicker_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                SetMode(eMode.Hour);
                Invalidate();
            }
        }

        /// <summary>
        /// [イベントハンドラ]
        /// オーナードロー(独自描画)処理
        /// </summary>
        /// <param name="sender">イベント元オブジェクト</param>
        /// <param name="e">イベントパラメータ</param>
        private void TimePicker_Paint(object sender, PaintEventArgs e)
        {
            // 変数定義
            int r;
            Rectangle t;
            SolidBrush b,bh,bm;
            Pen p;
            List<object[]> cursol;

            // 背景
            Gp.FillRectangle(GetBrush(eBrush.BG), 0, 0, Bmp.Width, Bmp.Height);
            // キャプション
            if(!Caption.Equals(""))
            {
                Gp.FillRectangle(GetBrush(eBrush.CELL),CaptionRect);
                r = CaptionHeight / 2 + CaptionStrVOffset;
                Gp.DrawString(Caption, CaptionFont, GetBrush(eBrush.SCELL), Center.X, r, Format);
            }
            // 閉じるボタン
            if (ChkInRect(CloseBtn))
            {
                b = GetBrush(eBrush.SCLOSE);
                p = GetPen(ePen.SCLOSE);
            }
            else
            {
                b = GetBrush(eBrush.CLOSE);
                p = GetPen(ePen.CLOSE);
            }
            t = CloseBtn;
            r = Convert.ToInt32(CloseBtnLine / 100 * CloseBtnSize);
            Gp.FillRectangle(b, t);
            Gp.DrawLine(p, t.Left + r, t.Top + r, t.Left + t.Width - r, t.Top + t.Height - r);
            Gp.DrawLine(p, t.Left + r, t.Top + t.Height - r, t.Left + t.Width - r, t.Top + r);
            // アナログ部
            r = BaseRadius;
            Gp.FillPie(GetBrush(eBrush.BASE), Center.X - r, Center.Y - r, r * 2, r * 2, 0, 360);
            if (Mode == eMode.Hour)
            {
                cursol = UpdateHour();
                // デジタル切り替え用ブラシを設定
                bh = GetBrush(eBrush.SCELL);
                bm = GetBrush(eBrush.RCELL);
            }
            else
            {
                cursol = UpdateMinute();
                // デジタル切り替え用ブラシを設定
                bh = GetBrush(eBrush.RCELL);
                bm = GetBrush(eBrush.SCELL);
            }
            UpdateCursol(cursol);
            // デジタル部
            r = DigitalRect.Top + DigitalRect.Height / 2 + DigitalStrVOffset;
            Gp.FillRectangle(GetBrush(eBrush.BASE), DigitalRect);
            Gp.DrawString(ClkStr(Hour), Font, bh, Center.X - DigitalStrHOffset, r, Format);
            Gp.DrawString(DigitalDelimiter, Font, GetBrush(eBrush.CELL), Center.X, r - DigitalDlmVOffset, Format);
            Gp.DrawString(ClkStr(Minute), Font, bm, Center.X + DigitalStrHOffset, r, Format);
            Image = Bmp;
        }

        /// <summary>
        /// 時間入力モード
        /// </summary>
        /// <returns>List&lt;object[]&gt; カーソルリスト</returns>
        private List<object[]> UpdateHour()
        {
            List<object[]> cursol = new List<object[]>();
            object[] obj = new object[0];

            for (int i = 23; i >= 0; i--)
            {
                int r, s;
                Font fnt;

                if ((i < 12) ^ Afternoon)
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
                double rad = Rad((i % 12) * 30 - 90); // 時計盤に表示する時間の角度を取得
                Point a = GetArcPos(rad, r); // 時計盤に表示する時間の座標を取得
                if (ChkInCircle(a, s)) // 時間表示の円の内側にマウスカーソルがあるとき
                {
                    // 選択カーソルを追加（マウス追随）
                    obj = new object[]
                    {
                            a,s,fnt,GetBrush(eBrush.SCELL),GetPen(ePen.SLINE),ClkStr(i)
                    };
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
                    cursol.Add(new object[]
                    {
                            a,s,fnt,GetBrush(eBrush.RCELL),GetPen(ePen.RLINE),ClkStr(i)
                    });
                }
                // 時間の描画
                Gp.FillPie(GetBrush(eBrush.CELL), a.X - s, a.Y - s, s * 2, s * 2, 0, 360);
                Gp.DrawString(ClkStr(i), fnt, GetBrush(eBrush.BASE), a, Format);
            }
            if (obj.Length > 0)
            {
                cursol.Add(obj);
            }
            return cursol;
        }

        /// <summary>
        /// 分入力モード
        /// </summary>
        /// <returns></returns>
        private List<object[]> UpdateMinute()
        {
            List<object[]> cursol = new List<object[]>();

            for (int i = 0; i < 60; i++)
            {
                Point a = GetArcPos(Rad(i * 6 - 90), ValueRadius);
                if (i % 5 == 0)
                {
                    Gp.DrawString(ClkStr(i), ClkFont, GetBrush(eBrush.BG), a, Format);
                }
                if (i == Minute) // 選択カーソルを追加（選択済分表示）
                {
                    cursol.Add(new object[]
                    {
                        a,ValueSize,ClkFont,GetBrush(eBrush.RCELL),GetPen(ePen.RLINE),ClkStr(i)
                    });

                }
            }
            if (ChkInCircle(Center, BaseRadius)) // マウスカーソルが時計盤の中にあるとき
            {
                double rad = Math.Atan2(Y - Center.Y, X - Center.X); // 中心座標とマウスカーソルの相対角度を取得
                Point a = GetArcPos(rad, ValueRadius);
                int min = Rad2Minute(rad); // 角度(ラジアン）から分を取得
                                           // 選択カーソルを追加（マウス追随）
                cursol.Add(new object[]
                {
                    a,ValueSize,ClkFont,GetBrush(eBrush.SCELL),GetPen(ePen.SLINE),ClkStr(min)
                });
                // クリックされていたら現在の分を選択する
                if (Clicked)
                {
                    Minute = min;
                    SetText();
                }
            }
            return cursol;
        }

        /// <summary>
        /// カーソル更新処理
        /// </summary>
        /// <param name="cursol">カーソルリスト</param>
        private void UpdateCursol(List<object[]> cursol)
        {
            foreach (object[] o in cursol)
            {
                Point a = (Point)o[(int)eCursol.Point];
                int s = (int)o[(int)eCursol.Size];
                SolidBrush b = (SolidBrush)o[(int)eCursol.Brush];
                Pen p = (Pen)o[(int)eCursol.Pen];
                Font fnt = (Font)o[(int)eCursol.Font];
                string v = (string)o[(int)eCursol.Value];
                int c = CenterRadius;

                Gp.DrawLine(p, Center, a);
                Gp.FillPie(b, Center.X - c, Center.Y - c, c * 2, c * 2, 0, 360);
                Gp.FillPie(b, a.X - s, a.Y - s, s * 2, s * 2, 0, 360);
                Gp.DrawString(v, fnt, GetBrush(eBrush.BASE), a, Format);
            }
        }

        /// <summary>
        /// 角度(degree)をラジアンに変換
        /// </summary>
        /// <param name="deg">角度(degree)</param>
        /// <returns>double 角度(radian)</returns>
        private double Rad(int deg)
        {
            return Math.PI / 180 * deg;
        }

        /// <summary>
        /// 指定角度の円弧座標を返す
        /// </summary>
        /// <param name="rad">角度(radian)</param>
        /// <param name="r">半径</param>
        /// <param name="x">中心X座標</param>
        /// <param name="y">中心Y座標</param>
        /// <returns>Point 円弧座標</returns>
        private Point GetArcPos(double rad, int r)
        {
            Point p = new Point();

            p.X = Convert.ToInt32(Math.Cos(rad) * r + Center.X);
            p.Y = Convert.ToInt32(Math.Sin(rad) * r + Center.Y);
            return p;
        }

        /// <summary>
        /// 2点間の距離を算出
        /// </summary>
        /// <param name="x1">点1のX座標</param>
        /// <param name="y1">点1のY座標</param>
        /// <param name="x2">点2のX座標</param>
        /// <param name="y2">点2のY座標</param>
        /// <returns>double 距離(pixel)</returns>
        private double GetDistance(int x1, int y1, int x2, int y2)
        {
            double xp = Math.Pow(Math.Abs(x2 - x1), 2);
            double yp = Math.Pow(Math.Abs(y2 - y1), 2);

            return Math.Sqrt(xp + yp);
        }

        /// <summary>
        /// ラジアンから分の数値を返す
        /// </summary>
        /// <param name="rad">角度(radian)</param>
        /// <returns>int 分</returns>
        private int Rad2Minute(double rad)
        {
            int m = Convert.ToInt32(rad / (Math.PI * 2) * 60 + 15);

            if (m < 0)
            {
                m += 60;
            }
            return m;
        }

        /// <summary>
        /// レンダリングサイズに対するUIサイズの比率を取得
        /// </summary>
        /// <param name="w1">矩形1の幅</param>
        /// <param name="h1">矩形1の幅</param>
        /// <param name="w2">矩形2の高さ</param>
        /// <param name="h2">矩形2の高さ</param>
        /// <returns>PointF UIサイズの比率</returns>
        private PointF GetScale(int w1, int h1, int w2, int h2)
        {
            PointF p = new PointF();

            p.X = (float)w2 / w1;
            p.Y = (float)h2 / h1;
            return p;
        }

        /// <summary>
        /// テキストプロパティを更新
        /// </summary>
        private void SetText()
        {
            string str = Hour.ToString(ClkFormat) + ":" + Minute.ToString(ClkFormat);

            if (!Text.Equals(str))
            {
                Text = str;
            }
        }

        /// <summary>
        /// モード切替
        /// </summary>
        /// <param name="mode">入力モード</param>
        private void SetMode(eMode mode)
        {
            Mode = mode;
        }

        /// <summary>
        /// 矩形の内部にマウスカーソルがあるか判定する
        /// </summary>
        /// <param name="r">矩形領域</param>
        /// <returns>true = 矩形の中, false = 矩形の外</returns>
        private bool ChkInRect(Rectangle r)
        {
            bool rx = X > r.Left && X < r.Right;
            bool ry = Y > r.Top && Y < r.Bottom;

            return (rx && ry);
        }

        /// <summary>
        /// 円領域の中にマウスカーソルがあるか判定する
        /// </summary>
        /// <param name="p">円の中心点</param>
        /// <param name="r">半径</param>
        /// <returns>true = 円の中, false = 円の外</returns>
        private bool ChkInCircle(Point p, int r)
        {
            return (GetDistance(X, Y, p.X, p.Y) < r);
        }

        /// <summary>
        /// ブラシ配列からブラシを取得
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private SolidBrush GetBrush(eBrush b)
        {
            return Brushes[(int)b];
        }

        /// <summary>
        /// ペン配列からペンを取得
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private Pen GetPen(ePen p)
        {
            return Pens[(int)p];
        }

        /// <summary>
        /// 時計に表示する文字列を返す
        /// </summary>
        /// <param name="c">数値</param>
        /// <returns>string 表示する文字</returns>
        private string ClkStr(int c)
        {
            return c.ToString(ClkFormat);
        }

        /// <summary>
        /// Textプロパティの値から時間、分の値をセットする
        /// </summary>
        private void RestoreValues()
        {
            Match m = Regex.Match(Text, "([0-9]+):([0-9]+)");
            bool ignore = true;

            if (m.Success)
            {
                DateTime buf;
                if(DateTime.TryParse(Text,out buf))
                {
                    Hour = Convert.ToInt32(m.Groups[1].Value);
                    Minute = Convert.ToInt32(m.Groups[2].Value);
                    ignore = false;
                    SetText();
                }
            }
            if (ignore)
            {
                Text = DefaultText;
            }
        }
    }
 }

