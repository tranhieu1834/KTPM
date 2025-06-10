using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nhom7.Admin
{
    public partial class themvasua : System.Web.UI.Page
    {
        private string connectionString = "Data Source=DESKTOP-5TBTUUI;Initial Catalog=demosach3;Integrated Security=True";

        protected void Page_Load(object sender, EventArgs e)
        {
            //if (Session["Role"] != null)
            //{
            //    lblMessage.Text = "Session Role: " + Session["Role"].ToString();
            //}
            //else
            //{
            //    lblMessage.Text = "Session Role is null";
            //}
            if (Session["UserID"] == null || Session["Role"].ToString() != "admin")
            {
                Response.Redirect("~/Login.aspx");
            }

            if (!IsPostBack)
            {
                LoadDanhMuc();
                if (!string.IsNullOrEmpty(Request.QueryString["ID_Sach"]))
                {
                    LoadSach(Request.QueryString["ID_Sach"]);
                }
            }
        }

        private void LoadDanhMuc()
        {

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sql = "SELECT ID_DanhMuc, TenDanhMuc FROM DanhMuc";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    ddlDanhMuc.DataSource = reader;
                    ddlDanhMuc.DataTextField = "TenDanhMuc";
                    ddlDanhMuc.DataValueField = "ID_DanhMuc";
                    ddlDanhMuc.DataBind();
                }
            }

            ddlDanhMuc.Items.Insert(0, new ListItem("--Chọn danh mục--", "0"));
        }


        public void LoadSach(string idSach)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Sach WHERE ID_Sach = @ID_Sach", conn);
                cmd.Parameters.AddWithValue("@ID_Sach", idSach);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    txtID.Text = reader["ID_Sach"].ToString();
                    txtTenSach.Text = reader["TenSach"].ToString();
                    txtMoTa.Text = reader["MoTa"].ToString();
                    txtGia.Text = reader["Gia"].ToString();
                    txtSoLuongTon.Text = reader["SoLuongTon"].ToString();
                    txtNgayXuatBan.Text = Convert.ToDateTime(reader["NgayXuatBan"]).ToString("yyyy-MM-dd");
                    ddlDanhMuc.SelectedValue = reader["ID_DanhMuc"].ToString();

                    // Load the image if it exists
                    if (!string.IsNullOrEmpty(reader["Anh"].ToString()))
                    {
                        imgPreview.ImageUrl = "~/image/" + reader["Anh"].ToString();
                    }
                }
            }
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {

            List<string> errors = new List<string>();

            // Kiểm tra từng trường cụ thể
            if (string.IsNullOrEmpty(txtTenSach.Text))
                errors.Add("⚠️ Tên sách không được để trống.");
                txtTenSach.BorderColor = System.Drawing.Color.Red;

            if (string.IsNullOrEmpty(txtMoTa.Text))
                errors.Add("⚠️ Mô tả sách không được để trống.");
                txtMoTa.BorderColor = System.Drawing.Color.Red;

            if (string.IsNullOrEmpty(txtGia.Text) || !decimal.TryParse(txtGia.Text, out _)) { 
                errors.Add("⚠️ Giá sách phải là một số hợp lệ.");
                txtGia.BorderColor = System.Drawing.Color.Red;
            }

            if (string.IsNullOrEmpty(txtSoLuongTon.Text) || !int.TryParse(txtSoLuongTon.Text, out _)) { 
                errors.Add("⚠️ Số lượng tồn phải là một số nguyên hợp lệ.");
                txtSoLuongTon.BorderColor = System.Drawing.Color.Red;
            }

            if (string.IsNullOrEmpty(txtNgayXuatBan.Text) || !DateTime.TryParse(txtNgayXuatBan.Text, out _)) { 

                errors.Add("⚠️ Ngày xuất bản không đúng định dạng.");
                txtNgayXuatBan.BorderColor = System.Drawing.Color.Red;
            }

            if (ddlDanhMuc.SelectedIndex == 0)
            {
                errors.Add("⚠️ Vui lòng chọn danh mục.");
                
            }

            // Nếu có lỗi, hiển thị toàn bộ danh sách
            if (errors.Count > 0)
            {
                lblMessage.Text = string.Join("<br>", errors);
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            try
            {
                decimal gia = Convert.ToDecimal(txtGia.Text);
                int soLuongTon = Convert.ToInt32(txtSoLuongTon.Text);
                DateTime ngayXuatBan = Convert.ToDateTime(txtNgayXuatBan.Text);

                string fileName = null;
                if (fileUpload.HasFile)
                {
                    fileName = Path.GetFileName(fileUpload.PostedFile.FileName);
                    string filePath = Server.MapPath("~/image/") + fileName;
                    fileUpload.SaveAs(filePath);
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = string.IsNullOrEmpty(txtID.Text)
                        ? "INSERT INTO Sach (TenSach, MoTa, Gia, SoLuongTon, NgayXuatBan, ID_DanhMuc, Anh) VALUES (@TenSach, @MoTa, @Gia, @SoLuongTon, @NgayXuatBan, @ID_DanhMuc, @Anh)"
                        : "UPDATE Sach SET TenSach=@TenSach, MoTa=@MoTa, Gia=@Gia, SoLuongTon=@SoLuongTon, NgayXuatBan=@NgayXuatBan, ID_DanhMuc=@ID_DanhMuc, Anh=@Anh WHERE ID_Sach=@ID_Sach";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(txtID.Text))
                        {
                            cmd.Parameters.AddWithValue("@ID_Sach", txtID.Text);
                        }
                        cmd.Parameters.AddWithValue("@TenSach", txtTenSach.Text);
                        cmd.Parameters.AddWithValue("@MoTa", txtMoTa.Text);
                        cmd.Parameters.AddWithValue("@Gia", gia);
                        cmd.Parameters.AddWithValue("@SoLuongTon", soLuongTon);
                        cmd.Parameters.AddWithValue("@NgayXuatBan", ngayXuatBan);
                        cmd.Parameters.AddWithValue("@ID_DanhMuc", ddlDanhMuc.SelectedValue);

                        if (!string.IsNullOrEmpty(fileName))
                        {
                            cmd.Parameters.AddWithValue("@Anh", fileName);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }

                lblMessage.Text = "Lưu thông tin sách thành công!";
                lblMessage.ForeColor = System.Drawing.Color.Green;
                pnlEdit.Visible = false;
                Response.Redirect("Sach.aspx");
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Lỗi khi lưu: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            //pnlEdit.Visible = false;
            //ClearForm();
            Response.Redirect("Sach.aspx");
        }



    }
}