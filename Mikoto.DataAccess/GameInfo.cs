namespace Mikoto.DataAccess
{
    public class GameInfo
    {
        /// <summary>
        /// 游戏名（非进程名，但在游戏名未知的情况下先使用进程所在的文件夹名替代）
        /// </summary>
        public string GameName { get; set; } = string.Empty;

        /// <summary>
        /// 游戏文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 游戏ID
        /// </summary>
        public Guid GameID { get; set; }

        /// <summary>
        /// 翻译模式,1=hook
        /// </summary>
        public int TransMode { get; set; }

        /// <summary>
        /// 源语言代码，同翻译API中语言代码
        /// </summary>
        public string SrcLang { get; set; } = string.Empty;

        /// <summary>
        /// 目标语言代码，同翻译API中语言代码
        /// </summary>
        public string DstLang { get; set; } = string.Empty;

        /// <summary>
        /// 去重方法，仅在hook模式有效
        /// </summary>
        public string? RepairFunc { get; set; }

        /// <summary>
        /// 去重方法所需参数1，仅在hook模式有效
        /// </summary>
        public string? RepairParamA { get; set; }

        /// <summary>
        /// 去重方法所需参数2，仅在hook模式有效
        /// </summary>
        public string? RepairParamB { get; set; }

        /// <summary>
        /// 特殊码值，仅在hook模式有效
        /// </summary>
        public string HookCode { get; set; } = string.Empty;

        /// <summary>
        /// 用户自定的特殊码值，如果用户这一项不是自定义的，那么应该为'NULL'，仅在hook模式有效，注意下次开启游戏时这里就需要注入一下
        /// </summary>
        public string? HookCodeCustom { get; set; }

        /// <summary>
        /// 检查是否是64位应用程序
        /// </summary>
        public bool Isx64 { get; set; }

        /// <summary>
        /// 包含hook地址信息的本软件特有的MisakaHookCode
        /// </summary>
        public string MisakaHookCode { get; set; } = string.Empty;

        public DateTime LastPlayAt { get; set; }
    }
}
