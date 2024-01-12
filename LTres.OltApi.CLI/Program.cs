using LTres.OltApi.CLI;

Console.WriteLine("-- LTres OLT API CLI");
Console.WriteLine("   for testing purporses");
Console.WriteLine("");

await new Menu(
    new MenuOption('1', "SNMP worker tests", async () => await new MenuSNMP().Run()),
    new MenuOption('2', "Calc expressions tests", async () => await new MenuCalc().Run()),
    new MenuOption('3', "Communication tests", async () => await new MenuCommunication().Run(), true),
    new MenuOption('q', "to quit")
).Run();