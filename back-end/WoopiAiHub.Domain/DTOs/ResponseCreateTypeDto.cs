using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs
{
    public record class ResponseCreateTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public string EmailCreator { get; set; } = string.Empty;
        public bool Duplicated { get; set; } = false;
    }
}