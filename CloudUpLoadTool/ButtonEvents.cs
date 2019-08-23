using System;
using System.Data;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;

namespace CloudUpLoadTool
{
    public class ButtonEvents : AbstractBillPlugIn
    {
        public override void BarItemClick(BarItemClickEventArgs e)
        {
            var resultMessage = string.Empty;

            //订单退回操作
            base.BarItemClick(e);

            if (e.BarItemKey == "tbUpLoadOrder")
            {
                //定义获取表头信息对像
                var docScddIds1 = View.Model.DataObject;
                //获取表头中单据编号信息(注:这里的BillNo为单据编号中"绑定实体属性"项中获得)
                var dhstr = docScddIds1["BillNo"].ToString();

                fcy.Service.CnnStr = "http://172.16.4.252/websys/service.asmx";
                fcy.Service.userdmstr = "feng";
                fcy.Service.passwordstr = "";

                //将获取的单据名称进行截取,取前两位
                var orderno = dhstr.Substring(0,2);
                //根据获取的标记分别进行单据上传至U订货相关单据
                switch (orderno)
                {
                    //应收单
                    case "AR":
                        resultMessage = UploadOrder(dhstr);
                        break;
                    //收款退款单
                    case "SK":
                        resultMessage = SkUpload(dhstr);
                        break;
                }
                //输出结果
                View.ShowMessage(resultMessage);
            }
        }

        /// <summary>
        /// 收款退款单上传使用（注:金额为正的就是上传至U订货的支付单  若为负的就是上传至U订货的退款单）
        /// </summary>
        /// <param name="dh"></param>
        /// <returns></returns>
        public static string SkUpload(string dh)
        {
            var ab = new fcydata.Service();
            var sqlList=new SqlList();
            var selstr = sqlList.Get_Sql(dh);
            var data = ab.FDataSet(selstr);
            return FcyUdhPosts.SKUpload(data.Tables[0].Rows[0]);

        }

        /// <summary>
        /// 将K3 CLOUD应收单上传至U订货发货单
        /// </summary>
        /// <param name="orderno"></param>
        /// <returns></returns>
        private string UploadOrder(string orderno)
        {
            var result = string.Empty;
            try
            {
                result = FcyUdhPosts.Xsfhup(orderno);
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }
            return result;
        }

        #region Hide Del function
        /// <summary>
        /// 执行删除销售订单
        /// </summary>
        /// <param name="orderno"></param>
        /// <returns></returns>
        //private string DelSalesOrder(string orderno)
        //{
        //    var ab = new Service();
        //    var result = string.Empty;

        //    var udhstr = ab.str("select udh from axsdd where dh='" + orderno + "'");
        //    if (udhstr.Trim().Length > 0)
        //    {
        //        ab.sqlcmd("delete uddzy where cOrderNo='" + udhstr + "'");
        //        ab.sqlcmd("delete uOrder where cOrderNo='" + udhstr + "'");
        //        FcyUdhPosts.Ddht(udhstr);

        //        result = $"{orderno}已在U订货平台内删除";
        //    }
        //    else
        //    {
        //        result = $"没有在U订货平台查找到{orderno}单据记录,故没有执行删除操作";
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 删除收款退款单
        ///// </summary>
        ///// <param name="orderno"></param>
        ///// <returns></returns>
        //private string DelReturnOrder(string orderno)
        //{
        //    var result = string.Empty;

        //    try
        //    {
        //        result = FcyUdhPosts.Tkddel(orderno);
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.Message;
        //    }
        //    return result;
        //}

        ///// <summary>
        ///// 删除应收单
        ///// </summary>
        ///// <param name="orderno"></param>
        ///// <returns></returns>
        //private string DelReceive(string orderno)
        //{
        //    var result = string.Empty;

        //    try
        //    {
        //        result = FcyUdhPosts.Xsfhdelup(orderno);
        //    }
        //    catch (Exception ex)
        //    {
        //        result = ex.Message;
        //    }
        //    return result;
        //}
        #endregion
    }
}
