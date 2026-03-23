namespace WorkTimer;

internal sealed class OptionsForm : Form
{
    private readonly ComboBox _keyCombo;
    private readonly NumericUpDown _intervalInput;

    public string SelectedKey => (string)_keyCombo.SelectedItem!;
    public int SelectedInterval => (int)_intervalInput.Value;

    public OptionsForm(Settings settings)
    {
        Text = "WorkTimer Options";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(320, 200);
        ShowIcon = false;

        var keyLabel = new Label
        {
            Text = "Simulated key:",
            Location = new Point(20, 22),
            AutoSize = true,
        };

        _keyCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(150, 18),
            Width = 120,
        };
        _keyCombo.Items.AddRange([.. Settings.AvailableKeys.Keys]);
        _keyCombo.SelectedItem = settings.VirtualKey;

        var intervalLabel = new Label
        {
            Text = "Key press interval (s):",
            Location = new Point(20, 62),
            AutoSize = true,
        };

        _intervalInput = new NumericUpDown
        {
            Minimum = 10,
            Maximum = 600,
            Value = settings.IntervalSeconds,
            Location = new Point(150, 58),
            Width = 120,
        };

        var okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(80, 115),
            Size = new Size(75, 28),
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(165, 115),
            Size = new Size(75, 28),
        };

        AcceptButton = okButton;
        CancelButton = cancelButton;

        Controls.AddRange([keyLabel, _keyCombo, intervalLabel, _intervalInput, okButton, cancelButton]);
    }
}
