using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace LibSlyBroadcast
{
    public enum WavOrMp3
    {
        Wav,
        Mp3,
    }

    public class SlyBroadcast
    {
        // Required keys
        static string keyUserId = "c_uid";
        static string keyPassword = "c_password";
        static string keyPhone = "c_phone";
        static string keyRecordAudio = "c_record_audio";
        static string keyCallerId = "c_callerID";
        static string keyDate = "c_date";
        static string keyAudio = "c_audio";
        static string keyMobileOnly = "mobile_only";

        // Optional keys
        static string keyPostbackUrl = "c_dispo_url";
        static string keyEndDate = "c_endtime";

        // Required values
        public readonly string UserId;
        public readonly string Password;
        public readonly string[] PhoneNumbers;
        public readonly string RecordAudio;
        public readonly string CallerId;
        public readonly string DeliveryDate;
        public readonly WavOrMp3 AudioType;
        public readonly bool MobileOnly;

        // Optional values
        public string PostbackUrl;
        public string EndDate;

        public SlyBroadcast(string userId, string password, string[] phoneNumbers, string recordAudio, string callerId, string deliveryDate, WavOrMp3 audioType, bool mobileOnly)
        {
            UserId = userId;
            Password = password;
            PhoneNumbers = phoneNumbers;
            RecordAudio = recordAudio;
            CallerId = callerId;
            DeliveryDate = deliveryDate;
            AudioType = audioType;
            MobileOnly = mobileOnly;

            for (var i = 0; i < PhoneNumbers.Length; i++)
                PhoneNumbers[i] = PhoneNumbers[i]
                    .Replace("-", "")
                    .Replace(" ", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace(".", "");
        }

        /// <param name="postbackUrl">Raw URL, not urlencoded.</param>
        public SlyBroadcast WithPostbackUrl(string postbackUrl)
        {
            PostbackUrl = postbackUrl;
            return this;
        }

        public SlyBroadcast WithEndDate(string endDate)
        {
            EndDate = endDate;
            return this;
        }

        Dictionary<string, string> ToParamDictionary()
        {
            var ret = new Dictionary<string, string>
            {
                [keyUserId] = UserId,
                [keyPassword] = Password,
                [keyPhone] = PhoneNumbers.JoinWith(","),
                [keyRecordAudio] = RecordAudio,
                [keyCallerId] = CallerId,
                [keyDate] = DeliveryDate,
                [keyAudio] = AudioType == WavOrMp3.Mp3 ? "mp3" : "wav",
                [keyMobileOnly] = MobileOnly ? "1" : "0",
            };

            if (!string.IsNullOrWhiteSpace(PostbackUrl))
                ret[keyPostbackUrl] = HttpUtility.UrlEncode(PostbackUrl);

            if (!string.IsNullOrWhiteSpace(keyEndDate))
                ret[keyEndDate] = EndDate;

            return ret;
        }

        public string SendMessage()
        {
            var url = "https://www.mobile-sphere.com/gateway/vmb.php?" +
                ToParamDictionary().Select(kv => kv.Key + "=" + kv.Value)
                .JoinWith("&");

            var req = WebRequest.Create(url);
            using (var _ = req.GetResponse().GetResponseStream())
            { }

            return url;
        }
    }
}