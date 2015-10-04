using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;

namespace _3045陈文坛exp
{
    /// <summary>
    /// Summary description for QueryTool.
    /// </summary>
    [Guid("dc3c87cf-779e-4540-aca9-7f66066bea31")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("_3045陈文坛exp.QueryTool")]
    public sealed class QueryTool : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            ControlsCommands.Register(regKey);

        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            ControlsCommands.Unregister(regKey);

        }

        #endregion
        #endregion

        private IHookHelper m_hookHelper;

        private IMapControlDefault m_mapCtrl;

        public delegate void LabelEventHandler(string city, IFeature pFeature);
        public event LabelEventHandler LabelEvent;

        public QueryTool()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = ""; //localizable text 
            base.m_caption = "";  //localizable text 
            base.m_message = "";  //localizable text
            base.m_toolTip = "";  //localizable text
            base.m_name = "";   //unique id, non-localizable (e.g. "MyCategory_MyTool")
            try
            {
                //
                // TODO: change resource name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods

        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            if (m_hookHelper == null)
                m_hookHelper = new HookHelperClass();

            m_hookHelper.Hook = hook;
            if (hook is IMapControlDefault)
            {
                m_mapCtrl = hook as IMapControlDefault;
            }
            else if (hook is IToolbarControl)
            {
                IToolbarControl toolbarCtrl = hook as IToolbarControl;
                m_mapCtrl = toolbarCtrl.Buddy as IMapControlDefault;
            }

            // TODO:  Add QueryTool.OnCreate implementation
        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            // TODO: Add QueryTool.OnClick implementation
            if (m_mapCtrl != null)
            {
                m_mapCtrl.MousePointer = esriControlsMousePointer.esriPointerArrowQuestion;
            }
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add QueryTool.OnMouseDown implementation
            if (m_mapCtrl == null) return;
            IPoint pt = m_mapCtrl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);

            ISpatialFilter sFilter = new SpatialFilterClass();
            sFilter.Geometry = pt;
            sFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin;

            IFeatureLayer lyr = Utilities.GetLayerByName("县区", m_mapCtrl.Map) as IFeatureLayer;
            IFeatureClass fCls = lyr.FeatureClass;
            IFeatureCursor featCursor = fCls.Search(sFilter, false);
            IFeature pFeat = featCursor.NextFeature();

            if (pFeat != null)
            {
                IFields pFields = pFeat.Fields;
                int i = pFields.FindField("NAME99");
                string cityName = pFeat.get_Value(i).ToString();

                char[] c = new char[] { '县', '市', '区' };
                int index = cityName.IndexOfAny(c, 2);
                if (index != -1)
                {
                    cityName = cityName.Substring(0, index);
                }
                LabelEvent(cityName, pFeat);
            }

        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add QueryTool.OnMouseMove implementation
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add QueryTool.OnMouseUp implementation
        }
        #endregion
    }
}
