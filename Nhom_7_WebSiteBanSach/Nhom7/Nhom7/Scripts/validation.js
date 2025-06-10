function validateForm() {
    let errors = [];

    if (!document.getElementById("txtTenSach").value.trim()) {
        errors.push("Tên sách không được để trống.");
    }

    let gia = document.getElementById("txtGia").value;
    if (!gia || isNaN(gia) || gia <= 0) {
        errors.push("Giá sách phải là một số hợp lệ.");
    }

    if (document.getElementById("ddlDanhMuc").selectedIndex === 0) {
        errors.push("Vui lòng chọn danh mục.");
    }

    if (errors.length > 0) {
        document.getElementById("errorBox").innerHTML = errors.join("<br>");
        document.getElementById("errorBox").style.display = "block";
        return false; // Ngăn không cho gửi form
    }

    return true; // Cho phép form gửi đi