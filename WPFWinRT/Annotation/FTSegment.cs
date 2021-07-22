using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFWinRT.Annotation
{
    class FTSegment
    {
        public PointF startPoint, endPoint;
        public float thickness;
        public Rectangle boundingRect;
        public float opacity;
        public bool isErased;
        public FTSegment(PointF startPoint, PointF endPoint, float thickness, Rectangle boundingRect, float opacity)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;
            this.boundingRect = boundingRect;
            this.thickness = thickness;
            this.opacity = opacity;
        }

        public void setAsErased(bool erased)
        {
            isErased = erased;
        }

        //TODO: implement on demand bounding rect calculation and cache them as iOS. 
    }
}
