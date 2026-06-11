using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace CastleDemo.Demos
{
    public class ReceivedWebhook
    {
        public int Id { get; set; }
        public string ReceivedAt { get; set; }
        public JToken Body { get; set; }
    }

    /// <summary>
    /// In-memory store of recently received webhooks. A real integration would
    /// persist these; for the demo, keeping the last few in memory is enough to
    /// show them on the page.
    /// </summary>
    public class WebhookStore
    {
        private const int MaxStored = 50;
        private readonly object _lock = new object();
        private readonly List<ReceivedWebhook> _webhooks = new List<ReceivedWebhook>();
        private int _nextId = 1;

        public void Add(JToken body)
        {
            lock (_lock)
            {
                _webhooks.Insert(0, new ReceivedWebhook
                {
                    Id = _nextId++,
                    ReceivedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Body = body
                });

                if (_webhooks.Count > MaxStored)
                {
                    _webhooks.RemoveRange(MaxStored, _webhooks.Count - MaxStored);
                }
            }
        }

        public IReadOnlyList<ReceivedWebhook> All()
        {
            lock (_lock)
            {
                return _webhooks.ToList();
            }
        }
    }
}
