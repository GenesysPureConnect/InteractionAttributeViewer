
using ININ.IceLib.Connection;
using ININ.InteractionClient.AddIn;
namespace InteractionAttributeViewer
{
    public class AttributeAddIn : ININ.InteractionClient.AddIn.AddInWindow
    {
        private IQueue _myQueue;
        private Session _session;

        protected override void OnLoad(System.IServiceProvider serviceProvider)
        {
            base.OnLoad(serviceProvider);
            _session = (Session)serviceProvider.GetService(typeof(Session));
            _myQueue = ((IQueueService)serviceProvider.GetService(typeof(IQueueService))).GetMyInteractions(new string[]{InteractionAttributes.State}, new string[]{});
        }
        protected override string CategoryDisplayName
        {
            get { return "Debug Utilities" ; }
        }

        protected override string CategoryId
        {
            get { return "Debug Utilities"; }
        }

        public override object Content
        {
            get 
            {
                var window = new AttributeView();
                window.DataContext = new AttributeViewerViewModel(_myQueue, _session);
                return window;
            }
        }

        protected override string DisplayName
        {
            get { return "Attribute Viewer"; }
        }

        protected override string Id
        {
            get { return "Attribute Viewer"; }
        }
    }
}
