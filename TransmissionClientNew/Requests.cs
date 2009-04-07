﻿using System;
using System.Collections.Generic;
using System.Text;
using Jayrock.Json;
using System.Windows.Forms;
using System.IO;

namespace TransmissionRemoteDotnet
{
    public enum ReannounceMode
    {
        All,
        Specific,
        RecentlyActive
    }

    public class Requests
    {
        public static JsonObject SessionGet()
        {
            return CreateBasicObject(ProtocolConstants.METHOD_SESSIONGET, ResponseTag.SessionGet);
        }

        public static JsonObject Generic(string method, JsonArray ids)
        {
            JsonObject request = CreateBasicObject(method);
            JsonObject args = GetArgObject(request);
            if (ids != null)
            {
                args.Put(ProtocolConstants.KEY_IDS, ids);
            }
            return request;
        }

        public static JsonObject Reannounce(ReannounceMode mode, JsonArray ids)
        {
            JsonObject request = CreateBasicObject(ProtocolConstants.METHOD_TORRENTREANNOUNCE);
            JsonObject arguments = GetArgObject(request);
            switch (mode)
            {
                case ReannounceMode.RecentlyActive:
                    arguments.Put(ProtocolConstants.KEY_IDS, ProtocolConstants.VALUE_RECENTLY_ACTIVE);
                    break;
                case ReannounceMode.All:
                    arguments.Put(ProtocolConstants.KEY_IDS, new JsonArray());
                    break;
                case ReannounceMode.Specific:
                    arguments.Put(ProtocolConstants.KEY_IDS, ids);
                    break;
            }
            return request;
        }

        public static JsonObject RemoveTorrent(JsonArray ids, bool delete)
        {
            JsonObject request = CreateBasicObject(ProtocolConstants.METHOD_TORRENTREMOVE);
            JsonObject arguments = GetArgObject(request);
            if (delete && Program.DaemonDescriptor.Revision >= 7331)
            {
                arguments.Put(ProtocolConstants.FIELD_DELETELOCALDATA, true);
            }
            arguments.Put(ProtocolConstants.KEY_IDS, ids);
            return request;
        }

        public static JsonObject Files(int id)
        {
            return Files(id, false);
        }

        private static JsonObject Files(int id, bool includePriorities)
        {
            JsonObject request = CreateBasicObject(ProtocolConstants.METHOD_TORRENTGET, ResponseTag.UpdateFiles);
            JsonObject arguments = GetArgObject(request);
            JsonArray ids = new JsonArray();
            ids.Push(id);
            arguments.Put(ProtocolConstants.KEY_IDS, ids);
            JsonArray fields = new JsonArray();
            fields.Put(ProtocolConstants.FIELD_FILES);
            fields.Put(ProtocolConstants.FIELD_ID);
            if (includePriorities)
            {
                fields.Put(ProtocolConstants.FIELD_PRIORITIES);
                fields.Put(ProtocolConstants.FIELD_WANTED);
            }
            arguments.Put(ProtocolConstants.KEY_FIELDS, fields);
            return request;
        }

        public static JsonObject FilesAndPriorities(int id)
        {
            return Files(id, true);
        }

