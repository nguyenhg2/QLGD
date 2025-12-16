using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QLGD_WinForm
{
    public class FormNhapMuon : Form
    {
        private TextBox txtMaNguoiMuon, txtHoTen, txtDonVi;
        private ComboBox cboThietBi, cboGiangDuong, cboPhong;
        private DateTimePicker dtpHanTra;
        private Button btnLuu, btnHuy;
        private bool _isNewUser = false;

        public FormNhapMuon()
        {
            InitializeUI();
            LoadDataCombobox();
        }

        #region UI Setup
        private void InitializeUI()
        {
            this.Text = "Đăng Ký Mượn Thiết Bị Mới";
            this.Size = new Size(550, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lblHeader = new Label
            {
                Text = "PHIẾU MƯỢN THIẾT BỊ",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Teal
            };

            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 350,
                ColumnCount = 2,
                Padding = new Padding(20),
                RowCount = 7
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            txtMaNguoiMuon = new TextBox { Height = 30, Font = new Font("Segoe UI", 10), PlaceholderText = "Nhập mã & Enter..." };
            txtMaNguoiMuon.KeyDown += TxtMaNguoiMuon_KeyDown;
            txtMaNguoiMuon.Leave += TxtMaNguoiMuon_Leave;
            AddRow(table, "Mã Người Mượn (*):", txtMaNguoiMuon);

            txtHoTen = new TextBox { Height = 30, Font = new Font("Segoe UI", 10), ReadOnly = true, BackColor = Color.WhiteSmoke };
            AddRow(table, "Họ Tên:", txtHoTen);

            txtDonVi = new TextBox { Height = 30, Font = new Font("Segoe UI", 10), ReadOnly = true, BackColor = Color.WhiteSmoke };
            AddRow(table, "Đơn Vị / Lớp:", txtDonVi);

            cboThietBi = CreateComboBox();
            // Fix lỗi NotSupported: Source trước Mode
            cboThietBi.AutoCompleteSource = AutoCompleteSource.ListItems;
            cboThietBi.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            AddRow(table, "Thiết Bị (Kho) (*):", cboThietBi);

            cboGiangDuong = CreateComboBox();
            cboGiangDuong.SelectedIndexChanged += CboGiangDuong_SelectedIndexChanged;
            AddRow(table, "Tại Giảng Đường (*):", cboGiangDuong);

            cboPhong = CreateComboBox();
            AddRow(table, "Tại Phòng (*):", cboPhong);

            dtpHanTra = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm",
                Width = 250,
                Value = DateTime.Now.AddHours(4)
            };
            AddRow(table, "Hạn Trả Dự Kiến:", dtpHanTra);

            btnLuu = new Button { Text = "MƯỢN NGAY", DialogResult = DialogResult.None, BackColor = Color.Teal, ForeColor = Color.White, Height = 45, Width = 140, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            btnHuy = new Button { Text = "Hủy Bỏ", DialogResult = DialogResult.Cancel, Height = 45, Width = 100 };
            btnLuu.Click += BtnLuu_Click;

            FlowLayoutPanel pnlBtn = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, Height = 80, Padding = new Padding(20) };
            pnlBtn.Controls.Add(btnHuy);
            pnlBtn.Controls.Add(btnLuu);

            this.Controls.Add(table);
            this.Controls.Add(pnlBtn);
            this.Controls.Add(lblHeader);
        }

        private void AddRow(TableLayoutPanel table, string label, Control ctrl)
        {
            Label lbl = new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Font = new Font("Segoe UI", 10) };
            ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            table.Controls.Add(lbl);
            table.Controls.Add(ctrl);
        }

        private ComboBox CreateComboBox()
        {
            return new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Height = 32, Font = new Font("Segoe UI", 10) };
        }
        #endregion

        #region Logic
        private void CheckNguoiMuon()
        {
            string maNM = txtMaNguoiMuon.Text.Trim();
            if (string.IsNullOrEmpty(maNM)) return;

            try
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("sp_GetThongTinNguoiMuon", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaNguoiMuon", maNM);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtHoTen.Text = reader["HoTen"].ToString();
                            txtDonVi.Text = reader["DonVi"].ToString();
                            txtHoTen.ReadOnly = true; txtDonVi.ReadOnly = true;
                            txtHoTen.BackColor = Color.WhiteSmoke; txtDonVi.BackColor = Color.WhiteSmoke;
                            _isNewUser = false;
                        }
                        else
                        {
                            txtHoTen.ReadOnly = false; txtDonVi.ReadOnly = false;
                            txtHoTen.BackColor = Color.White; txtDonVi.BackColor = Color.White;
                            txtHoTen.Text = ""; txtDonVi.Text = "";
                            txtHoTen.Focus();
                            _isNewUser = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kiểm tra người mượn: " + ex.Message);
            }
        }

        private void TxtMaNguoiMuon_Leave(object sender, EventArgs e) => CheckNguoiMuon();
        private void TxtMaNguoiMuon_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CheckNguoiMuon();
                e.SuppressKeyPress = true;
            }
        }

        private void LoadDataCombobox()
        {
            try
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();

                    var dtTB = new DataTable();
                    new SqlDataAdapter("sp_GetThietBiSanSang", conn).Fill(dtTB);
                    cboThietBi.DataSource = dtTB;
                    cboThietBi.DisplayMember = "TenTB";
                    cboThietBi.ValueMember = "MaTB";

                    var dtGD = new DataTable();
                    new SqlDataAdapter("sp_GetAllGiangDuong", conn).Fill(dtGD);

                    cboGiangDuong.SelectedIndexChanged -= CboGiangDuong_SelectedIndexChanged;
                    cboGiangDuong.DisplayMember = "MaGD";
                    cboGiangDuong.ValueMember = "MaGD";
                    cboGiangDuong.DataSource = dtGD;
                    cboGiangDuong.SelectedIndexChanged += CboGiangDuong_SelectedIndexChanged;

                    if (cboGiangDuong.Items.Count > 0)
                    {
                        cboGiangDuong.SelectedIndex = 0;
                        CboGiangDuong_SelectedIndexChanged(null, null);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        private void CboGiangDuong_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboGiangDuong.SelectedValue == null) return;
            string maGD = cboGiangDuong.SelectedValue.ToString();
            if (maGD.Contains("DataRowView")) return;

            try
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("sp_GetPhongByGD", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaGD", maGD);

                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    DataView dv = new DataView(dt);
                    dv.RowFilter = "MaPhong <> '000'";

                    cboPhong.DataSource = dv;
                    cboPhong.DisplayMember = "MaPhong";
                    cboPhong.ValueMember = "MaPhong";
                }
            }
            catch { }
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaNguoiMuon.Text) || string.IsNullOrWhiteSpace(txtHoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã và Tên người mượn!");
                return;
            }

            if (cboThietBi.SelectedValue == null || cboGiangDuong.SelectedValue == null || cboPhong.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn đầy đủ Thiết bị và Vị trí!");
                return;
            }

            using (var conn = new SqlConnection(AppConfig.ConnectionString))
            {
                conn.Open();
                SqlTransaction transaction = conn.BeginTransaction();

                try
                {
                    if (_isNewUser)
                    {
                        string sqlInsertUser = "INSERT INTO NGUOI_MUON (MaNguoiMuon, HoTen, DonVi, TrangThai) VALUES (@Ma, @Ten, @DV, N'Còn công tác')";
                        var cmdUser = new SqlCommand(sqlInsertUser, conn, transaction);
                        cmdUser.Parameters.AddWithValue("@Ma", txtMaNguoiMuon.Text.Trim());
                        cmdUser.Parameters.AddWithValue("@Ten", txtHoTen.Text.Trim());
                        cmdUser.Parameters.AddWithValue("@DV", txtDonVi.Text.Trim());
                        cmdUser.ExecuteNonQuery();
                    }

                    new SqlCommand("ALTER TABLE MUON_TRA NOCHECK CONSTRAINT ALL", conn, transaction).ExecuteNonQuery();

                    var cmd = new SqlCommand("sp_MuonThietBi", conn, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaDK", "PM" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                    cmd.Parameters.AddWithValue("@MaNguoiMuon", txtMaNguoiMuon.Text.Trim());
                    cmd.Parameters.AddWithValue("@MaTB", cboThietBi.SelectedValue);
                    cmd.Parameters.AddWithValue("@MaGD_Muon", cboGiangDuong.SelectedValue);
                    cmd.Parameters.AddWithValue("@MaPhong_Muon", cboPhong.SelectedValue);

                    if (dtpHanTra.Value > DateTime.Now)
                        cmd.Parameters.AddWithValue("@TGTraDuKien", dtpHanTra.Value);
                    else
                        cmd.Parameters.AddWithValue("@TGTraDuKien", DBNull.Value);

                    cmd.ExecuteNonQuery();

                    new SqlCommand("ALTER TABLE MUON_TRA WITH CHECK CHECK CONSTRAINT ALL", conn, transaction).ExecuteNonQuery();

                    transaction.Commit();
                    MessageBox.Show("Mượn thiết bị thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    try
                    {
                        using (var cn2 = new SqlConnection(AppConfig.ConnectionString))
                        {
                            cn2.Open();
                            new SqlCommand("ALTER TABLE MUON_TRA WITH CHECK CHECK CONSTRAINT ALL", cn2).ExecuteNonQuery();
                        }
                    }
                    catch { }

                    MessageBox.Show("Lỗi thực hiện: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion
    }
}