using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class ConfirmPaymentForm : Form
{
    private const int FormWidth = 420;
    private const int FormHeight = 250;
    private const int FieldX = 32;

    private readonly PaymentService _payments;
    private readonly int _orderId;

    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblAmount = new();
    private readonly TextBox txtAmount = new();
    private readonly Label lblDate = new();
    private readonly DateTimePicker dtpPaymentDate = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();

    public ConfirmPaymentForm(PaymentService payments, int orderId, decimal suggestedAmount)
    {
        _payments = payments;
        _orderId = orderId;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddFields(suggestedAmount);
        AddButtons();
    }

    private void BuildChrome()
    {
        Text = $"{Messages.ConfirmPayment} #{_orderId}";
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
        lblTitle.Text = $"{Messages.ConfirmPayment} #{_orderId}";
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 16);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddFields(decimal suggestedAmount)
    {
        lblAmount.Text = Messages.AmountReceived + ":";
        lblAmount.Location = new Point(FieldX, 60);
        lblAmount.AutoSize = true;
        Controls.Add(lblAmount);

        txtAmount.Location = new Point(FieldX + 140, 57);
        txtAmount.Size = new Size(210, 26);
        txtAmount.Text = suggestedAmount.ToString("F2");
        Controls.Add(txtAmount);

        lblDate.Text = Messages.PaymentDate + ":";
        lblDate.Location = new Point(FieldX, 102);
        lblDate.AutoSize = true;
        Controls.Add(lblDate);

        dtpPaymentDate.Location = new Point(FieldX + 140, 99);
        dtpPaymentDate.Size = new Size(210, 26);
        dtpPaymentDate.Format = DateTimePickerFormat.Short;
        dtpPaymentDate.Value = DateTime.Today;
        Controls.Add(dtpPaymentDate);
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
        if (!decimal.TryParse(txtAmount.Text.Trim(), out var amount) || amount <= 0)
        {
            MessageBox.Show(this, Messages.PaymentAmountInvalid, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var request = new CodPaymentRequest
        {
            OrderId = _orderId,
            AmountReceived = amount,
            PaymentDate = dtpPaymentDate.Value.Date
        };

        var result = _payments.ConfirmCodPayment(request);
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
