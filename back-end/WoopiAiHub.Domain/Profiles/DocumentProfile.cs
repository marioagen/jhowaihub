using AutoMapper;
using WoopiAiHub.Application.Dto;
using WoopiAiHub.Domain.Enum;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Domain.Profiles
{
    public class DocumentProfile: AutoMapper.Profile
    {
        public DocumentProfile()
        {
            CreateMap<RequestCreateDocumentDto, Document>().ConstructUsing(i => new Document(
                i.Name,
                i.Description,
                Guid.NewGuid().ToString("N"),
                Enum.Status.NotAnalyzed,
                true,
                i.EmailCreator,
                0,
                DateTime.Now));
        }
    }
}
