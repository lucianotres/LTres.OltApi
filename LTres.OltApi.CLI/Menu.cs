
namespace LTres.OltApi.CLI;

public class Menu
{
    public string? Description { get; set; }
    public List<MenuOption> Options { get; set; }

    public Menu(params MenuOption[] options)
    {
        Options = new List<MenuOption>(options);
    }

    public async Task<bool> Run()
    {
        while (true)
        {
            Console.WriteLine("");
            if (Description != null)
                Console.WriteLine(Description);

            foreach (var o in Options)
                Console.WriteLine($"{o.Option} - {o.Description}");

            Console.Write("Choose an option above: ");
            var read = Console.ReadLine();

            var choosed = (read ?? "").ToLower().FirstOrDefault('\0');
            var choosedOption = Options.FirstOrDefault(w => w.Option == choosed);
            if (choosedOption == null)
                Console.WriteLine("  -- no option choosed --");
            else
            {
                var optionResult = choosedOption.Action == null ? true : await choosedOption.Action();
                if (optionResult)
                    break;
            }
        }
        return false;
    }
}

public class MenuOption
{
    public char Option { get; set; } = '\0';
    public string Description { get; set; } = string.Empty;
    public Func<Task<bool>>? Action { get; set; }

    public MenuOption() { }

    public MenuOption(char option, string description)
    {
        Option = option;
        Description = description;
    }

    public MenuOption(char option, string description, Func<Task<bool>> action, bool catchError = false)
    {
        Option = option;
        Description = description;

        if (catchError)
            Action = () => CatchError(() => action());
        else
            Action = action;
    }

    private async Task<bool> CatchError(Func<Task<bool>> action)
    {
        try
        {
            return await action();
        }
        catch (Exception err)
        {
            var color = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write(err.Message);
            Console.BackgroundColor = color;
            Console.WriteLine();
            return false;
        }
    }
}