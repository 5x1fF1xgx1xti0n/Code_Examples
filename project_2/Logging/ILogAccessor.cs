namespace TVC.ImageServer.API.Logging
{
    using System.Collections.Generic;
    using TVC.ImageServer.API.ApiModels.Views;

    public interface ILogAccessor
    {
        List<LogItem> GetLogs();
        byte[] Export();
    }
}
