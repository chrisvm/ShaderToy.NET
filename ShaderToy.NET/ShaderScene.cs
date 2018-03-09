using System;
using System.Drawing;
using System.Drawing.Imaging;
using ShaderToy.NET.Helpers;
using SharpGL;
using SharpGL.Shaders;
using SharpGL.VertexBuffers;
using static System.DateTime;

namespace ShaderToy.NET
{
    public class ShaderScene
    {
        //  Constants that specify the attribute indexes.
	    private const uint AttributeIndexPosition = 0;

        //Texture Names Array
        private readonly uint[] _glTextureArray = { 0 , 0 };

	    private float _resolutionX;
	    private float _resolutionY;

	    private float _time;

        //  The vertex buffer array which contains the vertex and texture coords buffers.
	    private VertexBufferArray _vertexBufferArray;

	    private VertexBufferArray _texCoordsBufferArray;

        private bool _needsRefresh = true;

        private Shader _ashader;
        public Shader ActiveShader
        {
            get
            {
                return _ashader;
            }
            set
            {
                _ashader = value;
                _needsRefresh = true;
            }
        }

        //  The shader program for our vertex and fragment shader.
        private ShaderProgram _shaderProgram;
		
		/// <summary>
        /// Initialises the scene.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        public void Initialise(OpenGL gl)
        {
            _time = Now.Millisecond / 1000;

            //  Create the shader program.
            var vertexShaderSource = ResourceHelper.LoadTextFromRecource("ShaderToy.NET.Shaders.main.vert");
            _shaderProgram = new DynamicShaderProgram();
            _shaderProgram.Create(gl, vertexShaderSource, ActiveShader.Source, null);

            _shaderProgram.BindAttributeLocation(gl, AttributeIndexPosition, "position");
            _shaderProgram.AssertValid(gl);

            //Generate Textures
            gl.GenTextures(2, _glTextureArray);
            //shaderProgram.BindAttributeLocation(gl, glTextureArray[0], "iChannel0");
            //shaderProgram.BindAttributeLocation(gl, glTextureArray[1], "iChannel1");
            var ch0Loc = _shaderProgram.GetUniformLocation(gl, "iChannel0");
            gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[0]);
            gl.Uniform1(ch0Loc, 0);

            var ch1Loc = _shaderProgram.GetUniformLocation(gl, "iChannel1");
            gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[1]);
            gl.Uniform1(ch1Loc, 1);

            /*gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, glTextureArray[0]);
            gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, glTextureArray[1]);*/

            //  Now create the geometry for the square.
            CreateVerticesForSquare(gl);

