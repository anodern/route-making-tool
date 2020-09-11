namespace BusDriverFile {
    class Float2 {
        public float x;
        public float y;
        public Float2() {
            x=0.0f;
            y=0.0f;
        }
        public Float2(float x,float y) {
            this.x=x;
            this.y=y;
        }
        public Float2(System.IO.BinaryReader br) {
            x=br.ReadSingle();
            y=br.ReadSingle();
        }
        public override string ToString() {
            return string.Format("({0:g},{1:g})",x,y);
        }
    }
}
