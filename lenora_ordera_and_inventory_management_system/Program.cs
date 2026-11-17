using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Data;
using lenora_ordera_and_inventory_management_system.Forms;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var users = new UserRepository();
        var auth = new AuthService(users);
        if (!DatabaseIsReady(auth))
        {
            return;
        }

        var accounts = new UserService(users);
        var products = new ProductRepository();
        var productService = new ProductService(products);
        var wholesalers = new WholesalerRepository();
        var wholesalerService = new WholesalerService(wholesalers);
        var stockOrders = new StockOrderRepository();
        var stockService = new StockService(stockOrders, products);
        var customers = new CustomerRepository();
        var orders = new OrderRepository();
        var orderService = new OrderService(orders, customers, products);
        var deliveryService = new DeliveryService(orders, products);
        var paymentService = new PaymentService(orders);
        var exportService = new ExportService(products, orders);

        RunUntilExit(
            auth,
            accounts,
            productService,
            wholesalerService,
            stockService,
            orderService,
            deliveryService,
            paymentService,
            exportService);
    }

    private static bool DatabaseIsReady(AuthService auth)
    {
        var startup = auth.CheckDatabase();
        if (startup.Success)
        {
            return true;
        }

        MessageBox.Show(startup.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
    }

    private static void RunUntilExit(
        AuthService auth,
        UserService accounts,
        ProductService products,
        WholesalerService wholesalers,
        StockService stock,
        OrderService orders,
        DeliveryService delivery,
        PaymentService payments,
        ExportService export)
    {
        while (true)
        {
            using var login = new LoginForm(auth);
            if (login.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            using var main = new MainForm(
                auth,
                accounts,
                products,
                wholesalers,
                stock,
                orders,
                delivery,
                payments,
                export);
            main.ShowDialog();
        }
    }
}
