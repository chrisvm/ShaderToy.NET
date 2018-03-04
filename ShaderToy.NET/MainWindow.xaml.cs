using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;

using SharpGL;
using SharpGL.SceneGraph;
using Microsoft.Win32;

using NAudio.CoreAudioApi;
using ShaderToy.NET.Helpers;

namespace ShaderToy.NET
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly ShaderScene shaderScene;
        private readonly List<Shader> shaders = new List<Shader>();
		private readonly List<string> textures = new List<string>(17);
        private readonly AudioPlayback aud = new AudioPlayback();
        private readonly Microphone mic;
        private readonly AudioBitmap audioBitmap = new AudioBitmap();

	    private bool updateImageAfterGlLoad;
        private BitmapImage Ch0Image;
        private OpenGL gl;
        
        public MainWindow()
        {
            InitializeComponent();

            LoadWasapiDevicesCombo();
            MicCombo.SelectedIndex = 0;
            mic = new Microphone((MMDevice) MicCombo.SelectedItem);
            
            aud.FftCalculated += OnFftCalculated;
            mic.FftCalculated += OnFftCalculated;

	        shaders.Add(new Shader("Waves", "waves_audio"));
	        shaders.Add(new Shader("Menger", "menger"));
	        shaders.Add(new Shader("Boxy", "boxy_audio"));
	        shaders.Add(new Shader("Waves Remix", "wave_remix_audio"));
	        shaders.Add(new Shader("Polar", "polar_audio"));
	        shaders.Add(new Shader("Music Ball", "music_ball_audio"));
	        shaders.Add(new Shader("Cubescape", "cubescape_audio"));
	        shaders.Add(new Shader("Sea", "sea"));
	        shaders.Add(new Shader("Mandelbrot", "mandelbrot"));

	        shaderScene = new ShaderScene(shaders[0]);
			shaderSelector.ItemsSource = shaders;
			shaderSelector.SelectedIndex = 0;

			InitTextures();

			audioBitmap.OnBitmapUpdated += (s, a) => shaderScene.UpdateTextureBitmap(gl, 0, a.image);
        }

	    private void InitTextures()
	    {
		    var textureCount = 17;
		    for (var index = 0; index < textureCount; index++) {
			    var textureName = $"tex{index:00}.jpg";
			    textures.Add(textureName);
		    }

		    textureSelector.ItemsSource = textures;
		    textureSelector.SelectedIndex = 0;
	    }

        private void OpenGLControl_OpenGLDraw(object sender, OpenGLEventArgs args)
        {
            shaderScene.Draw(gl, (float) OpenGLControl.ActualWidth, (float) OpenGLControl.ActualHeight);
        }

        private void OpenGLControl_OpenGLInitialized(object sender, OpenGLEventArgs args)
        {
            gl = args.OpenGL;
            shaderScene.Initialise(gl);

            // init channel 0 (image)
	        if (updateImageAfterGlLoad) {
				shaderScene.UpdateTextureBitmap(gl, 1, ImageHelper.BitmapImage2Bitmap(Ch0Image));
			}
        }

        private void UpdateChannel0(string uriOfImage)
        {
            Ch0Image = new BitmapImage(new Uri(uriOfImage));
            CHO_ImageBox.Source = Ch0Image;
            shaderScene.UpdateTextureBitmap(gl, 1, ImageHelper.BitmapImage2Bitmap(Ch0Image));
        }

	    private void UpdateChannel0FromResource(string resourceName)
	    {
		    Ch0Image = ResourceHelper.LoadImageFromRecource(resourceName);
		    CHO_ImageBox.Source = Ch0Image;

		    if (gl == null) {
			    updateImageAfterGlLoad = true;
			    return;
		    }
		    shaderScene.UpdateTextureBitmap(gl, 1, ImageHelper.BitmapImage2Bitmap(Ch0Image));
		}
        
        private void OnFftCalculated(object sender, FftEventArgs e)
        {
            NAudio.Dsp.Complex[] result = e.Result;
            Dispatcher.Invoke(new Action(() => {
                SpecAnalyser.Update(result);
                audioBitmap.Update(OpenGLControl.OpenGL, result);
            }));
        }

        private void Play_Audio(object sender, RoutedEventArgs e)
        {
            aud.Play();
        }

        private void Pause_Audio(object sender, RoutedEventArgs e)
        {
            aud.Pause();
        }

        private void Stop_Audio(object sender, RoutedEventArgs e)
        {
            aud.Stop();
        }

        private void Load_Audio(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Audio File";
            ofd.Filter = "All Supported Files (*.wav;*.mp3)|*.wav;*.mp3";
            bool? result = ofd.ShowDialog();
            if(result.HasValue && result.Value)
            {
                aud.Stop();
                aud.Load(ofd.FileName);
                Play_Button.IsEnabled = true;
                Pause_Button.IsEnabled = true;
                Stop_Button.IsEnabled = true;
            }
        }

        private bool isFullScreen = false;
        private void MakeFullScreen(object sender, RoutedEventArgs e)
        {
            if (!isFullScreen)
            {
                ShowTitleBar = false;
                WindowState = WindowState.Maximized;
                MainGrid.ColumnDefinitions[1].Width = new GridLength(0);
                isFullScreen = true;
            }
            else
            {
                ShowTitleBar = true;
                WindowState = WindowState.Normal;
                MainGrid.ColumnDefinitions[1].Width = new GridLength(250);
                isFullScreen = false;
            }

        }

        private void Shader_SelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            shaderScene.ActiveShader = shaders[shaderSelector.SelectedIndex];
        }

	    private void TextureSelector_OnSelectionChanged_SelectedChanged(object sender, SelectionChangedEventArgs e)
	    {
		    var textureName = textures[textureSelector.SelectedIndex];
			UpdateChannel0FromResource($"ShaderToy.NET.Textures.{textureName}");
	    }

		private void Start_Mic(object sender, RoutedEventArgs e)
        {
            mic.StartRecording();
            MicStop_Button.IsEnabled = true;
            MicStart_Button.IsEnabled = false;
        }

        private void Stop_Mic(object sender, RoutedEventArgs e)
        {
            mic.StopRecording();
            MicStop_Button.IsEnabled = false;
            MicStart_Button.IsEnabled = true;
        }

        private void LoadWasapiDevicesCombo()
        {
            var deviceEnum = new MMDeviceEnumerator();
            var devices = deviceEnum.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();


            MicCombo.ItemsSource = devices;
            MicCombo.DisplayMemberPath = "FriendlyName";
        }

        private void Load_Image(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select Image File";
            //ofd.Filter = "All Supported Files (*.wav;*.mp3)|*.wav;*.mp3";
            bool? result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                UpdateChannel0(ofd.FileName);
            }
        }
    }
}
