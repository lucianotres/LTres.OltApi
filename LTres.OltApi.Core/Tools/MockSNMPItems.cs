using Csv;

namespace LTres.OltApi.Core.Tools;

public class MockSNMPItems
{
    private Dictionary<string, MockSNMPItemsData> _items;

    public MockSNMPItems(string srcSrvFile)
    {
        _items = new Dictionary<string, MockSNMPItemsData>();
        _ = ReadCSVFile(srcSrvFile);
    }

    public async Task ReadCSVFile(string srcSrvFile)
    {
        _items.Clear();

        using FileStream fileStream = new(srcSrvFile, FileMode.Open, FileAccess.Read);
        var csvFileEnumerator = CsvReader.ReadFromStreamAsync(fileStream, new CsvOptions()
        {
            HeaderMode = HeaderMode.HeaderAbsent
        });

        await foreach (var line in csvFileEnumerator)
        {
            if (line.ColumnCount < 3 || !int.TryParse(line.Values[1], out int iType))
                continue;

            MockSNMPItemsData data = new();

            if (iType == 1)
                data.ValuesInt = line.Values.Skip(2).Select(s => int.Parse(s)).ToArray();
            else if (iType == 2)
                data.ValuesUInt = line.Values.Skip(2).Select(s => uint.Parse(s)).ToArray();
            else
            {
                iType = 0;
                data.ValuesStr = line.Values.Skip(2).ToArray();
            }

            data.Type = iType;
            data.ValueCount = line.ColumnCount - 2;

            var Oid = line.Values[0];
            if (Oid.StartsWith('.'))
                Oid = Oid.Substring(1);

            _items[Oid] = data;
        }
    }
    public MockSNMPItemsData? this[string oid]
    {
        get
        {
            if (oid.StartsWith('.'))
                oid = oid.Substring(1);

            if (_items.TryGetValue(oid, out var data))
                return data;
            else
                return null;
        }
    }

    public IEnumerable<KeyValuePair<string, MockSNMPItemsData>> StartWithOid(string oid)
    {
        if (oid.StartsWith('.'))
            oid = oid.Substring(1);

        return _items.Where(w => w.Key.StartsWith(oid)).ToList();
    }
}


public class MockSNMPItemsData
{
    public int Type { get; set; }
    public int ValueCount { get; set; } = 1;
    public int[]? ValuesInt { get; set; }
    public uint[]? ValuesUInt { get; set; }
    public string[]? ValuesStr { get; set; }

    public int? GetRandomValueInt()
    {
        if (ValuesInt == null)
            return null;

        var index = Random.Shared.Next(0, ValueCount - 1);
        return ValuesInt[index];
    }

    public uint? GetRandomValueUInt()
    {
        if (ValuesUInt == null)
            return null;

        var index = Random.Shared.Next(0, ValueCount - 1);
        return ValuesUInt[index];
    }

    public string? GetRandomValueStr()
    {
        if (ValuesStr == null)
            return null;

        var index = Random.Shared.Next(0, ValueCount - 1);
        return ValuesStr[index];
    }
}