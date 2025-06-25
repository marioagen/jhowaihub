using AutoMapper;
using DocAnalyzer.Domain.Interfaces.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace DocAnalyzer.Application.Utils
{
    public class CoreDependencies : ICoreDependencies
    {
        public IConfiguration Configuration { get; }
        public IHttpContextAccessor HttpContextAccessor { get; }
        public IMapper Mapper { get; }

        public CoreDependencies(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            Configuration = configuration;
            HttpContextAccessor = httpContextAccessor;
            Mapper = mapper;
        }
    }
}
