namespace IpAddressCalculator.App;

using System.Globalization;
using IpAddressCalculator.Core;

public partial class Form1 : Form
{
    private TabControl tabControl = null!;
    private TextBox parentNetworkTextBox = null!;
    private TextBox splitPrefixTextBox = null!;
    private TextBox splitLimitTextBox = null!;
    private Button divideButton = null!;
    private Button clearDivisionButton = null!;
    private DataGridView subnetGrid = null!;
    private DataGridViewTextBoxColumn subnetCidrColumn = null!;
    private DataGridViewButtonColumn subnetCopyColumn = null!;

    public Form1()
    {
        InitializeComponent();
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? Icon;
        BuildTabbedLayout();
        WireEvents();
        addressTextBox.Text = "192.168.1.10/24";
        parentNetworkTextBox.Text = "192.168.1.0/24";
        splitPrefixTextBox.Text = "26";
        splitLimitTextBox.Text = "128";
        Calculate(showErrors: true);
        DivideSubnets(showErrors: true);
    }

    private void WireEvents()
    {
        calculateButton.Click += (_, _) => Calculate(showErrors: true);
        clearButton.Click += (_, _) => ClearForm();
        divideButton.Click += (_, _) => DivideSubnets(showErrors: true);
        clearDivisionButton.Click += (_, _) => ClearDivisionForm();
        resultGrid.CellContentClick += ResultGrid_CellContentClick;
        subnetGrid.CellContentClick += SubnetGrid_CellContentClick;
        addressTextBox.TextChanged += (_, _) => ScheduleAutoCalculate();
        prefixTextBox.TextChanged += (_, _) => ScheduleAutoCalculate();
        autoCalculateTimer.Tick += (_, _) =>
        {
            autoCalculateTimer.Stop();
            Calculate(showErrors: false);
        };
    }

    private void BuildTabbedLayout()
    {
        Controls.Remove(inputPanel);
        Controls.Remove(resultGrid);

        tabControl = new TabControl
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            Location = new Point(18, 58),
            Name = "tabControl",
            Size = new Size(ClientSize.Width - 36, ClientSize.Height - 92),
            TabIndex = 1
        };

        var calculateTab = new TabPage("地址计算");
        var divideTab = new TabPage("子网划分");

        inputPanel.Location = new Point(12, 14);
        inputPanel.Size = new Size(calculateTab.ClientSize.Width - 24, inputPanel.Height);
        inputPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        resultGrid.Location = new Point(12, 64);
        resultGrid.Size = new Size(calculateTab.ClientSize.Width - 24, calculateTab.ClientSize.Height - 76);
        resultGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

        calculateTab.Controls.Add(inputPanel);
        calculateTab.Controls.Add(resultGrid);

        BuildDivisionTab(divideTab);

