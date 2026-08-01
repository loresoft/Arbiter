using System.Collections.Frozen;

namespace Arbiter.Services;

/// <summary>
/// Maps common file extensions to media types and media types to canonical file extensions.
/// </summary>
/// <remarks>
/// <para>
/// Lookups are case-insensitive and accept an extension with or without a leading dot,
/// a bare file name, a full path, or a URL. Any query string or fragment suffix after
/// the extension is ignored.
/// </para>
/// <para>
/// On .NET 9 and later, lookups use <see cref="FrozenDictionary{TKey, TValue}"/> alternate
/// lookup support for <see cref="ReadOnlySpan{T}"/> keys to avoid allocating lookup strings.
/// </para>
/// <para>
/// On .NET 8, the same mappings are used with string-based dictionary lookups.
/// </para>
/// </remarks>
public static class MediaTypeMapping
{
    /// <summary>Application media type names.</summary>
    public static class Application
    {
        /// <summary>GZip archive media type.</summary>
        public const string Gzip = "application/gzip";
        /// <summary>BZip2 archive media type.</summary>
        public const string BZip2 = "application/x-bzip2";
        /// <summary>Brotli compressed data media type.</summary>
        public const string Brotli = "application/x-brotli";
        /// <summary>7-Zip archive media type.</summary>
        public const string SevenZip = "application/x-7z-compressed";
        /// <summary>ZIP archive media type.</summary>
        public const string Zip = "application/zip";
        /// <summary>RAR archive media type.</summary>
        public const string Rar = "application/vnd.rar";
        /// <summary>Tar archive media type.</summary>
        public const string Tar = "application/x-tar";

        /// <summary>EPUB document media type.</summary>
        public const string Epub = "application/epub+zip";

        /// <summary>JSON document media type.</summary>
        public const string Json = "application/json";
        /// <summary>JSON Lines document media type.</summary>
        public const string NdJson = "application/x-ndjson";
        /// <summary>JSON-LD document media type.</summary>
        public const string JsonLd = "application/ld+json";
        /// <summary>Web application manifest JSON media type.</summary>
        public const string ManifestJson = "application/manifest+json";

        /// <summary>Microsoft Excel binary workbook media type.</summary>
        public const string MsExcel = "application/vnd.ms-excel";
        /// <summary>Microsoft PowerPoint binary presentation media type.</summary>
        public const string MsPowerPoint = "application/vnd.ms-powerpoint";
        /// <summary>Microsoft Word binary document media type.</summary>
        public const string MsWord = "application/msword";

        /// <summary>OpenDocument presentation media type.</summary>
        public const string OpenDocumentPresentation = "application/vnd.oasis.opendocument.presentation";
        /// <summary>OpenDocument spreadsheet media type.</summary>
        public const string OpenDocumentSpreadsheet = "application/vnd.oasis.opendocument.spreadsheet";
        /// <summary>OpenDocument text media type.</summary>
        public const string OpenDocumentText = "application/vnd.oasis.opendocument.text";

        /// <summary>Generic binary stream media type.</summary>
        public const string OctetStream = "application/octet-stream";

        /// <summary>Office Open XML spreadsheet media type.</summary>
        public const string OpenXmlExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        /// <summary>Office Open XML presentation media type.</summary>
        public const string OpenXmlPowerPoint = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        /// <summary>Office Open XML word processing document media type.</summary>
        public const string OpenXmlWord = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        /// <summary>PDF document media type.</summary>
        public const string Pdf = "application/pdf";
        /// <summary>Rich Text Format document media type.</summary>
        public const string Rtf = "application/rtf";

        /// <summary>TOML document media type.</summary>
        public const string Toml = "application/toml";
        /// <summary>YAML document media type.</summary>
        public const string Yaml = "application/yaml";

        /// <summary>WebAssembly binary media type.</summary>
        public const string Wasm = "application/wasm";

