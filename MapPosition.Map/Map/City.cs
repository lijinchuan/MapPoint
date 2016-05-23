using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MapPosition.Map
{
    [Serializable]
    public class City : ChinaArea
    {
        [XmlElement("Piecearea")]
        public Piecearea[] Area
        {
            get;
            set;
        }

        public override ChinaArea Position(MapPoint point)
        {
            if (this.Rects == null && (this.Area == null || this.Area.Length == 0))
                return null;

            ChinaArea area = null;
            if (this.Rects != null && this.Rects.Length > 0)
            {
                area = base.Position(point);
                if (area != null && Area != null && Area.Length > 0)
                {
                    foreach (var a in Area)
                    {
                        if (a.Name.EndsWith("市辖区"))
                        {
                            continue;
                        }
                        var piecearea = a.Position(point);

                        if (piecearea != null)
                        {
                            area = piecearea;
                            break;
                        }
                    }
                }
            }
            else if (Area != null && Area.Length > 0)
            {
                foreach (var a in Area)
                {
                    if (a.Name.EndsWith("市辖区"))
                    {
                        continue;
                    }
                    var piecearea = a.Position(point);

                    if (piecearea != null)
                    {
                        area = piecearea;
                        break;
                    }
                }
            }

            return area;
        }

        public override void FillName(string owner)
        {
            base.FillName(owner);

            if (this.Area != null && this.Area.Length > 0)
            {
                foreach (var a in Area)
                {
                    a.FillName(this.Name);
                }
            }
        }
    }
}
