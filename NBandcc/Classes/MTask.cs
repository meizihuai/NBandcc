using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBandcc
{
    public class MTask
    {
        private TaskFlag Flag { get; set; }
        private Task Task { get; set; }

        public MTask()
        {
            Flag = new TaskFlag();
        }
        public MTask(Action action)
        {
            Flag = new TaskFlag();
            Task = new Task(action, Flag.Token);
        }
        public void SetAction(Action action)
        {
            Task = new Task(action, Flag.Token);
        }
        public void Start()
        {
            if (Task == null) return;
            Task.Start();
        }
        public void Cancel()
        {
            if (Flag == null) return;
            Flag.Cancel();
        }
        public bool IsCancelled()
        {
            if (Flag == null) return false;
            return this.Flag.IsCancelled();
        }

        public class TaskFlag
        {
            private CancellationTokenSource tokenSource;
            public CancellationToken Token { get; set; }
            public TaskFlag()
            {
                tokenSource = new CancellationTokenSource();
                Token = tokenSource.Token;
            }
            public void Cancel()
            {
                tokenSource.Cancel();
            }
            public bool IsCancelled()
            {
                return Token.IsCancellationRequested;
            }
        }
    }
}