        tabControl.TabPages.Add(calculateTab);
        tabControl.TabPages.Add(divideTab);
        Controls.Add(tabControl);
        tabControl.BringToFront();
    }

    private void BuildDivisionTab(TabPage divideTab)
    {
        var splitInputPanel = new TableLayoutPanel
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
            ColumnCount = 7,
            Location = new Point(12, 14),
            Name = "splitInputPanel",
            RowCount = 2,
            Size = new Size(divideTab.ClientSize.Width - 24, 84)
        };
        splitInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72F));
        splitInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
        splitInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72F));
        splitInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        splitInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        splitInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        splitInputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        splitInputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        splitInputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));

        parentNetworkTextBox = new TextBox
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            PlaceholderText = "例如：192.168.1.0/24 或 2001:db8::/48"
        };
        splitPrefixTextBox = new TextBox
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            PlaceholderText = "26"
        };
        splitLimitTextBox = new TextBox
        {
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            PlaceholderText = "128"
        };
        divideButton = new Button
        {
            Anchor = AnchorStyles.Left,
            Size = new Size(86, 32),
            Text = "划分",
            UseVisualStyleBackColor = true
        };
        clearDivisionButton = new Button
        {
            Anchor = AnchorStyles.Left,
            Size = new Size(86, 32),
            Text = "清空",
            UseVisualStyleBackColor = true
        };

        splitInputPanel.Controls.Add(CreateInputLabel("母网"), 0, 0);
        splitInputPanel.Controls.Add(parentNetworkTextBox, 1, 0);
        splitInputPanel.SetColumnSpan(parentNetworkTextBox, 6);
        splitInputPanel.Controls.Add(CreateInputLabel("新前缀"), 0, 1);
        splitInputPanel.Controls.Add(splitPrefixTextBox, 1, 1);
        splitInputPanel.Controls.Add(CreateInputLabel("显示数"), 2, 1);
        splitInputPanel.Controls.Add(splitLimitTextBox, 3, 1);
        splitInputPanel.Controls.Add(divideButton, 4, 1);
        splitInputPanel.Controls.Add(clearDivisionButton, 5, 1);

        subnetCidrColumn = new DataGridViewTextBoxColumn
        {
            HeaderText = "子网",
            Name = "subnetCidrColumn",
            ReadOnly = true,
            Width = 170
        };
        subnetCopyColumn = new DataGridViewButtonColumn
        {
            HeaderText = "操作",
            Name = "subnetCopyColumn",
            ReadOnly = true,
            Text = "复制",
            UseColumnTextForButtonValue = true,
            Width = 76
        };

        subnetGrid = new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.Fixed3D,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            Location = new Point(12, 112),
            MultiSelect = false,
            Name = "subnetGrid",
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            Size = new Size(divideTab.ClientSize.Width - 24, divideTab.ClientSize.Height - 124),
            TabIndex = 1
        };

        subnetGrid.Columns.AddRange(
            new DataGridViewTextBoxColumn { HeaderText = "序号", ReadOnly = true, Width = 58 },
            subnetCidrColumn,
            new DataGridViewTextBoxColumn { HeaderText = "网络/起始地址", ReadOnly = true, Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = "广播/末地址", ReadOnly = true, Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = "首个可用", ReadOnly = true, Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = "最后可用", ReadOnly = true, Width = 180 },
            new DataGridViewTextBoxColumn { HeaderText = "地址数", ReadOnly = true, Width = 130 },
            new DataGridViewTextBoxColumn { HeaderText = "可用主机", ReadOnly = true, Width = 120 },
            subnetCopyColumn);

        divideTab.Controls.Add(splitInputPanel);
        divideTab.Controls.Add(subnetGrid);
    }

    private static Label CreateInputLabel(string text)
    {
        return new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Text = text
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

    private void DivideSubnets(bool showErrors)
    {
        try
        {
            var maxRows = ParseMaxRows();
            var result = SubnetCalculator.DivideSubnets(
                parentNetworkTextBox.Text,
                null,
                splitPrefixTextBox.Text,
                maxRows);

            subnetGrid.Rows.Clear();
            foreach (var row in result.Rows)
            {
                subnetGrid.Rows.Add(
                    row.Index.ToString(CultureInfo.InvariantCulture),
                    row.Cidr,
                    row.NetworkAddress,
                    result.Kind == IpAddressKind.IPv4 ? row.BroadcastAddress : row.LastAddress,
                    row.FirstUsableAddress,
                    row.LastUsableAddress,
                    row.TotalAddresses,
                    row.UsableHosts,
                    "复制");
            }

            statusLabel.ForeColor = SystemColors.ControlText;
            statusLabel.Text = result.IsTruncated
                ? $"子网划分完成：{result.ParentCidr} -> /{result.NewPrefixLength}，共 {result.TotalSubnets} 个，当前显示前 {result.Rows.Count} 个。"
                : $"子网划分完成：{result.ParentCidr} -> /{result.NewPrefixLength}，共 {result.TotalSubnets} 个。";
        }
        catch (FormatException ex)
        {
            if (showErrors)
            {
                subnetGrid.Rows.Clear();
                statusLabel.ForeColor = Color.Firebrick;
                statusLabel.Text = ex.Message;
            }
        }
    }

    private int ParseMaxRows()
    {
        if (!int.TryParse(splitLimitTextBox.Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var maxRows))
        {
            throw new FormatException("显示数必须是 1-4096 之间的整数。");
        }

        return maxRows;
    }

    private void ClearDivisionForm()
    {
        parentNetworkTextBox.Clear();
        splitPrefixTextBox.Clear();
        splitLimitTextBox.Text = "128";
        subnetGrid.Rows.Clear();
        statusLabel.ForeColor = SystemColors.ControlText;
        statusLabel.Text = "请输入母网和新前缀，然后点击划分。";
        parentNetworkTextBox.Focus();
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

    private void SubnetGrid_CellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != subnetCopyColumn.Index)
        {
            return;
        }

        var value = subnetGrid.Rows[e.RowIndex].Cells[subnetCidrColumn.Index].Value?.ToString();
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        Clipboard.SetText(value);
        statusLabel.ForeColor = SystemColors.ControlText;
        statusLabel.Text = $"已复制子网：{value}";
    }
}
