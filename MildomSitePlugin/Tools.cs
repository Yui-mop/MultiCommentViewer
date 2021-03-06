﻿using ryu_s.BrowserCookie;
using SitePlugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.Core.Converters;

namespace MildomSitePlugin
{
    static class Tools
    {
        public static IMyUserInfo GetUserInfoFromCookie(IBrowserProfile browserProfile)
        {
            var cookies = browserProfile.GetCookieCollection("mildom.com");
            if (cookies.Exists(item => item.Name == "user"))
            {
                var cookie = cookies.Where(item => item.Name == "user").First();
                var val = cookie.Value;
                var decoded = System.Web.HttpUtility.UrlDecode(val);
                var userInfoLow = Tools.Deserialize<Low.UserInfo.RootObject>(decoded);
                string gid;
                if (cookies.Exists(c => c.Name == "gid"))
                {
                    gid = cookies.Where(item => item.Name == "gid").First().Value;
                }
                else
                {
                    gid = "";
                }
                return new LoggedinUserInfo(userInfoLow, gid);
            }
            else if (cookies.Exists(item => item.Name == "gid"))
            {
                var cookie = cookies.Where(item => item.Name == "gid").First();
                var gid = cookie.Value;
                var guestName = Tools.CreateGuestName();
                return new AnonymousUserInfo(gid, guestName);
            }
            else
            {
                var gid = "";
                var guestName = Tools.CreateGuestName();
                return new AnonymousUserInfo(gid, guestName);
            }
        }
        public static string CreateGuestName()
        {
            var rnd = new Random();
            var num = rnd.Next(100000, 1000000);
            return "guest" + num;
        }
        public static DateTime UnixTime2DateTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        }
        public static bool IsValidRoomUrl(string input)
        {
            return Regex.IsMatch(input, "mildom.com/\\d+");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <exception cref="ParseException"></exception>
        /// <returns></returns>
        public static T Deserialize<T>(string json)
        {
            T low;
            try
            {
                low = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                throw new ParseException(json, ex);
            }
            return low;
        }

        public static long? ExtractRoomId(string input)
        {
            var match = Regex.Match(input, "mildom.com/(\\d+)");
            if (match.Success)
            {
                return long.Parse(match.Groups[1].Value);
            }
            else
            {
                return null;
            }
        }
        public static async Task<Dictionary<long, GiftItem>> GetGiftDict(IDataServer server)
        {
            var url = "https://cloudac.mildom.com/nonolive/gappserv/gift/find";
            var headers = new Dictionary<string, string> { };
            var res = await server.GetAsync(url, headers);
            var obj = Tools.Deserialize<Low.gift_find.RootObject>(res);
            var dict = new Dictionary<long, GiftItem>();
            foreach (var item in obj.Body.Models)
            {
                dict.Add(item.GiftId, new GiftItem(item));
            }
            foreach (var item in obj.Body.Pack)
            {
                dict.Add(item.GiftId, new GiftItem(item));
            }
            return dict;
        }
    }
    class GiftItem
    {
        public string Name { get; }
        public string Url { get; }
        public long Id { get; }
        public GiftItem(string name)
        {
            Name = name;
        }
        public GiftItem(Low.gift_find.GiftItem low)
        {
            Name = low.Name;
            Url = low.Pic;
            Id = low.GiftId;
        }
    }
}
