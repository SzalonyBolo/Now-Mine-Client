using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NowMineCommon.Enums;
using System;
using System.Diagnostics;

namespace NowMineClient.Helpers
{
    public static class JsonMessageBuilder
    {
        static internal bool GetSuccess(string response)
        {
            try
            {
                var Jobj = JObject.Parse(response);
                var ResponseData = Jobj.GetValue("Success");
                return ResponseData.ToObject<bool>();
            }
            catch(Exception e)
            {
                Debug.WriteLine(string.Format("JSON/ Error in GetSuccess: {0}", e.Message));
                return false;
            }
        }

        internal static bool GetQueueClipResponseData(string response, out uint queueID, out int qPos)
        {
            var success = GetSuccess(response);
            if(success)
            {
                try
                {
                    queueID = (uint)JObject.Parse(response)["QueueID"].ToObject(typeof(uint));
                    qPos = (int)JObject.Parse(response)["QueuePosition"].ToObject(typeof(int));
                }
                catch(Exception e)
                {
                    Debug.WriteLine(string.Format("JSON/ Error in GetQueueClipResponseData: {0}", e.Message));
                    success = false;
                    queueID = 0;
                    qPos = -1;
                }
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
            try
            {
                if (typeof(T) != typeof(String))
                    serializedObj = JsonConvert.SerializeObject(data);
                else
                    serializedObj = data as string;
                var request = new JObject(JCommandProperty(commandType),
                           new JProperty(commandType.ToString(), serializedObj));
                return request.ToString();
            }
            catch(Exception e)
            {
                Debug.WriteLine(string.Format("JSON/ Error in GetDataCommandRequest: {0}", e.Message));
                return string.Empty;
            }
        }

        internal static T GetStandardResponseData<T>(string response, string dataNode)
        {
            try
            {
                var Jobj = JObject.Parse(response);
                var ResponseData = Jobj.GetValue(dataNode);
                return ResponseData.ToObject<T>();
            }
            catch(Exception e)
            {
                Debug.WriteLine(string.Format("JSON/ Error in GetStandardResponseData: {0}", e.Message));
                return default(T);
            }
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
