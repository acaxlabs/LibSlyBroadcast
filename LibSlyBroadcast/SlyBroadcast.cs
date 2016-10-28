using LibSlyBroadcast.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace LibSlyBroadcast
{
    public enum WavOrMp3
    {
        Unset = 0,
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
        static string keyDeliveryDate = "c_date";
        static string keyAudioFormat = "c_audio";
        static string keyMobileOnly = "mobile_only";

        static string keyFileUrl = "c_url";

        // Optional keys
        static string keyPostbackUrl = "c_dispo_url";
        static string keyEndDate = "c_endtime";

        // Required values
        public readonly string UserId;
        public readonly string Password;
        public readonly string[] PhoneNumbers;
        public readonly string CallerId;
        public readonly string DeliveryDate;
        public readonly bool MobileOnly;

        ///<summary>Name of recording created via 'SlyBroadcast Recording Center' or uploaded to site.</summary>
        public string RecordedAudioFileName;

        // Required, but only if the other is also given
        public string FileUrl;
        public WavOrMp3 AudioFormat;

        // Optional values
        public string PostbackUrl;
        public string EndDate;

        bool setPreRecordedAudio = false;
        bool setFileUrlAndAudioFormat = false;

        bool IsValid => (AudioFormat != WavOrMp3.Unset && !string.IsNullOrWhiteSpace(FileUrl))
            || !string.IsNullOrWhiteSpace(RecordedAudioFileName);

        public SlyBroadcast(string userId, string password, string[] phoneNumbers, string callerId, string deliveryDate, bool mobileOnly)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException(nameof(password));

            if (phoneNumbers == null || phoneNumbers.Length == 0)
                throw new InvalidOperationException($"Must not provide an empty '{nameof(phoneNumbers)}' array to SlyBroadcast constructor.");

            if (string.IsNullOrWhiteSpace(callerId))
                throw new ArgumentNullException(nameof(callerId));

            if (string.IsNullOrWhiteSpace(deliveryDate))
                throw new ArgumentNullException(nameof(deliveryDate));

            UserId = userId;
            Password = password;
            PhoneNumbers = phoneNumbers;
            CallerId = callerId;
            DeliveryDate = deliveryDate;
            MobileOnly = mobileOnly;

            for (var i = 0; i < PhoneNumbers.Length; i++)
                PhoneNumbers[i] = PhoneNumbers[i]
                    .Replace("-", "")
                    .Replace(" ", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace(".", "");
        }

        public SlyBroadcast UsingPreRecordedAudio(string audioFileName)
        {
            if (setFileUrlAndAudioFormat)
                throw new InvalidOperationException($"Must not call {nameof(UsingPreRecordedAudio)} after call to {nameof(UsingFileUrl)}");

            if (setPreRecordedAudio)
                throw new InvalidOperationException($"Must not call {nameof(UsingPreRecordedAudio)} multiple times.");

            if (string.IsNullOrWhiteSpace(audioFileName))
                throw new ArgumentNullException(nameof(audioFileName));

            setPreRecordedAudio = true;
            RecordedAudioFileName = audioFileName;
            return this;
        }

        public SlyBroadcast UsingFileUrl(string url, WavOrMp3 audioFormat)
        {
            if (setPreRecordedAudio)
                throw new InvalidOperationException($"Must not call {nameof(UsingFileUrl)} after call to {nameof(UsingPreRecordedAudio)}");

            if (setFileUrlAndAudioFormat)
                throw new InvalidOperationException($"Must not call {nameof(UsingFileUrl)} multiple times.");

            if (audioFormat == WavOrMp3.Unset)
                throw new ArgumentException($"Must not call {nameof(UsingFileUrl)} with {nameof(audioFormat)} of value {WavOrMp3.Unset}", nameof(audioFormat));

            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            setFileUrlAndAudioFormat = true;
            FileUrl = url;
            AudioFormat = audioFormat;
            return this;
        }

        /// <summary>Optional. Must not be URL-Encoded if provided.</summary>
        /// <param name="postbackUrl">The URL to POST data back to. Will be ignored if this is null, whitespace, or empty. Must start with http:// or https://</param>
        public SlyBroadcast WithPostbackUrl(string postbackUrl)
        {
            if (string.IsNullOrWhiteSpace(postbackUrl))
                return this;

            if (!postbackUrl.StartsWith("http://") && !postbackUrl.StartsWith("https://"))
                throw new ArgumentException($"{nameof(postbackUrl)} must start with http:// or https://", nameof(postbackUrl));

            PostbackUrl = postbackUrl;
            return this;
        }

        /// <summary>Optional. Must be URL-Encoded if provided.</summary>
        /// <param name="postbackUrl">The URL to POST data back to. Will be ignored if this is null, whitespace, or empty.</param>
        public SlyBroadcast WithEndDate(string endDate)
        {
            if (string.IsNullOrWhiteSpace(endDate))
                throw new ArgumentNullException(nameof(endDate));

            EndDate = endDate;
            return this;
        }

        Dictionary<string, string> ToParamDictionary()
        {
            if (!IsValid)
                return null;

            var ret = new Dictionary<string, string>
            {
                [keyUserId] = UserId,
                [keyPassword] = Password,
                [keyPhone] = PhoneNumbers.JoinWith(","),
                [keyCallerId] = CallerId,
                [keyDeliveryDate] = DeliveryDate,
                [keyMobileOnly] = MobileOnly ? "1" : "0",
            };

            if (!string.IsNullOrWhiteSpace(PostbackUrl))
                ret[keyPostbackUrl] = HttpUtility.UrlEncode(PostbackUrl);

            if (!string.IsNullOrWhiteSpace(keyEndDate))
                ret[keyEndDate] = EndDate;

            if (AudioFormat == WavOrMp3.Unset)
                ret[keyRecordAudio] = RecordedAudioFileName;
            else
            {
                ret[keyFileUrl] = FileUrl;
                ret[keyAudioFormat] = AudioFormat == WavOrMp3.Mp3 ? "mp3" : "wav";
            }

            return ret;
        }

        /// <returns>LF (0a)-delimited strings, first will be either OK or ERROR depending on the status of the request.</returns>
        public string SendMessage()
        {
            if (!IsValid)
                return null;

            var url = "https://www.mobile-sphere.com/gateway/vmb.php?" +
                ToParamDictionary().Select(kv => kv.Key + "=" + kv.Value)
                .JoinWith("&");

            var req = WebRequest.Create(url);
            using (var resp = req.GetResponse().GetResponseStream())
            using (var s = new StreamReader(resp))
                return s.ReadToEnd();
        }
    }
}
