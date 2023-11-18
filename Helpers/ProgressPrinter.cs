/* 2023/11/17 */

namespace FileInfoTool.Helpers
{
    internal class ProgressPrinter
    {
        private readonly string format;

        private bool started = false;

        private int maxProgressLength = 0;

        public ProgressPrinter(string format)
        {
            this.format = format;
        }

        private static void Revert()
        {
            //Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write("\r");
        }

        private static void Print(string value)
        {
            //Console.WriteLine(value);
            Console.Write(value);
        }

        private string FormatProgress(string progress)
        {
            var formattedProgress = string.Format(format, progress);
            if (formattedProgress.Length > maxProgressLength)
            {
                maxProgressLength = formattedProgress.Length;
            }
            return formattedProgress;
        }

        public void Start(string progress)
        {
            started = true;
            Print(FormatProgress(progress));
        }

        public void Update(string progress)
        {
            if (started)
            {
                Revert();
                Print(FormatProgress(progress));
            }
            else
            {
                Start(progress);
            }
        }

        public void End()
        {
            if (started)
            {
                Revert();

                var eraseText = new string(' ', maxProgressLength * 2);
                Print(eraseText);

                Revert();
            }
        }
    }
}
