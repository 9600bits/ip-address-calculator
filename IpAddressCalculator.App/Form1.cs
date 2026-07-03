namespace IpAddressCalculator.App;

using IpAddressCalculator.Core;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? Icon;
        WireEvents();
        addressTextBox.Text = "192.168.1.10/24";
        Calculate(showErrors: true);
    }

    private void WireEvents()
    {
        calculateButton.Click += (_, _) => Calculate(showErrors: true);
        clearButton.Click += (_, _) => ClearForm();
        resultGrid.CellContentClick += ResultGrid_CellContentClick;
        addressTextBox.TextChanged += (_, _) => ScheduleAutoCalculate();
        prefixTextBox.TextChanged += (_, _) => ScheduleAutoCalculate();
        autoCalculateTimer.Tick += (_, _) =>
        {
            autoCalculateTimer.Stop();
            Calculate(showErrors: false);
        };
    }

    private void ScheduleAutoCalculate()
    {
        autoCalculateTimer.Stop();
        if (!string.IsNullOrWhiteSpace(addressTextBox.Text))
        {
            autoCalculateTimer.Start();
        }
    }

    private void Calculate(bool showErrors)
    {
        try
        {
            var result = SubnetCalculator.Calculate(addressTextBox.Text, prefixTextBox.Text);
            resultGrid.Rows.Clear();
            foreach (var row in result.Rows)
            {
                resultGrid.Rows.Add(row.Label, row.Value, "复制");
            }

            statusLabel.ForeColor = SystemColors.ControlText;
            statusLabel.Text = $"计算成功：{result.Kind}，{result.Cidr}";
        }
        catch (FormatException ex)
        {
            if (showErrors)
            {
                resultGrid.Rows.Clear();
                statusLabel.ForeColor = Color.Firebrick;
                statusLabel.Text = ex.Message;
            }
            else
            {
                statusLabel.ForeColor = SystemColors.ControlDarkDark;
                statusLabel.Text = "等待有效输入...";
            }
        }
    }

    private void ClearForm()
    {
        autoCalculateTimer.Stop();
        addressTextBox.Clear();
        prefixTextBox.Clear();
        resultGrid.Rows.Clear();
        statusLabel.ForeColor = SystemColors.ControlText;
        statusLabel.Text = "请输入地址和前缀，然后点击计算。";
        addressTextBox.Focus();
    }

    private void ResultGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != copyColumn.Index)
        {
            return;
        }

        var value = resultGrid.Rows[e.RowIndex].Cells[valueColumn.Index].Value?.ToString();
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        Clipboard.SetText(value);
        statusLabel.ForeColor = SystemColors.ControlText;
        statusLabel.Text = $"已复制：{resultGrid.Rows[e.RowIndex].Cells[nameColumn.Index].Value}";
    }
}
