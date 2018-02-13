private static string GetQuery(OperationContext cxt)
{
    return
    $@"Sample Kusto Query Here";
}

[Definition(Id = "testfile1", Name = "Test File 1", Description = "Test File 1")]
public async static Task<SignalResponse> Run(DataProviders dp, OperationContext cxt, SignalResponse res)
{
    res.Dataset.Add(new DiagnosticData()
    {
            Table = await dp.Kusto.ExecuteQuery(GetQuery(cxt), cxt.Resource.Stamp)
    });

    return res;
}