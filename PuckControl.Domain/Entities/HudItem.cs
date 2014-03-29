namespace PuckControl.Domain.Entities
{
    using PuckControl.Domain.EventArg;
    using System;
    using System.Timers;
    using System.Windows;

    public class HUDItem : IDisposable
    {
        public event EventHandler<HUDItemEventArgs> MaximumTriggerReached;
        public event EventHandler<HUDItemEventArgs> MinimumTriggerReached;
        public event EventHandler<HUDItemEventArgs> Changed;
        private Timer _timer = new Timer();
        
        private HUDItemType _type;
        public HUDItemType ItemType
        {
            get { return _type; }
            set
            {
                _type = value;
                if (Changed != null)
                    Changed(this, new HUDItemEventArgs() { Item = this });
            }
        }
        public TimerType TimerOption { get; set; }
        public HUDItemStyle Style { get; set; }
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
                    Changed(this, new HUDItemEventArgs() { Item = this });
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
                    Changed(this, new HUDItemEventArgs() { Item = this });
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
                    Changed(this, new HUDItemEventArgs() { Item = this });
            }
        }

        public Int32 DefaultValue { get; set; }
        public Int32 MinValue { get; set; }
        public Int32 MaxValue { get; set; }

        public bool MinimumTrigger { get; set; }
        public bool MaximumTrigger { get; set; }

        public String DefaultText { get; set; }

        private String _text;
        public String Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (Changed != null)
                    Changed(this, new HUDItemEventArgs() { Item = this });
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

        public HUDItem()
        {
            Visible = false;
            Size = 1;
        }

        public void Reset()
        {
            this.Value = DefaultValue;
            this.Text = DefaultText;
        }

        #region IDispose Implementation
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _timer.Dispose();
                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }
        #endregion
    }
}
