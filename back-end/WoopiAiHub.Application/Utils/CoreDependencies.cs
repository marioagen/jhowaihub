using AutoMapper;
using WoopiAiHub.Domain.Interfaces.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace WoopiAiHub.Application.Utils
{
    public class CoreDependencies : ICoreDependencies
    {
        public IConfiguration Configuration { get; }
        public IHttpContextAccessor HttpContextAccessor { get; }
        public ICurrentUserService CurrentUserService { get; }
        public IMapper Mapper { get; }

        public CoreDependencies(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            Configuration = configuration;
            HttpContextAccessor = httpContextAccessor;
            CurrentUserService = currentUserService;
            Mapper = mapper;
        }
    }
}
