using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using WoopiAiHub.Domain.Models;

namespace WoopiAiHub.Repository.Util
{
    public static class AuditExtensions
    {
        /// <summary>
        /// Determines whether two values are equal, with special handling for byte arrays.
        /// </summary>
        /// <remarks>This method performs a deep comparison for byte arrays, checking each element for
        /// equality. For all other types, it uses the default equality comparison. Use this method when you need to
        /// compare values that may be byte arrays or other object types.</remarks>
        /// <param name="currentValue">The first value to compare. Can be null or any object.</param>
        /// <param name="newValue">The second value to compare. Can be null or any object.</param>
        /// <returns>true if both values are equal or both are null; otherwise, false. For byte arrays, returns true if the
        /// arrays have the same length and contents.</returns>
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

        /// <summary>
        /// Determines whether the specified entity entry should be skipped during processing based on its type or
        /// state.
        /// </summary>
        /// <param name="entry">The entity entry to evaluate for skipping. Cannot be null.</param>
        /// <returns>true if the entry represents an AuditLog entity, or if its state is Detached or Unchanged; otherwise, false.</returns>
        public static bool ShouldSkipEntry(EntityEntry entry)
        {
            return entry.Entity is AuditLog ||
                   entry.State == EntityState.Detached ||
                   entry.State == EntityState.Unchanged;
        }

        /// <summary>
        /// Creates an audit log entry representing the changes made to the specified entity entry by the given user.
        /// </summary>
        /// <remarks>The audit log includes the table name, the type of operation performed, and a
        /// serialized record of the changes. If no changes are detected in the entity entry, the method returns <see
        /// langword="null"/>.</remarks>
        /// <param name="entry">The entity entry containing the changes to be audited. Must not be null.</param>
        /// <param name="user">The user who performed the operation. Must not be null.</param>
        /// <returns>An <see cref="AuditLog"/> instance containing details of the changes if any changes are detected; otherwise,
        /// <see langword="null"/>.</returns>
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

        /// <summary>
        /// Collects the property values that have changed for the specified entity entry.
        /// </summary>
        /// <remarks>This method supports both regular entity entries and many-to-many join entities. For
        /// modified or deleted entities, the method compares current values to the database values to determine
        /// changes.</remarks>
        /// <param name="entry">The entity entry to inspect for changes. Must not be null.</param>
        /// <returns>A dictionary containing the names and new values of properties that have changed. The dictionary is empty if
        /// no changes are detected.</returns>
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

        /// <summary>
        /// Collects the current values of all properties in the specified entity entry and adds them to the provided
        /// changes dictionary.
        /// </summary>
        /// <param name="entry">The entity entry whose property values are to be collected.</param>
        /// <param name="changes">A dictionary to which the property names and their current values will be added. Existing entries with the
        /// same property names will be overwritten.</param>
        public static void CollectManyToManyChanges(EntityEntry entry, Dictionary<string, object?> changes)
        {
            foreach (var prop in entry.Properties)
            {
                changes[prop.Metadata.Name] = prop.CurrentValue;
            }
        }

        /// <summary>
        /// Populates a dictionary with the changes to entity properties based on the specified entity state.
        /// </summary>
        /// <remarks>For added entities, only non-primary key properties are included. For deleted
        /// entities, values are taken from the database. For modified entities, only properties that have changed are
        /// included. The method does not clear the dictionary before adding changes; callers should ensure the
        /// dictionary is empty if required.</remarks>
        /// <param name="entry">The entity entry representing the tracked entity and its current state.</param>
        /// <param name="changes">A dictionary to be populated with property names and their corresponding changed values.</param>
        /// <param name="databaseValues">The original property values from the database, or null if not available. Used to determine previous values
        /// for deleted or modified entities.</param>
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

        /// <summary>
        /// Examines a property entry and, if the property has been modified, records the original and current values in
        /// the specified changes dictionary.
        /// </summary>
        /// <remarks>This method only adds an entry to the changes dictionary if the property is marked as
        /// modified and its current value differs from the original value in the database. The changes dictionary is
        /// updated in place and may be empty if no modifications are detected.</remarks>
        /// <param name="property">The property entry to evaluate for modifications.</param>
        /// <param name="propertyName">The name of the property to check and use as the key in the changes dictionary.</param>
        /// <param name="databaseValues">An object containing the original values of properties as retrieved from the database, or null if
        /// unavailable.</param>
        /// <param name="changes">A dictionary to which the method adds an entry for the property if it has been modified. The entry contains
        /// the old and new values.</param>
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
