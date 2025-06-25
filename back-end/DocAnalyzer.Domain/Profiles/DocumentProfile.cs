using AutoMapper;
using DocAnalyzer.Application.Dto;
using DocAnalyzer.Domain.Enum;
using DocAnalyzer.Domain.Models;

namespace DocAnalyzer.Domain.Profiles
{
    public class DocumentProfile:Profile
    {
        public DocumentProfile()
        {
            CreateMap<RequestCreateDocumentDto, Document>().ConstructUsing(i => new Document(
                i.Name,
                i.Description,
                Guid.NewGuid().ToString("N"),
                Status.NotAnalyzed,
                true,
                i.EmailCreator,
                0,
                DateTime.Now));
        }
    }
}
