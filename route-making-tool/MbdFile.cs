using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BusDriverFile {
    abstract class Origin {
        public NodeBoundData dataHead;
        public uint flag;
        public byte distance;
        public abstract void Read(BinaryReader br);
    }
    class Node {
        public Float3 position;  //位置
        public Float3 direction; //方向
        public uint backwardIndex;//上一点编号
        public uint forwardIndex; //下一点编号(原点编号)
        public uint flag;         //存储位信息
        public byte divF;         //F分割
        public Node() : this(0x00) { }
        public Node(uint index) {
            position=new Float3();
            direction=new Float3();
            backwardIndex=0xFFFFFFFF;
            forwardIndex=index;
            flag=0x01;
            divF=0xFF;
        }
        public Node(BinaryReader br) {
            position=new Float3(br);
            direction=new Float3(br);
            backwardIndex=br.ReadUInt32();
            forwardIndex=br.ReadUInt32();
            flag=br.ReadUInt32();
            divF=br.ReadByte();
        }
    }

    class MbdFile {
        public const uint fileHead = 0xD4;
        private readonly BinaryReader br;
        public uint nodeCount;
        public uint originCount;
        public Origin[] origins;
        public Node[] nodes;

        public MbdFile(string mapPath) {
            br = new BinaryReader(new FileStream(mapPath,FileMode.Open));
            if(br.ReadUInt32()!=fileHead) System.Diagnostics.Debug.WriteLine("MBD文件格式错误");
            nodeCount=br.ReadUInt32();
            originCount=br.ReadUInt32();
            //DefLib.Load();
            //ReadFile();
            try {
                DefLib.Load();
                ReadFile();
            } catch(Exception e) {
                throw e;
            }
            br.Close();
        }

        ~MbdFile() {
            if(br!=null) br.Close();
        }

        public void ReadFile() {
            origins=new Origin[originCount];
            for(uint i = 0;i<originCount;i++) {
                uint nodeType = br.ReadUInt32();
                switch(nodeType) {
                    case 0x02: origins[i]=new Road(br); break;
                    case 0x03: origins[i]=new Prefab(br); break;
                    case 0x01: origins[i]=new Building(br); break;
                    case 0x04: origins[i]=new Model(br); break;
                    case 0x07: origins[i]=new CutPlane(br); break;
                    case 0x08: origins[i]=new Mover(br); break;
                    case 0x0B: origins[i]=new City(br); break;
                    case 0x0D: origins[i]=new QuestPoint(br); break;
                    case 0x0A: origins[i]=new NoWeather(br); break;
                    case 0x0E: origins[i]=new BusStop(br); break;
                    //case 0x0F: System.Diagnostics.Debug.WriteLine("原点:"+i+"|AnimatedModel:"+nodeType); break;
                    case 0x10: origins[i]=new MissionModel(br); break;
                    default: {
                        //System.Diagnostics.Debug.WriteLine("原点:"+i+"|错误的原点类型:"+nodeType);
                        //System.Diagnostics.Debug.WriteLine("\t\t|位置:"+br.BaseStream.Position);
                        continue;
                    }
                }
                //System.Diagnostics.Debug.WriteLine("原点:"+i+"|类型:"+origins[i].ToString()+"|位置:"+br.BaseStream.Position);
            }
            nodes=new Node[nodeCount];
            for(uint i = 0;i<nodeCount;i++) {
                nodes[i]=new Node(br);
            }
        }

        public class Road:Origin {
            public const uint nodeType = 0x02;
            public uint roadNum;
            public uint matNum;
            public uint startIndex;
            public uint endIndex;
            public RoadData dataLeft;
            public RoadData dataRight;
            public ushort centerNum;
            public float tangent;
            public uint unknown;
            public uint signCount;
            public SignData[] signs;
            public uint brushCount;
            public BrushData[] brushs;
            public Road(BinaryReader br) {
                Read(br);
            }

            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                roadNum=br.ReadUInt32();
                matNum=br.ReadUInt32();
                startIndex=br.ReadUInt32();
                endIndex=br.ReadUInt32();
                dataRight=new RoadData(br);
                dataLeft=new RoadData(br);
                centerNum=br.ReadUInt16();
                tangent=br.ReadSingle();
                unknown=br.ReadUInt32();
                signCount=br.ReadUInt32();
                if(signCount>0) {
                    signs=new SignData[signCount];
                    for(int i = 0;i<signCount;i++) {
                        signs[i]=new SignData(br);
                    }
                }
                brushCount=br.ReadUInt32();
                if(brushCount>0) {
                    brushs=new BrushData[brushCount];
                    for(int i = 0;i<brushCount;i++) {
                        brushs[i]=new BrushData(br);
                    }
                }
            }
        }

        public class RoadData {
            public ushort railNum;
            public ushort railOffset;
            public ushort modelNum;
            public ushort modelOffset;
            public ushort modelDistance;
            public ushort terrMatNum;
            public ushort terrQuad;
            public ushort terrType;
            public float terrCoef;
            public ushort tree;
            public ushort sidewalk;
            public uint height;
            public RoadData(BinaryReader br) {
                railNum=br.ReadUInt16();
                railOffset=br.ReadUInt16();
                modelNum=br.ReadUInt16();
                modelOffset=br.ReadUInt16();
                modelDistance=br.ReadUInt16();
                terrMatNum=br.ReadUInt16();
                terrQuad=br.ReadUInt16();
                terrType=br.ReadUInt16();
                terrCoef=br.ReadSingle();
                tree=br.ReadUInt16();
                sidewalk=br.ReadUInt16();
                height=br.ReadUInt32();
            }
        }
        public class SignData {
            public uint signNum;
            public float offset;
            public uint flag;
            public float posCoef;
            public float height;
            public float rotation;
            public SignSubData data1;
            public SignSubData data2;
            public SignSubData data3;
            public SignData(BinaryReader br) {
                signNum=br.ReadUInt32();
                offset=br.ReadSingle();
                flag=br.ReadUInt32();
                posCoef=br.ReadSingle();
                height=br.ReadSingle();
                rotation=br.ReadSingle();
                data1=new SignSubData(br);
                data2=new SignSubData(br);
                data3=new SignSubData(br);
            }

            public class SignSubData {
                public ulong road;
                public ulong text1;
                public ulong text2;
                public SignSubData(BinaryReader br) {
                    road=br.ReadUInt64();
                    text1=br.ReadUInt64();
                    text2=br.ReadUInt64();
                }
            }
        }
        public class BrushData {
            public uint matNum;
            public uint stampNum;
            public int tIndex;
            public int nIndex;
            public BrushData(BinaryReader br) {
                matNum=br.ReadUInt32();
                stampNum=br.ReadUInt32();
                tIndex=br.ReadInt32();
                nIndex=br.ReadInt32();
            }
        }

        public class Prefab:Origin {
            public const uint nodeType = 0x03;
            public uint prefabNum;     //路口表中编号
            public uint lookNum;       //look编号
            //public uint redIndex;      //原点编号
            public uint[] indexs; //点编号数组
            public uint unknown;          //0分隔符
            public short rotY;        //Y旋转
            public short rotZ;        //Z旋转
            public PrefabTerrain[] terrain;//地形数组
            public Prefab(BinaryReader br) {
                Read(br);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                prefabNum =br.ReadUInt32();
                lookNum=br.ReadUInt32();
                uint prefabnodeCount = DefLib.pdds[prefabNum].nodeCount;
                indexs=new uint[prefabnodeCount];
                for(uint i = 0;i<prefabnodeCount;i++) {
                    indexs[i]=br.ReadUInt32();
                }
                unknown=br.ReadUInt32();
                rotY=br.ReadInt16();
                rotZ=br.ReadInt16();
                terrain=new PrefabTerrain[prefabnodeCount];
                for(uint i = 0;i<prefabnodeCount;i++) {
                    terrain[i]=new PrefabTerrain(br);
                }
            }
        }
        public class PrefabTerrain {
            public byte terLength; //地形长度,平方
            public byte terNum;    //地形表中编号
            public uint empty;     //0分隔符
            public float terCoef;  //地形变形系数
            public ushort tree;    //植物
            public PrefabTerrain(BinaryReader br) {
                terLength=br.ReadByte();
                terNum=br.ReadByte();
                empty=br.ReadUInt32();
                terCoef=br.ReadSingle();
                tree=br.ReadUInt16();
            }
        }

        public class Building:Origin {
            public const uint nodeType = 0x01;
            public uint buildingNum;
            public uint index0;
            public uint index1;
            public uint index2;
            public uint index3;
            public float length;
            public uint firstBuilding;
            public uint fbLook;
            public uint seed;
            public float float1;
            public uint modelCount;
            public ModelData[] models;
            public uint addCount;
            public AddData[] adds;
            public Building(BinaryReader br) {
                Read(br);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                buildingNum=br.ReadUInt32();
                index0=br.ReadUInt32();
                index1=br.ReadUInt32();
                index2=br.ReadUInt32();
                index3=br.ReadUInt32();
                length=br.ReadSingle();
                firstBuilding=br.ReadUInt32();
                fbLook=br.ReadUInt32();
                seed=br.ReadUInt32();
                float1=br.ReadSingle();
                modelCount=br.ReadUInt32();
                models=new ModelData[modelCount];
                for(uint i=0;i<modelCount;i++) {
                    models[i].modelNum=br.ReadByte();
                    models[i].baseLook=br.ReadByte();
                    models[i].topLook=br.ReadByte();
                    models[i].accLook=br.ReadByte();
                }

                addCount=br.ReadUInt32();
                adds=new AddData[addCount];
                for(uint i = 0;i<addCount;i++) {
                    adds[i].baseModel=br.ReadByte();
                    adds[i].topModel=br.ReadByte();
                    adds[i].accModel=br.ReadByte();
                    adds[i].unbyte=br.ReadByte();
                }
            }
            public struct ModelData {
                public byte modelNum;
                public byte baseLook;
                public byte topLook;
                public byte accLook;
            }
            public struct AddData {
                public byte baseModel;
                public byte topModel;
                public byte accModel;
                public byte unbyte;
            }
        }
        
        public class Model:Origin {
            public const uint nodeType = 0x04;
            public uint modelNum;     //模型表中编号
            public uint matNum;       //模型变体
            public uint nodeIndex;    //原点号
            public Float3 rotation;  //旋转
            public Float3 scale;     //缩放
            public Model(BinaryReader br) {
                Read(br);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                modelNum =br.ReadUInt32();
                matNum=br.ReadUInt32();
                nodeIndex=br.ReadUInt32();
                rotation=new Float3(br);
                scale=new Float3(br);
            }
        }

        public class CutPlane:Origin {
            public const uint nodeType = 0x07;
            public uint index1;  //起点编号
            public uint index2;  //终点编号
            public CutPlane(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                index1=br.ReadUInt32();
                index2=br.ReadUInt32();
            }
            public override void Read(BinaryReader br) {
                throw new NotImplementedException();
            }
        }

        public class Mover:Origin {
            public const uint nodeType = 0x08;
            public ushort moverNum;   //mover表中编号
            public float speed;       //速度
            public float delay;       //delay at end,延迟时间
            public uint tangentCount; //控制线数量
            public float[] tangents;  //控制线长度
            public uint nodeCount;    //节点数量
            public uint[] nodeIndexs; //节点编号
            public Mover(BinaryReader br) {
                Read(br);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                moverNum=br.ReadUInt16();
                speed=br.ReadSingle();
                delay=br.ReadSingle();
                tangentCount=br.ReadUInt32();
                tangents=new float[tangentCount];
                for(uint i = 0;i<tangentCount;i++) {
                    tangents[i]=br.ReadSingle();
                }
                nodeCount=br.ReadUInt32();
                nodeIndexs=new uint[nodeCount];
                for(uint i = 0;i<nodeCount;i++) {
                    nodeIndexs[i]=br.ReadUInt32();
                }
            }
        }

        public class City:Origin {
            public const uint nodeType = 0x0B;
            public ulong cityName; //城市名,long存储字符串
            public float width;    //宽度
            public float height;   //高度
            public uint nodeIndex; //原点号
            public City(BinaryReader br) {
                Read(br);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                cityName =br.ReadUInt64();
                width=br.ReadSingle();
                height=br.ReadSingle();
                nodeIndex=br.ReadUInt32();
            }
        }

        public class QuestPoint:Origin {
            public const uint nodeType = 0x0D;
            public ulong missionName; //线路名,long存储字符串
            public uint nodeCount;    //节点数
            public uint[] nodeIndexs;  //节点编号数组
            public QuestPoint(BinaryReader br) {
                Read(br);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                missionName =br.ReadUInt64();
                nodeCount=br.ReadUInt32();
                nodeIndexs=new uint[nodeCount];
                for(int i = 0;i<nodeCount;i++) {
                    nodeIndexs[i]=br.ReadUInt32();
                }
            }
        }

        public class NoWeather:Origin {
            public const uint nodeType = 0x0A;
            public float width;    //宽度
            public float height;   //高度
            public uint nodeIndex; //原点号
            public NoWeather(BinaryReader br) {
                Read(br);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                width =br.ReadSingle();
                height=br.ReadSingle();
                nodeIndex=br.ReadUInt32();
            }
        }

        public class BusStop:Origin {
            public const uint nodeType = 0x0E;
            public ushort stopNum; //车站表中号
            public uint stopID;    //站点号
            public uint nodeIndex; //原点编号
            public static Dictionary<uint,BusStop> stops=new Dictionary<uint, BusStop>();
            public BusStop(BinaryReader br) {
                Read(br);
                stops.TryAdd(stopID,this);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                stopNum =br.ReadUInt16();
                stopID=br.ReadUInt32();
                nodeIndex=br.ReadUInt32();
            }
        }

        public class MissionModel:Origin {
            public const uint nodeType = 0x10;
            public uint modelNum;       //模型表中编号
            public uint matNum;         //模型变体号
            public uint nodeIndex;      //原点编号
            public Float3 rotation;    //旋转
            public Float3 scale;       //缩放
            public uint missionCount;   //线路数量
            public ulong[] missionName; //线路名,long存储字符串
            public MissionModel(BinaryReader br) {
                Read(br);
            }
            public override void Read(BinaryReader br) {
                dataHead=new NodeBoundData(br);
                flag=br.ReadUInt32();
                distance=br.ReadByte();
                modelNum =br.ReadUInt32();
                matNum=br.ReadUInt32();
                nodeIndex=br.ReadUInt32();
                rotation=new Float3(br);
                scale=new Float3(br);
                missionCount=br.ReadUInt32();
                missionName=new ulong[missionCount];
                for(int i = 0;i<missionCount;i++) {
                    missionName[i]=br.ReadUInt64();
                }
            }
        }
    }
}
