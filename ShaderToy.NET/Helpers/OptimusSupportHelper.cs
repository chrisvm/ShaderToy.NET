﻿using System.Runtime.InteropServices;

namespace ShaderToy.NET.Helpers
{
    class OptimusSupportHelper
    {
        //Import nVidia API to force nvOptimusRendering
        //see : http://developer.download.nvidia.com/devzone/devcenter/gamegraphics/files/OptimusRenderingPolicies.pdf

        [DllImport("nvapi64.dll")]
        public static extern int NvAPI_Initialize_64();

        [DllImport("nvapi.dll")]
        public static extern int NvAPI_Initialize_32();
    }
}
