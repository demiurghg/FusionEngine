#if false
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fusion.Drivers.Input;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using SharpDX.DXGI;
using OculusWrap;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using D3D = SharpDX.Direct3D11;

namespace Fusion.Drivers.Graphics.Display {
	class OculusRiftDisplay : BaseDisplay {

		StereoEye[] eyeList = new[] { StereoEye.Left, StereoEye.Right };

		SwapChain				swapChain = null;
		SwapChainDescription	swapChainDesc;
		
		Form window;

		RenderTarget2D backbufferColor1Resolved;
		RenderTarget2D backbufferColor2Resolved;
		RenderTarget2D backbufferColor;


		Wrap	oculus;		// Oculus Wraper
		Hmd		hmd;		// Head mounted display
		EyeTexture[]		eyeTextures;
		OVR.Posef[]			eyeRenderPose  = new OVR.Posef[2];
		OVR.EyeRenderDesc[]	eyeRenderDesc;

		OculusTextureSwapChain[] oculusSwapChains;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parameters"></param>
		public OculusRiftDisplay( Game game, GraphicsDevice device, GraphicsParameters parameters ) : base( game, device, parameters )
		{
			window = CreateForm(parameters, null);

			oculus = new Wrap();

			// Initialize the Oculus runtime.
			oculus.Initialize();

			OVR.GraphicsLuid graphicsLuid;

			// Use the head mounted display, if it's available, otherwise use the debug HMD.
			
			hmd = oculus.Hmd_Create(out graphicsLuid);


			if (hmd == null) {
				MessageBox.Show("Oculus Rift not detected.", "Uh oh", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (hmd.ProductName == string.Empty)
				MessageBox.Show("The HMD is not enabled.", "There's a tear in the Rift", MessageBoxButtons.OK, MessageBoxIcon.Error);


			// Create a backbuffer that's the same size as the HMD's resolution.
			OVR.Sizei backBufferSize;
			backBufferSize.Width	= hmd.Resolution.Width;
			backBufferSize.Height	= hmd.Resolution.Height;



			var deviceFlags = DeviceCreationFlags.None;
			deviceFlags |= parameters.UseDebugDevice ? DeviceCreationFlags.Debug : DeviceCreationFlags.None;

			var driverType = DriverType.Hardware;

			var featureLevel = HardwareProfileChecker.GetFeatureLevel(parameters.GraphicsProfile);


			swapChainDesc = new SwapChainDescription {
				BufferCount			= 1,
				ModeDescription		= new ModeDescription(backBufferSize.Width, backBufferSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
				IsWindowed			= true,
				OutputHandle		= window.Handle,
				SampleDescription	= new SampleDescription(parameters.MsaaLevel, 0),
				SwapEffect			= SwapEffect.Discard,
				Usage				= Usage.RenderTargetOutput| Usage.ShaderInput,
				Flags				= SwapChainFlags.None,
			};


			D3D.Device.CreateWithSwapChain(driverType, deviceFlags, new[] { featureLevel }, swapChainDesc, out d3dDevice, out swapChain);


			var factory = swapChain.GetParent<Factory>();
			factory.MakeWindowAssociation(window.Handle, WindowAssociationFlags.IgnoreAll);

		}



		/// <summary>
		/// 
		/// </summary>
		public override void CreateDisplayResources()
		{
			base.CreateDisplayResources();

			backbufferColor = new RenderTarget2D(device, swapChain.GetBackBuffer<D3D.Texture2D>(0));


			// Configure Stereo settings.
			OVR.Sizei recommenedTex0Size = hmd.GetFovTextureSize(OVR.EyeType.Left, hmd.DefaultEyeFov[0], 1.0f);
			OVR.Sizei recommenedTex1Size = hmd.GetFovTextureSize(OVR.EyeType.Right, hmd.DefaultEyeFov[1], 1.0f);

			int ms	= backbufferColor.SampleCount;

			backbufferColor1	=	new RenderTarget2D( device, ColorFormat.Rgba8, recommenedTex0Size.Width, recommenedTex0Size.Height, ms );
			backbufferDepth1	=	new DepthStencil2D( device, DepthFormat.D24S8, recommenedTex0Size.Width, recommenedTex0Size.Height, ms );
			backbufferColor2	=	new RenderTarget2D( device, ColorFormat.Rgba8, recommenedTex1Size.Width, recommenedTex1Size.Height, ms );
			backbufferDepth2	=	new DepthStencil2D( device, DepthFormat.D24S8, recommenedTex1Size.Width, recommenedTex1Size.Height, ms );

			if (ms>1) {
				backbufferColor1Resolved	=	new RenderTarget2D( device, ColorFormat.Rgba8, recommenedTex0Size.Width, recommenedTex0Size.Height );
				backbufferColor2Resolved	=	new RenderTarget2D( device, ColorFormat.Rgba8, recommenedTex1Size.Width, recommenedTex1Size.Height );
			}


			OVR.FovPort[] eyeFov = new OVR.FovPort[]
			{ 
				hmd.DefaultEyeFov[0], 
				hmd.DefaultEyeFov[1] 
			};

			OVR.Sizei size1 = new OVR.Sizei(recommenedTex0Size.Width, recommenedTex0Size.Height);
			OVR.Sizei size2 = new OVR.Sizei(recommenedTex1Size.Width, recommenedTex1Size.Height);

			OVR.Recti[] eyeRenderViewport	= new OVR.Recti[2];
			eyeRenderViewport[0].Position	= new OVR.Vector2i(0, 0);
			eyeRenderViewport[0].Size		= size1;
			eyeRenderViewport[1].Position	= new OVR.Vector2i(0, 0); ;
			eyeRenderViewport[1].Size		= size2;

			// Query D3D texture data.
			eyeTexture = new OVR.D3D11.D3D11TextureData[2];
			eyeTexture[0].Header.API			= OVR.RenderAPIType.D3D11;
			eyeTexture[0].Header.TextureSize	= size1;
			eyeTexture[0].Header.RenderViewport = eyeRenderViewport[0];
			

			// Right eye uses the same texture, but different rendering viewport.
			eyeTexture[1] = eyeTexture[0];
			eyeTexture[1].Header.RenderViewport = eyeRenderViewport[1];

			if (ms > 1) {
				eyeTexture[0].Texture				= backbufferColor1Resolved.Surface.Resource.NativePointer;
				eyeTexture[0].ShaderResourceView	= backbufferColor1Resolved.SRV.NativePointer;

				eyeTexture[1].Texture				= backbufferColor2Resolved.Surface.Resource.NativePointer;
				eyeTexture[1].ShaderResourceView	= backbufferColor2Resolved.SRV.NativePointer;
			} else {
				eyeTexture[0].Texture				= backbufferColor1.Surface.Resource.NativePointer;
				eyeTexture[0].ShaderResourceView	= backbufferColor1.SRV.NativePointer;

				eyeTexture[1].Texture				= backbufferColor2.Surface.Resource.NativePointer;
				eyeTexture[1].ShaderResourceView	= backbufferColor2.SRV.NativePointer;
			}

			// Configure d3d11.
			OVR.D3D11.D3D11ConfigData d3d11cfg	= new OVR.D3D11.D3D11ConfigData();
			d3d11cfg.Header.API					= OVR.RenderAPIType.D3D11;
			d3d11cfg.Header.BackBufferSize		= new OVR.Sizei(hmd.Resolution.Width, hmd.Resolution.Height);
			d3d11cfg.Header.Multisample			= 1;
			d3d11cfg.Device						= d3dDevice.NativePointer;
			d3d11cfg.DeviceContext				= d3dDevice.ImmediateContext.NativePointer;
			d3d11cfg.BackBufferRenderTargetView = backbufferColor.Surface.RTV.NativePointer;
			d3d11cfg.SwapChain					= swapChain.NativePointer;

			eyeRenderDesc = hmd.ConfigureRendering(d3d11cfg, OVR.DistortionCaps.ovrDistortionCap_Chromatic | OVR.DistortionCaps.ovrDistortionCap_Vignette | OVR.DistortionCaps.ovrDistortionCap_TimeWarp | OVR.DistortionCaps.ovrDistortionCap_Overdrive, eyeFov);
			if (eyeRenderDesc == null) {
				throw new ArgumentNullException("eyeRenderDesc", "Achtung eyeRenderDesc = null");
			}

			// Specify which head tracking capabilities to enable.
			hmd.SetEnabledCaps(OVR.HmdCaps.LowPersistence /*| OVR.HmdCaps.DynamicPrediction*/);

			// Start the sensor which informs of the Rift's pose and motion
			hmd.ConfigureTracking(OVR.TrackingCaps.ovrTrackingCap_Orientation | OVR.TrackingCaps.ovrTrackingCap_MagYawCorrection | OVR.TrackingCaps.ovrTrackingCap_Position, OVR.TrackingCaps.None);

		}



		/// <summary>
		/// 
		/// </summary>
		public override void Prepare()
		{
			hmd.BeginFrame(0);
			
			OVR.EyeType eye = hmd.EyeRenderOrder[0];
			eyeRenderPose[(int)eye] = hmd.GetHmdPosePerEye(eye);
			eye = hmd.EyeRenderOrder[1];
			eyeRenderPose[(int)eye] = hmd.GetHmdPosePerEye(eye);

			var trackingState = hmd.GetTrackingState(oculus.GetTimeInSeconds());
			var hPos = trackingState.HeadPose.ThePose.Position;
			var hRot = trackingState.HeadPose.ThePose.Orientation;

			var left = new OculusRiftSensors.Eye {
					Position	= new Vector3(eyeRenderPose[0].Position.X, eyeRenderPose[0].Position.Y, eyeRenderPose[0].Position.Z),
					Rotation	= new Quaternion(eyeRenderPose[0].Orientation.X, eyeRenderPose[0].Orientation.Y, eyeRenderPose[0].Orientation.Z, eyeRenderPose[0].Orientation.W),
				};

			var right = new OculusRiftSensors.Eye {
					Position	= new Vector3(eyeRenderPose[1].Position.X, eyeRenderPose[1].Position.Y, eyeRenderPose[1].Position.Z),
					Rotation	= new Quaternion(eyeRenderPose[1].Orientation.X, eyeRenderPose[1].Orientation.Y, eyeRenderPose[1].Orientation.Z, eyeRenderPose[1].Orientation.W),
				};


			var leftProj	= oculus.Matrix4f_Projection(eyeRenderDesc[0].Fov, 0.1f, 1000.0f, true).ToMatrix();
			leftProj.Transpose();
			var rightProj	= oculus.Matrix4f_Projection(eyeRenderDesc[1].Fov, 0.1f, 1000.0f, true).ToMatrix();
			rightProj.Transpose();


			left.Projection		= leftProj;
			right.Projection	= rightProj;

			OculusRiftSensors.LeftEye	= left;
			OculusRiftSensors.RightEye	= right;
			OculusRiftSensors.HeadPosition = new Vector3(hPos.X, hPos.Y, hPos.Z);
			OculusRiftSensors.HeadRotation = new Quaternion(hRot.X, hRot.Y, hRot.Z, hRot.W);

			//Console.WriteLine("Cam pose: " + trackingState.CameraPose.Position.X + " " + trackingState.CameraPose.Position.Y + " " +trackingState.CameraPose.Position.Z);
			//Console.WriteLine("Leveled Cam pose: " + trackingState.LeveledCameraPose.Position.X + " " + trackingState.LeveledCameraPose.Position.Y + " " + trackingState.LeveledCameraPose.Position.Z);
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Update()
		{

		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="syncInterval"></param>
		public override void SwapBuffers(int syncInterval)
		{
			if (backbufferColor1Resolved != null) {
				device.Resolve(backbufferColor1, backbufferColor1Resolved);
				device.Resolve(backbufferColor2, backbufferColor2Resolved);
			}


			hmd.SubmitFrame();
		}


			/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref backbufferColor );
				SafeDispose( ref backbufferColor1 );
				SafeDispose( ref backbufferDepth1 );
				SafeDispose( ref backbufferColor2 );
				SafeDispose( ref backbufferDepth2 );

				SafeDispose( ref backbufferColor1Resolved );
				SafeDispose( ref backbufferColor2Resolved );

				SafeDispose( ref swapChain );

				SafeDispose( ref hmd	);
				SafeDispose( ref oculus	);
			}
			base.Dispose( disposing );
		}


		/// <summary>
		/// 
		/// </summary>
		public override Rectangle Bounds
		{
			get {
				return new Rectangle(0, 0, window.ClientSize.Width, window.ClientSize.Height);
			}
		}


		/// <summary>
		/// List of stereo eye to render.
		/// </summary>
		public override StereoEye[] StereoEyeList {
			get { return eyeList; }
		}



		/// <summary>
		/// 
		/// </summary>
		public override RenderTarget2D	BackbufferColor {
			get { 
				if (TargetEye==StereoEye.Left) {
					return backbufferColor1; 
				} else if ( TargetEye==StereoEye.Right ) {
					return backbufferColor2; 
				} else {
					throw new InvalidOperationException("TargetEye must be StereoEye.Left or StereoEye.Right");
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public override DepthStencil2D	BackbufferDepth {
			get {
				if (TargetEye==StereoEye.Left) {
					return backbufferDepth1; 
				} else if ( TargetEye==StereoEye.Right ) {
					return backbufferDepth2; 
				} else {
					throw new InvalidOperationException("TargetEye must be StereoEye.Left or StereoEye.Right");
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		public override Form Window
		{
			get { return window; }
		}


		/// <summary>
		/// 
		/// </summary>
		public override StereoEye TargetEye
		{
			get; set;
		}
		


		bool fullscr = false;


		/// <summary>
		/// Gets and sets fullscreen mode.
		/// </summary>
		public override bool Fullscreen {
			get	{
				return fullscr;
			}
			set {
				if (value!=fullscr) {
					fullscr = value;

					if (fullscr) {
						window.FormBorderStyle	=	FormBorderStyle.None;
						window.WindowState		=	FormWindowState.Maximized;
						window.TopMost			=	true;
					} else {
						window.FormBorderStyle	=	FormBorderStyle.Sizable;
						window.WindowState		=	FormWindowState.Normal;
						window.TopMost			=	false;
					}
				}
			}
		}



		/// <summary>
		/// Contains all the fields used by each eye.
		/// </summary>
		public class EyeTexture : IDisposable
		{
			public EyeTexture(D3D.Device device, OculusTextureSwapChain swapTexture)
			{
				GraphicDevice	= device;
				SwapTexture		= swapTexture;

				Textures = new SharpDX.Direct3D11.Texture2D[SwapTexture.TextureCount];
				RenderTargetViews = new RenderTargetView[Textures.Length];

				for (int i = 0; i < SwapTexture.TextureCount; i++) {
					Textures[i] = new SharpDX.Direct3D11.Texture2D(SwapTexture.Texture2DResources[i]);
					RenderTargetViews[i] = new RenderTargetView(device, Textures[i],
						new RenderTargetViewDescription {Format = Format.R8G8B8A8_UNorm, Dimension = RenderTargetViewDimension.Texture2D});
				}

			}

			#region IDisposable Members

			/// <summary>
			/// Dispose contained fields.
			/// </summary>
			public void Dispose()
			{
				if (SwapTexture != null) {
					SwapTexture.Dispose();
					SwapTexture = null;
				}

				if (Textures != null) {
					foreach (var texture in Textures)
						texture.Dispose();

					Textures = null;
				}

				if (RenderTargetViews != null) {
					foreach (RenderTargetView renderTargetView in RenderTargetViews)
						renderTargetView.Dispose();

					RenderTargetViews = null;
				}

				if (DepthBuffer != null) {
					DepthBuffer.Dispose();
					DepthBuffer = null;
				}

				if (DepthStencilView != null) {
					DepthStencilView.Dispose();
					DepthStencilView = null;
				}
			}

			#endregion

			public D3D.Device						GraphicDevice;
			public OculusTextureSwapChain			SwapTexture;
			public SharpDX.Direct3D11.Texture2D[]	Textures;
			public RenderTargetView[]				RenderTargetViews;
			public Texture2DDescription				DepthBufferDescription;
			public SharpDX.Direct3D11.Texture2D		DepthBuffer;
			public DepthStencilView					DepthStencilView;
			public Viewport							Viewport;
			public OVR.FovPort						FieldOfView;
			public OVR.Recti						ViewportSize;
			public OVR.EyeRenderDesc				RenderDescription;
			public OVR.Vector3f						HmdToEyeViewOffset;
		}

	}
}
#endif