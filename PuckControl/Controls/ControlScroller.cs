using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PuckControl.Controls
{
    public class ControlScroller : ScrollViewer
    {
        //private StackPanel _content;
        //public StackPanel Content
        //{
        //    get
        //    {
        //        return _content;
        //    }
        //    set
        //    {
        //        _content = value;
        //        base.Content = value;
        //    }
        //}

        private Button LeftButton;
        private Button RightButton;
        const double SCROLL_AMOUNT = 200;
        const double ANIMATION_SPEED = 300;

        static ControlScroller()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ControlScroller), new FrameworkPropertyMetadata(typeof(ControlScroller)));
        }

        public ControlScroller()
        {

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            LeftButton = (Button)GetTemplateChild("Button_Left");
            RightButton = (Button)GetTemplateChild("Button_Right");

            LeftButton.Click +=LeftButton_Click;
            RightButton.Click +=RightButton_Click;
            this.LayoutUpdated += ControlScroller_LayoutUpdated;
        }

        void ControlScroller_LayoutUpdated(object sender, EventArgs e)
        {
            LeftButton.Visibility = this.ComputedHorizontalScrollBarVisibility;
            RightButton.Visibility = this.ComputedHorizontalScrollBarVisibility;
        }

        
        void RightButton_Click(object sender, RoutedEventArgs e)
        {
            double startOffset = base.HorizontalOffset;
            double destinationOffset = base.HorizontalOffset + SCROLL_AMOUNT;

            if (destinationOffset > base.ScrollableWidth)
                destinationOffset = base.ScrollableWidth;

            ScrollContent(destinationOffset);
        }

        void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            double startOffset = base.HorizontalOffset;
            double destinationOffset = base.HorizontalOffset - SCROLL_AMOUNT;

            if (destinationOffset < 0)
                destinationOffset = 0;

            ScrollContent(destinationOffset);
        }

        private void ScrollContent(double destinationOffset)
        {
            double distance = destinationOffset - base.HorizontalOffset;
            double animationTime = Math.Abs(distance / ANIMATION_SPEED);
            double startOffset = base.HorizontalOffset;
            
            DateTime startTime = DateTime.Now;

            EventHandler renderHandler = null;

            renderHandler = (s, args) =>
            {
                double elapsed = (DateTime.Now - startTime).TotalSeconds;

                if (elapsed >= animationTime)
                {
                    base.ScrollToHorizontalOffset(destinationOffset);
                    CompositionTarget.Rendering -= renderHandler;
                }

                double scrollTo = (elapsed * ANIMATION_SPEED);

                if (destinationOffset < startOffset)
                    scrollTo *= -1;

                base.ScrollToHorizontalOffset(startOffset + scrollTo);
            };

            CompositionTarget.Rendering += renderHandler;
        }
    }
}
