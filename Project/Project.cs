using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace Project
{
    public partial class Project : Form
    {
        private string connectionString = "Data Source=ZOHANUBIS;Initial Catalog=QL_TraHangLazda;Integrated Security=True";

        public Project()
        {
            InitializeComponent();
        }

        private void btnFind_Click(object sender, EventArgs e)
        {
            string maKhachHang = txtIDKH.Text;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string queryKhachHang = "SELECT Ten, SoDienThoai, DiaChi FROM KhachHang WHERE MaKhachHang = @MaKhachHang";
                string querySanPham = "SELECT * FROM ChiTietHoaDon WHERE MaDonHang IN (SELECT MaDonHang FROM ThongTinDonHang WHERE MaKhachHang = @MaKhachHang)";

                using (SqlCommand commandKhachHang = new SqlCommand(queryKhachHang, connection))
                using (SqlCommand commandSanPham = new SqlCommand(querySanPham, connection))
                {
                    commandKhachHang.Parameters.AddWithValue("@MaKhachHang", maKhachHang);
                    commandSanPham.Parameters.AddWithValue("@MaKhachHang", maKhachHang);

                    DataTable dataTable = new DataTable();

                    using (SqlDataAdapter adapter = new SqlDataAdapter(commandSanPham))
                    {
                        adapter.Fill(dataTable);
                    }

                    dataGridViewSP.DataSource = dataTable;

                    using (SqlDataReader reader = commandKhachHang.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtNameKH.Text = reader["Ten"].ToString();
                            txtPhone.Text = reader["SoDienThoai"].ToString();
                            txtAddress.Text = reader["DiaChi"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy thông tin cho mã khách hàng đã nhập.");
                        }
                    }
                }

                connection.Close();
            }
        }

        private void dataGridViewSP_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dataGridViewSP.Rows[e.RowIndex];
                string maSanPham = row.Cells["MaSanPham"].Value.ToString();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Lấy thông tin ngày bán và mã cửa hàng từ bảng ThongTinDonHang dựa trên MaSanPham
                    string queryDonHang = "SELECT ThongTinDonHang.NgayBan, CuaHang.TenCuaHang " +
                                           "FROM ThongTinDonHang " +
                                           "INNER JOIN CuaHang ON ThongTinDonHang.MaCuaHang = CuaHang.MaCuaHang " +
                                           "WHERE ThongTinDonHang.MaDonHang = (SELECT MaDonHang FROM ChiTietHoaDon WHERE MaSanPham = @MaSanPham)";
                    SqlCommand commandDonHang = new SqlCommand(queryDonHang, connection);
                    commandDonHang.Parameters.AddWithValue("@MaSanPham", maSanPham);

                    using (SqlDataReader reader = commandDonHang.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtNgayBanHang.Text = reader["NgayBan"].ToString();
                            txtShopDaMua.Text = reader["TenCuaHang"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy thông tin cho sản phẩm đã chọn.");
                        }
                    }

                    // Tính tổng tiền thanh toán (TienDaThanhToan) dựa trên MaDonHang
                    string queryTongTien = "SELECT SUM(SanPham.Gia * ChiTietHoaDon.SoLuong) AS TienDaThanhToan " +
                                           "FROM ChiTietHoaDon " +
                                           "INNER JOIN SanPham ON ChiTietHoaDon.MaSanPham = SanPham.MaSanPham " +
                                           "WHERE ChiTietHoaDon.MaDonHang = (SELECT MaDonHang FROM ChiTietHoaDon WHERE MaSanPham = @MaSanPham)";
                    SqlCommand commandTongTien = new SqlCommand(queryTongTien, connection);
                    commandTongTien.Parameters.AddWithValue("@MaSanPham", maSanPham);

                    using (SqlDataReader reader = commandTongTien.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtTienDaThanhToan.Text = reader["TienDaThanhToan"].ToString();
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy thông tin tổng tiền thanh toán cho sản phẩm đã chọn.");
                        }
                    }

                    connection.Close();
                }
            }
        }
    }
}
