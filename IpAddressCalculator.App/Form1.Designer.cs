namespace IpAddressCalculator.App;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null!;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        titleLabel = new Label();
        inputPanel = new TableLayoutPanel();
        addressLabel = new Label();
        addressTextBox = new TextBox();
        prefixLabel = new Label();
        prefixTextBox = new TextBox();
        calculateButton = new Button();
        clearButton = new Button();
        resultGrid = new DataGridView();
        nameColumn = new DataGridViewTextBoxColumn();
        valueColumn = new DataGridViewTextBoxColumn();
        copyColumn = new DataGridViewButtonColumn();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        autoCalculateTimer = new System.Windows.Forms.Timer(components);
        inputPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)resultGrid).BeginInit();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // titleLabel
        // 
        titleLabel.AutoSize = true;
        titleLabel.Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
        titleLabel.Location = new Point(18, 16);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(246, 30);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "IPv4 / IPv6 地址计算器";
        // 
        // inputPanel
        // 
        inputPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        inputPanel.ColumnCount = 6;
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76F));
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        inputPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        inputPanel.Controls.Add(addressLabel, 0, 0);
        inputPanel.Controls.Add(addressTextBox, 1, 0);
        inputPanel.Controls.Add(prefixLabel, 2, 0);
        inputPanel.Controls.Add(prefixTextBox, 3, 0);
        inputPanel.Controls.Add(calculateButton, 4, 0);
        inputPanel.Controls.Add(clearButton, 5, 0);
        inputPanel.Location = new Point(18, 62);
        inputPanel.Name = "inputPanel";
        inputPanel.RowCount = 1;
        inputPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        inputPanel.Size = new Size(948, 38);
        inputPanel.TabIndex = 1;
        // 
        // addressLabel
        // 
        addressLabel.Anchor = AnchorStyles.Left;
        addressLabel.AutoSize = true;
        addressLabel.Location = new Point(3, 9);
        addressLabel.Name = "addressLabel";
        addressLabel.Size = new Size(68, 20);
        addressLabel.TabIndex = 0;
        addressLabel.Text = "地址/CIDR";
        // 
        // addressTextBox
        // 
        addressTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        addressTextBox.Location = new Point(79, 5);
        addressTextBox.Name = "addressTextBox";
        addressTextBox.PlaceholderText = "例如：192.168.1.10/24 或 2001:db8::1/64";
        addressTextBox.Size = new Size(470, 27);
        addressTextBox.TabIndex = 1;
        // 
        // prefixLabel
        // 
        prefixLabel.Anchor = AnchorStyles.Left;
        prefixLabel.AutoSize = true;
        prefixLabel.Location = new Point(555, 9);
        prefixLabel.Name = "prefixLabel";
        prefixLabel.Size = new Size(69, 20);
        prefixLabel.TabIndex = 2;
        prefixLabel.Text = "前缀/掩码";
        // 
        // prefixTextBox
        // 
        prefixTextBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        prefixTextBox.Location = new Point(635, 5);
        prefixTextBox.Name = "prefixTextBox";
        prefixTextBox.PlaceholderText = "24 或 255.255.255.0";
        prefixTextBox.Size = new Size(126, 27);
        prefixTextBox.TabIndex = 3;
        // 
        // calculateButton
        // 
        calculateButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        calculateButton.Location = new Point(767, 3);
        calculateButton.Name = "calculateButton";
        calculateButton.Size = new Size(86, 32);
        calculateButton.TabIndex = 4;
        calculateButton.Text = "计算";
        calculateButton.UseVisualStyleBackColor = true;
        // 
        // clearButton
        // 
        clearButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        clearButton.Location = new Point(859, 3);
        clearButton.Name = "clearButton";
        clearButton.Size = new Size(86, 32);
        clearButton.TabIndex = 5;
        clearButton.Text = "清空";
        clearButton.UseVisualStyleBackColor = true;
        // 
        // resultGrid
        // 
        resultGrid.AllowUserToAddRows = false;
        resultGrid.AllowUserToDeleteRows = false;
        resultGrid.AllowUserToResizeRows = false;
        resultGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        resultGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        resultGrid.BackgroundColor = SystemColors.Window;
        resultGrid.BorderStyle = BorderStyle.Fixed3D;
        resultGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        resultGrid.Columns.AddRange(new DataGridViewColumn[] { nameColumn, valueColumn, copyColumn });
        resultGrid.Location = new Point(18, 118);
        resultGrid.MultiSelect = false;
        resultGrid.Name = "resultGrid";
        resultGrid.ReadOnly = true;
        resultGrid.RowHeadersVisible = false;
        resultGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        resultGrid.Size = new Size(948, 454);
        resultGrid.TabIndex = 2;
        // 
        // nameColumn
        // 
        nameColumn.HeaderText = "字段";
        nameColumn.MinimumWidth = 150;
        nameColumn.Name = "nameColumn";
        nameColumn.ReadOnly = true;
        nameColumn.Width = 180;
        // 
        // valueColumn
        // 
        valueColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        valueColumn.HeaderText = "值";
        valueColumn.MinimumWidth = 360;
        valueColumn.Name = "valueColumn";
        valueColumn.ReadOnly = true;
        // 
        // copyColumn
        // 
        copyColumn.HeaderText = "操作";
        copyColumn.MinimumWidth = 76;
        copyColumn.Name = "copyColumn";
        copyColumn.ReadOnly = true;
        copyColumn.Text = "复制";
        copyColumn.UseColumnTextForButtonValue = true;
        copyColumn.Width = 86;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 590);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(984, 24);
        statusStrip.TabIndex = 3;
        statusStrip.Text = "statusStrip1";
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(189, 19);
        statusLabel.Text = "请输入地址和前缀，然后点击计算。";
        // 
        // autoCalculateTimer
        // 
        autoCalculateTimer.Interval = 450;
        // 
        // Form1
        // 
        AcceptButton = calculateButton;
        AutoScaleDimensions = new SizeF(9F, 20F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(984, 614);
        Controls.Add(statusStrip);
        Controls.Add(resultGrid);
        Controls.Add(inputPanel);
        Controls.Add(titleLabel);
        Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        MinimumSize = new Size(880, 520);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "IPv4 / IPv6 地址计算器";
        inputPanel.ResumeLayout(false);
        inputPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)resultGrid).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label titleLabel;
    private TableLayoutPanel inputPanel;
    private Label addressLabel;
    private TextBox addressTextBox;
    private Label prefixLabel;
    private TextBox prefixTextBox;
    private Button calculateButton;
    private Button clearButton;
    private DataGridView resultGrid;
    private DataGridViewTextBoxColumn nameColumn;
    private DataGridViewTextBoxColumn valueColumn;
    private DataGridViewButtonColumn copyColumn;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private System.Windows.Forms.Timer autoCalculateTimer;
}
