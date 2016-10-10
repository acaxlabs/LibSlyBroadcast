using LibSlyBroadcast.Extensions;
using System;
using System.IO;
using System.Web;

namespace LibSlyBroadcast
{
    public class Postback
    {
        public string CampaignSessionId { get; set; }
        public string DestinationPhoneNumber { get; set; }
        public bool Successful { get; set; }

        public string ReasonForFailure { get; set; }
        public DateTime DeliveryTime { get; set; }

        public string Carrier { get; set; }

        public string[] ToStrings() => new[] {
            $"{nameof(CampaignSessionId)}={CampaignSessionId}",
            $"{nameof(DestinationPhoneNumber)}={DestinationPhoneNumber}",
            $"{nameof(Successful)}={Successful}",
            $"{nameof(ReasonForFailure)}={ReasonForFailure}",
            $"{nameof(DeliveryTime)}={DeliveryTime.ToFormattedString()} America/New_York",
            $"{nameof(Carrier)}={Carrier}"
        };

        /// <summary>Parse the postback into a useable object</summary>
        public static Postback FromEncodedString(string encodedString)
        {
            var decodedString = HttpUtility.UrlDecode(encodedString);
            var items = decodedString.Split('|');
            return FromItems(items);
        }

        const int requiredItemCount = 6;

        /// <summary>Parse the postback into a useable object</summary>
        public static Postback FromItems(string[] items)
        {
            if (items.Length != requiredItemCount)
                throw new InvalidDataException($"Length of '{nameof(items)}' must be {requiredItemCount} but is {items.Length}");

            var campaignSessionId = items[0].Replace("var=", "");
            var destinationPhoneNumber = items[1];
            var status = items[2];
            var reasonForFailure = items[3];
            var deliveryTime = items[4];
            var carrier = items[5];

            DateTime dt;
            return new Postback
            {
                CampaignSessionId = campaignSessionId,
                DestinationPhoneNumber = destinationPhoneNumber,
                Successful = string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase),
                ReasonForFailure = reasonForFailure,
                DeliveryTime = DateTime.TryParse(deliveryTime, out dt) ? dt : default(DateTime),
                Carrier = carrier,
            };
        }
    }
}
