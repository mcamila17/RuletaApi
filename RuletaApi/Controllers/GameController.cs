using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RuletaApi.Context;
using RuletaApi.Models;
using System.Data;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;

namespace RuletaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly AppDBContext _context;

        public GameController(AppDBContext context)
        {
            _context = context;
        }

        [HttpPost("/RuletaNueva")]
        public async Task<ActionResult<Ruleta>> CreaRuleta()
        {
            //Se crea un objeto de la clase Ruleta
            var rul = new Ruleta();
            //Se define el estado inicial como creada
            rul.estado = "creada";
            //Se guarda la nueva ruleta en base de datos
            _context.Ruletas.Add(rul);
            await _context.SaveChangesAsync();
            //Se devuelve el id de la ruleta creada
            return Ok("Id Ruleta: " + rul.id);
        }
        [HttpPost("/UsuarioNuevo")]
        public async Task<ActionResult<Ruleta>> CreaUsuario(int saldo)
        {
            //Se crea un objeto de la clase Usuario
            var user = new Usuario();
            //Se le asigna un salgo al nuevo usuario
            user.dineroDisponible = saldo;
            //Se guarda el nuevo usuario en la base de datos
            _context.Usuarios.Add(user);
            await _context.SaveChangesAsync();
            //Se devuelven los datos del nuevo usuario creado
            return Ok("Id Usuario: " + user.id + ", Saldo: " + user.dineroDisponible);
        }

        [HttpPut("/AbrirRuleta{id}")]       
        public async Task<IActionResult> AbrirRuleta(int id)
        {
            //Se consulta en la tabla de ruletas el id ingresado y se guarda en una variable
            var rul = await _context.Ruletas.FirstOrDefaultAsync(rulId => rulId.id == id);

            //Se verifica si la consulta devuelve una ruleta válida
            if (rul == null)
            {
                return BadRequest("Operación Denegada: Ruleta no existe");
            }
            else if (rul.estado == "abierta")
            {
                return BadRequest("Operación Denegada: Ruleta ya abierta");
            }
            else if (rul.estado == "cerrada")
            {
                return BadRequest("Operación Denegada: Ruleta cerrada");
            }
            //Si la ruleta es válida se cambia el estado a abierta y se guarda en la base de datos
            else
            {
                rul.estado = "abierta";
                await _context.SaveChangesAsync();
                return Ok("Operación Exitosa");
            }
        }
        [HttpPost("/ApuestaNueva")]
        public async Task<ActionResult<Apuesta>> CrearApuesta(int IdRuleta, int IdUsuario, string tipo_apuesta, string apuesta, int monto_apuesta)
        {
            //Se consulta la ruleta a la que se va a apostar
            var rul = await _context.Ruletas.FirstOrDefaultAsync(rulId => rulId.id == IdRuleta);
            //Se consulta el usuario que realiza la apuesta
            var user = await _context.Usuarios.FirstOrDefaultAsync(userId => userId.id == IdUsuario);

            //Se revisa que la ruleta sea válida
            if (rul == null)
            {
                return BadRequest("Operación Denegada: Ruleta no existe");
            }
            else if (rul.estado != "abierta")
            {
                return BadRequest("Operación Denegada: Ruleta no abierta o no disponible");
            }

            //Se verifica que el usuario sea válido
            if (user == null)
            {
                return BadRequest("Operación Denegada: Usuario no existe");
            }
            else if (user.dineroDisponible < monto_apuesta)
            {
                return BadRequest("Operación Denegada: Fondos insuficientes");
            }
            else if (monto_apuesta > 10000) 
            {
                return BadRequest("Operación Denegada: Excede el monto máximo");
            }

            //Se crea un nuevo objeto de la clase Apuesta
            var nueva_apuesta = new Apuesta();
            //Se asigna el usuario y la ruleta de la apuesta
            nueva_apuesta.idRuleta = IdRuleta;
            nueva_apuesta.idUsuario = IdUsuario;

            //Se verifica el tipo de apuesta
            if (tipo_apuesta == "numero")
            {
                //Se convierte la apuesta de string a int y se verifica que sea válida
                if(Int32.Parse(apuesta)>=0 && Int32.Parse(apuesta) <= 36)
                {
                    //se guardan los datos de la apuesta en el objeto apuesta
                    nueva_apuesta.tipoApuesta = tipo_apuesta;
                    nueva_apuesta.apuestaNumero = Int32.Parse(apuesta);
                    nueva_apuesta.valor = monto_apuesta;
                }
                else
                {
                    //Se retorna error si la apuesta no es válida
                    return BadRequest("Operación Denegada: Apuesta no válida");
                }
            }
            else if (tipo_apuesta == "color") 
            {
                //Se verifica el color al que se apuesta
                if(apuesta == "negro")
                {
                    //se guardan los datos de la apuesta en el objeto apuesta
                    nueva_apuesta.tipoApuesta = tipo_apuesta;
                    //apuestaColor es bool, negro = true
                    nueva_apuesta.apuestaColor = true;
                    nueva_apuesta.valor = monto_apuesta;
                }
                else if (apuesta == "rojo")
                {
                    //se guardan los datos de la apuesta en el objeto apuesta
                    nueva_apuesta.tipoApuesta = tipo_apuesta;
                    //apuestaColor es bool, rojo = false
                    nueva_apuesta.apuestaColor = false;
                    nueva_apuesta.valor = monto_apuesta;
                }
                else
                {
                    //Se retorna error si el color no es válido
                    return BadRequest("Operación Denegada: Apuesta no válida");
                }
            }
            else
            {
                //Se retorna error si el tipo de apuesta no es válido
                return BadRequest("Operación Denegada: Tipo de apuesta no válido");
            }
            //Si todas las condiciones son correctas, se guarda la nueva apuesta en la base de datos
            _context.Apuestas.Add(nueva_apuesta);
            //Se le resta al usuario el valor de su nueva apuesta
            user.dineroDisponible = user.dineroDisponible - monto_apuesta;
            //Se guardan los cambios en la base de datos
            await _context.SaveChangesAsync();
            //Se devuelve un mensaje de operación exitosa
            return Ok("Apuesta Realizada");
        }
        [HttpPut("/CerrarRuleta{id}")]
        public async Task<IActionResult> CerrarRuleta(int id)
        {
            //Se consulta la ruleta en la base de datos
            var rul = await _context.Ruletas.FirstOrDefaultAsync(rulId => rulId.id == id);

            //Se verifica que la ruleta exista y esté abierta
            if (rul == null)
            {
                return BadRequest("Operación Denegada: Ruleta no existe");
            }
            else if(rul.estado != "abierta")
            {
                return BadRequest("Operación Denedaga: Estado de ruleta inválido");
            }

            //Se selecciona el número ganador
            //Random random = new Random();
            //int num = random.Next(0, 36);
            int num = 8;
            //Se define el color del numero ganador (negro(impar) = true, rojo(par)=false)
            bool color = false;
            if (num % 2 != 0) {
                color = true;
            }

            //Se consulta en la tabla de apuestas todas las apuesta de la ruleta a cerrar
            //Esta consulta se convierte en una lista de objetos Apuesta, para poder iterar
            List<Apuesta> apuestas_ruleta = await _context.Apuestas.Where(rulId => rulId.idRuleta == id).ToListAsync();

            //Se crea un diccionario para almacenar los ganadores y ganancias
            var ganadores = new Dictionary<int, int>();

            //Se verifica que la ruleta tenga apuestas
            if (apuestas_ruleta == null)
            {
                //Si no tiene apuestas se devuelve un error
                return BadRequest("Operación denegada: La ruleta no tiene apuestas");
            }
            else
            {
                //Si tiene apuestas, se itera sobre la lista de apuestas para definir los ganadores
                foreach (var apuesta in apuestas_ruleta)
                {
                    //Se inician variables auxiliares
                    bool gana = false;
                    int ganancia = 0;

                    //Se verifica el tipo de apuesta de la linea
                    if(apuesta.tipoApuesta == "numero")
                    {
                        //Si es numero, se mira que sea igual al numero ganador
                        if(apuesta.apuestaNumero == num)
                        {
                            //Si gana, se cambia el valor de las variables auxiliares
                            gana = true;
                            ganancia = apuesta.valor * 5;
                        }
                    }
                    else if(apuesta.tipoApuesta == "color")
                    {
                        //Si es color, se mira que sea igual al color del numero ganador
                        if(apuesta.apuestaColor == color)
                        {
                            //Si gana, se cambia el valor de las variables auxiliares
                            gana = true;
                            //Al ser una mult por decimal, se redondea al entero mas cercano
                            ganancia = (int)Math.Round(apuesta.valor * 1.8, MidpointRounding.AwayFromZero);
                        }
                    }
                    //Si es ganador, se agrega el usuario y el monto ganado al diccionario de ganadores
                    if (gana)
                    {
                        //Se valida si el usuario existe en el diccionario
                        if (ganadores.ContainsKey(apuesta.idUsuario))
                        {
                            //Si existe, se le suma su nueva ganacia
                            ganadores[apuesta.idUsuario] += ganancia;
                        }
                        else
                        {
                            //Si no existe, se crea con su ganancia actual
                            ganadores[apuesta.idUsuario] = ganancia;
                        }
                    }

                }
                //Se cambia el estado de la ruleta
                rul.estado = "cerrada";
                //Se guardan los cambios en la base de datos
                await _context.SaveChangesAsync();
                
                //Si no hubo ganadores, se devuelve Ok diciendo que no hay ganadores
                if (ganadores.IsNullOrEmpty())
                {
                    return Ok("Ruleta Cerrada, no hay ganadores");
                }
                else
                {
                    //Si hay ganadores, se actualizan los saldos de los ganadores
                    foreach(var ganador in ganadores)
                    {
                        //Se consulta el saldo disponible
                        var user = await _context.Usuarios.FirstOrDefaultAsync(userId => userId.id == ganador.Key);
                        //Al saldo se le suma las ganancias
                        user.dineroDisponible = user.dineroDisponible + ganador.Value;
                        //Se guarda el saldo en la base de datos
                        await _context.SaveChangesAsync();
                    }
                }
            }
            //Se devuelve operacion exitosa si hay ganadores
            return Ok("Ruleta Cerrada, ganancias actualizadas");            
        }
    }
}
