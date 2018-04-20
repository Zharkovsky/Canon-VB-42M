using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Canon_VB_M42
{
    public static class TaskExtensions
    {
        public static T Wait<T>(this Task<T> task, TimeSpan timeOut)
        {
            if (!task.Wait(timeOut))
            {
                try { task.Dispose(); }
                catch { }
                throw new TimeoutException($"Превышен тайм-аут {timeOut}");
            }
            return task.Result;
        }
    }
}
