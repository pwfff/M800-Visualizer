using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace M800_Visualizer
{
    public partial class MainForm : Form
    {
        private bool _recording = false;

        private IWaveIn waveIn;

        int fftSize = 4096;

        SampleAggregator sampleAggregator;

        private FrequencyBinner binner;
        private int bins = 23;

        float[] fftResults;

        private Uri sseUri;

        public MainForm()
        {
            InitializeComponent();

            // TODO: add coreProps.json file selection
            var corePropsPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\SteelSeries\SteelSeries Engine 3\coreProps.json";
            using (StreamReader file = File.OpenText(corePropsPath)) {
                JsonSerializer serializer = new JsonSerializer();
                Dictionary<string, string> coreProps = JsonConvert.DeserializeObject<Dictionary<string, string>>(file.ReadToEnd());
                sseUri = new Uri("http://" + coreProps["address"] + "/game_event");
            }

            binner = new FrequencyBinner(bins);

            fftResults = new float[fftSize];

            sampleAggregator = new SampleAggregator(fftSize);
            
            // TODO: add device selection
            waveIn = new WasapiLoopbackCapture();
            waveIn.RecordingStopped += (s, e) => {
                _recording = false;
            };
            waveIn.DataAvailable += OnDataAvailable;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_recording) {
                waveIn.StopRecording();
            }
            else {
                _recording = true;
                waveIn.StartRecording();
                SendGameEvent();
            }
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            // the float values come in chunks of 4 bytes
            // first left channel then right channel
            for (int n = 0; n < e.Buffer.Length; n += 8) {
                float lb = BitConverter.ToSingle(e.Buffer, n);
                float rb = BitConverter.ToSingle(e.Buffer, n + 4);
                sampleAggregator.Add(lb, rb);
            }
        }

        async void SendGameEvent()
        {
            // the keyboard can only process one event at a time, and returns when it's ready.
            // for this reason we'll use this as the inner loop.
            while (_recording) {
                sampleAggregator.GetFFTResults(fftResults);

                // take the first X samples since we get frequency ranges beyond our hearing.
                //float[] fftResults2 = new float[fftSize / 4];
                //Array.Copy(fftResults, 0, fftResults2, 0, fftSize / 4);

                // we have way too many samples to fit on the keyboard, so bin them in to 23 values
                List<float> binList = binner.GetBinned(fftResults);

                // scale the values to bytes before sending them to the keyboard
                List<byte> scaledList = binList.Select(x => (byte)(x * byte.MaxValue)).ToList();

                GamesenseEvent gameEvent = new GamesenseEvent(sseUri, scaledList);
                await gameEvent.SendEvent();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            waveIn.StopRecording();
        }
    }
}
