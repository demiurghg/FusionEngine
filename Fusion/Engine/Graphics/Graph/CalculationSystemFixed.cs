using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fusion;
using Fusion.Input;

namespace Fusion.Engine.Graphics.Graph
{
	class CalculationSystemFixed : CalculationSystem
	{
		float	stepLength;


		public CalculationSystemFixed(LayoutSystem host) : base(host)
		{
			stepLength = 0.5f;//HostSystem.Environment.GetService<GraphSystem>().Config.StepSize;
		}



		protected override void initCalculations() { }


		protected override void resetState()
		{
			stepLength = 0.5f;//HostSystem.Environment.GetService<GraphSystem>().Config.StepSize;
			numIterations = 0;
		}


		protected override void update(int userCommand)
		{
			//var graphSys	= HostSystem.Environment.GetService<GraphSystem>();
			LayoutSystem.ComputeParams param = new LayoutSystem.ComputeParams();

			// manual step change:
			if (userCommand > 0)
			{
				stepLength = increaseStep(stepLength);
			}
			if (userCommand < 0)
			{
				stepLength = decreaseStep(stepLength);
			}

			if (HostSystem.CurrentStateBuffer != null)
			{
				param.StepLength = stepLength;

				if (HostSystem.RunPause == LayoutSystem.State.RUN)
				{
					for (int i = 0; i < 20; ++i) //graphSys.Config.IterationsPerFrame
					{
						HostSystem.CalcDescentVector(HostSystem.CurrentStateBuffer, param);	// calculate current descent vectors

						HostSystem.MoveVertices(HostSystem.CurrentStateBuffer,
							HostSystem.NextStateBuffer, param);	// move vertices in the descent direction

						// swap buffers: --------------------------------------------------------------------
						HostSystem.SwapBuffers();
						++numIterations;
					}
				}
			}
			
			//var debStr = HostSystem.Environment.GetService<DebugStrings>();

			//debStr.Add(Color.Black, "FIXED MODE");
			//debStr.Add(Color.Aqua, "Step factor  = " + stepLength);
			//debStr.Add(Color.Aqua, "Iteration      = " + numIterations);
		}



		/// <summary>
		/// This function returns increased step length
		/// </summary>
		/// <param name="step"></param>
		/// <returns></returns>
		float increaseStep(float step)
		{
			return step + 0.01f;
		}


		/// <summary>
		/// This function returns decreased step length
		/// </summary>
		/// <param name="step"></param>
		/// <returns></returns>
		float decreaseStep(float step)
		{
			return step - 0.01f;
		}
	}
}
