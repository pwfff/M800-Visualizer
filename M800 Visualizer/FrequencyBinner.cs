using System;
using System.Collections.Generic;
using System.Linq;

namespace M800_Visualizer
{
    class FrequencyBinner
    {
        private int numBins;

        private MaxTrackerBin[] bins;

        public FrequencyBinner(int numBins)
        {
            this.numBins = numBins;

            this.bins = new MaxTrackerBin[numBins];
            for (int i = 0; i < numBins; i++) {
                this.bins[i] = new MaxTrackerBin();
            }
        }

        public List<float> GetBinned(float[] values)
        {
            List<float> binList = new List<float>();

            int samplesPerBin = values.Length / numBins;

            // for each bin...
            for (int i = 0; i < numBins; i++) {
                // create a list of the scaled samples for this bin
                List<float> binSamples = new List<float>();
                for (int n = i * samplesPerBin; n < (i + 1) * samplesPerBin; n += 1) {
                    binSamples.Add(GetPercentOfMaxForBin(values[n], i));
                }

                // add the max value of the scaled samples for this bin
                binList.Add(binSamples.Max());
            }

            return binList;
        }

        private float GetPercentOfMaxForBin(float intensityDB, int bin)
        {
            bins[bin].AddSample(intensityDB);
            return intensityDB / bins[bin].MaxDB;
        }
    }

    class MaxTrackerBin
    {
        private float max;

        public float MaxDB { get { return max; } }

        public void AddSample(float intensity)
        {
            if (!float.IsInfinity(intensity)) {
                max = Math.Max(max, intensity);
            }
        }
    }
}
