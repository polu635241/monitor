using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    public class ChannelBase
    {
        public ChannelBase () 
        {
            networkRouter.BindingOnMsg (OnMsg);
        }

        public void BindEvent<T> (int eventID, Action<T> callback) where T:new ()
        {
            if (cacheEvents.Exists (pair => pair.eventID == eventID))
            {
                LoggerRouter.WriteLine ($"註冊重覆的ID -> {eventID}");
            }

            Action<string> proxy = (string str) =>
            {
                T data = JsonUtility.FromJson<T> (str);
                callback.Invoke (data);
            };

            cacheEvents.Add ((eventID, proxy));
        }

        

        List<(int eventID, Action<string> callback)> cacheEvents = new List<(int eventID, Action<string> callback)> ();

        void OnMsg (string msg) 
        {
            MsgContainer msgContainer = JsonUtility.FromJson<MsgContainer> (msg);

            var eventIndex = cacheEvents.FindIndex (pair => pair.eventID == msgContainer.eventID);

            if (eventIndex >= 0)
            {
                cacheEvents[eventIndex].callback.Invoke (msgContainer.json);
            }
            else
            {
                LoggerRouter.WriteLine ($"沒有被註冊的ID -> {msgContainer.eventID}");
            }
        }

        NetworkRouter networkRouter = new NetworkRouter ();

        protected byte[] GetMsgBuffers<T> (int eventID, T msg)
        {
            MsgContainer msgContainer = new MsgContainer ();
            msgContainer.eventID = eventID;
            msgContainer.json = JsonUtility.ToJson (msg);

            var buffers = networkRouter.GetMsgBuffers (msgContainer);
            return buffers;
        }

        /// <summary>
        /// 轉傳的依據
        /// </summary>
        [Serializable]
        internal class MsgContainer
        {
            public int eventID;
            public string json;
        }
    }
}
