using Mikoto.DataAccess;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.DataTransfer;
using Clipboard = Windows.ApplicationModel.DataTransfer.Clipboard;

namespace Mikoto.TextHook
{
    /// <summary>
    /// 该类使用TextractorCLI版本进行读写
    /// </summary>
    public partial class TextHookHandle : IDisposable
    {
        /// <summary>
        /// Textractor进程
        /// </summary>
        public Process? ProcessTextractor;

        /// <summary>
        /// Hook功能选择界面提供的数据收到事件
        /// </summary>
        public event HookMessageReceivedEventHandler? HookMessageReceived;

        /// <summary>
        /// 翻译界面或文本去重界面提供的数据收到事件
        /// </summary>
        public event MeetHookAddressMessageReceivedEventHandler? MeetHookAddressMessageReceived;

        /// <summary>
        /// 暂停Hook标志,为真时暂停获取文本
        /// </summary>
        public bool Paused { get; set; }


        public Queue<string> TextractorOutPutHistory { get; private set; }//Textractor的输出记录队列，用于查错

        public int GamePID { get; set; }//能够获取到文本的游戏进程ID
        private Dictionary<Process, bool> _possibleGameProcessList;//与gamePID进程同名的进程列表
        private int _handleMode;//处理的方式 1=已确定的单个进程 2=多个进程寻找能搜到文本的进程
        private Process? _maxMemoryProcess;//最大内存进程，用于智能处理时单独注入这个进程而不是PossibleGameProcessList中的每个进程都注入

        private int listIndex;//用于Hook功能选择界面的方法序号
        private Dictionary<string, int> _textractorFunIndexList;//Misaka特殊码与列表索引一一对应

        private GameInfo? _gameInfo;

        public TextHookHandle(int gamePID)
        {
            ProcessTextractor = null;
            _maxMemoryProcess = null;
            GamePID = gamePID;
            _possibleGameProcessList = new Dictionary<Process, bool>();
            TextractorOutPutHistory = new Queue<string>(1000);
            _handleMode = 1;
            listIndex = 0;
            _textractorFunIndexList = new Dictionary<string, int>();
        }

        public TextHookHandle(List<Process> GameProcessList)
        {
            ProcessTextractor = null;
            GamePID = -1;
            TextractorOutPutHistory = new Queue<string>(1000);
            _possibleGameProcessList = new Dictionary<Process, bool>();
            _maxMemoryProcess = GameProcessList[0];
            for (int i = 0; i < GameProcessList.Count; i++)
            {
                if (GameProcessList[i].WorkingSet64 > _maxMemoryProcess.WorkingSet64)
                {
                    _maxMemoryProcess = GameProcessList[i];
                }
                _possibleGameProcessList.Add(GameProcessList[i], false);
            }

            _handleMode = 2;
            listIndex = 0;
            _textractorFunIndexList = new Dictionary<string, int>();
        }

        public TextHookHandle()
        {
            //剪贴板方式读取专用
            _maxMemoryProcess = null;
            GamePID = -1;
            _possibleGameProcessList = new Dictionary<Process, bool>();
            TextractorOutPutHistory = new Queue<string>(1000);
            _handleMode = 3;
            listIndex = 0;
            _textractorFunIndexList = new Dictionary<string, int>();
        }

