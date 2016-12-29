using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Alpha.Quant.CustomControls.Controls
{
    public class FoldPanel : ContentControl
    {

        #region DependencyProperty Event
        public static readonly DependencyProperty DefaultWidthProperty = DependencyProperty.Register("DefaultWidth", typeof(double), typeof(FoldPanel));
        public static readonly DependencyProperty DefaultHeightProperty = DependencyProperty.Register("DefaultHeight", typeof(double), typeof(FoldPanel));
        /// <summary>
        /// 是否折叠了
        /// </summary>
        public static readonly DependencyProperty IsFoldedProperty = DependencyProperty.Register("IsFolded", typeof(bool), typeof(FoldPanel),
            new PropertyMetadata(true, IsFoldedChange));

        /// <summary>
        /// 折叠方向,
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(FoldOrientation), typeof(FoldPanel),
            new PropertyMetadata(FoldOrientation.Horizontal));

        public static readonly RoutedEvent IsFoldedChangedEvent =
            EventManager.RegisterRoutedEvent("IsFoldedChanged", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(FoldPanel));

        #endregion

        #region property event
        public event RoutedEventHandler IsFoldedChanged
        {
            add { this.AddHandler(IsFoldedChangedEvent, value); }
            remove { this.RemoveHandler(IsFoldedChangedEvent, value); }
        }


        /// <summary>
        /// 是否已经折叠内容
        /// </summary>
        public bool IsFolded
        {
            get { return (bool)this.GetValue(IsFoldedProperty); }
            set { this.SetValue(IsFoldedProperty, value); }
        }

        /// <summary>
        /// Width
        /// </summary>
        public double DefaultWidth
        {
            get { return (double)this.GetValue(DefaultWidthProperty); }
            set { this.SetValue(DefaultWidthProperty, value); }
        }

        /// <summary>
        /// Height
        /// </summary>
        public double DefaultHeight
        {
            get { return (double)this.GetValue(DefaultHeightProperty); }
            set { this.SetValue(DefaultHeightProperty, value); }
        }

        /// <summary>
        /// 折叠方向 默认折叠方向是Horizontal
        /// </summary>
        public FoldOrientation Orientation
        {
            get { return (FoldOrientation)this.GetValue(OrientationProperty); }
            set { this.SetValue(OrientationProperty, value); }
        }

        #endregion

        /// <summary>
        /// IsFoldedChange
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void IsFoldedChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (FoldPanel)d;
            var orientation = panel.Orientation;
            StartAnimation(panel, orientation, (bool)e.NewValue);
            panel.RaiseEvent(new RoutedEventArgs(IsFoldedChangedEvent));
        }

        /// <summary>
        /// 动画处理
        /// </summary>
        /// <param name="d"></param>
        /// <param name="orientation"></param>
        /// <param name="IsFolded"></param>
        private static void StartAnimation(FoldPanel d, FoldOrientation orientation, bool IsFolded)
        {

            DoubleAnimation Anima = new DoubleAnimation();
            if (orientation == FoldOrientation.Horizontal)
            {
                if (IsFolded)
                {
                    Anima.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };
                    Anima.Duration = new Duration(new TimeSpan(0, 0, 0, 0, (int)(d.Width)));
                    Anima.From = d.Width;
                    Anima.To = 0;
                }
                else
                {

                    Anima.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                    Anima.Duration = new Duration(new TimeSpan(0, 0, 0, 0, (int)(d.DefaultWidth)));
                    Anima.From = 0;
                    Anima.To = d.DefaultWidth;
                }
                d.BeginAnimation(FrameworkElement.WidthProperty, Anima);
            }
            else
            {
                if (IsFolded)
                {
                    Anima.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };
                    Anima.Duration = new Duration(new TimeSpan(0, 0, 0, 0, (int)(d.Height)));
                    Anima.From = d.Height;
                    Anima.To = 0;
                }
                else
                {
                    Anima.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                    Anima.Duration = new Duration(new TimeSpan(0, 0, 0, 0, (int)(d.DefaultHeight)));
                    Anima.From = 0;
                    Anima.To = d.DefaultHeight;
                }
                d.BeginAnimation(FrameworkElement.HeightProperty, Anima);
            }
        }
    }


    /// <summary>
    /// 折叠方向
    /// </summary>
    public enum FoldOrientation
    {
        /// <summary>
        /// 水平折叠
        /// </summary>
        Horizontal,
        /// <summary>
        /// 垂直折叠
        /// </summary>
        Vertical
    }
}
