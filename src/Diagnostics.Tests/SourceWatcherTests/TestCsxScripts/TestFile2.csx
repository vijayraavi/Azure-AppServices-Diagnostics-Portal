private static string GetQuery(OperationContext cxt)
{
    return
    $@"Sample Kusto Query Here";
}

[Definition(Id = "testfile2", Name = "Test File 2", Description = "Test File 2")]
public async static Task<SignalResponse> Run(DataProviders dp, OperationContext cxt, SignalResponse res)
{
    res.Dataset.Add(new DiagnosticData()
    {
            Table = await dp.Kusto.ExecuteQuery(GetQuery(cxt), cxt.Resource.Stamp)
    });

    return res;
}