            _needsRefresh = false;
        }

	    /// <summary>
	    /// Draws the scene.
	    /// </summary>
	    /// <param name="gl">The OpenGL instance.</param>
	    /// <param name="width"></param>
	    /// <param name="height"></param>
	    public void Draw(OpenGL gl, float width, float height)
        {
            if (_needsRefresh) Initialise(gl);

            _resolutionX = width;
            _resolutionY = height;

            //  Clear the scene.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT | OpenGL.GL_STENCIL_BUFFER_BIT);

            //  Bind the shader, set the matrices.
            _shaderProgram.Bind(gl);

            _shaderProgram.SetUniform3(gl, "iResolution", _resolutionX, _resolutionY, 0.0f);
            _shaderProgram.SetUniform1(gl, "iGlobalTime", _time);
            
            _time += 0.1f;

            //  Bind the out vertex array.
            _vertexBufferArray.Bind(gl);
            _texCoordsBufferArray.Bind(gl);

            //Bind Textures
            var ch0Loc = _shaderProgram.GetUniformLocation(gl, "iChannel0");
            gl.ActiveTexture(OpenGL.GL_TEXTURE0);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[0]);
            gl.Uniform1(ch0Loc, 0);

            var ch1Loc = _shaderProgram.GetUniformLocation(gl, "iChannel1");
            gl.ActiveTexture(OpenGL.GL_TEXTURE1);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[1]);
            gl.Uniform1(ch1Loc, 1);

            //  Draw the square.
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, 6);

            //  Unbind our vertex array and shader.
            _vertexBufferArray.Unbind(gl);
            _texCoordsBufferArray.Unbind(gl);


            _shaderProgram.Unbind(gl);
        }

        /// <summary>
        /// The width of the texture images.
        /// </summary>
        private readonly int[] _width = { 0, 0 };

        /// <summary>
        /// The height of the texture images.
        /// </summary>
        private readonly int[] _height = { 0, 0 };

        /// <summary>
        /// updates pixel data of the desired texture.
        /// </summary>
        public void UpdateTextureBitmap(OpenGL gl, int texIndex , Bitmap image)
        {
            int[] textureMaxSize = { 0 };
            gl.GetInteger(OpenGL.GL_MAX_TEXTURE_SIZE, textureMaxSize);

            if (image == null) return;
 
            //	Find the target width and height sizes, which is just the highest
            //	posible power of two that'll fit into the image.
            int targetWidth = textureMaxSize[0];
            int targetHeight = textureMaxSize[0];

            //Console.WriteLine("Updating Tex " + texIndex + "Tex Max Size : " + targetWidth + "x" + targetHeight);

            for (int size = 1; size <= textureMaxSize[0]; size *= 2)
            {
                if (image.Width < size)
                {
                    targetWidth = size / 2;
                    break;
                }
                if (image.Width == size)
                    targetWidth = size;

            }

            for (int size = 1; size <= textureMaxSize[0]; size *= 2)
            {
                if (image.Height < size)
                {
                    targetHeight = size / 2;
                    break;
                }
                if (image.Height == size)
                    targetHeight = size;
            }

            //  If need to scale, do so now.
            if (image.Width != targetWidth || image.Height != targetHeight)
            {
                //  Resize the image.
                Image newImage = image.GetThumbnailImage(targetWidth, targetHeight, null, IntPtr.Zero);

                //  Destory the old image, and reset.
                image.Dispose();
                image = (Bitmap)newImage;
            }

            //  Lock the image bits (so that we can pass them to OGL).
            BitmapData bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            //	Set the width and height.
            _width[texIndex] = image.Width;
            _height[texIndex] = image.Height;

	        gl.ActiveTexture(texIndex == 0 ? OpenGL.GL_TEXTURE0 : OpenGL.GL_TEXTURE1);

	        //	Bind our texture object (make it the current texture).
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, _glTextureArray[texIndex]);

            //  Set the image data.
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, (int)OpenGL.GL_RGBA,
                _width[texIndex], _height[texIndex], 0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE,
                bitmapData.Scan0);

            //  Unlock the image.
            image.UnlockBits(bitmapData);

            //  Dispose of the image file.
            image.Dispose();

            //  Set linear filtering mode.
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
        }


        /// <summary>
        /// Creates the geometry for the square, also creating the vertex buffer array.
        /// </summary>
        /// <param name="gl">The OpenGL instance.</param>
        private void CreateVerticesForSquare(OpenGL gl)
        {
            var vertices = new float[18];
            vertices[0] = -1.0f; vertices[1] = -1.0f; vertices[2] = 0.0f; // Bottom left corner  
            vertices[3] = -1.0f; vertices[4] = 1.0f; vertices[5] = 0.0f; // Top left corner  
            vertices[6] = 1.0f; vertices[7] = 1.0f; vertices[8] = 0.0f; // Top Right corner  
            vertices[9] = 1.0f; vertices[10] = -1.0f; vertices[11] = 0.0f; // Bottom right corner  
            vertices[12] = -1.0f; vertices[13] = -1.0f; vertices[14] = 0.0f; // Bottom left corner  
            vertices[15] = 1.0f; vertices[16] = 1.0f; vertices[17] = 0.0f; // Top Right corner   

            var texcoords = new float[12];
            texcoords[0] = 0.0f; texcoords[1] = 0.0f;
            texcoords[2] = 0.0f; texcoords[3] = 1.0f;
            texcoords[4] = 1.0f; texcoords[5] = 1.0f;
            texcoords[6] = 1.0f; texcoords[7] = 0.0f;
            texcoords[8] = 0.0f; texcoords[9] = 0.0f;
            texcoords[10] = 1.0f; texcoords[11] = 1.0f;

            //  Create the vertex array object.
            _vertexBufferArray = new VertexBufferArray();
            _vertexBufferArray.Create(gl);
            _vertexBufferArray.Bind(gl);

            _texCoordsBufferArray = new VertexBufferArray();
            _texCoordsBufferArray.Create(gl);
            _texCoordsBufferArray.Bind(gl);


            //  Create a vertex buffer for the vertex data.
            var vertexDataBuffer = new VertexBuffer();
            vertexDataBuffer.Create(gl);
            vertexDataBuffer.Bind(gl);
            vertexDataBuffer.SetData(gl, 0, vertices, false, 3);

            var texCoordsBuffer = new VertexBuffer();
            texCoordsBuffer.Create(gl);
            texCoordsBuffer.Bind(gl);
            texCoordsBuffer.SetData(gl, 1, texcoords, false, 2);

            //  Now do the same for the colour data.
            /*var colourDataBuffer = new VertexBuffer();
            colourDataBuffer.Create(gl);
            colourDataBuffer.Bind(gl);
            colourDataBuffer.SetData(gl, 1, colors, false, 3);*/

            //  Unbind the vertex array, we've finished specifying data for it.
            _vertexBufferArray.Unbind(gl);
            _texCoordsBufferArray.Unbind(gl);
        }
    }
}
