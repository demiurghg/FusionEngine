#if true
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

		SwapChain	swapChain;

		Form window;

		RenderTarget2D backbufferColor1Resolved;
		RenderTarget2D backbufferColor2Resolved;
		RenderTarget2D backbufferColor;


		Wrap	oculus;		// Oculus Wraper
		Hmd		hmd;		// Head mounted display
		EyeTexture[]		eyeTextures;
		OVR.Posef[]			eyePoses;
		MirrorTexture		mirrorTexture;
		Layers				layers;
		LayerEyeFov			layerEyeFov;
		long				frameIndex;
		double				sampleTime;

		OculusTextureSwapChain[] oculusSwapChains;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="game"></param>
		/// <param name="device"></param>
		/// <param name="parameters"></param>
		public OculusRiftDisplay( Game game, GraphicsDevice device, GraphicsParameters parameters ) : base( game, device, parameters )
		{
			oculus = new Wrap();

			// Initialize the Oculus runtime.
			oculus.Initialize();

			OVR.GraphicsLuid graphicsLuid;
			hmd = oculus.Hmd_Create(out graphicsLuid);

			if (hmd == null) {
				MessageBox.Show("Oculus Rift not detected.", "Uh oh", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			if (hmd.ProductName == string.Empty)
				MessageBox.Show("The HMD is not enabled.", "There's a tear in the Rift", MessageBoxButtons.OK, MessageBoxIcon.Error);


			parameters.Width	= hmd.Resolution.Width;
			parameters.Height	= hmd.Resolution.Height;

			window = CreateForm(parameters, null, false);

			var deviceFlags = DeviceCreationFlags.None;
			deviceFlags |= parameters.UseDebugDevice ? DeviceCreationFlags.Debug : DeviceCreationFlags.None;

			var driverType = DriverType.Hardware;

			var featureLevel = HardwareProfileChecker.GetFeatureLevel(parameters.GraphicsProfile);


			var swapChainDesc = new SwapChainDescription {
				BufferCount			= 1,
				ModeDescription		= new ModeDescription(hmd.Resolution.Width, hmd.Resolution.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
				IsWindowed			= true,
				OutputHandle		= window.Handle,
				SampleDescription	= new SampleDescription(parameters.MsaaLevel, 0),
				SwapEffect			= SwapEffect.Discard,
				Usage				= Usage.RenderTargetOutput| Usage.ShaderInput,
				Flags				= SwapChainFlags.None,
			};


			D3D.Device.CreateWithSwapChain(driverType, deviceFlags, new[] { featureLevel }, swapChainDesc, out d3dDevice, out swapChain);


			var myFactory = swapChain.GetParent<Factory>();
			myFactory.MakeWindowAssociation(window.Handle, WindowAssociationFlags.IgnoreAll);
		}



		/// <summary>
		/// 
		/// </summary>
		public override void CreateDisplayResources()
		{
			base.CreateDisplayResources();

			backbufferColor = new RenderTarget2D(device, swapChain.GetBackBuffer<D3D.Texture2D>(0));

			oculusSwapChains	= new OculusTextureSwapChain[2];
			eyeTextures			= new EyeTexture[2];

			for (int i = 0; i < 2; i++) {
				OVR.Sizei idealSize = hmd.GetFovTextureSize((OVR.EyeType)i, hmd.DefaultEyeFov[i], 1.0f);
				oculusSwapChains[i] = hmd.CreateTextureSwapChain(d3dDevice.NativePointer, idealSize.Width, idealSize.Height);
				
				eyeTextures[i]		= new EyeTexture(device, oculusSwapChains[i]) {
					DepthStencil2D		= new DepthStencil2D(device, DepthFormat.D24S8, idealSize.Width, idealSize.Height),
					Viewport			= new Viewport(0, 0, idealSize.Width, idealSize.Height),
					ViewportSize		= new OVR.Recti(new OVR.Vector2i(0, 0), new OVR.Sizei {Width = idealSize.Width, Height = idealSize.Height})
				};

				//eyeTextures[i].DepthBufferDescription = new Texture2DDescription {
				//	Width			= idealSize.Width,
				//	Height			= idealSize.Height,
				//	ArraySize		= 1,
				//	MipLevels		= 1,
				//	Format			= Format.D32_Float,
				//	CpuAccessFlags	= CpuAccessFlags.None,
				//	Usage			= ResourceUsage.Default,
				//	BindFlags		= BindFlags.DepthStencil,
				//	OptionFlags		= ResourceOptionFlags.None,
				//	SampleDescription = new SampleDescription(1, 0)
				//};

			}


			hmd.CreateMirrorTexture(d3dDevice.NativePointer,
				new OVR.MirrorTextureDesc {
					Format		= OVR.TextureFormat.OVR_FORMAT_R8G8B8A8_UNORM_SRGB,
					Width		= backbufferColor.Width,
					Height		= backbufferColor.Height,
					MiscFlags	 = OVR.TextureMiscFlags.None
				}, out mirrorTexture);


			layers		= new Layers();
			layerEyeFov = layers.AddLayerEyeFov();

			hmd.SetTrackingOriginType(OVR.TrackingOrigin.EyeLevel);

			frameIndex = 0;
			Game.RenderSystem.Width		= eyeTextures[0].Viewport.Width;
			Game.RenderSystem.Height	= eyeTextures[0].Viewport.Height;
		}



		/// <summary>
		/// 
		/// </summary>
		public override void Prepare()
		{
			double				displayMidpoint = hmd.GetPredictedDisplayTime(frameIndex);
			OVR.TrackingState	trackingState	= hmd.GetTrackingState(displayMidpoint);
								eyePoses		= new OVR.Posef[2];


			////////////////// Get Eye poses //////////////////////////////////////////////////////////////////////////
			var leftEyeRenderDesc = hmd.GetRenderDesc(OVR.EyeType.Left, hmd.DefaultEyeFov[0]);
			var rightEyeRenderDesc = hmd.GetRenderDesc(OVR.EyeType.Right, hmd.DefaultEyeFov[1]);

			var eyeRenderDescs = new [] { leftEyeRenderDesc, rightEyeRenderDesc };

			OVR.Vector3f[] hmdToEyeOffset = { leftEyeRenderDesc.HmdToEyeOffset, rightEyeRenderDesc.HmdToEyeOffset };

			hmd.GetEyePoses(frameIndex, 1, hmdToEyeOffset, eyePoses, out sampleTime);
			///////////////////////////////////////////////////////////////////////////////////////////////////////////

			var left = new OculusRiftSensors.Eye {
				Position = eyePoses[0].Position.ToVector3(),
				Rotation = eyePoses[0].Orientation.ToQuaternion(),
			};

			var right = new OculusRiftSensors.Eye {
				Position	= eyePoses[1].Position.ToVector3(),
				Rotation	= eyePoses[1].Orientation.ToQuaternion(),
			};


			var leftProj	= oculus.Matrix4f_Projection(eyeRenderDescs[0].Fov, 0.1f, 1000.0f, OVR.ProjectionModifier.None).ToMatrix();
			leftProj.Transpose();
			var rightProj	= oculus.Matrix4f_Projection(eyeRenderDescs[1].Fov, 0.1f, 1000.0f, OVR.ProjectionModifier.None).ToMatrix();
			rightProj.Transpose();

			left.Projection		= leftProj;
			right.Projection	= rightProj;

			OculusRiftSensors.LeftEye	= left;
			OculusRiftSensors.RightEye	= right;
			OculusRiftSensors.HeadPosition = trackingState.HeadPose.ThePose.Position.ToVector3();
			OculusRiftSensors.HeadRotation = trackingState.HeadPose.ThePose.Orientation.ToQuaternion();
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
			eyeTextures[0].SwapTexture.Commit();
			eyeTextures[1].SwapTexture.Commit();

			layerEyeFov.Header.Type		= OVR.LayerType.EyeFov;
			layerEyeFov.Header.Flags	= OVR.LayerFlags.None;
			layerEyeFov.SensorSampleTime = sampleTime;
			
			for (int i = 0; i < 2; i++) {
				layerEyeFov.ColorTexture[i] = eyeTextures[i].SwapTexture.TextureChain;
				layerEyeFov.Viewport[i]		= eyeTextures[i].ViewportSize;
				layerEyeFov.Fov[i]			= hmd.DefaultEyeFov[i];
				layerEyeFov.RenderPose[i]	= eyePoses[i];
			}

			if (hmd.SubmitFrame(frameIndex, layers) < 0) {
				Log.Warning("OculusRiftDisplay SubmitFrame returned error");
			}

			OVR.SessionStatus sessionStatus;
			hmd.GetSessionStatus(out sessionStatus);
			if (sessionStatus.ShouldQuit > 0)
				Application.Exit();
			if (sessionStatus.ShouldRecenter > 0)
				hmd.RecenterPose();

			frameIndex++;

			var mirrorTextureD3D11 = new SharpDX.Direct3D11.Texture2D(mirrorTexture.GetMirrorBufferPtr());

			d3dDevice.ImmediateContext.CopyResource(mirrorTextureD3D11, backbufferColor.Surface.Resource);

			mirrorTextureD3D11.Dispose();

			swapChain.Present(0, PresentFlags.None);
		}


			/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref backbufferColor );

				oculus.Shutdown();

				SafeDispose( ref eyeTextures );

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
					int textureIndex = eyeTextures[0].SwapTexture.GetCurrentTextureIndex();
					return eyeTextures[0].RenderTargets[textureIndex]; 
				} else if ( TargetEye==StereoEye.Right ) {
					int textureIndex = eyeTextures[1].SwapTexture.GetCurrentTextureIndex();
					return eyeTextures[1].RenderTargets[textureIndex];  
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
					return eyeTextures[0].DepthStencil2D; 
				} else if ( TargetEye==StereoEye.Right ) {
					return eyeTextures[1].DepthStencil2D; 
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


		/// <summary>
		/// Gets and sets fullscreen mode.
		/// </summary>
		public override bool Fullscreen {
			get	{
				return false;
			}
			set {

			}
		}



		/// <summary>
		/// Contains all the fields used by each eye.
		/// </summary>
		internal class EyeTexture : IDisposable
		{
			public EyeTexture(GraphicsDevice graphicDevice, OculusTextureSwapChain swapTexture)
			{
				SwapTexture		= swapTexture;

				Textures		= new SharpDX.Direct3D11.Texture2D[SwapTexture.TextureCount];
				RenderTargets	= new RenderTarget2D[Textures.Length];

				for (int i = 0; i < SwapTexture.TextureCount; i++) {
					Textures[i]			= new SharpDX.Direct3D11.Texture2D(SwapTexture.Texture2DResources[i]);
					RenderTargets[i]	= new RenderTarget2D(graphicDevice, Textures[i], new RenderTargetViewDescription {
						Format		= Format.R8G8B8A8_UNorm,
						Dimension	= RenderTargetViewDimension.Texture2D
					});
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

				if (RenderTargets != null) {
					foreach (var renderTargetView in RenderTargets)
						renderTargetView.Dispose();

					RenderTargets = null;
				}

				if (DepthStencil2D != null) {
					DepthStencil2D.Dispose();

					DepthStencil2D = null;
				}
			}

			#endregion

			public OculusTextureSwapChain			SwapTexture;
			public SharpDX.Direct3D11.Texture2D[]	Textures;
			public RenderTarget2D[]					RenderTargets;
			//public Texture2DDescription				DepthBufferDescription;
			public DepthStencil2D					DepthStencil2D;
			public Viewport							Viewport;
			public OVR.FovPort						FieldOfView;
			public OVR.Recti						ViewportSize;
			public OVR.EyeRenderDesc				RenderDescription;
			public OVR.Vector3f						HmdToEyeViewOffset;
		}

	}


	public static class OculusFusionHelpers
	{
		/// <summary>
		/// Convert a Vector4 to a Vector3
		/// </summary>
		/// <param name="vector4">Vector4 to convert to a Vector3.</param>
		/// <returns>Vector3 based on the X, Y and Z coordinates of the Vector4.</returns>
		public static Vector3 ToVector3(this Vector4 vector4)
		{
			return new Vector3(vector4.X, vector4.Y, vector4.Z);
		}

		/// <summary>
		/// Convert an ovrVector3f to SharpDX Vector3.
		/// </summary>
		/// <param name="ovrVector3f">ovrVector3f to convert to a SharpDX Vector3.</param>
		/// <returns>SharpDX Vector3, based on the ovrVector3f.</returns>
		public static Vector3 ToVector3(this OVR.Vector3f ovrVector3f)
		{
			return new Vector3(ovrVector3f.X, ovrVector3f.Y, ovrVector3f.Z);
		}

		/// <summary>
		/// Convert an ovrMatrix4f to a SharpDX Matrix.
		/// </summary>
		/// <param name="ovrMatrix4f">ovrMatrix4f to convert to a SharpDX Matrix.</param>
		/// <returns>SharpDX Matrix, based on the ovrMatrix4f.</returns>
		public static Matrix ToMatrix(this OVR.Matrix4f ovrMatrix4f)
		{
			return new Matrix(ovrMatrix4f.M11, ovrMatrix4f.M12, ovrMatrix4f.M13, ovrMatrix4f.M14, ovrMatrix4f.M21, ovrMatrix4f.M22, ovrMatrix4f.M23, ovrMatrix4f.M24, ovrMatrix4f.M31, ovrMatrix4f.M32, ovrMatrix4f.M33, ovrMatrix4f.M34, ovrMatrix4f.M41, ovrMatrix4f.M42, ovrMatrix4f.M43, ovrMatrix4f.M44);
		}

		/// <summary>
		/// Converts an ovrQuatf to a SharpDX Quaternion.
		/// </summary>
		public static Quaternion ToQuaternion(this OVR.Quaternionf ovrQuatf)
		{
			return new Quaternion(ovrQuatf.X, ovrQuatf.Y, ovrQuatf.Z, ovrQuatf.W);
		}
	}

}
#endif