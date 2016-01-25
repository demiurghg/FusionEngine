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
			public uint Level;
			public uint LevelMask;
			public uint Width;
			public uint Height;
		}


		enum ShaderFlags {
			BITONIC_SORT = 1,
			TRANSPOSE = 2,
		}

		const int NumberOfElements		=	512*512;
		const int BitonicBlockSize		=	512;
		const int TransposeBlockSize	=	16;
		const int MatrixWidth			=	BitonicBlockSize;
		const int MatrixHeight			=	NumberOfElements / BitonicBlockSize;

		ConstantBuffer		paramsCB;
		StructuredBuffer	tempBuffer;
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
			tempBuffer	=	new StructuredBuffer( GraphicsDevice, typeof(Vector2), NumberOfElements  , StructuredBufferFlags.None );
			paramsCB	=	new ConstantBuffer( GraphicsDevice, typeof(Params) );
			shader		=	Content.Load<Ubershader>("test");
			factory		=	new StateFactory( shader, typeof(ShaderFlags), Primitive.TriangleList, VertexInputElement.Empty );

			//
			//	Create and write data :
			//
			var	rand	=	new Random();
			var	input	=	Enumerable.Range(0, NumberOfElements).Select( i => new Vector2( rand.NextFloat(0,100), i ) ).ToArray();
			buffer1.SetData( input );
			
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
				SafeDispose( ref buffer1 );
				SafeDispose( ref buffer2 );
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

			if (e.Key == Keys.F2) {
				Parameters.ToggleVSync();
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


		    //	First sort the rows for the levels <= to the block size
			for( uint level=2; level<=BitonicBlockSize; level = level * 2 ) {

				SetConstants( level, level, MatrixWidth, MatrixHeight );

				// Sort the row data
				GraphicsDevice.SetCSRWBuffer( 0, buffer1 );
				GraphicsDevice.PipelineState	=	factory[ (int)ShaderFlags.BITONIC_SORT ];
				GraphicsDevice.Dispatch( NumberOfElements / BitonicBlockSize, 1, 1 );
			}


			for( uint level = (BitonicBlockSize * 2); level <= NumberOfElements; level = level * 2 ){

				SetConstants( (level / BitonicBlockSize), (uint)(level & ~NumberOfElements) / BitonicBlockSize, MatrixWidth, MatrixHeight );

				// Transpose the data from buffer 1 into buffer 2
				GraphicsDevice.ComputeShaderResources[0]	=	null;
				GraphicsDevice.SetCSRWBuffer( 0, buffer2 );
				GraphicsDevice.ComputeShaderResources[0]	=	buffer1;
				GraphicsDevice.PipelineState				=	factory[ (int)ShaderFlags.TRANSPOSE ];
				GraphicsDevice.Dispatch( MatrixWidth / TransposeBlockSize, MatrixHeight / TransposeBlockSize, 1 );

				// Sort the transposed column data
				GraphicsDevice.PipelineState	=	factory[ (int)ShaderFlags.BITONIC_SORT ];
				GraphicsDevice.Dispatch( NumberOfElements / BitonicBlockSize, 1, 1 );


				SetConstants( BitonicBlockSize, level, MatrixWidth, MatrixHeight );

				// Transpose the data from buffer 2 back into buffer 1
				GraphicsDevice.ComputeShaderResources[0]	=	null;
				GraphicsDevice.SetCSRWBuffer( 0, buffer1 );
				GraphicsDevice.ComputeShaderResources[0]	=	buffer2;
				GraphicsDevice.PipelineState				=	factory[ (int)ShaderFlags.TRANSPOSE ];
				GraphicsDevice.Dispatch( MatrixHeight / TransposeBlockSize, MatrixHeight / TransposeBlockSize, 1 );

				// Sort the row data
				GraphicsDevice.PipelineState	=	factory[ (int)ShaderFlags.BITONIC_SORT ];
				GraphicsDevice.Dispatch( NumberOfElements / BitonicBlockSize, 1, 1 );
			}



			//
			//	Check results 
			//
			if (InputDevice.IsKeyDown(Keys.S)) {

				var output = new Vector2[NumberOfElements];

				buffer1.GetData( output );
	
				Log.Message("--------------------");

				for (int i=0; i<NumberOfElements; i++) {
					
					bool error = (i < NumberOfElements-1) ? output[i].X>output[i+1].X : false;
					//bool error = (i < BufferSize-1) ? output[i&0xFFFFFFFE]>output[i&0xFFFFFFFE+1] : false;

					//if (error) {
						Log.Message("{0,4} : {1,6:0.00} - {2,6:0.00} {3}", i, output[i].X, output[i].Y, error?"<- Error":"" );
					//}
				}
			}
		}



		/// <summary>
		/// 
		/// </summary>
		/// <param name="iLevel"></param>
		/// <param name="iLevelMask"></param>
		/// <param name="iWidth"></param>
		/// <param name="iHeight"></param>
		void SetConstants( uint iLevel, uint iLevelMask, uint iWidth, uint iHeight )
		{
			Params p = new Params(){ Level = iLevel, LevelMask = iLevelMask, Width = iWidth, Height = iHeight };

			paramsCB.SetData( p );
			GraphicsDevice.ComputeShaderConstants[0]	= paramsCB ;
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
