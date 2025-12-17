using System;
using System.Collections.Generic;
using System.Text;

namespace Mikoto.Helpers
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this Task task)
            => _ = task.ConfigureAwait(false);  
    }
}
