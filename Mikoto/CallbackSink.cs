using Serilog.Core;
using Serilog.Events;

namespace Mikoto
{
    public class CallbackSink(Action<LogEvent> onEvent) : ILogEventSink
    {
        private readonly Action<LogEvent> _onEvent = onEvent;

        public void Emit(LogEvent logEvent)
        {
            _onEvent(logEvent);
        }
    }
}
