namespace TVC.ImageServer.API.Logging
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using Serilog.Sinks.AzureTableStorage;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using TVC.ImageServer.API.ApiModels.Views;

    public class LogAccessor : ILogAccessor
    {
        private readonly CloudStorageAccount _account;
        private readonly CloudTable _table;

        public LogAccessor()
        {
            _account = Authenticate();
            _table = GetTable();
        }

        public List<LogItem> GetLogs()
        {            
            var condition = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, DateTime.Now.AddHours(-DateTime.Now.Hour));
            
            var query = new TableQuery<LogEventEntity>().Where(condition);

            List<LogEventEntity> result = _table.ExecuteQuerySegmentedAsync<LogEventEntity>(query, null)
                                                .Result
                                                .OrderBy(c => c.Timestamp)
                                                .ToList();

            if (result.Count < 500)
            {
                query = new TableQuery<LogEventEntity>().Take(500);

                result = _table.ExecuteQuerySegmentedAsync<LogEventEntity>(query, null)
                                                    .Result
                                                    .OrderBy(c => c.Timestamp)
                                                    .ToList();
            }

            List<LogItem> logs = new List<LogItem>();

            foreach(var item in result)
            {
                var logItem = new LogItem() { Log = item.Timestamp + " " + item.RenderedMessage };
                if (item.Exception != "" && item.Exception != null)
                {
                    logItem.Log += " " + item.Exception.Split(".")[0] + item.Exception.Split(".")[1] + ".";
                }
                logs.Add(logItem);
            }

            logs.Reverse();
            return logs;
        }

        public byte[] Export()
        {
            var logs = GetLogs();
            byte[] bytes = { };

            UnicodeEncoding uniEncoding = new UnicodeEncoding();
            using (MemoryStream ms = new MemoryStream())
            {
                var writer = new StreamWriter(ms, uniEncoding);
                try
                {
                    foreach (var log in logs)
                    {
                        writer.Write(log.Log);
                    }
                    writer.Flush();
                    ms.Seek(0, SeekOrigin.Begin);

                    bytes = ms.ToArray();
                }
                finally
                {
                    writer.Dispose();
                }
            }

            return bytes;
        }

        private CloudStorageAccount Authenticate()
        {
            CloudStorageAccount account;
            CloudStorageAccount.TryParse("", out account);
            return account;
        }

        private CloudTable GetTable()
        {
            CloudTableClient client = _account.CreateCloudTableClient();

            CloudTable table = client.GetTableReference("logs");
            table.CreateIfNotExistsAsync();

            return table;
        }

    }
}
