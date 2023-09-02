using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorCore
{
    public class NetworkRouter
    {
        readonly int uintSize = sizeof (uint);

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
                if (buffer.Count >= uintSize)
                {
                    bufferStatus = BufferStatus.WaitingMsg;

                    var headerBuffer = new byte[uintSize];

                    buffer.CopyTo (0, headerBuffer, 0, uintSize);

                    headerSize = BitConverter.ToInt32 (headerBuffer, 0);

                    var deltaSize = buffer.Count - uintSize;

                    //還有剩
                    if (deltaSize > 0)
                    {
                        byte[] newBuffer = new byte[deltaSize];

                        //第一個參數是指從哪個index開始複製,
                        //第四個參數只要複製多長
                        buffer.CopyTo (uintSize, newBuffer, 0, deltaSize);

                        buffer = newBuffer.ToList ();

                        if (deltaSize > headerSize)
                        {
                            ProcessBuffer ();
                        }
                    }
                }
            }
            else if (bufferStatus == BufferStatus.WaitingMsg)
            {
                if (buffer.Count >= headerSize)
                {
                    bufferStatus = BufferStatus.WaitingHeader;

                    var msgBuffer = new byte[headerSize];

                    buffer.CopyTo (0, msgBuffer, 0, headerSize);

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

                    var deltaSize = buffer.Count - headerSize;

                    //還有剩
                    if (deltaSize > 0)
                    {
                        byte[] newBuffer = new byte[deltaSize];

                        //第一個參數是指從哪個index開始複製,
                        //第四個參數只要複製多長
                        buffer.CopyTo (headerSize, newBuffer, 0, deltaSize);

                        buffer = newBuffer.ToList ();

                        if (deltaSize > uintSize)
                        {
                            ProcessBuffer ();
                        }
                    }
                }
            }
        }

        Encoding encoding = Encoding.UTF8;

        int headerSize;

        BufferStatus bufferStatus = BufferStatus.WaitingHeader;

        List<byte> buffer;

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
