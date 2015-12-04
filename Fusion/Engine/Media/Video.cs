using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Engine.Common;
using Fusion.Engine.Graphics;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using System.IO;
using Fusion.Core.Content;

namespace Fusion.Engine.Media {

	
	[ContentLoader(typeof(Video))]
	public class VideoLoader : ContentLoader {

		public override object Load ( GameEngine game, Stream stream, Type requestedType, string assetPath )
		{
			return new Video( stream );
		}
	}


	public enum VideoSoundtrackType {
		/// <summary>
		/// This video contains only music.
		/// </summary>
		Music,

		/// <summary>
		/// This video contains only dialog.
		/// </summary>
		Dialog,

		/// <summary>
		/// This video contains music and dialog.
		/// </summary>
		MusicAndDialog,
	}


	/// <summary>
	/// Represents a video.
	/// </summary>
	public sealed partial class Video : DisposableBase {

		/// <summary>
		/// File name
		/// </summary>
		//public string FileName { get; private set; }

		/// <summary>
		/// Gets the duration of the Video.
		/// </summary>
		public TimeSpan Duration { get; internal set; }

		/// <summary>
		/// Gets the frame rate of this video.
		/// </summary>
		public float FramesPerSecond { get; internal set; }

		/// <summary>
		/// Gets the height of this video, in pixels.
		/// </summary>
		public int Height { get; internal set; }

		/// <summary>
		/// Gets the VideoSoundtrackType for this video.
		/// </summary>
		public VideoSoundtrackType VideoSoundtrackType { get; internal set; }

		/// <summary>
		/// Gets the width of this video, in pixels.
		/// </summary>
		public int Width { get; internal set; }



		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		public Video(string fileName)
		{
			PlatformInitialize( null, null, fileName );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		public Video(Stream stream)
		{
			PlatformInitialize( null, stream, null );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		public Video(byte[] bytes)
		{
			PlatformInitialize( bytes, null, null );
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				PlatformDispose(disposing);
			}
		}
	

		/*-----------------------------------------------------------------------------------------
		 * 
		 *	Media session stuff :
		 * 
		-----------------------------------------------------------------------------------------*/
	
		private Topology _topology;
		internal Topology Topology { get { return _topology; } }

		internal VideoSampleGrabber SampleGrabber { get { return sampleGrabber; } }
		VideoSampleGrabber sampleGrabber;

		MediaType _mediaType;

		internal DynamicTexture VideoFrame { get { return videoFrame; } }
		DynamicTexture videoFrame;


		/// <summary>
		/// 
		/// </summary>
		private void PlatformInitialize( byte[] bytes, Stream stream, string url )
		{
			if (Topology != null) {
				return;
			}

			MediaFactory.CreateTopology(out _topology);

			SharpDX.MediaFoundation.MediaSource mediaSource;
			{
				SourceResolver resolver = new SourceResolver();
				
				ObjectType otype;
				ComObject source = null;

				if (url!=null) {
					source = resolver.CreateObjectFromURL(url, SourceResolverFlags.MediaSource, null, out otype);
				}

				if (stream!=null) {
					var bs = new ByteStream( stream );
					source = resolver.CreateObjectFromStream(bs, null, SourceResolverFlags.MediaSource, null, out otype);
				}

				if (bytes!=null) {
					var bs = new ByteStream( bytes );
					source = resolver.CreateObjectFromStream(bs, null, SourceResolverFlags.MediaSource|SourceResolverFlags.ContentDoesNotHaveToMatchExtensionOrMimeType, null, out otype);
				}

				if (source==null) {
					throw new ArgumentException("'stream' and 'url' are null!");
				}

				mediaSource = source.QueryInterface<SharpDX.MediaFoundation.MediaSource>();

				
				resolver.Dispose();
				source.Dispose();
			}


			PresentationDescriptor presDesc;
			mediaSource.CreatePresentationDescriptor(out presDesc);
			
			for (var i = 0; i < presDesc.StreamDescriptorCount; i++) {

				RawBool selected = false;
				StreamDescriptor desc;
				presDesc.GetStreamDescriptorByIndex(i, out selected, out desc);
				
				if (selected) {

					TopologyNode sourceNode;
					MediaFactory.CreateTopologyNode(TopologyType.SourceStreamNode, out sourceNode);

					sourceNode.Set(TopologyNodeAttributeKeys.Source, mediaSource);
					sourceNode.Set(TopologyNodeAttributeKeys.PresentationDescriptor, presDesc);
					sourceNode.Set(TopologyNodeAttributeKeys.StreamDescriptor, desc);
					

					TopologyNode outputNode;
					MediaFactory.CreateTopologyNode(TopologyType.OutputNode, out outputNode);

					var majorType = desc.MediaTypeHandler.MajorType;
					
					if (majorType == MediaTypeGuids.Video) {

						Activate activate;
						
						sampleGrabber = new VideoSampleGrabber();

						_mediaType = new MediaType();
						
						_mediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);

						// Specify that we want the data to come in as RGB32.
						_mediaType.Set(MediaTypeAttributeKeys.Subtype, new Guid("00000016-0000-0010-8000-00AA00389B71"));

						MediaFactory.CreateSampleGrabberSinkActivate(_mediaType, SampleGrabber, out activate);
						outputNode.Object = activate;


						long frameSize = desc.MediaTypeHandler.CurrentMediaType.Get<long>(MediaTypeAttributeKeys.FrameSize);

						Width	= (int)(frameSize >> 32);
						Height	= (int) (frameSize & 0x0000FFFF);
					}

					if (majorType == MediaTypeGuids.Audio)
					{
						Activate activate;
						MediaFactory.CreateAudioRendererActivate(out activate);

						outputNode.Object = activate;
					}

					_topology.AddNode(sourceNode);
					_topology.AddNode(outputNode);
					sourceNode.ConnectOutput(0, outputNode, 0);
					

					Duration = new TimeSpan(presDesc.Get<long>(PresentationDescriptionAttributeKeys.Duration));
					

					sourceNode.Dispose();
					outputNode.Dispose();
				}

				desc.Dispose();
			}

			presDesc.Dispose();
			mediaSource.Dispose();


			videoFrame = new DynamicTexture(GameEngine.Instance.GraphicsEngine, Width, Height, typeof(ColorBGRA), false, false);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="disposing"></param>
		private void PlatformDispose(bool disposing)
		{						
			SafeDispose( ref videoFrame );
			SafeDispose( ref _topology );
			SafeDispose( ref sampleGrabber );
			SafeDispose( ref _mediaType );
		}
	}
}
