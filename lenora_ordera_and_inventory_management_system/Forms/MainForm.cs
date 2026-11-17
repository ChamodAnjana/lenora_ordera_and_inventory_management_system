using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class MainForm : Form
{
    private const int FormWidth = 1100;
    private const int FormHeight = 700;
    private const int SidebarWidth = 230;
    private const int NavBtnHeight = 40;

    private readonly AuthService _auth;
    private readonly UserService _accounts;
    private readonly ProductService _products;
    private readonly WholesalerService _wholesalers;
    private readonly StockService _stock;
    private readonly OrderService _orders;
    private readonly DeliveryService _delivery;
    private readonly PaymentService _payments;
    private readonly ExportService _export;

    private readonly Panel pnlSidebar = new();
    private readonly Panel pnlWorkspace = new();
    private readonly Panel pnlBrand = new();
    private readonly Label lblBrandTitle = new();
    private readonly Label lblBrandSub = new();
    private readonly Panel pnlAccentLine = new();
    private readonly Panel pnlUserCard = new();
    private readonly Label lblUserName = new();
    private readonly Label lblUserRole = new();
    private readonly Label lblSparkle = new();

    private readonly Button btnOrders = new();
    private readonly Button btnDelivery = new();
    private readonly Button btnProducts = new();
    private readonly Button btnPayments = new();
    private readonly Button btnWholesalers = new();
    private readonly Button btnStockOrders = new();
    private readonly Button btnAccounts = new();
    private readonly Button btnExport = new();
    private readonly Button btnLogout = new();

    private readonly Panel pnlGreetingCard = new();
    private readonly Label lblGreeting = new();
    private readonly Label lblGreetingSub = new();
    private readonly Button btnQuickOrder = new();
    private readonly Button btnRefresh = new();

    private readonly TableLayoutPanel tlpMetrics = new();
    private readonly Label lblPlacedValue = new();
    private readonly Label lblPlacedSub = new();
    private readonly Label lblPostedValue = new();
    private readonly Label lblPostedSub = new();
    private readonly Label lblStockValue = new();
    private readonly Label lblStockSub = new();
    private readonly Label lblPaymentValue = new();
    private readonly Label lblPaymentSub = new();

    private readonly Panel pnlRecentCard = new();
    private readonly Panel pnlRecentHeader = new();
    private readonly Label lblRecentTitle = new();
    private readonly Label lblRecentFilter = new();
    private readonly DataGridView dgvRecent = new();
    private bool _passwordPrompted;

    public MainForm(
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
        _auth = auth;
        _accounts = accounts;
        _products = products;
        _wholesalers = wholesalers;
        _stock = stock;
        _orders = orders;
        _delivery = delivery;
        _payments = payments;
        _export = export;

        BuildChrome();
        BuildSidebar();
        BuildWorkspace();
        pnlSidebar.SendToBack();
        pnlWorkspace.BringToFront();
        RefreshDashboardMetrics();
    }

    private void BuildChrome()
    {
        Text = "♥ Lenora · Orders and Inventory Management";
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(950, 620);
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(FormWidth, FormHeight);
        BackColor = Palette.Paper;
        CancelButton = btnLogout;
    }

    private void BuildSidebar()
    {
        pnlSidebar.Width = SidebarWidth;
        pnlSidebar.Dock = DockStyle.Left;
        pnlSidebar.BackColor = Palette.Ink;
        Controls.Add(pnlSidebar);

        BuildBrandHeader();
        BuildUserCard();
        BuildNavMenu();
        BuildLogoutButton();
    }

    private void BuildBrandHeader()
    {
        pnlBrand.Location = new Point(0, 0);
        pnlBrand.Size = new Size(SidebarWidth, 75);
        pnlSidebar.Controls.Add(pnlBrand);

        lblBrandTitle.Text = "lenora.lk";
        lblBrandTitle.Font = new Font("Segoe Script", 15F, FontStyle.Bold);
        lblBrandTitle.ForeColor = Palette.AccentPink;
        lblBrandTitle.Location = new Point(14, 12);
        lblBrandTitle.AutoSize = true;
        pnlBrand.Controls.Add(lblBrandTitle);

        lblBrandSub.Text = "BOUTIQUE";
        lblBrandSub.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        lblBrandSub.ForeColor = Palette.Tagline;
        lblBrandSub.Location = new Point(132, 22);
        lblBrandSub.AutoSize = true;
        pnlBrand.Controls.Add(lblBrandSub);

        pnlAccentLine.BackColor = Palette.Accent;
        pnlAccentLine.Location = new Point(18, 48);
        pnlAccentLine.Size = new Size(36, 2);
        pnlBrand.Controls.Add(pnlAccentLine);
    }

    private void BuildUserCard()
    {
        pnlUserCard.Location = new Point(14, 76);
        pnlUserCard.Size = new Size(SidebarWidth - 28, 54);
        pnlUserCard.BackColor = Palette.InkLight;
        pnlSidebar.Controls.Add(pnlUserCard);

        var user = _auth.CurrentUser();
        lblUserName.Text = user?.FullName ?? "Lenora Owner";
        lblUserName.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblUserName.ForeColor = Palette.White;
        lblUserName.Location = new Point(12, 8);
        lblUserName.Size = new Size(SidebarWidth - 65, 18);
        pnlUserCard.Controls.Add(lblUserName);

        lblUserRole.Text = (user?.Role.ToString() ?? "Owner").ToUpperInvariant();
        lblUserRole.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
        lblUserRole.ForeColor = Palette.AccentPink;
        lblUserRole.Location = new Point(12, 28);
        lblUserRole.AutoSize = true;
        pnlUserCard.Controls.Add(lblUserRole);

        lblSparkle.Text = "✨";
        lblSparkle.Font = new Font("Segoe UI", 11F);
        lblSparkle.ForeColor = Palette.AccentPink;
        lblSparkle.Location = new Point(SidebarWidth - 60, 16);
        lblSparkle.AutoSize = true;
        pnlUserCard.Controls.Add(lblSparkle);
    }

    private void BuildNavMenu()
    {
        var top = 144;
        top = StyleActiveNav(btnOrders, "📦  Customer Orders", top, OrdersClicked);
        top = AddNavItem(btnDelivery, "🚚  Delivery & Tracking", top, DeliveryClicked);
        top = AddNavItem(btnProducts, "🏷️  Products & Stock", top, ProductsClicked);

        if (!SignedInOwner())
        {
            return;
        }

        top = AddNavItem(btnPayments, "💳  COD Payments", top, PaymentsClicked);
        top = AddNavItem(btnWholesalers, "🏢  Wholesalers", top, WholesalersClicked);
        top = AddNavItem(btnStockOrders, "📥  Stock Orders", top, StockOrdersClicked);
        top = AddNavItem(btnAccounts, "👥  Accounts", top, AccountsClicked);
        AddNavItem(btnExport, "📤  Export Data", top, ExportClicked);
    }

    private int StyleActiveNav(Button btn, string text, int top, EventHandler onClick)
    {
        btn.Text = text;
        btn.UseMnemonic = false;
        btn.Location = new Point(14, top);
        btn.Size = new Size(SidebarWidth - 28, NavBtnHeight);
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = Palette.Accent;
        btn.ForeColor = Palette.White;
        btn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btn.TextAlign = ContentAlignment.MiddleLeft;
        btn.Padding = new Padding(12, 0, 0, 0);
        btn.Cursor = Cursors.Hand;
        btn.Click += onClick;
        pnlSidebar.Controls.Add(btn);
        return top + NavBtnHeight + 6;
    }

    private int AddNavItem(Button btn, string text, int top, EventHandler onClick)
    {
        btn.Text = text;
        btn.UseMnemonic = false;
        btn.Location = new Point(14, top);
        btn.Size = new Size(SidebarWidth - 28, NavBtnHeight);
        btn.FlatStyle = FlatStyle.Flat;
        btn.FlatAppearance.BorderSize = 0;
        btn.BackColor = Palette.Ink;
        btn.ForeColor = Palette.Tagline;
        btn.Font = new Font("Segoe UI", 9.5F);
        btn.TextAlign = ContentAlignment.MiddleLeft;
        btn.Padding = new Padding(12, 0, 0, 0);
        btn.Cursor = Cursors.Hand;
        btn.MouseEnter += (_, _) => { btn.BackColor = Palette.InkLight; btn.ForeColor = Palette.White; };
        btn.MouseLeave += (_, _) => { btn.BackColor = Palette.Ink; btn.ForeColor = Palette.Tagline; };
        btn.Click += onClick;
        pnlSidebar.Controls.Add(btn);
        return top + NavBtnHeight + 6;
    }

    private void BuildLogoutButton()
    {
        btnLogout.Text = "🚪  Sign Out";
        btnLogout.UseMnemonic = false;
        btnLogout.Location = new Point(14, ClientSize.Height - 55);
        btnLogout.Size = new Size(SidebarWidth - 28, 36);
        btnLogout.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
        btnLogout.FlatStyle = FlatStyle.Flat;
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.BackColor = Palette.Ink;
        btnLogout.ForeColor = Palette.MutedText;
        btnLogout.Font = new Font("Segoe UI", 9.5F);
        btnLogout.TextAlign = ContentAlignment.MiddleLeft;
        btnLogout.Padding = new Padding(12, 0, 0, 0);
        btnLogout.Cursor = Cursors.Hand;
        btnLogout.MouseEnter += (_, _) => btnLogout.ForeColor = Palette.AccentPink;
        btnLogout.MouseLeave += (_, _) => btnLogout.ForeColor = Palette.MutedText;
        btnLogout.Click += LogoutClicked;
        pnlSidebar.Controls.Add(btnLogout);
    }

    private void BuildWorkspace()
    {
        pnlWorkspace.Dock = DockStyle.Fill;
        pnlWorkspace.BackColor = Palette.Paper;
        Controls.Add(pnlWorkspace);
        PerformLayout();

        var cardWidth = pnlWorkspace.ClientSize.Width - 40;
        BuildGreetingCard(cardWidth);
        BuildMetricsArea(cardWidth);
        BuildRecentOrdersCard(cardWidth);
    }

    private void BuildGreetingCard(int width)
    {
        pnlGreetingCard.Location = new Point(20, 16);
        pnlGreetingCard.Size = new Size(width, 64);
        pnlGreetingCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlGreetingCard.BackColor = Palette.White;
        pnlGreetingCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlGreetingCard.ClientRectangle, Palette.Border, ButtonBorderStyle.Solid);
        pnlWorkspace.Controls.Add(pnlGreetingCard);

        var user = _auth.CurrentUser();
        lblGreeting.Text = $"Welcome back, {user?.FullName ?? "Owner"} ♥";
        lblGreeting.Font = new Font("Segoe UI", 13.5F, FontStyle.Bold);
        lblGreeting.ForeColor = Palette.Ink;
        lblGreeting.Location = new Point(16, 10);
        lblGreeting.AutoSize = true;
        pnlGreetingCard.Controls.Add(lblGreeting);

        lblGreetingSub.Text = "Bag Reselling Boutique · Cash on Delivery Dashboard";
        lblGreetingSub.Font = new Font("Segoe UI", 9F);
        lblGreetingSub.ForeColor = Palette.MutedText;
        lblGreetingSub.Location = new Point(16, 36);
        lblGreetingSub.AutoSize = true;
        pnlGreetingCard.Controls.Add(lblGreetingSub);

        AddGreetingButtons();
    }

    private void AddGreetingButtons()
    {
        btnQuickOrder.Text = "+ New Order";
        btnQuickOrder.Location = new Point(pnlGreetingCard.Width - 146, 14);
        btnQuickOrder.Size = new Size(130, 36);
        btnQuickOrder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnQuickOrder.FlatStyle = FlatStyle.Flat;
        btnQuickOrder.FlatAppearance.BorderSize = 0;
        btnQuickOrder.BackColor = Palette.Accent;
        btnQuickOrder.ForeColor = Palette.White;
        btnQuickOrder.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        btnQuickOrder.Cursor = Cursors.Hand;
        btnQuickOrder.Click += QuickNewOrderClicked;
        pnlGreetingCard.Controls.Add(btnQuickOrder);

        btnRefresh.Text = "⟳ Refresh";
        btnRefresh.Location = new Point(pnlGreetingCard.Width - 246, 14);
        btnRefresh.Size = new Size(92, 36);
        btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnRefresh.FlatStyle = FlatStyle.Flat;
        btnRefresh.FlatAppearance.BorderColor = Palette.Border;
        btnRefresh.FlatAppearance.BorderSize = 1;
        btnRefresh.BackColor = Palette.White;
        btnRefresh.ForeColor = Palette.Ink;
        btnRefresh.Font = new Font("Segoe UI", 9F);
        btnRefresh.Cursor = Cursors.Hand;
        btnRefresh.Click += (_, _) => RefreshDashboardMetrics();
        pnlGreetingCard.Controls.Add(btnRefresh);
    }

    private void BuildMetricsArea(int width)
    {
        tlpMetrics.Location = new Point(20, 88);
        tlpMetrics.Size = new Size(width, 136);
        tlpMetrics.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        tlpMetrics.ColumnCount = 4;
        tlpMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        tlpMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        tlpMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        tlpMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        tlpMetrics.RowCount = 1;
        tlpMetrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tlpMetrics.BackColor = Color.Transparent;
        pnlWorkspace.Controls.Add(tlpMetrics);

        tlpMetrics.Controls.Add(CreateMetricCard(Palette.Accent, "ORDERS PLACED", lblPlacedValue, lblPlacedSub, "Awaiting courier dispatch", "Orders Hub →", OrdersClicked), 0, 0);
        tlpMetrics.Controls.Add(CreateMetricCard(Palette.Warning, "COURIER TRACKING", lblPostedValue, lblPostedSub, "Packages in transit", "Tracking Hub →", DeliveryClicked), 1, 0);
        tlpMetrics.Controls.Add(CreateMetricCard(Palette.Success, "BAG CATALOG", lblStockValue, lblStockSub, "Warehouse inventory", "Stock Catalog →", ProductsClicked), 2, 0);

        if (SignedInOwner())
        {
            tlpMetrics.Controls.Add(CreateMetricCard(Palette.Accent, "COD PAYMENTS", lblPaymentValue, lblPaymentSub, "Cash on delivery due", "Reconcile Cash →", PaymentsClicked), 3, 0);
        }
        else
        {
            tlpMetrics.Controls.Add(CreateMetricCard(Palette.Success, "COMPLETED ORDERS", lblPaymentValue, lblPaymentSub, "Completed handovers", "Delivery Hub →", DeliveryClicked), 3, 0);
        }
    }

    private static Panel CreateMetricCard(Color accent, string category, Label lblVal, Label lblSub, string tag, string actionText, EventHandler onAction)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Palette.White,
            Margin = new Padding(4)
        };
        card.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, card.ClientRectangle, Palette.Border, ButtonBorderStyle.Solid);

        var topAccent = new Panel
        {
            Dock = DockStyle.Top,
            Height = 3,
            BackColor = accent
        };
        card.Controls.Add(topAccent);

        var lblCat = new Label
        {
            Text = category,
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            ForeColor = Palette.MutedText,
            Location = new Point(12, 10),
            AutoSize = true
        };
        card.Controls.Add(lblCat);

        lblVal.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lblVal.ForeColor = accent;
        lblVal.Location = new Point(12, 28);
        lblVal.AutoSize = true;
        card.Controls.Add(lblVal);

        lblSub.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblSub.ForeColor = Palette.Ink;
        lblSub.Location = new Point(12, 58);
        lblSub.AutoSize = true;
        card.Controls.Add(lblSub);

        var lblTag = new Label
        {
            Text = tag,
            Font = new Font("Segoe UI", 8F),
            ForeColor = Palette.MutedText,
            Location = new Point(12, 78),
            AutoSize = true
        };
        card.Controls.Add(lblTag);

        var btnAction = new Button
        {
            Text = actionText,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            Location = new Point(12, 102),
            Size = new Size(160, 26),
            FlatStyle = FlatStyle.Flat,
            BackColor = Palette.AccentLight,
            ForeColor = Palette.Accent,
            Cursor = Cursors.Hand
        };
        btnAction.FlatAppearance.BorderSize = 0;
        btnAction.MouseEnter += (_, _) => { btnAction.BackColor = Palette.Accent; btnAction.ForeColor = Palette.White; };
        btnAction.MouseLeave += (_, _) => { btnAction.BackColor = Palette.AccentLight; btnAction.ForeColor = Palette.Accent; };
        btnAction.Click += onAction;
        card.Resize += (_, _) => btnAction.Width = card.ClientSize.Width - 24;
        card.Controls.Add(btnAction);

        return card;
    }

    private void BuildRecentOrdersCard(int width)
    {
        pnlRecentCard.Location = new Point(20, 232);
        pnlRecentCard.Size = new Size(width, pnlWorkspace.ClientSize.Height - 248);
        pnlRecentCard.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
        pnlRecentCard.BackColor = Palette.White;
        pnlRecentCard.Paint += (s, e) => ControlPaint.DrawBorder(e.Graphics, pnlRecentCard.ClientRectangle, Palette.Border, ButtonBorderStyle.Solid);
        pnlWorkspace.Controls.Add(pnlRecentCard);

        BuildRecentHeader();
        ConfigureRecentGrid();
        pnlRecentCard.Controls.Add(dgvRecent);
    }

    private void BuildRecentHeader()
    {
        pnlRecentHeader.Location = new Point(1, 1);
        pnlRecentHeader.Size = new Size(pnlRecentCard.ClientSize.Width - 2, 38);
        pnlRecentHeader.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        pnlRecentHeader.BackColor = Palette.White;
        pnlRecentCard.Controls.Add(pnlRecentHeader);

        lblRecentTitle.Text = "✨ Recent Orders & Live Transactions";
        lblRecentTitle.UseMnemonic = false;
        lblRecentTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        lblRecentTitle.ForeColor = Palette.Ink;
        lblRecentTitle.Location = new Point(12, 10);
        lblRecentTitle.AutoSize = true;
        pnlRecentHeader.Controls.Add(lblRecentTitle);

        lblRecentFilter.Text = "Double-click a row to open in Orders Hub";
        lblRecentFilter.UseMnemonic = false;
        lblRecentFilter.Font = new Font("Segoe UI", 8.5F);
        lblRecentFilter.ForeColor = Palette.MutedText;
        lblRecentFilter.Location = new Point(pnlRecentHeader.Width - 260, 11);
        lblRecentFilter.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblRecentFilter.AutoSize = true;
        pnlRecentHeader.Controls.Add(lblRecentFilter);
    }

    private void ConfigureRecentGrid()
    {
        dgvRecent.Location = new Point(1, 39);
        dgvRecent.Size = new Size(pnlRecentCard.ClientSize.Width - 2, pnlRecentCard.ClientSize.Height - 40);
        dgvRecent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        GridTheme.Apply(dgvRecent);
        dgvRecent.ScrollBars = ScrollBars.Vertical;
        dgvRecent.AllowUserToResizeColumns = false;
        dgvRecent.AutoGenerateColumns = false;
        dgvRecent.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        AddRecentColumns();

        dgvRecent.CellFormatting += FormatRecentCell;
        dgvRecent.CellDoubleClick += (_, _) => OrdersClicked(this, EventArgs.Empty);
    }

    private void AddRecentColumns()
    {
        dgvRecent.Columns.Add("colId", "Order #");
        dgvRecent.Columns.Add("colDate", "Date");
        dgvRecent.Columns.Add("colCustomer", "Customer");
        dgvRecent.Columns.Add("colTotal", "Total Amount");
        dgvRecent.Columns.Add("colStatus", "Status");

        dgvRecent.Columns[0].FillWeight = 14;
        dgvRecent.Columns[0].MinimumWidth = 95;
        dgvRecent.Columns[1].FillWeight = 15;
        dgvRecent.Columns[1].MinimumWidth = 105;
        dgvRecent.Columns[2].FillWeight = 38;
        dgvRecent.Columns[2].MinimumWidth = 180;
        dgvRecent.Columns[3].FillWeight = 18;
        dgvRecent.Columns[3].MinimumWidth = 125;
        dgvRecent.Columns[4].FillWeight = 15;
        dgvRecent.Columns[4].MinimumWidth = 100;
    }

    private static void FormatRecentCell(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.Value == null)
        {
            return;
        }

        if (e.ColumnIndex == 0)
        {
            e.CellStyle!.ForeColor = Palette.Accent;
            e.CellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }
        else if (e.ColumnIndex == 3)
        {
            e.CellStyle!.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        }
        else if (e.ColumnIndex == 4)
        {
            GridTheme.HighlightStatusCell(e, e.Value.ToString());
        }
    }

    private void RefreshDashboardMetrics()
    {
        var orders = new List<CustomerOrder>();
        var ordersResult = _orders.ListOrders(orders);
        if (ordersResult.Success)
        {
            UpdateOrderMetrics(orders);
            PopulateRecentGrid(orders);
        }

        UpdateDeliveryMetrics();
        UpdateProductMetrics();
    }

    private void UpdateOrderMetrics(List<CustomerOrder> orders)
    {
        var placed = orders.Where(o => o.Status == OrderStatus.Placed).ToList();
        lblPlacedValue.Text = $"{placed.Count} Placed";
        lblPlacedSub.Text = $"LKR {placed.Sum(o => o.TotalAmount):N2}";

        var posted = orders.Where(o => o.Status == OrderStatus.Posted).ToList();
        lblPostedValue.Text = $"{posted.Count} In Delivery";

        if (SignedInOwner())
        {
            var pendingCod = orders.Where(o => o.Status == OrderStatus.Delivered && !o.PaymentReceived).ToList();
            lblPaymentValue.Text = $"LKR {pendingCod.Sum(o => o.TotalAmount):N2}";
            lblPaymentSub.Text = $"{pendingCod.Count} Uncollected COD";
        }
        else
        {
            var delivered = orders.Count(o => o.Status == OrderStatus.Delivered);
            lblPaymentValue.Text = $"{delivered} Delivered";
            lblPaymentSub.Text = "Completed Orders";
        }
    }

    private void UpdateDeliveryMetrics()
    {
        var followUps = new List<FollowUpOrder>();
        var followUpResult = _delivery.GetFollowUpList(followUps);
        if (followUpResult.Success)
        {
            lblPostedSub.Text = $"{followUps.Count} Overdue (2+ Days)";
            lblPostedSub.ForeColor = followUps.Count > 0 ? Palette.Warning : Palette.Ink;
        }
    }

    private void UpdateProductMetrics()
    {
        var products = new List<ProductCatalogItem>();
        var productsResult = _products.ListProductsForAssistant(products);
        if (productsResult.Success)
        {
            var lowStock = products.Count(p => p.Quantity <= 5);
            lblStockValue.Text = $"{products.Count} Products";
            lblStockSub.Text = $"{lowStock} Low Stock (<= 5)";
            lblStockSub.ForeColor = lowStock > 0 ? Palette.AccentPink : Palette.Ink;
        }
    }

    private void PopulateRecentGrid(List<CustomerOrder> orders)
    {
        dgvRecent.Rows.Clear();
        var recent = orders.Take(15);
        foreach (var o in recent)
        {
            var customer = FormatCustomerSummary(o);
            dgvRecent.Rows.Add(
                $"#ORD-{o.OrderId}",
                o.OrderDate.ToString("yyyy-MM-dd"),
                customer,
                $"LKR {o.TotalAmount:N2}",
                o.Status.ToString());
        }

        dgvRecent.ClearSelection();
    }

    private static string FormatCustomerSummary(CustomerOrder order)
    {
        var hasName = !string.IsNullOrWhiteSpace(order.CustomerName);
        var hasPhone = !string.IsNullOrWhiteSpace(order.CustomerPhone);
        var label = hasName switch
        {
            true when hasPhone => $"{order.CustomerName} ({order.CustomerPhone})",
            true => order.CustomerName!,
            false when hasPhone => order.CustomerPhone!,
            _ => "Walk-in Customer"
        };

        return order.IsRepeatCustomer ? $"{label}  ♥ Repeat" : label;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        RequirePasswordChange();
    }

    private void RequirePasswordChange()
    {
        if (_passwordPrompted)
        {
            return;
        }

        _passwordPrompted = true;
        var user = _auth.CurrentUser();
        if (user?.MustChangePassword != true)
        {
            return;
        }

        MessageBox.Show(this, Messages.MustChangePassword, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        using var changeForm = new ChangePasswordForm(_auth);
        if (changeForm.ShowDialog(this) == DialogResult.OK)
        {
            return;
        }

        _auth.Logout();
        Close();
    }

    private bool SignedInOwner() => _auth.CurrentUser()?.Role == UserRole.Owner;

    private void QuickNewOrderClicked(object? sender, EventArgs e)
    {
        using var form = new OrderEntryForm(_orders, _products);
        form.ShowDialog(this);
        RefreshDashboardMetrics();
    }

    private void OrdersClicked(object? sender, EventArgs e)
    {
        using var form = new OrdersForm(_orders, _products);
        form.ShowDialog(this);
        RefreshDashboardMetrics();
    }

    private void DeliveryClicked(object? sender, EventArgs e)
    {
        using var form = new DeliveryForm(_delivery, _orders);
        form.ShowDialog(this);
        RefreshDashboardMetrics();
    }

    private void ProductsClicked(object? sender, EventArgs e)
    {
        using var form = new ProductsForm(_products);
        form.ShowDialog(this);
        RefreshDashboardMetrics();
    }

    private void PaymentsClicked(object? sender, EventArgs e)
    {
        using var form = new PaymentsForm(_payments);
        form.ShowDialog(this);
        RefreshDashboardMetrics();
    }

    private void WholesalersClicked(object? sender, EventArgs e)
    {
        using var form = new WholesalersForm(_wholesalers);
        form.ShowDialog(this);
    }

    private void StockOrdersClicked(object? sender, EventArgs e)
    {
        using var form = new StockOrdersForm(_stock, _wholesalers, _products);
        form.ShowDialog(this);
        RefreshDashboardMetrics();
    }

    private void AccountsClicked(object? sender, EventArgs e)
    {
        using var form = new AccountsForm(_accounts);
        form.ShowDialog(this);
    }

    private void ExportClicked(object? sender, EventArgs e)
    {
        using var form = new ExportForm(_export);
        form.ShowDialog(this);
    }

    private void LogoutClicked(object? sender, EventArgs e)
    {
        _auth.Logout();
        Close();
    }
}
