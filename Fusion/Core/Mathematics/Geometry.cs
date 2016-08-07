using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Core.Mathematics
{
    public partial class Geometry
    {
        /// <summary>
        /// Calculate left turn predicate for three points.
        /// </summary>
        public static int PredicateTurn(Vector2 a, Vector2 b, Vector2 c)
        {
            return Math.Sign((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y));
        }


        /// <summary>
        /// Check intersection beetween two triangles.
        /// </summary>
        /// <param name="first">Points of first triangle. First.Count must equals 3</param>
        /// <param name="second">Points of second triangle. First.Count must equals 3</param>
        /// <returns>Return true if triangles intersect otherwise false.</returns>
        public static bool TriangleIntersection(List<Vector2> first, List<Vector2> second)
        {
            if (first == null)// || second == null || first.Count != 3 || second.Count != 3)
            {
                throw new ArgumentNullException("First can't be null");
            }

            if (second == null)
            {
                throw new ArgumentNullException("Second can't be null");
            }

            if (first.Count != 3)
            {
                throw new ArgumentException("First.Count must equals 3");
            }

            if (second.Count != 3)
            {
                throw new ArgumentException("Second.Count must equals 3");
            }

            float fMinX = Math.Min(Math.Min(first[0].X, first[1].X), first[2].X);
            float fMaxX = Math.Max(Math.Max(first[0].X, first[1].X), first[2].X);
            float sMinX = Math.Min(Math.Min(second[0].X, second[1].X), second[2].X);
            float sMaxX = Math.Max(Math.Max(second[0].X, second[1].X), second[2].X);

            if (fMaxX - fMinX > sMaxX - sMinX)
            {
                return TriangleIntersection(first, second, 0);
            }
            return TriangleIntersection(second, first, 0);

        }


        /// <summary>
        /// First triangle must be greater than second.
        /// </summary>
        private static bool TriangleIntersection(List<Vector2> firstTriangle, List<Vector2> secondTriangle, int flag)
        {
            if (PredicateTurn(firstTriangle[0], firstTriangle[1], firstTriangle[2]) != 1)
            {
                Vector2 temp = firstTriangle[1];
                firstTriangle[1] = firstTriangle[2];
                firstTriangle[2] = temp;
            }

            for (int i = 0; i < 3; i++)
            {
                Vector2 a = firstTriangle[i];
                Vector2 b = firstTriangle[(i + 1) % 3];
                int r0 = PredicateTurn(a, b, secondTriangle[0]);
                int r1 = PredicateTurn(a, b, secondTriangle[1]);
                int r2 = PredicateTurn(a, b, secondTriangle[2]);
                if (r0 == 0 && r1 == 0 && r2 == 0)
                {
                    //WARNING. second triangle is Line
                    return true;
                }
                else if (r0 <= 0 && r1 <= 0 && r2 <= 0)
                {
                    return false;
                }
            }

            return true;
        }


        /// <summary>
        /// Check intersection between each triangles.
        /// </summary>
        /// <param name="trianglesPoints">Each three points is coordinates for triangle. (trianglesPoints.Count % 3 must equals 0)</param>
        /// <returns>Return true if some triangles intersect</returns>
        public static bool TrianglesIntersection(List<Vector2> trianglesPoints)
        {

            if (trianglesPoints == null)
            {
                throw new ArgumentNullException("trianglesPoints can't be null");
            }

            if (trianglesPoints.Count % 3 != 0)
            {
                throw new ArgumentException("trianglesPoints.Count % 3 must equals 0");
            }
            //prepare. Put each triangle into bounding rectangle.
            List<BRectangle> bRectangle = new List<BRectangle>(trianglesPoints.Count / 3);
            for (int i = 0; i < trianglesPoints.Count; i += 3)
            {
                Vector2 min = trianglesPoints[i];
                Vector2 max = trianglesPoints[i];
                for (int j = 1; j < 3; j++)
                {
                    min.X = Math.Min(min.X, trianglesPoints[i + j].X);
                    min.Y = Math.Min(min.Y, trianglesPoints[i + j].Y);
                    max.X = Math.Max(max.X, trianglesPoints[i + j].X);
                    max.Y = Math.Max(max.Y, trianglesPoints[i + j].Y);
                }

                bRectangle.Add(new BRectangle(min, max, i / 3));
            }

            List<BPoint2D> points = new List<BPoint2D>(bRectangle.Count * 2);


            //Create event-point(start, end) for each bounding rectangle
            bRectangle.ForEach(t => { points.Add(t.min); points.Add(t.max); });

            //Sort event-point
            points.Sort((a, b) =>
            {
                if (a.pos.X < b.pos.X)
                {
                    return -1;
                }
                else if (a.pos.X > b.pos.X)
                {
                    return 1;
                }
                else
                {
                    if (a.isStart == b.isStart)
                    {
                        return 0;
                    }

                    if (!a.isStart)
                    {
                        return -1;
                    }
                    if (!b.isStart)
                    {
                        return 1;
                    }
                    return 0;
                }
            });


            //Use scan line for OX.

            SortedSet<BRectangle> rectangles = new SortedSet<BRectangle>(Comparer<BRectangle>.Create((a, b) =>
            {
                if (a.id == b.id)
                {
                    return 0;
                }
                else
                {
                    if (a.min.pos.Y.CompareTo(b.min.pos.Y) <= 0)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
            }));


            for (int i = 0; i < points.Count; i++) // O(N)
            {
                BRectangle temp = bRectangle[points[i].id];

                //if event is start, we should check our rectangle with each rectangle in SortedSet
                if (points[i].isStart)
                {
                    foreach (var r in rectangles)
                    {
                        if (r.min.pos.Y > temp.max.pos.Y)
                        {
                            break;
                        }

                        if (temp.min.pos.Y <= r.max.pos.Y && temp.min.pos.Y >= r.min.pos.Y || temp.max.pos.Y <= r.max.pos.Y && temp.max.pos.Y >= r.min.pos.Y)
                        {


                            List<Vector2> firstTriangle = new List<Vector2>() { trianglesPoints[temp.id * 3], trianglesPoints[temp.id * 3 + 1], trianglesPoints[temp.id * 3 + 2] };
                            List<Vector2> secondTriangle = new List<Vector2>() { trianglesPoints[r.id * 3], trianglesPoints[r.id * 3 + 1], trianglesPoints[r.id * 3 + 2] };

                            //Call private function. The first triangle must be greater.
                            if ((temp.max.pos.X - temp.min.pos.X) > (r.max.pos.X - r.min.pos.X))
                            {
                                if (TriangleIntersection(firstTriangle, secondTriangle))
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                if (TriangleIntersection(secondTriangle, firstTriangle))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    //Don't forget to put rectangle.
                    rectangles.Add(temp);
                }
                else
                {
                    //End event. Delete rectangle from sorted set.
                    rectangles.Remove(temp);
                }
            }


            return false;
        }
    }
}
