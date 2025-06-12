import unittest
from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait, Select
from selenium.webdriver.support import expected_conditions as EC
import openpyxl
import time
import os
import datetime


class TestEditBook(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.driver = webdriver.Chrome()
        cls.driver.maximize_window()
        cls.wait = WebDriverWait(cls.driver, 10)
        cls.wb = openpyxl.load_workbook('testcase.xlsx')
        cls.sheet = cls.wb['SuaSach']
        cls.driver.get("https://localhost:44329/Login")

        # Đăng nhập
        cls.driver.find_element(By.ID, "txtUsername").send_keys("admin")
        cls.driver.find_element(By.ID, "txtPassword").send_keys("123")
        Select(cls.driver.find_element(By.ID, "ddlRole")).select_by_value("1")
        cls.driver.find_element(By.ID, "btnLogin").click()
        cls.wait.until(EC.url_contains("/Admin"))

    def test_edit_book(self):
        for row in range(2, 7):
            try:
                print(f"🔁 Đang test dòng {row}")

                # Lấy dữ liệu từ Excel
                book_id = self.sheet[f'B{row}'].value
                book_name = self.sheet[f'C{row}'].value or ''
                category = self.sheet[f'D{row}'].value or ''
                description = self.sheet[f'E{row}'].value or ''
                price = self.sheet[f'F{row}'].value or ''
                quantity = self.sheet[f'G{row}'].value or ''
                publish_date = self.sheet[f'H{row}'].value or ''
                if isinstance(publish_date, datetime.datetime):
                    publish_date = publish_date.strftime('%Y-%m-%d')
                else:
                    publish_date = str(publish_date).strip()
                image_path = self.sheet[f'I{row}'].value or ''

                # # Kiểm tra dữ liệu thiếu
                # if not all([book_name.strip(), description.strip(), str(price).strip(), str(quantity).strip(), publish_date.strip()]) or category == "--Chọn danh mục--":
                #     self.sheet[f'J{row}'] = "Thiếu dữ liệu bắt buộc"
                #     self.sheet[f'K{row}'] = "FAIL"
                #     self.wb.save("testcase.xlsx")
                #     continue

                # Mở lại trang sửa sách
                self.driver.get(f"https://localhost:44329/Admin/SuaSach?ID_Sach={book_id}")
                time.sleep(1)

                # Xóa và nhập dữ liệu
                self.driver.find_element(By.ID, "MainContent_txtTenSach").clear()
                self.driver.find_element(By.ID, "MainContent_txtMoTa").clear()
                self.driver.find_element(By.ID, "MainContent_txtGia").clear()
                self.driver.find_element(By.ID, "MainContent_txtSoLuongTon").clear()
                self.driver.find_element(By.ID, "MainContent_txtNgayXuatBan").clear()

                self.driver.find_element(By.ID, "MainContent_txtTenSach").send_keys(book_name)
                self.driver.find_element(By.ID, "MainContent_txtMoTa").send_keys(description)
                self.driver.find_element(By.ID, "MainContent_txtGia").send_keys(str(price))
                self.driver.find_element(By.ID, "MainContent_txtSoLuongTon").send_keys(str(quantity))
                self.driver.find_element(By.ID, "MainContent_txtNgayXuatBan").send_keys(publish_date)

                # Chọn danh mục
                category_dropdown = Select(self.driver.find_element(By.ID, "MainContent_ddlDanhMuc"))
                try:
                    category_dropdown.select_by_visible_text(category)
                except:
                    self.sheet[f'J{row}'] = f"Danh mục không tồn tại: {category}"
                    self.sheet[f'K{row}'] = "FAIL"
                    self.wb.save("testcase.xlsx")
                    continue

                # Ảnh
                if image_path and os.path.exists(image_path):
                    file_input = self.driver.find_element(By.ID, "MainContent_fileUpload")
                    file_input.send_keys(os.path.abspath(image_path))

                # Lưu
                save_btn = self.wait.until(
                    EC.element_to_be_clickable((By.ID, "MainContent_btnSave"))
                )
                save_btn.click()
                time.sleep(2)

                current_url = self.driver.current_url.strip().lower()
                if current_url == "https://localhost:44329/admin/sach":
                    self.sheet[f'J{row}'] = "Sửa thành công"
                    self.sheet[f'K{row}'] = "PASS"
                else:
                    try:
                        message_label = self.wait.until(
                            EC.presence_of_element_located((By.ID, "MainContent_lblMessage"))
                        )
                        message_text = message_label.text.strip()
                        self.sheet[f'J{row}'] = message_text
                        self.sheet[f'K{row}'] = "FAIL"
                    except:
                        self.sheet[f'J{row}'] = "Không tìm thấy thông báo lỗi"
                        self.sheet[f'K{row}'] = "FAIL"

                self.wb.save("testcase.xlsx")

            except Exception as e:
                self.sheet[f'J{row}'] = f"Lỗi: {str(e)}"
                self.sheet[f'K{row}'] = "FAIL"
                self.wb.save("testcase.xlsx")
                continue

    @classmethod
    def tearDownClass(cls):
        try:
            cls.wb.save('testcase.xlsx')
            print("✅ Đã lưu kết quả vào Excel.")
        except Exception as e:
            print(f"❌ Lỗi khi lưu Excel: {str(e)}")
        finally:
            cls.driver.quit()


if __name__ == '__main__':
    unittest.main()
