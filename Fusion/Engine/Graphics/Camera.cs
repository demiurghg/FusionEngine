using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core.Mathematics;
using Fusion.Drivers.Graphics;
using Fusion.Drivers.Audio;

namespace Fusion.Engine.Graphics {
	public class Camera {
		
		private Matrix	cameraMatrix	;
		private Matrix	cameraMatrixL	;
		private Matrix	cameraMatrixR	;
		private Matrix	viewMatrix		;
		private Matrix	viewMatrixL		;
		private Matrix	viewMatrixR		;
		private Matrix	projMatrix		;
		private Matrix	projMatrixL		;
		private Matrix	projMatrixR		;
		private Vector3	velocity;


		/// <summary>
		/// 
		/// </summary>
		public Camera ()
		{
		}



		/// <summary>
		/// Sets camera up.
		/// </summary>
		/// <param name="viewMatrix">View matrix. The left-eye and right-eye view matricies will be constructed from this matrix.</param>
		/// <param name="height">Frustum with at near plane.</param>
		/// <param name="width">Frustum height ar near place.</param>
		/// <param name="near">Camera near clipping plane distance.</param>
		/// <param name="far">Camera far clipping plane distance.</param>
		/// <param name="convergenceDistance">Stereo convergence distance. </param>
		/// <param name="separation">Stereo separation or distance between eyes.</param>
		public void SetupCamera ( Matrix viewMatrix, Vector3 velocity, float height, float width, float near, float far, float convergence, float separation )
		{
			float offset		=	separation / convergence * near / 2;
			float nearHeight	=	height;
			float nearWidth		=	width;

			//	Projection :
			this.projMatrix		=	Matrix.PerspectiveOffCenterRH( -nearWidth/2, nearWidth/2, -nearHeight/2, nearHeight/2, near, far );
			this.projMatrixR	=	Matrix.PerspectiveOffCenterRH( -nearWidth/2 - offset, nearWidth/2 - offset, -nearHeight/2, nearHeight/2, near, far );
			this.projMatrixL	=	Matrix.PerspectiveOffCenterRH( -nearWidth/2 + offset, nearWidth/2 + offset, -nearHeight/2, nearHeight/2, near, far );
																					
			//	View :
			this.viewMatrix		=	viewMatrix;
			this.viewMatrixL	=	viewMatrix	*	Matrix.Translation( Vector3.UnitX * separation / 2 );
			this.viewMatrixR	=	viewMatrix	*	Matrix.Translation( -Vector3.UnitX * separation / 2 );

			//	Camera :
			this.cameraMatrix	=	Matrix.Invert( viewMatrix );
			this.cameraMatrixL	=	Matrix.Invert( viewMatrixL );
			this.cameraMatrixR	=	Matrix.Invert( viewMatrixR );

			//	Camera velocity :
			this.velocity	=	velocity;
		}



		/// <summary>
		/// Setups camera.
		/// </summary>
		/// <param name="origin">Camera's origin</param>
		/// <param name="target">Vector directed toward the target</param>
		/// <param name="up">Camera's up vector</param>
		/// <param name="velocity">Camera current velocity</param>
		/// <param name="fov">Camera FOV in radians</param>
		/// <param name="near">Near Z-clipping plane distance</param>
		/// <param name="far">Fat Z-clipping place distance</param>
		/// <param name="convergence">Stereo convergence distance</param>
		/// <param name="separation">Stereo camera separation</param>
		/// <param name="aspectRatio">Viewport width divided by viewport height</param>
		public void SetupCameraFov ( Vector3 origin, Vector3 target, Vector3 up, Vector3 velocity, float fov, float near, float far, float convergence, float separation, float aspectRatio )
		{
			var nearHeight	=	near * (float)Math.Tan( fov/2 ) * 2;
			var nearWidth	=	nearHeight * aspectRatio;
			var view		=	Matrix.LookAtRH( origin, target, up );

			SetupCamera( view, velocity, nearHeight, nearWidth, near, far, convergence, separation );
		}



		/// <summary>
		/// Setups camera.
		/// </summary>
		/// <param name="view"></param>
		/// <param name="velocity"></param>
		/// <param name="fov"></param>
		/// <param name="near"></param>
		/// <param name="far"></param>
		/// <param name="convergence"></param>
		/// <param name="separation"></param>
		public void SetupCameraFov ( Matrix view, Vector3 velocity, float fov, float near, float far, float convergence, float separation, float aspectRatio )
		{
			var nearHeight	=	near * (float)Math.Tan( fov/2 ) * 2;
			var nearWidth	=	nearHeight * aspectRatio;

			SetupCamera( view, velocity, nearHeight, nearWidth, near, far, convergence, separation );
		}



		/// <summary>
		/// Gets view matrix for given stereo eye.
		/// </summary>
		/// <param name="stereoEye"></param>
		/// <returns></returns>
		public Matrix GetViewMatrix ( StereoEye stereoEye )
		{
			if (stereoEye==StereoEye.Mono) return viewMatrix;
			if (stereoEye==StereoEye.Left) return viewMatrixL;
			if (stereoEye==StereoEye.Right) return viewMatrixR;
			throw new ArgumentException("stereoEye");
		}


		/// <summary>
		/// Gets projection matrix for given stereo eye.
		/// </summary>
		/// <param name="stereoEye"></param>
		/// <returns></returns>
		public Matrix GetProjectionMatrix ( StereoEye stereoEye )
		{
			if (stereoEye==StereoEye.Mono) return projMatrix;
			if (stereoEye==StereoEye.Left) return projMatrixL;
			if (stereoEye==StereoEye.Right) return projMatrixR;
			throw new ArgumentException("stereoEye");
		}



		/// <summary>
		/// Gets camera matrix for given stereo eye.
		/// </summary>
		/// <param name="stereoEye"></param>
		/// <returns></returns>
		public Matrix GetCameraMatrix ( StereoEye stereoEye )
		{
			if (stereoEye==StereoEye.Mono) return cameraMatrix;
			if (stereoEye==StereoEye.Left) return cameraMatrixL;
			if (stereoEye==StereoEye.Right) return cameraMatrixR;
			throw new ArgumentException("stereoEye");
		}



		/// <summary>
		/// Gets bounding frustum for camera
		/// </summary>
		/// <returns></returns>
		public BoundingFrustum Frustum {
			get {
				return new BoundingFrustum( viewMatrix * projMatrix );
			}
		}



		/// <summary>
		/// Gets audio listener attached to camera 
		/// </summary>
		/// <returns></returns>
		public AudioListener Listener {
			get {
				return new Drivers.Audio.AudioListener() {
					Position	=	cameraMatrix.TranslationVector,
					Up			=	cameraMatrix.Up,
					Forward		=	cameraMatrix.Forward,
					Velocity	=	velocity,
				};
			}
		}



		/// <summary>
		/// Gets current camera velocity
		/// </summary>
		/// <returns></returns>
		public Vector3 Velocity {
			get {
				return velocity;
			}
		}
	}
}
