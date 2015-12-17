using System;
using System.Collections.Generic;
using System.Linq;

namespace M800_Visualizer
{
    class FrequencyBinner
    {
        private int numBins;

        private MaxTracker tracker;

        public FrequencyBinner(int numBins)
        {
            this.numBins = numBins;

            // 256 is a magic number here. seems to work.
            this.tracker = new MaxTracker(256);
        }

        public List<float> GetBinned(float[] values)
        {
            List<float> binList = new List<float>();
            
            // gonna try logarithmic-ish binning... low frequencies tend to all end up
            // in the first bin, so i want it to be a smaller bin. each bin should be
            // roughly 1/4 to 1/2 an octave? 10 audible octaves in 23 bins

            // the values below cover roughly 0-15,000hz with fftSize = 4096

            // if anyone can turn this into an algorithm/function getSamplesPerBin(bin, fftSize)...
            // please...
            // also not that this doesn't even come close to using all the samples we get in `values`
            int[] samplesPerBin = new int[] {
                3, 3,
                4, 5, 6, 7,
                8, 10, 12, 14,
                16, 20, 24, 28,
                32, 40, 48, 56,
                64, 80, 96, 112,
                128, 256 };

            // for each bin...
            for (int i = 0; i < numBins; i++) {
                // create a list of the scaled samples for this bin
                List<float> binSamples = new List<float>();

                int samplesSoFar = samplesPerBin.Take(i + 1).Sum();

                for (int n = samplesSoFar; n < samplesSoFar + samplesPerBin[i]; n += 1) {
                    binSamples.Add(values[n]);
                }

                // add the max value of the scaled samples for this bin
                // the multiplication here is supposed to account for the
                // fact that bass tones drown out the rest... so the lower
                // the bin, the more of the penalty they get.

                // it's mostly magic (2? why 2??) but it works...
                binList.Add(binSamples.Max() * (i+2)/(numBins));
            }

            float currentMax = binList.Max();
            tracker.AddSample(currentMax);
            float max = Math.Max(currentMax, tracker.AverageIntensity);

            binList = binList.Select(s => { return s / max; }).ToList();
            return binList;
        }
    }

    class MaxTracker
    {
        private float[] intensities;
        private int index = 0;

        public float AverageIntensity
        {
            get { return intensities.Average(); }
        }

        public MaxTracker(int capacity)
        {
            this.intensities = new float[capacity];
        }

        public void AddSample(float intensity)
        {
            intensities[index] = intensity;
            index++;

            if (index == intensities.Length) {
                index = 0;
            }
        }
    }
}