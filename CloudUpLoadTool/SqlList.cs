using System;

namespace CloudUpLoadTool
{
    public class SqlList
    {
        String _result = string.Empty;

        /// <summary>
        /// K3-CLOUD收款退款单上传使用
        /// </summary>
        /// <param name="orderno"></param>
        /// <returns></returns>
        public string Get_Sql(string orderno)
        {
            _result = $@"
                            select FBILLNO as dh,FDATE as rq,FAMOUNT as je,FREMARK+FCOMMENT+FBILLNO+y as memo,FCONTACTUNIT as FCONTACTUNIT,FCOMMENT as FCOMMENT,
                                    FNUMBER as khbh,FDOCUMENTSTATUS as zt,jsfs as jsfs,y as y 
                            from qtysd where  FBILLNO='{orderno}'
                        ";
            return _result;
        }
    }
}
