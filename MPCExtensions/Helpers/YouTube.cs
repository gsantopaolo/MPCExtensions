using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MPCExtensions.Helpers
{
    /// <summary>Provides methods to access YouTube streams and data. </summary>
    public static class YouTube
    {

        private const string BotUserAgent = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";

        /// <summary>Gets the default minimum quality. </summary>
        public const YouTubeQuality DefaultMinQuality = YouTubeQuality.Quality144P;

        /// <summary>Returns the best matching YouTube stream URI which has an audio and video channel and is not 3D. </summary>
        /// <returns>The best matching <see cref="YouTubeUri"/> of the video. </returns>
        /// <exception cref="YouTubeUriNotFoundException">The <see cref="YouTubeUri"/> could not be found. </exception>
        /// <exception cref="WebException">An error occurred while downloading the resource. </exception>
        public static async Task<YouTubeUri> GetVideoUriAsync(string youTubeId, YouTubeQuality minQuality, YouTubeQuality maxQuality, CancellationToken token)
        {
            var uris = await GetUrisAsync(youTubeId, token);
            var uri = TryFindBestVideoUri(uris, minQuality, maxQuality);
            if (uri == null)
                throw new YouTubeUriNotFoundException();
            return uri;
        }

        /// <summary>Returns the best matching YouTube stream URI which has an audio and video channel and is not 3D. </summary>
        /// <returns>The best matching <see cref="YouTubeUri"/> of the video. </returns>
        /// <exception cref="YouTubeUriNotFoundException">The <see cref="YouTubeUri"/> could not be found. </exception>
        /// <exception cref="WebException">An error occurred while downloading the resource. </exception>
        public static Task<YouTubeUri> GetVideoUriAsync(string youTubeId, YouTubeQuality maxQuality)
        {
            return GetVideoUriAsync(youTubeId, DefaultMinQuality, maxQuality, CancellationToken.None);
        }

        /// <summary>Returns the best matching YouTube stream URI which has an audio and video channel and is not 3D. </summary>
        /// <returns>The best matching <see cref="YouTubeUri"/> of the video. </returns>
        /// <exception cref="YouTubeUriNotFoundException">The <see cref="YouTubeUri"/> could not be found. </exception>
        /// <exception cref="WebException">An error occurred while downloading the resource. </exception>
        public static Task<YouTubeUri> GetVideoUriAsync(string youTubeId, YouTubeQuality maxQuality, CancellationToken token)
        {
            return GetVideoUriAsync(youTubeId, DefaultMinQuality, maxQuality, token);
        }

        /// <summary>Returns the best matching YouTube stream URI which has an audio and video channel and is not 3D. </summary>
        /// <returns>The best matching <see cref="YouTubeUri"/> of the video. </returns>
        /// <exception cref="YouTubeUriNotFoundException">The <see cref="YouTubeUri"/> could not be found. </exception>
        /// <exception cref="WebException">An error occurred while downloading the resource. </exception>
        public static Task<YouTubeUri> GetVideoUriAsync(string youTubeId, YouTubeQuality minQuality, YouTubeQuality maxQuality)
        {
            return GetVideoUriAsync(youTubeId, minQuality, maxQuality, CancellationToken.None);
        }

        /// <summary>Returns all available URIs (audio-only and video) for the given YouTube ID. </summary>
        /// <returns>The <see cref="YouTubeUri"/>s of the video. </returns>
        /// <exception cref="WebException">An error occurred while downloading the resource. </exception>
        public static Task<YouTubeUri[]> GetUrisAsync(string youTubeId)
        {
            return GetUrisAsync(youTubeId, CancellationToken.None);
        }

        /// <summary>Returns all available URIs (audio-only and video) for the given YouTube ID. </summary>
        /// <returns>The <see cref="YouTubeUri"/>s of the video. </returns>
        /// <exception cref="WebException">An error occurred while downloading the resource. </exception>
        public static async Task<YouTubeUri[]> GetUrisAsync(string youTubeId, CancellationToken token)
        {
            var urls = new List<YouTubeUri>();
            string javaScriptCode = null;

            var response = await HttpGetAsync("https://www.youtube.com/watch?v=" + youTubeId + "&nomobile=1");
            var match = Regex.Match(response, "url_encoded_fmt_stream_map\": ?\"(.*?)\"");
            var data = Uri.UnescapeDataString(match.Groups[1].Value);
            match = Regex.Match(response, "adaptive_fmts\": ?\"(.*?)\"");
            var data2 = Uri.UnescapeDataString(match.Groups[1].Value);

            var arr = Regex.Split(data + "," + data2, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)"); // split by comma but outside quotes
            foreach (var d in arr)
            {
                var url = "";
                var signature = "";
                var tuple = new YouTubeUri();
                foreach (var p in d.Replace("\\u0026", "\t").Split('\t'))
                {
                    var index = p.IndexOf('=');
                    if (index != -1 && index < p.Length)
                    {
                        try
                        {
                            var key = p.Substring(0, index);
                            var value = Uri.UnescapeDataString(p.Substring(index + 1));
                            if (key == "url")
                                url = value;
                            else if (key == "itag")
                                tuple.Itag = int.Parse(value);
                            else if (key == "type" && (value.Contains("video/mp4") || value.Contains("audio/mp4")))
                                tuple.Type = value;
                            else if (key == "sig")
                                signature = value;
                            else if (key == "s")
                            {
                                if (javaScriptCode == null)
                                {
                                    string javaScriptUri;
                                    var urlMatch = Regex.Match(response, "\"\\\\/\\\\/s.ytimg.com\\\\/yts\\\\/jsbin\\\\/html5player-(.+?)\\.js\"");
                                    if (urlMatch.Success)
                                        javaScriptUri = "http://s.ytimg.com/yts/jsbin/html5player-" + urlMatch.Groups[1] + ".js";
                                    else
                                    {
                                        // new format
                                        javaScriptUri = "https://s.ytimg.com/yts/jsbin/player-" +
                                            Regex.Match(response, "\"\\\\/\\\\/s.ytimg.com\\\\/yts\\\\/jsbin\\\\/player-(.+?)\\.js\"").Groups[1] + ".js";
                                    }
                                    javaScriptCode = await HttpGetAsync(javaScriptUri);
                                }

                                signature = YouTubeDecipherer.Decipher(value, javaScriptCode);
                            }
                        }
                        catch 
                        {
                        }
                    }
                }

                if (!string.IsNullOrEmpty(url))
                {
                    if (url.Contains("&signature=") || url.Contains("?signature="))
                        tuple.Uri = new Uri(url, UriKind.Absolute);
                    else
                        tuple.Uri = new Uri(url + "&signature=" + signature, UriKind.Absolute);

                    if (tuple.IsValid)
                        urls.Add(tuple);
                }
            }

            return urls.ToArray();
        }

        private static YouTubeUri TryFindBestVideoUri(IEnumerable<YouTubeUri> uris, YouTubeQuality minQuality, YouTubeQuality maxQuality)
        {
            return uris
                .Where(u => u.HasVideo && u.HasAudio && !u.Is3DVideo && u.VideoQuality >= minQuality && u.VideoQuality <= maxQuality)
                .OrderByDescending(u => u.VideoQuality)
                .FirstOrDefault();
        }

        /// <exception cref="WebException">An error occurred while downloading the resource. </exception>
        private static async Task<string> HttpGetAsync(string uri)
        {
            var handler = new HttpClientHandler { AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip };
            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", BotUserAgent);
                var response = await client.GetAsync(new Uri(uri, UriKind.Absolute));
                return await response.Content.ReadAsStringAsync();
            }
        }


        /// <summary>Returns the title of the YouTube video. </summary>
        /// <exception cref="WebException">An error occurred while downloading the resource. </exception>
        public static Task<string> GetVideoTitleAsync(string youTubeId)
        {
            return GetVideoTitleAsync(youTubeId, CancellationToken.None);
        }

        /// <summary>Returns the title of the YouTube video. </summary>
        /// <exception cref="WebException">An error occurred while downloading the resource. </exception>
        public static async Task<string> GetVideoTitleAsync(string youTubeId, CancellationToken token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", BotUserAgent);
                var response = await client.GetAsync("http://www.youtube.com/watch?v=" + youTubeId + "&nomobile=1", token);
                var html = await response.Content.ReadAsStringAsync();
                var startIndex = html.IndexOf(" title=\"");
                if (startIndex != -1)
                {
                    startIndex = html.IndexOf(" title=\"", startIndex + 1);
                    if (startIndex != -1)
                    {
                        startIndex += 8;
                        var endIndex = html.IndexOf("\">", startIndex);
                        if (endIndex != -1)
                            return html.Substring(startIndex, endIndex - startIndex);
                    }
                }
                return null;
            }
        }



        /// <summary>Returns a thumbnail for the given YouTube ID. </summary>
        /// <exception cref="ArgumentException">The value of 'size' is not defined. </exception>
        public static Uri GetThumbnailUri(string youTubeId, YouTubeThumbnailSize size = YouTubeThumbnailSize.Medium)
        {
            switch (size)
            {
                case YouTubeThumbnailSize.Small:
                    return new Uri("http://img.youtube.com/vi/" + youTubeId + "/default.jpg", UriKind.Absolute);
                case YouTubeThumbnailSize.Medium:
                    return new Uri("http://img.youtube.com/vi/" + youTubeId + "/hqdefault.jpg", UriKind.Absolute);
                case YouTubeThumbnailSize.Large:
                    return new Uri("http://img.youtube.com/vi/" + youTubeId + "/maxresdefault.jpg", UriKind.Absolute);
            }
            throw new ArgumentException("The value of 'size' is not defined.");
        }


    }

    /// <summary>Exception which occurs when no <see cref="YouTubeUri"/> could be found. </summary>
    public class YouTubeUriNotFoundException : Exception
    {
        internal YouTubeUriNotFoundException()
            : base("No matching YouTube video or audio stream URI could be found. " +
                   "The video may not be available in your country, " +
                   "is private or uses RTMPE (protected streaming).")
        { }
    }

    /// <summary>An URI to a YouTube MP4 video or audio stream. </summary>
    public class YouTubeUri
    {
        /// <summary>Gets the Itag of the stream. </summary>
        public int Itag { get; internal set; }

        /// <summary>Gets the <see cref="Uri"/> of the stream. </summary>
        public Uri Uri { get; internal set; }

        /// <summary>Gets the stream type. </summary>
        public string Type { get; internal set; }

        /// <summary>Gets a value indicating whether the stream has audio. </summary>
        public bool HasAudio
        {
            get { return AudioQuality != YouTubeQuality.Unknown && AudioQuality != YouTubeQuality.NotAvailable; }
        }

        /// <summary>Gets a value indicating whether the stream has video. </summary>
        public bool HasVideo
        {
            get { return VideoQuality != YouTubeQuality.Unknown && VideoQuality != YouTubeQuality.NotAvailable; }
        }

        /// <summary>Gets a value indicating whether the stream has 3D video. </summary>
        public bool Is3DVideo
        {
            get
            {
                if (VideoQuality == YouTubeQuality.Unknown)
                    return false;

                return Itag >= 82 && Itag <= 85;
            }
        }

        /// <summary>Gets stream's video quality. </summary>
        public YouTubeQuality VideoQuality
        {
            get
            {
                switch (Itag)
                {
                    // video & audio
                    case 5: return YouTubeQuality.Quality240P;
                    case 6: return YouTubeQuality.Quality270P;
                    case 17: return YouTubeQuality.Quality144P;
                    case 18: return YouTubeQuality.Quality360P;
                    case 22: return YouTubeQuality.Quality720P;
                    case 34: return YouTubeQuality.Quality360P;
                    case 35: return YouTubeQuality.Quality480P;
                    case 36: return YouTubeQuality.Quality240P;
                    case 37: return YouTubeQuality.Quality1080P;
                    case 38: return YouTubeQuality.Quality2160P;

                    // 3d video & audio
                    case 82: return YouTubeQuality.Quality360P;
                    case 83: return YouTubeQuality.Quality480P;
                    case 84: return YouTubeQuality.Quality720P;
                    case 85: return YouTubeQuality.Quality520P;

                    // video only
                    case 133: return YouTubeQuality.Quality240P;
                    case 134: return YouTubeQuality.Quality360P;
                    case 135: return YouTubeQuality.Quality480P;
                    case 136: return YouTubeQuality.Quality720P;
                    case 137: return YouTubeQuality.Quality1080P;
                    case 138: return YouTubeQuality.Quality2160P;
                    case 160: return YouTubeQuality.Quality144P;

                    // audio only
                    case 139: return YouTubeQuality.NotAvailable;
                    case 140: return YouTubeQuality.NotAvailable;
                    case 141: return YouTubeQuality.NotAvailable;
                }

                return YouTubeQuality.Unknown;
            }
        }

        /// <summary>Gets stream's audio quality. </summary>
        public YouTubeQuality AudioQuality
        {
            get
            {
                switch (Itag)
                {
                    // video & audio
                    case 5: return YouTubeQuality.QualityLow;
                    case 6: return YouTubeQuality.QualityLow;
                    case 17: return YouTubeQuality.QualityLow;
                    case 18: return YouTubeQuality.QualityMedium;
                    case 22: return YouTubeQuality.QualityHigh;
                    case 34: return YouTubeQuality.QualityMedium;
                    case 35: return YouTubeQuality.QualityMedium;
                    case 36: return YouTubeQuality.QualityLow;
                    case 37: return YouTubeQuality.QualityHigh;
                    case 38: return YouTubeQuality.QualityHigh;

                    // 3d video & audio
                    case 82: return YouTubeQuality.QualityMedium;
                    case 83: return YouTubeQuality.QualityMedium;
                    case 84: return YouTubeQuality.QualityHigh;
                    case 85: return YouTubeQuality.QualityHigh;

                    // video only
                    case 133: return YouTubeQuality.NotAvailable;
                    case 134: return YouTubeQuality.NotAvailable;
                    case 135: return YouTubeQuality.NotAvailable;
                    case 136: return YouTubeQuality.NotAvailable;
                    case 137: return YouTubeQuality.NotAvailable;
                    case 138: return YouTubeQuality.NotAvailable;
                    case 160: return YouTubeQuality.NotAvailable;

                    // audio only
                    case 139: return YouTubeQuality.QualityLow;
                    case 140: return YouTubeQuality.QualityMedium;
                    case 141: return YouTubeQuality.QualityHigh;
                }
                return YouTubeQuality.Unknown;
            }
        }

        internal bool IsValid
        {
            get { return Uri != null && Uri.ToString().StartsWith("http") && Itag > 0 && Type != null; }
        }
    }

    /// <summary>Enumeration of stream qualities. </summary>
    public enum YouTubeQuality : short
    {
        // video
        Quality144P,
        Quality240P,
        Quality270P,
        Quality360P,
        Quality480P,
        Quality520P,
        Quality720P,
        Quality1080P,
        Quality2160P,

        // audio
        QualityLow,
        QualityMedium,
        QualityHigh,

        NotAvailable,
        Unknown,
    }

    /// <summary>Enumeration of thumbnail sizes. </summary>
    public enum YouTubeThumbnailSize : short
    {
        Small,
        Medium,
        Large
    }

    internal static class YouTubeDecipherer
    {
        public static string Decipher(string cipher, string javaScript)
        {
            //Find "C" in this: var A = B.sig||C (B.s)
            string functNamePattern = @"\.sig\s*\|\|([a-zA-Z0-9\$]+)\("; //Regex Formed To Find Word or DollarSign

            var funcName = Regex.Match(javaScript, functNamePattern).Groups[1].Value;

            if (funcName.Contains("$"))
            {
                funcName = "\\" + funcName; //Due To Dollar Sign Introduction, Need To Escape
            }

            string funcPattern = @funcName + @"=function\(\w+\)\{.*?\},"; //Escape funcName string
            var funcBody = Regex.Match(javaScript, funcPattern, RegexOptions.Singleline).Value; //Entire sig function
            var lines = funcBody.Split(';'); //Each line in sig function

            string idReverse = "", idSlice = "", idCharSwap = ""; //Hold name for each cipher method
            string functionIdentifier = "";
            string operations = "";

            foreach (var line in lines.Skip(1).Take(lines.Length - 2)) //Matches the funcBody with each cipher method. Only runs till all three are defined.
            {
                if (!string.IsNullOrEmpty(idReverse) && !string.IsNullOrEmpty(idSlice) &&
                    !string.IsNullOrEmpty(idCharSwap))
                {
                    break; //Break loop if all three cipher methods are defined
                }

                functionIdentifier = GetFunctionFromLine(line);
                string reReverse = string.Format(@"{0}:\bfunction\b\(\w+\)", functionIdentifier); //Regex for reverse (one parameter)
                string reSlice = string.Format(@"{0}:\bfunction\b\([a],b\).(\breturn\b)?.?\w+\.", functionIdentifier); //Regex for slice (return or not)
                string reSwap = string.Format(@"{0}:\bfunction\b\(\w+\,\w\).\bvar\b.\bc=a\b", functionIdentifier); //Regex for the char swap.

                if (Regex.Match(javaScript, reReverse).Success)
                {
                    idReverse = functionIdentifier; //If def matched the regex for reverse then the current function is a defined as the reverse
                }

                if (Regex.Match(javaScript, reSlice).Success)
                {
                    idSlice = functionIdentifier; //If def matched the regex for slice then the current function is defined as the slice.
                }

                if (Regex.Match(javaScript, reSwap).Success)
                {
                    idCharSwap = functionIdentifier; //If def matched the regex for charSwap then the current function is defined as swap.
                }
            }

            foreach (var line in lines.Skip(1).Take(lines.Length - 2))
            {
                Match m;
                functionIdentifier = GetFunctionFromLine(line);

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idCharSwap)
                {
                    operations += "w" + m.Groups["index"].Value + " "; //operation is a swap (w)
                }

                if ((m = Regex.Match(line, @"\(\w+,(?<index>\d+)\)")).Success && functionIdentifier == idSlice)
                {
                    operations += "s" + m.Groups["index"].Value + " "; //operation is a slice
                }

                if (functionIdentifier == idReverse) //No regex required for reverse (reverse method has no parameters)
                {
                    operations += "r "; //operation is a reverse
                }
            }

            operations = operations.Trim();

            return DecipherWithOperations(cipher, operations);
        }

        private static string ApplyOperation(string cipher, string op)
        {
            switch (op[0])
            {
                case 'r':
                    return new string(cipher.ToCharArray().Reverse().ToArray());

                case 'w':
                    {
                        int index = GetOpIndex(op);
                        return SwapFirstChar(cipher, index);
                    }

                case 's':
                    {
                        int index = GetOpIndex(op);
                        return cipher.Substring(index);
                    }

                default:
                    throw new NotImplementedException("Couldn't find cipher operation.");
            }
        }

        private static string DecipherWithOperations(string cipher, string operations)
        {
            return operations.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)
                .Aggregate(cipher, ApplyOperation);
        }

        private static string GetFunctionFromLine(string currentLine)
        {
            Regex matchFunctionReg = new Regex(@"\w+\.(?<functionID>\w+)\("); //lc.ac(b,c) want the ac part.
            Match rgMatch = matchFunctionReg.Match(currentLine);
            string matchedFunction = rgMatch.Groups["functionID"].Value;
            return matchedFunction; //return 'ac'
        }

        private static int GetOpIndex(string op)
        {
            string parsed = new Regex(@".(\d+)").Match(op).Result("$1");
            int index = Int32.Parse(parsed);

            return index;
        }

        private static string SwapFirstChar(string cipher, int index)
        {
            var builder = new StringBuilder(cipher);
            builder[0] = cipher[index];
            builder[index] = cipher[0];

            return builder.ToString();
        }
    }
}
