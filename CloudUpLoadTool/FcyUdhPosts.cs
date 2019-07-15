using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.Xml;

namespace CloudUpLoadTool
{
    public class FcyUdhPosts
    {
        /// <summary>
        /// 将K3 CLOUD的应收单上传至U订货发货单
        /// </summary>
        /// <param name="dhstr"></param>
        /// <returns></returns>
        public static string Xsfhup(string dhstr)
        {
            var ret = "";
            fcydata.Service ab = new fcydata.Service();
            var data = ab.FDataSet("select dh,udh,rq,fhrq,shr,shtel,shaddr,hydh,hysj,hytel,hyrq,hyqd,hymz,hydz,hyyf,hyjs,fid,memo from fhd where dh='" + dhstr + "'");
            var date = DateTime.Now;
            var date1 = DateTime.Now;
            var udhstr = data.Tables[0].Rows[0]["udh"].ToString().Trim();
            Xsfhdelup(dhstr);
            //   FcyUdhGet.xsdddmx(udhstr);
            var datas = ab.FDataSet("select dh,wlbh,sl,dj,je,idkey from fhdsv where dh='" + dhstr + "'");

            Dictionary<string, string> param = new Dictionary<string, string>();

            var delivery = @"{<M>,oDeliveryDetails:[<S>]}";

            var m = @" 'cOrderNo': '<udh>','cOutSysKey':'<dh>','dCreated':'<rq>','dSendDate' :'<fhrq>','cReceiveAddress': '<shaddr>',
                       'cReceiver': '<shr>','cReceiveMobile': '<shtel>','oShippingMemo': {'cRemark': '<memo>'}";
            try
            {
                date = (DateTime)data.Tables[0].Rows[0]["rq"];
            }
            catch
            {
            }
            try
            {
                date1 = (DateTime)data.Tables[0].Rows[0]["fhrq"];
            }
            catch
            {
            }
            m = m.Replace("<rq>", date.ToString("yyyy-MM-dd hh:mm:ss"));
            m = m.Replace("<fhrq>", date1.ToString("yyyy-MM-dd hh:mm:ss"));
            for (var i = 0; i < data.Tables[0].Columns.Count; i++)
            {
                m = m.Replace("<" + data.Tables[0].Columns[i].ColumnName + ">", data.Tables[0].Rows[0][i].ToString().Trim());
            }

            //        string s = @"{
            //'iOrderDetailId': '<uid>',
            // 'iSendQuantity': <sl>

            // }";

            var s = @"{'iOrderDetailId': '<uid>','iSendQuantity': <sl>,'fErpTransactionPrice': <dj>,'fErpSalePayMoney':<je>}";

            var s1 = s;
            var S = "";

            for (var i = 0; i < datas.Tables[0].Rows.Count; i++)
            {
                s1 = s;
                s1 = s1.Replace("<uid>", datas.Tables[0].Rows[i]["idkey"].ToString().Trim());
                s1 = s1.Replace("<sl>", datas.Tables[0].Rows[i]["sl"].ToString().Trim());
                s1 = s1.Replace("<dj>", datas.Tables[0].Rows[i]["dj"].ToString().Trim());
                s1 = s1.Replace("<je>", datas.Tables[0].Rows[i]["je"].ToString().Trim());
                S = S + s1 + ",";
            }
            S = S.Substring(0, S.Length - 1);
            delivery = delivery.Replace("<M>", m);
            delivery = delivery.Replace("<S>", S);
            param.Add("delivery", delivery);

            ret = FcyWeb.Post("/ws/Orders/saveDelivery", param);


            var fhdh = "";
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(ret);
            if (fcydata.FcyXml.GetNodeVal(xmldoc.DocumentElement, "code") == "200")
            {
                fhdh = fcydata.FcyXml.GetNodeVal(xmldoc.DocumentElement, "data/models.voucher.DeliveryVoucher/cDeliveryNo");
            }
            else
                return ret;

            //hydh,hysj,hytel,hyrq,hyqd,hymz,hydz,hyyf,hyjs,
            param = new Dictionary<string, string>();
            var json = "{deliveryNo: '" + fhdh + "',outSysNo:'<dh>',deliveryDefine:{define1:'<hyjs>',define2:'<hymz>',define3:'<hyyf>',define4:'<hydz>',define5:'<hydh>',define6:'<hysj>',define7:'<hytel>',define8:'<hyrq>'}}";
            for (var i = 0; i < data.Tables[0].Columns.Count; i++)
            {
                json = json.Replace("<" + data.Tables[0].Columns[i].ColumnName + ">", data.Tables[0].Rows[0][i].ToString().Trim());
            }
            param.Add("deliveryDefineVo", json);
            ret = FcyWeb.Post("/ws/Orders/saveDeliveryDefineVoApi", param);

            ab.sqlcmd("insert into upfhd (dh) values ('" + dhstr + "')");

            return ret;
        }

