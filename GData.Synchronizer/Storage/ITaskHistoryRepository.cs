
namespace GData.Synchronizer.Storage
{
    public interface ITaskHistoryRepository
    {
        void SaveHistory(TaskHistory taskHistory);
        TaskHistory GetHistory(string taskId, string uri);
    }
}
