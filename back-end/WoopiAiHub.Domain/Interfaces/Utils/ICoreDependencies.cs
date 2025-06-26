using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace WoopiAiHub.Domain.Interfaces.Utils
{
    public interface ICoreDependencies
    {
        IConfiguration Configuration { get; }
        IHttpContextAccessor HttpContextAccessor { get; }
        IMapper Mapper { get; }
    }
}
