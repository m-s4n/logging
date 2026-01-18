

using System.Collections.Concurrent;

namespace Otomasyon.DataAccess.Logs;

// hangi entity'ler audit'lenecek
public static class AuditEntityRegistry
{
    private static readonly ConcurrentDictionary<string, byte> _names = new();

    public static void Register<T>() => _names.TryAdd(typeof(T).Name, 0);

    public static bool IsAuditable(string entityName) => _names.ContainsKey(entityName);
}
