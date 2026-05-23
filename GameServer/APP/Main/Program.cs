using Fantasy;

try
{
    AssemblyHelper.Initialize();

    if (args.Length > 0 && string.Equals(args[0], "smoke", StringComparison.OrdinalIgnoreCase))
    {
        await FirstPlayableSmoke.Run();
        return;
    }

    await Fantasy.Platform.Net.Entry.Start();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Fatal error: {ex}");
    Environment.Exit(1);
}
