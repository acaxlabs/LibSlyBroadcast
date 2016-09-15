# LibSlyBroadcast

A library for using the SlyBroadcast API from .NET

```csharp
using LibSlyBroadcast;
using LibSlyBroadcast.Extensions;

class MainClass
{
    public static void Main(string[] args)
    {
        var broadcast = new SlyBroadcast(
            userId: "[your userid]",
            password: "[your password]",
            phoneNumbers: new[] { "1111111111", "2222222222" },
            recordAudio: "RickRoll",
            callerId: "0000000000",
            deliveryDate: DateTime.UtcNow.AddMinutes(5).ToFormattedEstString(),
            audioType: WavOrMp3.Wav,
            mobileOnly: true)
        // The `With*` methods are _not_ required.
        .WithPostbackUrl("http://example.com/endpoint")
        .WithEndDate(DateTime.UtcNow.AddHours(2).ToFormattedEstString());

        var url = broadcast.SendMessage();
        Console.WriteLine(url);
    }
}
```
