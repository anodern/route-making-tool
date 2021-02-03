using BusDriverFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace route_making_tool {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow:Window {
        public MainWindow() {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            if(File.Exists("./config.sii")) {
                DefReader def = new DefReader("./config.sii", Encoding.GetEncoding("GBK"));
                config=def.keys;
                if(config.TryGetValue("map", out string temp)) {
                    txt_map.Text=temp;
                }
                for(int i = 0; config.TryGetValue("vehicle"+i, out temp); i++) {
                    vehList.Items.Add(temp);
                }
                if(!config.ContainsKey("camera0")) AddDefaultCamera();
                for(int i = 0; config.TryGetValue("camera"+i, out temp); i++) {
                    camList.Items.Add(new CameraConfig(temp));
                }
            } else {
                config=new Dictionary<string, string>();
                LoadVehicle();
                AddDefaultCamera();
                for(int i = 0; config.TryGetValue("camera"+i, out string temp); i++) {
                    camList.Items.Add(new CameraConfig(temp));
                }
            }
            if(vehList.Items.Count>0) {
                vehList.SelectedIndex=0;
            }
            LoadSkySun();
        }

        private void Window_Drop(object sender, DragEventArgs e) {
            if(e.Data.GetDataPresent(DataFormats.FileDrop)) {
                string path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                switch(path[path.LastIndexOf('.')..]) {
                    case ".mbd":
                        txt_map.Text=path;
                        VerifyMap();
                        break;
                    case ".sii":
                        txt_mission.Text=path;
                        VerifyMission();
                        break;
                    default:
                        lab_info.Content="文件格式错误";
                        break;
                }
            }
        }
        private void txt_mission_TextChanged(object sender, TextChangedEventArgs e) => VerifyMission();
        private void txt_map_TextChanged(object sender, TextChangedEventArgs e) => VerifyMap();

        private void VerifyMission() {
            string path = txt_mission.Text;

            if(string.IsNullOrEmpty(path)) {
                lab_info.Content="请指定线路";
                return;
            }
            if(File.Exists(path)) {

            } else if(File.Exists(path + ".sii")) {
                path += ".sii";
            } else if(File.Exists("def/mission/"+ path + ".sii")) {
                path = "def/mission/"+ path + ".sii";
            } else if(File.Exists("base/def/mission/"+ path + ".sii")) {
                path = "base/def/mission/"+ path + ".sii";
            } else {
                lab_info.Content="请输入正确的线路名";
                return;
            }

            ReadMission(path);
            txt_num.Text=GetFileName(path);
            lab_info.Content="加入线路:"+GetFileName(path);
            LoadList();
            if(mbd==null) VerifyMap();
        }
        private void VerifyMap() {

            string path = txt_map.Text;
            if(string.IsNullOrEmpty(path)) {
                lab_info.Content="请指定地图";
                return;
            }
            if(File.Exists(path)) {

            } else if(File.Exists(path + ".mbd")) {
                path += ".mbd";
            } else if(File.Exists("map/"+path + ".mbd")) {
                path = "map/"+ path + ".mbd";
            } else if(File.Exists("base/map/"+path + ".mbd")) {
                path = "base/map/"+path + ".mbd";
            } else {
                lab_info.Content="请输入正确的地图名";
                return;
            }

            //mbd = new MbdFile(path);
            //lab_info.Content="加入地图:"+GetFileName(path);
            try {
                mbd = new MbdFile(path);
                lab_info.Content="加入地图:"+GetFileName(path);
            } catch(Exception e) {
                lab_info.Content="读取地图异常,已记录";
                StreamWriter sw = new StreamWriter((new Guid()).ToString().Replace("-", "")+".log");
                sw.Write(e);
                sw.Close();
            }
            if(string.IsNullOrEmpty(txt_mission.Text)) return;
            LoadList();
        }

        MbdFile mbd;
        Dictionary<string, string> config;

        private void LoadVehicle() {
            string path;
            if(Directory.Exists(@"./base/vehicle/driveable")) path = @"./base/vehicle/driveable/";
            else if(Directory.Exists(@"./vehicle/driveable")) path = @"./vehicle/driveable/";
            else return;

            DirectoryInfo root = new DirectoryInfo(path);
            FileInfo[] files = root.GetFiles("*.sii");
            for(int i = 0; i<files.Length; i++) {
                DefReader veh = new DefReader(files[i].FullName, Encoding.GetEncoding("GBK"));
                veh.keys.TryGetValue("driveable_vehicle_data", out string temp);
                vehList.Items.Add(temp);
                if(!config.ContainsKey("vehicle"+i))
                    config.Add("vehicle"+i, temp);
            }
        }

        private void LoadSkySun() {
            string skyPath, sunPath;
            if(File.Exists("def/sky_data.def")) skyPath = "def/sky_data.def";
            else if(File.Exists("base/def/sky_data.def")) skyPath = "base/def/sky_data.def";
            else return;
            if(File.Exists("def/sun_data.def")) sunPath = "def/sun_data.def";
            else if(File.Exists("base/def/sun_data.def")) sunPath = "base/def/sun_data.def";
            else return;

            DefReader sky = new DefReader(skyPath, Encoding.GetEncoding("GBK"));
            DefReader sun = new DefReader(sunPath, Encoding.GetEncoding("GBK"));

            if(sky.keys.TryGetValue("sky_count", out string temp)) {
                int skyNum = int.Parse(temp);
                for(int i = 0; i<skyNum; i++) {
                    if(sky.keys.TryGetValue(string.Format("sky{0:D2}", i), out temp)) {
                        skyList.Items.Add(FixDefValue(temp));
                    } else if(sky.keys.TryGetValue("sky"+i, out temp)) {
                        skyList.Items.Add(FixDefValue(temp));
                    } else {
                        skyList.Items.Add("unknown");
                    }
                }
                skyList.SelectedIndex=0;
            }

            if(sun.keys.TryGetValue("sun_count", out temp)) {
                int sunNum = int.Parse(temp);
                for(int i = 0; i<sunNum; i++) {
                    if(sun.keys.TryGetValue("sun"+i, out temp)) {
                        sunList.Items.Add(FixDefValue(temp));
                    } else {
                        sunList.Items.Add("unknown");
                    }
                }
                sunList.SelectedIndex=0;
            }
        }

        private string FixDefValue(string str) => str[1..str.IndexOf('|')].Trim();

        private string GetFileName(string path) {
            path = path.Replace('\\', '/');
            bool hasDic = path.IndexOf('/')>=0;
            bool hasDot = path.IndexOf('.')>=0;

            if(hasDot && hasDic) {
                int dIndex = path.LastIndexOf('/');
                return path.Substring(dIndex+1, path.LastIndexOf('.')-dIndex-1);
            } else if(hasDot && !hasDic) {
                return path.Substring(0, path.LastIndexOf('.'));
            } else {
                //lab_info.Content="获取名称出错";
                return path;
            }
        }


        readonly string[] pass = {
                "\"/model/misc/people/hotel_busstop.pmd\"",
                "\"/model/misc/people/hotel_busstop2.pmd\"",
                "\"/model/misc/people/hotel_invis.pmd\"",
                "\"/model/misc/people/hotel_ui.pmd\"",
                "\"/model/misc/people/factory_busstop.pmd\"",
                "\"/model/misc/people/factory_busstop2.pmd\"",
                "\"/model/misc/people/factory_invis.pmd\"",
                "\"/model/misc/people/factory_ui.pmd\"",
                "\"/model/misc/people/history_busstop.pmd\"",
                "\"/model/misc/people/history_busstop2.pmd\"",
                "\"/model/misc/people/history_invis.pmd\"",
                "\"/model/misc/people/history_ui.pmd\"",
                "\"/model/misc/people/crowd.pmd\"",
                "\"/model/misc/people/crowd_ui.pmd\"",
                "\"/model/misc/people/school_busstop.pmd\"",
                "\"/model/misc/people/school_invis.pmd\"",
                "\"/model/misc/people/school_ui.pmd\""
        };

        int stopNum;
        uint[] stopIndex;
        private void ReadMission(string path) {
            StreamReader sr = new StreamReader(path);
            string temp;
            sr.ReadLine();
            sr.ReadLine();
            sr.ReadLine();
            temp = sr.ReadLine();
            try {
                stopNum=int.Parse(temp[(temp.IndexOf(':')+1)..].Trim());
            } catch(Exception) {
                lab_info.Content="mission文件格式错误";
                return;
            }
            stopIndex=new uint[stopNum];
            for(int i = 0; i<stopNum; i++) sr.ReadLine();
            sr.ReadLine();
            sr.ReadLine();
            for(int i = 0; i<stopNum; i++) {
                sr.ReadLine();
                temp=sr.ReadLine();
                stopIndex[i]=uint.Parse(temp[(temp.IndexOf(':')+1)..].Trim());

                sr.ReadLine();
                sr.ReadLine();
            }
            sr.Close();
        }

        private void LoadList() {
            if(mbd==null) return;
            if(stopIndex==null || stopIndex.Length<1) return;
            stopList.Items.Clear();

            DefReader stopDef = null;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            if(File.Exists("base/script/zh_cn/local_bus_stop.def")) {
                stopDef=new DefReader("base/script/zh_cn/local_bus_stop.def", Encoding.GetEncoding("GBK"));
            } else if(File.Exists("base/script/zh_cn/local_bus_stop.def")) {
                stopDef=new DefReader("script/zh_cn/local_bus_stop.def", Encoding.GetEncoding("GBK"));
            } else {
                lab_info.Content="无站点文件";
            }

            int left = 0;
            for(int i = 0; i<stopNum; i++) {
                Random ran = new Random();

                string stopName = "";
                if(stopDef!=null) {
                    if(stopDef.keys.TryGetValue("ui_bus_stop"+stopIndex[i], out stopName)) {
                        if(stopName.Contains("<align ")) {
                            stopName=stopName[7..stopName.IndexOf("></ali")];
                        }
                    } else {
                        stopName="";
                    }
                }

                Float3 stopPos;
                if(MbdFile.BusStop.stops.TryGetValue(stopIndex[i], out MbdFile.BusStop stop)) {
                    Node a = mbd.nodes[stop.nodeIndex];
                    stopPos = a.position;
                } else {
                    stopPos=new Float3();
                }

                CameraConfig cc = (CameraConfig)camList.Items.GetItemAt(ran.Next(0, camList.Items.Count-1));

                int up, down;
                if(i==0) {
                    up=0;
                    down=0;
                } else if(i==stopNum-1) {
                    up=0;
                    down=99;
                } else {
                    up=ran.Next(0, 20);
                    down=ran.Next(0, 20);
                }
                left =left+up-down;
                if(left<0) left=0;
                stopList.Items.Add(new StopEntity(stopIndex[i], stopName, stopPos, cc, up, down, left));
            }
            stopList.IsEnabled=true;
            lab_info.Content="读取成功";
        }

        private void WriteRoute() {
            string missionName = GetFileName(txt_mission.Text);
            string mapName = GetFileName(txt_map.Text);
            StreamWriter sw = new StreamWriter(missionName+"r.sii");
            sw.WriteLine("SiiNunit{");
            sw.WriteLine("mission : mission."+missionName+" {");
            sw.WriteLine("controller_type: bus_lane_mission_ctrl");
            sw.WriteLine("short_desc: \"@@rt_"+missionName+"_tit@@\"");
            sw.WriteLine("long_desc: \"@@rt_"+missionName+"_brf@@\"");
            sw.WriteLine("map_name: "+mapName);
            sw.WriteLine("time_limit: 4500");
            sw.WriteLine("bonus_time_limit: 180");
            sw.WriteLine("sun_idx: 1");
            sw.WriteLine("sun_idx[0]: "+ sunList.SelectedIndex);
            sw.WriteLine("sky_idx: 1");
            sw.WriteLine("sky_idx[0]: "+ skyList.SelectedIndex);
            sw.WriteLine("tier: "+txt_tier.Text);
            sw.WriteLine("rank: "+txt_rank.Text);
            sw.WriteLine("overlay_offset_x: 0");
            sw.WriteLine("overlay_offset_y: 0");
            sw.WriteLine("vehicle_data: "+vehList.SelectedItem);
            sw.WriteLine("bus_number: \""+txt_num.Text+"\"");
            sw.WriteLine("time_table_name: \"/def/mission/"+missionName+".sii\"");
            if(mbd==null) {
                sw.WriteLine("bus_stop_cameras: 0");
            } else {
                sw.WriteLine("bus_stop_cameras: "+stopNum);
                sw.WriteLine("bus_stop_cameras[0]: (0, 0, 0) (1; 0, 0, 0)");
                for(int i = 1; i<stopNum; i++) {
                    StopEntity se = (StopEntity)stopList.Items.GetItemAt(i);
                    sw.WriteLine("bus_stop_cameras["+i+"]: " + se.Camera.GetCameraString(se.Position));
                }
            }

            sw.WriteLine("entering_passengers: "+stopNum);
            for(int i = 0; i<stopNum; i++) {
                sw.WriteLine("entering_passengers["+i+"]: "+ ((StopEntity)stopList.Items.GetItemAt(i)).Up);
            }

            sw.WriteLine("max_leaving_passengers: "+stopNum);
            for(int i = 0; i<stopNum; i++) {
                sw.WriteLine("max_leaving_passengers["+i+"]: "+ ((StopEntity)stopList.Items.GetItemAt(i)).Down);
            }

            sw.WriteLine("driving_times: 0");
            sw.WriteLine("bus_stop_times: 0");
            sw.WriteLine("icons: 0");

            Random ran = new Random();
            sw.WriteLine("passengers: "+stopNum);
            sw.WriteLine("passengers[0]: \"\"");
            for(int i = 1; i<stopNum-1; i++) {
                StopEntity se = (StopEntity)stopList.Items.GetItemAt(i);
                if(se.Up==0)
                    sw.WriteLine("passengers["+i+"]: \"\"");
                else
                    sw.WriteLine("passengers["+i+"]: "+ pass[ran.Next(0, pass.Length-1)]);
            }
            sw.WriteLine("passengers["+(stopNum-1)+"]: \"\"");

            sw.WriteLine("}");
            sw.WriteLine("}");
            sw.Close();
            MessageBox.Show("生成完成！\r\n保存在："+missionName+"r.sii");
        }

        private void WriteConfig() {

            StreamWriter sw = new StreamWriter("config.sii");
            foreach(KeyValuePair<string, string> kvp in config) {
                sw.WriteLine(kvp.Key+":"+kvp.Value);
            }
            sw.Close();
        }

        private void Submit_Click(object sender, RoutedEventArgs e) {
            if(string.IsNullOrEmpty(txt_mission.Text)) {
                lab_info.Content="未指定mission";
                return;
            }
            if(string.IsNullOrEmpty(txt_map.Text)) {
                lab_info.Content="未指定地图";
                return;
            }
            if(string.IsNullOrEmpty(txt_tier.Text)) txt_tier.Text="10";
            if(string.IsNullOrEmpty(txt_rank.Text)) txt_rank.Text="10";
            if(vehList.SelectedItem==null) {
                lab_info.Content="未指定车辆";
                return;
            }
            WriteConfig();
            WriteRoute();
        }

        private void stopList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(stopList.SelectedItem==null) return;
            grid_edit.Visibility=Visibility.Visible;
            StopEntity se = (StopEntity)stopList.SelectedItem;
            txt_up.Text = se.Up.ToString();
            txt_down.Text = se.Down.ToString();
            camList.SelectedItem=se.Camera;
        }

        private void EditStop_Click(object sender, RoutedEventArgs e) {
            StopEntity se = (StopEntity)stopList.SelectedItem;
            se.Up=int.Parse(txt_up.Text);
            se.Down=int.Parse(txt_down.Text);
            se.Camera=(CameraConfig)camList.SelectedItem;
            Refresh();
            grid_edit.Visibility=Visibility.Hidden;
        }

        private void Refresh() {
            StopEntity[] t = new StopEntity[stopList.Items.Count];
            for(int i = 0; i<stopList.Items.Count; i++) {
                t[i] = (StopEntity)stopList.Items.GetItemAt(i);
            }
            stopList.Items.Clear();

            int left = 0;
            for(int i = 0; i<t.Length; i++) {
                left=left+t[i].Up-t[i].Down;
                if(left<0) left=0;
                t[i].Left=left;
                stopList.Items.Add(t[i]);
            }
        }


        private void AddDefaultCamera() {
            config.TryAdd("camera0", "d西北,-8,2,-8,&bea3059a,&bc8ed18c,&3f728dee,&bcc8dd4f");
            config.TryAdd("camera1", "d西北偏北,-4,3,-8,&be7719db,&ba2de7af,&3f784af3,&3d06557b");
            config.TryAdd("camera2", "d西北偏西,-8,2,-4,&bec5ae43,&bd007c76,&3f6ba1ac,&bd5697a9");
            config.TryAdd("camera3", "d北,0,2,-8,&3e68353f,&bbebaf3b,&3f795111,&bbf8ab69");
            config.TryAdd("camera4", "d东北,8,2,-8,&3e82e21f,&bc0cad9e,&3f777c01,&3a42074e");
            config.TryAdd("camera5", "d西,-8,2,0,&bf5a03ae,&3d0426c3,&3f05b5ec,&3cf5ba1d");
            config.TryAdd("camera6", "d东,8,2,0,&bef1a09e,&3b53fc14,&3f619c70,&3cc7d458");
            config.TryAdd("camera7", "d西南,-8,2,8,&3f68e884,&3ce4398b,&3ed3b8d4,&bcb5f2c1");
            config.TryAdd("camera8", "d西南偏西,-8,2,4,&bf41c476,&3c469e8a,&3f272f74,&3cae2284");
            config.TryAdd("camera9", "d西南偏南,-4,2,8,&bf3dcf34,&3ca5f269,&3f2b8a4c,&3cee4b8a");
            config.TryAdd("camera10", "d南,0,2,8,&3f7e0705,&bd88a13b,&3dd5cc46,&bad5e5f7");
            config.TryAdd("camera11", "d东南,8,2,8,&3f6e211b,&3c7f2dd3,&3ebb9aa6,&bc7d97f4");
            config.TryAdd("camera12", "d东南偏南,4,4,8,&3f7b2cb2,&bdb6de1d,&3e2f5765,&3be7fcd4");
            config.TryAdd("camera13", "d东俯看,8,5,0,&3f20f4b4,&bd8bdcbd,&3f457aa5,&3d905921");
        }

    }

    class StopEntity {
        public uint Index { get; set; }
        public string Name { get; set; }
        public Float3 Position { get; set; }
        public CameraConfig Camera { get; set; }
        public int Up { get; set; }
        public int Down { get; set; }
        public int Left { get; set; }

        public StopEntity(uint i, string n, Float3 p, CameraConfig c, int u, int d, int l) {
            Index=i;
            Name=n;
            Position=p;
            Camera=c;
            Up=u;
            Down=d;
            Left=l;
        }
    }

    class CameraConfig {
        public string name;
        public float dx;
        public float dy;
        public float dz;
        public string qw;
        public string qx;
        public string qy;
        public string qz;
        public CameraConfig(string str) {
            string[] vs = str.Split(',');
            if(vs.Length!=8) throw new Exception("视角配置格式错误!");
            name=vs[0];
            try {
                dx=float.Parse(vs[1]);
                dy=float.Parse(vs[2]);
                dz=float.Parse(vs[3]);
            } catch(Exception) {
                throw new Exception("视角配置数据错误!");
            }
            qw=vs[4];
            qx=vs[5];
            qy=vs[6];
            qz=vs[7];
        }
        public string GetCameraString(Float3 pos) {
            return string.Format("({0:g}, {1:g}, {2:g}) ({3:g}; {4:g}, {5:g}, {6:g})", pos.x+dx, pos.y+dy, pos.z+dz, qw, qx, qy, qz);
        }
        public override string ToString() => name;
    }
}
