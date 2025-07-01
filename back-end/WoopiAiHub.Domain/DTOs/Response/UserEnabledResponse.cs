using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoopiAiHub.Domain.DTOs.Response
{
    public class UserEnabledResponse
    {
        [Column("Email", TypeName = "varchar(150)")]
        public string Email { get; set; } = string.Empty;

        [Column("Owner", TypeName = "bit")]
        public bool Owner { get; set; }

        [Column("Id_BuyerCompany", TypeName = "int")]
        public int BuyerCompanyId { get; set; }

        [Column("Reference_User", TypeName = "varchar(100)")]
        public string? ReferenceUser { get; set; } = string.Empty;
    }
}
