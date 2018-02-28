﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using Microsoft.AspNet.SignalR;
using KIAPI.Classes;
using MySql.Data.MySqlClient;

namespace KIAPI.Asyncs
{


    public class GameThreadWorker : IDisposable
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private System.Threading.Timer timer_update_markers;
        private System.Threading.Timer timer_update_players;
        private System.Threading.Timer timer_update_server;
        private IHubContext hub;
        private IDAL dal;
        private const int UPDATE_MARKERS_PERIOD = 10000;
        private const int UPDATE_PLAYERS_PERIOD = 5000;
        private const int UPDATE_SERVER_PERIOD = 30000;
        private MySqlConnection conn;
        public int ServerID { get; private set; }

        public GameThreadWorker(int serverID)
        {
            logger.Info("Game Thread Worker Starting (ServerID: " + serverID + ")");
            conn = new MySqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBMySqlConnect"].ConnectionString);
            conn.Open();
            hub = GlobalHost.ConnectionManager.GetHubContext<KIAPI.Hubs.GameHub>();
            dal = new DAL();
            this.ServerID = serverID;
            timer_update_markers = new System.Threading.Timer(this.UpdateMarkers, null, 0, UPDATE_MARKERS_PERIOD);
            timer_update_players = new System.Threading.Timer(this.UpdatePlayers, null, 0, UPDATE_PLAYERS_PERIOD);
            timer_update_server = new System.Threading.Timer(this.UpdateServer, null, 0, UPDATE_SERVER_PERIOD);
        }

        public void Pause()
        {
            timer_update_markers.Change(Timeout.Infinite, Timeout.Infinite);
            timer_update_players.Change(Timeout.Infinite, Timeout.Infinite);
            timer_update_server.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Resume()
        {
            timer_update_markers.Change(0, UPDATE_MARKERS_PERIOD);
            timer_update_players.Change(0, UPDATE_PLAYERS_PERIOD);
            timer_update_server.Change(0, UPDATE_SERVER_PERIOD);
        }

        public void Dispose()
        {
            logger.Info("Game Thread Worker Closing (ServerID: " + this.ServerID + ")");
            timer_update_markers.Dispose();
            timer_update_players.Dispose();
            timer_update_server.Dispose();
            conn.Close();
        }

        private void UpdateMarkers(object state)
        {
            hub.Clients.Group(ServerID.ToString())
                .UpdateMarkers(dal.GetMarkers(this.ServerID));
        }

        private void UpdatePlayers(object state)
        {
            hub.Clients.Group(ServerID.ToString())
                .UpdateOnlinePlayers(dal.GetOnlinePlayers(this.ServerID));
        }

        private void UpdateServer(object state)
        {
            hub.Clients.Group(ServerID.ToString())
                .UpdateServer(dal.GetServerInfo(this.ServerID));
        }
    }
}