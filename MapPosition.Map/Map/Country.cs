using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MapPosition.Map
{
    [Serializable]
    public class Country : ChinaArea
    {
        [XmlElement("province")]
        public Province[] ProvinceList
        {
            get;
            set;
        }

        public override ChinaArea Position(MapPoint point)
        {
            if (this.Rects == null && (this.ProvinceList == null || this.ProvinceList.Length == 0))
                return null;

            ChinaArea area = null;
            if (this.Rects != null && this.Rects.Length > 0)
            {
                area = base.Position(point);
                if (area != null)
                {
                    foreach (var a in ProvinceList)
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
            else if (this.ProvinceList.Length > 0)
            {
                foreach (var a in ProvinceList)
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

        public override void FillName(string owner)
        {
            base.FillName(owner);

            if (ProvinceList != null && ProvinceList.Length > 0)
            {
                foreach (var province in ProvinceList)
                {
                    province.FillName(this.Name);
                }
            }
        }
    }
}
