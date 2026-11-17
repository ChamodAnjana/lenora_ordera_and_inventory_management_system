using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Models;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class WholesalerEntryForm : Form
{
    private const int FormWidth = 440;
    private const int FormHeight = 400;
    private const int FieldX = 32;
    private const int FieldWidth = 376;
    private const int LabelGap = 22;

    private readonly WholesalerService _wholesalers;
    private readonly int? _wholesalerId;
    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblName = new();
    private readonly TextBox txtName = new();
    private readonly Label lblContactNo = new();
    private readonly TextBox txtContactNo = new();
    private readonly Label lblAddress = new();
    private readonly TextBox txtAddress = new();
    private readonly Button btnSave = new();
    private readonly Button btnCancel = new();

    public WholesalerEntryForm(WholesalerService wholesalers, Wholesaler? existingWholesaler = null)
    {
        _wholesalers = wholesalers;
        _wholesalerId = existingWholesaler?.WholesalerId;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddFields();
        AddActions();
        PopulateExisting(existingWholesaler);
    }

    private void BuildChrome()
    {
        Text = _wholesalerId.HasValue ? Messages.EditWholesaler : Messages.NewWholesaler;
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
        lblTitle.Text = _wholesalerId.HasValue ? Messages.EditWholesaler : Messages.NewWholesaler;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 20);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddFields()
    {
        Place(lblName, txtName, Messages.WholesalerNameRequired, 64);
        Place(lblContactNo, txtContactNo, Messages.ContactNo, 136);
        Place(lblAddress, txtAddress, Messages.Address, 208);

        txtName.MaxLength = Rules.WholesalerNameMaxLength;
        txtContactNo.MaxLength = Rules.WholesalerContactNoMaxLength;
        txtAddress.MaxLength = Rules.WholesalerAddressMaxLength;

        txtName.TabIndex = 0;
        txtContactNo.TabIndex = 1;
        txtAddress.TabIndex = 2;
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
        btnSave.Location = new Point(FieldX, 300);
        btnSave.Size = new Size(130, 36);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.BackColor = Palette.Accent;
        btnSave.ForeColor = Palette.White;
        btnSave.Cursor = Cursors.Hand;
        btnSave.TabIndex = 3;
        btnSave.Click += SaveClicked;

        btnCancel.Text = Messages.Cancel;
        btnCancel.Location = new Point(176, 300);
        btnCancel.Size = new Size(110, 36);
        btnCancel.FlatStyle = FlatStyle.Flat;
        btnCancel.FlatAppearance.BorderColor = Palette.Ink;
        btnCancel.FlatAppearance.BorderSize = 1;
        btnCancel.BackColor = Palette.White;
        btnCancel.ForeColor = Palette.Ink;
        btnCancel.TabIndex = 4;
        btnCancel.DialogResult = DialogResult.Cancel;

        Controls.Add(btnSave);
        Controls.Add(btnCancel);
    }

    private void PopulateExisting(Wholesaler? wholesaler)
    {
        if (wholesaler is null)
        {
            return;
        }

        txtName.Text = wholesaler.Name;
        txtContactNo.Text = wholesaler.ContactNo;
        txtAddress.Text = wholesaler.Address ?? string.Empty;
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        var request = new WholesalerRequest
        {
            Name = txtName.Text.Trim(),
            ContactNo = txtContactNo.Text.Trim(),
            Address = txtAddress.Text.Trim()
        };

        var result = _wholesalerId.HasValue
            ? _wholesalers.UpdateWholesaler(_wholesalerId.Value, request)
            : _wholesalers.AddWholesaler(request);

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
