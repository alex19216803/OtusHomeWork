using System.Reflection;
using System.Text;

namespace OtusHomeWork7Serialize;

public static class ReflectionCsvSerializer
{
    private const char Delimiter = ';';

    private static readonly BindingFlags FieldFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly BindingFlags PropertyFlags =
        BindingFlags.Instance | BindingFlags.Public;

    public static string Serialize<T>(T obj) where T : notnull
    {
        var type = typeof(T);

        var fields = type.GetFields(FieldFlags);
        var properties = type.GetProperties(PropertyFlags)
                             .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                             .ToArray();

        var headers = new StringBuilder();
        var values = new StringBuilder();

        bool first = true;

        foreach (var field in fields)
        {
            if (!first) { headers.Append(Delimiter); values.Append(Delimiter); }
            headers.Append(field.Name);
            values.Append(field.GetValue(obj));
            first = false;
        }

        foreach (var prop in properties)
        {
            if (!first) { headers.Append(Delimiter); values.Append(Delimiter); }
            headers.Append(prop.Name);
            values.Append(prop.GetValue(obj));
            first = false;
        }

        return headers.ToString() + Environment.NewLine + values.ToString();
    }

    public static T Deserialize<T>(string csv) where T : new()
    {
        var lines = csv.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2)
            throw new FormatException("CSV must contain at least a header row and a value row.");

        var headers = lines[0].Split(Delimiter);
        var rawValues = lines[1].Split(Delimiter);

        if (headers.Length != rawValues.Length)
            throw new FormatException("Header and value row have different column counts.");

        var type = typeof(T);
        var instance = new T();

        var fieldMap = type.GetFields(FieldFlags).ToDictionary(f => f.Name);
        var propMap = type.GetProperties(PropertyFlags)
                          .Where(p => p.CanWrite && p.GetIndexParameters().Length == 0)
                          .ToDictionary(p => p.Name);

        for (int i = 0; i < headers.Length; i++)
        {
            var name = headers[i];
            var raw = rawValues[i];

            if (fieldMap.TryGetValue(name, out var field))
            {
                var converted = Convert.ChangeType(raw, field.FieldType);
                field.SetValue(instance, converted);
            }
            else if (propMap.TryGetValue(name, out var prop))
            {
                var converted = Convert.ChangeType(raw, prop.PropertyType);
                prop.SetValue(instance, converted);
            }
        }

        return instance;
    }
}
