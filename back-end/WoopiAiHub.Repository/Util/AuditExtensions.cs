using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Util
{
    public static class AuditExtensions
    {
        public static bool ValidateEqualValues(object? currentValue, object? newValue)
        {
            if (currentValue == null && newValue == null)
                return true;

            if (currentValue == null || newValue == null)
                return false;

            if (currentValue is byte[] b1 && newValue is byte[] b2)
            {
                return b1.SequenceEqual(b2);
            }

            return Equals(currentValue, newValue);
        }

        public static bool ShouldSkipEntry(EntityEntry entry)
        {
            return entry.Entity is AuditLog ||
                   entry.State == EntityState.Detached ||
                   entry.State == EntityState.Unchanged;
        }

        public static AuditLog? CreateAuditLogFromEntry(EntityEntry entry, User user)
        {
            var tableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name;
            var actionType = entry.State.ToString();

            var changes = CollectChanges(entry);

            if (changes.Count == 0)
                return null;

            var actionJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                Operation = actionType,
                Changes = changes
            });

            return new AuditLog(0, DateTime.Now, tableName, actionJson, user.Id, user.Name);
        }

        public static Dictionary<string, object?> CollectChanges(EntityEntry entry)
        {
            var changes = new Dictionary<string, object?>();

            PropertyValues? databaseValues = null;
            if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                databaseValues = entry.GetDatabaseValues();
            }

            bool isManyToManyInternal = entry.Metadata.ClrType == typeof(Dictionary<string, object>)
                || entry.Metadata.ClrType == null;

            if (isManyToManyInternal)
            {
                CollectManyToManyChanges(entry, changes);
            }
            else
            {
                CollectEntityChanges(entry, changes, databaseValues);
            }

            return changes;
        }

        public static void CollectManyToManyChanges(EntityEntry entry, Dictionary<string, object?> changes)
        {
            foreach (var prop in entry.Properties)
            {
                changes[prop.Metadata.Name] = prop.CurrentValue;
            }
        }

        public static void CollectEntityChanges(EntityEntry entry, Dictionary<string, object?> changes, PropertyValues? databaseValues)
        {
            foreach (var property in entry.Properties)
            {
                if (entry.State == EntityState.Added && property.Metadata.IsPrimaryKey())
                    continue;

                string propertyName = property.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        changes[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        changes[propertyName] = databaseValues?[propertyName];
                        break;

                    case EntityState.Modified:
                        CollectModifiedPropertyChange(property, propertyName, databaseValues, changes);
                        break;
                }
            }
        }

        public static void CollectModifiedPropertyChange(
            PropertyEntry property,
            string propertyName,
            PropertyValues? databaseValues,
            Dictionary<string, object?> changes)
        {
            if (!property.IsModified)
                return;

            var oldValue = databaseValues?[propertyName];
            var newValue = property.CurrentValue;

            if (!ValidateEqualValues(oldValue, newValue))
            {
                changes[propertyName] = new
                {
                    Old = oldValue,
                    New = newValue
                };
            }
        }
    }
}
