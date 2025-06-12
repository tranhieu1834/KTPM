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
    public partial class suasach : System.Web.UI.Page
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
        private void ClearForm()
        {
            txtID.Text = "";
            txtTenSach.Text = "";
            txtMoTa.Text = "";
            txtGia.Text = "";
            txtSoLuongTon.Text = "";
            txtNgayXuatBan.Text = "";
            ddlDanhMuc.SelectedIndex = 0;
            imgPreview.ImageUrl = ""; // Xóa ảnh preview nếu có
            lblMessage.Text = "";
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

                if (reader.HasRows && reader.Read())
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
                else
                {
                    lblMessage.Text = "Không tìm thấy sách với ID đã cho!";
                }

            }
        }


        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTenSach.Text))
            {
                lblMessage.Text = "Tên sách không được để trống.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            else if (string.IsNullOrEmpty(txtMoTa.Text))
            {
                lblMessage.Text = "Mô tả không được để trống.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            else if (string.IsNullOrEmpty(txtGia.Text) || !decimal.TryParse(txtGia.Text, out _))
            {
                lblMessage.Text = "Giá sách phải là một số hợp lệ.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            else if (string.IsNullOrEmpty(txtSoLuongTon.Text) || !int.TryParse(txtSoLuongTon.Text, out _))
            {
                lblMessage.Text = "Số lượng tồn phải là một số nguyên hợp lệ.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            else if (string.IsNullOrEmpty(txtNgayXuatBan.Text) || !DateTime.TryParse(txtNgayXuatBan.Text, out _))
            {
                lblMessage.Text = "Ngày xuất bản không đúng định dạng.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }
            else if (ddlDanhMuc.SelectedIndex == 0)
            {
                lblMessage.Text = "Vui lòng chọn danh mục.";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            string fileName = null;
            if (fileUpload.HasFile)
            {
                fileName = Path.GetFileName(fileUpload.PostedFile.FileName);
                fileUpload.SaveAs(Server.MapPath("~/image/") + fileName);
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query;

                if (string.IsNullOrEmpty(txtID.Text))
                {
                    // Insert new record
                    query = "INSERT INTO Sach (TenSach, MoTa, Gia, SoLuongTon, NgayXuatBan, ID_DanhMuc, Anh) VALUES (@TenSach, @MoTa, @Gia, @SoLuongTon, @NgayXuatBan, @ID_DanhMuc, @Anh)";
                }
                else
                {
                    if (fileName != null)
                    {
                        // Update record with new image
                        query = "UPDATE Sach SET TenSach=@TenSach, MoTa=@MoTa, Gia=@Gia, SoLuongTon=@SoLuongTon, NgayXuatBan=@NgayXuatBan, ID_DanhMuc=@ID_DanhMuc, Anh=@Anh WHERE ID_Sach=@ID_Sach";
                    }
                    else
                    {
                        // Update record without changing image
                        query = "UPDATE Sach SET TenSach=@TenSach, MoTa=@MoTa, Gia=@Gia, SoLuongTon=@SoLuongTon, NgayXuatBan=@NgayXuatBan, ID_DanhMuc=@ID_DanhMuc WHERE ID_Sach=@ID_Sach";
                    }
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@TenSach", txtTenSach.Text);
                    cmd.Parameters.AddWithValue("@MoTa", txtMoTa.Text);
                    cmd.Parameters.AddWithValue("@Gia", txtGia.Text);
                    cmd.Parameters.AddWithValue("@SoLuongTon", txtSoLuongTon.Text);
                    cmd.Parameters.AddWithValue("@NgayXuatBan", txtNgayXuatBan.Text);
                    cmd.Parameters.AddWithValue("@ID_DanhMuc", ddlDanhMuc.SelectedValue);
                    cmd.Parameters.AddWithValue("@Anh", fileName ?? (object)DBNull.Value);

                    if (!string.IsNullOrEmpty(txtID.Text))
                    {
                        cmd.Parameters.AddWithValue("@ID_Sach", txtID.Text);
                    }

                    cmd.ExecuteNonQuery();
                }
            }

            Response.Redirect("Sach.aspx");
        }


        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("Sach.aspx");
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            ClearForm(); // ✨ Xóa sạch dữ liệu trước khi load lại
            Button btnEdit = (Button)sender;
            int id = Convert.ToInt32(btnEdit.CommandArgument);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Sach WHERE ID_Sach=@ID_Sach";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ID_Sach", id);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtID.Text = reader["ID_Sach"].ToString();
                        txtTenSach.Text = reader["TenSach"].ToString();
                        txtMoTa.Text = reader["MoTa"].ToString();
                        txtGia.Text = reader["Gia"].ToString();
                        txtSoLuongTon.Text = reader["SoLuongTon"].ToString();
                        txtNgayXuatBan.Text = Convert.ToDateTime(reader["NgayXuatBan"]).ToString("yyyy-MM-dd");

                        string selectedDanhMuc = reader["ID_DanhMuc"].ToString();
                        if (ddlDanhMuc.Items.FindByValue(selectedDanhMuc) != null)
                        {
                            ddlDanhMuc.SelectedValue = selectedDanhMuc;
                        }
                        else
                        {
                            ddlDanhMuc.SelectedIndex = 0; // Chọn giá trị mặc định nếu không tìm thấy giá trị phù hợp
                        }
                    }
                }
            }

            pnlEdit.Visible = true;
        }



    }
}