namespace Mikoto.TextHook
{

    /// <summary>
    /// Hook功能选择界面提供的数据收到事件，满足条件：任何一个不重复的输出都将触发此事件
    /// </summary>
    public delegate void HookMessageReceivedEventHandler(object sender, HookReceivedEventArgs e);


    public class HookReceivedEventArgs : EventArgs
    {
        //这个序号可以直接用于ListView
        public int Index { get; set; }
        public TextHookData Data { get; set; } = new();
    }
}
