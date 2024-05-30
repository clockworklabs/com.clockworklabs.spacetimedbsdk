using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;

namespace SpacetimeDB
{
    public delegate void WebSocketOpenEventHandler();

    public delegate void WebSocketMessageEventHandler(byte[] message);

    public delegate void WebSocketCloseEventHandler(WebSocketCloseStatus? code, WebSocketError? error);

    public delegate void WebSocketConnectErrorEventHandler(WebSocketError? error, string message);
    public delegate void WebSocketSendErrorEventHandler(Exception e);

    public struct ConnectOptions
    {
        public string Protocol;
    }


    public class WebSocket
    {
        // WebSocket buffer for incoming messages
        private static readonly int MAXMessageSize = 0x4000000; // 64MB

        // Connection parameters
        private readonly ConnectOptions _options;
        private readonly byte[] _receiveBuffer = new byte[MAXMessageSize];
        private readonly ConcurrentQueue<Action> dispatchQueue = new();

        protected ClientWebSocket Ws = new();

        public WebSocket(ConnectOptions options)
        {
            _options = options;
        }

        public event WebSocketOpenEventHandler? OnConnect;
        public event WebSocketConnectErrorEventHandler? OnConnectError;
        public event WebSocketSendErrorEventHandler? OnSendError;
        public event WebSocketMessageEventHandler? OnMessage;
        public event WebSocketCloseEventHandler? OnClose;

        public bool IsConnected { get { return Ws != null && Ws.State == WebSocketState.Open; } }

        public async Task Connect(string? auth, string host, string nameOrAddress, Address clientAddress)
        {
            var url = new Uri($"{host}/database/subscribe/{nameOrAddress}?client_address={clientAddress}");
            Ws.Options.AddSubProtocol(_options.Protocol);

            var source = new CancellationTokenSource(10000);
            if (!string.IsNullOrEmpty(auth))
            {
                var tokenBytes = Encoding.UTF8.GetBytes($"token:{auth}");
                var base64 = Convert.ToBase64String(tokenBytes);
                Ws.Options.SetRequestHeader("Authorization", "Basic " + base64);
            }
            else
            {
                Ws.Options.UseDefaultCredentials = true;
            }

            try
            {
                await Ws.ConnectAsync(url, source.Token);
                if (OnConnect != null) dispatchQueue.Enqueue(() => OnConnect());
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                if (OnConnectError != null)
                {
                    var message = ex.Message;
                    var code = (ex as WebSocketException)?.WebSocketErrorCode;
                    if (code == WebSocketError.NotAWebSocket)
                    {
                        // not a websocket happens when there is no module published under the address specified
                        message += " Did you forget to publish your module?";
                    }
                    dispatchQueue.Enqueue(() => OnConnectError(code, message));
                }
                return;
            }

            while (Ws.State == WebSocketState.Open)
            {
                try
                {
                    var receiveResult = await Ws.ReceiveAsync(new ArraySegment<byte>(_receiveBuffer),
                        CancellationToken.None);
                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        if (Ws.State != WebSocketState.Closed)
                        {
                            await Ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty,
                            CancellationToken.None);
                        }
                        if (OnClose != null) dispatchQueue.Enqueue(() => OnClose(receiveResult.CloseStatus, null));
                        return;
                    }

                    var count = receiveResult.Count;
                    while (receiveResult.EndOfMessage == false)
                    {
                        if (count >= MAXMessageSize)
                        {
                            // TODO: Improve this, we should allow clients to receive messages of whatever size
                            var closeMessage = $"Maximum message size: {MAXMessageSize} bytes.";
                            await Ws.CloseAsync(WebSocketCloseStatus.MessageTooBig, closeMessage,
                                CancellationToken.None);
                            if (OnClose != null) dispatchQueue.Enqueue(() => OnClose(WebSocketCloseStatus.MessageTooBig, null));
                            return;
                        }

                        receiveResult = await Ws.ReceiveAsync(
                            new ArraySegment<byte>(_receiveBuffer, count, MAXMessageSize - count),
                            CancellationToken.None);
                        count += receiveResult.Count;
                    }

                    if (OnMessage != null)
                    {
                        var message = _receiveBuffer.Take(count).ToArray();
                        dispatchQueue.Enqueue(() => OnMessage(message));
                    }
                }
                catch (WebSocketException ex)
                {
                    if (OnClose != null) dispatchQueue.Enqueue(() => OnClose(null, ex.WebSocketErrorCode));
                    return;
                }
            }
        }

        public Task Close(WebSocketCloseStatus code = WebSocketCloseStatus.NormalClosure)
        {
            Ws?.CloseAsync(code, "Disconnecting normally.", CancellationToken.None);

            return Task.CompletedTask;
        }

        private Task? senderTask;
        private readonly ConcurrentQueue<byte[]> messageSendQueue = new();

        /// <summary>
        /// This sender guarantees that that messages are sent out in the order they are received. Our websocket
        /// library only allows us to await one send call, so we have to wait until the current send call is complete
        /// before we start another one. This function is also thread safe, just in case.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void Send(ClientApi.Message message)
        {
            lock (messageSendQueue)
            {
                messageSendQueue.Enqueue(message.ToByteArray());
                senderTask ??= Task.Run(ProcessSendQueue);
            }
        }


        private async Task ProcessSendQueue()
        {
            try
            {
                while (true)
                {
                    byte[]? message;

                    lock (messageSendQueue)
                    {
                        if (!messageSendQueue.TryDequeue(out message))
                        {
                            // We are out of messages to send
                            senderTask = null;
                            return;
                        }
                    }

                    await Ws!.SendAsync(message, WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            }
            catch (Exception e)
            {
                senderTask = null;
                if (OnSendError != null) dispatchQueue.Enqueue(() => OnSendError(e));
            }
        }

        public WebSocketState GetState()
        {
            return Ws!.State;
        }

        public void Update()
        {
            while (dispatchQueue.TryDequeue(out var result))
            {
                result();
            }
        }
    }
}
