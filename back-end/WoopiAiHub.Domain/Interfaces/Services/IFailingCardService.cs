namespace WoopiAiHub.Domain.Interfaces.Services
{
    public interface IFailingCardService
    {
        Task SetFailingCard(int cardId, string? email);
    }
}
