using Csv;

namespace LTres.Olt.Api.Core.Tools;

public class MockSNMPItems
{
    private Dictionary<string, MockSNMPItemsData> _items;

    public MockSNMPItems(string? srcSrvFile)
    {
        _items = new Dictionary<string, MockSNMPItemsData>();

        if (srcSrvFile == null)
            CreateFixedMockItems();
        else
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

            var Oid = line.Values[0];
            if (Oid.StartsWith('.'))
                Oid = Oid.Substring(1);

            _items[Oid] = data;
        }
    }

    private void CreateFixedMockItems()
    {
        _items.Clear();

        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.1", new MockSNMPItemsData() { ValuesStr = ["ONU NUMBER 1"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.2", new MockSNMPItemsData() { ValuesStr = ["ONU NUMBER 2"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.3", new MockSNMPItemsData() { ValuesStr = ["ONU NUMBER 3"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.4", new MockSNMPItemsData() { ValuesStr = ["ONU NUMBER 4"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.5", new MockSNMPItemsData() { ValuesStr = ["ONU NUMBER 5"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.6", new MockSNMPItemsData() { ValuesStr = ["ONU NUMBER 6"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.7", new MockSNMPItemsData() { ValuesStr = ["ONU NUMBER 7"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.8", new MockSNMPItemsData() { ValuesStr = ["ONU NUMBER 8"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.2.268501248.9", new MockSNMPItemsData() { ValuesStr = ["ONU NUMBER 9"] });

        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.5.268501248.1", new MockSNMPItemsData() { ValuesStr = ["49 54 42 53 5F 7B 3F CD"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.5.268501248.2", new MockSNMPItemsData() { ValuesStr = ["49 54 42 53 5F 7B 43 CD"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.5.268501248.3", new MockSNMPItemsData() { ValuesStr = ["49 54 42 53 5F 7B 5F CD"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.5.268501248.4", new MockSNMPItemsData() { ValuesStr = ["49 54 42 53 5F 7B 1C CD"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.5.268501248.5", new MockSNMPItemsData() { ValuesStr = ["49 54 42 53 5F 7B CC CD"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.5.268501248.6", new MockSNMPItemsData() { ValuesStr = ["49 54 42 53 5F 7B AB CD"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.5.268501248.7", new MockSNMPItemsData() { ValuesStr = ["49 54 42 53 5F 7B 18 CD"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.5.268501248.8", new MockSNMPItemsData() { ValuesStr = ["49 54 42 53 5F 7B 1F CD"] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.1.1.5.268501248.9", new MockSNMPItemsData() { ValuesStr = ["49 54 42 53 5F 7B C1 CD"] });


        _items.Add("1.3.6.1.4.1.3902.1012.3.28.2.1.4.268501248.1", new MockSNMPItemsData() { Type = 1, ValuesInt = [3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 1] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.2.1.4.268501248.2", new MockSNMPItemsData() { Type = 1, ValuesInt = [3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 1] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.2.1.4.268501248.3", new MockSNMPItemsData() { Type = 1, ValuesInt = [3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 1] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.2.1.4.268501248.4", new MockSNMPItemsData() { Type = 1, ValuesInt = [3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 1] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.2.1.4.268501248.5", new MockSNMPItemsData() { Type = 1, ValuesInt = [3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 1] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.2.1.4.268501248.6", new MockSNMPItemsData() { Type = 1, ValuesInt = [3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 1] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.2.1.4.268501248.7", new MockSNMPItemsData() { Type = 1, ValuesInt = [3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 1] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.2.1.4.268501248.8", new MockSNMPItemsData() { Type = 1, ValuesInt = [3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 1] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.28.2.1.4.268501248.9", new MockSNMPItemsData() { Type = 1, ValuesInt = [3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 4, 1] });

        _items.Add("1.3.6.1.4.1.3902.1012.3.50.12.1.1.10.268501248.1.1", new MockSNMPItemsData() { Type = 1, ValuesInt = [3217, 2074, 2725, 2159, 2235, 2657] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.50.12.1.1.10.268501248.2.1", new MockSNMPItemsData() { Type = 1, ValuesInt = [3217, 2074, 2725, 2159, 2235, 2657] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.50.12.1.1.10.268501248.3.1", new MockSNMPItemsData() { Type = 1, ValuesInt = [3217, 2074, 2725, 2159, 2235, 2657] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.50.12.1.1.10.268501248.4.1", new MockSNMPItemsData() { Type = 1, ValuesInt = [3217, 2074, 2725, 2159, 2235, 2657] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.50.12.1.1.10.268501248.5.1", new MockSNMPItemsData() { Type = 1, ValuesInt = [3217, 2074, 2725, 2159, 2235, 2657] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.50.12.1.1.10.268501248.6.1", new MockSNMPItemsData() { Type = 1, ValuesInt = [3217, 2074, 2725, 2159, 2235, 2657] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.50.12.1.1.10.268501248.7.1", new MockSNMPItemsData() { Type = 1, ValuesInt = [3217, 2074, 2725, 2159, 2235, 2657] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.50.12.1.1.10.268501248.8.1", new MockSNMPItemsData() { Type = 1, ValuesInt = [3217, 2074, 2725, 2159, 2235, 2657] });
        _items.Add("1.3.6.1.4.1.3902.1012.3.50.12.1.1.10.268501248.9.1", new MockSNMPItemsData() { Type = 1, ValuesInt = [3217, 2074, 2725, 2159, 2235, 2657] });

        _items.Add("1.3.6.1.4.1.3902.1012.3.28.2.1.5.268501248.1", new MockSNMPItemsData() { Type = 2, ValuesUInt = [3, 4, 1] });
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
    public int[]? ValuesInt { get; set; }
    public uint[]? ValuesUInt { get; set; }
    public string[]? ValuesStr { get; set; }

    public int ValueCount
    {
        get
        {
            if (Type == 1)
                return ValuesInt?.Length ?? 1;
            else if (Type == 2)
                return ValuesUInt?.Length ?? 1;
            else
                return ValuesStr?.Length ?? 1;
        }
    }

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
