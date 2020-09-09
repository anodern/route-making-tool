using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BusDriverFile {
    class PrefabFile {
        public const uint fileHead = 0x0F;
        public uint nodeCount;

        private BinaryReader br;
        public PrefabFile(string path) {
            try {
                br=new BinaryReader(new FileStream(path,FileMode.Open));
                if(br.ReadUInt32()!=fileHead) System.Diagnostics.Debug.WriteLine("PDD文件格式错误");
                nodeCount=br.ReadUInt32();
                br.Close();
            }catch(Exception e) {
                throw e;
            }
        }
    }
    
}
