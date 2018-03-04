using System;
using System.Drawing;

namespace ShaderToy.NET
{
    class BitmapUpdateEventArgs : EventArgs
    {

        public BitmapUpdateEventArgs(Bitmap img)
        {
            this.image = img;
        }

        public Bitmap image { get; private set; }
    }
}
