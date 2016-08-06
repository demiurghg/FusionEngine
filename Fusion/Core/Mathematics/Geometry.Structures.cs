using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Mathematics
{
    public partial class Geometry
    {


        private class BRectangle
        {
            public BPoint2D min;
            public BPoint2D max;
            public int id;
            public BRectangle(Vector2 min, Vector2 max, int id)
            {
                this.id = id;
                this.min = new BPoint2D(min, true, id);
                this.max = new BPoint2D(max, false, id);
            }
        }

        private class BPoint2D
        {
            public bool isStart = false;
            public Vector2 pos;
            public int id;
            public BPoint2D(Vector2 pos, bool isStart, int id)
            {
                this.isStart = isStart;
                this.pos = pos;
                this.id = id;
            }
        }
    }
}
