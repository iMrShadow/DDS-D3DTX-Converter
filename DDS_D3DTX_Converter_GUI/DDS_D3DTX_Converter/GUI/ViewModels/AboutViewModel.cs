using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using HyperText.Avalonia.Models;

namespace DDS_D3DTX_Converter.ViewModels;

public class AboutViewModel : ViewModelBase
{
    /// <summary>
    /// TODO Hyperlinks are not yet supported. I want to add Github links in the future. Nothing in here is used.
    /// </summary>
    
    // public IEnumerable<HyperlinkContent> HyperlinkContentProvider => new[]
    // {
    //     new HyperlinkContent
    //         { Alias = "dedede", Url = "https://docs.avaloniaui.net/docs/styling/styles" },
    //     new HyperlinkContent { Alias = "edvyydebbvydebvyed    " },
    //     new HyperlinkContent { Url = "https://docs.avaloniaui.net/docs/styling/styles" }
    // };

    public string Url1 = "https://docs.avaloniaui.net/docs/styling/styles";
   
  
    public string Alias1 = "dedede/dedede/dedede/dedede";

    public HyperlinkContent HyperlinkContentProvider => new HyperlinkContent
        { Alias = "dedede", Url = "https://docs.avaloniaui.net/docs/styling/styles" };
    
    
    public void OpenUrl(object urlObj)
    {
        var url = urlObj as string;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //https://stackoverflow.com/a/2796367/241446
            using var proc = new Process { StartInfo = { UseShellExecute = true, FileName = url } };
            proc.Start();

            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("x-www-browser", url);
            return;
        }

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) throw new ArgumentException("invalid url: " + url);
        Process.Start("open", url);
        return;
    }
}