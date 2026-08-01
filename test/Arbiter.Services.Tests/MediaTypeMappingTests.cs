namespace Arbiter.Services.Tests;

public class MediaTypeMappingTests
{
    [Test]
    [Arguments("png", "image/png")]
    [Arguments(".png", "image/png")]
    [Arguments("image.PNG", "image/png")]
    [Arguments("image#1.png", "image/png")]
    [Arguments("image?.png", "image/png")]
    [Arguments("image.png#fragment", "image/png")]
    [Arguments(@"C:\\Temp\\image.jpg", "image/jpeg")]
    [Arguments("https://example.com/assets/app.css?v=1#main", "text/css")]
    [Arguments("tar", "application/x-tar")]
    [Arguments("gz", "application/gzip")]
    [Arguments("gzip", "application/gzip")]
    [Arguments("mp3", "audio/mpeg")]
    [Arguments("m4a", "audio/mp4")]
    [Arguments("7z", "application/x-7z-compressed")]
    [Arguments("rar", "application/vnd.rar")]
    [Arguments("br", "application/x-brotli")]
    [Arguments("map", "application/json")]
    [Arguments("jsonld", "application/ld+json")]
    [Arguments("toml", "application/toml")]
    [Arguments("odt", "application/vnd.oasis.opendocument.text")]
    [Arguments("epub", "application/epub+zip")]
    [Arguments("apng", "image/apng")]
    [Arguments("ics", "text/calendar")]
    [Arguments("app.dll", "application/octet-stream")]
    public void GetMediaTypeWithKnownExtensionReturnsMediaType(string extensionOrFileName, string expected)
    {
        var mediaType = MediaTypeMapping.GetMediaType(extensionOrFileName);

        mediaType.Should().Be(expected);
    }

    [Test]
    public void GetMediaTypeWithUnknownExtensionReturnsDefaultMediaType()
    {
        var mediaType = MediaTypeMapping.GetMediaType("unknown-extension");

        mediaType.Should().Be(MediaTypeMapping.Application.OctetStream);
    }

    [Test]
    public void TryGetMediaTypeWithKnownExtensionReturnsTrue()
    {
        var result = MediaTypeMapping.TryGetMediaType("archive.tar", out var mediaType);

        result.Should().BeTrue();
        mediaType.Should().Be("application/x-tar");
    }

    [Test]
    public void TryGetMediaTypeWithUnknownExtensionReturnsFalse()
    {
        var result = MediaTypeMapping.TryGetMediaType("file.unknown", out _);

        result.Should().BeFalse();
    }

    [Test]
    [Arguments("image/png", ".png")]
    [Arguments("IMAGE/JPEG", ".jpg")]
    [Arguments(" application/gzip ", ".gz")]
    [Arguments("application/x-tar", ".tar")]
    [Arguments("audio/mpeg", ".mp3")]
    [Arguments("application/x-7z-compressed", ".7z")]
    [Arguments("application/vnd.oasis.opendocument.text", ".odt")]
    [Arguments("image/apng", ".apng")]
    [Arguments("text/calendar", ".ics")]
    public void GetExtensionWithKnownMediaTypeReturnsCanonicalExtension(string mediaType, string expected)
    {
        var extension = MediaTypeMapping.GetExtension(mediaType);

        extension.Should().Be(expected);
    }

    [Test]
    public void GetExtensionWithUnknownMediaTypeReturnsNull()
    {
        var extension = MediaTypeMapping.GetExtension("application/unknown");

        extension.Should().BeNull();
    }

    [Test]
    public void TryGetExtensionWithKnownMediaTypeReturnsTrue()
    {
        var result = MediaTypeMapping.TryGetExtension("text/html", out var extension);

        result.Should().BeTrue();
        extension.Should().Be(".html");
    }

    [Test]
    public void TryGetExtensionWithUnknownMediaTypeReturnsFalse()
    {
        var result = MediaTypeMapping.TryGetExtension("application/unknown", out var extension);

        result.Should().BeFalse();
        extension.Should().BeNull();
    }

    [Test]
    [Arguments(".png")]
    [Arguments("file#1.png")]
    [Arguments("file?.png")]
    [Arguments("file.tar")]
    [Arguments("https://example.com/archive.gz?download=true")]
    [Arguments("https://example.com/scripts/app.js.map")]
    [Arguments("https://example.com/_framework/Arbiter.Client.dll")]
    public void IsKnownExtensionWithKnownExtensionReturnsTrue(string extensionOrFileName)
    {
        var result = MediaTypeMapping.IsKnownExtension(extensionOrFileName);

        result.Should().BeTrue();
    }

    [Test]
    public void IsKnownExtensionWithUnknownExtensionReturnsFalse()
    {
        var result = MediaTypeMapping.IsKnownExtension("file.unknown");

        result.Should().BeFalse();
    }

    [Test]
    [Arguments("image/png")]
    [Arguments(" application/x-tar ")]
    [Arguments("audio/wav")]
    [Arguments("application/epub+zip")]
    public void IsKnownMediaTypeWithKnownMediaTypeReturnsTrue(string mediaType)
    {
        var result = MediaTypeMapping.IsKnownMediaType(mediaType);

        result.Should().BeTrue();
    }

    [Test]
    public void IsKnownMediaTypeWithUnknownMediaTypeReturnsFalse()
    {
        var result = MediaTypeMapping.IsKnownMediaType("application/unknown");

        result.Should().BeFalse();
    }
}
