using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Ifound.Services
{
    class DataTableToList
    {
        public static List<TResult> ToList<TResult>(DataTable dt) where TResult : class,new()
        {
            //创建一个属性的列表。属性？
            List<PropertyInfo> prlist = new List<PropertyInfo>();
            //获取TResult的类型实例反射的入口
            Type t = typeof(TResult);
            //获得TResult的所有的Public属性，并找出TResult属性和DataTable的列名称相同的属性(PropertyInfo)，并加入到属性列表 
            Array.ForEach<PropertyInfo>
                (t.GetProperties(), p => { if (dt.Columns.IndexOf(p.Name) != -1)prlist.Add(p); });
            List<TResult> oblist = new List<TResult>();

            foreach (DataRow row in dt.Rows)
            {
                TResult ob = new TResult();
                //找到对应的数据赋值
                prlist.ForEach(p => { if (row[p.Name] != DBNull.Value)p.SetValue(ob, row[p.Name], null); });
                oblist.Add(ob);
            }
            return oblist;
        }
    }
}
