using NLog;
using NLog.Config;
using NLog.Targets;
using NUnit.Framework;

namespace Tests.Util
{
    public class LoggedTest
    {
        public LoggedTest()
        {
            var config = new LoggingConfiguration();
            var target = new XUnitTarget();
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, target);
            LogManager.Configuration = config;
        }
    }
    public sealed class XUnitTarget : TargetWithLayout
    {
        public XUnitTarget()
        {
            Layout = NLog.Layouts.Layout.FromString("${time}|${level:uppercase=true}|${logger:shortName=true}|${message}");
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var message = Layout.Render(logEvent);
            TestContext.Out.WriteLine(message);
        }
    }
}