        /// <summary>XML document media type.</summary>
        public const string Xml = "application/xml";
        /// <summary>XML DTD media type.</summary>
        public const string XmlDtd = "application/xml-dtd";
        /// <summary>XHTML document media type.</summary>
        public const string Xhtml = "application/xhtml+xml";

    }

    /// <summary>Audio media type names.</summary>
    public static class Audio
    {
        /// <summary>AAC audio media type.</summary>
        public const string Aac = "audio/aac";
        /// <summary>FLAC audio media type.</summary>
        public const string Flac = "audio/flac";
        /// <summary>MP4 audio media type.</summary>
        public const string Mp4 = "audio/mp4";
        /// <summary>MPEG audio media type.</summary>
        public const string Mpeg = "audio/mpeg";
        /// <summary>Ogg audio media type.</summary>
        public const string Ogg = "audio/ogg";
        /// <summary>Opus audio media type.</summary>
        public const string Opus = "audio/opus";
        /// <summary>WAV audio media type.</summary>
        public const string Wav = "audio/wav";
        /// <summary>WebM audio media type.</summary>
        public const string WebM = "audio/webm";
    }

    /// <summary>Font media type names.</summary>
    public static class Font
    {
        /// <summary>TrueType Collection font media type.</summary>
        public const string Collection = "font/collection";

        /// <summary>OpenType font media type.</summary>
        public const string Otf = "font/otf";
        /// <summary>SFNT font media type.</summary>
        public const string Sfnt = "font/sfnt";
        /// <summary>TrueType font media type.</summary>
        public const string Ttf = "font/ttf";
        /// <summary>Web Open Font Format media type.</summary>
        public const string Woff = "font/woff";
        /// <summary>Web Open Font Format 2 media type.</summary>
        public const string Woff2 = "font/woff2";
    }

    /// <summary>Image media type names.</summary>
    public static class Image
    {
        /// <summary>Animated PNG image media type.</summary>
        public const string Apng = "image/apng";
        /// <summary>AVIF image media type.</summary>
        public const string Avif = "image/avif";
        /// <summary>BMP image media type.</summary>
        public const string Bmp = "image/bmp";
        /// <summary>GIF image media type.</summary>
        public const string Gif = "image/gif";
        /// <summary>HEIC image media type.</summary>
        public const string Heic = "image/heic";
        /// <summary>HEIF image media type.</summary>
        public const string Heif = "image/heif";
        /// <summary>Icon image media type.</summary>
        public const string Icon = "image/x-icon";
        /// <summary>JPEG 2000 image media type.</summary>
        public const string Jp2 = "image/jp2";
        /// <summary>JPEG image media type.</summary>
        public const string Jpeg = "image/jpeg";
        /// <summary>JPEG XL image media type.</summary>
        public const string Jxl = "image/jxl";
        /// <summary>PNG image media type.</summary>
        public const string Png = "image/png";
        /// <summary>SVG image media type.</summary>
        public const string Svg = "image/svg+xml";
        /// <summary>TIFF image media type.</summary>
        public const string Tiff = "image/tiff";
        /// <summary>WebP image media type.</summary>
        public const string Webp = "image/webp";
    }

    /// <summary>Text media type names.</summary>
    public static class Text
    {
        /// <summary>CSS stylesheet media type.</summary>
        public const string Css = "text/css";
        /// <summary>CSV document media type.</summary>
        public const string Csv = "text/csv";
        /// <summary>Calendar document media type.</summary>
        public const string Calendar = "text/calendar";
        /// <summary>HTML document media type.</summary>
        public const string Html = "text/html";
        /// <summary>JavaScript media type.</summary>
        public const string JavaScript = "text/javascript";
        /// <summary>Markdown document media type.</summary>
        public const string Markdown = "text/markdown";
        /// <summary>Plain text media type.</summary>
        public const string Plain = "text/plain";
        /// <summary>Rich text media type.</summary>
        public const string RichText = "text/richtext";
        /// <summary>XML text media type.</summary>
        public const string Xml = "text/xml";
    }

