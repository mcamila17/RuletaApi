namespace RuletaApi.Models
{
    public class Ruleta
    {
        public int id { get; set; }
        public string estado { get; set; }
    }
    public class Apuesta
    {
        public int id { get; set; }
        public int idRuleta { get; set; }
        public int idUsuario { get; set; }
        public string tipoApuesta { get; set; }
        public int apuestaNumero { get; set; }
        public bool apuestaColor { get; set; }
        public int valor { get; set; }
    }
    public class Usuario
    {
        public int id { get; set; }
        public int dineroDisponible { get; set; }
    }

}
