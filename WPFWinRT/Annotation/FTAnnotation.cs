using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFWinRT.Annotation
{
    class FTAnnotation
    {
        public int version = 1;
        public bool readOnly = false;
        public Guid uuid = new Guid();
        public double modifiedTimeInterval = GetTimestamp(DateTime.Now);
        public double createdTimeInterval = DateTime.Now.Ticks;

        void 
    }
}