    /// <summary>Video media type names.</summary>
    public static class Video
    {
        /// <summary>Flash video media type.</summary>
        public const string Flv = "video/x-flv";
        /// <summary>Matroska video media type.</summary>
        public const string Matroska = "video/x-matroska";
        /// <summary>MP4 video media type.</summary>
        public const string Mp4 = "video/mp4";
        /// <summary>MPEG video media type.</summary>
        public const string Mpeg = "video/mpeg";
        /// <summary>AVI video media type.</summary>
        public const string MsVideo = "video/x-msvideo";
        /// <summary>Windows Media Video media type.</summary>
        public const string MsWmv = "video/x-ms-wmv";
        /// <summary>Ogg video media type.</summary>
        public const string Ogg = "video/ogg";
        /// <summary>QuickTime video media type.</summary>
        public const string QuickTime = "video/quicktime";
        /// <summary>3GPP video media type.</summary>
        public const string ThreeGPP = "video/3gpp";
        /// <summary>WebM video media type.</summary>
        public const string WebM = "video/webm";
    }

    /// <summary>
    /// Single source of truth: media type -> its file extensions (no dot).
    /// The first extension is canonical for media type -> extension lookups;
    /// the first media type listed for a shared extension wins the reverse lookup.
    /// </summary>
    private static readonly (string MediaType, string[] Extensions)[] Definitions =
    [
        // application
        (Application.BZip2,                     ["bz2"]),
        (Application.Brotli,                    ["br"]),
        (Application.Epub,                      ["epub"]),
        (Application.Gzip,                      ["gz", "gzip"]),
        (Application.Json,                      ["json", "map"]),
        (Application.JsonLd,                    ["jsonld"]),
        (Application.ManifestJson,              ["webmanifest"]),
        (Application.NdJson,                    ["ndjson"]),
        (Application.OctetStream,               ["bin", "dll", "pdb", "dat", "blat"]),
        (Application.OpenDocumentText,          ["odt"]),
        (Application.OpenDocumentSpreadsheet,   ["ods"]),
        (Application.OpenDocumentPresentation,  ["odp"]),
        (Application.Pdf,                       ["pdf"]),
        (Application.Rar,                       ["rar"]),
        (Application.Rtf,                       ["rtf"]),
        (Application.Tar,                       ["tar"]),
        (Application.Toml,                      ["toml"]),
        (Application.Wasm,                      ["wasm"]),
        (Application.Xml,                       ["xml"]),   // wins .xml over text/xml
        (Application.XmlDtd,                    ["dtd"]),
        (Application.Yaml,                      ["yaml", "yml"]),
        (Application.SevenZip,                  ["7z"]),
        (Application.Zip,                       ["zip"]),
        (Application.MsWord,                    ["doc"]),
        (Application.OpenXmlWord,               ["docx"]),
        (Application.MsExcel,                   ["xls"]),
        (Application.OpenXmlExcel,              ["xlsx"]),
        (Application.MsPowerPoint,              ["ppt"]),
        (Application.OpenXmlPowerPoint,         ["pptx"]),
        (Application.Xhtml,                     ["xhtml"]),

        // audio
        (Audio.Aac,             ["aac"]),
        (Audio.Flac,            ["flac"]),
        (Audio.Mp4,             ["m4a"]),
        (Audio.Mpeg,            ["mp3"]),
        (Audio.Ogg,             ["ogg", "oga"]),
        (Audio.Opus,            ["opus"]),
        (Audio.Wav,             ["wav"]),
        (Audio.WebM,            ["weba"]),

        // font
        (Font.Collection,       ["ttc"]),
        (Font.Otf,              ["otf"]),
        (Font.Sfnt,             ["sfnt"]),
        (Font.Ttf,              ["ttf"]),
        (Font.Woff,             ["woff"]),
        (Font.Woff2,            ["woff2"]),

        // image
        (Image.Apng,            ["apng"]),
        (Image.Avif,            ["avif"]),
        (Image.Bmp,             ["bmp"]),
        (Image.Gif,             ["gif"]),
        (Image.Icon,            ["ico"]),
        (Image.Jp2,             ["jp2"]),
        (Image.Jpeg,            ["jpg", "jpeg", "jpe", "jfif"]),
        (Image.Jxl,             ["jxl"]),
        (Image.Png,             ["png"]),
        (Image.Svg,             ["svg"]),
        (Image.Tiff,            ["tif", "tiff"]),
        (Image.Webp,            ["webp"]),
        (Image.Heic,            ["heic"]),
        (Image.Heif,            ["heif"]),

        // text
        (Text.Css,              ["css"]),
        (Text.Csv,              ["csv"]),
        (Text.Calendar,         ["ics"]),
        (Text.Html,             ["html", "htm"]),
        (Text.JavaScript,       ["js", "mjs"]),
        (Text.Markdown,         ["md", "markdown"]),
        (Text.Plain,            ["txt", "text", "log", "ini"]),
        (Text.RichText,         ["rtx"]),
        (Text.Xml,              ["xml"]),

        // video
        (Video.Mp4,             ["mp4", "m4v"]),
        (Video.Mpeg,            ["mpeg", "mpg"]),
        (Video.Ogg,             ["ogv"]),
        (Video.QuickTime,       ["mov", "qt"]),
        (Video.WebM,            ["webm"]),
        (Video.MsVideo,         ["avi"]),
        (Video.MsWmv,           ["wmv"]),
        (Video.Flv,             ["flv"]),
        (Video.Matroska,        ["mkv"]),
        (Video.ThreeGPP,        ["3gp"]),
    ];

