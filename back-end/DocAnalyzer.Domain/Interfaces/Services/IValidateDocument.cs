namespace DocAnalyzer.Domain.Interfaces.Services
{
    public interface IValidateDocument
    {
        void VerifyCreatorEmail(int idDocument,
                                string emailCreator);
    }
}
