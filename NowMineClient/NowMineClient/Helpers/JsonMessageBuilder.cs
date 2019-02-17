using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NowMineClient.Models;
using NowMineCommon.Enums;
using NowMineCommon.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NowMineClient.Helpers
{
    public static class JsonMessageBuilder
    {
        static internal bool GetSuccess(string response)
        {
            var Jobj = JObject.Parse(response);
            var ResponseData = Jobj.GetValue("Success");
            return ResponseData.ToObject<bool>();
        }

        internal static bool GetQueueClipResponseData(string response, out uint queueID, out int qPos)
        {
            var success = GetSuccess(response);
            if(success)
            {
                queueID = (uint)JObject.Parse(response)["QueueID"].ToObject(typeof(uint));
                qPos = (int)JObject.Parse(response)["QueuePosition"].ToObject(typeof(int));
            }
            else
            {
                queueID = 0;
                qPos = -1;
            }
            return success;
        }

        internal static string GetStandardCommandRequest(CommandType commandType)
        {
            JObject request = new JObject(JCommandProperty(commandType));
            return request.ToString();
        }

        internal static string GetDataCommandRequest<T>(CommandType commandType, T data)
        {
            string serializedObj;
            if (typeof(T) != typeof(String))
                serializedObj = JsonConvert.SerializeObject(data);
            else
                serializedObj = data as string;
            var request = new JObject(JCommandProperty(commandType),
                       new JProperty(commandType.ToString(), serializedObj));
            return request.ToString();
        }

        internal static T GetStandardResponseData<T>(string response, string dataNode)
        {
            //return JsonConvert.DeserializeObject<T>(response);
            var Jobj = JObject.Parse(response);
            var ResponseData = Jobj.GetValue(dataNode);
            return ResponseData.ToObject<T>();
        }

        internal static T GetStandardResponseData<T>(string response, CommandType dataNode)
        {
            return GetStandardResponseData<T>(response, dataNode.ToString());
        }

        private static JProperty JCommandProperty(CommandType commandType)
        {
            return new JProperty("Command", commandType);
        }
    }
}
