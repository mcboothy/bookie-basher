using Newtonsoft.Json;
using System;
using System.Text;

namespace BookieBasher.Core
{
    public static class Extensions
    {
        public static byte[] Encode(this object obj)
        {
            string json = JsonConvert.SerializeObject(obj);
            return Encoding.UTF8.GetBytes(json);
        }

        public static T Decode<T>(this byte[] bytes)
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static object Decode(this byte[] bytes, Type type)
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(json, type);
        }

        public static T Decode<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
