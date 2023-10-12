using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpacetimeDB
{
    internal abstract class MainThreadDispatch
    {
        public abstract void Execute();
    }

    class OnConnectMessage : MainThreadDispatch
    {
        private WebSocketOpenEventHandler receiver;

        public OnConnectMessage(WebSocketOpenEventHandler receiver)
        {
            this.receiver = receiver;
        }

        public override void Execute()
        {
            receiver.Invoke();
        }
    }

    class OnDisconnectMessage : MainThreadDispatch
    {
        private WebSocketCloseEventHandler receiver;
        private WebSocketError? error;
        private WebSocketCloseStatus? status;

        public OnDisconnectMessage(WebSocketCloseEventHandler receiver, WebSocketCloseStatus? status,
            WebSocketError? error)
        {
            this.receiver = receiver;
            this.error = error;
            this.status = status;
        }

        public override void Execute()
        {
            receiver.Invoke(status, error);
        }
    }

    class OnConnectErrorMessage : MainThreadDispatch
    {
        private WebSocketConnectErrorEventHandler receiver;
        private WebSocketError? error;
        private string? errorMsg;

        public OnConnectErrorMessage(WebSocketConnectErrorEventHandler receiver, WebSocketError? error, string? errorMsg)
        {
            this.receiver = receiver;
            this.error = error;
            this.errorMsg = errorMsg;
        }

        public override void Execute()
        {
            receiver.Invoke(error, errorMsg);
        }
    }
    
class OnSendErrorMessage : MainThreadDispatch
    {
        private WebSocketSendErrorEventHandler receiver;
        private Exception e;

        public OnSendErrorMessage(WebSocketSendErrorEventHandler receiver, Exception e)
        {
            this.receiver = receiver;
            this.e = e;
        }

        public override void Execute()
        {
            receiver.Invoke(e);
        }
    }

    class OnMessage : MainThreadDispatch
    {
        private WebSocketMessageEventHandler receiver;
        private byte[] message;

        public OnMessage(WebSocketMessageEventHandler receiver, byte[] message)
        {
            this.receiver = receiver;
            this.message = message;
        }

        public override void Execute()
        {
            receiver.Invoke(message);
        }
    }

    public delegate void WebSocketOpenEventHandler();

    public delegate void WebSocketMessageEventHandler(byte[] message);

    public delegate void WebSocketCloseEventHandler(WebSocketCloseStatus? code, WebSocketError? error);

    public delegate void WebSocketConnectErrorEventHandler(WebSocketError? error, string? message);
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
        private readonly ConcurrentQueue<MainThreadDispatch> dispatchQueue = new ConcurrentQueue<MainThreadDispatch>();

        protected ClientWebSocket Ws;

        private ISpacetimeDBLogger _logger;

        public WebSocket(ISpacetimeDBLogger logger, ConnectOptions options)
        {
            Ws = new ClientWebSocket();
            _logger = logger;
            _options = options;
        }

        public event WebSocketOpenEventHandler OnConnect;
        public event WebSocketConnectErrorEventHandler OnConnectError;
        public event WebSocketSendErrorEventHandler OnSendError;
        public event WebSocketMessageEventHandler OnMessage;
        public event WebSocketCloseEventHandler OnClose;

        public bool IsConnected { get { return Ws != null && Ws.State == WebSocketState.Open; } }

        public async Task Connect(string auth, string host, string nameOrAddress, Address clientAddress)
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
                dispatchQueue.Enqueue(new OnConnectMessage(OnConnect));
            }
            catch (WebSocketException ex)
            {
                string message = ex.Message;
                if(ex.WebSocketErrorCode == WebSocketError.NotAWebSocket)
                {
                    // not a websocket happens when there is no module published under the address specified
                    message = $"{message} Did you forget to publish your module?";
                }
                dispatchQueue.Enqueue(new OnConnectErrorMessage(OnConnectError, ex.WebSocketErrorCode, message));
                return;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                dispatchQueue.Enqueue(new OnConnectErrorMessage(OnConnectError, null, e.Message));
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
                        dispatchQueue.Enqueue(new OnDisconnectMessage(OnClose, receiveResult.CloseStatus, null));
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
                            dispatchQueue.Enqueue(new OnDisconnectMessage(OnClose, WebSocketCloseStatus.MessageTooBig, null));
                            return;
                        }

                        receiveResult = await Ws.ReceiveAsync(
                            new ArraySegment<byte>(_receiveBuffer, count, MAXMessageSize - count),
                            CancellationToken.None);
                        count += receiveResult.Count;
                    }

                    var buffCopy = new byte[count];
                    for (var x = 0; x < count; x++)
                        buffCopy[x] = _receiveBuffer[x];
                    dispatchQueue.Enqueue(new OnMessage(OnMessage, buffCopy));
                }
                catch (WebSocketException ex)
                {
                    dispatchQueue.Enqueue(new OnDisconnectMessage(OnClose, null, ex.WebSocketErrorCode));
                    return;
                }
            }
        }

        public Task Close(WebSocketCloseStatus code = WebSocketCloseStatus.NormalClosure, string reason = null)
        {
            Ws?.CloseAsync(code, "Disconnecting normally.", CancellationToken.None);

            return Task.CompletedTask;
        }

        private readonly object sendingLock = new object();
        private Task senderTask = null;
        private readonly ConcurrentQueue<byte[]> messageSendQueue = new ConcurrentQueue<byte[]>();

        /// <summary>
        /// This sender guarantees that that messages are sent out in the order they are received. Our websocket
        /// library only allows us to await one send call, so we have to wait until the current send call is complete
        /// before we start another one. This function is also thread safe, just in case.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void Send(byte[] message)
        {
            lock (messageSendQueue)
            {
                messageSendQueue.Enqueue(message);
                if (senderTask == null)
                {
                    senderTask = Task.Run(async () => { await ProcessSendQueue(); });
                }
            }
        }


        private async Task ProcessSendQueue()
        {
            try
            {
                while (true)
                {
                    byte[] message;
                    lock (messageSendQueue)
                    {
                        if (!messageSendQueue.TryDequeue(out message))
                        {
                            // We are out of messages to send
                            senderTask = null;
                            return;
                        }
                    }

                    await Ws!.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true,
                        CancellationToken.None);
                }
            }
            catch(Exception e)
            { 
                senderTask = null;
                dispatchQueue.Enqueue(new OnSendErrorMessage(OnSendError, e));
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
                result.Execute();
            }
        }
    }
}
