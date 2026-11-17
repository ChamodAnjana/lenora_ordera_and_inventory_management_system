using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class PostOrderForm : Form
{
    private const int FormWidth = 420;
    private const int FormHeight = 230;
    private const int FieldX = 32;

    private readonly DeliveryService _delivery;
    private readonly int _orderId;

    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblCourier = new();
    private readonly TextBox txtCourier = new();
    private readonly Label lblDate = new();
    private readonly DateTimePicker dtpPostingDate = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();

    public PostOrderForm(DeliveryService delivery, int orderId)
    {
        _delivery = delivery;
        _orderId = orderId;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddFields();
        AddButtons();
    }

    private void BuildChrome()
    {
        Text = $"{Messages.PostOrder} #{_orderId}";
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
        lblTitle.Text = $"{Messages.PostOrder} #{_orderId}";
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 16);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddFields()
    {
        lblCourier.Text = Messages.CourierName + ":";
        lblCourier.Location = new Point(FieldX, 60);
        lblCourier.AutoSize = true;
        Controls.Add(lblCourier);

        txtCourier.Location = new Point(FieldX + 130, 57);
        txtCourier.Size = new Size(220, 26);
        Controls.Add(txtCourier);

        lblDate.Text = Messages.PostingDate + ":";
        lblDate.Location = new Point(FieldX, 102);
        lblDate.AutoSize = true;
        Controls.Add(lblDate);

        dtpPostingDate.Location = new Point(FieldX + 130, 99);
        dtpPostingDate.Size = new Size(220, 26);
        dtpPostingDate.Format = DateTimePickerFormat.Short;
        dtpPostingDate.Value = DateTime.Today;
        Controls.Add(dtpPostingDate);
    }

    private void AddButtons()
    {
        btnSave.Text = Messages.Save;
        btnSave.Location = new Point(FormWidth - 230, FormHeight - 55);
        btnSave.Size = new Size(90, 32);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.BackColor = Palette.Accent;
        btnSave.ForeColor = Palette.White;
        btnSave.Click += SaveClicked;
        Controls.Add(btnSave);

        btnCancel.Text = Messages.Cancel;
        btnCancel.Location = new Point(FormWidth - 130, FormHeight - 55);
        btnCancel.Size = new Size(90, 32);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.BackColor = Palette.White;
        btnCancel.Click += (_, _) => Close();
        Controls.Add(btnCancel);
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        var request = new PostingRequest
        {
            OrderId = _orderId,
            CourierName = txtCourier.Text.Trim(),
            PostingDate = dtpPostingDate.Value.Date
        };

        var result = _delivery.RecordPosting(request);
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
