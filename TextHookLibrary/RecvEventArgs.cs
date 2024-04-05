namespace TextHookLibrary
{
    /// <summary>
    /// TextHook数据结构体
    /// </summary>
    public class TextHookData
    {
        /// <summary>
        /// 游戏进程ID
        /// </summary>
        public int GamePID { set; get; }

        /// <summary>
        /// Hook入口地址
        /// </summary>
        public string HookAddress { set; get; } = string.Empty;

        /// <summary>
        /// 通用特殊码
        /// </summary>
        public string HookCode { set; get; } = string.Empty;

        /// <summary>
        /// MisakaTranslator专用特殊码
        /// 注意 本软件单独处理一套特殊码规则：即Textrator输出的从进程到方法名之间的三组数字做记录
        /// 格式如下：特殊码【值1:值2:值3】（值1即HookAddress）
        /// </summary>
        public string MisakaHookCode { set; get; } = string.Empty;

        /// <summary>
        /// Hook方法名
        /// </summary>
        public string HookFunc { set; get; } = string.Empty;

        /// <summary>
        /// 实际内容
        /// </summary>
        public string? Data { set; get; }
    }


    /// <summary>
    /// Hook功能选择界面提供的数据收到事件，满足条件：任何一个不重复的输出都将触发此事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void HookMessageReceivedEventHandler(object sender, HookReceivedEventArgs e);

    /// <summary>
    /// Hook功能重新选择界面提供的数据收到事件，满足条件：特殊码满足，但附加特殊码不重复的输出都将触发此事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void MeetHookCodeMessageReceivedEventHandler(object sender, HookReceivedEventArgs e);

    /// <summary>
    /// 翻译界面或文本去重界面提供的数据收到事件，满足条件：特殊码满足，且附加特殊码满足或为NoMulti将触发此事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void MeetHookAddressMessageReceivedEventHandler(object sender, SolvedDataReceivedEventArgs e);

    public class HookReceivedEventArgs : EventArgs
    {
        //这个序号可以直接用于ListView
        public int Index { get; set; }
        public TextHookData Data { get; set; } = new();
    }

    public class SolvedDataReceivedEventArgs : EventArgs
    {
        public TextHookData Data { get; set; } = new();
    }
}
