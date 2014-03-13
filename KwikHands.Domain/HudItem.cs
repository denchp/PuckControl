namespace KwikHands.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Timers;
    using KwikHands.Domain.EventArg;

    public class HudItem
    {
        public event EventHandler<HudItemEventArgs> MaximumTriggerReached;
        public event EventHandler<HudItemEventArgs> MinimumTriggerReached;

        private Timer _timer = new Timer();

        public enum HudItemType
        {
            Numeric, Text, Timer
        }

        public enum TimerType
        {
            Up, Down
        }

        public enum HudItemStyle
        {
            Circle, Progress, Text
        }

        public enum HorizontalAlignment
        {
            Left, Center, Right
        }

        public enum VerticalAlignment
        {
            Top, Middle, Bottom
        }

        public HudItemType Type { get; set; }
        public TimerType TimerOption { get; set; }
        public HudItemStyle Style { get; set; }
        public HorizontalAlignment HorizontalPosition { get; set; }
        public VerticalAlignment VerticalPosition { get; set; }

        public String Name { get; set; }
        private Int16 _size;
        public Int16 Size { get { return _size; } set { _size = value; Changed = true; } }

        private bool _visible;
        public bool Visible { get { return _visible; } set { _visible = value; Changed = true; } }
        public Int16 Rotation { get; set; }

        private Int32 _value;
        public Int32 Value { get { return _value; } set { _value = value; Changed = true; } }

        public Int32 DefaultValue { get; set; }
        public Int32 MinValue { get; set; }
        public Int32 MaxValue { get; set; }
        
        public bool MinumumTrigger { get; set; }
        public bool MaximumTrigger { get; set; }
                
        public String DefaultText { get; set; }

        public String _text;
        public String Text { get { return _text; } set { _text = value; Changed = true; } }

        public String Label { get; set; }
        public bool Changed { get; private set; }

        public void StartTimer()
        {
            _timer.Interval = 1000;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this.TimerOption == TimerType.Up)
                this.Value += 1; 
        }

        public HudItem()
        {
            Visible = false;
            Size = 1;
        }

        public void Reset()
        {
            this.Value = DefaultValue;
            this.Text = DefaultText;
        }
    }
}
