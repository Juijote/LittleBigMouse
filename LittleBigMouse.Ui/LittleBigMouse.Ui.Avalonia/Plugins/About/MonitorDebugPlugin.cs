﻿using System.Threading.Tasks;
using HLab.Core.Annotations;
using LittleBigMouse.Plugins;
using LittleBigMouse.Plugins.Avalonia;

namespace LittleBigMouse.Ui.Avalonia.Plugins.About;

public class AboutMonitorPlugin(IMainService mainService) : IBootloader
{
    public Task LoadAsync(IBootContext bootstrapper)
    {
        mainService.AddControlPlugin(c =>
            c.AddViewModeButton<MonitorAboutViewMode>(
                "about",
                "Icon/MonitorAbout",
                "关于")/* 汉化相关 */
        );
        return Task.CompletedTask;
    }
}