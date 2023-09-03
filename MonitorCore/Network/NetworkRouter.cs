using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    /// <summary>
    /// 不對蒐集封包直到蒐集完畢後轉成字串還給上層
    /// </summary>
    public class NetworkRouter
    {
        readonly int HeaderSize = sizeof (uint);

        public void ReceiveMsg (byte[] msg) 
        {
            buffer.AddRange (msg);

            ProcessBuffer ();
        }

        public void BindingOnMsg (Action<string> onMsg) 
        {
            this.onMsg = onMsg;
        }

        Action<string> onMsg;

        void ProcessBuffer () 
        {
            //找出header
            if (bufferStatus == BufferStatus.WaitingHeader)
            {
                if (buffer.Count >= HeaderSize)
                {
                    bufferStatus = BufferStatus.WaitingMsg;

                    var headerBuffer = new byte[HeaderSize];

                    buffer.CopyTo (0, headerBuffer, 0, HeaderSize);

                    msgSize = BitConverter.ToInt32 (headerBuffer, 0);

                    var deltaSize = buffer.Count - HeaderSize;

                    //還有剩
                    if (deltaSize > 0)
                    {
                        byte[] newBuffer = new byte[deltaSize];

                        //第一個參數是指從哪個index開始複製,
                        //第四個參數指要複製多長
                        buffer.CopyTo (HeaderSize, newBuffer, 0, deltaSize);

                        buffer = newBuffer.ToList ();

                        if (deltaSize > msgSize)
                        {
                            ProcessBuffer ();
                        }
                    }
                }
            }
            else if (bufferStatus == BufferStatus.WaitingMsg)
            {
                if (buffer.Count >= msgSize)
                {
                    bufferStatus = BufferStatus.WaitingHeader;

                    var msgBuffer = new byte[msgSize];

                    buffer.CopyTo (0, msgBuffer, 0, msgSize);

                    string msg = encoding.GetString (msgBuffer);

                    if (onMsg != null)
                    {
                        try
                        {
                            onMsg.Invoke (msg);
                        }
                        catch(Exception e)
                        {
                            LoggerRouter.WriteLine (e.Message);
                            LoggerRouter.WriteLine (e.StackTrace);
                        }
                    }

                    var deltaSize = buffer.Count - msgSize;

                    //還有剩
                    if (deltaSize > 0)
                    {
                        byte[] newBuffer = new byte[deltaSize];

                        //第一個參數是指從哪個index開始複製,
                        //第四個參數指要複製多長
                        buffer.CopyTo (msgSize, newBuffer, 0, deltaSize);

                        buffer = newBuffer.ToList ();

                        if (deltaSize > HeaderSize)
                        {
                            ProcessBuffer ();
                        }
                    }
                }
            }
        }

        Encoding encoding = Encoding.UTF8;

        int msgSize;

        BufferStatus bufferStatus = BufferStatus.WaitingHeader;

        List<byte> buffer = new List<byte> ();

        public void Clear () 
        {
            buffer = new List<byte> ();
            bufferStatus = BufferStatus.WaitingHeader;
        }

        public byte[] GetMsgBuffers<T> (T msg) 
        {
            List<byte> msgBuffers = new List<byte> ();

            var json = JsonUtility.ToJson (msg);

            var jsonBuffer = encoding.GetBytes (json);

            //檔頭等於後續json的長度
            var header = (uint)jsonBuffer.Length;
            var headerBuffer = BitConverter.GetBytes (header);
            msgBuffers.AddRange (headerBuffer);
            msgBuffers.AddRange (jsonBuffer);

            return msgBuffers.ToArray ();
        }

        enum BufferStatus 
        {
            WaitingHeader,
            WaitingMsg
        }
    }
}
