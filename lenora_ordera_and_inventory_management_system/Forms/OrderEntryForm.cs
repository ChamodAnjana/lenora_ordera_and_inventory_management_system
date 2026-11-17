using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class OrderEntryForm : Form
{
    private const int FormWidth = 760;
    private const int FormHeight = 700;
    private const int FieldX = 32;

    private readonly OrderService _orders;
    private readonly ProductService _products;
    private readonly int _orderId;
    private readonly List<OrderItemRequest> _items = new();
    private readonly List<ProductCatalogItem> _allProducts = new();

    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly GroupBox grpCustomer = new();
    private readonly Label lblPhone = new();
    private readonly TextBox txtPhone = new();
    private readonly Button btnLookupPhone = new();
    private readonly Label lblName = new();
    private readonly TextBox txtName = new();
    private readonly Label lblAddress = new();
    private readonly TextBox txtAddress = new();
    private readonly Label lblRepeatNotice = new();

    private readonly GroupBox grpItems = new();
    private readonly Label lblFindProduct = new();
    private readonly TextBox txtFindProduct = new();
    private readonly Label lblProduct = new();
    private readonly ComboBox cmbProduct = new();
    private readonly Label lblPrice = new();
    private readonly TextBox txtPrice = new();
    private readonly Label lblAvailable = new();
    private readonly Label lblQuantity = new();
    private readonly TextBox txtQuantity = new();
    private readonly Button btnAddItem = new();
    private readonly Button btnRemoveItem = new();
    private readonly DataGridView dgvItems = new();
    private readonly Label lblTotal = new();

    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();

    public OrderEntryForm(OrderService orders, ProductService products, int orderId = 0)
    {
        _orders = orders;
        _products = products;
        _orderId = orderId;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddCustomerSection();
        AddItemsSection();
        AddFooter();
        LoadProducts();
        LoadExistingOrderIfEditing();
    }

    private void BuildChrome()
    {
        Text = _orderId > 0 ? Messages.EditOrder : Messages.NewOrder;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(FormWidth, FormHeight);
        BackColor = Palette.Paper;
        CancelButton = btnCancel;
    }

    private void AddAccent()
    {
        pnlAccent.BackColor = Palette.Accent;
        pnlAccent.Location = new Point(0, 0);
        pnlAccent.Size = new Size(FormWidth, 6);
        Controls.Add(pnlAccent);
    }

    private void AddTitle()
    {
        lblTitle.Text = _orderId > 0 ? Messages.EditOrder : Messages.NewOrder;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 16);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddCustomerSection()
    {
        grpCustomer.Text = Messages.Customer;
        grpCustomer.Location = new Point(FieldX, 55);
        grpCustomer.Size = new Size(FormWidth - (FieldX * 2), 140);
        grpCustomer.ForeColor = Palette.Ink;

        lblPhone.Text = Messages.CustomerPhone + ":";
        lblPhone.Location = new Point(16, 28);
        lblPhone.AutoSize = true;
        grpCustomer.Controls.Add(lblPhone);

        txtPhone.Location = new Point(130, 25);
        txtPhone.Size = new Size(180, 26);
        txtPhone.MaxLength = Rules.CustomerPhoneMaxLength;
        txtPhone.Leave += PhoneLeave;
        grpCustomer.Controls.Add(txtPhone);

        btnLookupPhone.Text = Messages.FindCustomer;
        btnLookupPhone.Location = new Point(320, 24);
        btnLookupPhone.Size = new Size(130, 28);
        btnLookupPhone.FlatStyle = FlatStyle.Flat;
        btnLookupPhone.BackColor = Palette.White;
        btnLookupPhone.Click += LookupClicked;
        grpCustomer.Controls.Add(btnLookupPhone);

        lblRepeatNotice.Location = new Point(460, 28);
        lblRepeatNotice.AutoSize = true;
        lblRepeatNotice.ForeColor = Palette.Accent;
        lblRepeatNotice.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpCustomer.Controls.Add(lblRepeatNotice);

        lblName.Text = Messages.CustomerName + ":";
        lblName.Location = new Point(16, 64);
        lblName.AutoSize = true;
        grpCustomer.Controls.Add(lblName);

        txtName.Location = new Point(130, 61);
        txtName.Size = new Size(270, 26);
        grpCustomer.Controls.Add(txtName);

        lblAddress.Text = Messages.Address + ":";
        lblAddress.Location = new Point(16, 100);
        lblAddress.AutoSize = true;
        grpCustomer.Controls.Add(lblAddress);

        txtAddress.Location = new Point(130, 97);
        txtAddress.Size = new Size(540, 26);
        grpCustomer.Controls.Add(txtAddress);

        Controls.Add(grpCustomer);
    }

    private void AddItemsSection()
    {
        grpItems.Text = Messages.OrderItems;
        grpItems.Location = new Point(FieldX, 205);
        grpItems.Size = new Size(FormWidth - (FieldX * 2), 401);
        grpItems.ForeColor = Palette.Ink;

        lblFindProduct.Text = Messages.FindProduct + ":";
        lblFindProduct.Location = new Point(16, 28);
        lblFindProduct.AutoSize = true;
        grpItems.Controls.Add(lblFindProduct);

        txtFindProduct.Location = new Point(110, 25);
        txtFindProduct.Size = new Size(250, 26);
        txtFindProduct.TextChanged += FindProductChanged;
        grpItems.Controls.Add(txtFindProduct);

        lblProduct.Text = Messages.Product + ":";
        lblProduct.Location = new Point(16, 64);
        lblProduct.AutoSize = true;
        grpItems.Controls.Add(lblProduct);

        cmbProduct.Location = new Point(80, 61);
        cmbProduct.Size = new Size(200, 26);
        cmbProduct.DropDownStyle = ComboBoxStyle.DropDown;
        cmbProduct.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        cmbProduct.AutoCompleteSource = AutoCompleteSource.ListItems;
        cmbProduct.SelectedIndexChanged += ProductSelectionChanged;
        grpItems.Controls.Add(cmbProduct);

        lblPrice.Text = Messages.SellingPrice + ":";
        lblPrice.Location = new Point(290, 64);
        lblPrice.AutoSize = true;
        grpItems.Controls.Add(lblPrice);

        txtPrice.Location = new Point(370, 61);
        txtPrice.Size = new Size(80, 26);
        txtPrice.ReadOnly = true;
        grpItems.Controls.Add(txtPrice);

        lblQuantity.Text = Messages.Quantity + ":";
        lblQuantity.Location = new Point(460, 64);
        lblQuantity.AutoSize = true;
        grpItems.Controls.Add(lblQuantity);

        txtQuantity.Location = new Point(525, 61);
        txtQuantity.Size = new Size(60, 26);
        grpItems.Controls.Add(txtQuantity);

        lblAvailable.Location = new Point(80, 92);
        lblAvailable.AutoSize = true;
        lblAvailable.ForeColor = Color.DimGray;
        grpItems.Controls.Add(lblAvailable);

        btnAddItem.Text = Messages.AddItem;
        btnAddItem.Location = new Point(595, 60);
        btnAddItem.Size = new Size(90, 28);
        btnAddItem.FlatStyle = FlatStyle.Flat;
        btnAddItem.BackColor = Palette.Accent;
        btnAddItem.ForeColor = Palette.White;
        btnAddItem.Click += AddItemClicked;
        grpItems.Controls.Add(btnAddItem);

        BuildItemsGrid();
        grpItems.Controls.Add(dgvItems);

        btnRemoveItem.Text = Messages.RemoveItem;
        btnRemoveItem.Location = new Point(16, 361);
        btnRemoveItem.Size = new Size(110, 30);
        btnRemoveItem.FlatStyle = FlatStyle.Flat;
        btnRemoveItem.BackColor = Palette.White;
        btnRemoveItem.Click += RemoveItemClicked;
        grpItems.Controls.Add(btnRemoveItem);

        lblTotal.Text = $"{Messages.TotalAmount}: Rs. 0.00";
        lblTotal.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        lblTotal.Location = new Point(450, 364);
        lblTotal.Size = new Size(235, 26);
        lblTotal.TextAlign = ContentAlignment.MiddleRight;
        grpItems.Controls.Add(lblTotal);

        Controls.Add(grpItems);
    }

    private void BuildItemsGrid()
    {
        dgvItems.Location = new Point(16, 116);
        dgvItems.Size = new Size(grpItems.Width - 32, 235);
        GridTheme.Apply(dgvItems);
        dgvItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        dgvItems.Columns.Add("ProductId", "ID");
        dgvItems.Columns["ProductId"]!.Visible = false;

        var colProduct = new DataGridViewTextBoxColumn
        {
            Name = "ProductName",
            HeaderText = Messages.Product,
            FillWeight = 220,
            MinimumWidth = 180,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        dgvItems.Columns.Add(colProduct);

        var colQty = new DataGridViewTextBoxColumn
        {
            Name = "Quantity",
            HeaderText = Messages.Quantity,
            FillWeight = 70,
            MinimumWidth = 65,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colQty.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
        dgvItems.Columns.Add(colQty);

        var colPrice = new DataGridViewTextBoxColumn
        {
            Name = "SellingPrice",
            HeaderText = Messages.SellingPrice,
            FillWeight = 95,
            MinimumWidth = 90,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colPrice.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvItems.Columns.Add(colPrice);

        var colSubtotal = new DataGridViewTextBoxColumn
        {
            Name = "Subtotal",
            HeaderText = "Subtotal",
            FillWeight = 105,
            MinimumWidth = 100,
            SortMode = DataGridViewColumnSortMode.NotSortable
        };
        colSubtotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        colSubtotal.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
        dgvItems.Columns.Add(colSubtotal);
    }

    private void AddFooter()
    {
        btnSave.Text = Messages.Save;
        btnSave.Location = new Point(FormWidth - 250, FormHeight - 65);
        btnSave.Size = new Size(100, 34);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.BackColor = Palette.Accent;
        btnSave.ForeColor = Palette.White;
        btnSave.Click += SaveClicked;
        Controls.Add(btnSave);

        btnCancel.Text = Messages.Cancel;
        btnCancel.Location = new Point(FormWidth - 140, FormHeight - 65);
        btnCancel.Size = new Size(100, 34);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.BackColor = Palette.White;
        btnCancel.Click += (_, _) => Close();
        Controls.Add(btnCancel);
    }

    private void LoadProducts()
    {
        _allProducts.Clear();
        var result = _products.ListProductsForAssistant(_allProducts);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        ShowProducts(MatchingProducts(txtFindProduct.Text));
    }

    private void FindProductChanged(object? sender, EventArgs e)
    {
        ShowProducts(MatchingProducts(txtFindProduct.Text));
    }

    private IEnumerable<ProductCatalogItem> MatchingProducts(string term)
    {
        var query = term.Trim();
        if (query.Length == 0)
        {
            return _allProducts;
        }

        return _allProducts.Where(product => product.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
    }

    private void ShowProducts(IEnumerable<ProductCatalogItem> products)
    {
        cmbProduct.AutoCompleteMode = AutoCompleteMode.None;
        cmbProduct.BeginUpdate();
        cmbProduct.Items.Clear();
        foreach (var product in products)
        {
            cmbProduct.Items.Add(product.Name);
        }

        cmbProduct.EndUpdate();
        cmbProduct.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
        cmbProduct.SelectedIndex = cmbProduct.Items.Count > 0 ? 0 : -1;
        if (cmbProduct.SelectedIndex < 0)
        {
            txtPrice.Clear();
            lblAvailable.Text = string.Empty;
        }
    }

    private ProductCatalogItem? SelectedCatalogProduct()
    {
        var name = cmbProduct.Text.Trim();
        if (name.Length == 0)
        {
            return null;
        }

        return _allProducts.FirstOrDefault(product => product.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    private void ProductSelectionChanged(object? sender, EventArgs e)
    {
        var selected = SelectedCatalogProduct();
        if (selected is null)
        {
            txtPrice.Clear();
            lblAvailable.Text = string.Empty;
            return;
        }

        txtPrice.Text = selected.SellingPrice.ToString("F2");
        lblAvailable.Text = string.Format(Messages.AvailableStock, selected.Quantity);
    }

    private void PhoneLeave(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtPhone.Text))
        {
            LookupByPhone();
        }
    }

    private void LookupClicked(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(txtPhone.Text))
        {
            LookupByPhone();
            return;
        }

        LookupByName();
    }

    private void LookupByPhone()
    {
        var result = _orders.FindCustomerByPhone(txtPhone.Text.Trim(), out var customer);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (customer is null)
        {
            lblRepeatNotice.Text = string.Empty;
            return;
        }

        ApplyCustomer(customer);
    }

    private void LookupByName()
    {
        var matches = new List<Customer>();
        var result = _orders.FindCustomersByName(txtName.Text.Trim(), matches);
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (matches.Count == 0)
        {
            MessageBox.Show(this, Messages.CustomerNotFound, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (matches.Count == 1)
        {
            ApplyCustomer(matches[0]);
            return;
        }

        using var picker = new CustomerPickForm(matches);
        if (picker.ShowDialog(this) == DialogResult.OK && picker.SelectedCustomer is not null)
        {
            ApplyCustomer(picker.SelectedCustomer);
        }
    }

    private void ApplyCustomer(Customer customer)
    {
        txtPhone.Text = customer.Phone;
        txtName.Text = customer.Name;
        txtAddress.Text = customer.Address;
        lblRepeatNotice.Text = customer.IsRepeat ? $"♥ {Messages.RepeatCustomer}" : string.Empty;
    }

    private void AddItemClicked(object? sender, EventArgs e)
    {
        var product = SelectedCatalogProduct();
        if (product is null)
        {
            MessageBox.Show(this, Messages.SelectProduct, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(txtQuantity.Text.Trim(), out var qty) || qty <= 0)
        {
            MessageBox.Show(this, Messages.QuantityInvalid, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var existing = _items.FirstOrDefault(i => i.ProductId == product.ProductId);
        if (existing != null)
        {
            _items.Remove(existing);
        }

        _items.Add(new OrderItemRequest
        {
            ProductId = product.ProductId,
            ProductName = product.Name,
            Quantity = qty,
            SellingPrice = product.SellingPrice
        });

        RefreshItemsGrid();
        txtQuantity.Clear();
    }

    private void RemoveItemClicked(object? sender, EventArgs e)
    {
        if (dgvItems.SelectedRows.Count == 0)
        {
            return;
        }

        var productId = Convert.ToInt32(dgvItems.SelectedRows[0].Cells["ProductId"].Value);
        _items.RemoveAll(i => i.ProductId == productId);
        RefreshItemsGrid();
    }

    private void RefreshItemsGrid()
    {
        dgvItems.Rows.Clear();
        decimal total = 0;
        foreach (var item in _items)
        {
            var subtotal = item.Quantity * item.SellingPrice;
            total += subtotal;
            dgvItems.Rows.Add(item.ProductId, item.ProductName, item.Quantity, item.SellingPrice.ToString("F2"), subtotal.ToString("F2"));
        }
        lblTotal.Text = $"{Messages.TotalAmount}: Rs. {total:F2}";
    }

    private void LoadExistingOrderIfEditing()
    {
        if (_orderId <= 0)
        {
            return;
        }

        var itemsList = new List<OrderItem>();
        var result = _orders.GetOrderDetails(_orderId, out var order, itemsList);
        if (!result.Success || order is null)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
            return;
        }

        txtPhone.Text = order.CustomerPhone;
        txtName.Text = order.CustomerName;
        txtAddress.Text = order.CustomerAddress;
        lblRepeatNotice.Text = order.IsRepeatCustomer ? $"♥ {Messages.RepeatCustomer}" : string.Empty;

        _items.Clear();
        foreach (var item in itemsList)
        {
            _items.Add(new OrderItemRequest
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                SellingPrice = item.SellingPrice
            });
        }
        RefreshItemsGrid();
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        var result = _orderId > 0 ? ExecuteUpdate() : ExecutePlacement();
        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }

    private Result ExecutePlacement()
    {
        var request = new OrderPlacementRequest
        {
            CustomerName = txtName.Text.Trim(),
            CustomerPhone = txtPhone.Text.Trim(),
            CustomerAddress = txtAddress.Text.Trim(),
            Items = _items
        };
        return _orders.PlaceOrder(request);
    }

    private Result ExecuteUpdate()
    {
        var request = new OrderUpdateRequest
        {
            OrderId = _orderId,
            CustomerName = txtName.Text.Trim(),
            CustomerPhone = txtPhone.Text.Trim(),
            CustomerAddress = txtAddress.Text.Trim(),
            Items = _items
        };
        return _orders.UpdateOrder(request);
    }
}
