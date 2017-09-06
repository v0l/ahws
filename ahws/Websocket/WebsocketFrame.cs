using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ahws.Websocket
{
    public class WebSocketFrame
    {
        public WebSocketFlags Flags { get; set; }

        public WebSocketOpCode OpCode { get; set; }

        public bool Mask { get; set; }

        public UInt64 PayloadLength { get; set; }

        public byte[] MaskingKey { get; set; }

        public byte[] Payload { get; set; }
        
        public async Task<byte[]> GetDataAsync()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await ms.WriteAsync(new byte[] { (byte)((int)Flags | (int)OpCode) }, 0, 1);

                //write payload length length
                byte pl = 0x00;
                if (PayloadLength < 126)
                {
                    pl = (byte)((byte)PayloadLength | (Mask ? 0x80 : 0x00));
                }
                else if (PayloadLength <= UInt16.MaxValue)
                {
                    pl = (byte)(0x7E | (Mask ? 0x80 : 0x00));
                }
                else
                {
                    pl = (byte)(0x7F | (Mask ? 0x80 : 0x00));
                }
                await ms.WriteAsync(new byte[] { (byte)pl }, 0, 1);

                //extended payload length
                if (PayloadLength > 125 && PayloadLength <= UInt16.MaxValue)
                {
                    var epl = BitConverter.GetBytes((UInt16)PayloadLength);
                    epl = epl.Reverse().ToArray();
                    await ms.WriteAsync(epl, 0, epl.Length);
                }
                else if (PayloadLength > UInt16.MaxValue)
                {
                    var epl = BitConverter.GetBytes(PayloadLength);
                    epl = epl.Reverse().ToArray();
                    await ms.WriteAsync(epl, 0, epl.Length);
                }

                if (Payload != null)
                {
                    if (Mask)
                    {
                        await ms.WriteAsync(MaskingKey, 0, MaskingKey.Length);
                        MaskPayload(); //mask
                    }

                    await ms.WriteAsync(Payload, 0, (int)PayloadLength);
                }

                return ms.ToArray();
            }
        }

        public static async Task<WebSocketFrame> PackData<T>(T data)
        {
            if (typeof(T) == typeof(string))
            {
                var p = PackData(Encoding.UTF8.GetBytes(data as string));
                p.OpCode = WebSocketOpCode.TextFrame;
                return p;

            }
            else if (typeof(T) == typeof(byte[]))
            {
                return await PackData(data);
            }

            return null;
        }

        public static WebSocketFrame PackData(byte[] data)
        {
            WebSocketFrame ret = new WebSocketFrame();

            ret.PayloadLength = (ulong)data.LongLength;
            ret.Payload = data;
            ret.Flags = WebSocketFlags.FinalFragment;

            return ret;
        }

        public void MaskPayload()
        {
            //mask/unmask payload
            for (var x = 0; x < Payload.Length; x++)
            {
                Payload[x] = (byte)(Payload[x] ^ MaskingKey[x % 4]);
            }
        }

        public void ParsePayload(WebSocketFlags fl, WebSocketOpCode op, bool mask, UInt64 len, byte[] maskkey, byte[] payload)
        {
            Flags = fl;
            OpCode = op;
            Mask = mask;
            PayloadLength = len;
            MaskingKey = maskkey;
            Payload = payload;

            if (Mask && Payload != null)
            {
                MaskPayload(); //unmask
            }
        }
    }
}
