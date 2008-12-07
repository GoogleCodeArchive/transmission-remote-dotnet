﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Jayrock.Json;

namespace TransmissionClientNew
{
    /* From remote/daemon.c
     * static const char * details_keys[] = {
    "activityDate", "addedDate", "announceResponse", "announceURL",
    "comment", "corruptEver", "creator", "dateCreated", "doneDate",
    "haveValid", "errorString", "eta", "hashString", "haveUnchecked",
    "haveValid", "id", "isPrivate", "lastAnnounceTime", "lastScrapeTime",
    "leechers", "leftUntilDone", "name", "nextAnnounceTime", "nextScrapeTime",
    "peersConnected", "peersGettingFromUs", "peersSendingToUs",
    "pieceCount", "pieceSize", "rateDownload", "rateUpload", "recheckProgress",
    "scrapeResponse", "seeders", "sizeWhenDone", "sizeWhenDone", "startDate",
    "status", "timesCompleted", "totalSize", "uploadedEver",
    "webseeds", "webseedsSendingToUs" };
    */

    public enum TorrentStatus
    {
        Downloading = 4,
        Seeding = 8
    }

    public class Torrent
    {
        public ListViewItem item;
        public JsonObject info;
        public long updateSerial;

        public Torrent(JsonObject info)
        {
            this.updateSerial = Program.updateSerial;
            this.info = info;
            item = new ListViewItem((string)info["name"]);
            item.ToolTipText = item.Name;
            item.Tag = this;
            item.SubItems.Add(this.Status);
            item.SubItems.Add(this.TotalSizeString);
            double percentage = this.Percentage;
            item.SubItems.Add(percentage.ToString() + "%");
            item.SubItems.Add(percentage >= 100 ? "N/A" : this.DownloadRate);
            item.SubItems.Add(this.UploadRate);
            item.SubItems.Add(this.ETA);
            item.SubItems.Add(this.HaveValidString);
            item.SubItems.Add(this.UploadedString);
            item.SubItems.Add(this.RatioString);
            item.SubItems.Add(this.Seeders + " (" + this.PeersSendingToUs + ")");
            item.SubItems.Add(this.Leechers + " (" + this.PeersGettingFromUs + ")");
            item.SubItems.Add(((string)info["announceURL"]));
            item.SubItems.Add(this.Added);
            Program.torrentIndex[this.Id] = this;
            Add();
        }

        private delegate void AddDelegate();
        private void Add()
        {
            Form1 form = Program.form;
            if (form.InvokeRequired)
            {
                form.Invoke(new AddDelegate(this.Add));
            }
            else
            {
                form.TorrentListView.Items.Add(item);
                form.StripeListView();
            }
        }

        private delegate void RemoveDelegate();
        public void Remove()
        {
            Form1 form = Program.form;
            if (form.InvokeRequired)
            {
                form.Invoke(new RemoveDelegate(this.Remove));
            }
            else
            {
                if (Program.infoDialogs.ContainsKey(this.Id))
                {
                    TorrentInfoDialog dialog = Program.infoDialogs[this.Id];
                    if (dialog != null && dialog.t == this)
                    {
                        dialog.Close();
                    }
                }
                Program.form.TorrentListView.Items.Remove(item);
                Program.form.StripeListView();
            }
        }


        private delegate void UpdateDelegate(JsonObject info);
        public void Update(JsonObject info)
        {
            Form1 form = Program.form;
            if (form.InvokeRequired)
            {
                form.Invoke(new UpdateDelegate(this.Update), info);
            }
            else
            {
                if (Program.form.NotifyIcon.Visible == true && ((JsonNumber)this.info["status"]).ToInt16() == (short)TorrentStatus.Downloading && ((JsonNumber)this.info["leftUntilDone"]).ToInt64() > 0 && ((JsonNumber)info["leftUntilDone"]).ToInt64() == 0)
                {
                    Program.form.NotifyIcon.ShowBalloonTip(4, (string)info["name"], "This torrent has finished downloading.", ToolTipIcon.Info);
                }
                this.info = info;
                item.SubItems[0].Text = (string)info["name"];
                item.ToolTipText = item.SubItems[0].Text;
                item.SubItems[1].Text = this.Status;
                item.SubItems[2].Text = Toolbox.GetFileSize(this.TotalSize);
                double percentage = this.Percentage;
                item.SubItems[3].Text = percentage.ToString() + "%";
                item.SubItems[4].Text = percentage >= 100 ? "N/A" : this.DownloadRate;
                item.SubItems[5].Text = this.UploadRate;
                item.SubItems[6].Text = this.ETA;
                item.SubItems[7].Text = this.HaveValidString;
                item.SubItems[8].Text = this.UploadedString;
                item.SubItems[9].Text = this.RatioString;
                item.SubItems[10].Text = this.Seeders + " (" + this.PeersSendingToUs + ")";
                item.SubItems[11].Text = this.Leechers + " (" + this.PeersGettingFromUs + ")";
                this.updateSerial = Program.updateSerial;
                if (Program.infoDialogs.ContainsKey(this.Id))
                {
                    Program.infoDialogs[this.Id].UpdateInfo();
                }
            }
        }

