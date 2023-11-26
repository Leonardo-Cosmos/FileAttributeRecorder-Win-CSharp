/* 2023/11/17 */

namespace FileInfoTool.Helpers
{
    internal class ProgressPrinter
    {
        private readonly string format;

        private volatile bool started = false;

        private volatile bool ended = false;

        private int lastProgressLength = 0;

        //private int startCursorTop = 0;

        public ProgressPrinter(string format)
        {
            this.format = format;
        }

        private static void MoveToHead()
        {
            //Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write("\r");
            //Console.SetCursorPosition(0, startCursorTop);
        }

        private static void Print(string value)
        {
            //Console.WriteLine(value);
            Console.Write(value);
        }

        private void EraseLine()
        {
            MoveToHead();
            var eraseText = new string(' ', Console.WindowWidth);
            Print(eraseText);
        }

        private string FormatProgress(params string[] progressValues)
        {
            var progress = string.Format(format, progressValues);
            if (progress.Length > Console.WindowWidth)
            {
                return progress[..Console.WindowWidth];
            }
            return progress;
        }

        public void Start(params string[] progressValues)
        {
            started = true;

            //(_, startCursorTop) = Console.GetCursorPosition();

            var progressText = FormatProgress(progressValues);
            Print(progressText);

            lastProgressLength = progressText.Length;
        }

        public void Update(params string[] progressValues)
        {
            if (ended)
            {
                return;
            }

            if (started)
            {
                var progressText = FormatProgress(progressValues);
                if (progressText.Length < lastProgressLength)
                {
                    EraseLine();
                }

                MoveToHead();
                Print(progressText);

                lastProgressLength = progressText.Length;
            }
            else
            {
                Start(progressValues);
            }
        }

        public void End()
        {
            ended = true;

            if (started)
            {
                EraseLine();
                MoveToHead();
            }
        }
    }
}
