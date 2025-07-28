using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class UserDtoPaged
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime Created { get; set; }
        public IEnumerable<TeamDto> Teams { get; set; } = Enumerable.Empty<TeamDto>();
        public IEnumerable<ProfileDto> Profiles { get; set; } = Enumerable.Empty<ProfileDto>();
    }
}
