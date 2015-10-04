using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;

namespace _3045陈文坛exp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string _imagePath = Application.StartupPath + "\\weather\\";
        private string _mapPath;  //地图文档的相对位置
        private bool _isRemoveLabels = true;
        private bool _isSingleProvince = false;
        private string _initialCityFile = "defaultcity.ini";

        private void Form1_Load(object sender, EventArgs e)
        {
            _mapPath = Application.StartupPath + "\\Map\\Weather.Mxd";
            axMapControl1.LoadMxFile(_mapPath);

            string[] provinces = new string[] { "全国", "黑龙江省", "内蒙古自治区", "新疆维吾尔自治区", "吉林省", "甘肃省", "河北省", "北京市", "山西省", "天津市", "陕西省", "宁夏回族自治区", "青海省", "辽宁省", "山东省", "西藏自治区", "河南省", "江苏省", "安徽省", "四川省", "湖北省", "重庆市", "上海市", "浙江省", "湖南省", "江西省", "云南省", "贵州省", "广西壮族自治区", "福建省", "台湾省", "海南省", "广东省", "香港特别行政区", "澳门" };

            ESRI.ArcGIS.RuntimeManager.Bind(ESRI.ArcGIS.ProductCode.EngineOrDesktop);
            foreach (string str in provinces)
            {
                comboBox1.Items.Add(str);
            }

            comboBox1.SelectedIndex = 0;

            QueryTool qryTool = new QueryTool();
            axToolbarControl1.AddItem(qryTool, -1, 0, false, -1, esriCommandStyles.esriCommandStyleIconOnly);
            qryTool.LabelEvent += new QueryTool.LabelEventHandler(qryTool_LabelEvent);

            RemoveLabesCmd delete = new RemoveLabesCmd();
            axToolbarControl1.AddItem(delete, -1, 0, false, -1, esriCommandStyles.esriCommandStyleIconOnly);
            delete.LabelEvent += new RemoveLabesCmd.LabelEventHandler(revLabels_RemoveLabelsEvent);

            ZoomtoProvince(provinces[0]);

            InitialWeather();

            AddField();
            
            cityTeme cityteme1 = getMaxTemp();
            cityTeme cityteme2 = getMinTemp();
            label3.Text = "明日气温最高的城市：" + cityteme1.City + "\n温度为:" + cityteme1.Teme + "℃";
            label4.Text = "明日气温最低的城市：" + cityteme2.City + "\n温度为:" + cityteme2.Teme + "℃";
            
        }

        void qryTool_LabelEvent(string cityName, IFeature pFeatCity)
        {
            ZoomtoProvince(cityName);

            label1.Text = pFeatCity.get_Value(8).ToString();
            label2.Text = "面积有：" + pFeatCity.get_Value(2).ToString();

            //标注天气
            TomorrowWeatherInfo wInfo = new TomorrowWeatherInfo(cityName);

            Labels lbs = new Labels();
            IPoint pt = Utilities.GetFeatureCenterPoint(pFeatCity);// 标签的位置
            string lblString = cityName + "\r" + wInfo.Weather + "\r" + wInfo.Temperature + "\r" + wInfo.Wind;
            lbs.LabelQueryInfo(cityName, lblString, pt, axMapControl1.Map, _isRemoveLabels);

            ShowWeatherInfo(wInfo);
        }


        void revLabels_RemoveLabelsEvent()
        {
            IMap pMap = axMapControl1.Map;
            IActiveView pActiveView = pMap as IActiveView;
            ICompositeGraphicsLayer pCompositeGraphicsLayer = pMap.BasicGraphicsLayer as ICompositeGraphicsLayer;
            IGraphicsLayer pGraphicsLayer = pCompositeGraphicsLayer.FindLayer("WeatherLabel");

            IGraphicsContainer pGraphicsContainer = pGraphicsLayer as IGraphicsContainer;
            pGraphicsContainer.DeleteAllElements();
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewAll, null, pActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selCity = comboBox1.SelectedItem.ToString();
            IFeatureLayer lyrProVince = Utilities.GetLayerByName("省市_Mask", axMapControl1.Map) as IFeatureLayer;
            if (selCity != "全国")
            {
                IFeatureClass featClsProvince = lyrProVince.FeatureClass;
                IFeatureCursor feaCursor = null;
                IQueryFilter queryFilter = new QueryFilterClass();
                string whereClause = "[NAME] = '" + selCity + "'";
                queryFilter.WhereClause = whereClause;
                feaCursor = featClsProvince.Search(queryFilter, false);
                IFeature pFeature = feaCursor.NextFeature();

                if (pFeature != null)
                {
                    Utilities.ZoomToFeature(pFeature, axMapControl1.Object as IMapControlDefault);
                    IFeatureLayerDefinition lyrDef = lyrProVince as IFeatureLayerDefinition;
                    lyrDef.DefinitionExpression = whereClause;
                    lyrProVince.Visible = true;
                }
            }
            else
            {
                axMapControl1.Map.ClipGeometry = null;
                axMapControl1.Extent = axMapControl1.FullExtent;
                lyrProVince.Visible = false;
            }
        }


        private IFeature ZoomtoProvince(string cityName) //Form1中的帮助函数 
        {
            IFeatureLayer lyrCity = Utilities.GetLayerByName("县区_Mask", axMapControl1.Map) as IFeatureLayer;
            IFeatureLayer lyrProVince = Utilities.GetLayerByName("省市_Mask", axMapControl1.Map) as IFeatureLayer;
            string cityCode = string.Empty;
            #region 获取县区的行政代码
            IFeatureClass featCls = lyrCity.FeatureClass;
            IFields pFields = featCls.Fields;
            int index = pFields.FindField("ADCODE99");
            //查找要素
            IFeatureCursor pFeatCursor = null;
            IQueryFilter pQueryFilter = new QueryFilterClass();
            string whereClause = "[NAME99] like '" + cityName + "*'";
            pQueryFilter.WhereClause = whereClause;
            pFeatCursor = featCls.Search(pQueryFilter, false);
            IFeature pFeat = pFeatCursor.NextFeature();
            if (pFeat != null)
            {
                cityCode = pFeat.get_Value(index).ToString().Substring(0, 2) + "0000";
                IFeatureLayerDefinition pFeatDef = (IFeatureLayerDefinition)lyrCity;
                pFeatDef.DefinitionExpression = whereClause;
                lyrCity.Visible = true;
            }
            #endregion
            //缩放到省份
            if (cityCode != string.Empty)
            {
                IFeatureClass featClsProvince = lyrProVince.FeatureClass;
                IFeatureCursor featCursor = null;
                IQueryFilter queryFilter = new QueryFilterClass();
                whereClause = "[ADCODE99] = " + cityCode;
                queryFilter.WhereClause = whereClause;
                featCursor = featClsProvince.Search(queryFilter, false);
                IFeature pFeature = featCursor.NextFeature();
                if (pFeature != null)
                {
                    Utilities.ZoomToFeature(pFeature, axMapControl1.Object as IMapControlDefault);
                    IFeatureLayerDefinition lyrDef = (IFeatureLayerDefinition)lyrProVince;
                    lyrDef.DefinitionExpression = whereClause;
                    lyrProVince.Visible = true;
                }
            }
            return pFeat;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IFeature pFeatCity = ZoomtoProvince(textBox1.Text.Trim());
            string city = textBox1.Text.Trim();
            if (pFeatCity == null)
            {
                return;
            }

            label1.Text = pFeatCity.get_Value(8).ToString();
            label2.Text = "面积有：" + pFeatCity.get_Value(2).ToString();

            //在下面增加弹出标签的测试代码，或者说是调用代码
            //思考一下，如果要创建一个标签，我们需要提供什么函数呢？...
            //标注
            TomorrowWeatherInfo wInfo = new TomorrowWeatherInfo(city);
            ShowWeatherInfo(wInfo);

            Labels lbs = new Labels();
            IPoint pt = Utilities.GetFeatureCenterPoint(pFeatCity);// 标签的位置
            string lblString = city + "\r" + wInfo.Weather + "\r" + wInfo.Temperature + "\r" + wInfo.Wind;

            Labels myLbl = new Labels();
            ILabelsStyle myLblStyle = lbs as ILabelsStyle;
            myLblStyle.FontSize = 22;
            myLblStyle.style = esriBalloonCalloutStyle.esriBCSRoundedRectangle;
            myLblStyle.fColor = 1;
            myLblStyle.tColor = 255;

            lbs.LabelQueryInfo(city, lblString, pt, axMapControl1.Map, _isRemoveLabels);
        }

        //窗体上显示天气信息
        private void ShowWeatherInfo(TomorrowWeatherInfo info)
        {
            this.lbl_Weather.Text = info.Weather;
            this.lbl_Date.Text = info.Date;
            this.lbl_Temperature.Text = info.Temperature;
            this.lbl_Wind.Text = info.Wind;
            string imageFromFile = _imagePath + info.ImageFrom;
            if (!System.IO.File.Exists(imageFromFile))
            {
                imageFromFile = _imagePath + "0.gif";
            }
            this.pb_From.Image = new Bitmap(imageFromFile);

            this.pb_From.Visible = true;
            this.pb_From.Left = 22;
            this.pb_To.Visible = true;
            string imageToFile = _imagePath + info.ImageTo;
            if (!System.IO.File.Exists(imageToFile))
            {
                imageToFile = _imagePath + "0.gif";
            }
            this.pb_To.Image = new Bitmap(imageToFile);

        }


        //显示错误信息
        private void ShowError(string error, string cityName)
        {
            this.lbl_Weather.Text = "";
            this.lbl_Date.Text = "";
            if (error == "NetError")
            {
                this.lbl_Temperature.Text = "网络错误，请检查网络";
            }
            else
            {
                this.lbl_Temperature.Text = "没有" + cityName + "\"天气信息";
            }
            this.lbl_Wind.Text = "";
            //this.pb_From.Image = new Bitmap(_imagePath + info.ImageFrom);
            this.pb_From.Visible = false;
            //this.pb_From.Left = 40;
            this.pb_To.Visible = false;

            return;
        }

        private void InitialWeather()
        {
            string fileName = Application.StartupPath + "\\" + _initialCityFile;
            if (!System.IO.File.Exists(fileName)) return;

            System.IO.StreamReader sr = System.IO.File.OpenText(fileName);
            string city = sr.ReadLine();
            sr.Close();

            if (city == null) return;
            TomorrowWeatherInfo info = new TomorrowWeatherInfo(city);
            if (info.Result != TomorrowWeatherInfo.ResultInfo.Success)
            {
                ShowError(info.Result.ToString(), city);
                return;
            }

            ShowWeatherInfo(info);

            IFeature pFeat = ZoomtoProvince(city);

            Labels lbs = new Labels();
            IPoint pt = Utilities.GetFeatureCenterPoint(pFeat);
            string lblString = city + "\r" + info.Weather + "\r" + info.Temperature + "\r" + info.Wind;
            lbs.LabelQueryInfo(city, lblString, pt, axMapControl1.Map, _isRemoveLabels);

            TomorrowWeatherInfo wInfo = new TomorrowWeatherInfo(city);
            ShowWeatherInfo(wInfo);
        }

        //添加字段并赋值
        private void AddField()
        {
            //添加温度字段
            string HighTemp = "HighTemperature";
            string LowTemp = "LowTemperature";
            IFeatureLayer lyrProVince = Utilities.GetLayerByName("省市", axMapControl1.Map) as IFeatureLayer;
            IFeatureClass ProClass = lyrProVince.FeatureClass; ;
            if (ProClass.Fields.FindField(HighTemp) < 0)
            {
                IField pField = new FieldClass();
                IFieldEdit pFieldEdit = pField as IFieldEdit;
                pFieldEdit.Name_2 = HighTemp;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                IClass pClass = ProClass as IClass;
                pClass.AddField(pFieldEdit);
            }
            if (ProClass.Fields.FindField(LowTemp) < 0)
            {
                IField pField = new FieldClass();
                IFieldEdit pFieldEdit = pField as IFieldEdit;
                pFieldEdit.Name_2 = LowTemp;
                pFieldEdit.Type_2 = esriFieldType.esriFieldTypeDouble;
                IClass pClass = ProClass as IClass;
                pClass.AddField(pFieldEdit);

            }
            //为字段赋值
            string[] provinces = new string[] { "黑龙江省", "内蒙古自治区", "新疆维吾尔自治区", "吉林省", "甘肃省", "河北省", "北京市", "山西省", "天津市", "陕西省", "宁夏回族自治区", "青海省", "辽宁省", "山东省", "西藏自治区", "河南省", "江苏省", "安徽省", "四川省", "湖北省", "重庆市", "上海市", "浙江省", "湖南省", "江西省", "云南省", "贵州省", "广西壮族自治区", "福建省", "台湾省", "海南省", "广东省", "香港特别行政区", "澳门" };
            TomorrowWeatherInfo weath;

            IFeatureCursor featCursor = null;
            IQueryFilter queryFilter = new QueryFilterClass();
            for (int i = 0; i < provinces.Length; i++)
            {
                string selCity = provinces[i];
                string whereClause = "[NAME] = '" + selCity + "'";
                queryFilter.WhereClause = whereClause;
                featCursor = ProClass.Search(queryFilter, false);
                IFeature pFeature = featCursor.NextFeature();
                string pcity = Utilities.getCity(selCity);
                weath = new TomorrowWeatherInfo(pcity);

                Random random=new Random();//测试用随机数产生器               
                if (pFeature != null)
                {
                    pFeature.set_Value(pFeature.Fields.FindField("HighTemperature"), random.Next(20,40));
                    pFeature.set_Value(pFeature.Fields.FindField("LowTemperature"), random.Next(1,20));
                    pFeature.Store();
                }
            }
            IGeoFeatureLayer geoProVince = Utilities.GetLayerByName("省市", axMapControl1.Map) as IGeoFeatureLayer;
            setColor(geoProVince);
        }

        private void setColor(IGeoFeatureLayer IGFL)
        {
            int classCount = 10;
            ITableHistogram tableHistogram;
            IBasicHistogram basicHistogram;
            ITable table;
            IGeoFeatureLayer geoFeatureLayer;
            geoFeatureLayer = IGFL;
            ILayer layer = geoFeatureLayer as ILayer;
            table = layer as ITable;
            tableHistogram = new BasicTableHistogramClass();

            tableHistogram.Table = table;
            tableHistogram.Field = "HighTemperature";
            basicHistogram = tableHistogram as IBasicHistogram;
            object values;
            object frequencys;

            basicHistogram.GetHistogram(out values, out frequencys);

            IClassifyGEN classifyGEN = new QuantileClass();

            classifyGEN.Classify(values, frequencys, ref classCount);
            double[] classes;
            classes = classifyGEN.ClassBreaks as double[];

            IEnumColors enumColors = CreateAlgorithmicColorRamp(classes.Length).Colors;
            IColor color;

            IClassBreaksRenderer classBreaksRenderer = new ClassBreaksRendererClass();
            classBreaksRenderer.Field = "HighTemperature";
            classBreaksRenderer.BreakCount = classCount + 1;
            classBreaksRenderer.SortClassesAscending = true;

            ISimpleFillSymbol simpleFillSymbol;
            for (int i = 0; i < classes.Length; i++)
            {
                color = enumColors.Next();
                simpleFillSymbol = new SimpleFillSymbolClass();
                simpleFillSymbol.Color = color;
                simpleFillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;

                classBreaksRenderer.set_Symbol(i, simpleFillSymbol as ISymbol);
                classBreaksRenderer.set_Break(i, classes[i]);
            }
            if (geoFeatureLayer != null)
                geoFeatureLayer.Renderer = classBreaksRenderer as IFeatureRenderer;
            axMapControl1.ActiveView.Refresh();
        }
        //创建颜色带
        private IColorRamp CreateAlgorithmicColorRamp(int count)
        {
            IAlgorithmicColorRamp algColorRamp = new AlgorithmicColorRampClass();
            IRgbColor fromColor = new RgbColorClass();
            IRgbColor toColor = new RgbColorClass();

            fromColor.Red = 255;
            fromColor.Green = 255;
            fromColor.Blue = 255;

            toColor.Red = 255;
            toColor.Green = 0;
            toColor.Blue = 0;

            algColorRamp.ToColor = toColor;

            algColorRamp.FromColor = fromColor;


            algColorRamp.Algorithm = esriColorRampAlgorithm.esriLabLChAlgorithm;

            algColorRamp.Size = count;

            bool bture = true;
            algColorRamp.CreateRamp(out bture);
            return algColorRamp;
        }
        //储存城市与温度的一个类
        class cityTeme
        {
            private string _city;
            private double _teme;
            public cityTeme(string City, double Teme)
            {
                this._city = City;
                this._teme = Teme;
             } 
            public string City
            {
                get { return _city; }
                set{_city=value;}
            }
            public double Teme
            {
                get { return _teme; }
                set { _teme = value; }
            }
        }
        //统计最高温
        private cityTeme getMaxTemp()
        {
            cityTeme CT1 = new cityTeme("高温城市", 30.0);

            IFeatureLayer featLayer = (IFeatureLayer)Utilities.GetLayerByName("省市", axMapControl1.Map);
            IFeatureClass featClass = featLayer.FeatureClass;
            IDataStatistics maxTemp = new DataStatistics();
            IDataStatistics minTemp = new DataStatistics();
            IFeatureCursor featCursor;
            IQueryFilter queryfilter = new QueryFilterClass();

            featCursor = featClass.Search(null, false);
            ICursor cursor = (ICursor)featCursor;
            maxTemp.Cursor = cursor;

            maxTemp.Field = "HighTemperature";
            IStatisticsResults MAXtemp;
            MAXtemp = maxTemp.Statistics;
            CT1.Teme = MAXtemp.Maximum;
            
            String maxWhereClause = "[HighTemperature] = " + CT1.Teme;
            queryfilter.WhereClause = maxWhereClause;
            featCursor = featClass.Search(queryfilter, false);
            IFeature MAXpFeature = featCursor.NextFeature();
            if (MAXpFeature != null)
            {
                int index1 = featClass.FindField("NAME");
                CT1.City = MAXpFeature.get_Value(index1).ToString();
            }                        
            return CT1;
        }
        //统计最低温
        private cityTeme getMinTemp()
        {
            cityTeme CT2 = new cityTeme("低温城市", 0.0);

            IFeatureLayer featLayer = (IFeatureLayer)Utilities.GetLayerByName("省市", axMapControl1.Map);
            IFeatureClass featClass = featLayer.FeatureClass;
            IDataStatistics maxTemp = new DataStatistics();
            IDataStatistics minTemp = new DataStatistics();
            IFeatureCursor featCursor;
            IQueryFilter queryfilter = new QueryFilterClass();

            featCursor = featClass.Search(null, false);
            ICursor cursor = (ICursor)featCursor;
            minTemp.Cursor = cursor;

            minTemp.Field = "LowTemperature";
            IStatisticsResults MINtemp;
            MINtemp = minTemp.Statistics;
            CT2.Teme = MINtemp.Minimum;

            String minWhereClause = "[LowTemperature] = " + CT2.Teme;
            queryfilter.WhereClause = minWhereClause;
            featCursor = featClass.Search(queryfilter, false);
            IFeature MINpFeature = featCursor.NextFeature();
            if (MINpFeature != null)
            {
                int index2 = featClass.FindField("NAME");
                CT2.City = MINpFeature.get_Value(index2).ToString();
            }
            return CT2;
        }
        
    }
}
