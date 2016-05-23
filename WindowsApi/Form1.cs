using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using MapPosition.Map;

namespace WindowsApi
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]     
    public partial class Form1 : Form
    {
        static string worldmaplistxml = AppDomain.CurrentDomain.BaseDirectory + "\\worldmaplistxml";

        const string OVER_QUERY_LIMIT = "OVER_QUERY_LIMIT";

        HtmlElement searchText = null;
        HtmlElement searchBtn = null;

        bool mapIsReady = false;
        bool stop = false;

        EventWaitHandle watiting = new EventWaitHandle(false, EventResetMode.ManualReset);
        EventWaitHandle reloadWating = null;
        string result = string.Empty;
        string lastresult = string.Empty;

        int finished, left;

        public Form1()
        {
            InitializeComponent();

            this.panel1.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            this.panel2.Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

            this.webBrowser1.ScriptErrorsSuppressed = true;

            this.webBrowser1.DocumentCompleted += webBrowser1_DocumentCompleted;

            this.WindowState = FormWindowState.Maximized;

            this.FormClosing += Form1_FormClosing;
        }

        void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(!stop)
            {
                stop = true;
                e.Cancel = true;

                MessageBox.Show("再点击关闭按钮一次确认！");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.webBrowser1.Navigate("http://www.gpsspg.com/maps.htm");

            timer1.Interval = 500;
            timer1.Start();
        }

        void ReLoad()
        {
            this.webBrowser1.ObjectForScripting = null;
            this.webBrowser1.Navigate("http://www.gpsspg.com/maps.htm");
        }

        void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.webBrowser1.ObjectForScripting == null)
            {
                this.webBrowser1.ObjectForScripting = this;

                searchText = this.webBrowser1.Document.GetElementById("s_t");

                searchBtn = this.webBrowser1.Document.GetElementById("s_btn");

                this.webBrowser1.Navigate("javascript:switchMaps(0);");

                if (reloadWating != null)
                {
                    this.webBrowser1.Navigate("javascript:switchMaps(0);");
                    reloadWating.Set();
                }
            }

            if(new Regex(@"/maps/google_\d{1,}.htm",RegexOptions.IgnoreCase).IsMatch(e.Url.AbsoluteUri))
            {
                mapIsReady=true;

                this.webBrowser1.Navigate("javascript:function getPostion(){var div=document.getElementById('m_r').querySelector('iframe').contentWindow.document.querySelector('.gm-style-iw');if(div!=null){window.external.GetContent(div.innerHTML);}}");

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(!mapIsReady)
            {
                MessageBox.Show("地图未准备好。");
                return;
            }

            double x,y;

            if(!double.TryParse(TB_X.Text.Trim(),out x))
            {
                MessageBox.Show("经度填写错误");
                return;
            }

            if(x>180||x<-180)
            {
                MessageBox.Show("经度填写错误，范围:[-180,180]");
                return;
            }

            if(!double.TryParse(TB_Y.Text.Trim(),out y))
            {
                MessageBox.Show("纬度填写错误");
                return;
            }

            if(y>90||y<-90)
            {
                MessageBox.Show("纬度填写错误，范围:[-90,90]");
                return;
            }

            new Action<MapPoint>(GetPostion).BeginInvoke(new MapPoint(x, y), null, null);
        }

        public void GetPostion(MapPoint mp)
        {
            if (mp == null)
                return;

            this.Invoke(new Action(() => searchText.SetAttribute("value", string.Format("{0},{1}", mp.Y, mp.X))));
            this.Invoke(new Action(() => this.webBrowser1.Navigate("javascript:document.getElementById('s_btn').click();")));

            lastresult = result;
            result = string.Empty;

            int errtime = 0;

            while (true)
            {
                watiting.Reset();

                this.Invoke(new Action(() => this.webBrowser1.Document.InvokeScript("getPostion")));
                WaitHandle.WaitAll(new WaitHandle[] { watiting }, 1000);

                if (!string.IsNullOrEmpty(result) && result != lastresult)
                {
                    break;
                }

                errtime++;

                if (errtime > 10)
                    break;
            }

            MessageBox.Show(result);
        }

        public void GetPostionTask(List<MapPoint> mps)
        {
            try
            {
                if (mps == null)
                    return;

                foreach (var mp in mps)
                {
                    if (stop)
                        break;

                    if (!string.IsNullOrEmpty(mp.Postion) && (mp.Postion.IndexOf(OVER_QUERY_LIMIT) == -1))
                        continue;

                    this.Invoke(new Action(() => searchText.SetAttribute("value", string.Format("{0},{1}", mp.Y, mp.X))));
                    this.Invoke(new Action(() => this.webBrowser1.Navigate("javascript:document.getElementById('s_btn').click();")));

                    lastresult = result;
                    result = string.Empty;

                    int errtime = 0;

                    while (true)
                    {
                        watiting.Reset();

                        this.Invoke(new Action(() => this.webBrowser1.Document.InvokeScript("getPostion")));
                        WaitHandle.WaitAll(new WaitHandle[] { watiting }, 1000);

                        if (!string.IsNullOrEmpty(result) && result != lastresult)
                        {
                            break;
                        }

                        errtime++;

                        if (errtime > 10)
                        {
                            result = string.Empty;
                            break;
                        }
                    }

                    //MessageBox.Show(result);
                    mp.Postion = result;

                    this.BeginInvoke(new Action(() => { this.TBLog.Text += "\r\n" + result; }));

                    Thread.Sleep(new Random().Next(3000));

                    if(result.IndexOf(OVER_QUERY_LIMIT)>-1)
                    {
                        XMLHelper.SerializerToXML(mps, worldmaplistxml);

                        this.reloadWating = new EventWaitHandle(false, EventResetMode.ManualReset);
                        this.ReLoad();
                        WaitHandle.WaitAll(new WaitHandle[] { reloadWating });

                        Thread.Sleep(5000);
                    }
                    else
                    {
                        left--;
                        finished++;
                    }
                }
            }
            catch (Exception ex)
            {
                
                MessageBox.Show("出错，结果已保存。");
            }

            XMLHelper.SerializerToXML(mps, worldmaplistxml);
            this.BeginInvoke(new Action(() => { this.TBLog.Text += "\r\n已经保存"; }));
        }

        public void GetContent(string content)
        {
            //<div style="width:280px;">谷歌地图：<span class="fcg">42.5530802890,98.3496093750</span><br>百度地图：42.5588315156,98.3562079767<br>腾讯高德：42.5530003420,98.3495585446<br>图吧地图：42.5432196220,98.3456832546<br>谷歌地球：42.5513296220,98.3487832546<br>北纬N42°33′4.79″ 东经E98°20′55.62″<br>海拔：1135.00米<br><span class="fcg"><strong>靠近</strong></span>：内蒙古自治区阿拉善盟额济纳旗 X941<br>参考：</div>
            //纬度，经度
            string xy = new Regex("<span class=\"fcg\">([\\d\\.\\-]+\\,[\\d\\.\\-]+)</span>").Match(content).Groups[1].Value;
            string near = new Regex("靠近</strong></span>[：:\\s]*\"?([^<]+)\"?<").Match(content).Groups[1].Value;

            if(string.IsNullOrEmpty(xy)||(string.IsNullOrEmpty(near)))
            {
                return;
            }

            if(near.IndexOf("解析中")>-1)
            {
                return;
            }

            result = xy + ":" + near;

            watiting.Set();
        }


        public List<MapPoint> EnumMapPoint()
        {
            List<MapPoint> mplist = new List<MapPoint>();

            List<Rect> exRect = new List<Rect>();

            //北冰洋的
            exRect.Add(new Rect(90, 180, 80, 0));
            exRect.Add(new Rect(90, 0, 80, -180));

            //大部中国的
            exRect.Add(new Rect(40, 120, 30, 80));
            exRect.Add(new Rect(30, 120, 23, 100));

            //太平洋
            exRect.Add(new Rect(23, 180, 0, 130));
            exRect.Add(new Rect(23, -110, 0, -180));

            exRect.Add(new Rect(30, 180, 23, 130));
            exRect.Add(new Rect(30, -120, 23, -180));

            exRect.Add(new Rect(50, 180, 30, 150));
            exRect.Add(new Rect(50, -130, 30, -180));

            exRect.Add(new Rect(10, -90, 0, -110));

            exRect.Add(new Rect(0, -90, -90, -180));

            //南极
            exRect.Add(new Rect(-60, 180, -90, 0));
            exRect.Add(new Rect(-60, 0, -90, -180));

            //印度洋
            exRect.Add(new Rect(0, 98, -90, 50));

            //大西洋
            exRect.Add(new Rect(58, -20, 0, -50));
            exRect.Add(new Rect(40, -50, 0, -70));

            exRect.Add(new Rect(0, 0, -90, -30));
            exRect.Add(new Rect(0, 15, -90, 0));

            double minx = -180, maxx = 180;
            double miny = -90, maxy = 90;

            for (double i = minx; i < maxx; i = i + 1)
            {
                for (double j = miny; j < maxy; j = j + 1)
                {
                    MapPoint mp = new MapPoint(i, j);
                    bool isin = false;
                    foreach (var rect in exRect)
                    {
                        if (rect.IsInclude(mp))
                        {
                            isin = true;
                            break;
                        }
                    }

                    if(!isin)
                    {
                        mplist.Add(mp);
                    }
                }
            }

            return mplist;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text.Equals("停止"))
            {
                stop = true;
                button2.Text = "批量获取";
            }
            else
            {
                button2.Text = "停止";
                stop = false;

                var mplist = XMLHelper.DeSerializerFile<List<MapPoint>>(worldmaplistxml, false) ?? EnumMapPoint();

                foreach(var mp in mplist)
                {
                    if (mp.Postion == null)
                        continue;

                    var subpostion = mp.Postion.Split(':');
                    if (subpostion.Length == 1)
                        continue;

                    var xy = subpostion[0].Split(',');
                    if (xy.Length != 2)
                        continue;

                    if((int)double.Parse(xy[0])!=(int)mp.Y
                        || (int)double.Parse(xy[1]) != (int)mp.X)
                    {
                        mp.Postion = null;
                    }
                }

                mplist = mplist.Select(p => new
                {
                    g = (Guid.NewGuid().ToString() + p.X + "" + p.Y).GetHashCode(),
                    p
                }).OrderBy(p => p.g).Select(p => p.p).ToList();

                var leftlist = mplist.Where(p => string.IsNullOrEmpty(p.Postion) || p.Postion.IndexOf(OVER_QUERY_LIMIT) > -1).ToList();

                left = leftlist.Count;
                finished = mplist.Count - left;

                new Action<List<MapPoint>>(GetPostionTask).BeginInvoke(mplist, null, null);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (finished > 0 || left > 0)
                this.LabNum.Text = string.Format("{0}:{1}", finished, left);
        }
    }
}
