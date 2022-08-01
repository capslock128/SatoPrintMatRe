using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Net.NetworkInformation;
using System.Configuration;
using System.Net.Sockets;
using log4net;

namespace SatoPrintMatRe
{
    public partial class SatoPrintService : ServiceBase
    {
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        Thread PrintWork;
        string _satoIp = ConfigurationManager.AppSettings["SatoIP"];
        List<string> ListTemPlate = new List<string>();
        List<string> CmdToPrinter = new List<string>();

        string LabelTemplateCmd = "##ÀSATCG4ÿÿAA3V+000H+000CS3#E3A1V00639H0639PM3PO0+00PO3+00IG2Z##                            APSWKMatReLabelV2.lbl%2H0217V00322P02RDA00,026,028,{User}%2H0274V00322P02RDA00,026,028,By :%2H0273V00392P02RDA00,026,028,Date :%2H0468V00322P02RDA00,026,028,{LotNo}%2H0576V00322P02RDA00,026,028,Lot No :%2H0459V00253P02RDA00,026,028,{BagNo}%2H0464V00046P02RDA00,026,028,{Weight}%2H0198V00392P02RDA00,026,028,{Date}%2H0426V00184P02RDA00,026,028,{MFGDate}%2H0426V00115P02RDA00,026,028,{EXPDate}%2H0460V00392P02RDA00,026,028,{ItemNo}%2H0576V00392P02RDA00,026,028,Item No :%2H0576V00253P02RDA00,026,028,Bag No :%2H0576V00471P02RDA00,036,039,{IngName}%2H0576V00046P02RDA00,026,028,Weight :%2H0576V00184P02RDA00,026,028,MFG Date :%2H0576V00115P02RDA00,026,028,EXP Date :%2H0436V00541P02RDA00,026,028,MMS Material EX1%2H0201V001742D30,M,04,1,0DN{QRData}~A1Q1Z##  ##  ";

        string PrinterSetting = "AA3V+000H+000CS3#E3A1V00600H0800PM3PO0+00PO3+00PH1IG2ZACT0001ZA~A0Q1ZAA3H0250V0001Z";
        string PrinterStart = "A";
        string PrinterExp_Label = "%3V0271H0241$A,024,029,0$=ExpDate:";
        string PrinterWeight_Label = "%3V0028H0030$A,024,029,0$=Weight:";
        string PrinterBagNo_Label = "%3V0027H0137$A,024,029,0$=Bag No:";
        string PrinterDate_Label = "%3V0305H0295$A,024,029,0$=Date:";
        string PrinterBy_Label = "%3V0027H0086$A,024,029,0$=By:";
        string PrinterItemNo_Label = "%3V0030H0295$A,024,029,0$=ItemNo:";
        string PrinterLot_Label = "%3V0027H0241$A,024,029,0$=LotNo:";
        string PrinterDesc_Label = "%3V0029H0188$A,024,029,0$=Description:";

        string PrinterExp = "%3V0379H0241$A,024,029,0$=";
        string PrinterWeight = "%3V0104H0029$A,024,029,0$=";
        string PrinterBagNo = "%3V0110H0137$A,024,029,0$=";
        string PrinterDate = "%3V0378H0295$A,024,029,0$=";
        string PrinterBy = "%3V0070H0086$A,024,029,0$=";
        string PrinterItemNo = "%3V0119H0297$A,024,029,0$=";
        string PrinterLot = "%3V0120H0243$A,024,029,0$=";
        string PrinterDesc = "%3V0140H0189$A,024,029,0$=";

        string PrinterQr = "%3V0361H00862D30,L,5,1,0DN";
        string PrinterStop = "EPZ";
        public SatoPrintService()
        {
            InitializeComponent();
            ListTemPlate = new List<string>();
            ListTemPlate.Add(PrinterSetting);
            ListTemPlate.Add(PrinterStart);
            ListTemPlate.Add(PrinterExp_Label);
            ListTemPlate.Add(PrinterWeight_Label);
            ListTemPlate.Add(PrinterBagNo_Label);
            ListTemPlate.Add(PrinterDate_Label);
            ListTemPlate.Add(PrinterBy_Label);
            ListTemPlate.Add(PrinterItemNo_Label);
            ListTemPlate.Add(PrinterLot_Label);
            ListTemPlate.Add(PrinterDesc_Label);
        }
        //[Conditional("DEBUG_SERVICE")]
        //private static void DebugMode()
        //{
        //    Debugger.Break();
        //}
        protected override void OnStart(string[] args)
        {
            //DebugMode();
            //PrintWork = new Thread(Getdata);
            PrintWork = new Thread(PrintLabelV2);
            PrintWork.Start();
        }

        protected override void OnStop()
        {
            PrintWork.Abort();
        }