        /// <summary>
        /// 删除U订货上的"发货通知单"(k3对应:应收单)
        /// </summary>
        /// <param name="cOutSysKey"></param>
        /// <returns></returns>
        public static string Xsfhdelup(string cOutSysKey)
        {
            var ret = "";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("cOutSysKey", cOutSysKey);
            ret = FcyWeb.Post("/ws/Orders/delDelivery", param);
            return ret;
        }

        //public static string xsfhdelup(string cOutSysKey)
        //{
        //    string ret = "";
        //    Dictionary<string, string> param = new Dictionary<string, string>();
        //    param.Add("cOutSysKey", cOutSysKey);
        //    ret = FcyWeb.Post("/ws/Orders/delDelivery", param);
        //    return ret;
        //}

        #region Hide
        /// <summary>
        /// 删除U订货上的"退款单"(K3对应:收款退款单 及 其他应收单)
        /// </summary>
        /// <param name="cOutSysKey"></param>
        /// <returns></returns>
        public static string Tkddel(string cOutSysKey)
        {
            string ret = "";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("outsyskey", cOutSysKey);
            ret = FcyWeb.Post("/ws/Payments/delRefund", param);
            return ret;
        }

        /// <summary>
        /// 删除付款单
        /// </summary>
        /// <param name="cOutSysKey"></param>
        /// <returns></returns>
        public static string Zfddel(string cOutSysKey)
        {
            string ret = "";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("cOutSysKey", cOutSysKey);
            ret = FcyWeb.Post("/ws/Payments/delPayment", param);
            return ret;
        }



        /// <summary>
        /// 订单 回退(k3:销售订单使用)
        /// </summary>
        /// <param name="dh"></param>
        /// <returns></returns>
        public static string Ddht(string dh)
        {
            string ret = "";
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("orderno", dh);
            ret = FcyWeb.Post("/ws/Orders/orderConfirmBackApi", param);
            return ret;
        }

        /// <summary>
        /// 上传应收单
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public static string Ysdup(DataRow row)
        {
            string ret = "";
            fcydata.Service ab = new fcydata.Service();
            if (row["je"].ToString().Trim().IndexOf("-") > 0)
            {
                row["je"] = -(decimal)row["je"];
                MessageBox.Show(row["je"].ToString());
                Dictionary<string, string> param = new Dictionary<string, string>();

                DateTime date = (DateTime)row["rq"];
                string strs = "{'iAmount':<je>,'cOutSysKey':'<dh>','oAgent':{'cCode':'<khbh>','cOutSysKey':'<khbh>'},'oSettlementWay':{'cErpCode':'<jsfs>'},'iPayMentStatusCode':2,'cRefundPayDirection': 'TOUDH','dPayFinishDate':'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "','dReceiptDate':'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "','remark':'<memo>'}";

                for (int j = 0; j < row.Table.Columns.Count; j++)
                {
                    strs = strs.Replace("<" + row.Table.Columns[j].ColumnName + ">", row[j].ToString().Trim());
                }
                param.Add("refund", strs);
                ret = FcyWeb.Post("/ws/Payments/saveRefund", param);
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(ret);
                if (fcydata.FcyXml.GetNodeVal(xmldoc.DocumentElement, "data") != "")
                {
                    MessageBox.Show(ret);
                }



            }
            else
            {
                Dictionary<string, string> param = new Dictionary<string, string>();

                DateTime date = (DateTime)row["rq"];
                string strs = "{'iAmount':<je>,'cOutSysKey':'<dh>','oAgent':{'cErpCode':'<khbh>'},'oSettlementWay':{'cErpCode':'<jsfs>'},'cVoucherType':'NORMAL','iPayMentStatusCode':2,'dPayFinishDate':'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "','dReceiptDate':'" + date.ToString("yyyy-MM-dd HH:mm:ss") + "','remark':'<memo>'}";

                for (int j = 0; j < row.Table.Columns.Count; j++)
                {
                    strs = strs.Replace("<" + row.Table.Columns[j].ColumnName + ">", row[j].ToString().Trim());
                }
                param.Add("payment", strs);

                ret = FcyWeb.Post("/ws/Payments/savePayment", param);
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.LoadXml(ret);
                if (fcydata.FcyXml.GetNodeVal(xmldoc.DocumentElement, "data") != "")
                {
                    MessageBox.Show(ret);
                }
            }
            return ret;
        }
        #endregion
    }
}
