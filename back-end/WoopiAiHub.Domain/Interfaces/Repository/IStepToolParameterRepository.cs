namespace WoopiAiHub.Domain.Interfaces.Repository
{
    public interface IStepToolParameterRepository
    {
        bool DeleteByIds(ICollection<int> ids);
        bool DeleteByStepToolsIds(ICollection<int> ids);
    }
}
