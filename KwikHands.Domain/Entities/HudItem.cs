namespace KwikHands.Domain.Entities
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
        public event EventHandler<HudItemEventArgs> Changed;
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

        private HudItemType _type;
        public HudItemType Type
        {
            get { return _type; }
            set
            {
                _type = value;
                if (Changed != null)
                    Changed(this, new HudItemEventArgs() { Item = this });
            }
        }
        public TimerType TimerOption { get; set; }
        public HudItemStyle Style { get; set; }
        public HorizontalAlignment HorizontalPosition { get; set; }
        public VerticalAlignment VerticalPosition { get; set; }

        public String Name { get; set; }
        private Int16 _size;
        public Int16 Size
        {
            get { return _size; }
            set
            {
                _size = value;
                if (Changed != null)
                    Changed(this, new HudItemEventArgs() { Item = this });
            }
        }

        private bool _visible;
        public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;
                if (Changed != null)
                    Changed(this, new HudItemEventArgs() { Item = this });
            }
        }
        public Int16 Rotation { get; set; }

        private Int32 _value;
        public Int32 Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (Changed != null)
                    Changed(this, new HudItemEventArgs() { Item = this });
            }
        }

        public Int32 DefaultValue { get; set; }
        public Int32 MinValue { get; set; }
        public Int32 MaxValue { get; set; }

        public bool MinumumTrigger { get; set; }
        public bool MaximumTrigger { get; set; }

        public String DefaultText { get; set; }

        public String _text;
        public String Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (Changed != null)
                    Changed(this, new HudItemEventArgs() { Item = this });
            }
        }

        public String Label { get; set; }

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
