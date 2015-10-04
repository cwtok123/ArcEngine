using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geometry;
using System.Xml;

namespace _3045陈文坛exp
{
    class Utilities
    {

        public static ILayer GetLayerByName(string lyrName, IMap mapName)
        {
            if (mapName == null || lyrName == null || lyrName == "")
                return null;
            else
            {
                ILayer lyr = null;
                for (int i = 0; i < mapName.LayerCount; i++)
                {
                    lyr = mapName.get_Layer(i);
                    if (lyr.Name == lyrName)
                        return lyr;
                }
            }
            return null;
        }

        public static void ZoomToFeature(IFeature pFeat, IMapControlDefault mapCtl)
        {
            if (pFeat.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
            {
                mapCtl.CenterAt((Point)pFeat);

            }
            else
            {
                mapCtl.ActiveView.Extent = pFeat.Extent;

            }
            mapCtl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
            return;
        }




        public static IPoint GetFeatureCenterPoint(IFeature pFeatCity)
        {
            if (pFeatCity.Shape.GeometryType == esriGeometryType.esriGeometryPoint)
            {
                return pFeatCity.Shape as IPoint;
            }

            else if (pFeatCity.Shape.GeometryType == esriGeometryType.esriGeometryPolygon)
            {
                IGeometry5 centerPoint = pFeatCity.Shape as IGeometry5;
                return centerPoint.CentroidEx;
            }

            else if (pFeatCity.Shape.GeometryType == esriGeometryType.esriGeometryPolyline)
            {
                IPoint centerPoint = new PointClass();
                IPolyline pl = pFeatCity.Shape as IPolyline;
                pl.QueryPoint(esriSegmentExtension.esriExtendAtFrom, 0.5, true, centerPoint);
                return centerPoint;
            }
            return null;
        }

        public static string getCity(string pro)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("City.xml");
            XmlNode xn = doc.SelectSingleNode("city/" + pro);
            XmlElement xe = (XmlElement)xn;
            string city = xn.InnerText;
            return city;

        }
    }
}
