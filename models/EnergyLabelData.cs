namespace Energy_printer.Models
{
    // ══════════════════════════════════════════════════════════════════════════
    //  ETIQUETA AMARILLA — EnergyGuide USA
    //  Equivale al objeto datosAmarillo del HTML original
    // ══════════════════════════════════════════════════════════════════════════
    public class EnergyLabelDataUSA
    {
        public string REF_TYPE            { get; set; }
        public string DEFROST_SYSTEM      { get; set; }
        public string DOORTYPE            { get; set; }
        public string ICE_SERVICE         { get; set; }
        public string CUST_NAME           { get; set; }
        public string MODEL               { get; set; }
        public string CAB_SIZE            { get; set; }
        public string PART_NUMBER         { get; set; }
        public int?    ENERGY_COST         { get; set; }
        public int?    LOW_SIMILAR_MODEL   { get; set; }
        public int?    HIGH_SIMILAR_MODEL  { get; set; }
        public int?    LOW_AMOUNT          { get; set; }
        public int?    HIGH_AMOUNT         { get; set; }
        public int?    ELECTRICITY_USE     { get; set; }
        /// <summary>"Y" = mostrar logo Energy Star</summary>
        public string ENERGY_LOGO         { get; set; }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  ETIQUETA BLANCA — EnerGuide Canada
    //  Equivale al objeto datosBlanco del HTML original
    // ══════════════════════════════════════════════════════════════════════════
    public class EnergyLabelDataCanada
    {
        public string MODEL        { get; set; }
        public int    MODEL_KW     { get; set; }
        public int    LOW_KW       { get; set; }
        public int    HIGH_KW      { get; set; }
        public string TYPE         { get; set; }
        public string RANGE        { get; set; }
        /// <summary>"Y" = mostrar logo Energy Star en el pie</summary>
        public string ENERGY_LOGO  { get; set; }
    }
}