        private void PrintLabelV2() {
            while (true) {
                try
                {
                    Ajinomoto_Weighing_SystemEntities db = new Ajinomoto_Weighing_SystemEntities();
                    CmdToPrinter = new List<string>();
                    LogPrint_MatRe LogMatRe = db.LogPrint_MatRe.Where(o => o.IsCompleted == false).OrderBy(o => o.Datetime).FirstOrDefault();
                    LogRePrint_MatRe LogReprint = db.LogRePrint_MatRe.Where(o => o.IsCompleted == false).OrderBy(o => o.Datetime).FirstOrDefault();
                    
                    if (LogMatRe != null || LogReprint != null) {
                        bool PrintMatRe = true;
                        if (LogMatRe != null && LogReprint != null)
                        {
                            PrintMatRe = LogMatRe.Datetime < LogReprint.Datetime;
                        }
                        else if (LogMatRe == null) {
                            PrintMatRe = false;
                        }
                        if (PrintMatRe)
                        {
                            string PrintCmd = LabelTemplateCmd.Replace("{User}", LogMatRe.UserName);
                            PrintCmd = PrintCmd.Replace("{LotNo}", LogMatRe.LotNo);
                            PrintCmd = PrintCmd.Replace("{BagNo}", LogMatRe.BagNo);
                            PrintCmd = PrintCmd.Replace("{Weight}", (LogMatRe.PackWeight ?? 0).ToString("00.00") + " KG");
                            PrintCmd = PrintCmd.Replace("{Date}", (LogMatRe.Datetime ?? DateTime.Now).ToString("dd-MM-yyyy"));
                            PrintCmd = PrintCmd.Replace("{MFGDate}", LogMatRe.MFG);
                            PrintCmd = PrintCmd.Replace("{EXPDate}", LogMatRe.Exp);
                            PrintCmd = PrintCmd.Replace("{ItemNo}", LogMatRe.IngItemNo);
                            string IngDesc = db.MasterIngs.Where(o => o.IngItemNo == LogMatRe.IngItemNo).Select(o => o.IngDescripton).FirstOrDefault();
                            if (IngDesc.Length > 20)
                            {
                                IngDesc = IngDesc.Substring(0, 20);
                            }
                            PrintCmd = PrintCmd.Replace("{IngName}", IngDesc);
                            string QrText = LogMatRe.IngItemNo + "|" + LogMatRe.BagNo + "|" + LogMatRe.LotNo;
                            int QrTextLength = QrText.Length;
                            PrintCmd = PrintCmd.Replace("{QRData}", QrTextLength.ToString("0000") + "," + QrText);
                            CmdToPrinter.Add(PrintCmd);
                            if (Print())
                            {
                                try
                                {
                                    LogMatRe.IsCompleted = true;
                                    db.Entry(LogMatRe).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch(Exception ex)
                                {
                                    log.Error(ex.ToString());
                                }
                            }
                        }
                        else {
                            string PrintCmd = LabelTemplateCmd.Replace("{User}", LogReprint.UserName);
                            PrintCmd = PrintCmd.Replace("{LotNo}", LogReprint.LotNo);
                            PrintCmd = PrintCmd.Replace("{BagNo}", LogReprint.BagNo);
                            PrintCmd = PrintCmd.Replace("{Weight}", (LogReprint.LogBagSeperate.LogPrint_MatRe.PackWeight ?? 0).ToString("00.00") + " KG");
                            PrintCmd = PrintCmd.Replace("{Date}", (LogReprint.LogBagSeperate.LogPrint_MatRe.Datetime ?? DateTime.Now).ToString("dd-MM-yyyy"));
                            PrintCmd = PrintCmd.Replace("{MFGDate}", LogReprint.LogBagSeperate.LogPrint_MatRe.MFG);
                            PrintCmd = PrintCmd.Replace("{EXPDate}", LogReprint.LogBagSeperate.LogPrint_MatRe.Exp);
                            PrintCmd = PrintCmd.Replace("{ItemNo}", LogReprint.IngItemNo);
                            string IngDesc = db.MasterIngs.Where(o => o.IngItemNo == LogReprint.IngItemNo).Select(o => o.IngDescripton).FirstOrDefault();
                            if (IngDesc.Length > 20)
                            {
                                IngDesc = IngDesc.Substring(0, 20);
                            }
                            PrintCmd = PrintCmd.Replace("{IngName}", IngDesc);
                            string QrText = LogReprint.IngItemNo + "|" + LogReprint.BagNo + "|" + LogReprint.LotNo;
                            int QrTextLength = QrText.Length;
                            PrintCmd = PrintCmd.Replace("{QRData}", QrTextLength.ToString("0000") + "," + QrText);
                            CmdToPrinter.Add(PrintCmd);
                            if (Print())
                            {
                                try
                                {
                                    LogReprint.IsCompleted = true;
                                    db.Entry(LogReprint).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    log.Error(ex.ToString());
                                }
                            }
                        }
                    }
                    
                    Thread.Sleep(200);
                }
                catch(Exception ex)
                {
                    log.Error(ex.ToString());
                }
            }
        }

        private void Getdata()
        {
            for(; ; )
            {
                try
                {
                    Ajinomoto_Weighing_SystemEntities db = new Ajinomoto_Weighing_SystemEntities();
                    CmdToPrinter=new List<string>();
                    ListTemPlate = new List<string>();
                    ListTemPlate.Add(PrinterSetting);
                    ListTemPlate.Add(PrinterStart);
                    ListTemPlate.Add(PrinterExp_Label);
                    ListTemPlate.Add(PrinterWeight_Label);
                    ListTemPlate.Add(PrinterBagNo_Label);
                    ListTemPlate.Add(PrinterDate_Label);
                    ListTemPlate.Add(PrinterBy_Label);
                    ListTemPlate.Add(PrinterItemNo_Label);
                    ListTemPlate.Add(PrinterLot_Label);
                    ListTemPlate.Add(PrinterDesc_Label);
                    CmdToPrinter = ListTemPlate;
                    LogPrint_MatRe LogMatRe = new LogPrint_MatRe();
                    LogMatRe = db.LogPrint_MatRe.Where(o => o.IsCompleted == false).OrderBy(o => o.Datetime).FirstOrDefault();
                    LogRePrint_MatRe LogReprint = new LogRePrint_MatRe();
                    LogReprint = db.LogRePrint_MatRe.Where(o => o.IsCompleted == false).OrderBy(o => o.Datetime).FirstOrDefault();
                    if (LogMatRe!=null && LogReprint != null)
                    {
                        if (LogMatRe.Datetime < LogReprint.Datetime)
                        {
                            CmdToPrinter.Add(PrinterExp + LogMatRe.Exp);
                            CmdToPrinter.Add(PrinterWeight + (LogMatRe.PackWeight ?? 0).ToString("00.00") + " KG");
                            CmdToPrinter.Add(PrinterBagNo + LogMatRe.BagNo);
                            CmdToPrinter.Add(PrinterDate + (DateTime.Now.Date).ToString("yyyyMMdd"));
                            CmdToPrinter.Add(PrinterBy + LogMatRe.UserName);
                            CmdToPrinter.Add(PrinterItemNo + LogMatRe.IngItemNo);
                            CmdToPrinter.Add(PrinterLot + LogMatRe.LotNo);
                            string IngDesc = db.MasterIngs.Where(o => o.IngItemNo == LogMatRe.IngItemNo).Select(o => o.IngDescripton).FirstOrDefault();
                            if (IngDesc.Length > 20)
                            {
                                IngDesc=IngDesc.Substring(0, 20);
                            }
                            CmdToPrinter.Add(PrinterDesc + IngDesc);
                            string QrText = LogMatRe.IngItemNo + "|" + LogMatRe.BagNo + "|" + LogMatRe.LotNo;
                            int QrTextLength = QrText.Length;
                            CmdToPrinter.Add(PrinterQr + QrTextLength.ToString("0000") + "," + QrText);
                            CmdToPrinter.Add(PrinterStop);
                            if (Print())
                            {
                                try
                                {
                                    LogMatRe.IsCompleted = true;
                                    db.Entry(LogMatRe).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch
                                {

                                }
                            }
                        }
                        else if (LogMatRe.Datetime > LogReprint.Datetime)
                        {
                            CmdToPrinter.Add(PrinterExp + LogReprint.LogBagSeperate.LogPrint_MatRe.Exp);
                            CmdToPrinter.Add(PrinterWeight + (LogReprint.LogBagSeperate.LogPrint_MatRe.PackWeight ?? 0).ToString("00.00") + " KG");
                            CmdToPrinter.Add(PrinterBagNo + LogReprint.BagNo);
                            CmdToPrinter.Add(PrinterDate + (DateTime.Now.Date).ToString("yyyyMMdd"));
                            CmdToPrinter.Add(PrinterBy + LogReprint.UserName);
                            CmdToPrinter.Add(PrinterItemNo + LogReprint.IngItemNo);
                            CmdToPrinter.Add(PrinterLot + LogReprint.LotNo);
                            string IngDesc = db.MasterIngs.Where(o => o.IngItemNo == LogReprint.IngItemNo).Select(o => o.IngDescripton).FirstOrDefault();
                            if (IngDesc.Length > 20)
                            {
                                IngDesc = IngDesc.Substring(0, 20);
                            }
                            CmdToPrinter.Add(PrinterDesc + IngDesc);
                            string QrText = LogReprint.IngItemNo + "|" + LogReprint.BagNo + "|" + LogReprint.LotNo;
                            int QrTextLength = QrText.Length;
                            CmdToPrinter.Add(PrinterQr + QrTextLength.ToString("0000") + "," + QrText);
                            CmdToPrinter.Add(PrinterStop);
                            if (Print())
                            {
                                try
                                {
                                    LogReprint.IsCompleted = true;
                                    db.Entry(LogReprint).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                    else if(LogReprint!=null)
                    {
                        CmdToPrinter.Add(PrinterExp + LogReprint.LogBagSeperate.LogPrint_MatRe.Exp);
                        CmdToPrinter.Add(PrinterWeight + (LogReprint.LogBagSeperate.LogPrint_MatRe.PackWeight ?? 0).ToString("00.00") + " KG");
                        CmdToPrinter.Add(PrinterBagNo + LogReprint.BagNo);
                        CmdToPrinter.Add(PrinterDate + (DateTime.Now.Date).ToString("yyyyMMdd"));
                        CmdToPrinter.Add(PrinterBy + LogReprint.UserName);
                        CmdToPrinter.Add(PrinterItemNo + LogReprint.IngItemNo);
                        CmdToPrinter.Add(PrinterLot + LogReprint.LotNo);
                        string IngDesc = db.MasterIngs.Where(o => o.IngItemNo == LogReprint.IngItemNo).Select(o => o.IngDescripton).FirstOrDefault();
                        if (IngDesc.Length > 20)
                        {
                            IngDesc = IngDesc.Substring(0, 20);
                        }
                        CmdToPrinter.Add(PrinterDesc + IngDesc);
                        string QrText = LogReprint.IngItemNo + "|" + LogReprint.BagNo + "|" + LogReprint.LotNo;
                        int QrTextLength = QrText.Length;
                        CmdToPrinter.Add(PrinterQr + QrTextLength.ToString("0000") + "," + QrText);
                        CmdToPrinter.Add(PrinterStop);
                        if (Print())
                        {
                            try
                            {
                                LogReprint.IsCompleted = true;
                                db.Entry(LogReprint).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            catch
                            {

                            }
                        }
                    }
                    else if(LogMatRe != null)
                    {
                        CmdToPrinter.Add(PrinterExp + LogMatRe.Exp);
                        CmdToPrinter.Add(PrinterWeight + (LogMatRe.PackWeight ?? 0).ToString("00.00") + " KG");
                        CmdToPrinter.Add(PrinterBagNo + LogMatRe.BagNo);
                        CmdToPrinter.Add(PrinterDate + (DateTime.Now.Date).ToString("yyyyMMdd"));
                        CmdToPrinter.Add(PrinterBy + LogMatRe.UserName);
                        CmdToPrinter.Add(PrinterItemNo + LogMatRe.IngItemNo);
                        CmdToPrinter.Add(PrinterLot + LogMatRe.LotNo);
                        string IngDesc = db.MasterIngs.Where(o => o.IngItemNo == LogMatRe.IngItemNo).Select(o => o.IngDescripton).FirstOrDefault();
                        if (IngDesc.Length > 20)
                        {
                            IngDesc = IngDesc.Substring(0, 20);
                        }
                        CmdToPrinter.Add(PrinterDesc + IngDesc);
                        string QrText = LogMatRe.IngItemNo + "|" + LogMatRe.BagNo + "|" + LogMatRe.LotNo;
                        int QrTextLength = QrText.Length;
                        CmdToPrinter.Add(PrinterQr + QrTextLength.ToString("0000") + "," + QrText);
                        CmdToPrinter.Add(PrinterStop);
                        //Debugger.Break();
                        //Debugger.Launch();
                        if (Print())
                        {
                            try
                            {
                                LogMatRe.IsCompleted = true;
                                db.Entry(LogMatRe).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            catch
                            {

                            }
                        }
                    }
                    Thread.Sleep(200);
                }
                catch
                {

                }
            }
        }

        private bool Print()
        {
            Ping pinger = new Ping();
            bool Pingable = false;
            PingReply reply = pinger.Send(_satoIp);
            Pingable = reply.Status == IPStatus.Success;
            if (Pingable)
            {
                return PrintCommandtoPrinter();
            }
            else
            {
                return false;
            }
        }
        private bool PrintCommandtoPrinter()
        {
            try
            {
                string PrinterIP = _satoIp;
                int PrinterPort = 9100;
                TcpClient client = new TcpClient();
                client.Connect(_satoIp, 9100);
                StreamWriter writer = new StreamWriter(client.GetStream());
                foreach (string line in CmdToPrinter)
                {
                    writer.Write(line);
                }
                writer.Flush();
                writer.Close();
                client.Close();

                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
