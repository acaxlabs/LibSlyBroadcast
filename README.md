# LibSlyBroadcast

A library for using the SlyBroadcast API from .NET

```csharp
using LibSlyBroadcast;
using LibSlyBroadcast.Extensions;

public static void Main(string[] args)
{
        var broadcast = new SlyBroadcast(
                userId: "[your userid]",
                password: "[your password]",
                phoneNumbers: new[] { "1111111111" },
                callerId: "0000000000",
                deliveryDate: DateTime.UtcNow.AddMinutes(5).ToFormattedEstString(),
                mobileOnly: true)

                // A single `Using*` method _is_ required.
                .UsingPreRecordedAudio("YourRecordingName") // or `UsingFileUrl`

                // The `With*` methods are _not_ required.
                .WithPostbackUrl("http://example.com/endpoint")
                .WithEndDate(DateTime.UtcNow.AddHours(2).ToFormattedEstString());

        var response = broadcast.SendMessage();
        var responseValues = response.Split(new[] { "\r\n", "\n" });
        Console.WriteLine(string.Join(Environment.NewLine, responseValues));
}
```

This library can not be used with Mono as `TimeZoneInfo.FindSystemTimeZoneById` (used to convert DateTimes to EST) uses the Windows Registry.

Eventually this will want to be rewritten using [NodaTime](http://nodatime.org/).
