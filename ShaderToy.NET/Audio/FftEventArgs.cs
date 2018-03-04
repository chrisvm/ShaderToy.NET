using System;
using System.Diagnostics;
using NAudio.Dsp; // The Complex and FFT are here!

namespace ShaderToy.NET
{
    public class FftEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public FftEventArgs(Complex[] result, float[] wav)
        {
            Result = result;
            Signal = wav;
        }

        [DebuggerStepThrough]
        public FftEventArgs(Complex[] result)
        {
            Result = result;
        }

        public Complex[] Result { get; private set; }
        public float[] Signal { get; private set; }
    }
}
