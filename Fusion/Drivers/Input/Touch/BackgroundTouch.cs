using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using Fusion.Core.Mathematics;
using Fusion.Engine.Input;
using Point = Fusion.Core.Mathematics.Point;

namespace Fusion.Input.Touch
{
    public class BackgroundTouch : BaseTouchHandler
    {
	    TouchForm touchForm;


        public BackgroundTouch(TouchForm tForm) : base()
        {
	        touchForm = tForm;

            Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION[] cfg = new Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION[]
            {
                new Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION(Win32TouchFunctions.INTERACTION.TAP,
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.TAP |
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.TAP_DOUBLE),

                new Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION(Win32TouchFunctions.INTERACTION.SECONDARY_TAP,
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.SECONDARY_TAP),

                new Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION(Win32TouchFunctions.INTERACTION.HOLD,
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.HOLD),

					new Win32TouchFunctions.INTERACTION_CONTEXT_CONFIGURATION(Win32TouchFunctions.INTERACTION.MANIPULATION,
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.MANIPULATION |
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.MANIPULATION_SCALING |
					Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.MANIPULATION_TRANSLATION_X |
                    Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.MANIPULATION_TRANSLATION_Y | 
					Win32TouchFunctions.INTERACTION_CONFIGURATION_FLAGS.MANIPULATION_ROTATION)
            };

            Win32TouchFunctions.SetInteractionConfigurationInteractionContext(Context, cfg.Length, cfg);
        }

        internal override void ProcessEvent(InteractionOutput output)
        {
            if (output.Data.Interaction == Win32TouchFunctions.INTERACTION.TAP) {
	            
				var p = touchForm.PointToClient(new System.Drawing.Point((int)output.Data.X, (int)output.Data.Y));

	            if (output.Data.Tap.Count == 1) {
					touchForm.NotifyTap(new TouchEventArgs {
						IsEventBegin	= output.IsBegin(),
						IsEventEnd		= output.IsEnd(),
						Position		= new Point(p.X, p.Y)
					});
	            } 
				else if (output.Data.Tap.Count == 2) {
					touchForm.NotifyDoubleTap(new TouchEventArgs {
						IsEventBegin	= output.IsBegin(),
						IsEventEnd		= output.IsEnd(),
						Position		= new Point(p.X, p.Y)
					});
                }
            }
            else if (output.Data.Interaction == Win32TouchFunctions.INTERACTION.SECONDARY_TAP)
            {
				var p = touchForm.PointToClient(new System.Drawing.Point((int)output.Data.X, (int)output.Data.Y));

				touchForm.NotifyTouchSecondaryTap(new TouchEventArgs {
					IsEventBegin	= output.IsBegin(),
					IsEventEnd		= output.IsEnd(),
					Position		= new Point(p.X, p.Y)
				});
            }
			else if (output.Data.Interaction == Win32TouchFunctions.INTERACTION.HOLD) {
				
			}
			else if (output.Data.Interaction == Win32TouchFunctions.INTERACTION.MANIPULATION) {
				var p	= touchForm.PointToClient(new System.Drawing.Point((int)output.Data.X, (int)output.Data.Y));
				
				touchForm.NotifyTouchManipulation(new TouchEventArgs {
					IsEventBegin	= output.IsBegin(),
					IsEventEnd		= output.IsEnd(),
					Position		= new Point(p.X, p.Y),
					RotationDelta	= output.Data.Manipulation.Delta.Rotation,
					ScaleDelta		= output.Data.Manipulation.Delta.Scale
				} );
				
				//output.IsEnd()
			}
        }
    }
}
