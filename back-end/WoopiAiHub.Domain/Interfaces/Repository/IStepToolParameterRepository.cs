namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolParameterRepository
    {
        bool DeleteByIds(IEnumerable<int> ids);
        bool DeleteByStepToolsIds(ICollection<int> ids);
        string FindByStepToolId(int stepToolId);
    }
}
