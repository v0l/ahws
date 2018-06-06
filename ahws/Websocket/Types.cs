using System;

namespace v0l.ahws.Websocket
{
    [Flags]
    public enum WebSocketFlags
    {
        RSV1 = 16,
        RSV2 = 32,
        RSV3 = 64,
        FinalFragment = 128
    }

    public enum WebSocketOpCode
    {
        ContinuationFrame = 0,
        TextFrame = 1,
        BinaryFrame = 2,
        ConnectionClose = 8,
        Ping = 9,
        Pong = 10
    }

    public enum WebsocketCloseCode
    {
        Normal = 1000,
        GoingAway = 1001,
        ProtocolError = 1002,
        InvalidDataType = 1003,
        Reserved = 1004,
        NoStatusCode = 1005,
        ExpectedCloseFrame = 1006,
        DataTypeError = 1007,
        PolicyError = 1008,
        DataTooBig = 1009,
        ClientExpectedExtensions = 1010,
        ServerError = 1011,
        TLSHandshakeFailed = 1015
    }
}
