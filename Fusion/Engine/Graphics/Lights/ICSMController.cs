using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Core;
using Fusion.Core.Mathematics;
using Fusion.Core.Configuration;
using Fusion.Engine.Common;
using Fusion.Drivers.Graphics;
using System.Runtime.InteropServices;


namespace Fusion.Engine.Graphics {

	/// <summary>
	/// Defines interface for cacascade shadow map controller.
	/// </summary>
	public interface ICSMController {

		/// <summary>
		/// Indicates whether specifed cascade should be updated.
		/// </summary>
		/// <param name="cascadeIndex"></param>
		/// <returns></returns>
		bool IsCascadeDirty ( int cascadeIndex );

		/// <summary>
		/// Gets number of currently active shadow cascades.
		/// </summary>
		/// <returns></returns>
		int GetActiveCascadeCount ();

		/// <summary>
		/// Gets view matrix for each cascade.
		/// </summary>
		/// <param name="cascadeIndex">Cascade index. Value must be within range 0..CascadedShadowMap.MaxCascadeCount</param>
		/// <returns></returns>
		Matrix GetShadowViewMatrix ( int cascadeIndex );

		/// <summary>
		/// Gets projection matrix for each cascade.
		/// </summary>
		/// <param name="cascadeIndex">Cascade index. Value must be within range 0..CascadedShadowMap.MaxCascadeCount</param>
		/// <returns></returns>
		Matrix GetShadowProjectionMatrix ( int cascadeIndex );
	}
}
