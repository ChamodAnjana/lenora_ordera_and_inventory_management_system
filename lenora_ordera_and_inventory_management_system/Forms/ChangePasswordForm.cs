using lenora_ordera_and_inventory_management_system.Common;
using lenora_ordera_and_inventory_management_system.Services;

namespace lenora_ordera_and_inventory_management_system.Forms;

internal sealed class ChangePasswordForm : Form
{
    private const int FormWidth = 420;
    private const int FormHeight = 360;
    private const int FieldX = 40;
    private const int FieldWidth = 340;
    private const int LabelGap = 22;

    private readonly AuthService _auth;
    private readonly Panel pnlAccent = new();
    private readonly Label lblTitle = new();
    private readonly Label lblCurrentPassword = new();
    private readonly TextBox txtCurrentPassword = new();
    private readonly Button btnToggleCurrent = new();
    private readonly Label lblNewPassword = new();
    private readonly TextBox txtNewPassword = new();
    private readonly Button btnToggleNew = new();
    private readonly Label lblConfirmPassword = new();
    private readonly TextBox txtConfirmPassword = new();
    private readonly Button btnToggleConfirm = new();
    private readonly Button btnSave = new();

    public ChangePasswordForm(AuthService auth)
    {
        _auth = auth;
        BuildChrome();
        AddAccent();
        AddTitle();
        AddFields();
        AddActions();
    }

    private void BuildChrome()
    {
        Text = Messages.AppName;
        Font = new Font("Segoe UI", 10F);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(FormWidth, FormHeight);
        BackColor = Palette.Paper;
        AcceptButton = btnSave;
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
        lblTitle.Text = Messages.ChangePassword;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.ForeColor = Palette.Ink;
        lblTitle.Location = new Point(FieldX, 20);
        lblTitle.AutoSize = true;
        Controls.Add(lblTitle);
    }

    private void AddFields()
    {
        PlaceSecret(lblCurrentPassword, txtCurrentPassword, btnToggleCurrent, Messages.CurrentPassword, 68);
        PlaceSecret(lblNewPassword, txtNewPassword, btnToggleNew, Messages.Password, 136);
        PlaceSecret(lblConfirmPassword, txtConfirmPassword, btnToggleConfirm, Messages.ConfirmPassword, 204);

        txtCurrentPassword.TabIndex = 0;
        txtNewPassword.TabIndex = 1;
        txtConfirmPassword.TabIndex = 2;

        Controls.Add(lblCurrentPassword);
        Controls.Add(txtCurrentPassword);
        Controls.Add(btnToggleCurrent);
        Controls.Add(lblNewPassword);
        Controls.Add(txtNewPassword);
        Controls.Add(btnToggleNew);
        Controls.Add(lblConfirmPassword);
        Controls.Add(txtConfirmPassword);
        Controls.Add(btnToggleConfirm);
    }

    private static void PlaceSecret(Label label, TextBox box, Button toggle, string caption, int top)
    {
        label.Text = caption;
        label.AutoSize = true;
        label.ForeColor = Palette.Ink;
        label.Location = new Point(FieldX, top);

        box.Location = new Point(FieldX, top + LabelGap);
        box.Size = new Size(FieldWidth - 42, 28);
        box.BorderStyle = BorderStyle.FixedSingle;
        box.UseSystemPasswordChar = true;

        toggle.Text = "👁";
        toggle.Location = new Point(FieldX + FieldWidth - 38, top + LabelGap);
        toggle.Size = new Size(38, 28);
        toggle.FlatStyle = FlatStyle.Flat;
        toggle.FlatAppearance.BorderSize = 0;
        toggle.BackColor = Palette.PaperLight;
        toggle.ForeColor = Palette.Ink;
        toggle.Cursor = Cursors.Hand;
        toggle.Click += (_, _) =>
        {
            box.UseSystemPasswordChar = !box.UseSystemPasswordChar;
            toggle.ForeColor = box.UseSystemPasswordChar ? Palette.Ink : Palette.Accent;
        };
    }

    private void AddActions()
    {
        btnSave.Text = Messages.Save;
        btnSave.Location = new Point(FieldX, 280);
        btnSave.Size = new Size(130, 36);
        btnSave.FlatStyle = FlatStyle.Flat;
        btnSave.FlatAppearance.BorderSize = 0;
        btnSave.BackColor = Palette.Accent;
        btnSave.ForeColor = Palette.White;
        btnSave.Cursor = Cursors.Hand;
        btnSave.TabIndex = 3;
        btnSave.Click += SaveClicked;
        Controls.Add(btnSave);
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        var current = txtCurrentPassword.Text;
        var newPass = txtNewPassword.Text;
        var confirm = txtConfirmPassword.Text;

        if (string.IsNullOrWhiteSpace(current))
        {
            ShowWarning(Messages.CurrentPasswordRequired);
            return;
        }

        if (string.IsNullOrWhiteSpace(newPass))
        {
            ShowWarning(Messages.NewPasswordRequired);
            return;
        }

        if (newPass != confirm)
        {
            ShowWarning(Messages.PasswordsDoNotMatch);
            return;
        }

        var user = _auth.CurrentUser();
        if (user is null)
        {
            ShowWarning(Messages.NotSignedIn);
            return;
        }

        var result = _auth.ChangePassword(user.UserId, newPass, confirm);
        if (!result.Success)
        {
            ShowWarning(result.Message);
            return;
        }

        MessageBox.Show(this, Messages.PasswordChanged, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ShowWarning(string message)
    {
        MessageBox.Show(this, message, Messages.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
