using System.Collections.Generic;
using System.Threading.Tasks;
using Tsd.Tabulator.Core.Models;

namespace Tsd.Tabulator.Core.Services;

public interface IClassConfigService
{
    /// <summary>
    /// Ensure event DB has class definition/alias tables and seed missing rows from the master global DB.
    /// </summary>
    Task SeedEventFromGlobalAsync(string eventDbPath);

    /// <summary>
    /// Upsert a class definition into the event DB and optionally the global master DB.
    /// Always writes to the event DB to keep event self-contained.
    /// </summary>
    Task UpsertClassDefinitionAsync(ClassDefinition def, string eventDbPath, bool saveGlobally = true);

    /// <summary>
    /// Upsert an alias mapping into the event DB and optionally the global master DB.
    /// Always writes to the event DB to keep event self-contained.
    /// </summary>
    Task UpsertAliasAsync(string alias, string classKey, string eventDbPath, bool saveGlobally = true);

    /// <summary>
    /// Delete a class definition and all its associated aliases from the event DB and optionally the global master DB.
    /// </summary>
    Task DeleteClassDefinitionAsync(string classKey, string eventDbPath, bool deleteGlobally = true);

    /// <summary>
    /// Delete a specific alias from the event DB and optionally the global master DB.
    /// </summary>
    Task DeleteAliasAsync(string alias, string eventDbPath, bool deleteGlobally = true);

    /// <summary>
    /// Returns distinct routine ClassText values that do not resolve via event aliases or definitions.
    /// </summary>
    Task<IEnumerable<string>> GetUnmappedClassesAsync(string eventDbPath);

    /// <summary>
    /// Get all class definitions from the event DB (snapshot).
    /// </summary>
    Task<IEnumerable<ClassDefinition>> GetClassDefinitionsAsync(string eventDbPath);

    /// <summary>
    /// Get all aliases from the event DB (snapshot).
    /// </summary>
    Task<IEnumerable<ClassAlias>> GetAliasesAsync(string eventDbPath);

    /// <summary>
    /// Resolve a routine class text to a ClassKey using event aliases first, then class keys.
    /// Returns null if nothing resolves.
    /// </summary>
    Task<string?> ResolveClassKeyAsync(string? classText, string eventDbPath);
}