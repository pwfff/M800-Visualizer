using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace M800_Visualizer
{
    class GamesenseEvent
    {
        private Uri sseUri;

        // Sample: {"game":"AUDIOVISUALIZER","event":"AUDIO","data":{"values":[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]}}
        [JsonProperty("game")]
        public const string Game = "AUDIOVISUALIZER";

        [JsonProperty("event")]
        public const string Event = "AUDIO";

        [JsonProperty("data")]
        public GamesenseAudioVisualizerEventData Data;

        public GamesenseEvent(Uri sseUri, List<byte> values)
        {
            this.sseUri = sseUri;
            Data = new GamesenseAudioVisualizerEventData(values);
        }

        async public Task SendEvent()
        {
            using (var client = new WebClient()) {
                client.Headers[HttpRequestHeader.ContentType] = "application/json";

                await client.UploadStringTaskAsync(sseUri, "POST", JsonConvert.SerializeObject(this));
                /* exception handling? what would we even do...
                try {
                    result = (await client.UploadStringTaskAsync(new Uri(url), "POST", json));
                }
                catch (WebException exception) {
                    string responseText;

                    if (exception.Response != null) {
                        var responseStream = exception.Response.GetResponseStream();

                        if (responseStream != null) {
                            using (var reader = new StreamReader(responseStream)) {
                                responseText = reader.ReadToEnd();
                                Debug.WriteLine(responseText);
                            }
                        }
                    }
                }
                */
            }
        }
    }

    class GamesenseAudioVisualizerEventData
    {
        [JsonProperty("values")]
        public List<byte> Values;

        public GamesenseAudioVisualizerEventData(List<byte> values)
        {
            Values = values;
        }
    }
}
