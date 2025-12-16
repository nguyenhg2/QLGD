using Microsoft.Data.SqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace QLGD_WinForm
{
    public class FormChiTietMuonTra : Form
    {
        private string _maDK;

        public FormChiTietMuonTra(string maDK)
        {
            _maDK = maDK;
            InitializeUI();
            LoadDetail();
        }

        #region UI Setup
        private void InitializeUI()
        {
            this.Text = "Chi Tiết Phiếu Mượn: " + _maDK;
            this.Size = new Size(650, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblHeader = new Label
            {
                Text = "THÔNG TIN CHI TIẾT PHIẾU MƯỢN",
                Dock = DockStyle.Top,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.Teal
            };
            this.Controls.Add(lblHeader);

            TableLayoutPanel table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 400,
                ColumnCount = 2,
                Padding = new Padding(40, 10, 40, 0),
                RowCount = 9
            };

            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));

            AddRow(table, "Mã Phiếu Mượn:", "lblMaDK");
            AddRow(table, "Họ Tên Người Mượn:", "lblNguoiMuon");
            AddRow(table, "Đơn Vị / Lớp:", "lblDonVi");
            AddRow(table, "Tên Thiết Bị:", "lblThietBi");
            AddRow(table, "Thời Gian Mượn:", "lblNgayMuon");
            AddRow(table, "Thời Gian Trả Dự Kiến:", "lblTraDuKien", Color.DarkBlue);
            AddRow(table, "Thời Gian Trả Thực Tế:", "lblTraThucTe", Color.DarkRed);
            AddRow(table, "Trạng Thái Phiếu:", "lblTrangThai");
            AddRow(table, "Ghi Chú:", "lblGhiChu");

            this.Controls.Add(table);

            Button btnClose = new Button { Text = "Đóng", Size = new Size(120, 40), Location = new Point(260, 460) };
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private void AddRow(TableLayoutPanel panel, string title, string name, Color? color = null)
        {
            Label lbl = new Label
            {
                Text = title,
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Anchor = AnchorStyles.Left
            };

            Label val = new Label
            {
                Name = name,
                Text = "...",
                AutoSize = true,
                Font = new Font("Segoe UI", 11),
                Anchor = AnchorStyles.Left
            };

            if (color.HasValue) val.ForeColor = color.Value;

            panel.Controls.Add(lbl);
            panel.Controls.Add(val);
        }
        #endregion

        #region Data Logic
        private void LoadDetail()
        {
            using (var conn = new SqlConnection(AppConfig.ConnectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT mt.MaDK, nm.HoTen, nm.DonVi, tb.TenTB, 
                           mt.TGMuon, mt.TGTraDuKien, mt.TGTra, mt.GhiChu,
                           CASE WHEN mt.TGTra IS NULL AND GETDATE() > mt.TGTraDuKien THEN N'QUÁ HẠN' 
                                WHEN mt.TGTra IS NULL THEN N'Đang mượn' 
                                ELSE N'Đã hoàn thành' END as TrangThai
                    FROM MUON_TRA mt
                    JOIN NGUOI_MUON nm ON mt.MaNguoiMuon = nm.MaNguoiMuon
                    JOIN THIET_BI tb ON mt.MaTB = tb.MaTB
                    WHERE mt.MaDK = @id";

                var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", _maDK);

                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        string fmt = "dd/MM/yyyy HH:mm";

                        SetText("lblMaDK", dr["MaDK"].ToString());
                        SetText("lblNguoiMuon", dr["HoTen"].ToString());
                        SetText("lblDonVi", dr["DonVi"].ToString());
                        SetText("lblThietBi", dr["TenTB"].ToString());
                        SetText("lblNgayMuon", Convert.ToDateTime(dr["TGMuon"]).ToString(fmt));

                        if (dr["TGTraDuKien"] != DBNull.Value)
                            SetText("lblTraDuKien", Convert.ToDateTime(dr["TGTraDuKien"]).ToString(fmt));
                        else
                            SetText("lblTraDuKien", "Chưa xác định");

                        if (dr["TGTra"] != DBNull.Value)
                            SetText("lblTraThucTe", Convert.ToDateTime(dr["TGTra"]).ToString(fmt));
                        else
                            SetText("lblTraThucTe", "Chưa trả");

                        SetText("lblGhiChu", dr["GhiChu"].ToString());

                        var lblStt = this.Controls.Find("lblTrangThai", true)[0];
                        lblStt.Text = dr["TrangThai"].ToString();
                        lblStt.ForeColor = lblStt.Text == "QUÁ HẠN" ? Color.Red : Color.Green;
                    }
                }
            }
        }

        private void SetText(string key, string value)
        {
            var c = this.Controls.Find(key, true);
            if (c.Length > 0) c[0].Text = value;
        }
        #endregion
    }
}