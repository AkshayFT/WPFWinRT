using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFWinRT.Annotation
{
    public enum FTPenType
    {
        pen, caligraphy, pencil, highlighter, pilotPen, flatHighlighter
    }

    public static class FTPenTypeExtensions
    {
        public static bool IsHighlighterPenType(this FTPenType p)
        {
            if (FTPenType.flatHighlighter == p)
            {
                return true;
            }
            else if (FTPenType.highlighter == p)
            {
                return true;
            }
            return false;
        }
    }

    class FTStroke
    {
        public int strokeColor;
        public float strokeWidth = 1;
        public FTPenType penType = FTPenType.pen;
        public bool strokeInProgress = false;
        protected bool hasErasedSegments = false;
        public int segmentCount = 0;
        public bool isErased = false;
        public float averageThickness = 0, averageAplha = 0;
        protected List<FTSegment> segmentsArray;

        public FTAnnotationType annotationType()
        {
            return FTAnnotationType.stroke;
        }
        public void addSegment(PointF startPoint,
                         PointF endPoint,
                         float thickness,
                         float opacity,
                         Rectangle boundingRect)
        {
            FTSegment newSegment = new FTSegment(startPoint, endPoint, thickness, boundingRect, opacity);
            segmentsArray.Add(newSegment);
            segmentCount += 1;
        }

        public FTSegment getSegmentAtIndex(int index)
        {
            return this.segmentsArray[index];
        }

        public List<FTSegment> getSegments()
        {
            return this.segmentsArray;
        }

        public bool intersectsRect(Rectangle rect)
        {
            bool hasIntersects = false;
            for (int i = 0; i < this.segmentsArray.Count; i++)
            {
                FTSegment segment = this.segmentsArray[i];
                if (segment.isErased == true)
                {
                    hasIntersects = segment.boundingRect.IntersectsWith(rect);
                }
                if (hasIntersects)
                {
                    break;
                }
            }
            return hasIntersects;
        }

        public bool IntersectsRect(Rectangle rect, Region region, float containerScale)
        {
            bool hasIntersects = false;
            for (int i = 0; i < this.segmentsArray.Count; i++)
            {
                FTSegment segment = this.segmentsArray[i];
                if (segment.boundingRect.IntersectsWith(rect))
                {
                    hasIntersects = IsInsideTheRegion(region, segment.startPoint, segment.endPoint, containerScale);
                    if (hasIntersects)
                    {
                        break;
                    }
                }
            }
            return hasIntersects;
        }

        private bool IsInsideTheRegion(Region region, PointF startPoint, PointF endPoint, float containerScale)
        {
            int startX = (int)(startPoint.X * containerScale);
            int startY = (int)(startPoint.Y * containerScale);

            int endX = (int)(endPoint.X * containerScale);
            int endY = (int)(endPoint.Y * containerScale);
            return region.IsVisible(startX, startY) || region.IsVisible(endX, endY);
        }


        public void setSegmentAtIndex(int index, FTSegment segment)
        {
            segmentsArray[index] = segment;
        }

        //Render related
        private PointF converPointToGL(PointF point, float scale)
        {
            PointF p = point;
           // p = FTGeometryUtils.scalePointF(p, scale);
            return p;
        }

        private PointF converPointToGL(PointF point, float scale, Rectangle clipRect, Rectangle metalViewRect)
        {
            PointF p = point;
          //  p = FTGeometryUtils.scalePointF(p, scale);
            p.X -= metalViewRect.Left;
            p.Y -= metalViewRect.Top;            
            return p;
        }
    }
}