﻿using System;

namespace TextHookLibrary {
    /// <summary>
    /// TextHook数据结构体
    /// </summary>
    public class TextHookData {
        /// <summary>
        /// 游戏进程ID
        /// </summary>
        public int GamePID { set; get; }

        /// <summary>
        /// Hook入口地址
        /// </summary>
        public string HookAddress { set; get; }

        /// <summary>
        /// 通用特殊码
        /// </summary>
        public string HookCode { set; get; }

        /// <summary>
        /// MisakaTranslator专用特殊码
        /// 注意 本软件单独处理一套特殊码规则：即Textrator输出的从进程到方法名之间的三组数字做记录
        /// 格式如下：特殊码【值1:值2:值3】（值1即HookAddress）
        /// </summary>
        public string MisakaHookCode { set; get; }

        /// <summary>
        /// Hook方法名
        /// </summary>
        public string HookFunc { set; get; }

        /// <summary>
        /// 实际内容
        /// </summary>
        public string Data { set; get; }
    }


    /// <summary>
    /// Hook功能选择界面提供的数据收到事件，满足条件：任何一个不重复的输出都将触发此事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void HookFunSelectDataRecvEventHandler(object sender, HookSelectRecvEventArgs e);

    /// <summary>
    /// Hook功能重新选择界面提供的数据收到事件，满足条件：特殊码满足，但附加特殊码不重复的输出都将触发此事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void HookFunReSelectDataRecvEventHandler(object sender, HookSelectRecvEventArgs e);

    /// <summary>
    /// 翻译界面或文本去重界面提供的数据收到事件，满足条件：特殊码满足，且附加特殊码满足或为NoMulti将触发此事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    /* 项目“TextHookLibrary (netframework4.7.2)”的未合并的更改
    在此之前:
        public delegate void SolvedDataRecvEventHandler(object sender, SolvedDataRecvEventArgs e);

        public class HookSelectRecvEventArgs : EventArgs
    在此之后:
        public delegate void SolvedDataRecvEventHandler(object sender, SolvedDataRecvEventArgs e);

        public class HookSelectRecvEventArgs : EventArgs
    */
    public delegate void SolvedDataRecvEventHandler(object sender, SolvedDataRecvEventArgs e);

    public class HookSelectRecvEventArgs : EventArgs {
        //方法序号（仅在HookFunSelectDataRecvEventHandler和HookFunReSelectDataRecvEventHandler事件中有效）
        //这个序号可以直接用于ListView
        public int Index { get; set; }
        public TextHookData Data { get; set; }
    }

    public class SolvedDataRecvEventArgs : EventArgs {
        public TextHookData Data { get; set; }
    }
}
