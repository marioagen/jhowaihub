using WoopiAiHub.Domain.Enum;

namespace WoopiAiHub.Repository.Util.Records
{
    public record CardProjection(
         int Id,
         string Name,
         DateTime Created,
         int? DocumentBatchId,
         int StepId,
         int DocumentId,
         string DocumentDescription,
         string DocumentOwner,
         DocumentStatus DocumentStatus,
         Guid? AssignedUserId,
         string? AssignedUserName,
         string? AssignedUserEmail,
         DateTime? AssignedUserCreated,
         int StatusId,
         string StatusName,
         string StatusColor);
}
