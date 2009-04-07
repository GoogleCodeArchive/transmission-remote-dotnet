﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Jayrock.Json;
using System.Net;
using System.IO;
using System.Globalization;
using System.Reflection;

namespace TransmissionRemoteDotnet
{
    public class Toolbox
    {
        private const int STRIPE_OFFSET = 15;
        public static readonly IFormatProvider NUMBER_FORMAT = (new CultureInfo("en-GB")).NumberFormat;

        public static decimal ToProgress(object o)
        {
            return Math.Round(ToDecimal(o), 2);
        }

        public static double ToDouble(object o)
        {
            if (o.GetType().Equals(typeof(string)))
            {
                return double.Parse((string)o, NUMBER_FORMAT);
            }
            else
            {
                return ((JsonNumber)o).ToDouble();
            }
        }

        public static long ToLong(object o)
        {
            return ((JsonNumber)o).ToInt64();
        }

        public static int ToInt(object o)
        {
            return ((JsonNumber)o).ToInt32();
        }

        public static decimal ToDecimal(object o)
        {
            if (o.GetType().Equals(typeof(string)))
            {
                return decimal.Parse((string)o, NUMBER_FORMAT);
            }
            else
            {
                return ((JsonNumber)o).ToDecimal();
            }
        }

        public static void CopyListViewToClipboard(ListView listView)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < listView.Columns.Count; i++)
            {
                sb.Append(listView.Columns[i].Text);
                if (i != listView.Columns.Count - 1)
                {
                    sb.Append(',');
                }
                else
                {
                    sb.Append(System.Environment.NewLine);
                }
            }
            lock (listView)
            {
                foreach (ListViewItem item in listView.SelectedItems)
                {
                    for (int i = 0; i < item.SubItems.Count; i++)
                    {
                        System.Windows.Forms.ListViewItem.ListViewSubItem si = item.SubItems[i];
                        sb.Append(si.Text.Contains(",") ? "\""+si.Text+"\"" : si.Text);
                        if (i != item.SubItems.Count - 1)
                        {
                            sb.Append(',');
                        }
                        else
                        {
                            sb.Append(System.Environment.NewLine);
                        }
                    }
                }
            }
            Clipboard.SetText(sb.ToString());
        }

        public static void StripeListView(ListView list)
        {
            Color window = SystemColors.Window;
            lock (list)
            {
                list.SuspendLayout();
                foreach (ListViewItem item in list.Items)
                {
                    item.BackColor = item.Index % 2 == 1 ?
                        Color.FromArgb(window.R - STRIPE_OFFSET,
                            window.G - STRIPE_OFFSET,
                            window.B - STRIPE_OFFSET)
                        : window;
                }
                list.ResumeLayout();
            }
        }

        public static short ToShort(object o)
        {
            return ((JsonNumber)o).ToInt16();
        }

        public static Boolean ToBool(object o)
        {
            if (o.GetType().Equals(typeof(Boolean)))
            {
                return (Boolean)o;
            }
            else
            {
                return ((JsonNumber)o).ToBoolean();
            }
        }

        public static DateTime DateFromEpoch(double e)
        {
            DateTime epoch = new DateTime(1970, 1, 1);
            return epoch.Add(TimeSpan.FromSeconds(e));
        }

        public static decimal CalcPercentage(long x, long total)
        {
            if (total > 0)
            {
                return Math.Round((x / (decimal)total) * 100, 2);
            }
            else
            {
                return 100;
            }
        }

        public static decimal CalcRatio(long upload_total, long download_total)
        {
            if (download_total <= 0 || upload_total <= 0)
            {
                return -1;
            }
            else
            {
                return Math.Round((decimal)upload_total / download_total, 3);
            }
        }

        public static string KbpsString(int rate)
        {
            return String.Format("{0} {1}/{2}", rate, OtherStrings.KilobyteShort, OtherStrings.Second.ToLower()[0]);
        }
        
        public static string FormatTimespanLong(TimeSpan span)
        {
            return String.Format("{0}{1} {2}{3} {4}{5} {6}{7}", new object[] { span.Days, OtherStrings.Day.ToLower()[0], span.Hours, OtherStrings.Hour.ToLower()[0], span.Minutes, OtherStrings.Minute.ToLower()[0], span.Seconds, OtherStrings.Second.ToLower()[0] });
        }

        public static string GetSpeed(long bytes)
        {
            return String.Format("{0}/{1}", GetFileSize(bytes), OtherStrings.Second.ToLower()[0]);
        }

        public static string GetSpeed(object o)
        {
            return GetSpeed(ToLong(o));
        }

        public static string GetFileSize(long bytes)
        {
            if (bytes >= 1073741824)
            {
                Decimal size = Decimal.Divide(bytes, 1073741824);
                return String.Format("{0:##.##} {1}", size, OtherStrings.GigabyteShort);
            }
            else if (bytes >= 1048576)
            {
                Decimal size = Decimal.Divide(bytes, 1048576);
                return String.Format("{0:##.##} {1}", size, OtherStrings.MegabyteShort);
            }
            else if (bytes >= 1024)
            {
                Decimal size = Decimal.Divide(bytes, 1024);
                return String.Format("{0:##.##} {1}", size, OtherStrings.KilobyteShort);
            }
            else if (bytes > 0 & bytes < 1024)
            {
                Decimal size = bytes;
                return String.Format("{0:##.##} {1}", size, OtherStrings.Byte[0]);
            }
            else
            {
                return "0 " + OtherStrings.Byte[0];
            }
        }

        public static string SupportFilePath(string file)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), file);
        }

        public static void SelectAll(ListView lv)
        {
            lock (lv)
            {
                lv.SuspendLayout();
                foreach (ListViewItem item in lv.Items)
                {
                    item.Selected = true;
                }
                lv.ResumeLayout();
            }
        }
    }
}