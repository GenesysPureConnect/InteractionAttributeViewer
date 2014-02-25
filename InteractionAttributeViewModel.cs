using System.ComponentModel;
using System.Threading;
using System.Timers;
using System.Windows.Media;

namespace InteractionAttributeViewer
{
    class InteractionAttributeViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _attributeName;
        private string _attributeValue;
        private Brush _backgroundBrush = Brushes.White;
        private System.Threading.Timer _timer;

        public InteractionAttributeViewModel(string name, string value)
        {
            _attributeName = name;
            _attributeValue = value;

            _timer = new System.Threading.Timer(new TimerCallback((o) =>
            {
                AttributeViewerViewModel.SyncContext.Post((obj) =>
                {
                    this.BackgroundBrush = Brushes.White;
                }, null);

            }), null, Timeout.Infinite, Timeout.Infinite);
        }

        public Brush BackgroundBrush
        {
            get
            {
                return _backgroundBrush;
            }
            set
            {
                _backgroundBrush = value;
                RaiseChanged("BackgroundBrush");
            }
        }

        public string AttributeName
        {
            get
            {
                return _attributeName;
            }
            set
            {
                _attributeName = value;
                RaiseChanged("AttributeName");
            }
        }

        public string AttributeValue
        {
            get
            {
                return _attributeValue;
            }
            set
            {

                _attributeValue = value;
                RaiseChanged("AttributeValue");
                this.BackgroundBrush = Brushes.LightBlue;


                _timer.Change(1500, Timeout.Infinite);
              
            }
        }

        private void RaiseChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
