using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class ProductEntryForm : Form
{
    private const int FormWidth = 440;
    private const int FormHeight = 510;
    private const int FieldX = 32;
    private const int FieldWidth = 376;
    private const int LabelGap = 22;

    private readonly ProductService _products;
    private readonly int? _productId;
    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblName = new();
    private readonly TextBox txtName = new();
    private readonly Label lblCategory = new();
    private readonly TextBox txtCategory = new();
    private readonly Label lblCostPrice = new();
    private readonly TextBox txtCostPrice = new();
    private readonly Label lblSellingPrice = new();
    private readonly TextBox txtSellingPrice = new();
    private readonly Label lblQuantity = new();
    private readonly TextBox txtQuantity = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();

    public ProductEntryForm(ProductService products, Product? existingProduct = null)
    {
        _products = products;
        _productId = existingProduct?.ProductId;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddFields();
        AddActions();
        PopulateExisting(existingProduct);
    }

    private void BuildChrome()
    {
        Text = _productId.HasValue ? Messages.EditProduct : Messages.NewProduct;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(FormWidth, FormHeight);
        BackColor = Palette.Paper;
        AcceptButton = btnSave;
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
        lblTitle.Text = _productId.HasValue ? Messages.EditProduct : Messages.NewProduct;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 20);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddFields()
    {
        Place(lblName, txtName, Messages.ProductName, 64);
        Place(lblCategory, txtCategory, Messages.Category, 128);
        Place(lblCostPrice, txtCostPrice, Messages.CostPrice, 192);
        Place(lblSellingPrice, txtSellingPrice, Messages.SellingPrice, 256);
        Place(lblQuantity, txtQuantity, Messages.Quantity, 320);

        txtName.MaxLength = Rules.ProductNameMaxLength;
        txtCategory.MaxLength = Rules.ProductCategoryMaxLength;
        txtCostPrice.Text = "0.00";
        txtQuantity.Text = "0";

        txtName.TabIndex = 0;
        txtCategory.TabIndex = 1;
        txtCostPrice.TabIndex = 2;
        txtSellingPrice.TabIndex = 3;
        txtQuantity.TabIndex = 4;
    }

    private void Place(Label label, TextBox box, string caption, int top)
    {
        label.Text = caption;
        label.AutoSize = true;
        label.ForeColor = Palette.Ink;
        label.Location = new Point(FieldX, top);
        box.Location = new Point(FieldX, top + LabelGap);
        box.Size = new Size(FieldWidth, 28);
        box.BorderStyle = BorderStyle.FixedSingle;
        Controls.Add(label);
        Controls.Add(box);
    }

    private void AddActions()
    {
        btnSave.Text = Messages.Save;
        btnSave.Location = new Point(FieldX, 410);
        btnSave.Size = new Size(130, 36);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.BackColor = Palette.Accent;
        btnSave.ForeColor = Palette.White;
        btnSave.Cursor = Cursors.Hand;
        btnSave.TabIndex = 5;
        btnSave.Click += SaveClicked;

        btnCancel.Text = Messages.Cancel;
        btnCancel.Location = new Point(176, 410);
        btnCancel.Size = new Size(110, 36);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderColor = Palette.Ink;
        btnCancel.FlatAppearance.BorderSize = 1;
        btnCancel.BackColor = Palette.White;
        btnCancel.ForeColor = Palette.Ink;
        btnCancel.TabIndex = 6;
        btnCancel.DialogResult = DialogResult.Cancel;

        Controls.Add(btnSave);
        Controls.Add(btnCancel);
    }

    private void PopulateExisting(Product? product)
    {
        if (product is null)
        {
            return;
        }

        txtName.Text = product.Name;
        txtCategory.Text = product.Category;
        txtCostPrice.Text = product.CostPrice.ToString("F2");
        txtSellingPrice.Text = product.SellingPrice.ToString("F2");
        txtQuantity.Text = product.Quantity.ToString();
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        if (!decimal.TryParse(txtCostPrice.Text.Trim(), out var costPrice) || costPrice < 0)
        {
            MessageBox.Show(this, Messages.CostPriceInvalid, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtCostPrice.Focus();
            return;
        }

        if (!decimal.TryParse(txtSellingPrice.Text.Trim(), out var sellingPrice) || sellingPrice <= 0)
        {
            MessageBox.Show(this, Messages.SellingPriceInvalid, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtSellingPrice.Focus();
            return;
        }

        if (!int.TryParse(txtQuantity.Text.Trim(), out var quantity) || quantity < 0)
        {
            MessageBox.Show(this, Messages.QuantityInvalid, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtQuantity.Focus();
            return;
        }

        var request = new ProductRequest
        {
            Name = txtName.Text.Trim(),
            Category = txtCategory.Text.Trim(),
            CostPrice = costPrice,
            SellingPrice = sellingPrice,
            Quantity = quantity
        };

        var result = _productId.HasValue
            ? _products.UpdateProduct(_productId.Value, request)
            : _products.AddProduct(request);

        if (!result.Success)
        {
            MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(this, result.Message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }
}
