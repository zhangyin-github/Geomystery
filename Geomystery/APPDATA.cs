﻿using Geomystery.Models;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Geomystery
{
    public class APPDATA
    {
        public bool ISMUTE { get; set; }
        public bool ISNIGHT { get; set; }
        public bool ISFULLSCREEN { get; set; }
        public int GAMEMODE { get; set; }
        public int HAVEDONE { get; set; }
        public List<ViewModel.ViewModel> Views;
        public Grid MAINGRID { get; set; }
        public static APPDATA app_data;

        public APPDATA()
        {
            Views = new List<ViewModel.ViewModel>();
            ISMUTE = false;
            ISNIGHT = false;
            ISFULLSCREEN=true;
            HAVEDONE = 0;
            GAMEMODE = -1;
            MAINGRID = new Grid();
        }
        public static void SAVE()
        {
            string DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "APPDATA.db");
            var conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
            conn.CreateTable<option_data>();// 创建 option_data 模型对应的表，如果已存在，则忽略该操作。
            var db = conn.Table<option_data>();
            var k = new option_data();
            if (db.Count() == 0) conn.Insert(k);
            else
            {
                conn.DeleteAll(typeof(option_data));
                conn.Insert(k);
            }
        }
        public static void LOAD()
        {
            string DbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "APPDATA.db");
            var conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
            conn.CreateTable<option_data>();// 创建 option_data 模型对应的表，如果已存在，则忽略该操作。
            var db = conn.Table<option_data>();
            if (db.Count() == 0) return;
            else
            {
                option_data k = db.First();
                app_data.ISFULLSCREEN = k.ISFULLSCREEN;
                app_data.ISMUTE = k.ISMUTE;
                app_data.ISNIGHT = k.ISNIGHT;
                app_data.HAVEDONE = k.HAVEDONE;
            }
        }
        public void setFullScreen()
        {
            ApplicationView current_window = ApplicationView.GetForCurrentView();
            app_data.ISFULLSCREEN = current_window.IsFullScreenMode;
            if (app_data.ISFULLSCREEN)
            {
                current_window.ExitFullScreenMode();
            }
            else
            {
                current_window.TryEnterFullScreenMode();
            }
            app_data.ISFULLSCREEN = current_window.IsFullScreenMode;
        }
        public void setNight()
        {
            app_data.ISNIGHT = !app_data.ISNIGHT;
            update_views();
        }

        public void setMute()
        {
            app_data.ISMUTE = !app_data.ISMUTE;
            BGMPlayer.setMute();
        }
        public void update_views()
        {
            foreach(var View in Views)
            {
                View.Theme = !app_data.ISNIGHT ? ElementTheme.Light : ElementTheme.Dark;
            }
        }
        async public Task<int> Reset()
        {
            var dialog = new ContentDialog()
            {
                Title = "警告",
                Content = "请确认您是否要重置游戏的所有进程？",
                PrimaryButtonText = "确定",
                SecondaryButtonText = "取消",
                FullSizeDesired = false,
            };
            dialog.PrimaryButtonClick += (_s, _e) => { };
            var res = await dialog.ShowAsync();
            if (res.ToString() != "Primary") return 0;
            var k =  new List<ViewModel.ViewModel>(app_data.Views);
            APPDATA.app_data = new APPDATA();
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            APPDATA.app_data.Views = k;
            update_views();
            return 0;
        }
        public string show()
        {
            var ss = "";
            ss = ISMUTE.ToString() + " " + ISNIGHT.ToString() + " " + ISFULLSCREEN;
            return ss;
        }
    }
    public class option_data
    {
        public bool ISMUTE { get; set; }
        public bool ISNIGHT { get; set; }
        public bool ISFULLSCREEN { get; set; }
        public int HAVEDONE { get; set; }
        public option_data()
        {
            ISMUTE = APPDATA.app_data.ISMUTE;
            ISNIGHT = APPDATA.app_data.ISNIGHT;
            ISFULLSCREEN = APPDATA.app_data.ISFULLSCREEN;
            HAVEDONE = APPDATA.app_data.HAVEDONE;
        }
    }
}