        /// <summary>
        /// 初始化Textractor,建立CLI与本软件间的通信
        /// </summary>
        /// <returns>成功返回真，失败返回假</returns>
        public bool Init(string path)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            ProcessTextractor = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = path,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    StandardInputEncoding = new UnicodeEncoding(false, false),
                    StandardOutputEncoding = Encoding.Unicode,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = Path.GetDirectoryName(path)
                },
            };

            ProcessTextractor.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            try
            {
                bool res = ProcessTextractor.Start();
                ProcessTextractor.BeginOutputReadLine();
                return res;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                ProcessTextractor.Dispose();
                ProcessTextractor = null;
                return false;
            }
        }

        /// <summary>
        /// 向Textractor CLI写入命令
        /// 注入进程
        /// </summary>
        /// <param name="pid"></param>
        public async Task AttachProcessAsync(int pid)
        {
            if (ProcessTextractor == null) { return; }
            await ProcessTextractor.StandardInput.WriteLineAsync("attach -P" + pid);
            await ProcessTextractor.StandardInput.FlushAsync();
        }

        /// <summary>
        /// 向Textractor CLI写入命令
        /// 结束注入进程
        /// </summary>
        /// <param name="pid"></param>
        public async Task DetachProcessAsync(int pid)
        {
            if (!ProcessHelper.IsProcessRunning(pid)
                || ProcessTextractor == null)
            {
                return;
            }

            try
            {
                await ProcessTextractor.StandardInput.WriteLineAsync("detach -P" + pid);
                await ProcessTextractor.StandardInput.FlushAsync();
            }
            //kill已经退出的进程会抛异常，无需处理
            catch { }
        }

        /// <summary>
        /// 向Textractor CLI写入命令
        /// 给定特殊码注入，由Textractor作者指导方法
        /// </summary>
        /// <param name="pid"></param>
        public async Task AttachProcessByHookCodeAsync(int pid, string HookCode)
        {
            if (ProcessTextractor == null) { return; }
            await ProcessTextractor.StandardInput.WriteLineAsync(HookCode + " -P" + pid);
            await ProcessTextractor.StandardInput.FlushAsync();
        }

        /// <summary>
        /// 向Textractor CLI写入命令
        /// 根据Hook入口地址卸载一个Hook，由Textractor作者指导方法
        /// </summary>
        /// <param name="pid"></param>
        public async Task DetachProcessByHookAddressAsync(int pid, string HookAddress)
        {
            //这个方法的原理是注入一个用户给定的钩子，给定一个Hook地址，由于hook地址存在，Textractor会自动卸载掉之前的
            //但是后续给定的模块并不存在，于是Textractor再卸载掉这个用户自定义钩子，达到卸载一个指定Hook办法
            if (!ProcessHelper.IsProcessRunning(pid)
                || ProcessTextractor == null)
            {
                return;
            }

            await ProcessTextractor.StandardInput.WriteLineAsync("HW0@" + HookAddress + ":module_which_never_exists" + " -P" + pid);
            await ProcessTextractor.StandardInput.FlushAsync();
        }

        /// <summary>
        /// 关闭Textractor进程，关闭前Detach所有Hook
        /// </summary>
        public async void CloseTextractor()
        {
            if (ProcessTextractor != null && ProcessTextractor.HasExited == false)
            {
                if (_handleMode == 1 && ProcessHelper.IsProcessRunning(GamePID))
                {
                    await DetachProcessAsync(GamePID);
                }
                else if (_handleMode == 2)
                {
                    foreach (var item in _possibleGameProcessList.ToList())
                        if (_possibleGameProcessList[item.Key] == true)
                        {
                            if (ProcessHelper.IsProcessRunning(item.Key.Id))
                                await DetachProcessAsync(item.Key.Id);
                            _possibleGameProcessList[item.Key] = false;
                        }
                }
                //kill已经退出的进程会抛异常，无需处理
                try
                {
                    ProcessTextractor.Kill();
                }
                catch { }
            }
            ProcessTextractor = null;
        }

        /// <summary>
        /// 开始注入，会判断是否智能注入
        /// </summary>
        public async Task StartHook(GameInfo gameInfo, bool AutoHook = false)
        {
            _gameInfo = gameInfo;
            if (_handleMode == 1)
            {
                await AttachProcessAsync(GamePID);
            }
            else if (_handleMode == 2)
            {
                //不管是否进行智能注入，为了保证再次开启游戏时某些用户自定义特殊码能直接导入，这里强制让游戏ID为最大进程ID
                GamePID = _maxMemoryProcess!.Id;

                if (AutoHook == false)
                {
                    //不进行智能注入
                    foreach (var item in _possibleGameProcessList.ToList())
                    {
                        await AttachProcessAsync(item.Key.Id);
                        _possibleGameProcessList[item.Key] = true;
                    }
                }
                else
                {
                    await AttachProcessAsync(_maxMemoryProcess.Id);
                }
            }
        }

        private string _bestMatchCode = string.Empty;

        private Stopwatch _lastMessageStopwatch = Stopwatch.StartNew();

        private TextHookData? _thData;
        private bool _disposedValue;

        /// <summary>
        /// 控制台输出事件，在这做内部消化处理
        /// </summary>
        /// <param name="sendingProcess"></param>
        /// <param name="outLine"></param>
        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            _lastMessageStopwatch.Restart();
            if (outLine.Data == null) { return; }
            AddTextractorHistory(outLine.Data);
            if (Paused) { return; }
            _thData = DealTextratorOutput(outLine.Data);
            if (_thData != null)
            {
                TextHookData data = _thData;

                if (data.HookFunc != "Console" && data.HookFunc != "Clipboard" && data.HookFunc != "")
                {
                    //Hook入口选择窗口处理
                    if (_textractorFunIndexList.ContainsKey(data.MisakaHookCode))
                    {
                        HookReceivedEventArgs e = new HookReceivedEventArgs();
                        e.Index = _textractorFunIndexList[data.MisakaHookCode];
                        e.Data = data;
                        HookMessageReceived?.Invoke(this, e);
                    }
                    else
                    {
                        _textractorFunIndexList.Add(data.MisakaHookCode, listIndex);
                        HookReceivedEventArgs e = new HookReceivedEventArgs();
                        e.Index = _textractorFunIndexList[data.MisakaHookCode];
                        e.Data = data;
                        HookMessageReceived?.Invoke(this, e);
                        listIndex++;
                    }
                    if (_gameInfo?.HookCode == data.HookCode)
                    {
                        Regex regex = MisakaCodeRegex();
                        //保存的misakacode
                        string savedMisakaCode = _gameInfo.MisakaHookCode;
                        //取得的misakacode
                        string obtainedMisakaCode = data.MisakaHookCode;

                        Match obtainedMatch = regex.Match(obtainedMisakaCode);
                        Debug.Assert(obtainedMatch.Success);//传入值应该合法
                        if (InvalidMisakaCodeRegex().IsMatch(obtainedMisakaCode))
                        {
                            //无效返回
                            return;
                        }

                        string obtainedMisakaCode1 = obtainedMatch.Groups[1].Value;

                        //文本去重窗口处理&游戏翻译窗口处理
                        // TODO 寻找更好的Hook Address确定方法
                        if (obtainedMisakaCode1.Length >= 4
                            && (obtainedMisakaCode == _bestMatchCode || IsMoreMatch(savedMisakaCode, obtainedMisakaCode, _bestMatchCode)))
                        {
                            _bestMatchCode = obtainedMisakaCode;

                            SolvedDataReceivedEventArgs e = new()
                            {
                                Data = data
                            };
                            MeetHookAddressMessageReceived?.Invoke(this, e);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// check相较于compare更接近target，返回true；compare更接近target，或者相似度相同，则返回false
        /// </summary>
        private static bool IsMoreMatch(string target, string check, string compare)
        {
            int dist1 = EditDistance.GetLevenshteinDistance(target, check);
            int dist2 = EditDistance.GetLevenshteinDistance(target, compare);
            return dist1 < dist2;
        }

        [GeneratedRegex("【0:FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF】|【FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF:FFFFFFFFFFFFFFFF】")]
        private static partial Regex InvalidMisakaCodeRegex();


        /// <summary>
        /// 卸载无关联Hook：缓解某些游戏同时进行很多Hook时造成的卡顿现象
        /// </summary>
        /// <param name="pid">欲操作的进程号（要求确保此进程号是游戏主进程且先对此进程号Attach）</param>
        /// <param name="UsedHookAddress">正在使用中的HookAddress列表，将卸载掉不存在于这个列表中的其他Hook</param>
        public async void DetachUnrelatedHooks(int pid, List<string> UsedHookAddress)
        {

            var FunList = _textractorFunIndexList.Keys.ToList();//这个得到的是MisakaCode列表
            for (int i = 0; i < _textractorFunIndexList.Count; i++)
            {
                string hookAddress = GetHookAddressByMisakaCode(FunList[i]) ?? string.Empty;
                if (!UsedHookAddress.Contains(hookAddress))
                {
                    await DetachProcessByHookAddressAsync(pid, hookAddress);
                }
            }
        }

        /// <summary>
        /// 通过MisakaCode提取HookAddress
        /// </summary>
        /// <param name="MisakaCode"></param>
        /// <returns></returns>
        public string? GetHookAddressByMisakaCode(string MisakaCode)
        {
            return GetMiddleString(MisakaCode, "【", ":", 0);
        }

        /// <summary>
        /// 记录Textractor的历史输出记录
        /// </summary>
        /// <param name="output"></param>
        public void AddTextractorHistory(string output)
        {
            if (TextractorOutPutHistory.Count >= 1000)
            {
                TextractorOutPutHistory.Dequeue();
            }
            TextractorOutPutHistory.Enqueue(output);
        }

        /// <summary>
        /// 取出文本中间一部分
        /// </summary>
        /// <param name="Text">整个文本</param>
        /// <param name="front">前面的文本</param>
        /// <param name="back">后面的文本</param>
        /// <param name="location">起始搜寻位置</param>
        /// <returns></returns>
        private string? GetMiddleString(string Text, string front, string back, int location)
        {

            if (front == "" || back == "")
            {
                return null;
            }

            int locA = Text.IndexOf(front, location);
            int locB = Text.IndexOf(back, locA + 1);
            if (locA < 0 || locB < 0)
            {
                return null;
            }
            else
            {
                locA += front.Length;
                locB -= locA;
                if (locA < 0 || locB < 0)
                {
                    return null;
                }
                return Text.Substring(locA, locB);
            }
        }

        /// <summary>
        /// 智能处理来自Textrator的输出并返回一个TextHookData用于下一步处理(TextHookData可为空)
        /// 具体的含义参见TextHookData定义
        /// </summary>
        /// <param name="OutputText">来自Textrator的输出</param>
        /// <returns></returns>
        private TextHookData? DealTextratorOutput(string OutputText)
        {
            if (OutputText == "" || OutputText == null)
            {
                return null;
            }

            string? Info = GetMiddleString(OutputText, "[", "]", 0);
            if (Info == null)
            {
                if (_thData == null)
                {
                    return null;
                }
                //得到的是第二段被截开的输出，需要连到上一段内
                _thData.Data += OutputText;
                return _thData;
            }

            string[] Infores = Info.Split(':');

            if (Infores.Length >= 7)
            {
                TextHookData thd = new TextHookData();

                string content = OutputText.Replace("[" + Info + "] ", "");//删除信息头部分
                try
                {
                    thd.GamePID = int.Parse(Infores[1], System.Globalization.NumberStyles.HexNumber); //游戏/本体进程ID（为0一般代表Textrator本体进程ID）

                }
                catch (FormatException)
                {
                    return null;
                }

                thd.HookFunc = Infores[5]; //方法名：Textrator注入游戏进程获得文本时的方法名（为 Console 时代表Textrator本体控制台输出；为 Clipboard 时代表从剪贴板获取的文本）

                thd.HookCode = Infores[6]; //特殊码：Textrator注入游戏进程获得文本时的方法的特殊码，是一个唯一值，可用于判断

                thd.Data = content; //实际获取到的内容

                thd.HookAddress = Infores[2]; //Hook入口地址：可用于以后卸载Hook

                thd.MisakaHookCode = "【" + Infores[2] + ":" + Infores[3] + ":" + Infores[4] + "】"; //【值1:值2:值3】见上方格式说明


                return thd;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// 让系统自动注入用户设定好的特殊码，没有就不注入
        /// </summary>
        public async Task<bool> AutoAddCustomHookToGameAsync()
        {
            if (!string.IsNullOrEmpty(_gameInfo?.HookCodeCustom) && _gameInfo?.HookCodeCustom != "NULL")
            {
                //自定义hook码需要等textractor先注入进程后再注入
                Stopwatch timeoutStopWatch = Stopwatch.StartNew();
                while (true)
                {
                    const int LAST_RECEIVE_DURATION = 3;
                    //在最后一次收到消息后一段时间内无消息，认为初始化已完毕
                    if (_lastMessageStopwatch.Elapsed > TimeSpan.FromSeconds(LAST_RECEIVE_DURATION))
                    {
                        await AttachProcessByHookCodeAsync(GamePID, _gameInfo!.HookCodeCustom);
                        return true;
                    }
                    //持续收到消息，超时后直接注入
                    const int ALWAYS_RECEIVE_TIMEOUT = 30;
                    if (timeoutStopWatch.Elapsed > TimeSpan.FromSeconds(ALWAYS_RECEIVE_TIMEOUT))
                    {
                        await AttachProcessByHookCodeAsync(GamePID, _gameInfo!.HookCodeCustom);
                        return false;
                    }
                }
            }
            return true;
        }


        /// <summary>
        /// 添加剪切板监视
        /// </summary>
        /// <param name="winHandle"></param>
        public void AddClipBoardWatcher()
        {
            Clipboard.ContentChanged += Clipboard_ContentChanged;
        }

        private async void Clipboard_ContentChanged(object? sender, object e)
        {
            DataPackageView dataPackageView = Clipboard.GetContent();
            if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                string text = await dataPackageView.GetTextAsync();

                ClipboardUpdated(text);
            }
        }

        /// <summary>
        /// 剪贴板更新事件
        /// </summary>
        /// <param name="ClipboardText"></param>
        private void ClipboardUpdated(string ClipboardText)
        {
            if (Paused) // 暂停时什么也不做
                return;
            SolvedDataReceivedEventArgs e = new SolvedDataReceivedEventArgs
            {
                Data = new TextHookData()
                {
                    GamePID = -1,
                    HookAddress = "0",
                    HookFunc = "Clipboard",
                    HookCode = "HB0@0",
                    MisakaHookCode = "【0:-1:-1】",
                    Data = ClipboardText
                }
            };
            MeetHookAddressMessageReceived?.Invoke(this, e);
        }





        [GeneratedRegex("【(.+):(.+):(.+)】")]
        private static partial Regex MisakaCodeRegex();


        /// <summary>
        /// 这个方法用于翻译窗口关闭或者导航窗口关闭时调用，进行TextractorCLI的全方法卸载和关闭，否则会出现无法hook其他游戏的情况
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                CloseTextractor();

                // TODO: 将大型字段设置为 null

                _disposedValue = true;
            }
        }

        ~TextHookHandle()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
