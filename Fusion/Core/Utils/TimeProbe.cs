using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Fusion.Core.Utils {

	public class TimeProbe : IDisposable {
		
		string what;
		Stopwatch sw;

		public TimeProbe( string what )
		{
			this.what = what;
			sw = new Stopwatch();
			sw.Start();
		}


		public void Dispose ()
		{
			sw.Stop();
			Log.Message("{0} : {1} ms", what, sw.Elapsed.TotalMilliseconds );
		}
	}
}
