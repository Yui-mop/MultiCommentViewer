﻿using System;
using Common;
using SitePlugin;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Linq;
using System.Net.Http;
using System.Diagnostics;
using Codeplex.Data;
using System.Text.RegularExpressions;

namespace ShowRoomSitePlugin
{
    abstract class InternalMessageBase : IInternalMessage
    {
        public InternalMessageBase(string raw)
        {
            Raw = raw;
        }
        public abstract InternalMessageType MessageType { get;}
        public string Raw { get; }
        protected string GetRawData()
        {
            var pos = Raw.IndexOf('{');
            return Raw.Substring(pos);
        }
    }
    class T1 : InternalMessageBase
    {
        /// <summary>
        /// comment
        /// </summary>
        public string Cm { get; }
        /// <summary>
        /// avatarImage
        /// </summary>
        public long Av { get; }
        /// <summary>
        /// delay
        /// </summary>
        public long D { get; }
        /// <summary>
        /// userName
        /// </summary>
        public string Ac { get; }
        /// <summary>
        /// UserId
        /// </summary>
        public long U { get; }
        public long CreatedAt { get; }
        public long At { get; }
        public T1(string raw) : base(raw)
        {
            var data = GetRawData();
            var json = DynamicJson.Parse(data);
            Cm = json.cm;
            Av = (long)json.av;
            D = (long)json.d;
            Ac = json.ac;
            U = (long)json.u;
            CreatedAt = (long)json.created_at;
            At = (long)json.at;
        }
        public override InternalMessageType MessageType { get; } = InternalMessageType.t1;
    }
    class T2 : InternalMessageBase
    {
        public long N { get; }
        public long Av { get; }
        public long D { get; }
        public string Ac { get; }
        public long CreatedAt { get; }
        public long U { get; }
        public long H { get; }
        public long G { get; }
        public long Gt { get; }
        public long At { get; }
        public T2(string raw) : base(raw)
        {
            var data = GetRawData();
            var json = DynamicJson.Parse(data);
            N = (long)json.n;
            Av = (long)json.av;
            D = (long)json.d;
            Ac = json.ac;
            CreatedAt = (long)json.created_at;
            U = (long)json.u;
            H = (long)json.h;
            G = (long)json.g;
            Gt = (long)json.gt;
            At = (long)json.at;
        }
        public override InternalMessageType MessageType { get; } = InternalMessageType.t2;
    }
    class T6 : InternalMessageBase
    {
        public long CreatedAt { get; }
        public long U { get; }
        public long At { get; }
        public T6(string raw) : base(raw)
        {
            var data = GetRawData();
            var json = DynamicJson.Parse(data);
            CreatedAt = (long)json.created_at;
            U = (long)json.u;
            At = (long)json.at;
        }
        public override InternalMessageType MessageType { get; } = InternalMessageType.t2;
    }
    static class MessageParser
    {
        public static IInternalMessage Parse(string raw)
        {
            var match = Regex.Match(raw, "\"t\":\"?(\\d+)");
            if (!match.Success)
            {
                throw new ParseException(raw);
            }
            IInternalMessage internalMessage;
            var type = match.Groups[1].Value;
            switch (type)
            {
                case "1":
                    internalMessage = new T1(raw);
                    break;
                case "2":
                    internalMessage = new T2(raw);
                    break;
                case "6":
                    internalMessage = new T6(raw);
                    break;
                case "8":
                    internalMessage = null;
                    break;
                default:
                    //"MSG\t6ce05f:8Jvx9D6M\t{\"created_at\":1561899486,\"t\":100}"
                    //"MSG\t6ce05f:8Jvx9D6M\t{\"telops\":[{\"color\":{\"r\":255,\"b\":255,\"g\":255},\"text\":\"最高です！ありがとう！ファミマ行けー！\",\"type\":\"user\"}],\"telop\":\"最高です！ありがとう！ファミマ行けー！\",\"interval\":6000,\"t\":8,\"api\":\"https://www.showroom-live.com/live/telop?live_id=7135327\"}"
                    throw new ParseException(raw);
            }
            return internalMessage;
        }
    }
    class MessageProvider : IMessageProvider
    {
        public event EventHandler<IInternalMessage> Received;
        private readonly IWebsocket _ws;
        private readonly ILogger _logger;
        string _bcsvr_key;
        public async Task ReceiveAsync(string bcsvr_host, string bcsvr_key)
        {
            if (string.IsNullOrEmpty(bcsvr_key))
            {
                throw new ArgumentNullException(nameof(bcsvr_key));
            }
            _bcsvr_key = bcsvr_key;

            await _ws.ReceiveAsync("wss://" + bcsvr_host + "/");
        }
        public MessageProvider(IWebsocket websocket, ILogger logger)
        {
            _ws = websocket;
            _logger = logger;
            websocket.Opened += Websocket_Opened;
            websocket.Received += Websocket_Received;
        }
        public void Disconnect()
        {
            _ws.Disconnect();
        }

        private void Websocket_Received(object sender, string e)
        {
            Debug.WriteLine(e);
            try
            {
                var internalMessage = MessageParser.Parse(e);
                if (internalMessage != null)
                {
                    Received?.Invoke(this, internalMessage);
                }
            }
            catch (ParseException ex)
            {
                _logger.LogException(ex);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
            }
        }

        private async void Websocket_Opened(object sender, EventArgs e)
        {
            await _ws.SendAsync($"SUB\t{_bcsvr_key}");
        }
    }
}