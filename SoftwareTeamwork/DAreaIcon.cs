﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Drawing = System.Drawing;

namespace SoftwareTeamwork
{
    public class DAreaIcon : UIElement, IDisposable
    {
        //DllImport
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);
        private IntPtr ico = IntPtr.Zero;

        //NormalProperties
        private System.Timers.Timer PopupHidetimer;
        private System.Windows.Forms.NotifyIcon FlowIcon;

        private PrivateFontCollection pfc;
        private Font DisIconFont;
        private DPopup flowIconPopup = null;
        public DPopup FlowIconPopup {
            get => flowIconPopup;
            set {
                flowIconPopup = value;
            }
        }

        //.NetProperties

        private float fontsize = 13.2f;
        public float Fontsize
        {
            get { return fontsize; }
            set
            {
                fontsize = value;
                
            }
        }

        #region AttachedWindow
        public object AttachedWindow {
            get { return (object)GetValue(AttachedWindowProperty); }
            set { SetValue(AttachedWindowProperty, value); }
        }
        public static readonly DependencyProperty AttachedWindowProperty =
            DependencyProperty.Register("AttachedWindow", typeof(object), typeof(DAreaIcon),
                new PropertyMetadata(null,new PropertyChangedCallback(OnAttachedWindowChanged)));
        private static void OnAttachedWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DAreaIcon dai = (DAreaIcon)d;
            dai.Dcontextmenu.CommandParameter = e.NewValue;
        }
        #endregion

        #region AreaVisibility
        public bool AreaVisibility {
            get { return (bool)GetValue(AreaVisibilityProperty); }
            set { SetValue(AreaVisibilityProperty, value); }
        }
        public static readonly DependencyProperty AreaVisibilityProperty =
            DependencyProperty.Register("AreaVisibility", typeof(bool), typeof(DAreaIcon),
                new PropertyMetadata(false,new PropertyChangedCallback(OnAreaVisibilityChanged)));
        private static void OnAreaVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DAreaIcon dai = (DAreaIcon)d;
            dai.FlowIcon.Visible = (bool)e.NewValue;
        }
        #endregion

        #region Dcontextmenu
        public DContextMenu Dcontextmenu {
            get { return (DContextMenu)GetValue(DcontextmenuProperty); }
            set { SetValue(DcontextmenuProperty, value); }
        }
        public static readonly DependencyProperty DcontextmenuProperty =
            DependencyProperty.Register("Dcontextmenu", typeof(DContextMenu), typeof(DAreaIcon), 
                new PropertyMetadata(null,new PropertyChangedCallback(OnDcontextmenuChanged)));
        private static void OnDcontextmenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region 初始化
        private void InitNotifyIcon()
        {
            InitRes();
            FlowIcon = new System.Windows.Forms.NotifyIcon
            {
                Visible = AreaVisibility,
                ContextMenu = new System.Windows.Forms.ContextMenu()
            };
            UpdataIconByStr("0");
            FlowIcon.MouseClick += FlowIcon_MouseClick;
            FlowIcon.MouseMove += FlowIcon_MouseMove;
            FlowIcon.MouseDoubleClick += FlowIcon_MouseDoubleClick;
            InitPopup();
        }

        private void InitRes()
        {
            byte[] myText = (byte[])Properties.Resources.ResourceManager.GetObject("Rect");
            pfc = new PrivateFontCollection();
            IntPtr MeAdd = Marshal.AllocHGlobal(myText.Length);
            Marshal.Copy(myText, 0, MeAdd, myText.Length);
            pfc.AddMemoryFont(MeAdd, myText.Length);
            DisIconFont = new Font(pfc.Families[0], fontsize);
        }

        private void InitPopup()
        {
            FlowIconPopup = new DPopup {
                PlacementRectangle = new Rect(SystemParameters.WorkArea.Width - 305, SystemParameters.WorkArea.Height - 85,
                0, 0),
                Child = (Border)Application.Current.FindResource("FlowContentPanel")
            };
            FlowIconPopup.MouseMove += FlowIconPopup_MouseMove;
            FlowIconPopup.MouseLeave += FlowIconPopup_MouseLeave;
        }
        #endregion

        #region 计时器方法

        private void InitTimers()
        {
            //图标弹框淡出计时
            PopupHidetimer = new System.Timers.Timer(3000);
            PopupHidetimer.Elapsed += new ElapsedEventHandler(HidePopup);
        }

        private void HidePopup(object sender, ElapsedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, (ThreadStart)delegate ()
            {
                FlowIconPopup.HidePopupAni();
            });
            PopupHidetimer.Enabled = false;
        }
        #endregion

        #region 事件方法

        private void FlowIconPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            PopupHidetimer.Enabled = true;
        }

        private void FlowIconPopup_MouseMove(object sender, MouseEventArgs e)
        {
            PopupHidetimer.Enabled = false;
        }

        private void FlowIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (e.Button)
            {
                case System.Windows.Forms.MouseButtons.Right:
                    Dcontextmenu.IsOpen = true;
                    break;
                case System.Windows.Forms.MouseButtons.Left:
                    FlowIconPopup.Child = null;
                    FlowIconPopup.Child = (Border)Application.Current.FindResource("FlowContentPanel");
                    if (!FlowIconPopup.IsOpen)
                        FlowIconPopup.ShowPopupAni();
                    PopupHidetimer.Enabled = true;
                    break;
            }
        }

        private void FlowIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {


        }

        private void FlowIcon_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {

        }
        #endregion

        #region GraphUpdata
        //托盘画图
        private Icon GetImageSourceByText(String Inf)
        {
            Drawing.Image bufferedimage;
            if (ico == IntPtr.Zero)
                bufferedimage = new Bitmap(35, 30, Drawing.Imaging.PixelFormat.Format32bppArgb);
            else
                bufferedimage = Bitmap.FromHicon(ico);

            Graphics g = Graphics.FromImage(bufferedimage);
            g.Clear(Drawing.Color.FromArgb(0, 255, 255, 255));
            g.SmoothingMode = SmoothingMode.HighSpeed;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;
            Drawing.Pen pen = new Drawing.Pen(Drawing.Color.FromArgb(255, 255, 255, 255), 1f);
            g.DrawString(Inf, DisIconFont, pen.Brush, new Drawing.Point(0,1),new StringFormat() { });
            ico = (bufferedimage as Bitmap).GetHicon();

            g.Dispose();
            return Icon.FromHandle(ico);
        }
        #endregion

        #region 公共方法
        public void UpdataIconByStr(String s)
        {
            FlowIcon.Icon = GetImageSourceByText(s);
        }


        #endregion

        //构造函数
        //public DAreaIcon(AIWindow a)
        //{
        //    AttachedWindow = a;
        //    OverallSettingManger.Instence.ThemeChanged += Instence_ThemeChanged;
        //    InitNotifyIcon();
        //    InitTimers();
        //}

        public DAreaIcon()
        {
            OverallSettingManger.Instence.ThemeChanged += Instence_ThemeChanged;
            InitNotifyIcon();
            InitTimers();
        }

        private void Instence_ThemeChanged(object sender, EventArgs e)
        {

        }

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: 释放托管状态(托管对象)。
                }
                FlowIcon.Dispose();
                PopupHidetimer.Dispose();
                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
