using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _3045陈文坛exp.WeatherService;

namespace _3045陈文坛exp
{
    class TomorrowWeatherInfo
    {
        #region
        private string _strDate = null;

        public string Date
        {
            get { return _strDate; }
            set { _strDate = value; }
        }
        private string _strWeather = null;

        public string Weather
        {
            get { return _strWeather; }
            set { _strWeather = value; }
        }
        private string _strTemperature = null;

        public string Temperature
        {
            get { return _strTemperature; }
            set { _strTemperature = value; }
        }
        private string _strWind = null;

        public string Wind
        {
            get { return _strWind; }
            set { _strWind = value; }
        }
        private string _strImage1 = "0.gif";

        public string ImageFrom
        {
            get { return _strImage1; }
            set { _strImage1 = value; }
        }
        private string _strImage2 = "1.gif";

        public string ImageTo
        {
            get { return _strImage2; }
            set { _strImage2 = value; }
        }
        private string _strCity = null;

        public string City
        {
            get { return _strCity; }
            set { _strCity = value; }
        }
        #endregion

        private double tH = 0;//高温
        private double tL = 0;//低温
        private double tt = 0;//均温

        public enum ResultInfo
        {
            NetError,
            NameError,
            Success
        }
        private ResultInfo _result;
        

        internal ResultInfo Result
        {
            get { return _result; }
            set { _result = value; }
        }

        public string[] GetWeatherString(string strCity)
        {
            string[] weatherInfo = null;
            try
            {
                WeatherWebService ws = new WeatherWebService();
                weatherInfo = ws.getWeatherbyCityName(strCity);
            }
            catch
            {
                _result = ResultInfo.NetError;
            }
            return weatherInfo;
        }

        public void ExtratWeatherInfo(string[] str)
        {
            _strDate = str[4];
            _strWeather = str[6];
            _strTemperature = str[5];
            _strWind = str[7];
            if (str[8].Trim() != "")
            {
                _strImage1 = str[8];
            }

            if (str[9].Trim() != "")
            {
                _strImage2 = str[9];
            }
        }

        public TomorrowWeatherInfo(string city)
        {
            string[] str = GetWeatherString(city);
            ExtratWeatherInfo(str);
            tt = 0;
            if (str[12] != null && str[12] != "")
            {
                //获取明日天气字符串并转换成数值
                //针对数值有一位数和两位数的情况，使用如下操作来去除℃符号
                int index = str[12].IndexOf("/");
                string str1 = str[12].Substring(0, index);
                string str2 = str[12].Substring(index + 1, str[12].Length - index - 1);
                string highT = str2.Substring(0, str2.Length - 1);
                string lowT = str1.Substring(0, str1.Length - 1);
                tH = double.Parse(highT);
                tL = double.Parse(lowT);
            }
        }

        public double getTomorrowHighTemperature()
        {
            return tH;
        }
        public double getTomorrowLowTemperature()
        {
            return tL;
        }
    }
}
