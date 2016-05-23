using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MapPosition.Map
{
    [Serializable]
    public class Province : ChinaArea
    {
        [XmlElement("City")]
        public City[] Cities
        {
            get;
            set;
        }

        public override void FillName(string owner)
        {
            base.FillName(owner);

            if (this.Cities != null && this.Cities.Length > 0)
            {
                foreach (var city in this.Cities)
                {
                    city.FillName(this.Name);
                }
            }
        }

        public override ChinaArea Position(MapPoint point)
        {
            if (this.Rects == null && (this.Cities == null || this.Cities.Length == 0))
                return null;

            ChinaArea area = null;
            if (this.Rects != null && this.Rects.Length > 0)
            {
                area = base.Position(point);
                if (area != null && Cities != null && this.Cities.Length > 0)
                {
                    foreach (var a in Cities)
                    {
                        var piecearea = a.Position(point);

                        if (piecearea != null)
                        {
                            area = piecearea;
                            break;
                        }
                    }
                }
            }
            else if (this.Cities != null && this.Cities.Length > 0)
            {
                foreach (var a in Cities)
                {
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
    }
}
