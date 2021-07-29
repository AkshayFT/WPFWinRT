using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFWinRT.Annotation
{
    public enum FTAnnotationType
    {
        none,
        stroke,
        image,
        text,
        sticky,
        audio
    }

    public class FTAnnotation
    {
        public int version = 1;
        public bool readOnly = false;
        public string uuid = new Guid().ToString();
        //TODO: ideally this should be timestamp.
        public double modifiedTimeInterval = DateTime.Now.Ticks;
        public double createdTimeInterval = DateTime.Now.Ticks;

        public bool hidden = false;
        public Rectangle boundingRect = new(0, 0, 0, 0);

        static public int DefaultAnnotationVersion()
        {
            return 1;
        }

        public FTAnnotationType AnnotationType()
        {
            return FTAnnotationType.none;
        }
    }
}
