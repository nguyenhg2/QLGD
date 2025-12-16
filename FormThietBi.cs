using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QLGD_WinForm
{
    public class FormThietBi : BaseManagementForm
    {
        private ComboBox cboGiangDuong;
        private TextBox txtTimKiemTB;

        public FormThietBi() : base("Quản Lý Thiết Bị Theo Giảng Đường", "ThietBi", new Size(1300, 750))
        {
            InitializeCustomToolbar();
            LoadComboboxGD();
        }

        #region UI Setup
        private void InitializeCustomToolbar()
        {
            pnlTop.Controls.Clear();

            Label lblChon = new Label
            {
                Text = "Chọn Giảng Đường:",
                Location = new Point(20, 23),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            cboGiangDuong = new ComboBox
            {
                Location = new Point(170, 20),
                Width = 150,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 11),
                Cursor = Cursors.Hand
            };

            Label lblTim = new Label
            {
                Text = "Tìm tên/mã TB:",
                Location = new Point(350, 23),
                AutoSize = true,
                Font = new Font("Segoe UI", 10)
            };
            txtTimKiemTB = new TextBox
            {
                Location = new Point(460, 20),
                Width = 200,
                Font = new Font("Segoe UI", 10)
            };

            Button btnAdd = new Button { Text = "Thêm TB", BackColor = Color.Teal, ForeColor = Color.White };
            Button btnEdit = new Button { Text = "Sửa" };
            Button btnDel = new Button { Text = "Xóa", BackColor = Color.IndianRed, ForeColor = Color.White };

            btnAdd.Location = new Point(700, 18); btnAdd.Size = new Size(100, 32);
            btnEdit.Location = new Point(810, 18); btnEdit.Size = new Size(80, 32);
            btnDel.Location = new Point(900, 18); btnDel.Size = new Size(80, 32);

            pnlTop.Controls.AddRange(new Control[] { lblChon, cboGiangDuong, lblTim, txtTimKiemTB, btnAdd, btnEdit, btnDel });

            cboGiangDuong.SelectedIndexChanged += (s, e) => LoadData();
            txtTimKiemTB.TextChanged += (s, e) => LoadData();

            btnAdd.Click += (s, e) => { new FormNhapThietBi().ShowDialog(); LoadData(); };
            btnEdit.Click += (s, e) => EditSelected();
            btnDel.Click += (s, e) => DeleteSelected();
        }
        #endregion

        #region Data Loading
        private void LoadComboboxGD()
        {
            try
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("sp_GetAllGiangDuong", conn);
                    var da = new SqlDataAdapter(cmd);
                    var dt = new DataTable();
                    da.Fill(dt);

                    cboGiangDuong.DisplayMember = "MaGD";
                    cboGiangDuong.ValueMember = "MaGD";
                    cboGiangDuong.DataSource = dt;

                    if (cboGiangDuong.Items.Count > 0)
                        cboGiangDuong.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load danh sách Giảng đường: " + ex.Message);
            }
        }

        protected override void LoadData(string search = "")
        {
            if (cboGiangDuong.SelectedValue == null) return;

            string maGD = cboGiangDuong.SelectedValue.ToString();
            string tuKhoa = txtTimKiemTB.Text.Trim();

            try
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand("sp_GetThietBiByGD", conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@MaGD", maGD);

                    var dt = new DataTable();
                    new SqlDataAdapter(cmd).Fill(dt);

                    if (!string.IsNullOrEmpty(tuKhoa))
                    {
                        dt.DefaultView.RowFilter = $"[Tên Thiết Bị] LIKE '%{tuKhoa}%' OR [Mã TB] LIKE '%{tuKhoa}%'";
                        dgvMain.DataSource = dt.DefaultView;
                    }
                    else
                    {
                        dgvMain.DataSource = dt;
                    }

                    ConfigureGridColumns();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách thiết bị: " + ex.Message);
            }
        }

        private void ConfigureGridColumns()
        {
            if (dgvMain.Columns.Count == 0) return;

            dgvMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            dgvMain.EnableHeadersVisualStyles = false;
            dgvMain.ColumnHeadersHeight = 45;
            dgvMain.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvMain.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            void SetCol(string dbCol, string headerText, int width, DataGridViewContentAlignment align = DataGridViewContentAlignment.MiddleLeft)
            {
                if (dgvMain.Columns.Contains(dbCol))
                {
                    dgvMain.Columns[dbCol].HeaderText = headerText;
                    dgvMain.Columns[dbCol].Width = width;
                    dgvMain.Columns[dbCol].DefaultCellStyle.Alignment = align;
                }
            }

            SetCol("Mã TB", "Mã Thiết Bị", 120, DataGridViewContentAlignment.MiddleCenter);
            SetCol("Tên Thiết Bị", "Tên Thiết Bị", 450);
            SetCol("Loại", "Loại Thiết Bị", 250);
            SetCol("Trạng Thái", "Trạng Thái", 150, DataGridViewContentAlignment.MiddleCenter);
            SetCol("Phòng", "Phòng", 120, DataGridViewContentAlignment.MiddleCenter);
            SetCol("Vị Trí", "Vị Trí Chi Tiết", 150);

            foreach (DataGridViewRow row in dgvMain.Rows)
            {
                string status = row.Cells["Trạng Thái"].Value?.ToString();
                if (status == "Hỏng")
                {
                    row.Cells["Trạng Thái"].Style.ForeColor = Color.Red;
                    row.Cells["Trạng Thái"].Style.Font = new Font(dgvMain.Font, FontStyle.Bold);
                }
                else if (status == "Tốt")
                {
                    row.Cells["Trạng Thái"].Style.ForeColor = Color.Green;
                }
            }
        }
        #endregion

        #region Actions
        protected override void EditSelected()
        {
            if (dgvMain.CurrentRow == null) return;
            string ma = dgvMain.CurrentRow.Cells["Mã TB"].Value.ToString();
            new FormNhapThietBi(ma).ShowDialog();
            LoadData();
        }

        protected override void DeleteSelected()
        {
            if (dgvMain.CurrentRow == null) return;
            string maTB = dgvMain.CurrentRow.Cells["Mã TB"].Value.ToString();

            if (MessageBox.Show($"Xóa thiết bị {maTB}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    using (var conn = new SqlConnection(AppConfig.ConnectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM THIET_BI WHERE MaTB = @id";
                        var cmd = new SqlCommand(sql, conn);
                        cmd.Parameters.AddWithValue("@id", maTB);
                        cmd.ExecuteNonQuery();
                        LoadData();
                    }
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }
        #endregion
    }
}