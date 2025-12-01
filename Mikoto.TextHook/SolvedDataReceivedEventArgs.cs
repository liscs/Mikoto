namespace Mikoto.TextHook
{
    /// <summary>
    /// 翻译界面或文本去重界面提供的数据收到事件，满足条件：特殊码满足，且附加特殊码满足或为NoMulti将触发此事件
    /// </summary>
    public delegate void MeetHookAddressMessageReceivedEventHandler(object sender, SolvedDataReceivedEventArgs e);

    public class SolvedDataReceivedEventArgs : EventArgs
    {
        public TextHookData Data { get; set; } = new();
    }
}
