using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CommonSupport;

namespace Arbiter.Transport
{
    /// <summary>
    /// The Guid for the session is kept in the Dictionary.
    /// </summary>
    class SessionResults
    {
        int _responcesRequired;
        Type _responceTypeRequired;

        public AutoResetEvent _sessionEndEvent = new AutoResetEvent(false);
        public AutoResetEvent SessionEndEvent
        {
            get 
            {
                return _sessionEndEvent; 
            }
        }


        List<TransportMessage> _responcesReceived = new List<TransportMessage>();
        public List<TransportMessage> Responces
        {
            get 
            {
                lock (_responcesReceived)
                {
                    return _responcesReceived;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="responcesRequired"></param>
        /// <param name="responceTypeRequired"></param>
        public SessionResults(int responcesRequired, Type responceTypeRequired)
        {
            _responcesRequired = responcesRequired;
            _responceTypeRequired = responceTypeRequired;
        }

        /// <summary>
        /// 
        /// </summary>
        public void ReceiveResponce(TransportMessage message)
        {
            if (message != null 
                && message.GetType() != _responceTypeRequired 
                && message.GetType().IsSubclassOf(_responceTypeRequired) == false)
            {
                SystemMonitor.Error("Session received invalid responce message type [expected (" + _responceTypeRequired.Name + "), received(" + message.GetType().Name + ")]. Message ignored.");
            }
            else
            {
                if (message == null)
                {// We received a NULL signalling to stop wait and abort session.
                    _sessionEndEvent.Set();
                    return;
                }
                else
                {
                    lock (_responcesReceived)
                    {
                        if (_responcesReceived.Count < _responcesRequired)
                        {
                            _responcesReceived.Add(message);

                            if (_responcesReceived.Count == _responcesRequired)
                            {
                                _sessionEndEvent.Set();
                            }
                        }
                        else
                        {// One more requestMessage responce received.
                            TracerHelper.TraceError("Session received too many responce messages. Message ignored.");
                        }
                    }
                }
            }
        }
    }

}