    // extension (lowercase, no dot) -> media type
    private static readonly FrozenDictionary<string, string> ExtensionToMediaType;

    // media type -> canonical extension (no dot)
    private static readonly FrozenDictionary<string, string> MediaTypeToExtension;

#if NET9_0_OR_GREATER
    // Span-keyed views over the above, so lookups don't need string allocation.
    private static readonly FrozenDictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> ExtensionLookup;
    private static readonly FrozenDictionary<string, string>.AlternateLookup<ReadOnlySpan<char>> MediaTypeLookup;
#endif

    static MediaTypeMapping()
    {
        var extToType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var typeToExt = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (mediaType, extensions) in Definitions)
        {
            typeToExt.TryAdd(mediaType, extensions[0]);   // first extension is canonical
            foreach (var ext in extensions)
                extToType.TryAdd(ext, mediaType);         // first media type wins per shared extension
        }

        ExtensionToMediaType = extToType.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
        MediaTypeToExtension = typeToExt.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

#if NET9_0_OR_GREATER
        ExtensionLookup = ExtensionToMediaType.GetAlternateLookup<ReadOnlySpan<char>>();
        MediaTypeLookup = MediaTypeToExtension.GetAlternateLookup<ReadOnlySpan<char>>();
#endif
    }

    /// <summary>
    /// Gets the media type for an extension, file name, path, or URL
    /// ("png", ".png", "a.png", @"C:\a.png", "https://x.com/a.png?v=2").
    /// Returns <see cref="Application.OctetStream"/> when unknown.
    /// </summary>
    /// <param name="name">The extension, file name, path, or URL to resolve.</param>
    /// <returns>The matching media type, or <see cref="Application.OctetStream"/> when no mapping exists.</returns>
    public static string GetMediaType(ReadOnlySpan<char> name)
    {
        var extension = NormalizeExtension(name);

#if NET9_0_OR_GREATER
        ExtensionLookup.TryGetValue(extension, out var type);
#else
        ExtensionToMediaType.TryGetValue(extension.ToString(), out var type);
#endif

        return type ?? Application.OctetStream;
    }

    /// <summary>
    /// Tries to get the media type for an extension, file name, path, or URL.
    /// </summary>
    /// <param name="name">The extension, file name, path, or URL to resolve.</param>
    /// <param name="mediaType">When this method returns, contains the matching media type if found.</param>
    /// <returns><see langword="true"/> when a matching media type exists; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetMediaType(ReadOnlySpan<char> name, out string? mediaType)
    {
        var extension = NormalizeExtension(name);

#if NET9_0_OR_GREATER
        return ExtensionLookup.TryGetValue(extension, out mediaType);
#else
        return ExtensionToMediaType.TryGetValue(extension.ToString(), out mediaType);
#endif
    }


    /// <summary>
    /// Gets the canonical file extension (with leading dot, e.g. ".png") for a media type,
    /// or <see langword="null"/> when unknown.
    /// </summary>
    /// <param name="mediaType">The media type to resolve.</param>
    /// <returns>The canonical file extension with a leading dot, or <see langword="null"/> when no mapping exists.</returns>
    public static string? GetExtension(ReadOnlySpan<char> mediaType)
        => TryGetExtension(mediaType, out var extension) ? extension : default;

    /// <summary>
    /// Tries to get the canonical file extension (with leading dot) for a media type.
    /// </summary>
    /// <param name="mediaType">The media type to resolve.</param>
    /// <param name="extension">When this method returns, contains the canonical file extension if found.</param>
    /// <returns><see langword="true"/> when a matching extension exists; otherwise, <see langword="false"/>.</returns>
    public static bool TryGetExtension(ReadOnlySpan<char> mediaType, out string? extension)
    {
        var key = mediaType.Trim();

#if NET9_0_OR_GREATER
        if (!key.IsEmpty && MediaTypeLookup.TryGetValue(key, out var ext))
#else
        if (!key.IsEmpty && MediaTypeToExtension.TryGetValue(key.ToString(), out var ext))
#endif
        {
            extension = "." + ext;
            return true;
        }

        extension = null;
        return false;
    }


    /// <summary>
    /// Determines whether the extension, file name, path, or URL maps to a known media type.
    /// </summary>
    /// <param name="name">The extension, file name, path, or URL to test.</param>
    /// <returns><see langword="true"/> when the value maps to a known media type; otherwise, <see langword="false"/>.</returns>
    public static bool IsKnownExtension(ReadOnlySpan<char> name)
    {
        var extension = NormalizeExtension(name);

#if NET9_0_OR_GREATER
        return ExtensionLookup.ContainsKey(extension);
#else
        return ExtensionToMediaType.ContainsKey(extension.ToString());
#endif
    }

    /// <summary>
    /// Determines whether the media type is known to this map.
    /// </summary>
    /// <param name="mediaType">The media type to test.</param>
    /// <returns><see langword="true"/> when the media type is known; otherwise, <see langword="false"/>.</returns>
    public static bool IsKnownMediaType(ReadOnlySpan<char> mediaType)
    {
        var key = mediaType.Trim();

#if NET9_0_OR_GREATER
        return !key.IsEmpty && MediaTypeLookup.ContainsKey(key);
#else
        return !key.IsEmpty && MediaTypeToExtension.ContainsKey(key.ToString());
#endif
    }


    /// <summary>
    /// Normalizes input to an extension slice without a leading dot (no allocation).
    /// Accepts a bare extension, a file name, a full path, or a URL;
    /// any query string or fragment suffix after the extension is ignored.
    /// </summary>
    private static ReadOnlySpan<char> NormalizeExtension(ReadOnlySpan<char> input)
    {
        var value = input.Trim();
        if (value.IsEmpty)
            return default;

        // Reduce a path or URL to its final segment.
        var lastSep = value.LastIndexOfAny('/', '\\');
        if (lastSep >= 0)
            value = value[(lastSep + 1)..];

        // Text after the last dot is the extension. A segment with no dot
        // is treated as the extension itself, so a bare "png" still works.
        var lastDot = value.LastIndexOf('.');
        if (lastDot >= 0)
            value = value[(lastDot + 1)..];

        // Drop URL query string / fragment suffixes from the extension
        // without treating '?' or '#' earlier in a file name as delimiters.
        var cut = value.IndexOfAny('?', '#');
        if (cut >= 0)
            value = value[..cut];

        return value;
    }
}
