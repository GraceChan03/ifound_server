using Ifound.Models;
using System;
using System.Data;
namespace Ifound.Services
{
    public interface IStudentService
    {
        DataTable AddStudentByExcel(string file, string sheetName);
    }
}
