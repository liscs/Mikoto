namespace Mikoto.TextHook
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
}
