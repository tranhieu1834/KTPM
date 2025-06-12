using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Nhom7.Admin
{
    public partial class themvasua : System.Web.UI.Page
    {
        private string connectionString = "Data Source=DESKTOP-5TBTUUI;Initial Catalog=demosach3;Integrated Security=True";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || Session["Role"].ToString() != "admin")
                Response.Redirect("~/Login.aspx");

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
                using (SqlCommand cmd = new SqlCommand("SELECT ID_DanhMuc, TenDanhMuc FROM DanhMuc", conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    ddlDanhMuc.DataSource = reader;
                    ddlDanhMuc.DataTextField = "TenDanhMuc";
                    ddlDanhMuc.DataValueField = "ID_DanhMuc";
                    ddlDanhMuc.DataBind();
                }
            }
            ddlDanhMuc.Items.Insert(0, new ListItem("--Chọn danh mục--", "0"));
        }

        private void LoadSach(string idSach)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Sach WHERE ID_Sach=@ID", conn))
                {
                    cmd.Parameters.AddWithValue("@ID", idSach);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtID.Text = reader["ID_Sach"].ToString();
                            txtTenSach.Text = reader["TenSach"].ToString();
                            txtMoTa.Text = reader["MoTa"].ToString();
                            txtGia.Text = reader["Gia"].ToString();
                            txtSoLuongTon.Text = reader["SoLuongTon"].ToString();
                            txtNgayXuatBan.Text = ((DateTime)reader["NgayXuatBan"]).ToString("yyyy-MM-dd");
                            ddlDanhMuc.SelectedValue = reader["ID_DanhMuc"].ToString();

                            if (!string.IsNullOrEmpty(reader["Anh"].ToString()))
                            {
                                imgPreview.ImageUrl = "~/image/" + reader["Anh"];
                                imgPreview.Visible = true;
                                ViewState["AnhCu"] = reader["Anh"].ToString();
                            }
                        }
                    }
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Reset màu viền
            txtTenSach.BorderColor = System.Drawing.Color.Empty;
            txtMoTa.BorderColor = System.Drawing.Color.Empty;
            txtGia.BorderColor = System.Drawing.Color.Empty;
            txtSoLuongTon.BorderColor = System.Drawing.Color.Empty;
            txtNgayXuatBan.BorderColor = System.Drawing.Color.Empty;
            ddlDanhMuc.BorderColor = System.Drawing.Color.Empty;

            List<string> errors = new List<string>();
            bool allEmpty = string.IsNullOrWhiteSpace(txtTenSach.Text)
                && string.IsNullOrWhiteSpace(txtMoTa.Text)
                && string.IsNullOrWhiteSpace(txtGia.Text)
                && string.IsNullOrWhiteSpace(txtSoLuongTon.Text)
                && string.IsNullOrWhiteSpace(txtNgayXuatBan.Text)
                && ddlDanhMuc.SelectedIndex == 0;

            if (allEmpty)
            {
                lblMessage.Text = "⚠️ Vui lòng nhập đầy đủ thông tin.";
                lblMessage.ForeColor = System.Drawing.Color.Red;

                // Đánh dấu viền đỏ cho các trường
                txtTenSach.BorderColor = System.Drawing.Color.Red;
                txtMoTa.BorderColor = System.Drawing.Color.Red;
                txtGia.BorderColor = System.Drawing.Color.Red;
                txtSoLuongTon.BorderColor = System.Drawing.Color.Red;
                txtNgayXuatBan.BorderColor = System.Drawing.Color.Red;
                ddlDanhMuc.BorderColor = System.Drawing.Color.Red;

                return;
            }

            if (string.IsNullOrWhiteSpace(txtTenSach.Text))
            {
                errors.Add("⚠️ Tên sách không được để trống.");
                txtTenSach.BorderColor = System.Drawing.Color.Red;
            }

            if (string.IsNullOrWhiteSpace(txtMoTa.Text))
            {
                errors.Add("⚠️ Mô tả sách không được để trống.");
                txtMoTa.BorderColor = System.Drawing.Color.Red;
            }

            if (string.IsNullOrWhiteSpace(txtGia.Text) || !decimal.TryParse(txtGia.Text, out _))
            {
                errors.Add("⚠️ Giá sách phải là một số hợp lệ.");
                txtGia.BorderColor = System.Drawing.Color.Red;
            }

            if (string.IsNullOrWhiteSpace(txtSoLuongTon.Text) || !int.TryParse(txtSoLuongTon.Text, out _))
            {
                errors.Add("⚠️ Số lượng tồn phải là một số nguyên hợp lệ.");
                txtSoLuongTon.BorderColor = System.Drawing.Color.Red;
            }

            if (string.IsNullOrWhiteSpace(txtNgayXuatBan.Text) || !DateTime.TryParse(txtNgayXuatBan.Text, out _))
            {
                errors.Add("⚠️ Ngày xuất bản không đúng định dạng.");
                txtNgayXuatBan.BorderColor = System.Drawing.Color.Red;
            }

            if (ddlDanhMuc.SelectedIndex == 0)
            {
                errors.Add("⚠️ Vui lòng chọn danh mục.");
                ddlDanhMuc.BorderColor = System.Drawing.Color.Red;
            }

            string fileName = null;
            if (fileUpload.HasFile)
            {
                string ext = Path.GetExtension(fileUpload.FileName).ToLower();
                if (ext != ".jpg" && ext != ".png" && ext != ".jpeg")
                {
                    errors.Add("⚠️ Chỉ cho phép ảnh định dạng JPG, PNG, JPEG.");
                }
            }

            if (errors.Count > 0)
            {
                lblMessage.Text = string.Join("<br />", errors);
                lblMessage.ForeColor = System.Drawing.Color.Red;
                return;
            }

            // Kiểm tra tên trùng
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string sqlCheck = string.IsNullOrEmpty(txtID.Text)
                    ? "SELECT COUNT(*) FROM Sach WHERE TenSach=@TenSach"
                    : "SELECT COUNT(*) FROM Sach WHERE TenSach=@TenSach AND ID_Sach <> @ID";

                using (SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn))
                {
                    cmdCheck.Parameters.AddWithValue("@TenSach", txtTenSach.Text);
                    if (!string.IsNullOrEmpty(txtID.Text))
                        cmdCheck.Parameters.AddWithValue("@ID", txtID.Text);

                    int count = (int)cmdCheck.ExecuteScalar();
                    if (count > 0)
                    {
                        lblMessage.Text = "⚠️ Tên sách đã tồn tại. Vui lòng nhập tên khác.";
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        txtTenSach.BorderColor = System.Drawing.Color.Red;
                        return;
                    }
                }
            }

            try
            {
                decimal gia = decimal.Parse(txtGia.Text);
                int sl = int.Parse(txtSoLuongTon.Text);
                DateTime dtOut = DateTime.Parse(txtNgayXuatBan.Text);

                if (fileUpload.HasFile)
                {
                    fileName = Path.GetFileName(fileUpload.FileName);
                    fileUpload.SaveAs(Server.MapPath("~/image/") + fileName);
                }
                else if (!string.IsNullOrEmpty(txtID.Text))
                {
                    fileName = ViewState["AnhCu"]?.ToString();
                }

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = string.IsNullOrEmpty(txtID.Text)
                        ? "INSERT INTO Sach (TenSach,MoTa,Gia,SoLuongTon,NgayXuatBan,ID_DanhMuc,Anh) VALUES (@TenSach,@MoTa,@Gia,@SL,@Ngay,@DM,@Anh)"
                        : "UPDATE Sach SET TenSach=@TenSach,MoTa=@MoTa,Gia=@Gia,SoLuongTon=@SL,NgayXuatBan=@Ngay,ID_DanhMuc=@DM,Anh=@Anh WHERE ID_Sach=@ID";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        if (!string.IsNullOrEmpty(txtID.Text))
                            cmd.Parameters.AddWithValue("@ID", txtID.Text);

                        cmd.Parameters.AddWithValue("@TenSach", txtTenSach.Text);
                        cmd.Parameters.AddWithValue("@MoTa", txtMoTa.Text);
                        cmd.Parameters.AddWithValue("@Gia", gia);
                        cmd.Parameters.AddWithValue("@SL", sl);
                        cmd.Parameters.AddWithValue("@Ngay", dtOut);
                        cmd.Parameters.AddWithValue("@DM", ddlDanhMuc.SelectedValue);
                        if (!string.IsNullOrEmpty(fileName))
                            cmd.Parameters.AddWithValue("@Anh", fileName);
                        else
                            cmd.Parameters.AddWithValue("@Anh", DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                lblMessage.Text = "✅ Lưu thành công. Chuyển trang sau 2s.";
                lblMessage.ForeColor = System.Drawing.Color.Green;
                ScriptManager.RegisterStartupScript(this, GetType(), "rdr", "setTimeout(()=>{window.location='Sach.aspx';},2000);", true);
            }
            catch (Exception ex)
            {
                lblMessage.Text = "❌ Lỗi khi lưu: " + ex.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
            }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("Sach.aspx");
        }
    }
}
