using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    /// <summary>
    /// 對其註冊事件
    /// 並且在傳入的byte[]達到一個封包時會解析並觸發綁定的事件
    /// </summary>
    public class ChannelTransport
    {
        public ChannelTransport () 
        {
            networkRouter.BindingOnMsg (OnMsg);
        }

        public const int port = 7339;

        public void BindEvent<T> (int eventID, Action<T> callback) where T : new()
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

        public void BindingSocket (Socket socket, Action onDisconnect)
        {
            this.socket = socket;
            this.isConnect = true;

            Task.Run (()=> 
            {
                byte[] result = new byte[2048];
                while (true)
                {
                    try
                    {
                        int receiveLength = socket.Receive (result);

                        if (receiveLength > 0)
                        {
                            var buffer = new byte[receiveLength];
                            result.ToList ().CopyTo (0, buffer, 0, receiveLength);
                            InputPackMsg (buffer);
                        }
                        else
                        {
                            if (receiveLength == 0)
                            {
                                Clear ();
                                onDisconnect ();
                                isConnect = false;

                                break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LoggerRouter.WriteLine (e.Message);
                        LoggerRouter.WriteLine (e.StackTrace);

                        Clear ();
                        onDisconnect ();
                        isConnect = false;

                        break;
                    }
                }
            });
        }

        public bool isConnect = false;
        Socket socket = null;

        public void InputPackMsg (byte[] pack) 
        {
            networkRouter.ReceiveMsg (pack);
        }

        List<(int eventID, Action<string> callback)> cacheEvents = new List<(int eventID, Action<string> callback)> ();

        void OnMsg (string msg) 
        {
            MsgContainer msgContainer = JsonUtility.FromJson<MsgContainer> (msg);

            var eventIndex = cacheEvents.FindIndex (pair => pair.eventID == msgContainer.eventID);

            if (eventIndex >= 0)
            {
                LoggerRouter.WriteLine ($"觸發事件ID -> {msgContainer.eventID}, {msgContainer.json}");

                cacheEvents[eventIndex].callback.Invoke (msgContainer.json);
            }
            else
            {
                LoggerRouter.WriteLine ($"沒有被註冊的ID -> {msgContainer.eventID}");
            }
        }

        NetworkRouter networkRouter = new NetworkRouter ();

        public void Clear () 
        {
            networkRouter.Clear ();
        }

        public void Disconnect () 
        {
            if (isConnect)
            {
                try 
                {
                    socket.Shutdown (SocketShutdown.Both);
                    socket.Close ();
                }
                catch (Exception e)
                {
                    LoggerRouter.WriteLine (e.Message);
                    LoggerRouter.WriteLine (e.StackTrace);
                }
            }
        }

        public void SendMsg<T> (int eventID, T msg) 
        {
            if (isConnect) 
            {
                var json = JsonUtility.ToJson (msg, false);

                var buffer = GetMsgBuffers (eventID, json);

                socket.Send (buffer);
                LoggerRouter.WriteLine ($"發送事件 {eventID}, {json}");
            }
            else
            {
                LoggerRouter.WriteLine ("斷線中無法發送訊息");
            }
        }

        protected byte[] GetMsgBuffers (int eventID, string json)
        {
            MsgContainer msgContainer = new MsgContainer ();
            msgContainer.eventID = eventID;
            msgContainer.json = json;

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
