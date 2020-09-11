namespace BusDriverFile {
    class Float3 {
        public float x;
        public float y;
        public float z;
        public Float3() {
            x=0.0f;
            y=0.0f;
            z=0.0f;
        }
        public Float3(float x,float y,float z) {
            this.x=x;
            this.y=y;
            this.z=z;
        }
        public Float3(System.IO.BinaryReader br) {
            x=br.ReadSingle();
            y=br.ReadSingle();
            z=br.ReadSingle();
        }
        public override string ToString() {
            return string.Format("({0:g},{1:g},{2:g})",x,y,z);
        }
    }
}
