using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Input;
using ININ.IceLib.Connection;
using ININ.IceLib.Interactions;
using ININ.InteractionClient.AddIn;

namespace InteractionAttributeViewer
{
    /// <summary>
    /// Command class to handle button clicks
    /// </summary>
    internal class Command : ICommand
    {
        Action<object> _execute;
        public Command(Action<object> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event System.EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }


    internal class AttributeViewerViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private  IQueue _myQueue;
        private string _callId;
        private ICommand _startWatchCommand;
        private Session _session;
        private Interaction _watchedInteraction;
        private readonly ReadOnlyCollection<string> _attributeNames;
        private string _errorMessage;

        public static SynchronizationContext SyncContext { get; set; }

        public AttributeViewerViewModel(IQueue myQueue, Session session)
        {
            SyncContext = SynchronizationContext.Current;
            _attributeNames = GetAllAttributeNames();
            Attributes = new ObservableCollection<InteractionAttributeViewModel>();
           
            CallIds = new ObservableCollection<string>();
            _myQueue = myQueue;
            _session = session;

            _myQueue.InteractionAdded += OnInteractionAdded;
            _myQueue.InteractionRemoved +=OnInteractionRemoved;

            foreach (var interaction in _myQueue.Interactions)
            {
                CallIds.Add(interaction.InteractionId);
            }

            StartWatch = new Command((o) => StartAttributeWatch());
        }

        private ReadOnlyCollection<string> GetAllAttributeNames()
        {
            List<string> attributeNames = new List<string>();

            //Grab call attributes out of the icelib definitions. 
            var fieldInfoList = new List<FieldInfo[]>()
                                {
                                    typeof(InteractionAttributeName).GetFields(),
                                    typeof(CallbackInteractionAttributeName).GetFields(),
                                    typeof(MonitorInteractionAttributeName).GetFields(),
                                    typeof(RecorderInteractionAttributeName).GetFields()
                                 };


            foreach (FieldInfo fieldInfo in fieldInfoList.SelectMany(fieldInfos => fieldInfos))
            {
                var attrName = fieldInfo.GetValue(null) as string;

                if (attrName != null && !attributeNames.Contains(attrName))
                {
                    attributeNames.Add(attrName);
                }
            }

            attributeNames.Sort();
            return attributeNames.AsReadOnly();
        }

        private void StartAttributeWatch()
        {
            ErrorMessage = String.Empty;

            if (String.IsNullOrEmpty(_callId))
            {
                return;
            }

            try
            {
                Interaction interaction = InteractionsManager.GetInstance(_session).CreateInteraction(new InteractionId(_callId));
                if (interaction == null)
                {
                    return;
                }
                _watchedInteraction = interaction;
                _watchedInteraction.AttributesChanged += OnInteractionAttributesChanged;
                _watchedInteraction.Deallocated += OnInteractionDeallocated;
                _watchedInteraction.StartWatching(_attributeNames.ToArray());
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }

        /// <summary>
        /// When the interaction is deallocated (removed from the system) clean up the watches on it. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInteractionDeallocated(object sender, EventArgs e)
        {
            StopWatching();
        }

        private void StopWatching()
        {
            if (_watchedInteraction != null)
            {
                _watchedInteraction.AttributesChanged -= OnInteractionAttributesChanged;
                _watchedInteraction.Deallocated -= OnInteractionDeallocated;
                _watchedInteraction = null;
                SyncContext.Post((x) =>
                        {
                            Attributes.Clear();
                        }, null);
            }
        }

        /// <summary>
        /// When the watched interaction's attributes change, update the list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInteractionAttributesChanged(object sender, AttributesEventArgs e)
        {
            
                foreach (var attribute in e.InteractionAttributeNames)
                {
                    var attributeViewModel = Attributes.FirstOrDefault((a) => a.AttributeName.ToLower() == attribute.ToLower());
                    if (attributeViewModel != null)
                    {
                        SyncContext.Post((x) =>
                        {
                            attributeViewModel.AttributeValue = _watchedInteraction.GetStringAttribute(attribute);
                        },null);
                    }
                    else
                    {
                        SyncContext.Post((x) =>
                        {
                            Attributes.Add(new InteractionAttributeViewModel(attribute, _watchedInteraction.GetStringAttribute(attribute)));
                        }, null);
                    }
                }
           
        }

        /// <summary>
        /// When a call is removed from the user's queue, remove it from the drop down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInteractionRemoved(object sender, ININ.InteractionClient.AddIn.InteractionEventArgs e)
        {
            SyncContext.Post((o) =>
            {
                CallIds.Remove(e.Interaction.InteractionId);
            }, null);
        }

        /// <summary>
        /// When a call is added to the user's queue, add it to the drop down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnInteractionAdded(object sender, ININ.InteractionClient.AddIn.InteractionEventArgs e)
        {
            SyncContext.Post((o) =>
            {
                CallIds.Add(e.Interaction.InteractionId);
            }, null);
        }

        private void RaiseChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ICommand StartWatch
        {
            get
            {
                return _startWatchCommand;
            }
            set
            {
                _startWatchCommand = value;
            }
        }

        
        public ObservableCollection<InteractionAttributeViewModel> Attributes
        {
            get;
            set;
        }

        public ObservableCollection<string> CallIds
        {
            get;
            set;
        }

        public string CallId
        {
            get
            {
                return _callId;
            }
            set
            {
                _callId = value;
                RaiseChanged("CallId");
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                RaiseChanged("ErrorMessage");
            }
        }

    }
}