        public static JsonObject TorrentAddByFile(string file, bool deleteAfter)
        {
            FileStream inFile = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] binaryData = new Byte[inFile.Length];
            if (inFile.Read(binaryData, 0, (int)inFile.Length) < 1)
            {
                throw new Exception("Empty file");
            }
            inFile.Close();
            JsonObject request = CreateBasicObject(ProtocolConstants.METHOD_TORRENTADD);
            JsonObject arguments = GetArgObject(request);
            arguments.Put(ProtocolConstants.FIELD_METAINFO, Convert.ToBase64String(binaryData, 0, binaryData.Length));
            arguments.Put(ProtocolConstants.FIELD_PAUSED, LocalSettingsSingleton.Instance.StartPaused);
            if (deleteAfter && File.Exists(file))
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }
            return request;
        }

        public static JsonObject TorrentAddByUrl(string url)
        {
            JsonObject request = CreateBasicObject(ProtocolConstants.METHOD_TORRENTADD);
            JsonObject arguments = GetArgObject(request);
            arguments.Put(ProtocolConstants.FIELD_FILENAME, url);
            arguments.Put(ProtocolConstants.FIELD_PAUSED, LocalSettingsSingleton.Instance.StartPaused);
            return request;
        }

        public static JsonObject CreateBasicObject(string method)
        {
            return CreateBasicObject(method, ResponseTag.DoNothing);
        }

        public static JsonObject CreateBasicObject(string method, ResponseTag tag)
        {
            JsonObject obj = new JsonObject();
            obj.Put(ProtocolConstants.KEY_TAG, (int)tag);
            obj.Put(ProtocolConstants.KEY_METHOD, method);
            obj.Put(ProtocolConstants.KEY_ARGUMENTS, new JsonObject());
            return obj;
        }

        public static JsonObject GetArgObject(JsonObject obj)
        {
            return (JsonObject)obj[ProtocolConstants.KEY_ARGUMENTS];
        }

        public static JsonObject SessionStats()
        {
            return CreateBasicObject(ProtocolConstants.METHOD_SESSIONSTATS, ResponseTag.SessionStats);
        }

        public static JsonObject BlocklistUpdate()
        {
            return CreateBasicObject(ProtocolConstants.METHOD_BLOCKLISTUPDATE);
        }

        public static JsonObject TorrentGet()
        {
            JsonObject request = CreateBasicObject(ProtocolConstants.METHOD_TORRENTGET, ResponseTag.TorrentGet);
            JsonObject arguments = GetArgObject(request);
            JsonArray fields = new JsonArray(new string[]{
                ProtocolConstants.FIELD_ID,
                ProtocolConstants.FIELD_ADDEDDATE,
                ProtocolConstants.FIELD_HAVEVALID,
                ProtocolConstants.FIELD_HAVEUNCHECKED,
                ProtocolConstants.FIELD_ETA,
                ProtocolConstants.FIELD_RECHECKPROGRESS,
                ProtocolConstants.FIELD_LEECHERS,
                ProtocolConstants.FIELD_RATEDOWNLOAD,
                ProtocolConstants.FIELD_RATEUPLOAD,
                ProtocolConstants.FIELD_SEEDERS,
                ProtocolConstants.FIELD_TOTALSIZE,
                ProtocolConstants.FIELD_UPLOADEDEVER,
                ProtocolConstants.FIELD_STATUS,
                ProtocolConstants.FIELD_LEFTUNTILDONE,
                ProtocolConstants.FIELD_ANNOUNCEURL,
                ProtocolConstants.FIELD_DOWNLOADLIMIT,
                ProtocolConstants.FIELD_DOWNLOADLIMITMODE,
                ProtocolConstants.FIELD_UPLOADLIMIT,
                ProtocolConstants.FIELD_UPLOADLIMITED,
                ProtocolConstants.FIELD_UPLOADLIMITMODE,
                ProtocolConstants.FIELD_SPEEDLIMITDOWN,
                ProtocolConstants.FIELD_DOWNLOADLIMITED,
                ProtocolConstants.FIELD_SPEEDLIMITDOWNENABLED,
                ProtocolConstants.FIELD_SPEEDLIMITUP,
                ProtocolConstants.FIELD_SPEEDLIMITUPENABLED,
                ProtocolConstants.FIELD_NAME,
                ProtocolConstants.FIELD_ERRORSTRING,
                ProtocolConstants.FIELD_PEERS,
                ProtocolConstants.FIELD_PEERSGETTINGFROMUS,
                ProtocolConstants.FIELD_PEERSSENDINGTOUS,
                ProtocolConstants.FIELD_PIECECOUNT,
                ProtocolConstants.FIELD_PIECES,
                ProtocolConstants.FIELD_MAXCONNECTEDPEERS,
                ProtocolConstants.FIELD_COMMENT,
                ProtocolConstants.FIELD_SWARMSPEED,
                ProtocolConstants.FIELD_DATECREATED,
                ProtocolConstants.FIELD_CREATOR,
                ProtocolConstants.FIELD_TRACKERS,
                ProtocolConstants.FIELD_HASHSTRING,
                ProtocolConstants.FIELD_DOWNLOADDIR,
                ProtocolConstants.FIELD_SEEDRATIOLIMIT,
                //ProtocolConstants.FIELD_SEEDRATIOLIMITED,
                ProtocolConstants.FIELD_SEEDRATIOMODE,
                ProtocolConstants.FIELD_HONORSSESSIONLIMITS,
                /*"sizeWhenDone",, "isPrivate",
                "hashString", "error",,
                "peersKnown"*/
            });
            arguments.Put(ProtocolConstants.KEY_FIELDS, fields);
            return request;
        }
    }
}