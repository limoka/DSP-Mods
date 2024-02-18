using System;
using System.Collections.Generic;
using System.IO;
using NebulaAPI;
using NebulaAPI.GameState;
using NebulaAPI.Interfaces;
using NebulaAPI.Networking;
using NebulaAPI.Packets;
using UnityEngine;

namespace BlueprintTweaks
{
    public class ReformPasteEventPacket
    {
        public int PlanetId { get; set; }
        public byte[] ReformData { get; set; }
        public int AuthorId { get; set; }

        public ReformPasteEventPacket() { }

        public ReformPasteEventPacket(int planetId, List<ReformData> reformPreviews, Color[] colors, int playerId)
        {
            AuthorId = playerId;
            PlanetId = planetId;

            using (IWriterProvider writer = NebulaModAPI.GetBinaryWriter())
            {
                writer.BinaryWriter.Write(reformPreviews.Count);
                foreach (ReformData data in reformPreviews)
                {
                    writer.BinaryWriter.Write(data.latitude);
                    writer.BinaryWriter.Write(data.longitude);
                    writer.BinaryWriter.Write(data.type);
                    writer.BinaryWriter.Write(data.color);
                }
                
                writer.BinaryWriter.Write(colors != null);
                if (colors != null)
                {
                    for (int i = 0; i < 16; i++)
                    {
                        writer.BinaryWriter.Write(colors[i].r);
                        writer.BinaryWriter.Write(colors[i].g);
                        writer.BinaryWriter.Write(colors[i].b);
                        writer.BinaryWriter.Write(colors[i].a);
                    }
                }

                ReformData = writer.CloseAndGetBytes();
            }
        }

        public void GetData(out List<ReformData> reforms, out Color[] colors)
        {
            reforms = new List<ReformData>();
            colors = null;

            using (IReaderProvider reader = NebulaModAPI.GetBinaryReader(ReformData))
            {
                int count = reader.BinaryReader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    ReformData data = new ReformData
                    {
                        latitude = reader.BinaryReader.ReadSingle(),
                        longitude = reader.BinaryReader.ReadSingle(),
                        type = reader.BinaryReader.ReadInt32(),
                        color = reader.BinaryReader.ReadInt32()
                    };
                    reforms.Add(data);
                }

                if (reader.BinaryReader.ReadBoolean())
                {
                    colors = new Color[16];
                    for (int i = 0; i < 16; i++)
                    {
                        colors[i] = new Color(
                            reader.BinaryReader.ReadSingle(),
                            reader.BinaryReader.ReadSingle(),
                            reader.BinaryReader.ReadSingle(),
                            reader.BinaryReader.ReadSingle());
                    }
                }
            }

            
        }
    }

    [RegisterPacketProcessor]
    public class ReformPasteEventHandler : BasePacketProcessor<ReformPasteEventPacket>
    {
        public override void ProcessPacket(ReformPasteEventPacket packet, INebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            if (planet.factory == null)
            {
                return;
            }

            PlayerAction_Build pab = GameMain.mainPlayer.controller != null ? GameMain.mainPlayer.controller.actionBuild : null;
            BuildTool_BlueprintPaste buildTool = pab.blueprintPasteTool;

            IFactoryManager factoryManager = NebulaModAPI.MultiplayerSession.Factories;

            if (buildTool == null) return;
            
            factoryManager.TargetPlanet = packet.PlanetId;
            factoryManager.PacketAuthor = packet.AuthorId;

            PlanetFactory tmpFactory = null;
            bool loadExternalPlanetData = GameMain.localPlanet?.id != planet.id;

            if (loadExternalPlanetData)
            {
                tmpFactory = buildTool.factory;
                factoryManager.AddPlanetTimer(packet.PlanetId);
            }

            factoryManager.EventFactory = planet.factory;

            buildTool.factory = planet.factory;
            pab.factory = planet.factory;

            using (factoryManager.IsIncomingRequest.On())
            {
                packet.GetData(out List<ReformData> reforms, out Color[] colors);
                BlueprintPasteExtension.CalculatePositions(buildTool, reforms, colors);
            }

            if (loadExternalPlanetData)
            {
                buildTool.factory = tmpFactory;
                pab.factory = tmpFactory;
            }

            GameMain.mainPlayer.mecha.buildArea = Configs.freeMode.mechaBuildArea;
            factoryManager.EventFactory = null;

            factoryManager.TargetPlanet = NebulaModAPI.PLANET_NONE;
            factoryManager.PacketAuthor = NebulaModAPI.AUTHOR_NONE;
        }
    }
}