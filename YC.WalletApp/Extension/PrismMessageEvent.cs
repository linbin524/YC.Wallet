using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.WalletApp.Extension
{
    public class PrismMessageEvent : PubSubEvent<string>
    {
    }
    public class EventSendExtension: BindableBase
    {
        private readonly IEventAggregator _eventAggregator;
        private string _messageToSend;

        public string MessageToSend
        {
            get { return _messageToSend; }
            set { SetProperty(ref _messageToSend, value); }
        }

        public DelegateCommand SendMessageCommand { get; }

        public EventSendExtension(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            SendMessageCommand = new DelegateCommand(SendMessage);
        }

        public void SendMessage()
        {
            if (!string.IsNullOrEmpty(MessageToSend))
            {
                _eventAggregator.GetEvent<PrismMessageEvent>().Publish(MessageToSend);
                MessageToSend = string.Empty;
            }
        }
    }
}
