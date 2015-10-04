using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Carto;
using System.Drawing;

namespace _3045陈文坛exp
{
    class Labels : ILabelsStyle
    {
        private IColor _backColor = null;
        private IColor _foreColor = null;

        public Labels()
        {
        }

        ///<summary>
        ///构建函数，目前暂时只提供前景色和背景色的参数设置
        ///</summary>
        ///<param name="backColor"></param>
        ///<param name="foreColor"></param>
        public Labels(IColor backColor, IColor foreColor)
        {
            _backColor = backColor;
            _foreColor = foreColor;
        }

        private int _FontSize = 12;
        public int FontSize
        {
            get
            {
                return _FontSize;
            }
            set
            {
                _FontSize = value;
            }
        }

        private esriBalloonCalloutStyle _style = esriBalloonCalloutStyle.esriBCSRoundedRectangle;
        public esriBalloonCalloutStyle style
        {
            get
            {
                return _style;
            }
            set
            {
                _style = value;
            }
        }

        private int _fColor = 1;
        public int fColor
        {
            get
            {
                return _fColor;
            }
            set
            {
                _fColor = value;
            }
        }
        private int _tColor = 255;
        public int tColor
        {
            get
            {
                return _tColor;
            }
            set
            {
                _tColor = value;
            }

        }

        public void LabelQueryInfo(string name, string strContent, IPoint pt, IMap pMap, bool isRemoveLast)
        {
            //获取并激活Graphic标注图层
            if (pt == null) return;
            IActiveView pActiveView = pMap as IActiveView;
            ICompositeGraphicsLayer pCompositeGraphicsLayer = pMap.BasicGraphicsLayer as ICompositeGraphicsLayer;

            //WeatherLabel标记组，通过ArcMap的Layers Properties的Annotation group可以看到
            IGraphicsLayer pGraphicsLayer = pCompositeGraphicsLayer.FindLayer("WeatherLabel");
            pMap.ActiveGraphicsLayer = pGraphicsLayer as ILayer;
            pGraphicsLayer.Activate(pActiveView.ScreenDisplay);
            IGraphicsContainer pGraphicsContainer = pGraphicsLayer as IGraphicsContainer;//转换到图形容器接口

            IElementCollection pElementCollection = new ElementCollectionClass();
            AddBalloonCalloutLabel(name, strContent, pt, pElementCollection);//我们需要一个新的私有函数来实现设置标签元素背景，BalloonCallout对象

            //添加标注
            if (isRemoveLast == true)
            {
                pGraphicsContainer.DeleteAllElements();
            }

            pGraphicsContainer.AddElements(pElementCollection, 1000);

            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds);

        }

        private void AddBalloonCalloutLabel(string strName, string strText, IPoint pPointAnchor, IElementCollection pElementCollection)
        {
            ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
            pSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
            IRgbColor pRgbColorFillSymbol = new RgbColorClass();

            pRgbColorFillSymbol.RGB = fColor;
            //pRgbColorFillSymbol.Red = Color.White.R;
            //pRgbColorFillSymbol.Green = Color.White.G;
            //pRgbColorFillSymbol.Blue = Color.White.B;
            //byte bt = 0;
            //pRgbColorFillSymbol.Transparency = bt;
            pSimpleFillSymbol.Color = pRgbColorFillSymbol;
            //IBalloonCallout 接口
            IBalloonCallout pBalloonCallout = new BalloonCalloutClass();//弹出标签的背景
            pBalloonCallout.Style = _style;//选择弹出标签样式，请尝试另外两种样式
            pBalloonCallout.Symbol = pSimpleFillSymbol;//填充
            pBalloonCallout.LeaderTolerance = 1;
            pBalloonCallout.AnchorPoint = pPointAnchor;//定位点
            //创建点标注
            ITextSymbol pTextSymbol = new TextSymbolClass();
            IFormattedTextSymbol pFormattedTextSymbol = pTextSymbol as IFormattedTextSymbol;
            pFormattedTextSymbol.Background = pBalloonCallout as ITextBackground;//设置背景

            //字体相关设置
            IRgbColor pRgbColorTextSymbol = new RgbColorClass();//字体颜色
            pRgbColorTextSymbol.RGB = tColor;
            //pRgbColorTextSymbol.Red = 0;
            //pRgbColorTextSymbol.Green = 0;
            //pRgbColorTextSymbol.Blue = 0;
            pFormattedTextSymbol.Color = pRgbColorTextSymbol;
            pFormattedTextSymbol.Font.Name = "宋体";
            pFormattedTextSymbol.Size = _FontSize;
            pFormattedTextSymbol.HorizontalAlignment = esriTextHorizontalAlignment.esriTHALeft;//对齐方式
            pFormattedTextSymbol.VerticalAlignment = esriTextVerticalAlignment.esriTVATop;

            ISimpleTextSymbol pSimpleTextSymbol = pTextSymbol as ISimpleTextSymbol;
            pSimpleTextSymbol.XOffset = 15;
            pSimpleTextSymbol.YOffset = 0;

            //加点标签
            ITextElement pTextElement = new TextElementClass();
            pTextElement.Symbol = pFormattedTextSymbol as ITextSymbol;
            pTextElement.Text = strText;//显示的标注文本

            IElement pElement = pTextElement as IElement;
            pElement.Geometry = pPointAnchor as IGeometry;//位置   pPointPosition 
            IElementProperties pElementProperties = pElement as IElementProperties;
            pElementProperties.Name = strName;//IElement的名称

            pElementCollection.Add(pElementProperties as IElement, -1);

        }
    }
}
