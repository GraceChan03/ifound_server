using System.Collections.Generic;
using Ifound.Models;
using System.Data;

namespace Ifound.Services
{
    public class StudentService : IStudentService
    {
        public DataTable AddStudentByExcel(string file, string sheetName)
        {
            List<Student> list = new List<Student>();
            DataTable dt = new DataTable();
            dt = NPOIExcelHelper.ReadExcelToDataTable(file);
            list = DataTableToList.ToList<Student>(dt);
            return dt;
        }
    }
}
