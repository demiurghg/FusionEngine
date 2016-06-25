﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX.MediaFoundation;

namespace Fusion.Engine.Media
{
	internal class VideoSampleGrabber : SharpDX.CallbackBase, SampleGrabberSinkCallback
	{
		internal byte[] TextureData { get; private set; }

		public void OnProcessSample(Guid guidMajorMediaType, int dwSampleFlags, long llSampleTime, long llSampleDuration, IntPtr sampleBufferRef, int dwSampleSize)
		{
			if (TextureData == null || TextureData.Length != dwSampleSize) {
				TextureData = new byte[dwSampleSize];
			}

			lock (TextureData) {
				Marshal.Copy(sampleBufferRef, TextureData, 0, dwSampleSize);
				for (int i = 3; i < TextureData.Length; i += 4) {
					TextureData[i] = (byte) 255;
				}
			}
		}

		public void OnSetPresentationClock(PresentationClock presentationClockRef)
		{

		}

		public void OnShutdown()
		{

		}

		public void OnClockPause(long systemTime)
		{

		}

		public void OnClockRestart(long systemTime)
		{

		}

		public void OnClockSetRate(long systemTime, float flRate)
		{

		}

		public void OnClockStart(long systemTime, long llClockStartOffset)
		{

		}

		public void OnClockStop(long hnsSystemTime)
		{

		}
	}
}