        public string Status
        {
            get
            {
                switch (this.StatusCode)
                {
                    case 1:
                        return "Waiting to check";
                    case 2:
                        return "Checking";
                    case 4:
                        return "Downloading";
                    case 8:
                        return "Seeding";
                    case 16:
                        return "Paused";
                    default:
                        return "Unknown";
                }
            }
        }

        public short StatusCode
        {
            get
            {
                return ((JsonNumber)info["status"]).ToInt16();
            }
        }

        public int Id
        {
            get
            {
                return ((JsonNumber)info["id"]).ToInt32();
            }
        }

        public string ETA
        {
            get
            {
                if (this.Percentage >= 100)
                {
                    return "N/A";
                }
                else
                {
                    double eta = ((JsonNumber)info["eta"]).ToDouble();
                    if (eta > 0)
                    {
                        return TimeSpan.FromSeconds(eta).ToString();
                    }
                    else
                    {
                        return "Unknown";
                    }
                }
            }
        }

        public double Percentage
        {
            get
            {
                return Toolbox.CalcPercentage(this.HaveValid, this.TotalSize);
            }
        }

        public string Seeders
        {
            get
            {
                return ((JsonNumber)info["seeders"]).ToString();
            }
        }

        public string Leechers
        {
            get
            {
                return ((JsonNumber)info["leechers"]).ToString();
            }
        }

        public string SwarmSpeed
        {
            get
            {
                return Toolbox.GetFileSize(((JsonNumber)info["swarmSpeed"]).ToInt64()) + "/s";
            }
        }

        public long TotalSize
        {
            get
            {
                return ((JsonNumber)info["totalSize"]).ToInt64();
            }
        }

        public string TotalSizeString
        {
            get
            {
                return Toolbox.GetFileSize(this.TotalSize);
            }
        }

        public string Added
        {
            get
            {
                return Toolbox.DateFromEpoch(((JsonNumber)info["addedDate"]).ToDouble()).ToString();
            }
        }

        public string PeersSendingToUs
        {
            get
            {
                return ((JsonNumber)info["peersSendingToUs"]).ToString();
            }
        }

        public string PeersGettingFromUs
        {
            get
            {
                return ((JsonNumber)info["peersGettingFromUs"]).ToString();
            }
        }

        public string Created
        {
            get
            {
                return Toolbox.DateFromEpoch(((JsonNumber)info["dateCreated"]).ToDouble()).ToString();
            }
        }

        public long Uploaded
        {
            get
            {
                return ((JsonNumber)info["uploadedEver"]).ToInt64();
            }
        }

        public string UploadedString
        {
            get
            {
                return Toolbox.GetFileSize(this.Uploaded);
            }
        }

        public long HaveValid
        {
            get
            {
                return ((JsonNumber)info["haveValid"]).ToInt64();
            }
        }

        public string HaveValidString
        {
            get
            {
                return Toolbox.GetFileSize(this.HaveValid);
            }
        }

        public string DownloadRate
        {
            get
            {
                return Toolbox.GetFileSize(((JsonNumber)info["rateDownload"]).ToInt64()) + "/s";
            }
        }

        public string UploadRate
        {
            get
            {
                return Toolbox.GetFileSize(((JsonNumber)info["rateUpload"]).ToInt64()) + "/s";
            }
        }

        public decimal Ratio
        {
            get
            {
                return Toolbox.CalcRatio(this.Uploaded, this.HaveValid);
            }
        }

        public string RatioString
        {
            get
            {
                decimal ratio = this.Ratio;
                return ratio < 0 ? "∞" : ratio.ToString();
            }
        }
    }
}