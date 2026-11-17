using RetailSalesSystem.GUI;

namespace RetailSalesSystem;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Sua Data Source neu SQL Server tren may dung instance khac.
        string connectionString =
            "Data Source=.;Initial Catalog=SalesDB;Integrated Security=True;TrustServerCertificate=True";

        Application.Run(new MainForm(connectionString));
    }
}
