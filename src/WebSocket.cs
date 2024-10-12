using SpacetimeDB.BSATN;
using SpacetimeDB.ClientApi;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Codice.Client.GameUI.Update;
using UnityEngine;

namespace SpacetimeDB
{
    internal class WebSocket
    {
        public delegate void OpenEventHandler();

        public delegate void MessageEventHandler(byte[] message, DateTime timestamp);

        public delegate void CloseEventHandler(WebSocketCloseStatus? code, WebSocketError? error);

        public delegate void ConnectErrorEventHandler(Exception e);
        public delegate void SendErrorEventHandler(Exception e);

        public struct ConnectOptions
        {
            public string Protocol;
        }

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

        public event OpenEventHandler? OnConnect;
        public event ConnectErrorEventHandler? OnConnectError;
        public event SendErrorEventHandler? OnSendError;
        public event MessageEventHandler? OnMessage;
        public event CloseEventHandler? OnClose;

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
                Ws.Options.SetRequestHeader("Authorization", $"Basic {base64}");
            }
            else
            {
                Ws.Options.UseDefaultCredentials = true;
            }
            
            try
            {
                await Ws.ConnectAsync(url, source.Token);
                if (Ws.State == WebSocketState.Open)
                {
                    if (OnConnect != null)
                    {
                        dispatchQueue.Enqueue(() => OnConnect());
                    }
                }
                else
                {
                    if (OnConnectError != null) 
                    {
                        dispatchQueue.Enqueue(() => OnConnectError(
                            new Exception($"WebSocket connection failed. Current state: {Ws.State}")));
                    }
                    return;
                }
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.Success)
            {
                // Debug.LogError($"Error code: {ex} {ex.Message} {ex.ErrorCode} {ex.NativeErrorCode} {Ws.CloseStatus} {Ws.State} {ex.InnerException}");
                // Debug.LogException(ex);
                
                // How can we get here:
                // - When you go to connect and the server isn't running (port closed) - target machine actively refused
                // - 404 - No module with at that module address instead of 101 upgrade
                // - 401? - When the identity received by SpacetimeDB wasn't signed by its signing key
                // - 400 - When the auth is malformed
                
                if (OnConnectError != null)
                {
                    // .net 6,7,8 has support for Ws.HttpStatusCode as long as you set
                    // ClientWebSocketOptions.CollectHttpResponseDetails = true
                    dispatchQueue.Enqueue(() => OnConnectError(new Exception("")));
                }
                

                // var builder = new StringBuilder();
                // builder.Append(
                //     "WebSocketException occurred, but WebSocketErrorCode indicates success. This might be due to a network issue.");
                // // Log.Info("");
                // Log.Info($"Exception message: {ex.Message}");
                // Log.Info($"Inner exception: {ex.InnerException?.Message}");
                // Log.Exception(ex);
                // if (OnConnectError != null)
                // {
                //     var message = ex.Message;
                //     var code = ex.WebSocketErrorCode;
                //     if (code == WebSocketError.NotAWebSocket)
                //     {
                //         // not a websocket happens when there is no module published under the address specified
                //         message += " Did you forget to publish your module?";
                //     }
                //
                //     dispatchQueue.Enqueue(() => OnConnectError(code, message));
                // }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket connection failed: {ex.WebSocketErrorCode}");
                Console.WriteLine($"Exception message: {ex.Message}");
            }
            catch (SocketException ex)
            {
                // This might occur if the server is unreachable or the DNS lookup fails.
                Console.WriteLine($"SocketException occurred: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
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

                    var startReceive = DateTime.UtcNow;
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
                        dispatchQueue.Enqueue(() => OnMessage(message, startReceive));
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
        private readonly ConcurrentQueue<ClientMessage> messageSendQueue = new();

        /// <summary>
        /// This sender guarantees that that messages are sent out in the order they are received. Our websocket
        /// library only allows us to await one send call, so we have to wait until the current send call is complete
        /// before we start another one. This function is also thread safe, just in case.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void Send(ClientMessage message)
        {
            lock (messageSendQueue)
            {
                messageSendQueue.Enqueue(message);
                senderTask ??= Task.Run(ProcessSendQueue);
            }
        }


        private async Task ProcessSendQueue()
        {
            try
            {
                while (true)
                {
                    ClientMessage message;

                    lock (messageSendQueue)
                    {
                        if (!messageSendQueue.TryDequeue(out message))
                        {
                            // We are out of messages to send
                            senderTask = null;
                            return;
                        }
                    }

                    var messageBSATN = new ClientMessage.BSATN();
                    var encodedMessage = IStructuralReadWrite.ToBytes(messageBSATN, message);
                    await Ws!.SendAsync(encodedMessage, WebSocketMessageType.Binary, true, CancellationToken.None);
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
