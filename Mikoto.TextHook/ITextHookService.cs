using Mikoto.DataAccess;

namespace Mikoto.TextHook
{
    public interface ITextHookService : IDisposable
    {
        void ClearHistory();

        int GamePID { get; set; }
        bool Paused { get; set; }
        Queue<string> TextractorOutPutHistory { get; }

        event HookMessageReceivedEventHandler? HookMessageReceived;
        event MeetHookAddressMessageReceivedEventHandler? MeetHookAddressMessageReceived;

        void AddClipBoardWatcher();
        void AddTextractorHistory(string output);
        Task AttachProcessByHookCodeAsync(int pid, string HookCode);
        Task<bool> AutoAddCustomHookToGameAsync();
        void CloseTextractor();
        void DetachUnrelatedHooks(int pid, List<string> UsedHookAddress);
        string? GetHookAddressByMisakaCode(string MisakaCode);
        bool Init(string path);
        Task StartHookAsync(GameInfo gameInfo, bool AutoHook = false);
        /// <summary>
        /// 自动启动 Textractor 并附加到指定进程
        /// </summary>
        Task AutoStartAsync(string textractorPath,  GameInfo gameInfo);
    }
}
