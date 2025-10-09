using System.ComponentModel.DataAnnotations.Schema;

namespace WoopiAiHub.Domain.Models
{
    public class StepProfilePermission
    {
        public StepProfilePermission(int stepId, int profileId, int permissionId)
        {
            StepId = stepId;
            ProfileId = profileId;
            PermissionId = permissionId;
        }

        public int StepId { get; set; }
        public int ProfileId { get; set; }
        public int PermissionId { get; set; }

        public virtual Step? Step { get; set; }
        public virtual Profile? Profile { get; set; }
        public virtual Permission? Permission { get; set; }
    }
}
