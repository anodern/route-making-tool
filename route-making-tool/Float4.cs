namespace BusDriverFile {
    class Float4 {
        public float w;
        public float x;
        public float y;
        public float z;
        public Float4() {
            w=1.0f;
            x=0.0f;
            y=0.0f;
            z=0.0f;
        }
        public Float4(float w,float x,float y,float z) {
            this.w=w;
            this.x=x;
            this.y=y;
            this.z=z;
        }
        public Float4(System.IO.BinaryReader br) {
            w=br.ReadSingle();
            x=br.ReadSingle();
            y=br.ReadSingle();
            z=br.ReadSingle();
        }
        public override string ToString() {
            return string.Format("({0:g},{1:g},{2:g},{3:g})",w,x,y,z);
        }
    }
}
