namespace BusDriverFile {
    class NodeBoundData {
        public Float3 bound_1;
        public float bound_1a;
        public float bound_1b;
        public Float3 bound_2;
        public float bound_2a;
        public float bound_2b;
        public NodeBoundData() {
            bound_1=new Float3();
            bound_1a=0f;
            bound_1b=0f;
            bound_2=new Float3();
            bound_2a=0f;
            bound_2b=0f;
        }
        public NodeBoundData(System.IO.BinaryReader br) {
            bound_1=new Float3(br);
            bound_1a=br.ReadSingle();
            bound_1b=br.ReadSingle();
            bound_2=new Float3(br);
            bound_2a=br.ReadSingle();
            bound_2b=br.ReadSingle();
        }
    }
}
