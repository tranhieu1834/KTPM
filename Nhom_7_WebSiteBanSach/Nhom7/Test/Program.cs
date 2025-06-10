using System;
using Nhom7;

class Program
{
    static void Main(string[] args)
    {
        // Ví dụ kiểm tra lớp DatabaseHelper
        DatabaseHelper dbHelper = new DatabaseHelper();
        var books = dbHelper.GetBooks();

        foreach (var book in books)
        {
            Console.WriteLine($"Tên sách: {book.TenSach}, Giá: {book.Gia}");
        }

        Console.WriteLine("Chương trình chạy xong!");
        Console.ReadKey();
    }
}
