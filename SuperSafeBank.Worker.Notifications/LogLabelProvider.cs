using System.Collections.Generic;
using System.Linq;
using Serilog.Sinks.Loki.Labels;

namespace SuperSafeBank.Worker.Notifications
{
    public class LogLabelProvider : ILogLabelProvider
    {
        private readonly List<LokiLabel> _labels;

        public LogLabelProvider(IDictionary<string, string> labels)
        {
            _labels = (labels ?? new Dictionary<string, string>())
                .Select(kv => new LokiLabel(kv.Key, kv.Value))
                .ToList();
        }

        public IList<LokiLabel> GetLabels() => _labels;
    }
}