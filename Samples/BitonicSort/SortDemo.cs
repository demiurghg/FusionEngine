using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Mathematics;
using Fusion.Audio;
using Fusion.Content;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Development;

namespace ComputeDemo {
	public class SortDemo : Game {
		/// <summary>
		/// ComputeDemo constructor
		/// </summary>
		public SortDemo ()
			: base()
		{
			//	enable object tracking :
			Parameters.TrackObjects = true;

			//	add services :
			AddService( new SpriteBatch( this ), false, false, 0, 0 );
			AddService( new DebugStrings( this ), true, true, 9999, 9999 );
			AddService( new DebugRender( this ), true, true, 9998, 9998 );

			//	add here additional services :

			//	load configuration for each service :
			LoadConfiguration();

			//	make configuration saved on exit :
			Exiting += FusionGame_Exiting;
		}

		#pragma warning disable 649 

		struct Params {
			public int Size;
			public int Log2Size;
			public int Dummy1;
			public int Dummy2;
		}


		enum ShaderFlags {
			NONE = 0,
		}

		const int BufferSize =	512;
		const int BlockSize =	64;

		ConstantBuffer		paramsCB;
		StructuredBuffer	buffer;
		Ubershader			shader;
		StateFactory		factory;


		/// <summary>
		/// Add services :
		/// </summary>
		protected override void Initialize ()
		{
			
			//	initialize services :
			base.Initialize();

			//	create structured buffers and shaders :
			buffer		=	new StructuredBuffer( GraphicsDevice, typeof(float), BufferSize  , StructuredBufferFlags.None );
			paramsCB	=	new ConstantBuffer( GraphicsDevice, typeof(Params) );
			shader		=	Content.Load<Ubershader>("test");
			factory		=	new StateFactory( shader, typeof(ShaderFlags), Primitive.TriangleList, VertexInputElement.Empty );

			
			
			//	add keyboard handler :
			InputDevice.KeyDown += InputDevice_KeyDown;
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose ( bool disposing )
		{
			if (disposing) {
				SafeDispose( ref factory );
				SafeDispose( ref buffer );
				SafeDispose( ref paramsCB );
			}
			base.Dispose( disposing );
		}



		/// <summary>
		/// Handle keys for each demo
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void InputDevice_KeyDown ( object sender, Fusion.Input.InputDevice.KeyEventArgs e )
		{
			if (e.Key == Keys.F1) {
				//DevCon.Show(this);
			}

			if (e.Key == Keys.F12) {
				GraphicsDevice.Screenshot();
			}

			if (e.Key == Keys.Escape) {
				Exit();
			}
		}



		/// <summary>
		/// Save configuration on exit.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void FusionGame_Exiting ( object sender, EventArgs e )
		{
			SaveConfiguration();
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		protected override void Update ( GameTime gameTime )
		{
			var ds	=	GetService<DebugStrings>();

			ds.Add( Color.Orange, "FPS {0}", gameTime.Fps );
			ds.Add( "F1   - show developer console" );
			ds.Add( "F5   - build content and reload textures" );
			ds.Add( "F12  - make screenshot" );
			ds.Add( "ESC  - exit" );

			base.Update( gameTime );


			//	write data :
			var	rand	=	new Random();

			var	input		=	Enumerable.Range(0, BufferSize).Select( i => rand.NextFloat(0,100) ).ToArray();
			var output		=	new float[BufferSize];

			buffer.SetData( input );

			paramsCB.SetData( new Params(){ Size = BufferSize, Log2Size = MathUtil.LogBase2(BufferSize)-1 } );

			
			//	bind objects :
			GraphicsDevice.SetCSRWBuffer( 0, buffer );
			GraphicsDevice.ComputeShaderConstants[0]	= paramsCB ;
		
			//	set compute shader and dispatch threadblocks :
			GraphicsDevice.PipelineState	=	factory[0];

			//	compute
			GraphicsDevice.Dispatch( MathUtil.IntDivUp(BufferSize,BlockSize) );

			//	get data :
			buffer.GetData( output );



			if (InputDevice.IsKeyDown(Keys.S)) {
				Log.Message("--------------------");
				Log.Message("Size / Log Size = {0} / {1}", BufferSize, MathUtil.LogBase2(BufferSize)-1 );

				for (int i=0; i<BufferSize; i++) {
					
					bool error = (i < BufferSize-1) ? output[i]>output[i+1] : false;
					//bool error = (i < BufferSize-1) ? output[i&0xFFFFFFFE]>output[i&0xFFFFFFFE+1] : false;

					Log.Message("{0,4} : {1,6:0.00} - {2,6:0.00} {3}", i, input[i], output[i], error?"<- Error":"" );
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="stereoEye"></param>
		protected override void Draw ( GameTime gameTime, StereoEye stereoEye )
		{
			base.Draw( gameTime, stereoEye );
		}
	}
}
