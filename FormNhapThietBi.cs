using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace QLGD_WinForm
{
    public class FormNhapThietBi : Form
    {
        private TextBox txtMaTB, txtTenTB, txtGiaTri;
        private ComboBox cboLoai, cboPhong;
        private Button btnLuu, btnHuy;
        private string _maTB = null;

        public FormNhapThietBi(string maTB = null)
        {
            _maTB = maTB;
            InitializeUI();
            LoadComboboxes();
            if (_maTB != null) LoadDataDetail();
        }

        #region UI Setup
        private void InitializeUI()
        {
            this.Text = _maTB == null ? "Thêm Thiết Bị Mới" : "Cập Nhật Thiết Bị";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;

            int y = 20;
            void AddControl(string label, Control ctrl)
            {
                Controls.Add(new Label { Text = label, Location = new Point(30, y), AutoSize = true });
                ctrl.Location = new Point(150, y - 3); ctrl.Width = 250;
                Controls.Add(ctrl);
                y += 50;
            }

            txtMaTB = new TextBox();
            if (_maTB != null) txtMaTB.ReadOnly = true;
            AddControl("Mã TB (*):", txtMaTB);

            txtTenTB = new TextBox();
            AddControl("Tên TB:", txtTenTB);

            cboLoai = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddControl("Loại TB:", cboLoai);

            cboPhong = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            AddControl("Vị Trí (Phòng):", cboPhong);

            txtGiaTri = new TextBox();
            AddControl("Giá Trị (VNĐ):", txtGiaTri);

            btnLuu = new Button { Text = "Lưu", Location = new Point(150, y), Size = new Size(100, 35), BackColor = Color.Teal, ForeColor = Color.White };
            btnHuy = new Button { Text = "Hủy", Location = new Point(270, y), Size = new Size(100, 35) };

            btnLuu.Click += BtnLuu_Click;
            btnHuy.Click += (s, e) => this.Close();

            Controls.AddRange(new Control[] { btnLuu, btnHuy });
        }
        #endregion

        #region Data Logic
        private void LoadComboboxes()
        {
            using (var conn = new SqlConnection(AppConfig.ConnectionString))
            {
                conn.Open();

                var daLoai = new SqlDataAdapter("SELECT MaLoai, TenLoai FROM LOAI_THIET_BI", conn);
                var dtLoai = new DataTable(); daLoai.Fill(dtLoai);
                cboLoai.DataSource = dtLoai; cboLoai.DisplayMember = "TenLoai"; cboLoai.ValueMember = "MaLoai";

                var daPhong = new SqlDataAdapter("SELECT MaGD, MaPhong, (MaGD + ' - ' + MaPhong) as TenHienThi FROM GIANG_DUONG", conn);
                var dtPhong = new DataTable(); daPhong.Fill(dtPhong);
                cboPhong.DataSource = dtPhong;
                cboPhong.DisplayMember = "TenHienThi";
            }
        }

        private void LoadDataDetail()
        {
            using (var conn = new SqlConnection(AppConfig.ConnectionString))
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM THIET_BI WHERE MaTB = @id", conn);
                cmd.Parameters.AddWithValue("@id", _maTB);
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        txtMaTB.Text = dr["MaTB"].ToString();
                        txtTenTB.Text = dr["TenTB"].ToString();
                        txtGiaTri.Text = dr["GiaTri"].ToString();
                        cboLoai.SelectedValue = dr["MaLoai"].ToString();

                        string maGD = dr["MaGD"].ToString();
                        string maPhong = dr["MaPhong"].ToString();
                        foreach (DataRowView item in cboPhong.Items)
                        {
                            if (item["MaGD"].ToString() == maGD && item["MaPhong"].ToString() == maPhong)
                            {
                                cboPhong.SelectedItem = item;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void BtnLuu_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMaTB.Text) || string.IsNullOrWhiteSpace(txtTenTB.Text))
            {
                MessageBox.Show("Vui lòng nhập Mã và Tên thiết bị!");
                return;
            }

            try
            {
                using (var conn = new SqlConnection(AppConfig.ConnectionString))
                {
                    conn.Open();
                    string sql;
                    if (_maTB == null)
                    {
                        sql = @"INSERT INTO THIET_BI (MaTB, TenTB, MaLoai, MaGD, MaPhong, GiaTri, TrangThai) 
                                VALUES (@MaTB, @TenTB, @MaLoai, @MaGD, @MaPhong, @GiaTri, N'Tốt')";
                    }
                    else
                    {
                        sql = @"UPDATE THIET_BI SET TenTB=@TenTB, MaLoai=@MaLoai, MaGD=@MaGD, MaPhong=@MaPhong, GiaTri=@GiaTri 
                                WHERE MaTB=@MaTB";
                    }

                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@MaTB", txtMaTB.Text);
                    cmd.Parameters.AddWithValue("@TenTB", txtTenTB.Text);
                    cmd.Parameters.AddWithValue("@MaLoai", cboLoai.SelectedValue);

                    DataRowView drv = (DataRowView)cboPhong.SelectedItem;
                    cmd.Parameters.AddWithValue("@MaGD", drv["MaGD"]);
                    cmd.Parameters.AddWithValue("@MaPhong", drv["MaPhong"]);

                    decimal giaTri = 0;
                    decimal.TryParse(txtGiaTri.Text, out giaTri);
                    cmd.Parameters.AddWithValue("@GiaTri", giaTri);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Lưu thành công!");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
        #endregion
    }
}