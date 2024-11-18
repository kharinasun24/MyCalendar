namespace MyCalendar.logger
{



    public sealed class SimpleLogger
    {
        private static readonly Lazy<SimpleLogger> _instance = new Lazy<SimpleLogger>(() => new SimpleLogger());
        private readonly StreamWriter _writer;

        private SimpleLogger()
        {
            _writer = new StreamWriter("log.log", false); // true zum anhängen
        }

        public static SimpleLogger Instance => _instance.Value;

        public void Log(string message)
        {
            _writer.WriteLine($"{DateTime.Now}: {message}");
            _writer.Flush();
        }
    }
}
