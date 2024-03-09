using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Win32;

namespace HLab.Sys.Windows.Monitors;

public class MonitorDevice : IEquatable<MonitorDevice>
{
    [DataMember] public string Id { get; init; }
    [DataMember] public string PnpCode { get; init; }
    [DataMember] public string PhysicalId { get; set; }
    [DataMember] public string SourceId { get; set; }
    [DataMember] public IEdid Edid { get; init; }
    [DataMember] public string MonitorNumber { get; set; }

    public List<MonitorDeviceConnection> Connections = new ();

    public override bool Equals(object obj)
    {
        if(obj is MonitorDevice other) return Id == other.Id;
        return base.Equals(obj);
    }

    public override int GetHashCode() => HashCode.Combine(Id);

    public bool Equals(MonitorDevice other) => Id == other.Id;
}

public class MonitorDeviceConnection : DisplayDevice
{
    public new PhysicalAdapter Parent
    {
        get => base.Parent as PhysicalAdapter;
        set => base.Parent = value;
    }

    public MonitorDevice Monitor { get; set; }
    /* 汉化相关 */
    class EdidDesign : IEdid
    {
        public EdidDesign() 
        {        
            if(!Design.IsDesignMode) throw new InvalidOperationException("仅适用于设计模式");
        }

        public string HKeyName => "HKLM://";
        public string ManufacturerCode => "SAM";
        public string ProductCode { get; }
        public string Serial { get; }
        public int Week => 42;
        public int Year { get; }
        public string Version { get; }
        public bool Digital { get; }
        public int BitDepth { get; }
        public string VideoInterface { get; }
        public Size PhysicalSize => new Size(600, 340);
        public string Model => "S24D300";
        public string SerialNumber => "S/N: 123456789";
        public double Gamma => 2.2;
        public bool DpmsStandbySupported => true;
        public bool DpmsSuspendSupported => true;
        public bool DpmsActiveOffSupported => true;
        public bool YCrCb444Support => true;
        public bool YCrCb422Support => true;
        public double sRGB => 0.98;
        public double RedX => 0.64;
        public double RedY => 0.33;
        public double GreenX => 0.3;
        public double GreenY => 0.6;
        public double BlueX => 0.15;
        public double BlueY => 0.06;
        public double WhiteX => 0.3127;
        public double WhiteY => 0.3127; 
        public int Checksum => int.MinValue;
    }

    public static MonitorDeviceConnection MonitorDesign
    {
        get
        {
            if(!Design.IsDesignMode) throw new InvalidOperationException("仅适用于设计模式");

            return new MonitorDeviceConnection
            {
                //Edid = new EdidDesign()
            };
        }
    }

    //------------------------------------------------------------------------
    public override bool Equals(object obj) => obj is MonitorDeviceConnection other ? Id == other.Id : base.Equals(obj);


    public override int GetHashCode() {
        return ("显示监视器" + Id).GetHashCode();
    }

    void OpenRegKey(string keyString) {

        keyString = keyString.Replace(@"\MACHINE\",@"\HKEY_LOCAL_MACHINE\");
        keyString = keyString.Replace(@"\USER\",@"\HKEY_CURRENT_USER\");

        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit", true)) {
            
            if(key == null) return;
            var value = key.GetValue("LastKey").ToString();

            var list = value.Split('\\');
            if(list.Length > 0)
            {
                keyString = keyString.Replace(@"\REGISTRY\",@$"{list[0]}\");
                key.SetValue("LastKey", keyString);
            }
        }

        Process.Start("regedit.exe");
    }


    public void DisplayValues(Action<string, string, Action, bool> addValue) {
        //addValue("Registry", Edid.HKeyName, () => { OpenRegKey(Edid.HKeyName); }, false);
        //addValue("Microsoft Id", PhysicalId, null, false);

        // EnumDisplaySettings
        addValue("", "枚举显示设置", null, true);
        addValue("显示方向", Parent?.CurrentMode?.DisplayOrientation.ToString(), null, false);
        addValue("位置", Parent?.CurrentMode?.Position.ToString(), null, false);
        addValue("分辨率", Parent?.CurrentMode?.Pels.ToString(), null, false);
        addValue("每像素位数", Parent?.CurrentMode?.BitsPerPixel.ToString(), null, false);
        addValue("显示频率", Parent?.CurrentMode?.DisplayFrequency.ToString(), null, false);
        addValue("显示标志", Parent?.CurrentMode?.DisplayFlags.ToString(), null, false);
        addValue("显示固定输出", Parent?.CurrentMode?.DisplayFixedOutput.ToString(), null, false);

        // GetDeviceCaps
        addValue("", "获取设备上限", null, true);
        addValue("尺寸", Parent.Capabilities.Size.ToString(), null, false);
        addValue("资源", Parent.Capabilities.Resolution.ToString(), null, false);
        addValue("对数像素", Parent.Capabilities.LogPixels.ToString(), null, false);
        addValue("位像素", Parent.Capabilities.BitsPixel.ToString(), null, false);
        //AddValue("Color Planes", Monitor.Adapter.DeviceCaps.Planes.ToString());
        addValue("角度", Parent.Capabilities.Aspect.ToString(), null, false);
        //AddValue("BltAlignment", Monitor.Adapter.DeviceCaps.BltAlignment.ToString());

        //GetDpiForMonitor
        addValue("", "获取显示器的 DPI", null, true);
        addValue("有效分辨率", Parent.EffectiveDpi.ToString(), null, false);
        addValue("角度 DPI", Parent.AngularDpi.ToString(), null, false);
        addValue("原始 DPI", Parent.RawDpi.ToString(), null, false);

        // GetMonitorInfo
        addValue("", "获取显示器信息", null, true);
        addValue("基本", Parent.Primary.ToString(), null, false);
        addValue("监控区域", Parent.MonitorArea.ToString(), null, false);
        addValue("工作区域", Parent.WorkArea.ToString(), null, false);


        //// EDID
        //addValue("", "EDID", null, true);
        //addValue("ManufacturerCode", Edid?.ManufacturerCode, null, false);
        //addValue("ProductCode", Edid?.ProductCode, null, false);
        //addValue("Serial", Edid?.Serial, null, false);
        //addValue("Model", Edid?.Model, null, false);
        //addValue("SerialNo", Edid?.SerialNumber, null, false);
        //addValue("SizeInMm", Edid?.PhysicalSize.ToString(), null, false);
        //addValue("VideoInterface", Edid?.VideoInterface.ToString(), null, false);

        // GetScaleFactorForMonitor
        addValue("", "获取显示器的缩放比例", null, true);
        addValue("缩放比例", Parent.ScaleFactor.ToString(), null, false);

        // EnumDisplayDevices
        addValue("", "枚举显示设备", null, true);
        addValue("设备 ID", Parent?.Id, null, false);
        addValue("设备密钥", Parent?.DeviceKey, null, false);
        addValue("设备字符串", Parent?.DeviceString, null, false);
        addValue("设备名称", Parent?.DeviceName, null, false);
        addValue("状态标识", Parent?.State.ToString(), null, false);

        addValue("", "枚举显示设备", null, true);
        addValue("设备 ID", Id, null, false);
        addValue("设备密钥", DeviceKey, null, false);
        addValue("设备字符串", DeviceString, null, false);
        addValue("设备名称", DeviceName, null, false);
        addValue("状态标识", State.ToString(), null, false);

    }
    public override string ToString() => DeviceString;

}