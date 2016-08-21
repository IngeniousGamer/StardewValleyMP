﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using StardewValley;
using StardewModdingAPI;
using StardewValleyMP.Vanilla;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValleyMP.Packets
{
    // Server -> Client
    // Tell the client about everyone else.
    public class OtherFarmerDataPacket : Packet
    {
        public Dictionary< byte, string > others = new Dictionary< byte, string >();

        public OtherFarmerDataPacket()
            : base(ID.OtherFarmerData)
        {
        }

        protected override void read(BinaryReader reader)
        {
            others.Clear();

            byte amount = reader.ReadByte();
            for ( byte i = 0; i < amount; ++i )
            {
                byte id = reader.ReadByte();
                string data = reader.ReadString();
                others.Add(id, data);
            }
        }

        protected override void write(BinaryWriter writer)
        {
            writer.Write((byte)others.Count);
            foreach ( KeyValuePair< byte, string > entry in others )
            {
                writer.Write(entry.Key);
                writer.Write(entry.Value);
            }
        }

        public override void process( Client client )
        {
            Log.Async("Got farmer data for other players.");

            client.others.Clear();
            foreach (KeyValuePair<byte, string> other in others)
            {
                Farmer farmer = (Farmer)SaveGame.farmerSerializer.Deserialize(Util.stringStream(other.Value));
                farmer.uniqueMultiplayerID += 1 + client.id; // For IsMainPlayer

                //Farmer oldPlayer = Game1.player;
                NewSaveGame.loadDataToFarmer(farmer, farmer);
                //Game1.player = oldPlayer; // Seriously, why does this get reassigned in there?

                client.others.Add(other.Key, farmer);

                if ( other.Key == 0 )
                {
                    string[] checkMail = new string[] { "ccCraftsRoom", "ccBoilerRoom", "ccVault", "ccFishTank", "ccBulletin", "ccPantry", "JojaMember" };
                    foreach ( string mail in checkMail )
                    {
                        if (farmer.mailReceived.Contains(mail) && !SaveGame.loaded.player.mailReceived.Contains(mail))
                            SaveGame.loaded.player.mailReceived.Add(mail);
                    }
                    // Maybe more
                }
            }
        }
    }
}
