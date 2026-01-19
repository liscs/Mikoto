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
        Task AttachProcessAsync(int pid);
        Task AttachProcessByHookCodeAsync(int pid, string HookCode);
        Task<bool> AutoAddCustomHookToGameAsync();
        void CloseTextractor();
        Task DetachProcessAsync(int pid);
        Task DetachProcessByHookAddressAsync(int pid, string HookAddress);
        void DetachUnrelatedHooks(int pid, List<string> UsedHookAddress);
        string? GetHookAddressByMisakaCode(string MisakaCode);
        bool Init(string path);
        Task StartHookAsync(GameInfo gameInfo, bool AutoHook = false);
        Task StartAsync(string textractorPath, int pid, GameInfo filePath);
    }
}
